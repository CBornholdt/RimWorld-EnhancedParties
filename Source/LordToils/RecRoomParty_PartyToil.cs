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
    public class RecRoomParty_PartyToil : EnhancedLordToil_Party
    {
        RoleDutyLordToil subToil;
        static public readonly string SnackMakers = "SnackMakers";
        static public readonly string PartyGoers = "PartyGoers";
    
        public RecRoomParty_PartyToil(EnhancedPartyDef partyDef)
        {
            this.data = new PartyToilData(){ def = partyDef }; 
        }

        public override StateGraph CreateInternalGraph()
        {
            StateGraph graph = new StateGraph();

            RoleDutyLordToil roleToil = new RoleDutyLordToil(this, cancelExistingJobsOnEntry: true) {
                roleDutyMap = new Dictionary<string, Func<Pawn, PawnDuty>>(){ 
                    { 
                        PartyGoers, 
                        (Pawn pawn) => new EnhancedPawnDuty(EnhancedDutyDefOf.EP_PartyWithAllowedJoyKinds
                                                        , focus: LordJob.PartySpot){
                            allowedJoyKinds = new List<JoyKindDef>(){ 
                                DefDatabase<JoyKindDef>.GetNamed("Gaming_Dexterity"),
                                DefDatabase<JoyKindDef>.GetNamed("Gaming_Cerebral")
            } } } } };

            graph.AddToil(roleToil);
            subToil = roleToil;

            return graph;
        }
        
        public override void Init()
        {
            base.Init();
            Log.Message("PartyInit");
            LordJob.GetRole(SnackMakers).Configure(enabled: false, priority: 2, reassignableFrom: false
                , seekReplacements: true, seekReplenishment: true);
            LordJob.GetRole(PartyGoers).Configure(enabled: true, priority: 1, reassignableFrom: true
                , seekReplacements: false, seekReplenishment: true);
            EnhancedLordJob.nextCheckUseRefresh = true;
        }

        public override LordToil SelectSubToil()
        {
            return subToil;
        }
    }
}