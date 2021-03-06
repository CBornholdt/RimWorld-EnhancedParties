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
    public class DutyJob_BringThingsToFocus : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;
            EnhancedLordJob lordJob = pawn.GetLord()?.LordJob as EnhancedLordJob;

            if(duty == null || lordJob == null)
                return null;

            var potentialThings = pawn.Map.listerThings.ThingsOfDef(duty.dutyThingDef)
                                .Where(thing => !thing.IsForbidden(pawn)
                                        && !lordJob.IsCellInDutyArea(pawn, thing.PositionHeld)
                                        && pawn.CanReserveAndReach(thing, PathEndMode.ClosestTouch
                                                                    , Danger.Some));

            if(!potentialThings.Any()) {
                if(!EnhancedLordDebugSettings.disableThinkNodeLogging && EnhancedLordDebugSettings.verboseThinkNodeLogging)
                    Log.Message($"DutyJob_BringThingsToFocus: Nothing of def { duty.dutyThingDef } to bring for pawn { pawn.LabelShort }");
                return null;
            }

            var chosenCell = AvailableDestinations(duty, pawn).FirstOrDefault();
            if(chosenCell == default(IntVec3)) {
                if(!EnhancedLordDebugSettings.disableThinkNodeLogging && EnhancedLordDebugSettings.verboseThinkNodeLogging)
                    Log.Message($"DutyJob_BringThingsToFocus: No available destinations for pawn { pawn.LabelShort }");
                return null;
            }

            var chosenThing = potentialThings.MinBy(thing => thing.PositionHeld.DistanceToSquared(pawn.Position));

            int thingsAtFocus = pawn.Map.listerThings.ThingsOfDef(duty.dutyThingDef)
                                    .Where(thing => lordJob.IsCellInDutyArea(pawn, thing.PositionHeld))
                                    .Sum(thing => thing.stackCount);
                                    
            var job = new Job(JobDefOf.HaulToCell, chosenThing, chosenCell) {
                count = Math.Max(1, duty.thingCount - thingsAtFocus)
            };

            if(duty.registerForCleanup) {
                job = new JobWithAdjustment(job) {
                    adjuster = new JobAdjuster_AddCleanupAction(new Cleanable_Haulable(chosenThing))
                };
                            
            }   

            return job;
        }

        //Will prevent placement onto any cell with a Thing of the same category
        public static IEnumerable<IntVec3> AvailableDestinations(EnhancedPawnDuty duty, Pawn pawn)
        {
            bool cellValidator(IntVec3 cell) => !cell.GetThingList(pawn.Map)
                    .Any(thing => thing.def.category == duty.dutyThingDef.category)
                && pawn.CanReserveAndReach(cell, PathEndMode.ClosestTouch, Danger.Some, maxPawns: 1, stackCount: -1);

            IEnumerable<IntVec3> potentialCells = null;
            if(duty.stayInRoom)
                potentialCells = duty.focus.Cell.GetRoom(pawn.Map)?.Cells;
            if(potentialCells == null)
                potentialCells = duty.focus.Cell.PassableCellsInRadiusAround(pawn.Map, Constants.MaxOutsideSearchRadius);

            if(duty.useOnlyTables) {
                foreach(var cell in potentialCells.Where(cell => cell.HasTableAt(pawn.Map) && cellValidator(cell)))
                    yield return cell;
                yield break;
            }

            if(duty.useTablesIfPossible) {
                var noTablesHere = new Queue<IntVec3>(GenRadial.NumCellsInRadius(Constants.MaxOutsideSearchRadius));
                foreach(var cell in potentialCells.Where(cellValidator)) {
                    if(cell.HasTableAt(pawn.Map))
                        yield return cell;
                    else
                        noTablesHere.Enqueue(cell);
                }
                foreach(var cell in noTablesHere)
                    yield return cell;
                yield break;
            }

            foreach(var cell in potentialCells)
                yield return cell;
        }    
    }
}
