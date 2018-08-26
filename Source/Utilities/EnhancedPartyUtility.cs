using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    static public class EnhancedPartyUtility
    {
		static public bool CanPawnJoinPartyBasicChecks(Pawn pawn)
		{
			return CanPawnKeepPartyingBasicChecks(pawn);
		}

		static public bool CanPawnKeepPartyingBasicChecks(Pawn pawn)
		{
			return true;
		}

		static public bool TryStartEnhancedPartyDef(Faction faction, Map map, EnhancedPartyDef partyDef)
		{
            Pawn organizer = null;
            IntVec3 startingSpot = default(IntVec3);

			if(!partyDef.TryGetOrganizerAndStartingSpot(faction, map, out organizer, out startingSpot))
				return false;
                
            if(organizer == null || startingSpot == IntVec3.Invalid)
                return false;
                
            var startingPawns = partyDef.forceAddOrganizer
                                        ? new List<Pawn>(1) { organizer }
                                        : null;
                                        
            var lordJob = partyDef.CreateLordJob(organizer, startingSpot);
            Lord lord = LordMaker.MakeNewLord(faction, lordJob, map, startingPawns);
            Log.Message($"Lord owned pawns {lord.ownedPawns.Count}  organizer: {organizer.Name}");
            if(partyDef.partyPreparationStartLetterTitle != null)
                Find.LetterStack.ReceiveLetter(partyDef.partyPreparationStartLetterTitle
                    , partyDef.partyPreparationStartLetterText ?? partyDef.partyPreparationStartLetterTitle
                    , LetterDefOf.PositiveEvent, new TargetInfo(lordJob.PartySpot, map), debugInfo: null);

            if(partyDef.forceAddOrganizer)
                organizer.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob: true);

			return true;
		}

		static public bool TryStartEnhancedParty(Faction faction, Map map)
		{  
			var potentialEnhancedParties = DefDatabase<EnhancedPartyDef>.AllDefs
											.Where(def => def.PartyCanBeHadWith(faction, map))
											.ToList();

			if(!potentialEnhancedParties.Any())
				return false;

			potentialEnhancedParties.Shuffle();	
			EnhancedPartyDef partyDef = null;

			foreach(var def in potentialEnhancedParties)
				if(TryStartEnhancedPartyDef(faction, map, def)) {
					partyDef = def;
				}

			if(partyDef == null)
				return false;

			return true;
		}
    }
}
