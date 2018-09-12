using System;
using Verse;
using Verse.AI;

namespace EnhancedParty
{
    public class JobWithDutyMessage : Job
    {
		public DutyOpMessageType messageType;
		public string opName;
    
        public JobWithDutyMessage(DutyOpMessageType messageType, string opName)
        {
			this.messageType = messageType;
			this.opName = opName;
        }

		public new void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<DutyOpMessageType>(ref this.messageType, "MessageType", DutyOpMessageType.OpFailed);
			Scribe_Values.Look<string>(ref this.opName, "OpName", "Blank");
		}
    }
}
