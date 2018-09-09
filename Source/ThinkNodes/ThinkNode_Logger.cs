using System;
using EnhancedParty;

namespace Verse.AI
{
    public class ThinkNode_Logger : ThinkNode_Priority
    {     
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_Logger thinkNode = (ThinkNode_Logger)base.DeepCopy(resolve);
            return thinkNode;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams p)
        {
            if(!EnhancedLordDebugSettings.disableThinkNodeLogging) {
                // if(EnhancedLordDebugSettings.verboseThinkNodeLogging)
                Log.Message($"ThinkNode_Logger for {pawn.Name} with role {pawn.GetLordPawnRole()?.name ?? "NONE"}");
            }
            return base.TryIssueJobPackage(pawn, p);
        }
    }
}