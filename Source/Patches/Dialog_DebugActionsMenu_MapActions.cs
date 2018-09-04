using System;
using System.Collections.Generic;
using Harmony;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    [HarmonyPatch(typeof(Dialog_DebugActionsMenu))]
    [HarmonyPatch("DoListingItems_MapActions")]
    static public class Dialog_DebugActionsMenu_MapActions
    {
		static void Postfix(Dialog_DebugActionsMenu __instance)
		{
			Map map = Find.CurrentMap; 
            Traverse.Create(__instance).Method("DoGap").GetValue();
			Traverse.Create(__instance).Method("DoLabel", new object[1] { "Tools - Enhanced Parties" }).GetValue();
            Traverse.Create(__instance).Method("DebugAction", new object[2] { "Start Party ...", (Action)delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				Faction faction = Faction.OfPlayer;
				
				foreach (EnhancedPartyDef current in DefDatabase<EnhancedPartyDef>.AllDefs)
				{
                    if(current.PartyCanBeHadWith(faction, map))
        				list.Add(new DebugMenuOption(current.defName, DebugMenuOptionMode.Action, (Action)delegate
        				{
							if(EnhancedPartyUtility.TryStartEnhancedPartyDef(faction, map, current))
								Messages.Message($"Debug starting enhanced party ${current.defName} for faction ${faction.Name}"
                                                    , MessageTypeDefOf.TaskCompletion);
							else   
                                Messages.Message($"Debug failed to start enhanced party ${current.defName} for faction ${faction.Name}"
                                                    , MessageTypeDefOf.TaskCompletion);  
        				}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			}}).GetValue();    
        //    if(map.lordManager.lords.Any(lord => lord.LordJob is EnhancedLordJob_Party))
		}  
    }
}
