using System.Collections.Generic;

namespace ListViewMemoryLeak.Mutable
{
	public class MutableImageSourceList : List<MutableImageSource>
	{
		public void MuteAndClear()
		{
			Mute();
			Clear();
		}
		public void Mute()
		{
			foreach (var mutableImageSource in this)
			{
				mutableImageSource.Mute();
			}
		}

		public void Unmute()
		{
			foreach (var mutableImageSource in this)
			{
				mutableImageSource.Unmute();
			}
		}
	}
}