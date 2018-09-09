using System;
using Verse;
using Verse.AI;
using EnhancedParty;
using Verse.AI.Group;

namespace RimWorld
{
    public class ThinkNode_RoleDutyTaskComplete : ThinkNode
    {
        static readonly public string MemoBegin = "Duty";
        static readonly public string MemoEnd = "Complete";
    
        public bool addPawnNameToMemo = false;
        public bool repeatProtection = true;

        bool alreadyTriggered = false;
    
        public ThinkNode_RoleDutyTaskComplete()
        {
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode node = new ThinkNode_RoleDutyTaskComplete() {
                addPawnNameToMemo = this.addPawnNameToMemo,
                repeatProtection = this.repeatProtection
            };
            return node;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if(repeatProtection && alreadyTriggered)
                return ThinkResult.NoJob;

            string pawnNamePart = (addPawnNameToMemo) ? pawn.Name + "." : string.Empty;
            string taskName = (pawn.mindState.duty as EnhancedPawnDuty)?.taskName;
            string taskNamePart = (taskName != null) ? taskName + "." : string.Empty;
            string memo = MemoBegin + "." + pawnNamePart + taskNamePart + MemoEnd;
            pawn.GetLord().ReceiveMemo(memo);

            alreadyTriggered = true;
        
            return ThinkResult.NoJob;
        }
    }
}
