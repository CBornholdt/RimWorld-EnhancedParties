using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    [HarmonyPatch(typeof(Lord))]
    [HarmonyPatch(nameof(Lord.AddPawn))]
    static public class Lord_AddPawn
    {
        //Insert Notify_PawnAdded call before UpdateAllDuties, and then remove post call
        static public IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo updateAllDuties = AccessTools.Method(typeof(LordToil), nameof(LordToil.UpdateAllDuties));
            MethodInfo notifyPawnAdded = AccessTools.Method(typeof(LordJob), nameof(LordJob.Notify_PawnAdded));
            MethodInfo helper = AccessTools.Method(typeof(Lord_AddPawn), nameof(Lord_AddPawn.Helper));

            foreach(var instruction in instructions) {
                if(instruction.opcode == OpCodes.Callvirt && instruction.operand == updateAllDuties) {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);  //Lord on stack
                    yield return new CodeInstruction(OpCodes.Ldarg_1);  //Lord, Pawn on stack
                    yield return new CodeInstruction(OpCodes.Call, helper);
                }
                if(instruction.opcode == OpCodes.Callvirt && instruction.operand == notifyPawnAdded) {
                    //Two arguments are on stack, Lord & Pawn, will need to pop both
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Pop);
                    continue;   //ignore this instruction
                }
                yield return instruction;
            }
        }

        static public void Helper(Lord lord, Pawn pawn)
        {
            lord.LordJob.Notify_PawnAdded(pawn);
        }
    }
}
