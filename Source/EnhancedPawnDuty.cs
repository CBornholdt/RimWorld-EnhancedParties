﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    public class EnhancedPawnDuty : PawnDuty
    {
		public ThingDef dutyThingDef;
		public int thingCount;
        public RecipeDef dutyRecipe;
		public bool useTablesIfPossible = true;
		public bool useOnlyTables = false;
		public bool stayInRoom = true;
		public string taskName;
    
        public EnhancedPawnDuty(DutyDef duty, LocalTargetInfo focus, float radius = -1) : base(duty, focus, radius)
        {
        }

		public new void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<RecipeDef>(ref this.dutyRecipe, "DutyRecipe");
			Scribe_Values.Look<bool>(ref this.useTablesIfPossible, "UseTablesIfPossible");
			Scribe_Values.Look<bool>(ref this.useOnlyTables, "UseOnlyTables");
			Scribe_Values.Look<bool>(ref this.stayInRoom, "StayInRoom");
			Scribe_Values.Look<string>(ref this.taskName, "TaskName");
			Scribe_Defs.Look<ThingDef>(ref this.dutyThingDef, "DutyThingDef");
			Scribe_Values.Look<int>(ref this.thingCount, "ThingCount", 1);
		}	
    }
}