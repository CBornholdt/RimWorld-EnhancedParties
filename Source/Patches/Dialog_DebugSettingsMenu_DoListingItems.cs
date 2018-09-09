using System;
using System.Linq;
using System.Collections.Generic;
using Harmony;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;
using System.Reflection;
using System.Reflection.Emit;

namespace EnhancedParty
{
    [HarmonyPatch(typeof(Dialog_DebugSettingsMenu))]
    [HarmonyPatch("DoListingItems")]
    static public class Dialog_DebugSettingsMenu_DoListingItems
    {
		static public void Postfix(Dialog_DebugSettingsMenu __instance)
		{
			Listing_Standard listing = Traverse.Create(__instance).Field("listing").GetValue<Listing_Standard>();
			listing.Gap();
			listing.Label("Enhanced Lords", -1f, null);
			foreach(var field in typeof(EnhancedLordDebugSettings).GetFields())
				Traverse.Create(__instance).Method("DoField", field).GetValue();
		}
    }
}
