using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	// ReSharper disable once PartialTypeWithSinglePart
	public partial class BlankPage : ContentPage
	{
		public BlankPage(string title)
		{
			InitializeComponent();
			Title = title;
			__MyLabel.Text = title;
		}

		public Label __MyLabel
		{
			get { return MyLabel; }
		}
	}
}