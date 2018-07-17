using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    public class LordToil_EnhancedParty_Prepare : LordToil
    {
		public LordJob_EnhancedParty LordJob => (LordJob_EnhancedParty)this.lord.LordJob;
        
        public EnhancedParty_PrepareData Data => (EnhancedParty_PrepareData)this.data;
    
        public LordToil_EnhancedParty_Prepare()
        {
			this.data = new EnhancedParty_PrepareData();
        }

		public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p) => LordJob.Def.dutyHook;

		public override void UpdateAllDuties()
		{
			for(int i = 0; i < this.lord.ownedPawns.Count; i++)
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(LordJob.Def.prepareDuty
						, LordJob.PartySpot, radius: -1f);
		}

		public override void LordToilTick()
		{
			if(--this.Data.ticksToNextPulse <= 0) {
				this.Data.ticksToNextPulse = LordJob.Def.ticksPerPreparationPulse;

				List<Pawn> ownedPawns = this.lord.ownedPawns;
				for(int i = 0; i < ownedPawns.Count; i++) {
					if(LordJob.IsAttendingParty(ownedPawns[i])) {
                        if(LordJob.Worker.TryGivePreparationMemory(ownedPawns[i], out ThoughtDef memory))
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
