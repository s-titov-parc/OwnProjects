using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace BillSplitter.Models
{
	public class BillItem : INotifyPropertyChanged
    {
        public BillItem()
        {
            Participants = new ObservableCollection<Participant>();
        }

		private string _Name;
		public string Name
		{
			get { return _Name; }
			set
			{
				if (value != _Name)
				{
					_Name = value;
					RaisePropertyChanged("Name");
					RaisePropertyChanged("Caption");
				}
			}
		}

		private double _Price;
		public double Price
		{
			get { return _Price; }
			set
			{
				if (value != _Price)
				{
					_Price = value;
					RaisePropertyChanged("Price");
					RaisePropertyChanged("Caption");
				}
			}
		}

		public string Caption
		{
			get { return String.Format("{0}; {1}", Name, Price, Participants.Count); }
		}
		
        public ObservableCollection<Participant> Participants { get; set; }

		public override string ToString()
		{
			return String.Format("{0} -> {1}", Name, Price);
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
