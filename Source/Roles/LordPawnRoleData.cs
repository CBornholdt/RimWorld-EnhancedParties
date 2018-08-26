using System;
using System.Collections.Generic;

using Verse;

namespace EnhancedParty
{
    public class LordPawnRoleData : IExposable
    {
		public LordPawnRoleData() { }
    
		public LordPawnRoleData(string roleName)
		{
			this.roleName = roleName;
			currentPawns = new List<Pawn>();
		}
        
        public string roleName;    
        public bool enabled = true;
        public int priority = -1;
        public bool isReassignableFrom = false;
        public bool shouldSeekReplacement = true;
        public bool opportunisticallyReplenish = false; 
        public List<Pawn> currentPawns;

		public void ExposeData()
		{
			Scribe_Values.Look<string>(ref this.roleName, "RoleName", "None");
            Scribe_Values.Look<bool>(ref this.enabled, "Enabled", true);
            Scribe_Values.Look<int>(ref this.priority, "Priority", -1);
            Scribe_Collections.Look<Pawn>(ref this.currentPawns, "CurrentPawns", LookMode.Reference);
            Scribe_Values.Look<bool>(ref this.isReassignableFrom, "IsReassignableFrom", false);
            Scribe_Values.Look<bool>(ref this.shouldSeekReplacement, "ShouldSeekReplacement", true);
            Scribe_Values.Look<bool>(ref this.opportunisticallyReplenish, "OpportunisticallyReplenish", false);
		}
    }
}
