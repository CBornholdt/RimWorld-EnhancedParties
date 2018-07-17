using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    public class LordToil_EnhancedParty_Party : LordToil
    {
        public LordJob_EnhancedParty LordJob => (LordJob_EnhancedParty)this.lord.LordJob;
        
        public EnhancedParty_PartyData Data => (EnhancedParty_PartyData)this.data;
    
        public LordToil_EnhancedParty_Party()
        {
            this.data = new EnhancedParty_PartyData();
        }
        
        virtual public float PreparationScore {
			get => Data.preparationScore;
			set => Data.preparationScore = value;
		}

        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p) => LordJob.Def.dutyHook;

        public override void UpdateAllDuties()
        {
            for(int i = 0; i < this.lord.ownedPawns.Count; i++)
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(LordJob.Def.partyDuty
                        , LordJob.PartySpot, radius: -1f);
        }

        public override void LordToilTick()
        {
            if(--this.Data.ticksToNextPulse <= 0) {
                this.Data.ticksToNextPulse = LordJob.Def.ticksPerPartyPulse;

                List<Pawn> ownedPawns = this.lord.ownedPawns;
                for(int i = 0; i < ownedPawns.Count; i++) {
                    if(LordJob.IsAttendingParty(ownedPawns[i])) {
                        if(LordJob.Worker.TryGivePartyMemory(ownedPawns[i], out ThoughtDef memory))
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
