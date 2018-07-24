using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    public class ThinkNodeConditional_Duty_HasEnoughThingsAvailable : ThinkNode_Conditional
    {
        public ThinkNodeConditional_Duty_HasEnoughThingsAvailable()
        {
        }

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			return new ThinkNodeConditional_Duty_HasEnoughThingsAvailable();
		}

		protected override bool Satisfied(Pawn pawn)
		{
			EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;

			if(duty == null || duty.dutyThingDef == null) {
				Log.ErrorOnce($"ThinkNodeConditional_Duty_HasEnoughThingsAvailable without either an enhancedDuty, or a dutyThingDef", -92373);
				return false;
			}

			return pawn.Map.itemAvailability.ThingsAvailableAnywhere(
				new ThingCountClass(duty.dutyThingDef, duty.thingCount), pawn);
		}
	}
}
