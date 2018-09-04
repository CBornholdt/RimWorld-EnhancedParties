using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using EnhancedParty;

namespace RimWorld
{
    //Based on JobGiver_GotoTravelDestination
    public class JobGiver_CleanRoom : ThinkNode_JobGiver
    {
		static public readonly int maxNumOfFilthToCleanAtOnce = 15; //same as WorkGiver_Clean

		protected override Job TryGiveJob(Pawn pawn)
		{
			Log.Message($"Cleaning room with {pawn.Name}");
        
			Job job = new Job(JobDefOf.Clean);
			Room roomToBeCleaned = pawn.mindState.duty?.focus.Cell.GetRoom(pawn.Map, RegionType.Set_Passable);

			if(roomToBeCleaned == null) {
				Log.Error($"Attempted to clean room with pawn {pawn.Name} but their duty's focus is not inside a room");
				return null;
			}

			var potentialFilth = roomToBeCleaned.Cells.SelectMany(cell => cell.GetThingList(pawn.Map))
													  .OfType<Filth>().Cast<Filth>();
                                                      
            WorkGiverDef cleanFilthGiver = DefDatabase<WorkGiverDef>.GetNamed("CleanFilth");                                                      

			int num = 0;
			foreach(var filth in potentialFilth) {
				if((cleanFilthGiver.Worker as WorkGiver_Scanner).HasJobOnThing(pawn, filth, forced: false)) {
					job.AddQueuedTarget(TargetIndex.A, filth);
					if(++num >= maxNumOfFilthToCleanAtOnce)
						break;
				}
			}

			if(num == 0)
				return null;

			if(job.targetQueueA.Count >= 5)
				job.targetQueueA.SortBy(filth => filth.Cell.DistanceToSquared(pawn.Position));

			return job;
		}
	}
}

