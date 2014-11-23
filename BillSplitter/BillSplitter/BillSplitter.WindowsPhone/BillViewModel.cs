using BillSplitter.Common;
using BillSplitter.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Windows.ApplicationModel.Contacts;

namespace BillSplitter
{
	public class BillViewModel
	{
		public BillViewModel()
		{
			Bill = new Bill();
			Links = new ObservableCollection<PaymentAndItemViewModel>();
		}
		public Bill Bill { get; set; }

		public ObservableCollection<PaymentAndItemViewModel> Links { get; set; }

		private ICommand _addObjectCommand;
		public ICommand AddObjectCommand
		{
			get
			{
				return _addObjectCommand
					?? (_addObjectCommand = new ActionCommand((obj) =>
					{
						if (obj is string && ((string)obj).EndsWith("Participants"))
						{
							var contactPicker = new ContactPicker();
							var contactsTask = contactPicker.PickContactsAsync().AsTask();

							contactsTask.Wait();

							Bill.Participants.Clear();

							foreach (Contact contact in contactsTask.Result)
							{
								Bill.Participants.Add(new Participant() { FullName = contact.DisplayName, Id = contact.Id });
							}

						}
					}));
			}
		}

		public Participant AddOrEditParticipant(string name, string id, double sum)
		{
			var participant = Bill.Participants.FirstOrDefault(p => p.Id == id);

			if (participant == null)
			{
				participant = new Participant()	{ Id = id };

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
	}

	public class PaymentAndItemViewModel
	{
		public Participant Participant { get; set; }
		public List<BillItem> Items { get; set; }
		public double Sum { get; set; }
	}
}
