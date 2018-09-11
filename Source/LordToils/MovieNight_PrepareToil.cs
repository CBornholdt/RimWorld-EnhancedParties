using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;
using Harmony;

namespace EnhancedParty
{
    public class MovieNight_PrepareToil : EnhancedLordToil_PrepareParty
    {
        LordToil subToil;
    
        public MovieNight_PrepareToil()
        {
            this.data = new PreparePartyToilData();
        }
        
        public new PartyJob_MovieNight LordJob => this.lord?.LordJob as PartyJob_MovieNight;

        public override StateGraph CreateInternalGraph()  
        {
            StateGraph graph = new StateGraph();

            SimpleLordToil stubToil = new SimpleLordToil(this); 
            graph.AddToil(stubToil);
            subToil = stubToil;

            return graph;
        }

    /*	public bool CellHasChairFacingTelevision(IntVec3 cell) =>
            cell.GetThingList(Map).Any(thing => thing.Rotation == LordJob.viewingDirection
                                            && (thing.def.building?.isSittable ?? false));  */
        
        public void AssignRoleAndDuty(Pawn pawn)
        {
            var seat = LordJob.GetAssignedSeating(pawn);
            var chair = LordJob.GetAssignedChair(pawn);
            if(chair.Position == seat && chair.Rotation == LordJob.viewingDirection) {
                pawn.SetLordPawnRole(PartyJob_MovieNight.Viewers);
                pawn.mindState.duty = MakeViewersDuty(pawn, seat);
            }
            else {
                pawn.SetLordPawnRole(PartyJob_MovieNight.ChairMovers);
                pawn.mindState.duty = MakeChairMoverDuty(pawn, chair, seat);
            }
        }

        public EnhancedPawnDuty MakeChairMoverDuty(Pawn pawn, Thing chair, IntVec3 destination) =>
            new EnhancedPawnDuty(EnhancedDutyDefOf.EP_MoveBuildingToFocus, chair) {
                focusSecond = destination,
                direction = LordJob.viewingDirection
            };
        

        public EnhancedPawnDuty MakeViewersDuty(Pawn pawn, IntVec3 cell) =>
            new EnhancedPawnDuty(EnhancedDutyDefOf.EP_GotoAndCleanFocusRoom, cell);          

        public override LordToil SelectSubToil() => subToil;
    }
}
