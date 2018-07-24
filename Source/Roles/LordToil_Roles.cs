using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    abstract public class LordToil_Roles : LordToil
    {
        public LordToil_Roles()
        {
			if(LordJob == null)
				Log.ErrorOnce($"Error constructing {this.GetType()}, LordJob is not subclass of LordJob_JoinableRoles", 87228);
        }

		public LordJob_JoinableRoles LordJob => this.lord.LordJob as LordJob_JoinableRoles;

		public override void UpdateAllDuties()
		{
			LordJob.CheckAndUpdateRoles();
		}

		public virtual void Notify_PawnJoinedRole(LordPawnRole role, Pawn pawn, LordPawnRole prevPawnRole) =>
				LordJob.Notify_PawnJoinedRole(role, pawn, prevPawnRole);

		public virtual void Notify_PawnLeftRole(LordPawnRole role, Pawn pawn, LordPawnRole newPawnRole) =>
				LordJob.Notify_PawnLeftRole(role, pawn, newPawnRole);

		public virtual void Notify_PawnReplacedPawnInRole(LordPawnRole role, Pawn newPawn, Pawn oldPawn
							, LordPawnRole newPawnOldRole, LordPawnRole oldPawnNewRole) =>
				LordJob.Notify_PawnReplacedPawnInRole(role, newPawn, oldPawn, newPawnOldRole, oldPawnNewRole);
	}
}
