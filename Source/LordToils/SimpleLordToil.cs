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
			if (cancelExistingJobsOnEntry)
				this.lord.CancelAllPawnJobs();
		}

		public override void UpdateAllDuties()
		{
			LordJob.CheckAndUpdateRoles();
		}
	}
}
