using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class NestableLordToilData : LordToilData
    {
		public int currentIndex = 0;

		public override void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.currentIndex, "CurrentIndex");
		}
	}
}
