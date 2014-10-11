using System;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	public class ImageSource
	{
		public static UriImageSource Create(string uri = null, bool cache = true)
		{
			uri = uri ?? Images.Image50Kb;
            return new UriImageSource
			{
				Uri = new Uri(uri),
				CachingEnabled = cache,
			};
		}
	}
}