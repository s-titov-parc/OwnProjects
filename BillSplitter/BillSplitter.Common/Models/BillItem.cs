using System;
using System.Collections.Generic;
using System.Text;

namespace BillSplitter.Models
{
    public class BillItem
    {
        public BillItem()
        {
            Participants = new List<Participant>();
        }

        public string Name { get; set; }

        public double Price { get; set; }

        public List<Participant> Participants { get; set; }
    }
}
