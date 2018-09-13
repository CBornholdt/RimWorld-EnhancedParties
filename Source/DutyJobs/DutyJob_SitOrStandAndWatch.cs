using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using EnhancedParty;
using Harmony;

namespace RimWorld
{
    public class DutyJob_SitOrStandAndWatch : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            var duty = pawn.mindState.duty;   
            if(duty == null || !duty.focus.Cell.IsValid || !duty.focusSecond.Cell.IsValid)
                return null;
            return new Job(MoreJobDefs.SitOrStandFacingCell, duty.focus, duty.focusSecond);
        }
    }
}
