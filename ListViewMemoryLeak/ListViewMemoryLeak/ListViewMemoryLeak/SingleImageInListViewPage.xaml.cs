using System.Collections.Generic;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class SingleImageInListViewPage : ContentPage
	{
		public ListViewPageConfiguration Configuration { get; set; }

		public SingleImageInListViewPage(string title, ListViewPageConfiguration configuration)
		{
			Configuration = configuration;
			InitializeComponent();
			Title = title;
			var itemsSource = new List<MyListViewItem>();
			__MyList.RowHeight = Configuration.RowHeight;
			for (var i = 0; i < configuration.Repetitions; i++)
			{
				itemsSource.Add(new MyListViewItem(Images.LocalWebImage143Kb));
			}
			__MyList.ItemsSource = itemsSource;
		}

		public ListView __MyList
		{
			get { return MyList; }
		}
	}
}