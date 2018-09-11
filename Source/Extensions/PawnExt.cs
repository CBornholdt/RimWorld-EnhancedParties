using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    static public class PawnExt
    {
        static public void SetLordPawnRole(this Pawn pawn, string name)
        {
            var role = (pawn.GetLord()?.LordJob as EnhancedLordJob)?.GetRole(name);
            if(role == null)
                return;
            if(role.CurrentPawns.Contains(pawn))
                Log.Message($"Attempted to add pawn { pawn.Name } to role { name } but was already present");
            else
                role.CurrentPawns.Add(pawn);
        }
    
        static public LordPawnRole GetLordPawnRole(this Pawn pawn) =>
            (pawn.GetLord()?.LordJob as EnhancedLordJob)?.GetRole(pawn);

        static public EnhancedLordJob GetEnhancedLordJob(this Pawn pawn) =>
            pawn.GetLord()?.LordJob as EnhancedLordJob;
    
        static public bool AbleToStopJobForParty(this Pawn pawn)
        {
            return !(pawn.CurJob.targetA.Thing is Pawn
                        || pawn.CurJob.targetB.Thing is Pawn
                        || pawn.CurJob.targetC.Thing is Pawn) 
                   && pawn.jobs.jobQueue.Count == 0;
        }
        
        static public bool IsInDutyArea(this Pawn pawn)
        {
            Lord lord = pawn.GetLord();

            if(lord == null)
                return false;

            EnhancedLordToil lordToil = lord.CurLordToil as EnhancedLordToil;

            if(lordToil != null)
                return lordToil.IsInDutyArea(pawn);

            EnhancedLordJob lordJob = lord.LordJob as EnhancedLordJob;

            if(lordJob != null)
                return lordJob.IsInDutyArea(pawn);

            return false;
        }
        
        static public IEnumerable<IntVec3> DutyAreaCells(this Pawn pawn)
        {
            Lord lord = pawn.GetLord();

            if(lord == null)
                return Enumerable.Empty<IntVec3>();

            EnhancedLordToil lordToil = lord.CurLordToil as EnhancedLordToil;

            if(lordToil != null)
                return lordToil.DutyAreaCells(pawn);

            EnhancedLordJob lordJob = lord.LordJob as EnhancedLordJob;

            if(lordJob != null)
                return lordJob.DutyAreaCells(pawn);

            return Enumerable.Empty<IntVec3>();
        }
        
        static public bool IsCellInDutyArea(this Pawn pawn, IntVec3 cell)
        {
            Lord lord = pawn.GetLord();

            if(lord == null)
                return false;

            EnhancedLordToil lordToil = lord.CurLordToil as EnhancedLordToil;

            if(lordToil != null)
                return lordToil.IsCellInDutyArea(pawn, cell);

            EnhancedLordJob lordJob = lord.LordJob as EnhancedLordJob;

            if(lordJob != null)
                return lordJob.IsCellInDutyArea(pawn, cell);

            return false;
        }
    }
}
