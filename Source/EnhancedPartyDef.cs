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
		public DutyDef partyDuty;
		public DutyDef prepareDuty;

		public Type lordToilPartyClass = typeof(LordToil_EnhancedParty_Party);
		public Type lordToilPrepareClass = typeof(LordToil_EnhancedParty_Prepare);
    
		public Type workerClass;
		public int minNumOfPartiers = 4;
		public int maxNumOfPartiers = 1000;
		public bool allowCrossFaction = false;
		public ThinkTreeDutyHook dutyHook = ThinkTreeDutyHook.MediumPriority;
		public string partyPreparationStartLetterTitle;
        public string partyPreparationStartLetterText;
		public int preparationTimeout = 5000;
		public bool failOnPreparationTimeout = false;
		public bool progressToPartyWhenPreparationComplete = true;

		public bool forceAddOrganizer = true;
        
		public int partyTimeout = 5000;
		public bool failOnPartyTimeout = false;

		public int ticksPerPreparationPulse = 1200;
		public int ticksPerPartyPulse = 600;

		private EnhancedPartyWorker workerInternal;

		public bool PartyCanBeHadWith(Faction faction, Map map)
		{
			return (workerInternal ?? CreatePrivateWorker()).PartyCanBeHadWith(faction, map);
		}

		public bool TryGetOrganizerAndStartingSpot(Faction faction, Map map, out Pawn organizer, out IntVec3 startingSpot)
		{
			organizer = null;
			startingSpot = default(IntVec3);
			return (workerInternal ?? CreatePrivateWorker())
                .TryGetOrganizerAndStartingSpot(faction, map, out organizer, out startingSpot);
		}

		public EnhancedPartyWorker CreateWorker(LordJob_EnhancedParty lordJob = null)
		{
			EnhancedPartyWorker newWorker = (EnhancedPartyWorker)Activator
                                                .CreateInstance(workerClass, new object[2] { this, lordJob });
			newWorker.def = this;
			return newWorker;
		}   

		private EnhancedPartyWorker CreatePrivateWorker()
		{
			workerInternal = CreateWorker();
			return workerInternal;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach(var error in base.ConfigErrors())
				yield return error;
        
			if(!typeof(EnhancedPartyWorker).IsAssignableFrom(workerClass))
				yield return $"<workerClass> configured in EnhancedPartyDef {this.label} is not a sub-class of EnhancedPartyWorker";
            if(!typeof(LordToil_EnhancedParty_Prepare).IsAssignableFrom(lordToilPrepareClass))
                yield return $"<lordToilPrepareClass> configured in EnhancedPartyDef {this.label} is not a sub-class of LordToil_EnhancedParty_Prepare";
            if(!typeof(LordToil_EnhancedParty_Party).IsAssignableFrom(lordToilPartyClass))
                yield return $"<lordToilPartyClass> configured in EnhancedPartyDef {this.label} is not a sub-class of LordToil_EnhancedParty_Party";
            if(prepareDuty == null && lordToilPrepareClass == typeof(LordToil_EnhancedParty_Prepare))
                yield return $"<prepareDuty> configured in EnhancedPartyDef {this.label} is null without a change in lordToilPrepareClass. This is a requirement";
            if(partyDuty == null && lordToilPartyClass == typeof(LordToil_EnhancedParty_Party))
                yield return $"<partyDuty> configured in EnhancedPartyDef {this.label} is null without a change in lordToilPartyClass. This is a requirement";
		}
	}
}
