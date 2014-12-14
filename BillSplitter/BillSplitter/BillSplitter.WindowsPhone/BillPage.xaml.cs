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
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using System.Net;
using System.Text;
using Windows.Storage.Streams;
using System.Net.Http;
using Windows.Web.Http;
using System.Globalization;
using BillSplitter.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BillSplitter
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class BillPage : Page
	{
		public BillPage()
		{
			this.InitializeComponent();

			this.NavigationCacheMode = NavigationCacheMode.Required;

			Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

			fileSelector = new FileSelector();
			fileSelector.Initialise();

			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
		}

		/// <summary>
		/// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
		/// </summary>
		public NavigationHelper NavigationHelper
		{
			get { return this.navigationHelper; }
		}

		/// <summary>
		/// Populates the page with content passed during navigation.  Any saved state is also
		/// provided when recreating a page from a prior session.
		/// </summary>
		/// <param name="sender">
		/// The source of the event; typically <see cref="NavigationHelper"/>
		/// </param>
		/// <param name="e">Event data that provides both the navigation parameter passed to
		/// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
		/// a dictionary of state preserved by this page during an earlier
		/// session.  The state will be null the first time a page is visited.</param>
		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			Load(e.NavigationParameter);
		}

		/// <summary>
		/// Preserves state associated with this page in case the application is suspended or the
		/// page is discarded from the navigation cache.  Values must conform to the serialization
		/// requirements of <see cref="SuspensionManager.SessionState"/>.
		/// </summary>
		/// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
		/// <param name="e">Event data that provides an empty dictionary to be populated with
		/// serializable state.</param>
		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
			Save();
		}

		bool grouped;

		#region NavigationHelper registration

		/// <summary>
		/// The methods provided in this section are simply used to allow
		/// NavigationHelper to respond to the page's navigation methods.
		/// <para>
		/// Page specific logic should be placed in event handlers for the  
		/// <see cref="NavigationHelper.LoadState"/>
		/// and <see cref="NavigationHelper.SaveState"/>.
		/// The navigation parameter is available in the LoadState method 
		/// in addition to page state preserved during an earlier session.
		/// </para>
		/// </summary>
		/// <param name="e">Provides data for navigation methods and event
		/// handlers that cannot cancel the navigation request.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedTo(e);

			Load(e.Parameter);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);
			Save();
		}

		public void Load(object parameter)
		{
			Bill bill = parameter as Bill;

			if (bill != null)
			{
				(this.DataContext as BillViewModel).Bill = bill;
				grouped = true;
			}
			else
			{
				string fName = parameter as string;
				(this.DataContext as BillViewModel).Bill.Name = fName;
				(this.DataContext as BillViewModel).Load();
				grouped = false;
			}
		}

		public void Save()
		{
				(this.DataContext as BillViewModel).Save(grouped);
		}

		#endregion

		FileSelector fileSelector;
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


		private void ShowEditParticipantPopup()
		{
			this.tbNewParticipant.Text = editedParticipant != null ? editedParticipant.FullName : string.Empty;
			tbNewParticipant.SelectAll();
			var payments = (this.DataContext as BillViewModel).Bill.Payments;
			Payment existedPayment = null;
			if (editedParticipant != null && payments.Any(p => p.Participant == editedParticipant))
			{
				existedPayment = payments.Single(p => p.Participant == editedParticipant);
			}
			this.tbParticipantPay.Text = existedPayment != null ? existedPayment.Sum.ToString() : string.Empty;
			lvParticipants.Visibility = Visibility.Collapsed;
			this.popupParticipants.IsOpen = true;
			pivot.IsLocked = true;
			RefreshAppButtonsState(true);
			this.tbNewParticipant.Focus(FocusState.Keyboard);
		}

		private void ShowUploadPopup()
		{
			//lvItems.Visibility = Visibility.Collapsed;
			//this.popupUpload.IsOpen = true;
			//pivot.IsLocked = true;
			loadingBar.IsEnabled = true;
			loadingBar.Visibility = Visibility.Visible;
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
			RefreshAppButtonsState(true);
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
			RefreshAppButtonsState(false);
			if (saveChanges)
			{
				(this.DataContext as BillViewModel).Bill.Process();
			}
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
			RefreshAppButtonsState(false);
			if (saveChanges)
			{
				(this.DataContext as BillViewModel).Bill.Process();
			}
		}

		private void CloseUploadPopup()
		{
			(this.DataContext as BillViewModel).Bill.Process();
			//this.popupUpload.IsOpen = false;
			//pivot.IsLocked = false;
			//lvItems.Visibility = Visibility.Visible;
			loadingBar.IsEnabled = false;
			loadingBar.Visibility = Visibility.Collapsed;
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

		private void RefreshAppButtonsState(bool forPopup)
		{
			btnOk.Visibility = forPopup ? Visibility.Visible : Visibility.Collapsed;
			btnCancel.Visibility = forPopup ? Visibility.Visible : Visibility.Collapsed;
			foreach (var item in cmdBar.PrimaryCommands.OfType<AppBarButton>())
			{
				if (item.Tag != null && item.Tag is string)
				{
					var strTag = item.Tag as string;
					item.Visibility = (!forPopup && strTag.Contains((pivot.SelectedItem as PivotItem).Name)) ? Visibility.Visible : Visibility.Collapsed;
				}
			}

		}

		private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			lvParticipants.SelectionMode = ListViewSelectionMode.None;
			lvItems.SelectionMode = ListViewSelectionMode.None;
			//btnDelete.Visibility = Visibility.Collapsed;
			//btnContacts.Visibility = pivot.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
			RefreshAppButtonsState(false);
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

		async Task<Stream> MakeImage(StorageFile file)
		{
			BitmapImage bitmapImage = null;

			if (file != null)
			{
				return await file.OpenStreamForReadAsync();
			}
			return null;
		}


		private Stream lastImage;
		private async void btnPhoto_Click(object sender, RoutedEventArgs e)
		{
			FileOpenPicker picker = new FileOpenPicker();
			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".png");
			StorageFile file = await this.fileSelector.DisplayPickerAsync(picker);

			// this code may never run on Windows Phone 8.1 because we could get  
			// suspended and terminated as part of the regular file selection  
			// process.  
			if (file != null)
			{
				lastImage = await this.MakeImage(file);
			}

			if (lastImage != null)
			{
				ShowUploadPopup();
				var response = await PostAsync();
				var items = response.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var item in items)
				{
					(this.DataContext as BillViewModel).AddBillItem(item, 0);
				}
				CloseUploadPopup();
			}
		}

		public async Task<string> PostAsync()
		{
			var httpClient = new Windows.Web.Http.HttpClient();

			HttpMultipartFormDataContent form = new HttpMultipartFormDataContent();
			form.Add(new HttpStringContent(this.langCode), "language");
			var fileContent = new HttpStreamContent(lastImage.AsInputStream());
			fileContent.Headers.Add("Content-Type", "image/" + System.IO.Path.GetExtension(this.photoName).Replace(".", ""));
			form.Add(fileContent, "image", photoName);
			form.Add(new HttpStringContent(this.apiKey), "apikey");
			var response = await httpClient.PostAsync(new Uri("http://api.ocrapiservice.com/1.0/rest/ocr"), form);

			response.EnsureSuccessStatusCode();
			var content = await response.Content.ReadAsStringAsync();
			return content;
		}



		private string apiKey = "Bz2ndsDqmS";
		private string langCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;//  "rus";
		private string photoName = "1.jpg";
		private NavigationHelper navigationHelper;


		private void btnAccept_Click(object sender, RoutedEventArgs e)
		{
			if (popupParticipants.IsOpen)
			{
				CloseEditParticipantPopup(true);
			}
			if (popupItems.IsOpen)
			{
				CloseEditItemPopup(true);
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			if (popupParticipants.IsOpen)
			{
				CloseEditParticipantPopup(false);
			}
			if (popupItems.IsOpen)
			{
				CloseEditItemPopup(false);
			}
		}
	}
}
