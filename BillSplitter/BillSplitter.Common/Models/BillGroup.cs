using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BillSplitter.Models
{
	[DataContract]
	public class BillGroup : INotifyPropertyChanged
	{
		public BillGroup()
		{
			Changes = new ObservableCollection<Payment>();
			Transfers = new ObservableCollection<Transfer>();
			Bills = new ObservableCollection<Bill>();
		}

		[DataMember]
		public ObservableCollection<Bill> Bills { get; set; }

		private string _Name;
		[DataMember]
		public string Name
		{
			get { return _Name; }
			set
			{
				if (value != _Name)
				{
					_Name = value;
					OnPropertyChanged("Name");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public ObservableCollection<Transfer> Transfers { get; set; }
		public ObservableCollection<Payment> Changes { get; set; }

		private double _BillSum;

		[DataMember]
		public double BillSum
		{
			get { return _BillSum; }
			set
			{
				if (value != _BillSum)
				{
					_BillSum = value;
					OnPropertyChanged("BillSum");
					OnPropertyChanged("Change");
				}
			}
		}

		private double _PaySum;

		[DataMember]
		public double PaySum
		{
			get { return _PaySum; }
			set
			{
				if (value != _PaySum)
				{
					_PaySum = value;
					OnPropertyChanged("PaySum");
					OnPropertyChanged("Change");
				}
			}
		}

		public double Change
		{
			get
			{
				return PaySum - BillSum;
			}
		}

		public void Process()
		{
			var tempBill = new Bill();

			List<Participant> participants = new List<Participant>();
			foreach (var bill in Bills)
			{
				foreach (var part in bill.Participants)
				{
					if (!tempBill.Participants.Any(p => p.Id == part.Id))
					{
						tempBill.Participants.Add(part);
					}
				}
				foreach (var pay in bill.Payments)
				{
					var existedPart = tempBill.Participants.First(p => p.Id == pay.Participant.Id);
					tempBill.Payments.Add(new Payment(existedPart, pay.Sum));
				}
				foreach (var item in bill.Items)
				{
					var newItem = new BillItem();
					newItem.Price = item.Price;
					foreach (var part in item.Participants.Select(ip => bill.Participants.First(p => p.Id == ip.Id)))
					{
						newItem.Participants.Add(part);
					}
				}
			}

			tempBill.Process();
			Transfers = tempBill.Transfers;
			Changes = tempBill.Changes;

			PaySum = tempBill.PaySum;
			BillSum = tempBill.BillSum;

		}
	}
}
