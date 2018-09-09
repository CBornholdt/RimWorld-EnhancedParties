using System;

using System.Collections.Generic;
using System.Linq;
using EnhancedParty;

namespace Verse
{
    static public class CellExt
    {
        static public bool OnRoomEdge(this IntVec3 cell, Room room)
        {
            for(int i = 0; i < 8; i++) {
                IntVec3 prospective = cell + GenAdj.AdjacentCells[i];
                Region region = (cell + GenAdj.AdjacentCells[i]).GetRegion(room.Map, RegionType.Set_Passable);
                if(region == null || region.Room != room)
                    return true;
            }
            return false;
        }

        static public bool HasTableAt(this IntVec3 cell, Map map) =>
            cell.GetThingList(map).Any(thing => thing.def.surfaceType == SurfaceType.Eat);

        static public IEnumerable<Thing> AllTablesWithin(this IEnumerable<IntVec3> cells, Map map) =>
            cells.Select(cell => cell.GetThingList(map)
                                        .FirstOrDefault(thing => thing.def.surfaceType == SurfaceType.Eat))
                 .Where(thing => thing != null)
                 .Distinct();

        static public IEnumerable<IntVec3> PassableCellsInRadiusAround(this IntVec3 center, Map map, float radius) =>
            GenRadial.RadialCellsAround(center, radius, true)
                     .Where(cell => cell.InBounds(map) && cell.Walkable(map));

        static public bool TryGetEnhancedPartyLordJob(this IntVec3 cell, Map map, out EnhancedLordJob_Party job) =>
            (job = (EnhancedLordJob_Party) map.lordManager.lords
                                            .Select(lord => lord.LordJob)
                                            .FirstOrDefault(lordJob => lordJob is EnhancedLordJob_Party partyJob 
                                                && partyJob.PartySpot == cell)) != null; 
    }
}
