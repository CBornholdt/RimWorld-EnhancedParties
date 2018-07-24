using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;


namespace EnhancedParty
{
    public class PartyWorker_RecRoom : EnhancedPartyWorker
    {
        public PartyWorker_RecRoom(EnhancedPartyDef def, LordJob_EnhancedParty lordJob) 
            : base(def, lordJob) { }
    
        override public bool PartyCanBeHadWith(Faction faction, Map map)
        {   
            bool value = PartyUtility.AcceptableGameConditionsToStartParty(map)
                        && map.regionGrid.allRooms.Any(room => room.Role == RoomRoleDefOf.RecRoom)
                        && map.mapPawns.FreeColonistsSpawned.Count() >= def.minNumOfPartiers;
            Log.Message($"PartyCanBeHadWith: {value}   Party: {def.label}");                    
    
            return PartyUtility.AcceptableGameConditionsToStartParty(map)
                        && map.regionGrid.allRooms.Any(room => room.Role == RoomRoleDefOf.RecRoom)
                        && map.mapPawns.FreeColonistsSpawned.Count() >= def.minNumOfPartiers;
        }

		override public bool AllowedToOrganize(Pawn pawn) =>
			pawn.RaceProps.Humanlike && !pawn.InBed() && !pawn.InMentalState
				&& pawn.GetLord() == null
				&& pawn.AbleToStopJobForParty()
				&& PartyUtility.ShouldPawnKeepPartying(pawn);

        public override bool TryGetOrganizerAndStartingSpot(Faction faction, Map map, out Pawn organizer, out IntVec3 startingSpot)
        {
            var potentialOrganizers = map.mapPawns.SpawnedPawnsInFaction(faction)
                        .Where(pawn => AllowedToOrganize(pawn)).ToList();
            potentialOrganizers.Shuffle();
            Log.Message($"Party: {def.label}  Potential Organizers: {potentialOrganizers.Count}");

            var recRooms = map.regionGrid.allRooms.Where(room => room.Role == RoomRoleDefOf.RecRoom)
                                                        //    && room.RegionType == RegionType.Set_Passable)
                                                  .OrderByDescending(room => room.GetStat(RoomStatDefOf.Beauty));                                                   

            foreach(var potentialOrganizer in potentialOrganizers) {
                foreach(var recRoom in recRooms) {  //TODO speed
                    Log.Message($"Party: {def.label}   Non Edge Recroom Cells: {recRoom.Cells.Where(cell => !cell.OnRoomEdge(recRoom)).Count()}");
                
                    if(recRoom.Cells.Where(cell => !cell.OnRoomEdge(recRoom)
                                        && potentialOrganizer.CanReserveAndReach(cell, PathEndMode.Touch
                                                                , potentialOrganizer.NormalMaxDanger(), maxPawns: 4))
                                    .TryRandomElement(out startingSpot)){
                        organizer = potentialOrganizer;
                        return true;
                    }
                }
            }

            startingSpot = IntVec3.Invalid;
            organizer = null;
            return false;
        }

		public Room PartyRoom => this.lordJob.PartySpot.GetRoom(this.lordJob.lord.Map);

		public override PreparationStatus CurrentPreparationStatus()
		{
			Room room = PartyRoom;
			int filthCount = room.ThingsInside().OfType<Filth>().Count();

			switch(filthCount) {
				case var test when test == 0:
					return PreparationStatus.Maximal;
				case var test when test <= room.CellCount / 10:
					return PreparationStatus.Complete;
				default:
					return PreparationStatus.Ongoing;
			}
		}

		public override PartyStatus CurrentPartyStatus()
		{
			return PartyStatus.Ongoing;
		}

		override public void ExposeData()
        {
            base.ExposeData();
        }
    }
}
