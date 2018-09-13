using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using EnhancedParty;

namespace RimWorld
{
    public class DutyConditional_HasEnoughThingsAtDutyArea : ThinkNode_Conditional_Else
    {
        protected override bool Satisfied(Pawn pawn)
        {
            EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;

            if(duty == null || duty.dutyThingDef == null) {
                Log.ErrorOnce($"DutyConditional_HasEnoughThingsAtDutyArea without either an enhancedDuty, or a dutyThingDef", -92373);
                return false;
            }

            int count = pawn.Map.listerThings.ThingsOfDef(duty.dutyThingDef)
                            .Where(thing => pawn.IsCellInDutyArea(thing.PositionHeld))
                            .Sum(thing => thing.stackCount);
            
            return count
                    >= duty.thingCount;    
        }
    }
}
