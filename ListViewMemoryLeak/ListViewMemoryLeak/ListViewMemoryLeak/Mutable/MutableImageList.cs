using System.Collections.Generic;

namespace ListViewMemoryLeak
{
	public class MutableImageList : List<MutableImage>
	{
		private readonly object _lock = new object();

		public void Mute()
		{
			lock (_lock)
			{
				foreach (var mutableImage in this)
				{
					mutableImage.Mute();
				}
			}
		}

		public void Unmute()
		{
			lock (_lock)
			{
				foreach (var mutableImage in this)
				{
					mutableImage.Unmute();
				}
			}
		}
	}
}