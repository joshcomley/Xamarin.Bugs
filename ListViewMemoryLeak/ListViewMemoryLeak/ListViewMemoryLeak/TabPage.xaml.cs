using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class TabPage : TabbedPage
	{
		private readonly ManualResetEvent _muteWait = new ManualResetEvent(true);
		private readonly ManualResetEvent _pageChangedWait = new ManualResetEvent(true);
		private readonly List<IMutable> _pagesWithMuteEvent = new List<IMutable>();
		private readonly List<IMutable> _pagesWithUnmuteEvent = new List<IMutable>();
		private readonly ManualResetEvent _unmuteWait = new ManualResetEvent(true);

		private IMutable _latestPage = null;
		private Page _latestReadyPage;
		private IMutable _waitingFor;

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

		private Page LastPage { get; set; }

		private void OnCurrentPageChanged(object sender, EventArgs eventArgs)
		{
			//var oldPage = LastPage;
			LastPage = CurrentPage;
			//var oldPageMutable = oldPage as IMutable;
			if (_waitingFor != null)
			{
				return;
			}
			//if (oldPageMutable != null && oldPageMutable.IsLoading)
			//{
			//	return;
			//}
			LoadCurrentTab();
		}

		private void OnPageReady(object sender, EventArgs eventArgs)
		{
			_waitingFor.PageReady -= OnPageReady;
			_waitingFor = null;
			_latestReadyPage = sender as Page;
			if(_latestReadyPage != CurrentPage)
				LoadCurrentTab();
		}

		private void LoadCurrentTab()
		{
			var lastMutable = _latestReadyPage as IMutable;
			if (lastMutable != null)
			{
				Device.BeginInvokeOnMainThread(async () =>
				{
					lastMutable.PageMuted += LastMutableOnPageMuted;
					await lastMutable.MuteAsync();
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
				_waitingFor = currentMutable;
				_waitingFor.PageReady += OnPageReady;
				Device.BeginInvokeOnMainThread(currentMutable.Unmute);
			});
		}

		private void LastMutableOnPageMuted(object sender, EventArgs eventArgs)
		{
			(sender as IMutable).PageMuted -= LastMutableOnPageMuted;
			LoadCurrentPage();
		}

		private async void OnCurrentPageChanged2(object sender, EventArgs eventArgs)
		{
			var lastPage = LastPage;
			LastPage = CurrentPage;
			// Wait for unmute to complete
			//_pageChangedWait.WaitOne();
			_pageChangedWait.Reset();
			if (lastPage != null && (lastPage as IMutable).IsLoading)
			{
				(lastPage as IMutable).PageReady += OnPageReady;
				return;
			}
			Func<Task> action = async () =>
			{
				_unmuteWait.WaitOne();
				_muteWait.WaitOne();

				if (lastPage != null)
				{
					var mutable = lastPage as IMutable;
					if (mutable != null)
					{
						if (!_pagesWithMuteEvent.Contains(mutable))
						{
							_pagesWithMuteEvent.Add(mutable);
							mutable.PageMuted += MutableOnPageMuted;
						}
						_muteWait.Reset();
						Device.BeginInvokeOnMainThread(async () => { await mutable.MuteAsync(); });
						//Device.BeginInvokeOnMainThread(mutable.MuteAsync);
						// Wait for mute to complete
						_muteWait.WaitOne();
						//mutable.PageMuted -= MutableOnPageMuted;
					}
				}
				// Start in a new thread to give GC a chance
				await Task.Factory.StartNew(() =>
				{
					var unmutable = CurrentPage as IMutable;
					if (unmutable != null)
					{
						if (!_pagesWithUnmuteEvent.Contains(unmutable))
						{
							_pagesWithUnmuteEvent.Add(unmutable);
							unmutable.PageReady += UnmutableOnPageReady;
						}
						_unmuteWait.Reset();
						Device.BeginInvokeOnMainThread(unmutable.Unmute);
						// Wait for unmute to complete
						_unmuteWait.WaitOne();
						//unmutable.PageUnmuted -= UnmutableOnPageUnmuted;
					}
				})
					.ContinueWith(t =>
					{
						_pageChangedWait.Set();
						IsEnabled = true;
					});
				lastPage = CurrentPage;
			};
			await action();
		}

		private void MutableOnPageMuted(object sender, EventArgs eventArgs)
		{
			_muteWait.Set();
		}

		private void UnmutableOnPageReady(object sender, EventArgs eventArgs)
		{
			_unmuteWait.Set();
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