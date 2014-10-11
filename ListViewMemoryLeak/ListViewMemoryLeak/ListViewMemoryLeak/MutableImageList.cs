using System.Collections.Generic;
using System.Threading.Tasks;

namespace ListViewMemoryLeak
{
	public class MutableImageList : List<MutableImage>
	{
		private readonly object _lock = new object();

		public async Task<IEnumerable<bool>>  MuteAsync()
		{
			var tasks = new List<Task<bool>>();
			lock (_lock)
			{
				foreach (var mutableImage in this)
				{
					tasks.Add(mutableImage.MuteAsync());
				}
			}
			var results = new List<bool>();
			foreach (var result in tasks)
			{
				results.Add(await result);
			}
			return results;
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