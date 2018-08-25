using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Harmony;

namespace EnhancedParty
{
    [StaticConstructorOnStartup]
    static public class JoyGiverExt
    {
		static BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField
										| BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Static;   
    
		static Func<JoyGiver_Ingest, Pawn, Predicate<Thing>, Job> joyGiver_Ingest__TryGiveJobInternal = null;
        
		static Func<JoyGiver_InteractBuilding, bool> joyGiver_InteractBuilding__CanDoDuringParty = null;
		static Func<JoyGiver_InteractBuilding, Pawn, Thing, Job> joyGiver_InteractBuilding__TryGivePlayJob = null;
		static Func<JoyGiver_InteractBuilding, Pawn, Thing, bool, bool> joyGiver_InteractBuilding__CanInteractWith = null;
		static Func<JoyGiver, Pawn, List<Thing>> joyGiver__GetSearchSet = null;
        
		static Func<JoyGiver_SocialRelax, Pawn, Predicate<Thing>, Job> joyGiver_SocialRelax__TryGiveJobInt = null;

		static void HookupDelegate(FieldInfo field)
		{
			var names = field.Name.Split(new string[] { "__" }, StringSplitOptions.None);
			MethodInfo privateMethod = Type.GetType(names[0].CapitalizeFirst()).GetMethod(names[1], flags);
			if(privateMethod == null)
				privateMethod = Type.GetType(names[0].CapitalizeFirst()).GetProperty(names[1], flags)?.GetGetMethod();
			if(privateMethod == null) 
				Log.Error($"Invalid field {field.Name} provided for delegate hookup");
			else
			    field.SetValue(null, Delegate.CreateDelegate(field.FieldType, privateMethod));
		}

		static JoyGiverExt()
		{
			foreach(var field in typeof(JoyGiverExt).GetFields(flags).Where(field => field.FieldType.IsSubclassOf(typeof(Delegate))))
				HookupDelegate(field);
		}

		static public Job TryGiveJobInDutyArea(this JoyGiver joyGiver, Pawn pawn)
		{
			switch(joyGiver) {
				case JoyGiver_Ingest joyIngest:
					return joyIngest.TryGiveJobInDutyArea(pawn);
				case JoyGiver_InteractBuilding joyBuilding:
					return joyBuilding.TryGiveJobInDutyArea(pawn);
                case JoyGiver_SocialRelax joySocialRelax:
					return joySocialRelax.TryGiveJobInDutyArea(pawn);
			}

			return null;
		}

		static public Job TryGiveJobInDutyArea(this JoyGiver_Ingest joyGiver, Pawn pawn)
		{
			return joyGiver_Ingest__TryGiveJobInternal(joyGiver, pawn, (Thing x) => !x.Spawned || pawn.IsCellInDutyArea(x.Position));
		}  
        
        static public Job TryGiveJobInDutyArea(this JoyGiver_InteractBuilding joyGiver, Pawn pawn)
        {
            if (!joyGiver_InteractBuilding__CanDoDuringParty(joyGiver)) //TODO add support for Duty level solution
            {
                return null;
            }
            Thing thing = joyGiver.FindBestDutyGame(pawn);
            if (thing != null)
            {
                return joyGiver_InteractBuilding__TryGivePlayJob(joyGiver, pawn, thing);
            }
            return null;
        }

		static public Thing FindBestDutyGame(this JoyGiver_InteractBuilding joyGiver, Pawn pawn)
		{
            List<Thing> searchSet = joyGiver__GetSearchSet(joyGiver, pawn);
            Predicate<Thing> predicate = (Thing t) => joyGiver_InteractBuilding__CanInteractWith(joyGiver, pawn, t, false)
                                                        && pawn.IsCellInDutyArea(t.Position);

			float radius = pawn.mindState.duty.radius;
			float maxDistance = radius >= 1f ? 1.2f * radius : 9999f;   //TODO add some higher level constraint

            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, searchSet, PathEndMode.OnCell
                , TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxDistance, predicate, null);
		}
        
        static public Job TryGiveJobInDutyArea(this JoyGiver_SocialRelax joyGiver, Pawn pawn)
        {
            return joyGiver_SocialRelax__TryGiveJobInt(joyGiver, pawn, (Thing x) => !x.Spawned || pawn.IsCellInDutyArea(x.Position));
        } 
    }
}   
