using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace EnhancedParty
{
    static public class EnhancedLordJobExt
    {
        static public IEnumerable<Bill> CurrentCleanableBills(this EnhancedLordJob job) =>
            job.cleanupActions.OfType<CleanableBill>().Cast<CleanableBill>()
                              .Where(cleanableBill => cleanableBill.CleanupStillNeeded())
                              .Select(cleanableBill => cleanableBill.bill);
    }
}
