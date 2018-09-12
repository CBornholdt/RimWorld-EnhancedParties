using System;
using Verse;
using Verse.AI;
using EnhancedParty;

namespace RimWorld
{
    public class DutyJob_WanderNearFocus : JobGiver_Wander
    {
        public DutyJob_WanderNearFocus()
        {
            this.wanderRadius = 5f; //From JobGiver_WanderCurrentRoom
            this.locomotionUrgency = LocomotionUrgency.Amble;
        }
    
        protected override Job TryGiveJob(Pawn pawn)
        {
            EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;
            if(duty == null)
                return null;

            this.wanderDestValidator = (Pawn p, IntVec3 c, IntVec3 root) => p.IsCellInDutyArea(c);
            var job = base.TryGiveJob(pawn);
            if(!EnhancedLordDebugSettings.disableThinkNodeLogging && EnhancedLordDebugSettings.verboseThinkNodeLogging) {
                IntVec3 dest = GetExactWanderDest(pawn);
                Log.Message($"Wandering for {pawn.Name} with nextMoveWait { pawn.mindState.nextMoveOrderIsWait } and destination { job.targetA.Cell }");
            }
            return job;
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            return WanderUtility.BestCloseWanderRoot(pawn.mindState.duty.focus.Cell, pawn);
        }
    }
}