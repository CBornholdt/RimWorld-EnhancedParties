using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    public abstract class EnhancedLordToil_PrepareParty : EnhancedLordToil
    {
		public new EnhancedLordJob_Party LordJob => (EnhancedLordJob_Party)this.lord.LordJob;
        
        public PreparePartyToilData PrepareData => (PreparePartyToilData)this.data;

		public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p) => LordJob.Def.dutyHook;

		public override void UpdateAllDuties()
		{
			for(int i = 0; i < this.lord.ownedPawns.Count; i++)
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(LordJob.Def.prepareDuty
						, LordJob.PartySpot, radius: -1f);
		}

		protected virtual bool TryGivePreparationMemory(Pawn pawn, out ThoughtDef memory)
		{
			memory = null;
			return false;
		}

		public override void LordToilTick()
		{
			if(--this.PrepareData.ticksToNextPulse <= 0) {
				this.PrepareData.ticksToNextPulse = LordJob.Def.ticksPerPreparationPulse;

				List<Pawn> ownedPawns = this.lord.ownedPawns;
				for(int i = 0; i < ownedPawns.Count; i++) {
					if(LordJob.IsAttendingParty(ownedPawns[i])) {
                        if(TryGivePreparationMemory(ownedPawns[i], out ThoughtDef memory))
                            ownedPawns[i].needs.mood.thoughts.memories.TryGainMemory(memory, otherPawn: null);
                            
						TaleRecorder.RecordTale(TaleDefOf.AttendedParty, new object[]
						{
							ownedPawns[i],
							LordJob.Organizer
						});
					}
				}
			}
		}
	}
}
