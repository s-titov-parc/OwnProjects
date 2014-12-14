using BillSplitter.Common;
using BillSplitter.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Windows.ApplicationModel.Contacts;
using Windows.Storage;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BillSplitter
{
	public class BillViewModel : INotifyPropertyChanged
	{
		public BillViewModel()
		{
			Bill = new Bill();
			Links = new ObservableCollection<PaymentAndItemViewModel>();
		}

		public async void Load()
		{
			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Bills", CreationCollisionOption.OpenIfExists);
			string fName = Bill.Name;
			var file = await folder.GetFileAsync(fName);
			var serialized = Utils.Read(file);
			Bill = serialized.Deserialize<Bill>();
		}

		public async void Save(bool grouped)
		{
			var folder = grouped ? ApplicationData.Current.LocalCacheFolder : await ApplicationData.Current.LocalFolder.CreateFolderAsync("Bills", CreationCollisionOption.OpenIfExists);
			var file = await folder.CreateFileAsync(Bill.Name, CreationCollisionOption.ReplaceExisting);
			Utils.Write(file, Bill.Serialize());
		}

		private Bill _bill;
		public Bill Bill
		{
			get { return _bill; }
			set
			{
				if (value != _bill)
				{
					_bill = value;
					OnPropertyChanged("Bill");
				}
			}
		}


		public ObservableCollection<PaymentAndItemViewModel> Links { get; set; }

		public Participant AddOrEditParticipant(string name, string id, double sum)
		{
			var participant = Bill.Participants.FirstOrDefault(p => p.Id == id);

			if (participant == null)
			{
				participant = new Participant() { Id = id };

				Bill.Participants.Add(participant);
			}

			participant.FullName = name;

			var payments = Bill.Payments;

			if (!payments.Any(p => p.Participant == participant))
			{
				payments.Add(new Payment(participant, sum));
			}
			else
			{
				var existedPayment = payments.Single(p => p.Participant == participant);
				existedPayment.Sum = sum;
			}

			return participant;
			//Links.Add(new PaymentAndItemViewModel()
			//{
			//	Participant = participant,
			//	Items = new List<BillItem>(),
			//	Sum = 0
			//});
		}

		public BillItem AddBillItem(string name, double price)
		{
			var item = new BillItem()
					{
						Name = name,
						Price = price
					};
			Bill.Items.Add(item);
			return item;
			//foreach (var link in Links)
			//{
			//	link.Items.Add(item);
			//}
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
	}

	public class PaymentAndItemViewModel
	{
		public Participant Participant { get; set; }
		public List<BillItem> Items { get; set; }
		public double Sum { get; set; }
	}
}
