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
    public class LordJob_EnhancedParty : LordJob_JoinableRoles
    {
        EnhancedPartyWorker cachedWorker;
        Pawn organizer;
        EnhancedPartyDef def;
        IntVec3 startingSpot;
        int partySpotIndex;
        IntVec3 currentPartySpot = IntVec3.Invalid;
		bool partyHasStarted;

        Trigger_TicksPassed preparationTimeout;
        Trigger_TicksPassed partyTimeout;

        List<Func<IntVec3>> partySpotGenerators;

        public LordJob_EnhancedParty() { }
        
        public LordJob_EnhancedParty(EnhancedPartyDef def, Pawn organizer, IntVec3 startingSpot)
        {
            cachedWorker = def.CreateWorker(lordJob: this);
            this.def = def;
            this.organizer = organizer;
            this.startingSpot = startingSpot;
            this.partySpotIndex = 0;
			this.partyHasStarted = false;
        }

        public EnhancedPartyDef Def => def;

        public EnhancedPartyWorker Worker => cachedWorker;

        public override bool AllowStartNewGatherings => false;

		public bool PartyHasStarted => this.partyHasStarted;

        public Pawn Organizer => organizer;

        public IntVec3 PartySpot => currentPartySpot;

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

        public bool IsAttendingParty(Pawn pawn) => Worker.IsAttendingParty(pawn);

        public override StateGraph CreateGraph()
        {
            partySpotGenerators = new List<Func<IntVec3>>(Worker.PartySpotProgressionFrom(startingSpot));
            UpdatePartySpot();
            
            StateGraph stateGraph = new StateGraph();

            LordToil_EnhancedParty_Prepare prepareToil = new LordToil_EnhancedParty_Prepare();
            stateGraph.AddToil(prepareToil);

            LordToil_EnhancedParty_Party partyToil = new LordToil_EnhancedParty_Party();
            stateGraph.AddToil(partyToil);

            LordToil_End endToil = new LordToil_End();
            stateGraph.AddToil(endToil);
            
            Log.Message($"here2 and worker == null: {Worker == null}");
            Log.Message($"PreparationScore: {partyToil.PreparationScore}");
            Log.Message($"WorkerPreparationScore: {Worker.PreparationScore()}");

            this.preparationTimeout = new Trigger_TicksPassed(Def.preparationTimeout);
            this.partyTimeout = new Trigger_TicksPassed(Def.partyTimeout);

            Transition preparationSucceeded = new Transition(prepareToil, partyToil);
            preparationSucceeded.AddTrigger(new Trigger_Memo("PreparationComplete"));
            preparationSucceeded.AddPreAction(new TransitionAction_Custom(
                () => partyToil.PreparationScore = Worker.PreparationScore()));
			preparationSucceeded.AddPostAction(new TransitionAction_Custom(() => {
				this.CancelJobsForAttendees();
				this.partyHasStarted = true;
			}));

			EnhancedPartyWorker.PreparationStatus successStatus = def.progressToPartyWhenPreparationComplete
														? EnhancedPartyWorker.PreparationStatus.Complete
														: EnhancedPartyWorker.PreparationStatus.Maximal;
            preparationSucceeded.AddTrigger(new Trigger_TickCondition(
                    () => Worker.CurrentPreparationStatus() >= successStatus));

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
            partyOverFail.AddPreAction(new TransitionAction_Message("EP.PartyFail.TransitionMessage".Translate()
                                                , MessageTypeDefOf.NegativeEvent, new TargetInfo(PartySpot, Map)));
            partyOverFail.AddTrigger(new Trigger_TickCondition(
                                () => Worker.CurrentPartyStatus() == EnhancedPartyWorker.PartyStatus.Interrupted)); 
                                                               

            Transition partyOverSuccess = new Transition(partyToil, endToil);
            partyOverSuccess.AddTrigger(new Trigger_Memo("PartySuccess"));
			partyOverSuccess.AddTrigger(new Trigger_TickCondition(
				                () => Worker.CurrentPartyStatus() == EnhancedPartyWorker.PartyStatus.Finished));

            if(Def.failOnPartyTimeout)
                partyOverFail.AddTrigger(this.partyTimeout);
            else
                partyOverSuccess.AddTrigger(this.partyTimeout);

            stateGraph.AddTransition(partyOverFail);
            stateGraph.AddTransition(partyOverSuccess);
            return stateGraph;
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
            Scribe_Deep.Look<EnhancedPartyWorker>(ref this.cachedWorker, "CachedWorker", new object[2] { this.def, this });    
            Scribe_References.Look<Pawn>(ref this.organizer, "Organizer");
            Scribe_Values.Look<IntVec3>(ref this.startingSpot, "StartingSpot");
            Scribe_Values.Look<int>(ref this.partySpotIndex, "PartySpotIndex");
        }

        private bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableGameConditionsToContinueParty(this.Map) || !this.PartySpot.Roofed(this.Map);
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
			float result = Worker.VoluntaryJoinPriorityFor(p);
		//	Log.Message($"Join priority for {p.Name} is {result}");
			return result;
        }

		public override void Notify_PawnAdded(Pawn p)
		{
			base.Notify_PawnAdded(p);
			Log.Message($"Gained pawn {p.Name}");
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			base.Notify_PawnLost(p, condition);

			Log.Message($"Lost pawn {p.Name}");

		/*	ThinkTreeDef treeDef = null;
			object[] arguments = new object[1] { null };
			MethodInfo determineNextJob = typeof(Pawn_JobTracker).GetMethod("DetermineNextJob"
							, BindingFlags.NonPublic | BindingFlags.Instance);
			ThinkResult normalResult = (ThinkResult)determineNextJob.Invoke(p.jobs, arguments);
			treeDef = (ThinkTreeDef)arguments[0];									

			Log.Message($"Lost pawn {p.Name}   Normal ThinkTree: {treeDef?.label ?? "NULL"}   Normal Result: {normalResult}     Normal Result Job: {normalResult.Job}");*/
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
