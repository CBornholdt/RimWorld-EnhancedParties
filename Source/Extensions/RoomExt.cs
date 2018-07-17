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
    }
}
