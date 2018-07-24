using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class Trigger_RoleTaskComplete : Trigger
    {
        public string RoleName { get; private set; }
        public string TaskName { get; private set; }
    
        public Trigger_RoleTaskComplete(string roleName, string taskName)
        {
            RoleName = roleName;
			TaskName = taskName;
        }

        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if(signal.type != TriggerSignalType.Memo)
                return false;

			return signal.memo.StartsWith($"LordPawnRole.{RoleName}.{TaskName}.Complete", StringComparison.OrdinalIgnoreCase);
        }
    }
}
