using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BillSplitter.Models
{
	public class Participant : INotifyPropertyChanged
	{
		public string Id { get; set; }

		private string _FullName;
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
