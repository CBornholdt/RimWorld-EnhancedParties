using System;
using Harmony;
using Verse;
using RimWorld;

namespace EnhancedParty
{
    static public class PartyUtility_UseWholeRoomAsPartyArea
    {
		static public bool Prefix(IntVec3 partySpot, Map map)
        {
            if(partySpot.TryGetEnhancedPartyLordJob(map, out EnhancedLordJob_Party partyJob)
                && partyJob.UseWholePartyRoom)
    }
}
