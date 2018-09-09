using System;
using System.Reflection;
using RimWorld;
using Verse.AI;
using EnhancedParty;
using System.Collections.Generic;

namespace Verse
{
    public class EnhancedPartyDef : Def
    {
		public Type enhancedLordJobClass;
		public int minNumOfPartiers = 4;
		public int maxNumOfPartiers = 1000;
		public bool allowCrossFaction = false;
		public ThinkTreeDutyHook dutyHook = ThinkTreeDutyHook.MediumPriority;
		public string partyPreparationStartLetterTitle;
        public string partyPreparationStartLetterText;
		public int preparationTimeout = 5000;
		public bool failOnPreparationTimeout = false;

		public bool forceAddOrganizer = true;
        
		public int partyTimeout = 5000;
		public bool failOnPartyTimeout = false;
		public int ticksLeftWhenPartyAboutToEnd = 1200;

		public int ticksPerPreparationPulse = 1200;
		public int ticksPerPartyPulse = 600;

		public bool useWholePartyRoom = false;
		public bool keepPartyInRoom = false;

		private EnhancedLordJob_Party intLordJob;
		private EnhancedLordJob_Party IntLordJob => intLordJob;

		public EnhancedLordJob_Party CreateLordJob(Pawn organizer, IntVec3 startingSpot) =>
			(EnhancedLordJob_Party)Activator.CreateInstance(enhancedLordJobClass
														, new object[3] { this, organizer, startingSpot }); 

		public bool PartyCanBeHadWith(Faction faction, Map map)
		{
			return IntLordJob.PartyCanBeHadWith(faction, map);
		}

		public bool TryGetOrganizerAndStartingSpot(Faction faction, Map map, out Pawn organizer, out IntVec3 startingSpot)
		{
			organizer = null;
			startingSpot = default(IntVec3);
			return IntLordJob.TryGetOrganizerAndStartingSpot(faction, map, out organizer, out startingSpot);
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach(var error in base.ConfigErrors())
				yield return error;

			if(!typeof(EnhancedLordJob_Party).IsAssignableFrom(enhancedLordJobClass))
				yield return $"<enhancedLordJobClass> configured in EnhancedPartyDef {this.label} is not a sub-class of EnhancedLordJob_Party";
			else {
				intLordJob = (EnhancedLordJob_Party)Activator.CreateInstance(enhancedLordJobClass);
				intLordJob.def = this;
			}
		}
	}
}
