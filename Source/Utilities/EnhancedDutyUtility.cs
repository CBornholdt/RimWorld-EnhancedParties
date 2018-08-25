using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    static public class EnhancedDutyUtility
    {
        static public bool IsCellInDutyArea(Pawn pawn, IntVec3 cell)
        {
            EnhancedPawnDuty duty = pawn?.mindState.duty as EnhancedPawnDuty;
            if(duty == null)
                return cell == (pawn?.mindState.duty.focus.Cell ?? IntVec3.Invalid);

            if(duty.stayInRoom)
                return cell.GetRoom(pawn.Map, RegionType.Set_Passable)
                        == duty.focus.Cell.GetRoom(pawn.Map, RegionType.Set_Passable);

            if(duty.radius >= 1)
                return cell.DistanceToSquared(duty.focus.Cell) <= (duty.radius * duty.radius);
            
            return cell == duty.focus.Cell;
        }
    
        static public bool IsInDutyArea(Pawn pawn)
        {
			EnhancedPawnDuty duty = pawn?.mindState.duty as EnhancedPawnDuty;
			if(duty == null)
				return pawn.Position == (pawn?.mindState.duty.focus.Cell ?? IntVec3.Invalid);

			if(duty.stayInRoom)
				return pawn.Position.GetRoom(pawn.Map, RegionType.Set_Passable)
					    == duty.focus.Cell.GetRoom(pawn.Map, RegionType.Set_Passable);

			if(duty.radius >= 1)
				return pawn.Position.DistanceToSquared(duty.focus.Cell) <= (duty.radius * duty.radius);
            
			return pawn.Position == duty.focus.Cell;
        }

		static public IEnumerable<IntVec3> DutyAreaCells(Pawn pawn)
		{
            EnhancedPawnDuty duty = pawn?.mindState.duty as EnhancedPawnDuty;
			if(duty == null) {
				yield return pawn?.mindState.duty.focus.Cell ?? IntVec3.Invalid;
				yield break;
			}

			if(duty.stayInRoom) {
				foreach(var cell in pawn.Position.GetRoom(pawn.Map, RegionType.Set_Passable).Cells)
					yield return cell;
				yield break;
			}

			if(duty.radius >= 1) {
				foreach(var cell in pawn.Position.PassableCellsInRadiusAround(pawn.Map, duty.radius))
					yield return cell;
				yield break;
			}

			yield return duty.focus.Cell;
		}
    }
}
