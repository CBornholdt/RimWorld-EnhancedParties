using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class JobAdjuster_AddCleanupAction : JobDriverAdjuster
    {
        public ICleanableAction action;

        public JobAdjuster_AddCleanupAction(ICleanableAction action)
        {
            this.action = action;
        }

        public JobAdjuster_AddCleanupAction()
        {

        }
    
        public override List<Action> ProcessGlobalFinishActions(List<Action> actions, JobDriver driver)
        {
            actions.Add(() => driver.pawn.GetEnhancedLordJob()?.RegisterCleanupAction(action));
            return actions;
        }

        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ICleanableAction>(ref this.action, "Action");
        }
    }
}
