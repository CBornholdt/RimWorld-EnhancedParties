using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

using Harmony;

namespace EnhancedParty
{
    static public class LordPawnRole_Organizer
    {
        static public readonly string Name = "Organizer";

        static public bool IsOrganizer(EnhancedLordJob job, Pawn pawn)
        {
            var dJob = Traverse.Create(job);
            return (dJob.Property("Organizer")?.GetValue<Pawn>()
                        ?? dJob.Field("organizer")?.GetValue<Pawn>()) == pawn;
        }

        static public LordPawnRole Create(EnhancedLordJob lordJob, string name = "Organizer")
        {
            var role = new LordPawnRole(name, lordJob);
            role.pawnValidator = (Pawn pawn) => IsOrganizer(role.lordJob, pawn);
            return role;
        }
    }
}
