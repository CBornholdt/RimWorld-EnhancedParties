using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class ComplexLordToilData : LordToilData
    {
        protected int currentIndex = 0;

		public int CurrentIndex => currentIndex;

        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.currentIndex, "CurrentIndex");
        }
    }
}
