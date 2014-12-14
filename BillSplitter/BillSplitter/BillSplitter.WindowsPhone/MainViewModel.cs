using BillSplitter.Models;
using BillSplitter.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BillSplitter
{
	public class MainViewModel
	{
		public MainViewModel()
		{
			BillNames = new ObservableCollection<string>();
			GroupNames = new ObservableCollection<string>();

			var folderTask = ApplicationData.Current.LocalFolder.CreateFolderAsync("Bills", CreationCollisionOption.OpenIfExists);
			folderTask.AsTask().Wait();
			var folder = folderTask.GetResults();
			var filesTask = folder.GetFilesAsync();
			filesTask.AsTask().Wait();
			var files = filesTask.GetResults();
			foreach (var file in files)
			{
				BillNames.Add(file.Name);
			}

			folderTask = ApplicationData.Current.LocalFolder.CreateFolderAsync("Groups", CreationCollisionOption.OpenIfExists);
			folderTask.AsTask().Wait();
			folder = folderTask.GetResults();
			filesTask = folder.GetFilesAsync();
			filesTask.AsTask().Wait();
			files = filesTask.GetResults();
			foreach (var file in files)
			{
				GroupNames.Add(file.Name);
			}
		}

		public ObservableCollection<string> BillNames { get; set; }

		public ObservableCollection<string> GroupNames { get; set; }

		internal async void RemoveBill(string name)
		{
			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Bills", CreationCollisionOption.OpenIfExists);
			var file = await folder.GetFileAsync(name);
			if (file != null)
			{
				await Utils.Delete(file);
			}
			BillNames.Remove(name);
		}

		internal async void RemoveGroup(string name)
		{
			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Groups", CreationCollisionOption.OpenIfExists);
			var file = await folder.GetFileAsync(name);
			if (file != null)
			{
				await Utils.Delete(file);
			}
			GroupNames.Remove(name);
		}


		private async Task<T> CreateNew<T>(string newName, StorageFolder folder) where T : class, new()
		{
			T bill = new T();
			var serializer = new DataContractJsonSerializer(typeof(T));

			var file = await folder.CreateFileAsync(newName);
			Utils.Write(file, bill.Serialize());
			return bill;
		}

		private int InvariantIndex(ObservableCollection<string> strings, string s)
		{
			for (int i = 0; i < strings.Count; i++)
			{
				if (string.Equals(strings[i],s,StringComparison.CurrentCultureIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		internal async void AddOrEditBill(string oldName, string newName)
		{
			if (!BillNames.Any(n => string.Equals(n,newName,StringComparison.CurrentCultureIgnoreCase)))
			{
				var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Bills", CreationCollisionOption.OpenIfExists);
				if (string.IsNullOrWhiteSpace(oldName))
				{
					await CreateNew<Bill>(newName, folder);
					BillNames.Add(newName);
				}
				else
				{
					var file = await folder.GetFileAsync(oldName);
					if (file == null)
					{
						await CreateNew<Bill>(newName, folder);
					}
					else
					{
						await file.RenameAsync(newName);
					}
					BillNames[InvariantIndex(BillNames,oldName)] = newName;
				}
			}
		}

		internal async void AddOrEditGroup(string oldName, string newName)
		{
			if (!GroupNames.Any(n => string.Equals(n,newName,StringComparison.CurrentCultureIgnoreCase)))
			{
				var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Groups", CreationCollisionOption.OpenIfExists);
				if (string.IsNullOrWhiteSpace(oldName))
				{
					await CreateNew<BillGroup>(newName, folder);
					GroupNames.Add(newName);
				}
				else
				{
					var file = await folder.GetFileAsync(oldName);
					if (file == null)
					{
						await CreateNew<BillGroup>(newName, folder);
					}
					else
					{
						await file.RenameAsync(newName);
					}
					GroupNames[InvariantIndex(GroupNames,oldName)] = newName;
				}
			}
		}
	}
}
