using System;
using Verse;
using RimWorld;
using Verse.AI;
using EnhancedParty;

namespace RimWorld
{
    public class DutyConditional_BuildingAtDesiredLocation : ThinkNode_Conditional_Else
    {
        protected override bool Satisfied(Pawn pawn)
        {
            EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;

            if(duty == null)
                return false;

            Thing thing = duty.focus.Thing;
            if(thing == null || thing.Position != duty.focusSecond.Cell
                || thing.Rotation != duty.direction)
                return false;

            return true;
        }
    }
}
