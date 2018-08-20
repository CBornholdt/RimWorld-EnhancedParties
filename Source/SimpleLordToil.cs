using System;
namespace EnhancedParty
{
    public class SimpleLordToil : EnhancedLordToil
    {
		bool cancelExistingJobsOnEntry;
        
        public SimpleLordToil(ComplexLordToil parentToil = null
            , bool cancelExistingJobsOnEntry = false) : base(parentToil)
        {
			this.cancelExistingJobsOnEntry = cancelExistingJobsOnEntry;
        }

		public override void Init()
		{
			UpdateAllDuties();

			if (cancelExistingJobsOnEntry)
				this.lord.CancelAllPawnJobs();
		}
	}
}
