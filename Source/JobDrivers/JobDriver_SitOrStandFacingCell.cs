using System;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace EnhancedParty
{
    public class JobDriver_SitOrStandFacingCell : JobDriver
    {
        public TargetIndex destinationIndex = TargetIndex.A;
        public TargetIndex facingDirectionIndex = TargetIndex.B;
    
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.CanReserveAndReach(TargetA, PathEndMode.OnCell, pawn.NormalMaxDanger()
                            , maxPawns: 1, stackCount: -1, layer: null, ignoreOtherReservations: false);       
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.OnCell);
            Toil sitOrStand = new Toil();
            if(TargetB.IsValid)
                sitOrStand.initAction = () => pawn.rotationTracker.FaceCell(TargetB.Cell);

            sitOrStand.tickAction = () => {
                this.pawn.GainComfortFromCellIfPossible();
            };

            sitOrStand.handlingFacing = true;
            sitOrStand.defaultCompleteMode = ToilCompleteMode.Never;

            yield return sitOrStand;
        }    
    }
}
