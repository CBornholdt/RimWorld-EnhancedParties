using System;
using System.Collections.Generic;
using Verse.AI;
namespace EnhancedParty
{
    public class JobDriver_DutyMessage : JobDriver
    {
        public JobDriver_DutyMessage()
        {
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil messageToil = new Toil() {
                initAction = () => {
                    JobWithDutyMessage messageJob = this.job as JobWithDutyMessage;
                    if(messageJob == null)
                        return;
                    if(messageJob.messageType == DutyOpMessageType.OpFailed)
                        DutyOpUtility.Notify_DutyOpFailed(messageJob.opName, this.pawn);
                    else
                        DutyOpUtility.Notify_DutyOpComplete(messageJob.opName, this.pawn);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return messageToil;
        }
    }
}
