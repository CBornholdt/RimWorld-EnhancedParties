using System;
using System.Collections.Generic;
using Verse.AI;
using Verse;

namespace RimWorld
{
    abstract public class ThinkNode_Conditional_Else : ThinkNode
    {
        List<ThinkNode> else_subNodes = new List<ThinkNode>();

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_Conditional_Else thinkNode = (ThinkNode_Conditional_Else)base.DeepCopy(resolve);
            foreach(var elseNode in else_subNodes)
                thinkNode.else_subNodes.Add(elseNode.DeepCopy(resolve));
            return thinkNode;
        }

        //taken from ThinkNode_Priority
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            var listToTry = this.Satisfied(pawn) ? subNodes : else_subNodes;
            int count = listToTry.Count;

            for (int i = 0; i < count; i++)
            {
                ThinkResult result = ThinkResult.NoJob;
                try
                {
                    result = listToTry[i].TryIssueJobPackage(pawn, jobParams);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Concat(new object[]
                    {
                        "Exception in ",
                        base.GetType(),
                        " TryIssueJobPackage: ",
                        ex.ToString()
                    }), false);
                }
                if (result.IsValid)
                {
                    return result;
                }
            }
            return ThinkResult.NoJob;
        }
        
        protected abstract bool Satisfied(Pawn pawn);
    }
}
