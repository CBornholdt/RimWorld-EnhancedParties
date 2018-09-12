using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace EnhancedParty
{
    public class Cleanable_Blueprint : ICleanableAction
    {
        Thing blueprint;
        string id;
    
        public Cleanable_Blueprint(Thing blueprint)
        {
            this.blueprint = blueprint;
            this.id = blueprint.GetUniqueLoadID() + "_blueprint_CA";
        }

        public void AssignCleanupToPawn(Pawn pawn) => PerformCleanup();

        public bool CleanupStillNeeded() => !blueprint.DestroyedOrNull();

        public void ExposeData()
        {
            Scribe_References.Look<Thing>(ref this.blueprint, "Blueprint");
            Scribe_Values.Look<string>(ref this.id, "ID", "Blank");
        }

        public string GetUniqueLoadID() => id;

        public void PerformCleanup() => blueprint?.Destroy(DestroyMode.Cancel);

        public bool ReferencesBroken() => blueprint == null;
    }
}
