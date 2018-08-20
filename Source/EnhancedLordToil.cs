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

        public ComplexLordToil ParentToil => ParentToil;
    
        public EnhancedLordToil(ComplexLordToil parentToil = null) : base()
        {
            if (LordJob == null)
                Log.ErrorOnce($"Error constructing {this.GetType()}, LordJob is not subclass of EnhancedLordJob", 87228);

			this.parentToil = parentToil;
        }
        
        public EnhancedLordJob LordJob => this.lord.LordJob as EnhancedLordJob;

        public override void UpdateAllDuties()
        {
            LordJob.CheckAndUpdateRoles();
        }

        public virtual void Notify_PawnJoinedRole(LordPawnRole role, Pawn pawn, LordPawnRole prevPawnRole) =>
                LordJob.Notify_PawnJoinedRole(role, pawn, prevPawnRole);

        public virtual void Notify_PawnLeftRole(LordPawnRole role, Pawn pawn, LordPawnRole newPawnRole) =>
                LordJob.Notify_PawnLeftRole(role, pawn, newPawnRole);
        
        public virtual void Notify_PawnReplacedPawnInRole(LordPawnRole role, Pawn newPawn, Pawn oldPawn
                            , LordPawnRole newPawnOldRole, LordPawnRole oldPawnNewRole) =>
                LordJob.Notify_PawnReplacedPawnInRole(role, newPawn, oldPawn, newPawnOldRole, oldPawnNewRole);

		public StateGraph AttachAnyInternalStateGraphTo(StateGraph graph)
		{
			StateGraph subGraph;
			if (this is ComplexLordToil complexToil && (subGraph = complexToil.CreateInternalGraph()) != null)
				graph.AttachSubgraph(subGraph);
			return graph;
		}
	}
}