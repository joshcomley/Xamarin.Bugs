using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace ListViewMemoryLeak.Mutable
{
	public class MutableElementManager
	{
		private static MutableElementManager _instance;

		public static MutableElementManager Instance
		{
			get { return _instance = _instance ?? new MutableElementManager(); }
		}

		public MutableElementManager()
		{
			Pages = new Dictionary<Page, MutablePage>();
			Mutables = new Dictionary<Type, IMutableElement>();
		}

		private Dictionary<Page, MutablePage> Pages { get; set; }

		private Dictionary<Type, IMutableElement> Mutables { get; set; }

		[DebuggerDisplay("Title: {CurrentPage.Page.Title}")]
		public MutablePage CurrentPage { get; set; }
		[DebuggerDisplay("Title: {QueuedPage.Page.Title}")]
		public MutablePage QueuedPage { get; set; }

		public void SetElementLoading(Element element, bool isLoading)
		{
			var page = element.FindNearestAncestorOfType<ContentPage>();
			var mutablePage = GetMutablePage(page);
			if (isLoading)
				mutablePage.MarkElementAsLoading(element);
			else mutablePage.MarkElementAsNotLoading(element);
		}

		public void RegisterMutableAction<T>(IMutableElement mutableElement)
		{
			RegisterMutableAction(typeof (T),
				mutableElement);
		}

		public IMutableElement GetMutableAction(Type type)
		{
			return Mutables.ContainsKey(type)
				? Mutables[type]
				: null;
		}

		public void RegisterMutableAction(Type type, IMutableElement mutableElement)
		{
			if (Mutables.ContainsKey(type))
			{
				Mutables[type] = mutableElement;
			}
			else
			{
				Mutables.Add(type,
					mutableElement);
			}
		}

		public MutablePage GetMutablePage(Element element)
		{
			var page = element.FindNearestAncestorOfType<ContentPage>();
			return GetMutablePage(page);
		}

		public MutablePage GetMutablePage(Page page)
		{
			if (!Pages.ContainsKey(page))
			{
				Pages.Add(page,
					new MutablePage(page));
			}
			return Pages[page];
		}
	}
}