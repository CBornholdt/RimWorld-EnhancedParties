﻿using System;
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
            //Log.Message($"Duty ThingCount for pawn { pawn.Name } is { duty.thingCount } and area count is { count }");
            

            return count
                    >= duty.thingCount;    
        }
    }
}
