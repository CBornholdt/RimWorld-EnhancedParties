using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EnhancedParty
{
 /*   public abstract class EnhancedPartyWorker : IExposable
    {   
        //Will be set by containing LordJob on re-load

		public EnhancedPartyDef def;

		public enum PreparationStatus { NotStarted, NothingDone, Ongoing, Incompletable, Complete, Maximal };
		public enum PartyStatus { NotStarted, Ongoing, Interrupted, Interrupted_Continuable, Continued, Finished };

		public EnhancedPartyWorker() { }    //Will be created to use PartyCanBeHadWith

		public abstract PreparationStatus CurrentPreparationStatus();

		public abstract PartyStatus CurrentPartyStatus();
        
        abstract public bool PartyCanBeHadWith(Faction faction, Map map);

		abstract public bool TryGetOrganizerAndStartingSpot(Faction faction, Map map, out Pawn organizer, out IntVec3 startingSpot);
        
		abstract public bool AllowedToOrganize(Pawn pawn);

		virtual public float PreparationScore() => 1f;
        
        virtual public bool IsInvited(Pawn pawn) => this.LordJob.lord.faction != null && pawn.Faction == this.LordJob.lord.faction;

		virtual public bool IsAttendingParty(Pawn pawn) =>
			PartyUtility.InPartyArea(pawn.Position, lordJob.PartySpot, pawn.Map);

		virtual public IEnumerable<Func<IntVec3>> PartySpotProgressionFrom(IntVec3 initialSpot)
		{
			yield return () => initialSpot;
		}

		//Taken from LordJob_Joinable_Party.VoluntaryJoinPriorityFor
		virtual public float VoluntaryJoinPriorityFor(Pawn pawn)
		{
            if (!this.IsInvited(pawn) 
                || !PartyUtility.ShouldPawnKeepPartying(pawn)
                || this.lordJob.PartySpot.IsForbidden(pawn)
                || (!this.lordJob.lord.ownedPawns.Contains(pawn) && this.lordJob.IsPartyAboutToEnd()))
                return 0f;

			if(pawn == lordJob.Organizer)
				return EnhancedPartyJoinPriorities.organizer;
            
            return EnhancedPartyJoinPriorities.normalGuest;
		}

		virtual public bool ShouldPawnKeepPreparing(Pawn pawn) => PartyUtility.ShouldPawnKeepPartying(pawn);
        virtual public bool ShouldPawnKeepPartying(Pawn pawn) => PartyUtility.ShouldPawnKeepPartying(pawn);

		virtual public bool TryGivePreparationMemory(Pawn pawn, out ThoughtDef memory)
		{
			memory = null;
			return false;
		}

		virtual public bool TryGivePartyMemory(Pawn pawn, out ThoughtDef memory)
		{
			memory = null;
			return false;
		}

		virtual public int PreparationTimeoutTicks() => def.preparationTimeout;
		virtual public int PartyTimeoutTicks() => def.partyTimeout;

		virtual public void ExposeData()
		{
			Scribe_Defs.Look<EnhancedPartyDef>(ref this.def, "Def");
		}
    }   */
}
