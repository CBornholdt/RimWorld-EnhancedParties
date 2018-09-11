using System;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class TransitionAction_SendMemo : TransitionAction
    {
        string memo;
    
        public TransitionAction_SendMemo(string memo)
        {
            this.memo = memo;
        }

        public override void DoAction(Transition trans)
        {
            trans.target.lord.ReceiveMemo(memo);
        }
    }
}
