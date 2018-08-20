using System;
using System.Linq;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    static public class LordExt
    {
		static public void CancelAllPawnJobs(this Lord lord)
		{
            foreach(var pawn in lord.ownedPawns) {
                pawn.jobs.ClearQueuedJobs();
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
		}

		static public bool IsInEnhancedToil(this Lord lord, EnhancedLordToil toil)
		{
			return lord.CurLordToil.ThisAndEnclosingToils().Contains(toil);
		}
    }
}
