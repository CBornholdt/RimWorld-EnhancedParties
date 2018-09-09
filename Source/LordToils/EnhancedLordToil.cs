using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Harmony;

namespace EnhancedParty
{
    abstract public class EnhancedLordToil : LordToil
    {
        private ComplexLordToil parentToil;

        public ComplexLordToil ParentToil => parentToil;

		private List<Tuple<string, Pawn>> completeDutyOps = new List<Tuple<string, Pawn>>();
        
        private List<Tuple<string, Pawn>> failedDutyOps = new List<Tuple<string, Pawn>>();
    
        public EnhancedLordToil(ComplexLordToil parentToil = null) : base()
        {
			this.parentToil = parentToil;
        }
        
        public EnhancedLordJob LordJob => this.lord.LordJob as EnhancedLordJob;

		public void RegisterDutyOpComplete(string dutyOp, Pawn pawn) =>
			completeDutyOps.Add(Tuple.Create(dutyOp, pawn));
            
        public void RegisterDutyOpFailed(string dutyOp, Pawn pawn) =>
            failedDutyOps.Add(Tuple.Create(dutyOp, pawn));    

		public virtual bool IsCellInDutyArea(Pawn pawn, IntVec3 cell)
		{
			return EnhancedDutyUtility.IsCellInDutyArea(pawn, cell);
		}
        
        public virtual bool IsInDutyArea(Pawn pawn)
        {
            return EnhancedDutyUtility.IsInDutyArea(pawn);
        }

        public virtual IEnumerable<IntVec3> DutyAreaCells(Pawn pawn)
        {
            return EnhancedDutyUtility.DutyAreaCells(pawn);
        }

		public override void UpdateAllDuties()
		{
			parentToil?.UpdateAllDuties();
		}

		public virtual void RefreshAllDuties()
		{
		}

		public virtual void Notify_PawnJoinedLord(Pawn pawn) { }

		public virtual void Notify_PawnJoinedRole(LordPawnRole role, Pawn pawn, LordPawnRole prevPawnRole) =>
                LordJob.Notify_PawnJoinedRole(role, pawn, prevPawnRole);

        public virtual void Notify_PawnLeftRole(LordPawnRole role, Pawn pawn, LordPawnRole newPawnRole) =>
                LordJob.Notify_PawnLeftRole(role, pawn, newPawnRole);
        
        public virtual void Notify_PawnReplacedPawnInRole(LordPawnRole role, Pawn newPawn, Pawn oldPawn
                            , LordPawnRole newPawnOldRole, LordPawnRole oldPawnNewRole) =>
                LordJob.Notify_PawnReplacedPawnInRole(role, newPawn, oldPawn, newPawnOldRole, oldPawnNewRole);

		virtual public void Notify_PawnDutyOpComplete(string dutyOp, Pawn pawn) =>
			ParentToil?.Notify_PawnDutyOpComplete(dutyOp, pawn);

		virtual public void Notify_PawnDutyOpFailed(string dutyOp, Pawn pawn) =>
			ParentToil?.Notify_PawnDutyOpFailed(dutyOp, pawn);

		public StateGraph AttachAnyInternalStateGraphTo(StateGraph graph)
		{
			StateGraph subGraph;
			if (this is ComplexLordToil complexToil && (subGraph = complexToil.CreateInternalGraph()) != null)
				graph.AttachSubgraph(subGraph);
			return graph;
		}

		public override void LordToilTick()
		{
			foreach(var completeOp in completeDutyOps)
				Notify_PawnDutyOpComplete(completeOp.Item1, completeOp.Item2);
			foreach(var failedOp in failedDutyOps)
				Notify_PawnDutyOpFailed(failedOp.Item1, failedOp.Item2);

			completeDutyOps.Clear();
			failedDutyOps.Clear();   
		}

		public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
		{
			return LordJob.DefaultToilDutyHook;
		}

		public override void Init()
		{
			EnhancedLordJob.transitioningToils = true;
		}
	}
}