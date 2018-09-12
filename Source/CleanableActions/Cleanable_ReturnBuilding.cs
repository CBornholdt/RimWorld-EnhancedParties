using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace EnhancedParty
{
    public class Cleanable_ReturnBuilding : ICleanableAction
    {
        public Thing thing;
        public IntVec3 original;
        public IntVec3 destination;
        public Rot4 rot;
        public string id;

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

        protected Blueprint CreateBlueprintForCleanup()
        {
            Thing blueprint = InstallBlueprintUtility.ExistingBlueprintFor(thing);

            if(thing.Spawned && thing.Position == original && thing.Rotation == rot) {
                if(blueprint != null && blueprint.Position == destination)
                    blueprint.Destroy(DestroyMode.Cancel);
                return null;
            }   //At correct place, removed any blueprints to destination

            Thing buildingToMove = thing;
            if(!buildingToMove.Spawned) {
                if(buildingToMove.ParentHolder is MinifiedThing mini)
                    buildingToMove = mini;
                else {
                    if(!EnhancedLordDebugSettings.disableThinkNodeLogging)
                        Log.Message($"Cleanable_ReturnBuilding, thing {thing.ThingID} isn't spawned or minified");
                    return null;
                }
            }   

            if(blueprint != null && blueprint.Position == original && blueprint.Rotation == rot)
                return null; //Blueprint on original location

            if(blueprint != null && (blueprint.Position == original
                || GenConstruct.CanPlaceBlueprintAt(thing.def, original, rot, thing.Map
                                                    , godMode: false, thingToIgnore: thing).Accepted)) {
                blueprint.Destroy(DestroyMode.Cancel);
                blueprint = null;
            }   //Blueprint not in correct location and can create blueprint at proper place, destroy blueprint

            if(blueprint != null)   //Blueprint exists, but could not place new one on original space. Micro maybe?
                return null;

            Blueprint newBlueprint;

            if(buildingToMove is MinifiedThing miniThing)
                newBlueprint = (Blueprint)GenConstruct.PlaceBlueprintForInstall(miniThing, original, thing.Map, rot, thing.Faction);
            else 
                newBlueprint = (Blueprint)GenConstruct.PlaceBlueprintForReinstall((Building)buildingToMove, original, thing.Map, rot, thing.Faction);
                                                        
            MoteMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(original, rot, returningDef.Size), thing.MapHeld);

            return newBlueprint;          
        }

        public void PerformCleanup()
        {
            CreateBlueprintForCleanup();
        }

        public bool ReferencesBroken() => thing == null;

        public void AssignCleanupToPawn(Pawn pawn)
        {
            var blueprint = CreateBlueprintForCleanup();
            if(blueprint == null)
                return;
            var job = DutyJob_MoveBuildingToFocus_Helper.intWorkGiver.JobOnThing(pawn, blueprint);
            if(job != null) {
                Log.Message($"Starting pawn cleanup for {pawn.LabelShort}");
                pawn.jobs.StartJob(job, lastJobEndCondition: JobCondition.InterruptForced, jobGiver: null
                                    , resumeCurJobAfterwards: false, cancelBusyStances: true, thinkTree: null
                                    , tag: JobTag.UnspecifiedLordDuty, fromQueue: false);
            }
        }
    }
}
