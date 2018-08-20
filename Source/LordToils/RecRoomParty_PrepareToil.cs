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
        public RecRoomParty_PrepareToil()
        {
        }

        public override StateGraph CreateInternalGraph()
        {
            StateGraph graph = new StateGraph();

            LordToil roleToil = new RoleDutyLordToil(this, true) {   
                roleDutyMap = new Dictionary<LordPawnRole, Func<PawnDuty>>(){ 
                    {   
                        LordJob.GetRole("SnackMakers"), 
                        () => new EnhancedPawnDuty(EnhancedDutyDefOf.EP_MakeThingsToFocus, LordJob.PartySpot){   
                            dutyRecipe = RecipeDefOf.CookMealSimple,
                            dutyThingDef = ThingDefOf.MealSimple 
                        } 
                    }, 
                    {   
                        LordJob.GetRole("PartyGoers"), 
                        () => new EnhancedPawnDuty(EnhancedDutyDefOf.EP_GotoAndCleanFocusRoom, LordJob.PartySpot) 
                    } 
                }
            };

            graph.AddToil(roleToil);

            return graph;
        }

		public override void Init()
		{
			base.Init();
			LordJob.SetRoleEnabled("SnackMakers", true);
            LordJob.SetRoleEnabled("PartyGoers", true);
		}
	}
}