using System;
using Harmony;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    [HarmonyPatch(typeof(Lord))]
    [HarmonyPatch("CheckTransitionOnSignal")]
    static public class Lord_CheckTransitionOnSignal
    {
		static public void Postfix(Lord __instance, ref bool __result, TriggerSignal signal)
		{
			if(__result)
				return;

			if(__instance.CurLordToil is EnhancedLordToil toil) {
				while (toil.ParentToil != null) {
					for (int i = 0; i < __instance.Graph.transitions.Count; i++) {
						if(__instance.Graph.transitions[i].sources.Contains(toil.ParentToil) && __instance.Graph.transitions[i].CheckSignal(__instance, signal)) {
							__result = true;
							return;
						}
					}
					toil = toil.ParentToil;
				}
			}
		}
    }
}
