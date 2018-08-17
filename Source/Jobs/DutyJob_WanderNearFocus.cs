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
            this.wanderRadius = 7f; //From JobGiver_WanderCurrentRoom
            this.locomotionUrgency = LocomotionUrgency.Amble;
        }
    
        protected override Job TryGiveJob(Pawn pawn)
        {
			EnhancedPawnDuty duty = pawn.mindState.duty as EnhancedPawnDuty;
			if(duty == null)
				return null;

			if(duty.stayInRoom) {
				Room pawnRoom = pawn.Position.GetRoom(pawn.Map);

				if(pawnRoom == null || pawnRoom != pawn.mindState.duty?.focus.Cell.GetRoom(pawn.Map))
					return null;

				this.wanderDestValidator = (Pawn p, IntVec3 loc) => loc.GetRoom(p.Map) ==
											GetWanderRoot(p).GetRoom(p.Map);
			}
			else {
				this.wanderDestValidator = (Pawn p, IntVec3 loc) => true;
			}

            Log.Message($"Wandering for {pawn.Name} about { GetWanderRoot(pawn) }");
            return base.TryGiveJob(pawn);
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            return pawn.mindState.duty.focus.Cell;
        }
    }
}