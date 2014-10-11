using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	public class MutableImage : Image
	{
		public Xamarin.Forms.ImageSource MutedSource { get; private set; }
		public bool Muted { get; private set; }

		public Action ClearMemory { get; set; }

		public async Task<bool> MuteAsync()
		{
			if (Muted) return true;
			var cancelled = false;
			if (Source != null)
			{
				//					cancelled = await Source.Cancel();
				//var r = new ManualResetEvent(false);
				//Source.Cancel()
				//	.ContinueWith(task =>
				//	{
				//		r.Set();
				//	});
				//r.WaitOne();
			}
			MutedSource = Source;
			//		if (ClearMemory != null) ClearMemory();
			//			(Source as UriImageSource).Uri = null;
			Source = null;
			Muted = true;
			return cancelled;
		}

		public static FileImageSource Tiny = new FileImageSource
		{
			File = "Tiny.bmp"
		};

		public void Unmute()
		{
			if (!Muted) return;
			Source = MutedSource;
			MutedSource = null;
			Muted = false;
		}
	}
}