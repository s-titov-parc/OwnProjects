using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BillSplitter.Models
{
    public class Bill
    {
        public Bill()
        {
            Participants = new List<Participant>();
            Items = new List<BillItem>();
            Payments = new List<Payment>();
            Transfers = new List<Transfer>();
        }
        public List<Participant> Participants { get; set; }

        public List<BillItem> Items { get; set; }

        public List<Payment> Payments { get; set; }

        public List<Transfer> Transfers { get; set; }

        #region methods
        public void Process()
        {
            foreach (var part in Participants)
            {
                Transfers.AddRange(Participants.Where(p => p != part).Select(p => new Transfer(part, p)));
            }
            Dictionary<Participant, double> debts = Participants.ToDictionary(p => p, p => 0.0);
            foreach (var item in Items.Where(i => i.Price > 0 && i.Participants.Any()))
            {
                double sum = item.Price / item.Participants.Count;
                foreach (var part in item.Participants)
                {
                    debts[part] += sum;
                }
            }
            foreach (var pay in Payments)
            {
                debts[pay.Participant] -= pay.Sum;
            }
            var debtsSorted = debts.OrderBy(d => d.Value).Select(kvp => new Payment(kvp.Key, -kvp.Value)).ToList();
            int from = debtsSorted.Count - 1;
            int to = 0;
            do
            {
                var debtTo = debtsSorted[to];
                var debtFrom = debtsSorted[from];
                var transferSum = Math.Min(debtTo.Sum, Math.Abs(debtFrom.Sum));
                Transfers.Single(t => t.From == debtFrom.Participant && t.To == debtTo.Participant).Sum += transferSum;
                debtTo.Sum -= transferSum;
                debtFrom.Sum += transferSum;
                if (debtTo.Sum == 0)
                {
                    to++;
                }
                if (debtFrom.Sum == 0)
                {
                    from--;
                }
            } while (to != from && to < debtsSorted.Count && from > 0);
        }
        #endregion
    }
}
