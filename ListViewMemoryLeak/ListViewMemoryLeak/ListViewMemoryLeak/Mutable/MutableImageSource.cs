using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ListViewMemoryLeak.Mutable
{
	public class MutableImageSource
	{
		public MutableImageSource(Xamarin.Forms.ImageSource baseImageSource)
		{
			BaseImageSource = baseImageSource;
		}

		public Xamarin.Forms.ImageSource BaseImageSource { get; set; }
		public object Identifier { get; set; }
		public bool Muted { get; private set; }

		public void Mute()
		{
			if (Muted) return;
			Func<bool>[] attempts =
			{
				() => Try<FileImageSource>(f =>
				{
					Identifier = f.File;
					f.File = null;
				}),
				() => Try<UriImageSource>(f =>
				{
					Identifier = f.Uri;
					f.Uri = null;
				}),
				() => Try<StreamImageSource>(f =>
				{
					Identifier = f.Stream;
					f.Stream = null;
				})
			};
			foreach (var attempt in attempts)
			{
				if (attempt())
				{
					Muted = true;
					break;
				}
			}
		}

		public void Unmute()
		{
			if (!Muted) return;
			Func<bool>[] attempts =
			{
				() => Try<FileImageSource>(f => { f.File = (string) Identifier; }),
				() => Try<UriImageSource>(f => { f.Uri = (Uri) Identifier; }),
				() => Try<StreamImageSource>(f => { f.Stream = (Func<CancellationToken, Task<Stream>>) Identifier; })
			};
			foreach (var attempt in attempts)
			{
				if (attempt())
				{
					Muted = false;
					break;
				}
			}
			Identifier = null;
		}

		private bool Try<T>(Action<T> action)
			where T : Xamarin.Forms.ImageSource
		{
			if (BaseImageSource is T)
			{
				action((T) BaseImageSource);
				return true;
			}
			return false;
		}
	}
}