using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
    static public class RoomExt
    {
        static public IEnumerable<Thing> ThingsInside(this Room room) =>
                    room.Cells.SelectMany(cell => cell.GetThingList(room.Map));
    
        static public IEnumerable<Building> TablesInside(this Room room) =>
                    room.ThingsInside()
                        .OfType<Building>()
                        .Where(building => building.def.surfaceType == SurfaceType.Eat);
    
        static public IEnumerable<Building> GatherableTablesInside(this Room room) =>
                    room.TablesInside()
                        .Where(table => table.TryGetComp<CompGatherSpot>()?.Active ?? false);
    }
}
