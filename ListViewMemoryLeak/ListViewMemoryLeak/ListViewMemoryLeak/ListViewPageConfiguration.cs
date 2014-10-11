using System;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	public class ListViewPageConfiguration
	{
		public ListViewPageMode Mode { get; set; }
		public bool CancelPendingRequestsOnDisappear { get; set; }
		public bool LoadOnAppearAndClearOnDisappear { get; set; }
		public int Repetitions { get; set; }
		public int RowHeight { get; set; }
		public bool MuteImages { get; set; }

		public Xamarin.Forms.ImageSource NewImageSource()
		{
			switch (Mode)
			{
				case ListViewPageMode.ResourceImages:
					return new FileImageSource
					{
						File = Images.ResourceImage50Kb
					};
				case ListViewPageMode.WebImages:
					return new UriImageSource
					{
						Uri = new Uri(Images.LocalWebImage143Kb),
						CachingEnabled = true,
					};
			}
			throw new NotImplementedException();
		}
	}
}