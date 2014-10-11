using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class ScrollViewImagePage : ContentPage, IMutable
	{
		public MutableImageList MutableImageList { get; set; }
		public ScrollViewImagePage(string title, ListViewPageConfiguration configuration)
		{
			MutableImageList = new MutableImageList();
			Configuration = configuration;
			InitializeComponent();
			Title = title;

			Appearing += OnAppearing;
			Disappearing += OnDisappearing;
			__MyButton.Clicked += MyButtonOnClicked;
		}

		private void MyButtonOnClicked(object sender, EventArgs eventArgs)
		{
			MutableImageList.Mute();
		}

		public ListViewPageConfiguration Configuration { get; set; }
		public StackLayout __MyStack
		{
			get { return MyStack; }
		}
		public Button __MyButton
		{
			get { return MyButton; }
		}

		public ManualResetEvent LoadEvent = new ManualResetEvent(false);
		private List<Image> _loadingImages = new List<Image>(); 
		private object _lock = new object();
		private bool _loading = false;
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
//				_loadingImages.Add(image);
                image.PropertyChanged += ImageOnPropertyChanged;
				__MyStack.Children.Add(image);
			}
			LoadEvent.Set();
		}

		private void ImageOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == Image.IsLoadingProperty.PropertyName)
			{
				var image = sender as Image;
				lock (_lock)
				{
					if (image.IsLoading)
					{
						if (!_loadingImages.Contains(image))
						{
							_loadingImages.Add(image);
						}
					}
					else
					{
						_loadingImages.Remove(image);
					}
				}
				if (!_loadingImages.Any())
				{
					if (_loading)
					{
						_loading = false;
						OnPageLoaded();
					}
					if (_unloading)
					{
						_unloading = false;
						OnPageMuted();
					}
				}
			}
		}

		private async void OnDisappearing(object sender, EventArgs eventArgs)
		{
			foreach (var item in MutableImageList)
			{
				try
				{
					if (item.IsLoading)
					{
//						await item.Source.Cancel();
					}
				}
				catch (Exception e)
				{
					
				}
            }
			if (Configuration.LoadOnAppearAndClearOnDisappear)
			{
				__MyStack.Children.Clear();
			}
		}

		public View ContentBackup { get; set; }

		private void OnAppearing(object sender, EventArgs eventArgs)
		{
			if (Configuration.LoadOnAppearAndClearOnDisappear)
			{
				Load();
			}
		}

		public event EventHandler PageReady;
		public event EventHandler PageMuted;

		protected virtual void OnPageMuted()
		{
			var handler = PageMuted;
			if (handler != null)
				handler(this,
					EventArgs.Empty);
		}

		protected virtual void OnPageLoaded()
		{
			var handler = PageReady;
			if (handler != null)
				handler(this,
					EventArgs.Empty);
		}

		public bool IsLoading
		{
			get { return _loadingImages.Any(); }
		}

		private bool _unloading = false;
		public void Mute()
		{
			_unloading = true;
			MutableImageList.Mute();
		}

		private bool _loaded;
		public void Unmute()
		{
			_loading = true;
            if (!_loaded)
			{
				_loaded = true;
				Load();
			}
			MutableImageList.Unmute();
		}
	}

	public interface IMutable
	{
		event EventHandler PageReady;
		event EventHandler PageMuted;
		bool IsLoading { get; }
		void Mute();
		void Unmute();
	}
}