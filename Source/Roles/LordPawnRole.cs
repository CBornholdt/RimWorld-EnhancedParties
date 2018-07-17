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
		CanAcceptMore = 0x0002,
		CanDischargeSome = 0x0004,
		WantsMore = 0x0008,
		WantsLess = 0x0010,
		NeedsMore = 0x0020,
		NeedsLess = 0x0040
	}
    
    public class LordPawnRole
    {
		public int order = -1;
        public string name;
        public List<Pawn> currentPawns;
		public Func<LordJob, IEnumerable<Pawn>> pawnSelector;
		public Func<List<Pawn>, LordPawnRoleStatus> statusValidator;

		public void ReselectFor(LordJob job)
		{
			currentPawns = new List<Pawn>(pawnSelector(job));
		}

		public LordPawnRoleStatus CurrentStatus => statusValidator(currentPawns);
    }
}
