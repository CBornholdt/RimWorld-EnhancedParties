using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace EnhancedParty
{
    public abstract class JobDriverAdjuster : IExposable
    {
        public virtual void ExposeData() { }
        public virtual IEnumerable<Toil> ProcessToils(IEnumerable<Toil> toils) => toils;
        public virtual List<Action> ProcessGlobalFinishActions(List<Action> actions, JobDriver driver) => actions;
        public virtual List<Func<JobCondition>> ProcessGlobalFailConditions
            (List<Func<JobCondition>> conditions, JobDriver driver) => conditions;
        
    }
}
