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
        public EnhancedLordToil()
        {
            if (LordJob == null)
                Log.ErrorOnce($"Error constructing {this.GetType()}, LordJob is not subclass of EnhancedLordJob", 87228);

			this.data = new EnhancedLordToilData();
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

		private EnhancedLordToilData Data => (EnhancedLordToilData)this.data;
        
        public int CurrentIndex => Data.CurrentIndex;

        protected abstract IEnumerable<LordToil> CreateSubToils(out IEnumerable<Transition> transitions);

        protected List<LordToil> containedToils;
        protected List<Transition> containedTransitions;

        public List<LordToil> ContainedToils => containedToils;
        public List<Transition> ContainedTransitions => containedTransitions;

        //TODO adjust Init to use some type of initial states
        override public void Init()
        {
            Traverse.Create(this.lord).Field("curLordToil").SetValue(containedToils[CurrentIndex]);
            containedToils[CurrentIndex].Init();
        }

        virtual public void ConfigureFor(StateGraph graph)
        {
            this.containedToils = new List<LordToil>(CreateSubToils(out IEnumerable<Transition> transitions));
            this.containedTransitions = new List<Transition>(transitions);

            foreach (var toil in containedToils)
                graph.AddToil(toil);

            //Any transitions from this state, also transition from all child states. Will be applied recursively
            foreach (var transition in graph.transitions)
                if (transition.sources.Contains(this))
                    transition.sources.AddRange(containedToils);

            foreach (var transition in transitions)
                graph.AddTransition(transition);
            
            foreach (var toil in containedToils)
                if (toil is EnhancedLordToil enhancedToil)
                    enhancedToil.ConfigureFor(graph);
        }
    }
}