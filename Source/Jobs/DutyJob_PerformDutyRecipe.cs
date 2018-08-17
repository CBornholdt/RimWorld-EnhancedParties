using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using EnhancedParty;

namespace RimWorld
{
    public class DutyJob_PerformDutyRecipe : ThinkNode_JobGiver
    {
		private WorkGiver_DoBill intWorkGiver;
    
        public DutyJob_PerformDutyRecipe()
        {
			intWorkGiver = new WorkGiver_DoBill();
        }

		protected override Job TryGiveJob(Pawn pawn)
		{
			EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;

            if(duty == null)
                return null;

			RecipeDef recipe = duty.dutyRecipe;

			if (recipe == null || !recipe.AvailableNow)
				return null;

			if (duty.useFoodGuard && recipe.products.Any(thingCount => thingCount.thingDef.IsNutritionGivingIngestible)
				&& pawn.Map.resourceCounter.TotalHumanEdibleNutrition < (float)pawn.Map.mapPawns.ColonistsSpawnedCount * 1.5f)
				return null;

			var potentialLocations = (pawn.Faction == Faction.OfPlayer)
				? pawn.Map.listerBuildings.allBuildingsColonist
						  .Where(building => recipe.AllRecipeUsers.Contains(building.def)
											&& ((building as IBillGiver)?.CurrentlyUsableForBills() ?? false))
				: pawn.Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial))
						  .Cast<Building>()
						  .Where(building => building.Faction == pawn.Faction
											&& recipe.AllRecipeUsers.Contains(building.def)
											&& ((building as IBillGiver)?.CurrentlyUsableForBills() ?? false));

			Building chosenBuilding = null;

			if (!potentialLocations.TryRandomElementByWeight(building => building.Position.DistanceToSquared(pawn.Position)
																, out chosenBuilding))
				return null;
                
			Bill recipeBill = recipe.MakeNewBill();
			IBillGiver billGiver = chosenBuilding as IBillGiver;
			billGiver.BillStack.AddBill(recipeBill);
			billGiver.BillStack.Reorder(recipeBill, billGiver.BillStack.IndexOf(recipeBill) * -1);  //Should make this the top bill
            return intWorkGiver.JobOnThing(pawn, chosenBuilding, forced: false);                    
		}
	}
}
