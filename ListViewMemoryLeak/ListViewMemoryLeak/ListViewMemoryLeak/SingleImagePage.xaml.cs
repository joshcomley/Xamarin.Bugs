using System;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class SingleImagePage : ContentPage
	{
		public SingleImagePage(string title)
		{
			Title = title;
			InitializeComponent();
			__MyImage.Source = new UriImageSource
			{
				Uri =
					new Uri(Images.Image76Kb),
				CachingEnabled = false,
			};
		}

		public Image __MyImage
		{
			get { return MyImage; }
		}
	}
}