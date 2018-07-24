using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
	[Flags]
	public enum LordPawnRoleStatus
	{   //Some are obviously mutually exclusive
		Disabled = 0x0000,
		Valid = 0x0001,
	/*	CanAcceptMore = 0x0002,
		CanDischargeSome = 0x0004,
		WantsMore = 0x0008,
		WantsLess = 0x0010,
		NeedsMore = 0x0020,
		NeedsLess = 0x0040  */
	}
    
    public class LordPawnRole : IExposable
    {
		public LordJob_JoinableRoles lordJob;
		public int priority = -1;
        public string name;
        public List<Pawn> currentPawns;
		public Func<Pawn, bool> pawnValidator;
		public bool enabled = true;
        
		public bool isReassignableFrom = false;
		public bool shouldSeekReplacement = true;
	//	public bool seekReplacementWithPriority = true; //Look for replacement before normal replenishment
		public bool opportunisticallyReplenish = false;
        public Func<Pawn, float> pawnReplenishPriority;
		public Func<List<Pawn>, bool> replenishCompleter;

		public void ExposeData()
		{
			Scribe_Values.Look<bool>(ref this.enabled, "Enabled", true);
			Scribe_Values.Look<int>(ref this.priority, "Priority", -1);
			Scribe_Collections.Look<Pawn>(ref this.currentPawns, "CurrentPawns", LookMode.Reference);
			Scribe_Values.Look<bool>(ref this.isReassignableFrom, "IsReassignableFrom", false);
			Scribe_Values.Look<bool>(ref this.shouldSeekReplacement, "ShouldSeekReplacement", true);
			Scribe_Values.Look<bool>(ref this.opportunisticallyReplenish, "OpportunisticallyReplenish", false);
		}
	}
}
