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
    
        public RecRoomParty_PrepareToil()
        {
        }

		private int GetDesiredSnackCount()
		{
			return Math.Min(lord.ownedPawns.Count, 3);
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
                            dutyThingDef = ThingDefOf.MealSimple,
                            thingCount = GetDesiredSnackCount(),
                            taskName = SnackOpName 
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
			LordJob.GetRole("SnackMakers").Configure(enabled: true, priority: 2, reassignableFrom: false
                , seekReplacements: true, seekReplenishment: true);
            LordJob.GetRole("PartyGoers").Configure(enabled: true, priority: 1, reassignableFrom: true
                , seekReplacements: false, seekReplenishment: true);
		}

		public override void Notify_PawnDutyOpComplete(string dutyOp, Pawn pawn)
		{
			if(dutyOp == SnackOpName)
				this.lord.ReceiveMemo(EnhancedLordJob_Party.PreparationCompleteMemo);
		}

		public override void Notify_PawnDutyOpFailed(string dutyOp, Pawn pawn)
		{
			if(dutyOp == SnackOpName)
                this.lord.ReceiveMemo(EnhancedLordJob_Party.PreparationFailedMemo);
		}
	}
}