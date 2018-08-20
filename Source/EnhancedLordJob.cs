using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    abstract public class EnhancedLordJob : LordJob_VoluntarilyJoinable
    {
        protected List<LordPawnRole> roles;

        public bool TryGetRole(string name, out LordPawnRole role)
        {
            int index = roles.FindIndex(r => r.name == name);
            if (index == -1)
            {
                role = null;
                return false;
            }
            role = roles[index];
            return true;
        }
        public LordPawnRole GetRole(string name) => roles.FirstOrDefault(role => role.name == name);

        public bool TryGetRole(Pawn pawn, out LordPawnRole role)
        {
            int index = roles.FindIndex(r => r.currentPawns.Contains(pawn));
            if (index == -1)
            {
                role = null;
                return false;
            }
            role = roles[index];
            return true;
        }
        public LordPawnRole GetRole(Pawn pawn) => roles.FirstOrDefault(role => role.currentPawns.Contains(pawn));

        public bool HasRole(string name) => roles.Any(role => role.name == name);

		public void SetRoleEnabled(string name, bool enabled)
		{
			var role = GetRole(name);
			if(role != null)
				role.enabled = enabled;
		}

        public EnhancedLordToil CurrentEnhancedToil => this.lord.CurLordToil as EnhancedLordToil;

		virtual public bool AllowRolelessPawnsToReplenish => false;
        
        abstract protected void Initialize();

        public EnhancedLordJob() => Initialize();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<LordPawnRole>(ref this.roles, "Roles", LookMode.Deep);
        }

        public void AddRole(LordPawnRole role)
        {
            role.lordJob = this;
            roles.Add(role);
        }

        virtual public void Notify_RoleNowEmpty(LordPawnRole role) =>
            this.lord.ReceiveMemo($"LordPawnRole.{role.name}.Empty");

        virtual public void Notify_PawnLeftRole(LordPawnRole role, Pawn pawn, LordPawnRole newRole) { }

        virtual public void Notify_PawnJoinedRole(LordPawnRole role, Pawn pawn, LordPawnRole oldRole) { }

        virtual public void Notify_PawnReplacedPawnInRole(LordPawnRole role, Pawn replacer, Pawn prevPawn
                        , LordPawnRole replacerOldRole, LordPawnRole prevPawnNewRole)
        { }

        //Does not directly handle addition of new pawns to lord
        public void CheckAndUpdateRoles()
        {   //Everything is sorted descending by current role priority, allows priorities to change
            var sortedRoles = roles.OrderByDescending(role => role.priority);

            HashSet<Pawn> pawnsWithLowerPriorityRoles = new HashSet<Pawn>(lord.ownedPawns);
            var pawnsLostSorted = new List<Tuple<LordPawnRole, Pawn, ReasonPawnLeftRole>>(); //Sorted by lost role priority
            var pawnsReplaced = new List<Tuple<LordPawnRole, Pawn, Pawn, LordPawnRole>>();   //Role, replacer, replacee, replacer old role
            var pawnsAdded = new List<Tuple<LordPawnRole, Pawn, LordPawnRole>>();            //Role, new pawn, old role

            //Check for pawns that have left roles due to leaving lord or other, adjust current role pawns
            foreach (var role in sortedRoles)
            {
                for (int i = role.currentPawns.Count - 1; i >= 0; i--)
                {
                    Pawn pawn = role.currentPawns[i];
                    if (pawn.GetLord() != lord)
                    {
                        pawnsLostSorted.Add(Tuple.Create(role, pawn, ReasonPawnLeftRole.LeftLord));
                        role.currentPawns.RemoveAt(i);
                    }
                    else if (!role.pawnValidator(pawn))
                    {
                        pawnsLostSorted.Add(Tuple.Create(role, pawn, ReasonPawnLeftRole.Invalid));
                        role.currentPawns.RemoveAt(i);
                    }
                    else if (!pawnsWithLowerPriorityRoles.Contains(pawn))
                    {
                        pawnsLostSorted.Add(Tuple.Create(role, pawn, ReasonPawnLeftRole.InNewRole));
                        role.currentPawns.RemoveAt(i);
                    }
                    else if (!role.enabled)
                    {
                        pawnsLostSorted.Add(Tuple.Create(role, pawn, ReasonPawnLeftRole.RoleDisabled));
                        role.currentPawns.RemoveAt(i);
                    }
                    else
                        pawnsWithLowerPriorityRoles.Remove(pawn);
                }
            }

            IEnumerable<Tuple<LordPawnRole, Pawn>> tuplePawnsWithRole(LordPawnRole role)
            {
                foreach (var pawn in role.currentPawns)
                    yield return Tuple.Create(role, pawn);
            }
			var potentialReplacements = AllowRolelessPawnsToReplenish
											? this.lord.ownedPawns.Select(pawn => Tuple.Create(GetRole(pawn), pawn))
																  .Where(tuple => tuple.Item1?.isReassignableFrom ?? true)
																  .ToList()
											: sortedRoles.Where(role => role.isReassignableFrom)
														 .SelectMany(tuplePawnsWithRole)
														 .ToList();
                                                           
            void replaceLostPawnWith(Tuple<LordPawnRole, Pawn, ReasonPawnLeftRole> lostPawn
                                                    , Tuple<LordPawnRole, Pawn> replacement)
            {
                lostPawn.Item1.currentPawns.Add(replacement.Item2);
                replacement.Item1.currentPawns.Remove(replacement.Item2);
                //Will place this new lost pawn entry at the last position for its priority
                pawnsLostSorted.Insert(pawnsLostSorted.FindLastIndex(pl => pl.Item1.priority >= replacement.Item1.priority)
                                    , Tuple.Create(replacement.Item1, replacement.Item2, ReasonPawnLeftRole.Replacement));
                pawnsLostSorted.Remove(lostPawn);
                pawnsReplaced.Add(Tuple.Create(lostPawn.Item1, replacement.Item2, lostPawn.Item2, replacement.Item1));
                potentialReplacements.Remove(replacement);
            }

            //Attempt to perform replacements
            for (int i = 0; i < pawnsLostSorted.Count;)
            {    //Adding to list, so need for-loop not foreach
                var pawnLost = pawnsLostSorted[i];
				if (pawnLost.Item1.shouldSeekReplacement 
                    && potentialReplacements.Where(vr => vr.Item1.priority < pawnLost.Item1.priority
													        && pawnLost.Item1.pawnValidator(vr.Item2))
					                        .TryRandomElementByWeight(vr => pawnLost.Item1.pawnReplenishPriority(vr.Item2)
													                    , out Tuple<LordPawnRole, Pawn> replacement))
					replaceLostPawnWith(pawnLost, replacement);
				else
					i++;    //Only increment if not adjusted to allow chain replacements
            }
			//pawnsLostSorted will now contain any role slots that remain unfilled

			List<Tuple<LordPawnRole, Pawn>> validReplacements = null;
            void replenishPawnTo(LordPawnRole role, Tuple<LordPawnRole, Pawn> replacement)
            {
                role.currentPawns.Add(replacement.Item2);
                replacement.Item1.currentPawns.Remove(replacement.Item2);
                pawnsLostSorted.Insert(pawnsLostSorted.FindLastIndex(pl => pl.Item1.priority > replacement.Item1.priority)
                                        , Tuple.Create(replacement.Item1, replacement.Item2, ReasonPawnLeftRole.InNewRole));
                pawnsAdded.Add(Tuple.Create(role, replacement.Item2, replacement.Item1));
				potentialReplacements.Remove(replacement);
				validReplacements.Remove(replacement);
            }

            //Perform replenishment loops by priorty order
            foreach (var role in sortedRoles.Where(role => role.opportunisticallyReplenish))
            {
                validReplacements = potentialReplacements.Where(vr => vr.Item1.priority < role.priority
                                                                        && role.pawnValidator(vr.Item2))
                                                          .OrderByDescending(vr => role.pawnReplenishPriority(vr.Item2))
                                                          .ToList();
				while (!role.replenishCompleter(role.currentPawns)
					&& validReplacements.TryRandomElementByWeight(vr => role.pawnReplenishPriority(vr.Item2)
																  , out Tuple<LordPawnRole, Pawn> replacement)) 
					replenishPawnTo(role, replacement);
            }

			//Notify Toil if such
			EnhancedLordToil toil = CurrentEnhancedToil;
            if (toil != null)
            {
                foreach (var pawnLost in pawnsLostSorted)
                    toil.Notify_PawnLeftRole(pawnLost.Item1, pawnLost.Item2, GetRole(pawnLost.Item2));
                foreach (var pawnReplaced in pawnsReplaced)
                    toil.Notify_PawnReplacedPawnInRole(pawnReplaced.Item1, pawnReplaced.Item2
                                    , pawnReplaced.Item3, pawnReplaced.Item4, GetRole(pawnReplaced.Item2));
                foreach (var pawnAdded in pawnsAdded)
                    toil.Notify_PawnJoinedRole(pawnAdded.Item1, pawnAdded.Item2, pawnAdded.Item3);
            }
            else
            {  //Fallback to LordJob notifications if Toil does not support Role Logic
                foreach (var pawnLost in pawnsLostSorted)
                    this.Notify_PawnLeftRole(pawnLost.Item1, pawnLost.Item2, GetRole(pawnLost.Item2));
                foreach (var pawnReplaced in pawnsReplaced)
                    this.Notify_PawnReplacedPawnInRole(pawnReplaced.Item1, pawnReplaced.Item2
                                    , pawnReplaced.Item3, pawnReplaced.Item4, GetRole(pawnReplaced.Item2));
                foreach (var pawnAdded in pawnsAdded)
                    this.Notify_PawnJoinedRole(pawnAdded.Item1, pawnAdded.Item2, pawnAdded.Item3);
            }

            //Notify if any roles have become empty
            if (pawnsLostSorted.Any())
            {
                foreach (var role in roles)
                {
                    if (role.currentPawns.Count == 0 && pawnsLostSorted.Any(pawnLost => pawnLost.Item1 == role))
                        Notify_RoleNowEmpty(role);
                }
            }
        }
    }
}
