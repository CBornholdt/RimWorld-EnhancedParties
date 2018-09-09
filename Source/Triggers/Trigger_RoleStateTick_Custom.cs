using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class Trigger_RoleStateTick_Custom : Trigger
    {
        public string RoleName { get; private set; }
        public Func<LordPawnRole, bool> Condition { get; private set; }
    
        public Trigger_RoleStateTick_Custom(string roleName, Func<LordPawnRole, bool> condition)
        {
            RoleName = roleName;
            Condition = condition;
        }

        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if(signal.type != TriggerSignalType.Tick)
                return false;

            EnhancedLordJob lordJob = lord.LordJob as EnhancedLordJob;
            if(lordJob == null) {
                Log.ErrorOnce($"Set Trigger_RoleEmpty but LordJob is not of type LordJob_JoinableRoles. It is a {lord.LordJob.GetType().Name}", 2323896);
                return false;
            }

            if(lordJob.TryGetRole(RoleName, out LordPawnRole role))
                return Condition(role);

            Log.ErrorOnce($"Attempted to activate {this.GetType().Name} on non-existent role {RoleName}", 428191);
            return false;
        }
    }
}
