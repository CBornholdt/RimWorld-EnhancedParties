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
        public RecRoomParty_PartyToil()
        {
        }

		public override StateGraph CreateInternalGraph()
        {
            StateGraph graph = new StateGraph();

            LordToil roleToil = new RoleDutyLordToil(this, true) 
                { roleDutyMap = new Dictionary<LordPawnRole, Func<PawnDuty>>() 
                   { { LordJob.GetRole("PartyGoers"), () => new EnhancedPawnDuty(EnhancedDutyDefOf.EP_GotoAndCleanFocusRoom
                        , LordJob.PartySpot) { dutyRecipe = RecipeDefOf.CookMealSimple, dutyThingDef = ThingDefOf.MealSimple } } }
            };

            graph.AddToil(roleToil);

            return graph;
        }
        
        public override void Init()
        {
            base.Init();
            LordJob.GetRole("SnackMakers").Configure(enabled: false, priority: 2, reassignableFrom: false
                , seekReplacements: true, seekReplenishment: true);
            LordJob.GetRole("PartyGoers").Configure(enabled: true, priority: 1, reassignableFrom: true
                , seekReplacements: false, seekReplenishment: true);
        }
	}
}