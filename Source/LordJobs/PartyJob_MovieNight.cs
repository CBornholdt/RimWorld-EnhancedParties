using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;
using Harmony;

namespace EnhancedParty
{
    public class PartyJob_MovieNight : EnhancedLordJob_Party
    {
        protected MovieNight_PrepareToil prepareToil;
        protected MovieNight_PartyToil partyToil;

        protected Thing television;

        public static readonly string Viewers = "Viewers";
        public static readonly string ChairMovers = "ChairMovers";

        public class Cell_Pawn_Chair_Tuple : IExposable
        {
            public IntVec3 cell;
            public Pawn pawn;
            public Thing chair;

            public Cell_Pawn_Chair_Tuple(IntVec3 cell, Pawn pawn, Thing chair)
            {
                this.pawn = pawn;
                this.cell = cell;
                this.chair = chair;
            }

            public void ExposeData()
            {
                Scribe_Values.Look<IntVec3>(ref this.cell, "Cell", IntVec3.Invalid);
                Scribe_References.Look<Pawn>(ref this.pawn, "Pawn");
                Scribe_References.Look<Thing>(ref this.chair, "Chair");
            }
        }

        public List<Cell_Pawn_Chair_Tuple> seatingAssignments = new List<Cell_Pawn_Chair_Tuple>();
        
        public Rot4 viewingDirection;

        public PartyJob_MovieNight(EnhancedPartyDef def, Pawn organizer, IntVec3 spot) : base(def, organizer, spot)
        {
            this.television = spot.GetThingList(organizer.Map).FirstOrDefault(ThingExt.IsTelevision);
            InitializeSeatingAssignments();
        }

        public PartyJob_MovieNight() : base()
        {
        }

        protected void InitializeSeatingAssignments()
        {
            if(television.DestroyedOrNull())
                return;
            seatingAssignments.Clear();
            seatingAssignments.AddRange(WatchBuildingUtility.CalculateWatchCells(television.def, television.Position
                                                                                    , television.Rotation, television.Map)
                                                            .Select(cell => new Cell_Pawn_Chair_Tuple(cell, null, null)));
            viewingDirection = television.Rotation.Opposite;
        }

        public IntVec3 GetAssignedSeating(Pawn pawn)
        {
            return seatingAssignments.FirstOrDefault(cellPawnTuple => cellPawnTuple.pawn == pawn)?.cell
                    ?? AssignNewSeatIfPossible(pawn);
        }

        public IntVec3 AssignNewSeatIfPossible(Pawn pawn)
        {
            Thing chair = null;
            var tuple = seatingAssignments.FirstOrDefault(cellPawnTuple => cellPawnTuple.pawn == null
                                                            && (chair = cellPawnTuple.cell.GetThingList(Map).FirstOrDefault(ThingExt.IsChair))
                                                                != null);
            if(tuple != null) {
                tuple.pawn = pawn;
                tuple.chair = chair;
                return tuple.cell;
            }
            tuple = seatingAssignments.FirstOrDefault(cellPawnTuple => cellPawnTuple.pawn == null);
            if(tuple != null)
                tuple.pawn = pawn;
            return tuple?.cell ?? IntVec3.Invalid;          
        }

        public Thing GetAssignedChair(Pawn pawn)
        {
            return seatingAssignments.FirstOrDefault(cellPawnTuple => cellPawnTuple.pawn == pawn)?.chair
                    ?? AssignNewChairIfPossible(pawn);
        }

        public Thing AssignNewChairIfPossible(Pawn pawn)
        {
            var tuple = seatingAssignments.FirstOrDefault(cellPawnTuple => cellPawnTuple.pawn == pawn);
            if(tuple == null || !tuple.cell.IsValid)
                return null;
            var chairOnSeatAssignment = tuple.cell.GetThingList(Map).FirstOrDefault(thing => thing.def.building?.isSittable ?? false);
            if(chairOnSeatAssignment != null)
                return (tuple.chair = chairOnSeatAssignment);
            return (tuple.chair = FindChairToMove(pawn, tuple.cell));
        }
        
        public Thing FindChairToMove(Pawn pawn, IntVec3 cell)
        {
            //All sittable things not already in seating assignments with valid pawn
            var availableChairs = Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                                        .Where(thing => thing.def.building?.isSittable ?? false)
                                        .Except(seatingAssignments.Where(seat => seat.pawn != null && seat.chair != null)
                                                                  .Select(seat => seat.chair))
                                        .Where(thing => pawn.CanReserveAndReach(thing, PathEndMode.Touch, pawn.NormalMaxDanger()
                                                            , maxPawns: 1, stackCount: -1, layer: null, ignoreOtherReservations: false));

            

            if(!availableChairs.Any()) {
                Log.Message($"Could not find available chair for pawn {pawn.Name}");
                return null;
            }
            
            var t = availableChairs.MinBy(thing => thing.Position.DistanceToSquared(cell));
            
            Log.Message($"Trying for { pawn.Name.ToStringShort }   Thing Position { t?.Position.ToString() ?? "NONE"}");
            return t;
        }

        public bool IsSeatingAvailable() =>
            seatingAssignments.Any(cellPawnTuple => cellPawnTuple.pawn == null);

        protected override EnhancedLordToil_Party PartyToil => partyToil;

        protected override EnhancedLordToil_PrepareParty PrepareToil => prepareToil;

        public override bool AllowedToOrganize(Pawn pawn) =>
            pawn.RaceProps.Humanlike && !pawn.InBed() && !pawn.InMentalState
                && pawn.GetLord() == null
                && pawn.AbleToStopJobForParty()
                && EnhancedPartyUtility.ShouldPawnKeepPartyingBasicChecks(pawn);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Cell_Pawn_Chair_Tuple>(ref this.seatingAssignments, "SeatingAssignments");
                                                    
        }

        override public bool PartyCanBeHadWith(Faction faction, Map map)
        {   //Ensures that RecRoom exists and has TV
            bool value = PartyUtility.AcceptableGameConditionsToStartParty(map)
                        && map.regionGrid.allRooms.Any(room => room.Role == RoomRoleDefOf.RecRoom
                                                        && room.ThingsInside().Any(thing => (thing.def.building?.joyKind ?? null)
                                                                                        == MoreJoyKindDefs.Television))
                        && map.mapPawns.FreeColonistsSpawned.Count() >= def.minNumOfPartiers;
            Log.Message($"PartyCanBeHadWith: {value}   Party: {def.label}");                    
    
            return PartyUtility.AcceptableGameConditionsToStartParty(map)
                        && map.regionGrid.allRooms.Any(room => room.Role == RoomRoleDefOf.RecRoom)
                        && map.mapPawns.FreeColonistsSpawned.Count() >= def.minNumOfPartiers;
        }

        public override bool TryGetOrganizerAndStartingSpot(Faction faction, Map map, out Pawn organizer, out IntVec3 startingSpot)
        {
            var potentialOrganizers = map.mapPawns.SpawnedPawnsInFaction(faction)
                        .Where(pawn => AllowedToOrganize(pawn)).ToList();
            potentialOrganizers.Shuffle();
        
            var recRoomTuples = map.regionGrid.allRooms
                .Where(room => room.Role == RoomRoleDefOf.RecRoom)
                .Select(room => Tuple.Create(room, room.ThingsInside()
                                                        .Where(thing => thing.def.building?.joyKind == MoreJoyKindDefs.Television
                                                                         && thing.HasPower())
                                                        .ToList()))
                .Where(roomTvListTuple => roomTvListTuple.Item2.Any())
                .OrderByDescending(roomTvListTuple => roomTvListTuple.Item1.GetStat(RoomStatDefOf.Beauty));                                                   
        
            foreach(var potentialOrganizer in potentialOrganizers) {
                foreach(var recRoomTuple in recRoomTuples) {  
                    if(recRoomTuple.Item2.Any()) {
                        startingSpot = recRoomTuple.Item2.MaxBy(tv => WatchBuildingUtility.CalculateWatchCells(tv.def, tv.Position
                                                                        , tv.Rotation, tv.Map).Count()).Position;
                        if(potentialOrganizer.CanReach(startingSpot, PathEndMode.Touch, potentialOrganizer.NormalMaxDanger())) {
                            organizer = potentialOrganizer;
                            return true;
                        }
                    }   
                }
            }
        
            startingSpot = IntVec3.Invalid;
            organizer = null;
            return false;
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            float priority = base.VoluntaryJoinPriorityFor(p);

            if(EnhancedLordDebugSettings.logJoinPriorities)
                Log.Message($"{this.GetType().Name} VoluntaryJoinPriorityFor Pawn: {p.Name} is {priority}");

            return priority;
        }

        protected override void CreatePartyRolesAndToils()
        {
            var viewers = new LordPawnRole(Viewers, this);   
            roles.Add(viewers);

            var chairMovers = new LordPawnRole(ChairMovers, this);
            roles.Add(chairMovers);
            
            prepareToil = new MovieNight_PrepareToil(Def);
            partyToil = new MovieNight_PartyToil(Def);
        }

        public override bool ShouldBeCalledOff()
        {
            //Log.Message($"Television position { television.Position }  CurrentPartySpot { this.currentPartySpot }   Television Poweron { (television.TryGetComp<CompPowerTrader>()?.PowerOn ?? true) }");
        
            var result = base.ShouldBeCalledOff()
                || television.DestroyedOrNull()
                || television.IsBurning()
                || !(television.TryGetComp<CompPowerTrader>()?.PowerOn ?? true)
                || television.Position != this.currentPartySpot;

            return result;
        }

        public bool EnoughChairsAvailable() =>
            Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                .Where(thing => thing.def.building?.isSittable ?? false)
                .Count() >= (lord.ownedPawns.Count + 1);

        public override bool IsInvited(Pawn pawn)
        {
            return base.IsInvited(pawn) && IsSeatingAvailable() && EnoughChairsAvailable();
        }

        public override void LogDebuggingInfo()
        {
            LogChairAssignments();
        }

        public void LogChairAssignments()
        {
            foreach(var pawnCellChair in seatingAssignments) {
                Log.Message($"Pawn { pawnCellChair.pawn?.Name.ToStringShort ?? "None" }   Seat { pawnCellChair.cell }   Chair { pawnCellChair.chair?.PositionHeld.ToString() ?? "NONE"}");
            }
        }
    }
}
