using System;
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

		abstract public StateGraph CreateInternalGraph();
    }
}