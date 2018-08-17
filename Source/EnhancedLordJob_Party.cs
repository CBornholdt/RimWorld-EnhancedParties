using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;
using Harmony;

namespace EnhancedParty
{
    public abstract class EnhancedLordJob_Party : EnhancedLordJob
    {
        Pawn organizer;
        EnhancedPartyDef def;
        IntVec3 startingSpot;
        int partySpotIndex;
        IntVec3 currentPartySpot = IntVec3.Invalid;
		bool partyHasStarted;

        Trigger_TicksPassed preparationTimeout;
        Trigger_TicksPassed partyTimeout;

        List<Func<IntVec3>> partySpotGenerators;

        public EnhancedLordJob_Party() { }
        
        public EnhancedLordJob_Party(EnhancedPartyDef def, Pawn organizer, IntVec3 startingSpot)
        {
            this.def = def;
            this.organizer = organizer;
            this.startingSpot = startingSpot;
            this.partySpotIndex = 0;
			this.partyHasStarted = false;
        }

        public EnhancedPartyDef Def => def;

        public override bool AllowStartNewGatherings => false;

		public bool PartyHasStarted => this.partyHasStarted;

        public Pawn Organizer => organizer;

        public IntVec3 PartySpot => currentPartySpot;

		public virtual bool PartyCanBeHadWith(Faction faction, Map map) => false;
        
        public virtual bool TryGetOrganizerAndStartingSpot(Faction faction, Map map, out Pawn organizer, out IntVec3 startingSpot)
        {
            organizer = null;
            startingSpot = default(IntVec3);
			return false;
        }
        
        protected virtual EnhancedLordToil_Party PartyToil { get; }
        
        protected virtual EnhancedLordToil_PrepareParty PrepareToil { get; }

        private void UpdatePartySpot() => currentPartySpot = partySpotGenerators[partySpotIndex]();

        public bool TryNextPartySpot()
        {
            if(partySpotIndex + 1 >= partySpotGenerators.Count)
                return false;
            partySpotIndex++;
            UpdatePartySpot();
            lord.CurLordToil.UpdateAllDuties();
            return true;
        }

        public virtual bool IsAttendingParty(Pawn pawn) => this.lord.ownedPawns.Contains(pawn);
        
        virtual public int PreparationTimeoutTicks() => def.preparationTimeout;
        virtual public int PartyTimeoutTicks() => def.partyTimeout;

        public override StateGraph CreateGraph()
        {
			return new StateGraph();
        }

		public void CancelJobsForAttendees()
		{
			foreach(var pawn in this.lord.ownedPawns) {
				pawn.jobs.ClearQueuedJobs();
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}   

        public bool IsPartyAboutToEnd() => false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<EnhancedPartyDef>(ref this.def, "Def");
            Scribe_References.Look<Pawn>(ref this.organizer, "Organizer");
            Scribe_Values.Look<IntVec3>(ref this.startingSpot, "StartingSpot");
            Scribe_Values.Look<int>(ref this.partySpotIndex, "PartySpotIndex");
        }

        private bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableGameConditionsToContinueParty(this.Map) || !this.PartySpot.Roofed(this.Map);
        }

		public override float VoluntaryJoinPriorityFor(Pawn p) => 1f;
        
		public override void Notify_PawnAdded(Pawn p)
		{
			base.Notify_PawnAdded(p);
			Log.Message($"Gained pawn {p.Name}");
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			base.Notify_PawnLost(p, condition);

			Log.Message($"Lost pawn {p.Name}");
		}

		public override string GetReport() => PartyHasStarted
												? "EP.Party.Report".Translate()
                                                : "EP.Prepare.Report".Translate();

		protected override void Initialize()
		{
			throw new NotImplementedException();
		}
	}
}
