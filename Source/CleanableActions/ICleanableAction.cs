using System;
using Verse;
using RimWorld;

namespace EnhancedParty
{
    public interface ICleanableAction : IExposable, ILoadReferenceable
    {
		bool CleanupStillNeeded();
    
		void PerformCleanup(); 	
	}
}
