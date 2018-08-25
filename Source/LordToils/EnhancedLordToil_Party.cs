using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    public abstract class EnhancedLordToil_Party : ComplexLordToil
    {
        public new EnhancedLordJob_Party LordJob => (EnhancedLordJob_Party)this.lord.LordJob;
        
        public PartyToilData Data => (PartyToilData)this.data;
        
        virtual public float PreparationScore {
			get => Data.preparationScore;
			set => Data.preparationScore = value;
		}

		virtual public PartyStatus CurrentPartyStatus() => PartyStatus.Ongoing;

        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p) => LordJob.Def.dutyHook;

        public override void UpdateAllDuties()
        {
            for(int i = 0; i < this.lord.ownedPawns.Count; i++)
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(LordJob.Def.partyDuty
                        , LordJob.PartySpot, radius: -1f);
        }

		public virtual bool TryGivePartyMemory(Pawn pawn, out ThoughtDef memory)
		{
			memory = null;
			return false;
		}

        public override void LordToilTick()
        {
            if(--this.Data.ticksToNextPulse <= 0) {
                this.Data.ticksToNextPulse = LordJob.Def.ticksPerPartyPulse;

                List<Pawn> ownedPawns = this.lord.ownedPawns;
                for(int i = 0; i < ownedPawns.Count; i++) {
                    if(LordJob.IsAttendingParty(ownedPawns[i])) {
                        if(TryGivePartyMemory(ownedPawns[i], out ThoughtDef memory))
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
