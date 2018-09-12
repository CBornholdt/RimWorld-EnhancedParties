using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using EnhancedParty;

namespace RimWorld
{
	public class DutyJob_GetJoyInDutyArea : JobGiver_GetJoyInPartyArea
	{
		public float stopGettingJoyWhenPctFull = 0.92f;
	
		protected override Job TryGiveJobFromJoyGiverDefDirect(JoyGiverDef def, Pawn pawn)
		{
			List<JoyKindDef> allowedJoyKinds = (pawn.mindState?.duty as EnhancedPawnDuty)?.allowedJoyKinds;
			if(allowedJoyKinds != null && !allowedJoyKinds.Contains(def.joyKind))
				return null;

			Log.Message($"Trying {def.defName}");

			if (pawn.mindState.duty == null)
			{
				return null;
			}
			if (pawn.needs.joy == null)
			{
				return null;
			}
			float curLevelPercentage = pawn.needs.joy.CurLevelPercentage;
			if (curLevelPercentage > stopGettingJoyWhenPctFull)
			{
				return null;
			}
			
			return def.Worker.TryGiveJobInDutyArea(pawn);
		}
	}
}
