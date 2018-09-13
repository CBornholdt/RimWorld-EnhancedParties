using System;
using Verse;
using RimWorld;

namespace EnhancedParty
{
	public class PartyToilData : ComplexLordToilData
	{
		public int ticksToNextPulse = 0;
		public float preparationScore = 0f;
		public EnhancedPartyDef def;

		public override void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.ticksToNextPulse, "TicksToNextPulse");
			Scribe_Values.Look<float>(ref this.preparationScore, "PreparationScore");
			Scribe_Defs.Look<EnhancedPartyDef>(ref this.def, "Def");
		}
	}
}
