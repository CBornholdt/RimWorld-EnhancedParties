using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using Harmony;

namespace EnhancedParty
{
    abstract public class NestableLordToil : EnhancedLordToil
    {
		private ComplexLordToil parentToil;
    
        public NestableLordToil(ComplexLordToil parentToil = null) : base()
        {
			this.parentToil = parentToil;
        }

		public ComplexLordToil ParentToil => ParentToil;
    }
}