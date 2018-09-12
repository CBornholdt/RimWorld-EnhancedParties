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
    [HarmonyPatch(typeof(Lord))]
    [HarmonyPatch(nameof(Lord.GotoToil))]
    static public class Lord_GotoToil
    {
        static public IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo cleanupMethod = AccessTools.Method(typeof(LordToil), nameof(LordToil.Cleanup));
            MethodInfo initMethod = AccessTools.Method(typeof(LordToil), nameof(LordToil.Init));
            MethodInfo selectAppropriateToil = AccessTools.Method(typeof(Lord_GotoToil)
                , nameof(Lord_GotoToil.SelectAppropriateToil));
            MethodInfo initNewlyEnclosingToils = AccessTools.Method(typeof(Lord_GotoToil)
                , nameof(Lord_GotoToil.InitNewlyEnclosingToils));
            MethodInfo cleanupFormerlyEnclosingToils = AccessTools.Method(typeof(Lord_GotoToil)
                , nameof(Lord_GotoToil.CleanupFormerlyEnclosingToils));
            MethodInfo updateAllDuties = AccessTools.Method(typeof(LordToil), nameof(LordToil.UpdateAllDuties));
            MethodInfo updateAllDutiesWrapper = AccessTools.Method(typeof(Lord_GotoToil), nameof(Lord_GotoToil.UpdateAllDutiesWrapper));

            yield return new CodeInstruction(OpCodes.Ldarg_1);  //Place newLordToil on stack
            yield return new CodeInstruction(OpCodes.Call, selectAppropriateToil);  //find correct toil
            yield return new CodeInstruction(OpCodes.Starg_S, 1);   //Replace newLordToil argument

            foreach(var instruction in instructions) {
                if(instruction.opcode == OpCodes.Callvirt && instruction.operand == cleanupMethod) {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);   //prevToil, newToil (arg 1)
                    yield return new CodeInstruction(OpCodes.Call, cleanupFormerlyEnclosingToils);
                    continue;
                }
                if(instruction.opcode == OpCodes.Callvirt && instruction.operand == initMethod) {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);   //newToil, prevToil
                    yield return new CodeInstruction(OpCodes.Call, initNewlyEnclosingToils);
                    break;
                }
                yield return instruction;
            }
        }
    
        static public LordToil SelectAppropriateToil(LordToil toil)
        {          
            while(toil is ComplexLordToil complex)
                toil = complex.SelectSubToil();

            return toil;
        }       
    
        //Will perform inits from super state on down
        static public void InitNewlyEnclosingToils(LordToil newToil, LordToil oldToil)
        {
            var newEnclosingToils = newToil.ThisAndEnclosingToils().Reverse();
            var oldEnclosingToils = oldToil?.ThisAndEnclosingToils() ?? Enumerable.Empty<LordToil>();

            List<LordToil> newlyEnclosedToils = newEnclosingToils.Except(oldEnclosingToils).ToList();

            foreach(var newlyEnclosedToil in newlyEnclosedToils)
                newlyEnclosedToil.Init();

            foreach(var transition in newToil.lord.Graph.transitions)
                if(transition.sources.Intersect(newlyEnclosedToils).Any()) {
                    //Log.Message($"Reseting transition {transition.triggers.Any(trigger => trigger is Trigger_TicksPassed)}");
                    transition.SourceToilBecameActive(transition, oldToil);
                }

            newToil.UpdateAllDuties();
        }

        //Will perform cleanups from sub state on up
        static public void CleanupFormerlyEnclosingToils(LordToil oldToil, LordToil newToil)
        {
            var newEnclosingToils = newToil?.ThisAndEnclosingToils() ?? Enumerable.Empty<LordToil>();
            var oldEnclosingToils = oldToil.ThisAndEnclosingToils();

            foreach(var formerlyEnclosedToil in oldEnclosingToils.Except(newEnclosingToils))
                formerlyEnclosedToil.Cleanup();
        }

        static public void UpdateAllDutiesWrapper(LordToil curToil)
        {
            EnhancedLordJob.updateDueToToilChange = true;
            curToil.UpdateAllDuties();
            EnhancedLordJob.updateDueToToilChange = false;
        }
    }
}
