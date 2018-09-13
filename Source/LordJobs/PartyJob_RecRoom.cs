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

        public PartyJob_RecRoom(EnhancedPartyDef def, Pawn organizer, IntVec3 spot) : base(def, organizer, spot) 
        {
        }
        
        public PartyJob_RecRoom() : base()
        {
        }

        protected override EnhancedLordToil_Party PartyToil => partyToil;

        protected override EnhancedLordToil_PrepareParty PrepareToil => prepareToil;

        public override bool AllowRolelessPawnsToReplenish => true;

        public int TotalSnacksNeeded() => prepareToil.GetDesiredSnackCount();

        public int SnacksAlreadySetup() => prepareToil.GetSetupSnackCount(); 

        protected override void CreatePartyRolesAndToils()
        {
            var partyGoers = new LordPawnRole("PartyGoers", this) {
                pawnValidator = (Pawn p) => true,
                pawnReplenishPriority = (Pawn p) => 1f,
                replenishCompleter = (List<Pawn> pawns) => false    //hungry, will default to everything
            };
            partyGoers.Configure(enabled: true, priority: 1, reassignableFrom: true
                , seekReplacements: false, seekReplenishment: true);
            roles.Add(partyGoers);

            var snackMakers = new LordPawnRole("SnackMakers", this) {
                pawnValidator = (Pawn p) => RecipeDefOf.CookMealSimple.PawnSatisfiesSkillRequirements(p),
                pawnReplenishPriority = (Pawn p) => p.skills.GetSkill(SkillDefOf.Cooking).Level,
                replenishCompleter = (List<Pawn> pawns) => pawns.Any()
                    && pawns.Count >= (TotalSnacksNeeded() - SnacksAlreadySetup()) / 2
            };
            snackMakers.Configure(enabled: true, priority: 2, reassignableFrom: false
                , seekReplacements: true, seekReplenishment: true);
            roles.Add(snackMakers);

            prepareToil = new RecRoomParty_PrepareToil(Def);
            partyToil = new RecRoomParty_PartyToil(Def);
        }

        override public bool AllowedToOrganize(Pawn pawn) =>
            pawn.RaceProps.Humanlike && !pawn.InBed() && !pawn.InMentalState
                && pawn.GetLord() == null
                && pawn.AbleToStopJobForParty()
                && EnhancedPartyUtility.ShouldPawnKeepPartyingBasicChecks(pawn)
                && RecipeDefOf.CookMealSimple.PawnSatisfiesSkillRequirements(pawn);   

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

        public bool IsInPartyArea(IntVec3 cell)
        {
            Map map = this.lord.Map;
            return cell.GetRoom(map) == PartySpot.GetRoom(map);
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            float priority = base.VoluntaryJoinPriorityFor(p);

            if(EnhancedLordDebugSettings.logJoinPriorities)
                Log.Message($"{this.GetType().Name} VoluntaryJoinPriorityFor Pawn: {p.Name} is {priority}");

            return priority;
        }

        public override void AssignActiveCleanup()
        {
            Log.Message($"{cleanupActions.Count} cleanup actions");
            for(int i = cleanupActions.Count - 1; i >= 0; i--) {
                var action = cleanupActions[i] as Cleanable_Haulable;
                if(action != null
                    && !action.haulable.DestroyedOrNull()
                    && action.haulable.Spawned 
                    && lord.ownedPawns.TryRandomElement(out Pawn pawn)) {
                    action.AssignCleanupToPawn(pawn);
                    cleanupActions.RemoveAt(i);
                    this.lord.ownedPawns.Remove(pawn);
                }
            }
        }
    }
}
