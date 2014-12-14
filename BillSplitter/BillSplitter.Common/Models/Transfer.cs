using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BillSplitter.Models
{
	[DataContract]
    public class Transfer
    {
        public Transfer(Participant from, Participant to)
        {
            this.From = from;
            this.To = to;
            this.Sum = 0.0;
        }

		[DataMember]
        public Participant From { get; set; }
		
		[DataMember]
        public Participant To { get; set; }

		[DataMember]
        public double Sum { get; set; }
    }
}
