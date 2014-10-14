using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class ScrollViewImagePage : ContentPage
	{

		public ScrollViewImagePage(string title, ListViewPageConfiguration configuration)
		{
			MutableImageList = new MutableImageList();
			Configuration = configuration;
			InitializeComponent();
			Title = title;

			Appearing += OnAppearing;
			Disappearing += OnDisappearing;
			__MyButton.Clicked += MyButtonOnClicked;
			MutableElementManager.Instance.GetMutablePage(this).PageUnmuting += OnPageUnmuting;
		}

		private bool _loaded;
		private void OnPageUnmuting(object sender, EventArgs eventArgs)
		{
			if (!_loaded)
			{
				_loaded = true;
				Load();
			}
		}

		public MutableImageList MutableImageList { get; set; }

		public ListViewPageConfiguration Configuration { get; set; }

		public StackLayout __MyStack
		{
			get { return MyStack; }
		}

		public Button __MyButton
		{
			get { return MyButton; }
		}

		private void MyButtonOnClicked(object sender, EventArgs eventArgs)
		{
			MutableImageList.Mute();
		}

		private void Load()
		{
			__MyStack.Children.Clear();
			for (var i = 0; i < Configuration.Repetitions; i++)
			{
				var image = new MutableImage
				{
					WidthRequest = Configuration.RowHeight,
					HeightRequest = Configuration.RowHeight,
					Source = Configuration.NewImageSource()
				};
				MutableImageList.Add(image);
				//image.PropertyChanged += ImageOnPropertyChanged;
				__MyStack.Children.Add(image);
			}
		}

		//private void ImageOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		//{
		//	if (propertyChangedEventArgs.PropertyName == Image.IsLoadingProperty.PropertyName)
		//	{
		//		var image = sender as Image;
		//		lock (_lock)
		//		{
		//			if (image.IsLoading)
		//			{
		//				if (!_loadingImages.Contains(image))
		//				{
		//					_loadingImages.Add(image);
		//				}
		//			}
		//			else
		//			{
		//				_loadingImages.Remove(image);
		//			}
		//		}
		//		if (!_loadingImages.Any())
		//		{
		//			if (_loading)
		//			{
		//				_loading = false;
		//				OnPageLoaded();
		//			}
		//			if (_unloading)
		//			{
		//				_unloading = false;
		//				OnPageMuted();
		//			}
		//		}
		//	}
		//}

		private void OnDisappearing(object sender, EventArgs eventArgs)
		{
			if (Configuration.LoadOnAppearAndClearOnDisappear)
			{
				__MyStack.Children.Clear();
			}
		}

		private void OnAppearing(object sender, EventArgs eventArgs)
		{
			if (Configuration.LoadOnAppearAndClearOnDisappear)
			{
				Load();
			}
		}
	}
}