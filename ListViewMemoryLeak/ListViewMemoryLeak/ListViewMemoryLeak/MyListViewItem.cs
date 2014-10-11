namespace ListViewMemoryLeak
{
	public class MyListViewItem
	{
		public static MyListViewItem FromImageSource(Xamarin.Forms.ImageSource imageSource)
		{
			return new MyListViewItem
			{
				WebImage = imageSource
			};
		}

		public MyListViewItem(string url = null, bool cache = true)
		{
			WebImage = ImageSource.Create(url,
				cache);
		}

		public string MyText { get; set; }

		public Xamarin.Forms.ImageSource WebImage { get; set; }
	}
}