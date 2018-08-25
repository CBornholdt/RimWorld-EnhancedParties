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
        protected Pawn organizer;
        protected EnhancedPartyDef def;
        protected IntVec3 startingSpot;
        protected int partySpotIndex;
        protected IntVec3 currentPartySpot = IntVec3.Invalid;
		protected bool partyHasStarted;

        protected Trigger_TicksPassed preparationTimeout;
        protected Trigger_TicksPassed partyTimeout;

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

		public virtual bool AllowedToOrganize(Pawn pawn) => false;

		public virtual bool UseWholePartyRoom => def.useWholePartyRoom;
        
        public virtual bool TryGetOrganizerAndStartingSpot(Faction faction, Map map, out Pawn organizer, out IntVec3 startingSpot)
        {
            organizer = null;
            startingSpot = default(IntVec3);
			return false;
        }
        
        protected abstract EnhancedLordToil_Party PartyToil { get; }
        
        protected abstract EnhancedLordToil_PrepareParty PrepareToil { get; }

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

		virtual public IEnumerable<Func<IntVec3>> PartySpotProgression()
		{
			yield return () => startingSpot;
		}    
        
        public override StateGraph CreateGraph()
        {
            partySpotGenerators = new List<Func<IntVec3>>(PartySpotProgression());
            UpdatePartySpot();
            
            StateGraph stateGraph = new StateGraph();

            EnhancedLordToil_PrepareParty prepareToil = PrepareToil;
			prepareToil.AttachTo(stateGraph);

			EnhancedLordToil_Party partyToil = PartyToil;
			partyToil.AttachTo(stateGraph);

            LordToil_End endToil = new LordToil_End();
            stateGraph.AddToil(endToil);
            
            Log.Message($"PreparationScore: {partyToil.PreparationScore}");

            this.preparationTimeout = new Trigger_TicksPassed(Def.preparationTimeout);
            this.partyTimeout = new Trigger_TicksPassed(Def.partyTimeout);

            Transition preparationSucceeded = new Transition(prepareToil, partyToil);
            preparationSucceeded.AddTrigger(new Trigger_Memo("PreparationComplete"));
            preparationSucceeded.AddPreAction(new TransitionAction_Custom(
                () => partyToil.PreparationScore = prepareToil.CalculatePreparationScore()));
            preparationSucceeded.AddPostAction(new TransitionAction_Custom(() => {
               // this.CancelJobsForAttendees();
                this.partyHasStarted = true;
            }));

            PreparationStatus successStatus = def.progressToPartyWhenPreparationComplete
                                                        ? PreparationStatus.Complete
                                                        : PreparationStatus.Maximal;
            preparationSucceeded.AddTrigger(new Trigger_TickCondition(
                    () => prepareToil.CurrentPreparationStatus() >= successStatus));

            Transition preparationFailed = new Transition(prepareToil, endToil);
            preparationFailed.AddTrigger(new Trigger_Memo("PreparationFailed"));
            preparationFailed.AddTrigger(new Trigger_PawnLostViolently());
            preparationFailed.AddPreAction(new TransitionAction_Message("EP.PreparationFailed.TransitionMessage".Translate()
                                                , MessageTypeDefOf.NegativeEvent, new TargetInfo(PartySpot, Map)));  //TODO make PartySpot lazily updatable

            if(Def.failOnPreparationTimeout)
                preparationFailed.AddTrigger(preparationTimeout);
            else
                preparationSucceeded.AddTrigger(preparationTimeout);

            stateGraph.AddTransition(preparationSucceeded);
            stateGraph.AddTransition(preparationFailed);

            Transition partyOverFail = new Transition(partyToil, endToil);
            partyOverFail.AddTrigger(new Trigger_Memo("PartyFailed"));
            partyOverFail.AddTrigger(new Trigger_PawnLostViolently());
            partyOverFail.AddPreAction(new TransitionAction_Message("EP.PartyFailed.TransitionMessage".Translate()
                                                , MessageTypeDefOf.NegativeEvent, new TargetInfo(PartySpot, Map)));
            partyOverFail.AddTrigger(new Trigger_TickCondition(
                                () => partyToil.CurrentPartyStatus() == PartyStatus.Interrupted)); 
                                                               
            Transition partyOverSuccess = new Transition(partyToil, endToil);
            partyOverSuccess.AddTrigger(new Trigger_Memo("PartySuccess"));
            partyOverSuccess.AddTrigger(new Trigger_TickCondition(
                                () => partyToil.CurrentPartyStatus() == PartyStatus.Finished));

            if(Def.failOnPartyTimeout)
                partyOverFail.AddTrigger(this.partyTimeout);
            else
                partyOverSuccess.AddTrigger(this.partyTimeout);

            stateGraph.AddTransition(partyOverFail);
            stateGraph.AddTransition(partyOverSuccess);
            
            return stateGraph;
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

	}
}
