using System;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class ImageWithReloadButton : ContentPage
	{
		public ImageWithReloadButton(string title)
		{
			InitializeComponent();
			Title = title;
			__Reload.Clicked += ReloadOnClicked;
		}

		public Button __Reload
		{
			get { return Reload; }
		}

		public Image __MyImage
		{
			get { return MyImage; }
		}

		private void ReloadOnClicked(object sender, EventArgs eventArgs)
		{
			__MyImage.Source = null;
			__MyImage.Source = ImageSource.Create(Images.Image800Kb);
		}
	}
}