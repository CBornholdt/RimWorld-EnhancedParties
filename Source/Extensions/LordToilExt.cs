using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;


namespace EnhancedParty
{
    static public class LordToilExt
    {
        static public IEnumerable<LordToil> ThisAndEnclosingToils(this LordToil toil)
        {
            yield return toil;
            if(toil is EnhancedLordToil eToil) {
                while(eToil.ParentToil != null) 
                    yield return (eToil = eToil.ParentToil);
            }
        }
    }
}
