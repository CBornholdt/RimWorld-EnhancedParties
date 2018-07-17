using System;
using Verse;
using Verse.AI;

namespace EnhancedParty
{
    static public class PawnExt
    {
		static public bool AbleToStopJobForParty(this Pawn pawn)
		{
			return !(pawn.CurJob.targetA.Thing is Pawn
						|| pawn.CurJob.targetB.Thing is Pawn
						|| pawn.CurJob.targetC.Thing is Pawn) 
                   && pawn.jobs.jobQueue.Count == 0;
		}
    }
}
