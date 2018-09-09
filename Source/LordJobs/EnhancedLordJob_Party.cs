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
        public EnhancedPartyDef def;
        protected IntVec3 startingSpot;
        protected int partySpotIndex;
        protected IntVec3 currentPartySpot = IntVec3.Invalid;
		protected bool partyHasStarted;

        protected Trigger_TicksPassed preparationTimeout;
        protected Trigger_TicksPassed partyTimeout;

        List<Func<IntVec3>> partySpotGenerators;

		static public readonly string PreparationCompleteMemo = "PartyPreparationComplete";
        static public readonly string PreparationFailedMemo = "PartyPreparationFailed";

		public Transition preparationFailed;
		public Transition preparationSucceeded;
		public Transition partyFailed;
		public Transition partySucceeded;

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

		public virtual bool IsPartyAboutToEnd => 
            def.ticksLeftWhenPartyAboutToEnd > 0
                ? partyTimeout.TicksLeft < def.ticksLeftWhenPartyAboutToEnd
                : false;

        //Not sure about this null check, copied from LordJob_Joinable_Party
		public virtual bool IsInvited(Pawn pawn) =>
			lord.faction != null && pawn.Faction == lord.faction;
        
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

		protected abstract void CreatePartyRoles();    
        
        protected override StateGraph CreateGraphAndRoles()
        {
			CreatePartyRoles();
        
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

            preparationSucceeded = new Transition(prepareToil, partyToil);
            preparationSucceeded.AddTrigger(new Trigger_Memo(PreparationCompleteMemo));
            preparationSucceeded.AddPreAction(new TransitionAction_Custom(
                () => partyToil.PreparationScore = prepareToil.CalculatePreparationScore()));
            preparationSucceeded.AddPostAction(new TransitionAction_Custom(() => {
                this.partyHasStarted = true;
            }));
            preparationSucceeded.AddPostAction(new TransitionAction_Message("EP.PartyStarted.TransitionMessage".Translate()
                                                , MessageTypeDefOf.PositiveEvent, new TargetInfo(PartySpot, Map)));

            preparationFailed = new Transition(prepareToil, endToil);
            preparationFailed.AddTrigger(new Trigger_Memo(PreparationFailedMemo));
			preparationFailed.AddTrigger(new Trigger_TickCondition(() => this.ShouldBeCalledOff()));
            preparationFailed.AddPreAction(new TransitionAction_Message("EP.PreparationFailed.TransitionMessage".Translate()
                                                , MessageTypeDefOf.NegativeEvent, new TargetInfo(PartySpot, Map)));  //TODO make PartySpot lazily updatable

            if(Def.failOnPreparationTimeout)
                preparationFailed.AddTrigger(preparationTimeout);
            else
                preparationSucceeded.AddTrigger(preparationTimeout);

            stateGraph.AddTransition(preparationSucceeded);
            stateGraph.AddTransition(preparationFailed);

            partyFailed = new Transition(partyToil, endToil);
            partyFailed.AddTrigger(new Trigger_Memo("PartyFailed"));
            partyFailed.AddTrigger(new Trigger_TickCondition(() => this.ShouldBeCalledOff()));
            partyFailed.AddPreAction(new TransitionAction_Message("EP.PartyFailed.TransitionMessage".Translate()
                                                , MessageTypeDefOf.NegativeEvent, new TargetInfo(PartySpot, Map)));
            partyFailed.AddTrigger(new Trigger_TickCondition(
                                () => partyToil.CurrentPartyStatus() == PartyStatus.Interrupted)); 
                                                               
            partySucceeded = new Transition(partyToil, endToil);
            partySucceeded.AddTrigger(new Trigger_Memo("PartySuccess"));
            partySucceeded.AddTrigger(new Trigger_TickCondition(
                                () => partyToil.CurrentPartyStatus() == PartyStatus.Finished));

            if(Def.failOnPartyTimeout)
                partyFailed.AddTrigger(this.partyTimeout);
            else
                partySucceeded.AddTrigger(this.partyTimeout);

            stateGraph.AddTransition(partyFailed);
            stateGraph.AddTransition(partySucceeded);
            
            return stateGraph;
        }   

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<EnhancedPartyDef>(ref this.def, "Def");
            Scribe_References.Look<Pawn>(ref this.organizer, "Organizer");
            Scribe_Values.Look<IntVec3>(ref this.startingSpot, "StartingSpot");
            Scribe_Values.Look<int>(ref this.partySpotIndex, "PartySpotIndex");
        }

        public virtual bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableGameConditionsToContinueParty(this.Map) || !this.PartySpot.Roofed(this.Map);
        }

		public override float VoluntaryJoinPriorityFor(Pawn p)
		{
			if(!IsInvited(p))
				return 0f;

			if(!EnhancedPartyUtility.CanPawnKeepPartyingBasicChecks(p))
				return 0f;

			if(PartySpot.IsForbidden(p))
				return 0f;
                
            if (!this.lord.ownedPawns.Contains(p) && this.IsPartyAboutToEnd)
                return 0f;

			if(p == Organizer)
				return EnhancedPartyJoinPriorities.organizer;

			return EnhancedPartyJoinPriorities.normalGuest;
		}
        
		public override void Notify_PawnAdded(Pawn p)
		{
			base.Notify_PawnAdded(p);
	//		Log.Message($"Gained pawn {p.Name}");
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			base.Notify_PawnLost(p, condition);

	//		Log.Message($"Lost pawn {p.Name}");
		}

		public override string GetReport() => PartyHasStarted
												? "EP.Party.Report".Translate()
                                                : "EP.Prepare.Report".Translate();
	}
}
