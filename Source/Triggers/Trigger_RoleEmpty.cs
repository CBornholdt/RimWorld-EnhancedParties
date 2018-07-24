using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class Trigger_RoleEmpty : Trigger
    {
        public string RoleName { get; private set; }
    
        public Trigger_RoleEmpty(string roleName)
        {
			RoleName = roleName;
        }

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if(signal.type != TriggerSignalType.Memo)
				return false;

			return signal.memo == $"LordPawnRole.{RoleName}.Empty";
		}
	}
}
