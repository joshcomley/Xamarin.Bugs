using System;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class TabPage : TabbedPage
	{
		public TabPage()
		{
			InitializeComponent();
			//Children.Add(new ImageWithReloadButton("Reload1"));
			//Children.Add(new ImageWithReloadButton("Reload2"));
			//Children.Add(new ImageWithReloadButton("Reload3"));
			//Children.Add(new SingleImageInListViewPage("ILV1"));
			//Children.Add(new SingleImageInListViewPage("ILV2"));
			//Children.Add(new SingleImageInListViewPage("ILV3"));
			//Children.Add(new SingleImagePage("Image1"));
			//Children.Add(new SingleImagePage("Image2"));
			//Children.Add(new SingleImagePage("Image3"));
			//Children.Add(new BlankPage("Image1"));
			//Children.Add(new BlankPage("Image2"));
			//Children.Add(new BlankPage("Image3"));
			//Children.Add(new ImagePage("Scroll1"));
			//Children.Add(new ImagePage("Scroll"));
			Children.Add(new ContentPage { Title = "Home" });

			const int numberOfRepetitions = 10;
			var configuration = new ListViewPageConfiguration
			{
				Mode = ListViewPageMode.WebImages,
				CancelPendingRequestsOnDisappear = false,
				LoadOnAppearAndClearOnDisappear = false,
				Repetitions = numberOfRepetitions,
				RowHeight = 150,
				MuteImages = true,
			};
			Func<string, ContentPage> singleImageInListViewPageConstructor = title =>
				new SingleImageInListViewPage(title,
					configuration);
			Func<string, ContentPage> scrollViewImagePageConstructor = title =>
				new ScrollViewImagePage(title,
					configuration);
			Func<string, ContentPage> listViewWithConfiguration = title =>
				new ListViewPage(title,
					configuration);

			IsEnabled = false;
			var constructor = scrollViewImagePageConstructor;

			const int numberOfTabsAndPages = 4;
			for (var i = 0; i < numberOfTabsAndPages; i++)
			{
				Add(string.Format("P {0}",
					i + 1),
					constructor);
			}
		}

		private void Add(string title, Func<string, ContentPage> constructor)
		{
			Children.Add(constructor(title));

			ToolbarItems.Add(
				new ToolbarItem
			{
				Name = title,
				Command = new Command(async () => { await Navigation.PushAsync(constructor(title)); }),
			});
		}
	}
}