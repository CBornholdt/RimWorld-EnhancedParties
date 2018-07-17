using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
	public enum PawnRoleLostCondition { LeftLord, InValid, NewRole };
    
    abstract public class LordJob_JoinableRoles : LordJob_VoluntarilyJoinable
    {
		protected ListDictionary roles;

		public LordPawnRole GetRole(string name) => roles[name] as LordPawnRole;
		public bool HasRole(string name) => roles.Contains(name);

		public LordToil_Roles CurrentToil => this.lord.CurLordToil as LordToil_Roles;

		public void AddRole(LordPawnRole role)
		{
			role.lordJob = this;
            roles.Add(role.name, role);
		}

		virtual public void Notify_PawnLeftRole(LordPawnRole role, Pawn pawn, LordPawnRole newRole) { }

		virtual public void Notify_PawnJoinedRole(LordPawnRole role, Pawn pawn, LordPawnRole oldRole) { }

		virtual public void Notify_PawnWasReplacedInRole(LordPawnRole role, Pawn replacer, Pawn prevPawn
						, LordPawnRole replacerOldRole, LordPawnRole prevPawnNewRole)
		{ }

        //Does not directly handle addition of new pawns to lord
		public void CheckAndUpdateRoles()
		{   //Everything is sorted descending by role priority
			var sortedRoles = roles.Values.OfType<LordPawnRole>()
										  .Cast<LordPawnRole>()
										  .OrderByDescending(role => role.priority);

			HashSet<Pawn> pawnsWithLowerPriorityRoles = new HashSet<Pawn>(lord.ownedPawns);
            var pawnsLostSorted = new List<Tuple<LordPawnRole, Pawn, PawnRoleLostCondition>>(); //Sorted by lost role priority
			var pawnsReplaced = new List<Tuple<LordPawnRole, Pawn, Pawn, LordPawnRole>>();   //Role, replacer, replacee, replacer old role
			var pawnsAdded = new List<Tuple<LordPawnRole, Pawn, LordPawnRole>>();            //Role, new pawn, old role

			foreach(var role in sortedRoles) {
				for(int i = role.currentPawns.Count - 1; i >= 0; i--) {
					Pawn pawn = role.currentPawns[i];
					if(pawn.GetLord() != lord) {
						pawnsLostSorted.Add(Tuple.Create(role, pawn, PawnRoleLostCondition.LeftLord));
						role.currentPawns.RemoveAt(i);
					}
					else if(!role.pawnValidator(pawn)) {
						pawnsLostSorted.Add(Tuple.Create(role, pawn, PawnRoleLostCondition.InValid));
						role.currentPawns.RemoveAt(i);
					}
					else if(!pawnsWithLowerPriorityRoles.Contains(pawn)) {
						pawnsLostSorted.Add(Tuple.Create(role, pawn, PawnRoleLostCondition.NewRole));
						role.currentPawns.RemoveAt(i);
					}
					else
						pawnsWithLowerPriorityRoles.Remove(pawn);
				}
			}

            IEnumerable<Tuple<LordPawnRole, Pawn>> tuplePawnsWithRole(LordPawnRole role) {
				foreach(var pawn in role.currentPawns)
					yield return Tuple.Create(role, pawn);
			}
			var potentialReplacements = sortedRoles.Where(role => role.isReassignableFrom)
											   .SelectMany(tuplePawnsWithRole)
											   .ToList();
                                               
            void replaceLostPawnWith(Tuple<LordPawnRole, Pawn, PawnRoleLostCondition> lostPawn
                                                    , Tuple<LordPawnRole, Pawn> replacement) {
				lostPawn.Item1.currentPawns.Add(replacement.Item2);
				replacement.Item1.currentPawns.Remove(replacement.Item2);
				pawnsLostSorted.Insert(pawnsLostSorted.FindLastIndex(pl => pl.Item1.priority > replacement.Item1.priority)
									, Tuple.Create(replacement.Item1, replacement.Item2, PawnRoleLostCondition.NewRole));
				pawnsLostSorted.Remove(lostPawn);
				pawnsReplaced.Add(Tuple.Create(lostPawn.Item1, replacement.Item2, lostPawn.Item2, replacement.Item1));
                potentialReplacements.Remove(replacement);
			}

			for(int i = 0; i < pawnsLostSorted.Count; i++) {    //Adding to list, so need for-loop not foreach
				var pawnLost = pawnsLostSorted[i];
				if(!pawnLost.Item1.shouldSeekReplacement)
					continue;   
				if(potentialReplacements.Where(vr => vr.Item1.priority < pawnLost.Item1.priority)
					.TryRandomElementByWeight(vr => pawnLost.Item1.pawnReplenishPriority(vr.Item2)
												, out Tuple<LordPawnRole, Pawn> replacement))
					replaceLostPawnWith(pawnLost, replacement);
			}
			//pawnsLostSorted will now contain any role slots that remain unfilled
			//

			List<Tuple<LordPawnRole, Pawn>> validReplacements = null;
            void replenishPawnTo(LordPawnRole role, Tuple<LordPawnRole, Pawn> replacement)
			{
				role.currentPawns.Add(replacement.Item2);
				replacement.Item1.currentPawns.Remove(replacement.Item2);
                pawnsLostSorted.Insert(pawnsLostSorted.FindLastIndex(pl => pl.Item1.priority > replacement.Item1.priority)
                                    , Tuple.Create(replacement.Item1, replacement.Item2, PawnRoleLostCondition.NewRole));
				pawnsAdded.Add(Tuple.Create(role, replacement.Item2, replacement.Item1));
				potentialReplacements.Remove(replacement);
			}

			foreach(var role in sortedRoles.Where(role => role.opportunisticallyReplenish)) {
				validReplacements = potentialReplacements.Where(vr => vr.Item1.priority < role.priority
																		&& role.pawnValidator(vr.Item2))
															.OrderByDescending(vr => role.pawnReplenishPriority(vr.Item2))
                                                            .ToList();
				while(!role.replenishCompleter(role.currentPawns)
					&& validReplacements.TryRandomElementByWeight(vr => role.pawnReplenishPriority(vr.Item2)
																	, out Tuple<LordPawnRole, Pawn> replacement)) 
					replenishPawnTo(role, replacement);     
			}

			if(CurrentToil != null) {
				foreach(var pawnLost in pawnsLostSorted)
					CurrentToil.Notify_PawnLeftRole(pawnLost.Item1, pawnLost.Item2, null);
				foreach(var pawnReplaced in pawnsReplaced)
					CurrentToil.Notify_PawnReplacedPawnInRole(pawnReplaced.Item1, pawnReplaced.Item2,
														pawnReplaced.Item3, pawnReplaced.Item4, null);
				foreach(var pawnAdded in pawnsAdded)
					CurrentToil.Notify_PawnJoinedRole(pawnAdded.Item1, pawnAdded.Item2, pawnAdded.Item3);
			}                          
		}       
	}
}
