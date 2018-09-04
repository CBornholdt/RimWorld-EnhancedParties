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
    public class DutyConditional_HasEnoughThingsAtDutyArea : ThinkNode_Conditional
    {
        public DutyConditional_HasEnoughThingsAtDutyArea()
        {
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            return new DutyConditional_HasEnoughThingsAtDutyArea();
        }

        protected override bool Satisfied(Pawn pawn)
        {
            EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;

            if(duty == null || duty.dutyThingDef == null) {
                Log.ErrorOnce($"DutyConditional_HasEnoughThingsAtDutyArea without either an enhancedDuty, or a dutyThingDef", -92373);
                return false;
            }

			return pawn.Map.listerThings.ThingsOfDef(duty.dutyThingDef)
							.Where(thing => pawn.IsCellInDutyArea(thing.PositionHeld))
							.Count() >= duty.thingCount;    
        }
    }
}
