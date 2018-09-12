using System;
using Verse;
using RimWorld;

namespace EnhancedParty
{
    static public class ThingExt
    {
        //TODO add table logic
        static public bool AtDutyDestination(this Thing thing, EnhancedPawnDuty duty, Pawn pawn)
        {
            if(duty.stayInRoom) {
                Room room = duty.focus.Cell.GetRoom(pawn.Map, RegionType.Set_Passable);
                if(room != null)
                    return thing.PositionHeld.GetRoom(pawn.Map, RegionType.Set_Passable) == room;
            }

            return thing.PositionHeld.DistanceToSquared(duty.focus.Cell) <= (duty.radius * duty.radius); 
        }

        static public bool HasPower(this Thing thing, bool defaultIfNoPowerComp = true) =>
            thing.TryGetComp<CompPowerTrader>()?.PowerOn ?? thing.TryGetComp<CompPowerPlant>()?.PowerOn ?? defaultIfNoPowerComp;

        static public bool IsChair(this Thing thing) =>
            thing.def.building?.isSittable ?? false;

        static public bool IsTelevision(this Thing thing) =>
            thing.def.building?.joyKind == MoreJoyKindDefs.Television;
    }
}
