using System;
using Verse;

namespace EnhancedParty
{
    public class SimpleLordToil : EnhancedLordToil
    {	
        public SimpleLordToil(ComplexLordToil parentToil = null) : base(parentToil)
        {	
        }

        public override void Notify_PawnJoinedLord(Pawn pawn)
        {
            ParentToil?.Notify_PawnJoinedLord(pawn);
        }
    }
}
