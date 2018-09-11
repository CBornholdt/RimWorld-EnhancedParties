using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace EnhancedParty
{
    public class Cleanable_ReturnBuilding : ICleanableAction
    {
        Thing thing;
        IntVec3 original;
        IntVec3 destination;
        Rot4 rot;
        string id;

        public Cleanable_ReturnBuilding(Thing thing, IntVec3 original, IntVec3 destination, Rot4 rot)
        {
            this.thing = thing;
            this.original = original;
            this.destination = destination;
            this.rot = rot;
            this.id = thing.GetUniqueLoadID() + original.ToString() + "_CA";
        }

        public bool CleanupStillNeeded()
        {
            Thing blueprint = InstallBlueprintUtility.ExistingBlueprintFor(thing);
            if(blueprint == null) {
                if(thing.PositionHeld == original && thing.Rotation == rot)
                    return false;
                return true;
            }

            if(thing.PositionHeld == original && thing.Rotation == rot)
                return blueprint.Position == destination;

            if(blueprint.Position == original)
                return false;

            return true;
        }

        public void ExposeData()
        {
            Scribe_References.Look<Thing>(ref this.thing, "Thing");
            Scribe_Values.Look<IntVec3>(ref this.original, "Original", IntVec3.Invalid);
            Scribe_Values.Look<IntVec3>(ref this.destination, "Destination", IntVec3.Invalid);
            Scribe_Values.Look<Rot4>(ref this.rot, "Rot", Rot4.Invalid);
            Scribe_Values.Look<string>(ref this.id, "ID", "Blank");
        }

        public string GetUniqueLoadID() => id;

        ThingDef returningDef => thing.GetInnerIfMinified().def;

        public void PerformCleanup()
        {
            Thing blueprint = InstallBlueprintUtility.ExistingBlueprintFor(thing);

            if(thing.PositionHeld == original && thing.Rotation == rot) {
                if(blueprint != null && blueprint.Position == destination)
                    blueprint.Destroy(DestroyMode.Cancel);
                return;
            }   //At correct place, removed any blueprints to destination

            if(blueprint != null && blueprint.Position == original && blueprint.Rotation == rot)
                return; //Blueprint on original location

            if(blueprint != null && (blueprint.Position == original
                || GenConstruct.CanPlaceBlueprintAt(thing.def, original, rot, thing.Map).Accepted)) {
                blueprint.Destroy(DestroyMode.Cancel);
                blueprint = null;
            }   //Blueprint not in correct location and can create blueprint at proper place, destroy blueprint

            if(blueprint != null)   //Blueprint exists, but could not place new one on original space. Micro maybe?
                return;
            
            if(thing is MinifiedThing miniThing)
                GenConstruct.PlaceBlueprintForInstall(miniThing, original, thing.Map, rot, thing.Faction);
            else
                GenConstruct.PlaceBlueprintForReinstall((Building)thing, original, thing.Map, rot, thing.Faction);
            MoteMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(original, rot, returningDef.Size), thing.MapHeld);              	
        }

        public bool ReferencesBroken() => thing == null;
    }
}
