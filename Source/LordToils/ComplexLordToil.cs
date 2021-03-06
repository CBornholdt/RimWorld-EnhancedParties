﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Harmony;

namespace EnhancedParty
{
    abstract public class ComplexLordToil : EnhancedLordToil
    {
        public ComplexLordToil() : base()
        {
            this.data = new ComplexLordToilData();
        }

        private ComplexLordToilData Data => (ComplexLordToilData)this.data;
        
        public int CurrentIndex => Data.currentIndex;

        protected List<LordToil> containedToils;
        protected List<Transition> containedTransitions;

        public List<LordToil> ContainedToils => containedToils;
        public List<Transition> ContainedTransitions => containedTransitions;

        public virtual LordToil SelectSubToil() => containedToils[CurrentIndex];

        public IEnumerable<LordToil> AllDescendantToils()
        {
            foreach(LordToil toil in ContainedToils) {
                if(toil is ComplexLordToil complexToil) {
                    foreach(var innerToil in complexToil.AllDescendantToils())
                        yield return innerToil;
                }
                else
                    yield return toil;
            }
        }

        public void AttachTo(StateGraph graph)
        {
            graph.AddToil(this);
            StateGraph internalGraph = this.CreateInternalGraph();
            graph.AttachSubgraph(this.CreateInternalGraph());
            containedToils = internalGraph.lordToils;
            containedTransitions = internalGraph.transitions;
        }

        abstract public StateGraph CreateInternalGraph();
    }
}