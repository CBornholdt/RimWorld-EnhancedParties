using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class DutyJob_WanderInDutyRoom : JobGiver_Wander
	{
		public DutyJob_WanderInDutyRoom()
		{
			this.wanderRadius = 7f; //From JobGiver_WanderCurrentRoom
			this.locomotionUrgency = LocomotionUrgency.Amble;
			this.wanderDestValidator = (Pawn pawn, IntVec3 loc, IntVec3 root) => loc.GetRoom(pawn.Map) == 
											root.GetRoom(pawn.Map);
		}
	
		protected override Job TryGiveJob(Pawn pawn)
		{
			Room pawnRoom = pawn.Position.GetRoom(pawn.Map);
		
			if(pawnRoom == null || pawnRoom != pawn.mindState.duty?.focus.Cell.GetRoom(pawn.Map))
				return null;

			//Log.Message($"Wandering for {pawn.Name} about { GetWanderRoot(pawn) }");
			return base.TryGiveJob(pawn);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.mindState.duty.focus.Cell;
		}
	}
}