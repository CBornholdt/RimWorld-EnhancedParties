using System;
using Verse;

namespace EnhancedParty
{
    public class SimpleLordToil : EnhancedLordToil
    {	
        public SimpleLordToil(ComplexLordToil parentToil = null) : base(parentToil)
        {	
        }

        public override void Notify_PawnDutyOpComplete(string dutyOp, Pawn pawn)
        {
            ParentToil?.Notify_PawnDutyOpComplete(dutyOp, pawn);
        }

        public override void Notify_PawnDutyOpFailed(string dutyOp, Pawn pawn)
        {
            ParentToil?.Notify_PawnDutyOpFailed(dutyOp, pawn);
        }

        public override void Notify_PawnJoinedLord(Pawn pawn)
        {
            ParentToil?.Notify_PawnJoinedLord(pawn);
        }     
    }
}
