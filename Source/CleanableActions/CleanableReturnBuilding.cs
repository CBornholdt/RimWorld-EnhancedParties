using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace EnhancedParty
{
    public class CleanableReturnBuilding : ICleanableAction
    {
        Thing thing;
        IntVec3 spot;
        Rot4 rot;
        string id;

        public bool CleanupStillNeeded() => (thing.Position == spot && thing.Rotation == rot)
            || InstallBlueprintUtility.ExistingBlueprintFor(thing) != null;

        public void ExposeData()
        {
            Scribe_References.Look<Thing>(ref this.thing, "Thing");
            Scribe_Values.Look<IntVec3>(ref this.spot, "Spot", IntVec3.Invalid);
            Scribe_Values.Look<Rot4>(ref this.rot, "Rot", Rot4.Invalid);
            Scribe_Values.Look<string>(ref this.id, "ID", "Blank");
        }

        public string GetUniqueLoadID() => id;

        ThingDef returningDef => thing.GetInnerIfMinified().def;

        public void PerformCleanup()
        {
            GenSpawn.WipeExistingThings(spot, rot, returningDef.installBlueprintDef, thing.MapHeld, DestroyMode.Deconstruct);
            if(thing is MinifiedThing miniThing)
                GenConstruct.PlaceBlueprintForInstall(miniThing, spot, thing.Map, rot, thing.Faction);
            else
                GenConstruct.PlaceBlueprintForReinstall((Building)thing, spot, thing.Map, rot, thing.Faction);
            MoteMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(spot, rot, returningDef.Size), thing.MapHeld);              	
        }

        public bool ReferencesBroken() => thing == null;
    }
}
