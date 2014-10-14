using System;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	public interface IMutableElement
	{
		Action<Element, Page> Mute { get; }
		Action<Element, Page> Unmute { get; }
	}

	public class MutableElement<T> : IMutableElement
		where T : Element
	{
		public Action<T, Page> Mute
		{
			get { return _mute; }
			set
			{
				_mute = value;
				_muteBase = (element, page) => _mute((T) element,
					page);
			}
		}

		public Action<T, Page> Unmute
		{
			get { return _unmute; }
			set
			{
				_unmute = value;
				_unmuteBase = (element, page) => _unmute((T) element,
					page);
			}
		}

		private Action<Element, Page> _muteBase = null;
		Action<Element, Page> IMutableElement.Mute { get { return _muteBase; } }
		private Action<Element, Page> _unmuteBase = null;
		private Action<T, Page> _mute;
		private Action<T, Page> _unmute;
		Action<Element, Page> IMutableElement.Unmute { get { return _unmuteBase; } }

        public MutableElement(Action<T, Page> mute, Action< T, Page> unmute)
		{
			Mute = mute;
			Unmute = unmute;
		}
	}
}