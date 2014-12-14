using BillSplitter.Common;
using BillSplitter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace BillSplitter
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class GroupPage : Page
	{
		private NavigationHelper navigationHelper;
		private ObservableDictionary defaultViewModel = new ObservableDictionary();

		public GroupPage()
		{
			this.InitializeComponent();

			Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

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
		/// Gets the view model for this <see cref="Page"/>.
		/// This can be changed to a strongly typed view model.
		/// </summary>
		public ObservableDictionary DefaultViewModel
		{
			get { return this.defaultViewModel; }
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
			string fName = e.NavigationParameter as string;
			Load(fName);
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

			string fName = e.Parameter as string;
			Load(fName);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			this.navigationHelper.OnNavigatedFrom(e);

			Save();
		}

		private static object _loadLock = new object();

		private void Load(string fName)
		{
			lock (_loadLock)
			{
				if (fName != null)
				{
					(this.DataContext as GroupViewModel).Group.Name = fName;
					(this.DataContext as GroupViewModel).Load();
				}
			}
		}

		private void Save()
		{
			(this.DataContext as GroupViewModel).Save();
		}

		#endregion

		void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
		{
			if (this.popupBill.IsOpen)
			{
				CloseEditBillPopup(false);
				e.Handled = true;
			}
			if (lvBills.SelectionMode == ListViewSelectionMode.Multiple)
			{
				lvBills.SelectionMode = ListViewSelectionMode.None;
				btnDelete.Visibility = Visibility.Collapsed;
				e.Handled = true;
			}
		}

		Bill editedBill = null;

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

		private void ShowEditBillPopup()
		{
			this.tbNewBill.Text = editedBill != null ? editedBill.Name : string.Empty;
			tbNewBill.SelectAll();
			lvBills.Visibility = Visibility.Collapsed;
			this.popupBill.IsOpen = true;
			pivot.IsLocked = true;
			RefreshAppButtonsState(true);
			this.tbNewBill.Focus(FocusState.Keyboard);
		}

		private void CloseEditBillPopup(bool saveChanges)
		{
			if (saveChanges && !string.IsNullOrWhiteSpace(tbNewBill.Text))
			{
				(this.DataContext as GroupViewModel).AddOrEditBill(editedBill, tbNewBill.Text);
			}
			editedBill = null;
			this.popupBill.IsOpen = false;
			pivot.IsLocked = false;
			lvBills.Visibility = Visibility.Visible;
			RefreshAppButtonsState(false);
		}

		public ListView GetCurrentListView()
		{
			ListView lvCurrent = null;
			switch (pivot.SelectedIndex)
			{
				case 0:
					lvCurrent = lvBills;
					break;
			}
			return lvCurrent;
		}

		private void btnNewBill_Click(object sender, RoutedEventArgs e)
		{
			ShowEditBillPopup();
		}

		private void btnSelect_Click(object sender, RoutedEventArgs e)
		{
			ListView lvCurrent = GetCurrentListView();
			if (lvCurrent != null)
			{
				bool nowNotInSelectMode = lvCurrent.SelectionMode == ListViewSelectionMode.None;
				lvCurrent.SelectionMode = (nowNotInSelectMode && lvCurrent.Items.Count > 0) ? ListViewSelectionMode.Multiple : ListViewSelectionMode.None;
				btnDelete.Visibility = (nowNotInSelectMode && lvCurrent.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			ListView lvCurrent = GetCurrentListView();
			List<object> removed = new List<object>(lvCurrent.SelectedItems);
			foreach (var item in removed)
			{
				switch (lvCurrent.Name)
				{
					case "lvBills":
						(this.DataContext as MainViewModel).RemoveBill(item as string);
						break;
				}
			}
			lvCurrent.SelectionMode = ListViewSelectionMode.None;
			btnDelete.Visibility = Visibility.Collapsed;
		}

		private void btnAccept_Click(object sender, RoutedEventArgs e)
		{
			if (popupBill.IsOpen)
			{
				CloseEditBillPopup(true);
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			if (popupBill.IsOpen)
			{
				CloseEditBillPopup(false);
			}
		}

		private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			lvBills.SelectionMode = ListViewSelectionMode.None;
			RefreshAppButtonsState(false);
		}

		private void MenuFlyoutItemRename_Click(object sender, RoutedEventArgs e)
		{
			var fElement = (e.OriginalSource as FrameworkElement);
			if (fElement != null)
			{
				var currentLv = GetCurrentListView();
				if (currentLv.Name == "lvBills")
				{
					editedBill = (fElement.DataContext as Bill);
					ShowEditBillPopup();
				}
			}
		}

		private void MenuFlyoutItemDelete_Click(object sender, RoutedEventArgs e)
		{
			var fElement = (e.OriginalSource as FrameworkElement);
			if (fElement != null)
			{
				var currentLv = GetCurrentListView();
				if (currentLv.Name == "lvBills")
				{
					(this.DataContext as GroupViewModel).RemoveBill(fElement.DataContext as string);
				}
			}
		}

		private void tbNewBill_KeyUp(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter)
			{
				CloseEditBillPopup(true);
				e.Handled = true;
			}
		}

		private void Bill_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var fElement = (e.OriginalSource as FrameworkElement);
			if (fElement != null)
			{
				Frame.Navigate(typeof(BillPage), fElement.DataContext);
			}
		}
	}
}
