using System;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	public class MutableImage : Image
	{
		public Xamarin.Forms.ImageSource MutedSource { get; private set; }
		public bool Muted { get; private set; }

		public Action ClearMemory { get; set; }

		public void Mute()
		{
			if (Muted) return;
			MutedSource = Source;
			Source = null;
			Muted = true;
		}

		public void Unmute()
		{
			if (!Muted) return;
			Source = MutedSource;
			MutedSource = null;
			Muted = false;
		}
	}
}