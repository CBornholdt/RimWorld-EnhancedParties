using System;
using System.Collections.Generic;
using System.Linq;
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

            if(map.lordManager.lords.Any(lord => lord.LordJob is EnhancedLordJob_Party)) {
                Traverse.Create(__instance).Method("DebugAction", new object[2] { "Stop Party ...", (Action)delegate
                {
                    List<DebugMenuOption> list = new List<DebugMenuOption>();

                    foreach(var lord in map.lordManager.lords.Where(lord => lord.LordJob is EnhancedLordJob_Party)){
                        list.Add(new DebugMenuOption(lord.LordJob.GetType().Name + lord.loadID, DebugMenuOptionMode.Action
                                                        , () => map.lordManager.RemoveLord(lord)));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }}).GetValue();
            }
            
            if(map.lordManager.lords.Any(lord => lord.LordJob is EnhancedLordJob_Party)) {
                Traverse.Create(__instance).Method("DebugAction", new object[2] { "Log Party Info ...", (Action)delegate
                {
                    List<DebugMenuOption> list = new List<DebugMenuOption>();

                    foreach(var lord in map.lordManager.lords.Where(lord => lord.LordJob is EnhancedLordJob_Party)){
                        list.Add(new DebugMenuOption(lord.LordJob.GetType().Name + lord.loadID, DebugMenuOptionMode.Action
                                                        , () => (lord.LordJob as EnhancedLordJob_Party).LogDebuggingInfo()));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }}).GetValue();
            }
            
            Traverse.Create(__instance).Method("DebugAction", new object[2] { "Cancel non partier jobs", (Action)delegate
            {
                foreach(var pawn in map.mapPawns.AllPawnsSpawned.Where(p => !(p.GetLord()?.LordJob is EnhancedLordJob_Party))) {
                    pawn.jobs.ClearQueuedJobs();
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }                        
            }}).GetValue();    
        //    if(map.lordManager.lords.Any(lord => lord.LordJob is EnhancedLordJob_Party))
        }  
    }
}
