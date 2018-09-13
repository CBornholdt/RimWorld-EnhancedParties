using System;
using Verse;

namespace EnhancedParty
{
    public class MovieNightPartyToilData : PartyToilData
    {
		public bool intermissionHasHappened = false;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.intermissionHasHappened, "IntermissionHasHappened", false);
		}
	}
}
