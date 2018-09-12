using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class ThinkNode_EnhancedDuty : ThinkNode
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (pawn.GetLord() == null)
            {
                Log.Error(pawn + " doing ThinkNode_Duty with no Lord.", false);
                return ThinkResult.NoJob;
            }
            if (pawn.mindState.duty == null)
            {
                Log.Error(pawn + " doing ThinkNode_Duty with no duty.", false);
                return ThinkResult.NoJob;
            }
        //	if(pawn.mindState.duty is EnhancedPawnDuty duty && duty.localThinkTreeCopy != null)
        //		return duty.localThinkTreeCopy.TryIssueJobPackage(pawn, jobParams);
            
            return this.subNodes[(int)pawn.mindState.duty.def.index].TryIssueJobPackage(pawn, jobParams);
        }

        protected override void ResolveSubnodes()
        {
            foreach (DutyDef current in DefDatabase<DutyDef>.AllDefs)
            {
                current.thinkNode.ResolveSubnodesAndRecur();
                this.subNodes.Add(current.thinkNode.DeepCopy(true));
            }
        }
    }
}
