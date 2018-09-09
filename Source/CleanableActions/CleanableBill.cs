using System;
using RimWorld;
using Verse;

namespace EnhancedParty
{
    public class CleanableBill : ICleanableAction
    {
		public Bill bill;
		public bool destroyUft = true;
    
        public CleanableBill(Bill bill, bool destroyUft = true)
        {
			this.bill = bill;
			this.destroyUft = destroyUft;
        }

		public bool CleanupStillNeeded()
		{
			return !this.bill.DeletedOrDereferenced;
		}

		public void ExposeData()
		{
			Scribe_References.Look<Bill>(ref this.bill, "Bill");
			Scribe_Values.Look<bool>(ref this.destroyUft, "DestroyUft", true);
		}

		public string GetUniqueLoadID()
		{
			return this.bill.GetUniqueLoadID() + "_CA";
		}

		public void PerformCleanup()
		{
			this.bill.billStack.Delete(bill);
			if(bill is Bill_ProductionWithUft uftBill) {
				UnfinishedThing unfinished = uftBill.BoundUft;
				if(!unfinished.DestroyedOrNull() && this.destroyUft)
					unfinished.Destroy(DestroyMode.Cancel);
			}
		}
	}
}
