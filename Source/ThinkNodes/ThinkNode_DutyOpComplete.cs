using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using EnhancedParty;
using Verse.AI.Group;

namespace RimWorld
{
    public class ThinkNode_DutyOpComplete : ThinkNode
    {
        public bool repeatProtection = true;
        public int repeatProtectionTicks = 250;

        Queue<Tuple<Pawn, int>> pawnsLastUsed = new Queue<Tuple<Pawn, int>>();
    
        public ThinkNode_DutyOpComplete()
        {
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode node = new ThinkNode_DutyOpComplete() {
                repeatProtection = this.repeatProtection
            };
            return node;
        }

        public void SetRepeatProtection(Pawn pawn) =>
            pawnsLastUsed.Enqueue(Tuple.Create(pawn, Find.TickManager.TicksGame + repeatProtectionTicks));

        public void RemoveExpiredProtection()
        {
            while(pawnsLastUsed.Any() && pawnsLastUsed.Peek().Item2 <= Find.TickManager.TicksGame)
                pawnsLastUsed.Dequeue();
        }

        public bool AlreadyTriggered(Pawn pawn) => pawnsLastUsed.Any(tuple => tuple.Item1 == pawn);

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            EnhancedPawnDuty duty = pawn.mindState?.duty as EnhancedPawnDuty;

            if(repeatProtection)
                RemoveExpiredProtection();
        
            if(duty == null || (repeatProtection && AlreadyTriggered(pawn)))
                return ThinkResult.NoJob;

            if(repeatProtection)
                SetRepeatProtection(pawn);

            return new ThinkResult(new JobWithDutyMessage(DutyOpMessageType.OpSucceeded, duty.taskName) {
                                                                def = MoreJobDefs.DutyMessage
                                                            }
                                    , sourceNode: this, tag: JobTag.UnspecifiedLordDuty, fromQueue: false);
        }
    }
}
