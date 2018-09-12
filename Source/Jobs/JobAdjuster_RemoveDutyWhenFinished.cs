using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace EnhancedParty
{
    public class JobAdjuster_RemoveDutyWhenFinished : JobDriverAdjuster
    {
        public override List<Action> ProcessGlobalFinishActions(List<Action> actions, JobDriver driver)
        {
            actions.Add(() => driver.pawn.mindState.duty = null);
            return actions;
        }
    }
}
