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
    
    public class LordPawnRole
    {
		public LordPawnRole(string name, EnhancedLordJob lordJob)
		{
			this.name = name;
			this.lordJob = lordJob;
			this.data = new LordPawnRoleData(name);
		}

		public void Configure(bool enabled, int priority, bool reassignableFrom
			, bool seekReplacements, bool seekReplenishment)
		{
			data.enabled = enabled;
			data.priority = priority;
			data.isReassignableFrom = reassignableFrom;
			data.shouldSeekReplacement = seekReplacements;
			data.opportunisticallyReplenish = seekReplenishment;
		}
    
		public EnhancedLordJob lordJob;
        public string name;       
		public Func<Pawn, bool> pawnValidator;	
        public Func<Pawn, float> pawnReplenishPriority;
		public Func<List<Pawn>, bool> replenishCompleter;

		public LordPawnRoleData data;
		public bool IsEnabled => data.enabled;
		public int Priority => data.priority;
        
        public bool IsReassignableFrom => data.isReassignableFrom;
		public bool ShouldSeekReplacement => data.shouldSeekReplacement;
        public bool OpportunisticallyReplenish => data.opportunisticallyReplenish; 
        public List<Pawn> CurrentPawns => data.currentPawns;
	}
}
