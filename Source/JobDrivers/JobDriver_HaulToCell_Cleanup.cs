using System;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace EnhancedParty
{
    public class JobDriver_HaulToCell_Cleanup : JobDriver_HaulToCell
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var baseToils = base.MakeNewToils().ToList();
            var toilToWrap = baseToils[baseToils.Count - 2];    //magical index
            Action oldInitAction = toilToWrap.initAction;
            toilToWrap.initAction = () => {
                oldInitAction?.Invoke();
                toilToWrap.actor.GetEnhancedLordJob().RegisterCleanupAction(new Cleanable_Haulable
                    (toilToWrap.actor.carryTracker.CarriedThing));
            };
            return baseToils;
        }
    }
}
