using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace EnhancedParty
{
    public class Cleanable_Haulable : ICleanableAction
    {
        public Thing haulable;
        string id;
    
        public Cleanable_Haulable(Thing haulable)
        {
            this.haulable = haulable;
            this.id = haulable.GetUniqueLoadID() + "_CA";
        }

        public void AssignCleanupToPawn(Pawn pawn, bool addRemoveDutyWrapper = true)
        {
            PerformCleanup();
            var job = HaulAIUtility.HaulToStorageJob(pawn, haulable);
            if(job != null)
                pawn.jobs.StartJob(job, lastJobEndCondition: JobCondition.InterruptForced, jobGiver: null
                                    , resumeCurJobAfterwards: false, cancelBusyStances: true, thinkTree: null
                                    , tag: JobTag.UnspecifiedLordDuty, fromQueue: false);     
        }

        public bool CleanupStillNeeded()
        {
            return haulable != null && !StoreUtility.IsInValidStorage(haulable);
        }

        public void ExposeData()
        {
            Scribe_References.Look<Thing>(ref this.haulable, "Haulable");
            Scribe_Values.Look<string>(ref this.id, "ID", "Blank");
        }

        public string GetUniqueLoadID() => id;
        
        public void PerformCleanup()
        {
            haulable?.Map.designationManager.AddDesignation(new Designation(haulable, DesignationDefOf.Haul));
        }

        public bool ReferencesBroken() => haulable == null;
    }
}
