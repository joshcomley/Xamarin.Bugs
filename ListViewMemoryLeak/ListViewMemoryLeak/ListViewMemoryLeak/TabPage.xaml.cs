using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class TabPage : TabbedPage
	{
		private Page _latestReadyPage;
		private IMutable _waitingForPage;

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
			Children.Add(new ContentPage {Title = "Home"});

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
			CurrentPageChanged += OnCurrentPageChanged;
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

		private void OnCurrentPageChanged(object sender, EventArgs eventArgs)
		{
			if (_waitingForPage != null)
			{
				return;
			}
			LoadCurrentTab();
		}

		private void OnPageReady(object sender, EventArgs eventArgs)
		{
			_waitingForPage.PageReady -= OnPageReady;
			_waitingForPage = null;
			_latestReadyPage = sender as Page;
			if(_latestReadyPage != CurrentPage)
				LoadCurrentTab();
		}

		private void LoadCurrentTab()
		{
			var lastMutable = _latestReadyPage as IMutable;
			if (lastMutable != null)
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					lastMutable.PageMuted += LastMutableOnPageMuted;
					lastMutable.Mute();
				});
			}
			else
			{
				LoadCurrentPage();
			}
		}

		private void LoadCurrentPage()
		{
			Task.Factory.StartNew(() =>
			{
				var currentMutable = CurrentPage as IMutable;
				if (currentMutable == null) return;
				_waitingForPage = currentMutable;
				_waitingForPage.PageReady += OnPageReady;
				Device.BeginInvokeOnMainThread(currentMutable.Unmute);
			});
		}

		private void LastMutableOnPageMuted(object sender, EventArgs eventArgs)
		{
			(sender as IMutable).PageMuted -= LastMutableOnPageMuted;
			LoadCurrentPage();
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