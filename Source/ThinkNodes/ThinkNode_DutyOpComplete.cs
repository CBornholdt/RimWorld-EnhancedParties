using System;
using Verse;
using Verse.AI;
using EnhancedParty;
using Verse.AI.Group;

namespace RimWorld
{
    public class ThinkNode_DutyOpComplete : ThinkNode
    {
        public bool repeatProtection = true;

        bool alreadyTriggered = false;
    
        public ThinkNode_DutyOpComplete()
        {
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode node = new ThinkNode_DutyOpComplete() {
                repeatProtection = this.repeatProtection
            };
            return node;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;
        
            if(duty == null || (repeatProtection && alreadyTriggered))
                return ThinkResult.NoJob;

            alreadyTriggered = true;

            DutyOpUtility.Notify_DutyOpComplete(duty.taskName, pawn);
        
            return ThinkResult.NoJob;
        }
    }
}
