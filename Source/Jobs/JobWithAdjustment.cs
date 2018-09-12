using System;
using Verse;
using Verse.AI;

namespace EnhancedParty
{
    public class JobWithAdjustment : Job
    {
        public JobDriverAdjuster adjuster;
    
        public JobWithAdjustment() : base() { }

        public JobWithAdjustment(JobDef def) : base(def) { }

        public JobWithAdjustment(JobDef def, LocalTargetInfo targetA) : this(def, targetA, null) { }

        public JobWithAdjustment(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB) : base(def, targetA, targetB) { }

        public JobWithAdjustment(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB, LocalTargetInfo targetC)
            : base(def, targetA, targetB, targetC) { }

        public JobWithAdjustment(JobDef def, LocalTargetInfo targetA, int expiryInterval, bool checkOverrideOnExpiry = false)
            : base(def, targetA, expiryInterval, checkOverrideOnExpiry) { }

        public JobWithAdjustment(JobDef def, int expiryInterval, bool checkOverrideOnExpiry = false)
            : base(def, expiryInterval, checkOverrideOnExpiry) { }

        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<JobDriverAdjuster>(ref this.adjuster, "Adjuster");
        }     
    }
}
