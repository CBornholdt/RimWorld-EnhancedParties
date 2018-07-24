using System;

namespace Verse.AI
{
    public class ThinkNode_Logger : ThinkNode
    {     
        public override ThinkNode DeepCopy(bool resolve = true)
        {
			ThinkNode_Logger thinkNode = new ThinkNode_Logger();
            return thinkNode;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams p)
        {
			Log.Message($"Hitting ThinkNode_Logger for {pawn.Name}");
			return ThinkResult.NoJob;
        }
    }
}