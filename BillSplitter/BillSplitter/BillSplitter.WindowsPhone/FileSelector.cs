using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace BillSplitter
{
	public class FileSelector
	{
		public void Initialise()
		{
			// Feels a bit wrong to call into CoreApplication here without an
			// abstraction but for demo purposes...

			// This event can fire either if the app keeps running after a
			// file selection or if the app is killed/re-launched after a
			// file selection.
			CoreApplication.GetCurrentView().Activated += OnApplicationActivated;
		}
		void OnApplicationActivated(CoreApplicationView sender,
		  IActivatedEventArgs args)
		{
			FileOpenPickerContinuationEventArgs continueArgs =
			  args as FileOpenPickerContinuationEventArgs;

			if (continueArgs != null)
			{
				// assumes we have one file here at least.
				if (continueArgs.Files != null && continueArgs.Files.Count > 0)
				{
					this.selectedFile = continueArgs.Files[0];
				}

				if (this.completionSource != null)
				{
					this.completionSource.SetResult(this.selectedFile);

					this.completionSource = null;
				}
			}
		}
		public async Task<StorageFile> DisplayPickerAsync(FileOpenPicker picker)
		{
			this.completionSource = new TaskCompletionSource<StorageFile>();

			picker.PickSingleFileAndContinue();

			StorageFile file = await this.completionSource.Task;

			return (file);
		}
		public StorageFile GetAndClearLastPickedFile()
		{
			StorageFile file = this.selectedFile;

			this.selectedFile = null;

			return (file);
		}
		TaskCompletionSource<StorageFile> completionSource;
		StorageFile selectedFile;
	}
}
