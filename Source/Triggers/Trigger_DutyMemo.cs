using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class Trigger_DutyTaskComplete : Trigger
    {
		public string TaskName { get; private set; }

		public Trigger_DutyTaskComplete(string taskName)
		{
			TaskName = taskName;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if(signal.type != TriggerSignalType.Memo || signal.memo.NullOrEmpty())
				return false;

			var memoTokens = signal.memo.Split('.');

			if(memoTokens[0] == ThinkNode_DutyTaskComplete.MemoBegin
				&& memoTokens.Last() == ThinkNode_DutyTaskComplete.MemoEnd
				&& memoTokens.Any(token => token == TaskName))
				return true;
            
			return false;   
		}
	}
}
