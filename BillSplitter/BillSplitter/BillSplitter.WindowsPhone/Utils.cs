using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BillSplitter
{
	public class Utils
	{
		public static void Write(StorageFile file, string text)
		{
			int i = 3;
			do
			{
				try
				{
					var task = FileIO.WriteTextAsync(file, text).AsTask();
					task.Wait();
					return;				
				}
				catch (Exception e)
				{
					if (i==1) { throw e; }
				}
			}
			while(i-->0);
		}

		public static string Read(StorageFile file)
		{
			int i = 3;
			do
			{
				try
				{
					var task = FileIO.ReadTextAsync(file).AsTask();
					task.Wait();
					return task.Result;
				}
				catch (Exception e)
				{
					if (i == 1) { throw e; }
				}
			}
			while (i-- > 0);
			return null;
		}

		public static async Task Delete(StorageFile file)
		{
			int i = 3;
			do
			{
				try
				{
					await file.DeleteAsync();
					return;
				}
				catch (FileNotFoundException)
				{
					return;
				}
				catch (Exception e)
				{
					if (i == 1) { throw e; }
				}
				if (i > 1) await Task.Delay(TimeSpan.FromMilliseconds(100));
			}
			while (i-- > 0);
		}

		internal static async Task<string> ReadAndDelete(StorageFile cached)
		{
			int i = 3;
			do
			{
				try
				{
					var task = FileIO.ReadTextAsync(cached).AsTask();
					task.Wait();
					await Delete(cached);
					return task.Result;
				}
				catch (Exception e)
				{
					if (i == 1) { throw e; }
				}
			}
			while (i-- > 0);
			return null;
		}
	}
}
