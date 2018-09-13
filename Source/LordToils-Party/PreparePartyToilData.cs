using System;
using Verse;
using Verse.AI;

namespace EnhancedParty
{
	public class PreparePartyToilData : ComplexLordToilData
	{
		public int ticksToNextPulse;
		public EnhancedPartyDef def;
	
		public PreparePartyToilData()
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksToNextPulse, "TicksToNextPulse", 100);
			Scribe_Defs.Look<EnhancedPartyDef>(ref this.def, "Def");
		}
	}
}
