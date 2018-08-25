using System;
using Verse;
using Verse.AI;
using EnhancedParty;

namespace RimWorld
{
    public class DutyConditional_InDutyArea : ThinkNode_Conditional
    {
		protected override bool Satisfied(Pawn pawn) => pawn.IsInDutyArea();
    }
}
