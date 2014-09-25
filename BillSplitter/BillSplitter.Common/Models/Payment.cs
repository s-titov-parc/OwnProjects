using System;
using System.Collections.Generic;
using System.Text;

namespace BillSplitter.Models
{
    public class Payment
    {
        private double p;

        public Payment(Models.Participant participant, double sum)
        {
            this.Participant = participant;
            this.Sum = sum;
        }
        public Participant Participant { get; set; }

        public double Sum { get; set; }
    }
}
