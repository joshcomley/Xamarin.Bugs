using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	public class MutablePage : IMutablePage
	{
		public bool IsLoading
		{
			get { return ElementsLoading.Any(); }
		}

		public event EventHandler PageUnmuting;

		protected virtual void OnPageUnmuting()
		{
			var handler = PageUnmuting;
			if (handler != null)
				handler(this,
					EventArgs.Empty);
		}

		public event EventHandler PageReady;
		public event EventHandler PageMuted;
		public Page Page { get; private set; }

		public MutablePage(Page page)
		{
			Page = page;
			State = MutableState.Mute;
			ElementsLoading = new ObservableCollection<Element>();
			MutableElements = new List<Element>();
			page.Appearing += ContentPageOnAppearing;
			page.Disappearing += ContentPageOnDisappearing;
			ElementsLoading.CollectionChanged += ElementsLoadingOnCollectionChanged;
		}

		private void ElementsLoadingOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (ElementsLoading.Any()) return;
			if (State == MutableState.Unmuting)
			{
				OnPageReady();
			}
			if (State == MutableState.Muting)
			{
				OnPageMuted();
			}
		}

		private void ContentPageOnDisappearing(object sender, EventArgs eventArgs)
		{
			Mute();
		}

		private void ContentPageOnAppearing(object sender, EventArgs eventArgs)
		{
			Unmute();
		}

		internal ObservableCollection<Element> ElementsLoading { get; set; }
		private ICollection<Element> MutableElements { get; set; }

		public MutableState State { get; private set; }

		public void RegisterMutableElement(Element element)
		{
			if (MutableElements.Contains(element)) return;
			MutableElements.Add(element);
		}

		public void DeregisterMutableElement(Element element)
		{
			if (!MutableElements.Contains(element)) return;
			MutableElements.Remove(element);
		}

		internal void MarkElementAsLoading(Element element)
		{
			var page = element.FindNearestAncestorOfType<ContentPage>();
			if (page == null)
				throw new InvalidOperationException(
					"No page found for element");
			if (ElementsLoading.Contains(element)) return;
			ElementsLoading.Add(element);
		}

		internal void MarkElementAsNotLoading(Element element)
		{
			ElementsLoading.Remove(element);
		}

		public void Mute()
		{
			//if (_isWaitingForCurrentPage)
			//{
			//	_isWaitingForCurrentPage = false;

			//}
			if (State == MutableState.Unmute)
			{
				State = MutableState.Muting;
				ActionElements((mutableElement, element) => mutableElement.Mute(element,
					Page));
				if (IsLoading) return;
				State = MutableState.Mute;
				OnPageMuted();
			}
			else if (State == MutableState.Unmuting)
			{
				PageReady += OnPageUnmutedQueueMute;
			}
		}

		private void OnPageUnmutedQueueMute(object sender, EventArgs eventArgs)
		{
			PageReady -= OnPageUnmutedQueueMute;
			Mute();
		}

		private void ActionElements(Action<IMutableElement, Element> action)
		{
			var toMute = new List<Element>();
			foreach (var elem in MutableElements)
			{
				MarkElementAsLoading(elem);
				toMute.Add(elem);
			}
			foreach (var elem in toMute)
			{
				var mutableAction = MutableElementManager.Instance.GetMutableAction(elem.GetType());
				if (mutableAction != null)
				{
					action(mutableAction,
						elem);
				}
			}
		}

		public bool CurrentPageIsNotThisAndNotNull()
		{
			return MutableElementManager.Instance.CurrentPage != null
					&& MutableElementManager.Instance.CurrentPage != this;
		}

		public void Unmute()
		{
			if (CurrentPageIsNotThisAndNotNull())
			{
				switch (MutableElementManager.Instance.CurrentPage.State)
				{
					case MutableState.Muting:
					case MutableState.Unmute:
					case MutableState.Unmuting:
						MutableElementManager.Instance.QueuedPage = this;
						MutableElementManager.Instance.CurrentPage.PageMuted += WaitingForCurrentPageMuted;
						return;
				}
			}
			MutableElementManager.Instance.CurrentPage = this;
			State = MutableState.Unmuting;
			Task.Factory.StartNew(() => Device.BeginInvokeOnMainThread(() =>
			{
				OnPageUnmuting();
				ActionElements((mutableElement, element) => mutableElement.Unmute(element,
					Page));
				if (IsLoading) return;
				OnPageReady();
			}));
		}

		private void WaitingForCurrentPageMuted(object sender, EventArgs eventArgs)
		{
			MutableElementManager.Instance.CurrentPage.PageMuted -= WaitingForCurrentPageMuted;
			if (MutableElementManager.Instance.QueuedPage == this)
			{
				MutableElementManager.Instance.QueuedPage = null;
				Unmute();
			}
		}

		protected virtual void OnPageMuted()
		{
			State = MutableState.Mute;
			var handler = PageMuted;
			if (handler != null)
				handler(this,
					EventArgs.Empty);
		}

		protected virtual void OnPageReady()
		{
			State = MutableState.Unmute;
			var handler = PageReady;
			if (handler != null)
				handler(this,
					EventArgs.Empty);
		}
	}
}