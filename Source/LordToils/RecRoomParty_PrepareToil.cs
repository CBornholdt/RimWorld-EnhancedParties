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
    public class RecRoomParty_PrepareToil : EnhancedLordToil_PrepareParty
    {
        static public readonly string SnackOpName = "MakeSnacks";
        static public readonly string SnackMakers = "SnackMakers";
        static public readonly string PartyGoers = "PartyGoers";
        RoleDutyLordToil subToil;
    
        public RecRoomParty_PrepareToil()
        {
            this.data = new PreparePartyToilData();
        }

        public override LordToil SelectSubToil()
        {
            return subToil;
        }

        public new PartyJob_RecRoom LordJob => this.lord?.LordJob as PartyJob_RecRoom;

        public int GetDesiredSnackCount()
        {
            int value = Math.Min(lord.ownedPawns.Count, 6);
            return value;
        }

        public int GetSetupSnackCount()
        {
            int value = lord.Map.listerThings.ThingsOfDef(ThingDefOf.MealSimple)
                            .Where(thing => LordJob.IsInPartyArea(thing.PositionHeld))
                            .Sum(thing => thing.stackCount);

            return value;
        }

        public override StateGraph CreateInternalGraph()
        {
            StateGraph graph = new StateGraph();

            RoleDutyLordToil roleToil = new RoleDutyLordToil(this, true) {   
                roleDutyMap = new Dictionary<string, Func<PawnDuty>>(){ 
                    {   
                        SnackMakers, 
                        () => new EnhancedPawnDuty(EnhancedDutyDefOf.EP_MakeThingsToFocus, LordJob.PartySpot){   
                            dutyRecipe = RecipeDefOf.CookMealSimple,
                            dutyThingDef = ThingDefOf.MealSimple,
                            thingCount = GetDesiredSnackCount(),
                            taskName = SnackOpName 
                        } 
                    }, 
                    {   
                        PartyGoers, 
                        () => new EnhancedPawnDuty(EnhancedDutyDefOf.EP_GotoAndCleanFocusRoom, LordJob.PartySpot) 
                    } 
                }
            };

            graph.AddToil(roleToil);
            subToil = roleToil;

            return graph;
        }

        public override void Init()
        {
            base.Init();
            LordJob.GetRole(SnackMakers).Configure(enabled: true, priority: 2, reassignableFrom: false
                , seekReplacements: true, seekReplenishment: true);
            LordJob.GetRole(PartyGoers).Configure(enabled: true, priority: 1, reassignableFrom: true
                , seekReplacements: false, seekReplenishment: true);
        }

        public override void Notify_PawnDutyOpComplete(string dutyOp, Pawn pawn)
        {
            if(dutyOp == SnackOpName) {
                Log.Message("Sending complete memo");
                this.lord.ReceiveMemo(EnhancedLordJob_Party.PreparationCompleteMemo);
            }
        }

        public override void Notify_PawnDutyOpFailed(string dutyOp, Pawn pawn)
        {
            if(dutyOp == SnackOpName)
                this.lord.ReceiveMemo(EnhancedLordJob_Party.PreparationFailedMemo);
        }

        public override void Notify_PawnLeftRole(LordPawnRole role, Pawn pawn, LordPawnRole newPawnRole)
        {
            if(role.name == SnackMakers)
                DutyJob_PerformDutyRecipe.RemoveBillsWithCreator(LordJob, pawn);
        }

        public override void Notify_PawnReplacedPawnInRole(LordPawnRole role, Pawn newPawn, Pawn oldPawn, LordPawnRole newPawnOldRole, LordPawnRole oldPawnNewRole)
        {
            if(role.name == SnackMakers)
                DutyJob_PerformDutyRecipe.ReplaceBillCreatorWith(LordJob, replacement: newPawn, replaced: oldPawn);
        }

        public override void UpdateAllDuties()
        {
            foreach(var pawn in LordJob.GetRole(SnackMakers).CurrentPawns) {
                (pawn.mindState.duty as EnhancedPawnDuty).thingCount = GetDesiredSnackCount();
            }
        }
    }
}