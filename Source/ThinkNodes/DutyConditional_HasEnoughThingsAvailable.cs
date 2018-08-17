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

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			return new DutyConditional_HasEnoughThingsAvailable();
		}

		protected override bool Satisfied(Pawn pawn)
		{
			EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;

			if(duty == null || duty.dutyThingDef == null) {
				Log.ErrorOnce($"DutyConditional_HasEnoughThingsAvailable without either an enhancedDuty, or a dutyThingDef", -92373);
				return false;
			}

			return pawn.Map.itemAvailability.ThingsAvailableAnywhere(
				new ThingCountClass(duty.dutyThingDef, duty.thingCount), pawn);
		}
	}
}
