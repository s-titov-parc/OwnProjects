using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BillSplitter.Models
{
    public class Transfer
    {
        private Participant part;
        private Participant p;

        public Transfer(Participant from, Participant to)
        {
            this.From = from;
            this.To = to;
            this.Sum = 0.0;
        }
        public Participant From { get; set; }

        public Participant To { get; set; }

        public double Sum { get; set; }
    }
}
