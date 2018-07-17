using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class Trigger_DutyMemo : Trigger
    {
        private string receivedDuty;
        static public readonly string dutyMemoMarker = "DutyMemo.";

		public DutyDef ReceivedDuty => DefDatabase<DutyDef>.GetNamed(this.receivedDuty);

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if(signal.type == TriggerSignalType.Memo
				&& signal.memo.StartsWith(dutyMemoMarker, StringComparison.Ordinal)) {
				this.receivedDuty = signal.memo.Substring(dutyMemoMarker.Length);
				return true;
			}
			return false;   
		}
	}
}
