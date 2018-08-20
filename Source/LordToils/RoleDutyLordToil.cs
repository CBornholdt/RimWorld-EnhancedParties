using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;
using Harmony;

namespace EnhancedParty
{
    public class RoleDutyLordToil : SimpleLordToil
    {
        public RoleDutyLordToil(ComplexLordToil parentToil = null, bool cancelExistingJobsOnTransition = false) 
            : base(parentToil, cancelExistingJobsOnTransition)
        {
        }

		public Dictionary<LordPawnRole, Func<PawnDuty>> roleDutyMap;

		public override void Notify_PawnJoinedRole(LordPawnRole role, Pawn pawn, LordPawnRole prevPawnRole)
		{
			AssignDutyTo(pawn, role);
		}

		public override void Notify_PawnLeftRole(LordPawnRole role, Pawn pawn, LordPawnRole newPawnRole)
		{
			
		}

		public override void Notify_PawnReplacedPawnInRole(LordPawnRole role, Pawn newPawn, Pawn oldPawn, LordPawnRole newPawnOldRole, LordPawnRole oldPawnNewRole)
		{
			AssignDutyTo(newPawn, role);
		}

		public void AssignDutyTo(Pawn pawn, LordPawnRole role)
		{   //Might need a guard on the .mindState
            if (role != null && roleDutyMap.TryGetValue(role, out Func<PawnDuty> dutyGen) && dutyGen != null)
                pawn.mindState.duty = dutyGen();
		}

		public void AssignDutyTo(Pawn pawn) => AssignDutyTo(pawn, LordJob.GetRole(pawn));
	}
}
