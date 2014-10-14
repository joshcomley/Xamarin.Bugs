using System;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	public enum MutableState
	{
		Mute,
		Muting,
		Unmute,
		Unmuting
	}
    public interface IMutablePage
	{
		bool IsLoading { get; }
		event EventHandler PageReady;
		event EventHandler PageMuted;
		event EventHandler PageUnmuting;
		MutableState State { get; }
        void RegisterMutableElement(Element element);
		void DeregisterMutableElement(Element element);
		Page Page { get; }
		void Mute();
		void Unmute();
	}
}