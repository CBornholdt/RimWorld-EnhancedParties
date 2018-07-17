using System;
using Verse;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class EnhancedParty_PartyData : LordToilData
    {
        public int ticksToNextPulse = 0;
		public float preparationScore = 0f;

        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToNextPulse, "TicksToNextPulse");
			Scribe_Values.Look<float>(ref this.preparationScore, "PreparationScore");
        }
    }
}
