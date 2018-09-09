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
		static public bool nextCheckUseRefresh = false;
    
        protected List<LordPawnRole> roles = new List<LordPawnRole>();
		protected List<LordPawnRoleData> roleData;
        
        private List<Tuple<string, Pawn>> completeDutyOps = new List<Tuple<string, Pawn>>();
        
        private List<Tuple<string, Pawn>> failedDutyOps = new List<Tuple<string, Pawn>>();

		public List<ICleanableAction> cleanupActions = new List<ICleanableAction>();

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
            int index = roles.FindIndex(r => r.CurrentPawns.Contains(pawn));
            if (index == -1)
            {
                role = null;
                return false;
            }
            role = roles[index];
            return true;
        }
        public LordPawnRole GetRole(Pawn pawn) => roles.FirstOrDefault(role => role.CurrentPawns.Contains(pawn));

        public bool HasRole(string name) => roles.Any(role => role.name == name);

		public void SetRoleEnabled(string name, bool enabled)
		{
			var role = GetRole(name);
			if(role != null)
				role.data.enabled = enabled;
		}

		protected void ConfigureRoleData()
		{
			foreach(var role in roles) {
				var data = roleData?.FirstOrDefault(d => d.roleName == role.name);
				if(data != null)
					role.data = data;
				else if(role.data == null)
					role.data = new LordPawnRoleData(role.name);
			}
		}

		public void ClearAllRolePawns()
		{
			foreach(var role in roles)
				role.CurrentPawns.Clear();
		}
        
        public virtual bool IsCellInDutyArea(Pawn pawn, IntVec3 cell)
        {
            return EnhancedDutyUtility.IsCellInDutyArea(pawn, cell);
        }

		public virtual bool IsInDutyArea(Pawn pawn)
		{
			return EnhancedDutyUtility.IsInDutyArea(pawn);
		}

		public virtual IEnumerable<IntVec3> DutyAreaCells(Pawn pawn)
		{
			return EnhancedDutyUtility.DutyAreaCells(pawn);
		}

		protected abstract StateGraph CreateGraphAndRoles();

		public override StateGraph CreateGraph()
		{
			var graph = CreateGraphAndRoles();
			ConfigureRoleData();
			return graph;
		}

		public EnhancedLordToil CurrentEnhancedToil => this.lord.CurLordToil as EnhancedLordToil;

		virtual public bool AllowRolelessPawnsToReplenish => false;

		public EnhancedLordJob() { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<LordPawnRoleData>(ref this.roleData, "RoleData", LookMode.Deep);
			Scribe_Collections.Look<ICleanableAction>(ref this.cleanupActions, "CleanupActions", LookMode.Deep);

			if(Scribe.mode == LoadSaveMode.PostLoadInit)    //Remove cleanupActions with broken references
				this.cleanupActions = cleanupActions.Where(action => !action.ReferencesBroken()).ToList();
        }

        public void AddRole(LordPawnRole role)
        {
            role.lordJob = this;
            roles.Add(role);
        }

        virtual public void Notify_RoleNowEmpty(LordPawnRole role) =>
            this.lord.ReceiveMemo($"LordPawnRole.{role.name}.Empty");
            
        public override void Notify_PawnAdded(Pawn p)
        {
			CurrentEnhancedToil?.Notify_PawnJoinedLord(p);
        }

        virtual public void Notify_PawnLeftRole(LordPawnRole role, Pawn pawn, LordPawnRole newRole) { }

        virtual public void Notify_PawnJoinedRole(LordPawnRole role, Pawn pawn, LordPawnRole oldRole) { }

        virtual public void Notify_PawnReplacedPawnInRole(LordPawnRole role, Pawn replacer, Pawn prevPawn
                        , LordPawnRole replacerOldRole, LordPawnRole prevPawnNewRole)
        { }

		virtual public void Notify_PawnDutyOpComplete(string dutyOp, Pawn pawn) { }
        
        virtual public void Notify_PawnDutyOpFailed(string dutyOp, Pawn pawn) { }
        
        public void RegisterDutyOpComplete(string dutyOp, Pawn pawn) =>
            completeDutyOps.Add(Tuple.Create(dutyOp, pawn));
            
        public void RegisterDutyOpFailed(string dutyOp, Pawn pawn) =>
            failedDutyOps.Add(Tuple.Create(dutyOp, pawn));

		public void RegisterCleanupAction(ICleanableAction action) =>
			cleanupActions.Add(action);

		public virtual ThinkTreeDutyHook DefaultToilDutyHook =>
			ThinkTreeDutyHook.MediumPriority;

        public void CheckAndUpdateRoles()
        {   //Everything is sorted descending by current role priority, allows priorities to change
            var sortedRoles = roles.OrderByDescending(role => role.Priority);

            HashSet<Pawn> pawnsWithLowerPriorityRoles = new HashSet<Pawn>(lord.ownedPawns);
            var pawnsLostSorted = new List<Tuple<LordPawnRole, Pawn, ReasonPawnLeftRole>>(); //Sorted by lost role priority
            var pawnsReplaced = new List<Tuple<LordPawnRole, Pawn, Pawn, LordPawnRole>>();   //Role, replacer, replacee, replacer old role
            var pawnsAdded = new List<Tuple<LordPawnRole, Pawn, LordPawnRole>>();            //Role, new pawn, old role

            //Check for pawns that have left roles due to leaving lord or other, adjust current role pawns
            foreach (var role in sortedRoles)
            {
                for (int i = role.CurrentPawns.Count - 1; i >= 0; i--)
                {
                    Pawn pawn = role.CurrentPawns[i];
                    if (pawn.GetLord() != lord)
                    {
                        pawnsLostSorted.Add(Tuple.Create(role, pawn, ReasonPawnLeftRole.LeftLord));
                        role.CurrentPawns.RemoveAt(i);
                    }
                    else if (!role.pawnValidator(pawn))
                    {
                        pawnsLostSorted.Add(Tuple.Create(role, pawn, ReasonPawnLeftRole.Invalid));
                        role.CurrentPawns.RemoveAt(i);
                    }
                    else if (!pawnsWithLowerPriorityRoles.Contains(pawn))
                    {
                        pawnsLostSorted.Add(Tuple.Create(role, pawn, ReasonPawnLeftRole.InNewRole));
                        role.CurrentPawns.RemoveAt(i);
                    }
                    else if (!role.IsEnabled)
                    {
                        pawnsLostSorted.Add(Tuple.Create(role, pawn, ReasonPawnLeftRole.RoleDisabled));
                        role.CurrentPawns.RemoveAt(i);
                    }
                    else
                        pawnsWithLowerPriorityRoles.Remove(pawn);
                }
            }

            IEnumerable<Tuple<LordPawnRole, Pawn>> tuplePawnsWithRole(LordPawnRole role)
            {
                foreach (var pawn in role.CurrentPawns)
                    yield return Tuple.Create(role, pawn);
            }
			var potentialReplacements = AllowRolelessPawnsToReplenish
											? this.lord.ownedPawns.Select(pawn => Tuple.Create(GetRole(pawn), pawn))
																  .Where(tuple => tuple.Item1?.IsReassignableFrom ?? true)
																  .ToList()
											: sortedRoles.Where(role => role.IsReassignableFrom)
														 .SelectMany(tuplePawnsWithRole)
														 .ToList();
                                                           
            void replaceLostPawnWith(Tuple<LordPawnRole, Pawn, ReasonPawnLeftRole> lostPawn
                                                    , Tuple<LordPawnRole, Pawn> replacement)
            {
                lostPawn.Item1.CurrentPawns.Add(replacement.Item2);
				//Will place this new lost pawn entry at the last position for its priority
				if(replacement.Item1 != null) {   //Prevents role-less pawns from adding to lost list
					replacement.Item1.CurrentPawns.Remove(replacement.Item2);
					pawnsLostSorted.Insert(pawnsLostSorted.FindLastIndex(pl => pl.Item1.Priority >= replacement.Item1.Priority) + 1
											, Tuple.Create(replacement.Item1, replacement.Item2, ReasonPawnLeftRole.Replacement));
				}
                pawnsLostSorted.Remove(lostPawn);
                pawnsReplaced.Add(Tuple.Create(lostPawn.Item1, replacement.Item2, lostPawn.Item2, replacement.Item1));
                potentialReplacements.Remove(replacement);
            }

            //Attempt to perform replacements
            for (int i = 0; i < pawnsLostSorted.Count;)
            {    //Adding to list, so need for-loop not foreach
                var pawnLost = pawnsLostSorted[i];
				if (pawnLost.Item1.IsEnabled && pawnLost.Item1.ShouldSeekReplacement 
                    && potentialReplacements.Where(vr => (vr.Item1?.Priority ?? 0) < pawnLost.Item1.Priority
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
                role.CurrentPawns.Add(replacement.Item2);
				if(replacement.Item1 != null) {  //role-less pawns will not be seen as lost
					replacement.Item1.CurrentPawns.Remove(replacement.Item2);
					pawnsLostSorted.Insert(pawnsLostSorted.FindLastIndex(pl => pl.Item1.Priority > replacement.Item1.Priority) + 1
											, Tuple.Create(replacement.Item1, replacement.Item2, ReasonPawnLeftRole.InNewRole));
				}
                pawnsAdded.Add(Tuple.Create(role, replacement.Item2, replacement.Item1));
				potentialReplacements.Remove(replacement);
				validReplacements.Remove(replacement);
            }

            //Perform replenishment loops by priorty order
            foreach (var role in sortedRoles.Where(role => role.IsEnabled && role.OpportunisticallyReplenish))
            {
                validReplacements = potentialReplacements.Where(vr => (vr.Item1?.Priority ?? 0) < role.Priority
                                                                        && role.pawnValidator(vr.Item2))
                                                         .OrderByDescending(vr => role.pawnReplenishPriority(vr.Item2))
                                                         .ToList();
				while (!role.replenishCompleter(role.CurrentPawns)
					&& validReplacements.TryRandomElementByWeight(vr => role.pawnReplenishPriority(vr.Item2)
																  , out Tuple<LordPawnRole, Pawn> replacement)) 
					replenishPawnTo(role, replacement);
            }

            EnhancedLordToil toil = CurrentEnhancedToil;
			if(!nextCheckUseRefresh && toil != null) {
				//Notify Toil if such
				foreach(var pawnLost in pawnsLostSorted)
					toil.Notify_PawnLeftRole(pawnLost.Item1, pawnLost.Item2, GetRole(pawnLost.Item2));
				foreach(var pawnReplaced in pawnsReplaced)
					toil.Notify_PawnReplacedPawnInRole(pawnReplaced.Item1, pawnReplaced.Item2
									, pawnReplaced.Item3, pawnReplaced.Item4, GetRole(pawnReplaced.Item2));
				foreach(var pawnAdded in pawnsAdded)
					toil.Notify_PawnJoinedRole(pawnAdded.Item1, pawnAdded.Item2, pawnAdded.Item3);
			}
			else {  
				if(toil == null) {  //Refresh requires enhanced toil, so notify LordJob was fallback
					foreach(var pawnLost in pawnsLostSorted)
						this.Notify_PawnLeftRole(pawnLost.Item1, pawnLost.Item2, GetRole(pawnLost.Item2));
					foreach(var pawnReplaced in pawnsReplaced)
						this.Notify_PawnReplacedPawnInRole(pawnReplaced.Item1, pawnReplaced.Item2
										, pawnReplaced.Item3, pawnReplaced.Item4, GetRole(pawnReplaced.Item2));
					foreach(var pawnAdded in pawnsAdded)
						this.Notify_PawnJoinedRole(pawnAdded.Item1, pawnAdded.Item2, pawnAdded.Item3);
				}
				else {
					Log.Message("refreshing");
					toil.RefreshAllDuties();
				}
			}

			//Notify if any roles have become empty
			if(!nextCheckUseRefresh && pawnsLostSorted.Any()) {
				foreach(var role in roles) {
					if(role.CurrentPawns.Count == 0 && pawnsLostSorted.Any(pawnLost => pawnLost.Item1 == role))
						Notify_RoleNowEmpty(role);
				}
			}   

			if(EnhancedLordDebugSettings.logRoleChanges) {
				var builder = new StringBuilder();

				builder.AppendLine($"EnhancedLordJob {this.GetType().Name} on map {this.lord.Map.Index} at tick {Find.TickManager.TicksGame}");
				builder.AppendLine($"Pawns Left Role: {pawnsLostSorted.Count}   Pawns Replaced: {pawnsReplaced.Count}   Pawns Added: {pawnsAdded.Count}");
				foreach(var lostPawn in pawnsLostSorted)
					builder.AppendLine($"\tLOST  Role: {lostPawn.Item1.name}  Pawn: {lostPawn.Item2.Name}   Reason: {lostPawn.Item3.ToString()}");
				foreach(var replacedPawn in pawnsReplaced)
					builder.AppendLine($"\tREPLACED  Role: {replacedPawn.Item1.name}  NewPawn: {replacedPawn.Item2.Name}   OldPawn: {replacedPawn.Item3.Name}   NewPawnOldRole: {replacedPawn.Item4?.name ?? "NONE"}");
				foreach(var addedPawn in pawnsAdded)
					builder.AppendLine($"\tADDED  Role: {addedPawn.Item1.name}  Pawn: {addedPawn.Item2.Name}   OldRole: {addedPawn.Item3?.name ?? "NONE"}");

				Log.Message(builder.ToString());
			}

			nextCheckUseRefresh = false;
        }

		public override void Cleanup()
		{
			base.Cleanup();
			foreach(var action in cleanupActions)
				if(action.CleanupStillNeeded())
					action.PerformCleanup();

			cleanupActions.Clear();
		}
	}
}
