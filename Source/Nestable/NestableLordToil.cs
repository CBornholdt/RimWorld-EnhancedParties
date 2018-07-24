using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Harmony;

namespace EnhancedParty
{
    abstract public class NestableLordToil : LordToil
    {
		private NestableLordToilData Data => (NestableLordToilData)this.data;

		protected int CurrentIndex => Data.currentIndex;

		protected abstract IEnumerable<LordToil> CreateSubToils(out IEnumerable<Transition> transitions);

		protected List<LordToil> containedToils;
		protected List<Transition> containedTransitions;

		public List<LordToil> ContainedToils => containedToils;
		public List<Transition> ContainedTransitions => containedTransitions;

		public NestableLordToil()
		{
			this.data = new NestableLordToilData();
		}

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
            
			foreach(var toil in containedToils)
				graph.AddToil(toil);
                
            //Any transitions from this state, also transition from all child states. Will be applied recursively
			foreach(var transition in graph.transitions)
				if(transition.sources.Contains(this))
					transition.sources.AddRange(containedToils);
                
			foreach(var transition in transitions)
				graph.AddTransition(transition);

			foreach(var toil in containedToils)
				if(toil is NestableLordToil nestedToil)
					nestedToil.ConfigureFor(graph);
		}
    }
}
