using System;
using Verse;
using RimWorld;

namespace EnhancedParty
{
    public interface ICleanableAction : IExposable, ILoadReferenceable
    {
		bool CleanupStillNeeded();

		bool ReferencesBroken();
    
		void PerformCleanup(); 	
	}
}
