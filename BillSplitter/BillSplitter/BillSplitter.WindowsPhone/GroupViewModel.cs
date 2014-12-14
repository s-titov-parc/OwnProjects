using BillSplitter.Models;
using BillSplitter.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BillSplitter
{
	public class GroupViewModel : INotifyPropertyChanged
	{
		public GroupViewModel()
		{
			Group = new BillGroup();
		}

		private BillGroup _Group;
		public BillGroup Group
		{
			get { return _Group; }
			set
			{
				if (value != _Group)
				{
					_Group = value;
					OnPropertyChanged("Group");
				}
			}
		}

		internal async void AddOrEditBill(Bill bill, string newName)
		{

			var existedNew = Group.Bills.FirstOrDefault(b => string.Equals(b.Name, newName, StringComparison.CurrentCultureIgnoreCase));
			if (existedNew == null)
			{
				if (bill != null)
				{
					bill.Name = newName;
				}
				else
				{
					Group.Bills.Add(new Bill() { Name = newName });
				}
			}
		}

		internal async void RemoveBill(string name)
		{
			var existed = Group.Bills.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.CurrentCultureIgnoreCase));
			if (existed != null)
			{
				Group.Bills.Remove(existed);
			}
		}

		public async void Load()
		{
			var cacheFolder = ApplicationData.Current.LocalCacheFolder;
			var cachedFiles = await cacheFolder.GetFilesAsync();

			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Groups", CreationCollisionOption.OpenIfExists);
			string fName = Group.Name;
			var file = await folder.GetFileAsync(fName);
			var serialized = Utils.Read(file);
			Group = serialized.Deserialize<BillGroup>();
			Group.Name = fName;

			foreach (var cached in cachedFiles)
			{
				var existedBill = Group.Bills.FirstOrDefault(b => string.Equals(b.Name, cached.Name));
				if (existedBill!=null)
				{
					var fBillName = existedBill.Name;
					var serializedBill = await Utils.ReadAndDelete(cached);
					existedBill = serializedBill.Deserialize<Bill>();
					existedBill.Name = fBillName;
					break;
				}
			}
		}

		public async void Save()
		{
			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Groups", CreationCollisionOption.OpenIfExists);
			var file = await folder.GetFileAsync(Group.Name);
			Utils.Write(file, Group.Serialize());
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
}
