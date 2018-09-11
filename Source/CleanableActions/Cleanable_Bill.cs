using System;
using RimWorld;
using Verse;

namespace EnhancedParty
{
    public class Cleanable_Bill : ICleanableAction
    {
        public Bill bill;
        public bool destroyUft = true;
        string id;
    
        public Cleanable_Bill(Bill bill, bool destroyUft = true)
        {
            this.bill = bill;
            this.destroyUft = destroyUft;
            this.id = bill.GetUniqueLoadID() + "_CA";
        }

        public bool CleanupStillNeeded()
        {
            return !this.bill.DeletedOrDereferenced;
        }

        public void ExposeData()
        {
            Scribe_References.Look<Bill>(ref this.bill, "Bill");
            Scribe_Values.Look<bool>(ref this.destroyUft, "DestroyUft", true);
            Scribe_Values.Look<string>(ref this.id, "ID", "Blank");
        }

        public string GetUniqueLoadID() => id;		

        public void PerformCleanup()
        {
            this.bill.billStack.Delete(bill);
            if(bill is Bill_ProductionWithUft uftBill) {
                UnfinishedThing unfinished = uftBill.BoundUft;
                if(!unfinished.DestroyedOrNull() && this.destroyUft)
                    unfinished.Destroy(DestroyMode.Cancel);
            }
        }

        public bool ReferencesBroken() => bill == null;	
    }
}
