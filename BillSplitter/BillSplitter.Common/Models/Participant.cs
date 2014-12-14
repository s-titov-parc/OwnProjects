using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace BillSplitter.Models
{
	[DataContract]
	public class Participant : INotifyPropertyChanged
	{
		[DataMember]
		public string Id { get; set; }

		private string _FullName;

		[DataMember]
		public string FullName
		{
			get { return _FullName; }
			set
			{
				if (value != _FullName)
				{
					_FullName = value;
					RaisePropertyChanged("FullName");
				}
			}
		}

		public override string ToString()
		{
			return FullName;
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
