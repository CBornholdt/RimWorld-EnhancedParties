using System;
using Verse;
using Verse.AI;
using EnhancedParty;

namespace RimWorld
{
    public class DutyJob_EatInDutyArea : ThinkNode_JobGiver
    {
		public float stopEatingWhenPctFull = 0.9f;
    
        protected override Job TryGiveJob(Pawn pawn)
        {
            EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;
            if (duty == null)
            {
                return null;
            }
            float curLevelPercentage = pawn.needs?.food.CurLevelPercentage ?? 1f;
            if ((double)curLevelPercentage > stopEatingWhenPctFull)
            {
                return null;
            }
            Thing thing = this.FindFood(pawn);
            if (thing == null)
            {
                return null;
            }
            return new Job(JobDefOf.Ingest, thing)
            {
                count = FoodUtility.WillIngestStackCountOf(pawn, thing.def, thing.def.ingestible.CachedNutrition)
            };
        }

        private Thing FindFood(Pawn pawn)
        {
            Predicate<Thing> validator = (Thing x) => x.IngestibleNow && x.def.IsNutritionGivingIngestible && pawn.IsCellInDutyArea(x.Position) && !x.def.IsDrug && x.def.ingestible.preferability > FoodPreferability.RawBad && pawn.RaceProps.WillAutomaticallyEat(x) && !x.IsForbidden(pawn) && x.IsSociallyProper(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 14f, validator, null, 0, 12, false, RegionType.Set_Passable, false);
        }
}
}
