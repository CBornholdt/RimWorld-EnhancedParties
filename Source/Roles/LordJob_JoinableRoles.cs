using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EnhancedParty
{
    abstract public class LordJob_JoinableRoles : LordJob_VoluntarilyJoinable
    {
		protected ListDictionary roles;

		public LordPawnRole GetRole(string name) => roles[name] as LordPawnRole;
		public bool HasRole(string name) => roles.Contains(name);

		public void AddRole(LordPawnRole role) => roles.Add(role.name, role);
        
        
	}
}
