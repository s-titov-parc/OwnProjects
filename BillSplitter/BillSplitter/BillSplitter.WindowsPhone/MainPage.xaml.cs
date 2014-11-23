using System.ComponentModel;
using Windows.System;
using BillSplitter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BillSplitter
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();

			this.NavigationCacheMode = NavigationCacheMode.Required;

			Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

		}

		Participant editedParticipant = null;
		BillItem editedItem = null;

		void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
		{
			if (this.popupParticipants.IsOpen)
			{
				CloseEditParticipantPopup(false);
				e.Handled = true;
			}
			if (this.popupItems.IsOpen)
			{
				CloseEditItemPopup(false);
				e.Handled = true;
			}
			if (lvParticipants.SelectionMode == ListViewSelectionMode.Multiple)
			{
				lvParticipants.SelectionMode = ListViewSelectionMode.None;
				btnDelete.Visibility = Visibility.Collapsed;
				e.Handled = true;
			}
			if (lvItems.SelectionMode == ListViewSelectionMode.Multiple)
			{
				lvItems.SelectionMode = ListViewSelectionMode.None;
				btnDelete.Visibility = Visibility.Collapsed;
				e.Handled = true;
			}
		}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.
		/// This parameter is typically used to configure the page.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			// TODO: Prepare page for display here.

			// TODO: If your application contains multiple pages, ensure that you are
			// handling the hardware Back button by registering for the
			// Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
			// If you are using the NavigationHelper provided by some templates,
			// this event is handled for you.
		}

		private void ShowEditParticipantPopup()
		{
			this.tbNewParticipant.Text = editedParticipant != null ? editedParticipant.FullName : string.Empty;
			tbNewParticipant.SelectAll();
			var payments = (this.DataContext as BillViewModel).Bill.Payments;
			Payment  existedPayment = null;
				if (editedParticipant != null && payments.Any(p => p.Participant == editedParticipant))
				{
					existedPayment = payments.Single(p => p.Participant == editedParticipant);
				}
			this.tbParticipantPay.Text =  existedPayment!=null ? existedPayment.Sum.ToString() : string.Empty;
			lvParticipants.Visibility = Visibility.Collapsed;
			this.popupParticipants.IsOpen = true;
			pivot.IsLocked = true;
			this.tbNewParticipant.Focus(FocusState.Keyboard);
		}

		private void ShowEditItemPopup()
		{
			this.tbNewItemName.Text = editedItem != null ? editedItem.Name : string.Empty;
			tbNewItemName.SelectAll();
			this.tbNewItemCost.Text = editedItem != null ? editedItem.Price.ToString() : string.Empty;
			lvItems.Visibility = Visibility.Collapsed;
			lvParticipantsInItem.SelectedItems.Clear();
			foreach (var part in editedItem != null ? editedItem.Participants : (this.DataContext as BillViewModel).Bill.Participants)
			{
				lvParticipantsInItem.SelectedItems.Add(part);
			}
			this.popupItems.IsOpen = true;
			pivot.IsLocked = true;
			this.tbNewItemName.Focus(FocusState.Keyboard);
		}

		private void CloseEditParticipantPopup(bool saveChanges)
		{
			if (saveChanges && !string.IsNullOrWhiteSpace(tbNewParticipant.Text))
			{
				double price = 0;
				double.TryParse(tbParticipantPay.Text, out price);
				editedParticipant = (this.DataContext as BillViewModel).AddOrEditParticipant(tbNewParticipant.Text, tbNewParticipant.Text, price);
			}
			editedParticipant = null;
			this.popupParticipants.IsOpen = false;
			pivot.IsLocked = false;
			lvParticipants.Visibility = Visibility.Visible;
			(this.DataContext as BillViewModel).Bill.Process();
		}

		private void CloseEditItemPopup(bool saveChanges)
		{
			if (saveChanges && !string.IsNullOrWhiteSpace(tbNewItemName.Text))
			{
				double price = 0;
				double.TryParse(tbNewItemCost.Text, out price);
				if (editedItem == null)
				{
					editedItem = (this.DataContext as BillViewModel).AddBillItem(tbNewItemName.Text, price);
				}
				else
				{
					editedItem.Name = tbNewItemName.Text;
					editedItem.Price = price;
				}
				editedItem.Participants.Clear();
				foreach (var part in lvParticipantsInItem.SelectedItems)
				{
					editedItem.Participants.Add(part as Participant);
				}
			}
			editedItem = null;
			this.popupItems.IsOpen = false;
			pivot.IsLocked = false;
			lvItems.Visibility = Visibility.Visible;
			(this.DataContext as BillViewModel).Bill.Process();
		}

		private void btnAdd_Click(object sender, RoutedEventArgs e)
		{
			editedParticipant = null;
			editedItem = null;
			switch (pivot.SelectedIndex)
			{
				case 0:
					ShowEditParticipantPopup();
					break;
				case 1:
					ShowEditItemPopup();
					break;
			}
		}

		private void btnAddParticipant_Click(object sender, RoutedEventArgs e)
		{
			CloseEditParticipantPopup(true);
		}

		private void btnAddItem_Click(object sender, RoutedEventArgs e)
		{
			CloseEditItemPopup(true);
		}

		private void tbNewParticipant_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter)
			{
				tbParticipantPay.SelectAll();
				tbParticipantPay.Focus(FocusState.Keyboard);
				e.Handled = true;
			}
		}

		private void tbParticipantPay_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter)
			{
				CloseEditParticipantPopup(true);
				e.Handled = true;
			}

		}

		private void tbNewItemName_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter)
			{
				tbNewItemCost.SelectAll();
				tbNewItemCost.Focus(FocusState.Keyboard);
				e.Handled = true;
			}

		}

		private void tbNewItemCost_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter)
			{
				lvParticipantsInItem.Focus(FocusState.Pointer);
				e.Handled = true;
			}

		}

		private void MenuFlyoutItemRename_Click(object sender, RoutedEventArgs e)
		{
			var fElement = (e.OriginalSource as FrameworkElement);
			if (fElement != null)
			{
				if (fElement.DataContext is Payment)
				{
					editedParticipant = (fElement.DataContext as Payment).Participant;
					ShowEditParticipantPopup();
				}
				if (fElement.DataContext is BillItem)
				{
					editedItem = fElement.DataContext as BillItem;
					ShowEditItemPopup();
				}
			}
		}

		private void MenuFlyoutItemDelete_Click(object sender, RoutedEventArgs e)
		{
			var fElement = (e.OriginalSource as FrameworkElement);
			if (fElement != null)
			{
				if (fElement.DataContext is Participant)
				{
					(this.DataContext as BillViewModel).Bill.Participants.Remove(fElement.DataContext as Participant);
				}
				if (fElement.DataContext is BillItem)
				{
					(this.DataContext as BillViewModel).Bill.Items.Remove(fElement.DataContext as BillItem);
				}
			}
		}

		public ListView GetCurrentListView()
		{
			ListView lvCurrent = null;
			switch (pivot.SelectedIndex)
			{
				case 0:
					lvCurrent = lvParticipants;
					break;
				case 1:
					lvCurrent = lvItems;
					break;
			}
			return lvCurrent;
		}

		private async void btnSelect_Click(object sender, RoutedEventArgs e)
		{
			ListView lvCurrent = GetCurrentListView();
			bool nowNotInSelectMode = lvCurrent.SelectionMode == ListViewSelectionMode.None;
			lvCurrent.SelectionMode = (nowNotInSelectMode && lvCurrent.Items.Count > 0) ? ListViewSelectionMode.Multiple : ListViewSelectionMode.None;
			btnDelete.Visibility = (nowNotInSelectMode && lvCurrent.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			ListView lvCurrent = GetCurrentListView();
			List<object> removed = new List<object>(lvCurrent.SelectedItems);
			foreach (var item in removed)
			{
				switch (lvCurrent.Name)
				{
					case "lvParticipants":
						(this.DataContext as BillViewModel).Bill.Participants.Remove(item as Participant);
						break;
					case "lvItems":
						(this.DataContext as BillViewModel).Bill.Items.Remove(item as BillItem);
						break;
				}
			}
			lvCurrent.SelectionMode = ListViewSelectionMode.None;
			btnDelete.Visibility = Visibility.Collapsed;
		}

		private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			lvParticipants.SelectionMode = ListViewSelectionMode.None;
			lvItems.SelectionMode = ListViewSelectionMode.None;
			btnDelete.Visibility = Visibility.Collapsed;
			btnContacts.Visibility = pivot.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private async void btnContacts_Click(object sender, RoutedEventArgs e)
		{
			var contactPicker = new Windows.ApplicationModel.Contacts.ContactPicker();
			contactPicker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.Address);
			var contacts = await contactPicker.PickContactsAsync();
			//contacts.Wait();
			foreach (var contact in contacts)
			{
				editedParticipant = (this.DataContext as BillViewModel).AddOrEditParticipant(contact.DisplayName, contact.DisplayName, 0);
			}
		}

	}
}
