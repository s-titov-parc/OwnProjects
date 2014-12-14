using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace BillSplitter.Models
{
	[DataContract]
	public class Payment : INotifyPropertyChanged
    {
        public Payment(Models.Participant participant, double sum)
        {
            this.Participant = participant;
            this.Sum = sum;
        }

		[DataMember]
        public Participant Participant { get; set; }

		private double _Sum;

		[DataMember]
		public double Sum
		{
			get { return _Sum; }
			set
			{
				if (value != _Sum)
				{
					_Sum = value;
					RaisePropertyChanged("Sum");
				}
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		
		private void RaisePropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
