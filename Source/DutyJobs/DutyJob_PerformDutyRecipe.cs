using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using EnhancedParty;
using Harmony;

namespace RimWorld
{
	public class DutyJob_PerformDutyRecipe : ThinkNode_JobGiver
	{
		public DutyJob_PerformDutyRecipe()
		{
		}

		public WorkGiver_DoBill IntWorkGiver =>
			DutyJob_PerformDutyRecipe_Helper.intWorkGiver;

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
				
			Thing chosenLocation = null;
			EnhancedLordJob lordJob = pawn.GetLord().LordJob as EnhancedLordJob;
			Bill recipeBill = null;
            
            Func<IBillGiver, bool> billGiverValidator = (IBillGiver bg) =>
                bg.UsableForBillsAfterFueling()
                && bg is Thing thing
                && !thing.IsBurning() && !thing.IsForbidden(pawn)
                && IntWorkGiver.ThingIsUsableBillGiver(thing)
                && pawn.CanReserveAndReach(thing, PathEndMode.InteractionCell, pawn.NormalMaxDanger()
                                                        , maxPawns: 1, stackCount: -1, layer: null, ignoreOtherReservations: false);
                                                        
			if(lordJob != null) {
				var usableBills = lordJob.CurrentCleanableBills()
										 .Where(bill => bill.pawnRestriction == pawn && billGiverValidator(bill.billStack.billGiver));
				if(usableBills.Any()) {
					recipeBill = usableBills.MinBy(bill => (bill.billStack.billGiver as Thing).Position.DistanceToSquared(pawn.Position));
					chosenLocation = recipeBill.billStack.billGiver as Thing;
				}
			}

			if(chosenLocation == null) {    
				var potentialLocations = recipe.AllRecipeUsers.Where(thingDef => thingDef.IsBuildingArtificial)
											   .SelectMany(thingDef => pawn.Map.listerThings.ThingsOfDef(thingDef))
											   .Where(thing => thing.Faction == pawn.Faction
																&& thing is IBillGiver bg
																&& billGiverValidator(bg));

				if(!potentialLocations.Any()) {
					Log.Message($"No potential locations for pawn {pawn.Name}");
					return null;
				}

				chosenLocation = potentialLocations.MinBy(building => building.Position.DistanceToSquared(pawn.Position));
				
				recipeBill = recipe.MakeNewBill();
				recipeBill.pawnRestriction = pawn;
				(chosenLocation as IBillGiver).BillStack.AddBill(recipeBill);
                if(duty.registerForCleanup)
				    lordJob.cleanupActions.Add(new Cleanable_Bill(recipeBill));
			}

			IBillGiver billGiver = chosenLocation as IBillGiver;
			if(billGiver.BillStack.IndexOf(recipeBill) != 0)    	
				billGiver.BillStack.Reorder(recipeBill, billGiver.BillStack.IndexOf(recipeBill) * -1);  //Should make this the top bill
				
			var job = IntWorkGiver.JobOnThing(pawn, chosenLocation, forced: false);

			return job;
		}

		static public void ReplaceBillCreatorWith(EnhancedLordJob job, Pawn replacement, Pawn replaced)
		{
			foreach(var action in job.cleanupActions)
				if(action is Cleanable_Bill cleanableBill
					&& cleanableBill.bill is Bill_ProductionWithUft uftBill
					&& uftBill.BoundWorker == replaced)
					uftBill.BoundUft.Creator = replacement;
		}

		static public void RemoveBillsWithCreator(EnhancedLordJob job, Pawn creator)
		{
			for(int i = job.cleanupActions.Count - 1 ; i >= 0 ; i--) {
				var action = job.cleanupActions[i];
				if(action is Cleanable_Bill cleanableBill
					&& cleanableBill.bill is Bill_ProductionWithUft uftBill
					&& uftBill.BoundWorker == creator) {
					if(action.CleanupStillNeeded())
						action.PerformCleanup();
					uftBill.billStack?.Delete(uftBill);
					job.cleanupActions.RemoveAt(i);
				}
			}
		}
	}

	[StaticConstructorOnStartup]
	static public class DutyJob_PerformDutyRecipe_Helper
	{
		static public WorkGiver_DoBill intWorkGiver = new WorkGiver_DoBill() {
			def = DefDatabase<WorkGiverDef>.GetNamed("DoBillsCook")
		};
	}
}
