using System;
using System.Threading;
using Xamarin.Forms;

namespace ListViewMemoryLeak
{
	public class MutableImage : Image
	{
		public Xamarin.Forms.ImageSource MutedSource { get; private set; }
		public bool Muted { get; private set; }

		static MutableImage()
		{
			MutableElementManager.Instance.RegisterMutableAction<MutableImage>(
				new MutableElement<MutableImage>(
					(element, page) => element.Mute(),
					(element, page) => element.Unmute())
				);
		}

		public MutableImage()
		{
			PropertyChanged += BindableObject_PropertyChanged;
		}

		private void BindableObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName != IsLoadingProperty.PropertyName) return;
			MutableElementManager.Instance.SetElementLoading(this, IsLoading);
			var mutabePage = MutableElementManager.Instance.GetMutablePage(this);
			mutabePage.RegisterMutableElement(this);
		}

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