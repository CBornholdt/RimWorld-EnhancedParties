using System;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace EnhancedParty
{
    [StaticConstructorOnStartup]
    static public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance.DEBUG = true;
        
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.cbornholdt.enhancedparty");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
