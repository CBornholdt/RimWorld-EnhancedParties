using Verse;

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
    }
}
