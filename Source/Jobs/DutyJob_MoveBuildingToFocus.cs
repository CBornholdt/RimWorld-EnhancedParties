﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;
using EnhancedParty;
using Harmony;

namespace RimWorld
{
    public class DutyJob_MoveBuildingToFocus : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;
            EnhancedLordJob lordJob = pawn.GetLord()?.LordJob as EnhancedLordJob;

            Log.Message("Moving building");

            if(duty == null || lordJob == null)
                return null;

            Thing buildingToMove = duty.focus.Thing;
            if(buildingToMove == null || !pawn.CanReserveAndReach(duty.focusSecond, PathEndMode.OnCell
                                            , pawn.NormalMaxDanger(), maxPawns: 1, stackCount: -1))
                return null;                                            

            Blueprint blueprint = InstallBlueprintUtility.ExistingBlueprintFor(buildingToMove);

            if(blueprint != null && blueprint.Position != duty.focusSecond.Cell && blueprint.Rotation != duty.direction) 
                blueprint.Destroy(DestroyMode.Cancel);

            if(blueprint.DestroyedOrNull()) {   //Create blueprint
                if(!GenConstruct.CanPlaceBlueprintAt(buildingToMove.def
                                                        , duty.focusSecond.Cell, duty.direction, pawn.Map).Accepted)
                    return null;
            
                if(buildingToMove is MinifiedThing miniThing)
                    blueprint = GenConstruct.PlaceBlueprintForInstall(miniThing, duty.focusSecond.Cell
                                                                        , pawn.Map, duty.direction, pawn.Faction);
                else {
                    if(buildingToMove is Building building)
                        blueprint = GenConstruct.PlaceBlueprintForReinstall(building, duty.focusSecond.Cell
                                                                            , pawn.Map, duty.direction, pawn.Faction);
                }

                if(blueprint.DestroyedOrNull())
                    return null;

                if(duty.registerForCleanup)
                    lordJob.RegisterCleanupAction(new Cleanable_ReturnBuilding(buildingToMove, original: buildingToMove.PositionHeld
                                                                    , destination: duty.focusSecond.Cell, rot: buildingToMove.Rotation));
            }

            return HaulAIUtility.HaulToContainerJob(pawn, buildingToMove, blueprint);
        }
    }
}
