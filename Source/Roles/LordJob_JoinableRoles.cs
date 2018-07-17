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
	public enum PawnRoleLostCondition { LeftLord, InValid, NewRole };
        

    abstract public class LordJob_JoinableRoles : LordJob_VoluntarilyJoinable
    {
		protected ListDictionary roles;

		public LordPawnRole GetRole(string name) => roles[name] as LordPawnRole;
		public bool HasRole(string name) => roles.Contains(name);

		public void AddRole(LordPawnRole role)
		{
			role.lordJob = this;
            roles.Add(role.name, role);
		}

		virtual public void Notify_PawnLeftRole(LordPawnRole role, Pawn pawn, LordPawnRole newRole) { }

		virtual public void Notify_PawnJoinedRole(LordPawnRole role, Pawn pawn, LordPawnRole oldRole) { }

		virtual public void Notify_PawnWasReplacedInRole(LordPawnRole role, Pawn replacer, Pawn prevPawn
						, LordPawnRole replacerOldRole, LordPawnRole prevPawnNewRole)
		{ }

		public void CheckAndUpdateRoles()
		{
			var sortedRoles = roles.Values.OfType<LordPawnRole>()
										  .Cast<LordPawnRole>()
										  .OrderBy(role => role.order);

			HashSet<Pawn> pawnsWithLowerPriorityRoles = new HashSet<Pawn>(lord.ownedPawns);
            var allPawnsLost = new List<Tuple<Pawn, LordPawnRole, PawnRoleLostCondition>>();

			foreach(var role in sortedRoles) {
                var pawnsLost = new List<Tuple<Pawn, LordPawnRole, PawnRoleLostCondition>>();
				for(int i = role.currentPawns.Count - 1; i >= 0; i--) {
					Pawn pawn = role.currentPawns[i];
					if(pawn.GetLord() != lord) {
						pawnsLost.Add(Tuple.Create(pawn, role, PawnRoleLostCondition.LeftLord));
						role.currentPawns.RemoveAt(i);
					}
					else if(!role.pawnValidator(pawn)) {
						pawnsLost.Add(Tuple.Create(pawn, role, PawnRoleLostCondition.InValid));
						role.currentPawns.RemoveAt(i);
					}
					else if(!pawnsWithLowerPriorityRoles.Contains(pawn)) {
						pawnsLost.Add(Tuple.Create(pawn, role, PawnRoleLostCondition.NewRole));
						role.currentPawns.RemoveAt(i);
					}
					else
						pawnsWithLowerPriorityRoles.Remove(pawn);
				}

				if(pawnsLost.Count > 0 && role.isReassignable) {
					for(int count = 0; count < pawnsLost.Count; count++) {
						if(pawnsWithLowerPriorityRoles
								.Where(role.pawnValidator)
								.TryRandomElementByWeight(role.pawnAssignPriority, out Pawn pawn)) {



						}
						else
							break;
					}
				}
			}                                       
		}       
	}
}
