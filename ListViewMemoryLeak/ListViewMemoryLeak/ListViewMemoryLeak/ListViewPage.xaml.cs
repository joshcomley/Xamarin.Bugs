using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class ListViewPage : ContentPage
	{
		public ListViewPageConfiguration Configuration { get; set; }

		public ListViewPage(
			string title,
			ListViewPageConfiguration configuration)
		{
			Configuration = configuration;
			InitializeComponent();
			Title = title;
			Disappearing += OnDisappearing;
			Appearing += OnAppearing;
			__MyListView.RowHeight = Configuration.RowHeight;
			if (!Configuration.LoadOnAppearAndClearOnDisappear)
			{
				LoadListView();
			}
		}

		private List<Xamarin.Forms.ImageSource> ImageSources { get; set; }

		public ListView __MyListView
		{
			get { return MyListView; }
		}

		private void OnAppearing(object sender, EventArgs eventArgs)
		{
			if (Configuration.LoadOnAppearAndClearOnDisappear)
			{
				LoadListView();
			}
		}

		private void LoadListView()
		{
			ImageSources = new List<Xamarin.Forms.ImageSource>();
			var items = new List<MyListViewItem>();
			for (var i = 0; i < Configuration.Repetitions; i++)
			{
				var imageSource = Configuration.NewImageSource();
                ImageSources.Add(imageSource);
				items.Add(MyListViewItem.FromImageSource(imageSource));
			}
			__MyListView.ItemsSource = items;
			__MyListView.ItemAppearing += MyListViewOnItemAppearing;
			__MyListView.ItemDisappearing += MyListViewOnItemDisappearing;
		}

		private List<Image> Images = new List<Image>(); 
		private void ImageOnBindingContextChanged(object sender, EventArgs eventArgs)
		{
			var image = sender as Image;
			if (!Images.Contains(image))
			{
				Images.Add(image);
			}
		}

		private void MyListViewOnItemAppearing(object sender, ItemVisibilityEventArgs itemVisibilityEventArgs)
		{
		}

		private void MyListViewOnItemDisappearing(object sender, ItemVisibilityEventArgs itemVisibilityEventArgs)
		{
		}

		private void OnDisappearing(object sender, EventArgs eventArgs)
		{
			if (Configuration.CancelPendingRequestsOnDisappear && ImageSources != null)
			{
				foreach (var source in ImageSources)
				{
					source.Cancel();
				}
			}
			if (Configuration.LoadOnAppearAndClearOnDisappear)
			{
				foreach (var image in Images)
				{
					image.Source = null;
				}
//				__MyListView.ItemsSource = null;
				ImageSources = null;
			}
		}
	}
}