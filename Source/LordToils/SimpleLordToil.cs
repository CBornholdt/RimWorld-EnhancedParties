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
        }

        public override void UpdateAllDuties()
        {
            LordJob.CheckAndUpdateRoles();
            base.UpdateAllDuties();
        }

        public override void RefreshAllDuties()
        {
            if(cancelExistingJobsOnEntry)
                lord.CancelAllPawnJobs();
        }
    }
}
