using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace EnhancedParty
{
    public class CleanableHaulable : ICleanableAction
    {
		public Thing haulable;
		string id;
    
        public CleanableHaulable(Thing haulable)
        {
			this.haulable = haulable;
			this.id = haulable.GetUniqueLoadID() + "_CA";
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
