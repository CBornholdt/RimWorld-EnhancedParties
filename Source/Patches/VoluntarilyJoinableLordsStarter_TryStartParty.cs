using System;
using Harmony;
using Verse;
using RimWorld;

namespace EnhancedParty
{
    [HarmonyPatch(typeof(VoluntarilyJoinableLordsStarter))]
    [HarmonyPatch(nameof(VoluntarilyJoinableLordsStarter.TryStartParty))]
    static public class VoluntarilyJoinableLordsStarter_TryStartParty
    {
        static public bool Prefix(VoluntarilyJoinableLordsStarter __instance, ref bool __result)
        {
            Map map = (Map)Traverse.Create(__instance).Field("map").GetValue();
            if(EnhancedPartyUtility.TryStartEnhancedParty(Faction.OfPlayer, map)) {
                Traverse.Create(__instance).Field("lastLordStartTick").SetValue(Find.TickManager.TicksGame);
                Traverse.Create(__instance).Field("startPartyASAP").SetValue(false);
                __result = true;
                return false;
            }
            
            return true;
        }
    }
}
