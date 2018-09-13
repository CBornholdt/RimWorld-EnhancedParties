using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Verse;
using RimWorld;
using Verse.AI;

namespace EnhancedParty
{
    [HarmonyPatch(typeof(JobDriver))]
    [HarmonyPatch("SetupToils")]
    static public class JobDriver_SetupToils
    {
        static public IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo makeNewToils = AccessTools.Method(typeof(JobDriver), "MakeNewToils");
            MethodInfo toilHelper = AccessTools.Method(typeof(JobDriver_SetupToils), nameof(JobDriver_SetupToils.ProcessToilsHelper));
            MethodInfo actionHelper = AccessTools.Method(typeof(JobDriver_SetupToils), nameof(JobDriver_SetupToils.ProcessActionsConditionsHelper));

            foreach(var code in instructions) {
                if(code.opcode == OpCodes.Callvirt && code.operand == makeNewToils) {
                    yield return code;                                  //IEnumerable<Toil> on stack
                    yield return new CodeInstruction(OpCodes.Ldarg_0);  //IEnumerable<Toil>, JobDriver on stack
                    yield return new CodeInstruction(OpCodes.Call, toilHelper); //Consume JobDriver / IEnumerable<Toil> on stack
                    continue;
                }
                if(code.opcode == OpCodes.Ret) {
                    yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = code.labels };  
                    yield return new CodeInstruction(OpCodes.Call, actionHelper); 
                    yield return new CodeInstruction(OpCodes.Ret);
                    continue;
                }
                yield return code;
            }
        }

        static public IEnumerable<Toil> ProcessToilsHelper(IEnumerable<Toil> toils, JobDriver driver)
        {
            var adjustmentJob = driver?.job as JobWithAdjustment;
            if(adjustmentJob == null || adjustmentJob.adjuster == null)
                return toils;
            return adjustmentJob.adjuster.ProcessToils(toils);
        }

        static public void ProcessActionsConditionsHelper(JobDriver driver)
        {
            var adjustmentJob = driver?.job as JobWithAdjustment;
            
            if(adjustmentJob == null || adjustmentJob.adjuster == null)
                return;

            driver.globalFinishActions = adjustmentJob.adjuster.ProcessGlobalFinishActions(driver.globalFinishActions, driver);
            driver.globalFailConditions = adjustmentJob.adjuster.ProcessGlobalFailConditions(driver.globalFailConditions, driver);   
        }
    }
}
