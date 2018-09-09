using System;
using Harmony;
using Verse;
using RimWorld;

namespace EnhancedParty
{
    [HarmonyPatch(typeof(PartyUtility))]
    [HarmonyPatch(nameof(PartyUtility.UseWholeRoomAsPartyArea))]
    static public class PartyUtility_UseWholeRoomAsPartyArea
    {
        static public bool Prefix(IntVec3 partySpot, Map map, ref bool __result)
        {
            if(partySpot.TryGetEnhancedPartyLordJob(map, out EnhancedLordJob_Party partyJob)
                && partyJob.UseWholePartyRoom)
                return (__result = true) == true;
            return false;
        }
    }
}
