using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using EnhancedParty;

namespace RimWorld
{
    public class DutyConditional_HasEnoughThingsAvailable : ThinkNode_Conditional
    {
        public DutyConditional_HasEnoughThingsAvailable()
        {
        }

		protected override bool Satisfied(Pawn pawn)
		{
			EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;

			if(duty == null || duty.dutyThingDef == null) {
				Log.ErrorOnce($"DutyConditional_HasEnoughThingsAvailable without either an enhancedDuty, or a dutyThingDef", -92373);
				return false;
			}

		//	Log.Message($"Checking for {duty.thingCount} number of {duty.dutyThingDef.label}");

			return pawn.Map.itemAvailability.ThingsAvailableAnywhere(
				new ThingDefCountClass(duty.dutyThingDef, duty.thingCount), pawn);
		}
	}
}
