﻿using System;
using Verse;
using Verse.AI.Group;

namespace EnhancedParty
{
    static public class DutyOpUtility
    {
        static public void Notify_DutyOpComplete(string dutyOp, Pawn pawn)
        {
            EnhancedLordJob job = pawn.GetLord()?.LordJob as EnhancedLordJob;
            if(job == null) {
                Log.Error($"Attempted to notify duty op complete but pawn {pawn.Name} does not have an Enhanced LordJob");
                return;
            }

            EnhancedLordToil toil = job.CurrentEnhancedToil;
            if(toil != null)
                toil.Notify_PawnDutyOpComplete(dutyOp, pawn);
            else
                job.Notify_PawnDutyOpComplete(dutyOp, pawn);               
        }
        
        static public void Notify_DutyOpFailed(string dutyOp, Pawn pawn)
        {
            EnhancedLordJob job = pawn.GetLord()?.LordJob as EnhancedLordJob;
            if(job == null) {
                Log.Error($"Attempted to notify duty op complete but pawn {pawn.Name} does not have an Enhanced LordJob");
                return;
            }

            EnhancedLordToil toil = job.CurrentEnhancedToil;
            if(toil != null)
                toil.Notify_PawnDutyOpFailed(dutyOp, pawn);
            else
                job.Notify_PawnDutyOpFailed(dutyOp, pawn);               
        }
    }
}
