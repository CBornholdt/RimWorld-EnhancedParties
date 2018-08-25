﻿using System;
using Verse;
using Verse.AI;
using EnhancedParty;

namespace RimWorld
{
    public class DutyJob_WanderNearFocus : JobGiver_Wander
    {
        public DutyJob_WanderNearFocus()
        {
            this.wanderRadius = 7f; //From JobGiver_WanderCurrentRoom
            this.locomotionUrgency = LocomotionUrgency.Amble;
        }
    
        protected override Job TryGiveJob(Pawn pawn)
        {
			EnhancedPawnDuty duty = pawn.mindState.duty as EnhancedPawnDuty;
			if(duty == null)
				return null;

			this.wanderDestValidator = (Pawn p, IntVec3 c) => p.IsCellInDutyArea(c);

            Log.Message($"Wandering for {pawn.Name} about { GetWanderRoot(pawn) }");
            return base.TryGiveJob(pawn);
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            return pawn.mindState.duty.focus.Cell;
        }
    }
}