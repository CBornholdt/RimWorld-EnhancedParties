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
    public class PartyJob_RecRoom : EnhancedLordJob_Party
    {
		protected RecRoomParty_PrepareToil prepareToil;
		protected RecRoomParty_PartyToil partyToil;
    
        public PartyJob_RecRoom()
        {
			prepareToil = new RecRoomParty_PrepareToil();
			partyToil = new RecRoomParty_PartyToil();
        }

		public override bool AllowStartNewGatherings => base.AllowStartNewGatherings;

		protected override EnhancedLordToil_Party PartyToil => partyToil;

		protected override EnhancedLordToil_PrepareParty PrepareToil => prepareToil;

		public override bool IsAttendingParty(Pawn pawn)
		{
			return base.IsAttendingParty(pawn);
		}
        
        override public bool AllowedToOrganize(Pawn pawn) =>
            pawn.RaceProps.Humanlike && !pawn.InBed() && !pawn.InMentalState
                && pawn.GetLord() == null
                && pawn.AbleToStopJobForParty()
                && PartyUtility.ShouldPawnKeepPartying(pawn);   

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
        
        public Room PartyRoom => PartySpot.GetRoom(this.lord.Map);

        override public void ExposeData()
        {
            base.ExposeData();
        }
	}
}
