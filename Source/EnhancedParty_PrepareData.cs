using System;
using Verse;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class EnhancedParty_PrepareData : LordToilData
    {
		public int ticksToNextPulse = 0;

		public override void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.ticksToNextPulse, "TicksToNextPulse");
		}
    }
}
