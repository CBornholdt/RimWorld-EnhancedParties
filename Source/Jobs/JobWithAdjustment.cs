using System;
using Verse;
using Verse.AI;
using Harmony;

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

        public JobWithAdjustment(Job baseJob) : base()
        {
            this.def = baseJob.def;
            this.targetA = baseJob.targetA;
            this.targetB = baseJob.targetB;
            this.targetC = baseJob.targetC;
            this.targetQueueA = baseJob.targetQueueA;
            this.targetQueueB = baseJob.targetQueueB;
            this.count = baseJob.count;
            this.countQueue = baseJob.countQueue;
            this.loadID = baseJob.loadID;
            this.startTick = baseJob.startTick;
            this.expiryInterval = baseJob.expiryInterval;
            this.checkOverrideOnExpire = baseJob.checkOverrideOnExpire;
            this.playerForced = baseJob.playerForced;
            this.placedThings = baseJob.placedThings;
            this.maxNumMeleeAttacks = baseJob.maxNumMeleeAttacks;
            this.maxNumStaticAttacks = baseJob.maxNumStaticAttacks;
            this.locomotionUrgency = baseJob.locomotionUrgency;
            this.haulMode = baseJob.haulMode;
            this.bill = baseJob.bill;
            this.commTarget = baseJob.commTarget;
            this.plantDefToSow = baseJob.plantDefToSow;
            this.verbToUse = baseJob.verbToUse;
            this.haulOpportunisticDuplicates = baseJob.haulOpportunisticDuplicates;
            this.exitMapOnArrival = baseJob.exitMapOnArrival;
            this.failIfCantJoinOrCreateCaravan = baseJob.failIfCantJoinOrCreateCaravan;
            this.killIncappedTarget = baseJob.killIncappedTarget;
            this.ignoreForbidden = baseJob.ignoreForbidden;
            this.ignoreDesignations = baseJob.ignoreDesignations;
            this.canBash = baseJob.canBash;
            this.haulDroppedApparel = baseJob.haulDroppedApparel;
            this.restUntilHealed = baseJob.restUntilHealed;
            this.ignoreJoyTimeAssignment = baseJob.ignoreJoyTimeAssignment;
            this.overeat = baseJob.overeat;
            this.attackDoorIfTargetLost = baseJob.attackDoorIfTargetLost;
            this.takeExtraIngestibles = baseJob.takeExtraIngestibles;
            this.expireRequiresEnemiesNearby = baseJob.expireRequiresEnemiesNearby;
            this.lord = baseJob.lord;
            this.collideWithPawns = baseJob.collideWithPawns;
            this.forceSleep = baseJob.forceSleep;
            this.interaction = baseJob.interaction;
            this.endIfCantShootTargetFromCurPos = baseJob.endIfCantShootTargetFromCurPos;
            this.checkEncumbrance = baseJob.checkEncumbrance;
            this.followRadius = baseJob.followRadius;
            this.endAfterTendedOnce = baseJob.endAfterTendedOnce;
            JobDriver driver = Traverse.Create(baseJob).Field("cachedDriver").GetValue<JobDriver>();
            if(driver != null) {
                driver.job = this;
                Traverse.Create(this).Field("cachedDriver").SetValue(driver);
            }
        }
            
        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<JobDriverAdjuster>(ref this.adjuster, "Adjuster");
        }     
    }
}
