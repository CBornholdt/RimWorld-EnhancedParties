using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
    public class MovieNight_PartyToil : EnhancedLordToil_Party
    {
		static public readonly float PctPartyTimeInIntermission = 0.15f;

		protected Trigger_TicksPassed movieTimeout;
		protected Trigger_TicksPassed intermissionTimeout;

		SimpleLordToil watchMovies1;
		SimpleLordToil intermission;
		SimpleLordToil watchMovies2;
    
        public MovieNight_PartyToil(EnhancedPartyDef partyDef)
        {
			this.data = new PartyToilData() { def = partyDef };
        }
        
        public new PartyJob_MovieNight LordJob => this.lord?.LordJob as PartyJob_MovieNight;

        public override StateGraph CreateInternalGraph()  
        {
            StateGraph graph = new StateGraph();

            watchMovies1 = new SimpleLordToil(this);
			graph.AddToil(watchMovies1);
            
			intermission = new SimpleLordToil(this);
			graph.AddToil(intermission);
            
            watchMovies2 = new SimpleLordToil(this);
            graph.AddToil(watchMovies2);

			this.intermissionTimeout = new Trigger_TicksPassed((int)(PctPartyTimeInIntermission * Data.def.partyTimeout));
            //2 movie sections
			this.movieTimeout = new Trigger_TicksPassed((int)((1 - PctPartyTimeInIntermission) * Data.def.partyTimeout / 2f) + 2);

			var goToIntermission = new Transition(watchMovies1, intermission);
			goToIntermission.AddTrigger(movieTimeout);
			graph.AddTransition(goToIntermission);

			var goToSecondHalf = new Transition(intermission, watchMovies2);
			goToSecondHalf.AddTrigger(intermissionTimeout);
			graph.AddTransition(goToSecondHalf);

			var notifyMovieComplete = new Transition(watchMovies2, watchMovies2, canMoveToSameState: true, updateDutiesIfMovedToSameState: false);
			notifyMovieComplete.AddPostAction(new TransitionAction_SendMemo(EnhancedLordJob_Party.PartyCompleteMemo));
			graph.AddTransition(notifyMovieComplete);

            return graph;
        }

		public void AssignMovieWatchingDuties()
		{
			foreach(var pawn in lord.ownedPawns)
				pawn.mindState.duty = new EnhancedPawnDuty(EnhancedDutyDefOf.EP_MovieNight_WatchMovie, LordJob.GetAssignedSeating(pawn)
                                                    , LordJob.television);
		}

		public void AssignIntermissionDuties()
		{
            foreach(var pawn in lord.ownedPawns)
                pawn.mindState.duty = new EnhancedPawnDuty(EnhancedDutyDefOf.EP_MovieNight_Intermission, LordJob.television);
		}

        public override LordToil SelectSubToil() => watchMovies1;

		public override void UpdateAllDuties()  //Will be called by UpdateAllDuties on the various subToils
		{
			var toil = lord.CurLordToil;
			if(toil == watchMovies1)
				AssignMovieWatchingDuties();
			if(toil == intermission) {
				AssignIntermissionDuties();
				lord.CancelAllPawnJobs();
			}
			if(toil == watchMovies2) {
				AssignMovieWatchingDuties();
				lord.CancelAllPawnJobs();
			}
		}
	}
}
