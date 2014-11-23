using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;

namespace BillSplitter.Models
{
	public class Bill : INotifyPropertyChanged
	{
		private ObservableCollection<Participant> _participants;

		public Bill()
		{
			Participants = new ObservableCollection<Participant>();
			Items = new ObservableCollection<BillItem>();
			Payments = new ObservableCollection<Payment>();
			Changes = new ObservableCollection<Payment>();
			Transfers = new ObservableCollection<Transfer>();
		}

		public ObservableCollection<Participant> Participants
		{
			get { return _participants; }
			set
			{
				_participants = value;
				OnPropertyChanged();
			}
		}

		private double _BillSum;
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

		public ObservableCollection<BillItem> Items { get; set; }

		public ObservableCollection<Payment> Payments { get; set; }

		public ObservableCollection<Payment> Changes { get; set; }

		public ObservableCollection<Transfer> Transfers { get; set; }

		private static double epsilon = 0.000001;
		#region methods
		public void Process()
		{
			BillSum = Items.Sum(i => i.Price);
			PaySum = Payments.Sum(i => i.Sum);
			var unlinkedChange = Change;
			Changes.Clear();
			Transfers.Clear();
			foreach (var item in Participants.SelectMany(part => 
										Participants.Where(p => p != part).Select(p => new Transfer(part, p))))
			{
				Transfers.Add(item);
			}

			Dictionary<Participant, double> debts = Participants.ToDictionary(p => p, p => 0.0);
			foreach (var pay in Payments)
			{
				debts[pay.Participant] += pay.Sum;
			}

			foreach (var item in Items.Where(i => i.Price > 0 && i.Participants.Any()))
			{
				double sum = item.Price / item.Participants.Count;
				foreach (var part in item.Participants)
				{
					debts[part] -= sum;
				}
			}

			var debtsSorted = debts.OrderByDescending(d => d.Value).Select(kvp => new Payment(kvp.Key, kvp.Value)).ToList();
			int from = debtsSorted.Count - 1;
			int to = 0;
			while (to != from && to < debtsSorted.Count && from > 0)
			{
				var debtTo = debtsSorted[to];
				var debtFrom = debtsSorted[from];
				if (debtTo.Sum >= 0 && unlinkedChange > epsilon)
				{
					unlinkedChange = LinkChange(debtTo, unlinkedChange);
				}
				if (debtFrom.Sum >= epsilon && unlinkedChange > epsilon)
				{
					unlinkedChange = LinkChange(debtFrom, unlinkedChange);
				}
				if (debtTo.Sum >= epsilon && debtFrom.Sum < 0)
				{
					var transferSum = Math.Min(debtTo.Sum, Math.Abs(debtFrom.Sum));
					Transfers.Single(t => t.From == debtFrom.Participant && t.To == debtTo.Participant).Sum += transferSum;
					debtTo.Sum -= transferSum;
					debtFrom.Sum += transferSum;
				}
				if (debtTo.Sum < epsilon)
				{
					to++;
				}
				if (debtFrom.Sum < epsilon)
				{
					from--;
				}
			};

		}

		private double LinkChange(Payment debtTo, double unlinkedChange)
		{
			var existedChange = Changes.SingleOrDefault(c => c.Participant == debtTo.Participant);
			if (existedChange == null)
			{
				existedChange = new Payment(debtTo.Participant, 0);
				Changes.Add(existedChange);
			}
			var transferSum = Math.Min(debtTo.Sum, unlinkedChange);
			existedChange.Sum += transferSum;
			debtTo.Sum -= transferSum;
			return unlinkedChange - transferSum;
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
