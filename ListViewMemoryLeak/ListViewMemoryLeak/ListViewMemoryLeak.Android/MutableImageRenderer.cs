using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using ListViewMemoryLeak;
using ListViewMemoryLeak.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

//[assembly: ExportRenderer(typeof(MutableImage), typeof(MutableImageRenderer))]
[assembly: ExportRenderer(typeof(MutableImage), typeof(FixedImageRenderer))]

namespace ListViewMemoryLeak.Droid
{
	public class MutableImageRenderer : ImageRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);
			Action clearMemory = () =>
			{
				BitmapDrawable bitmapDrawable;
				if (this.Control != null && (bitmapDrawable = this.Control.Drawable as BitmapDrawable) != null)
				{
					Bitmap bitmap = bitmapDrawable.Bitmap;
					if (bitmap != null)
					{
						//Control.SetImageResource(0);
						Control.SetImageBitmap(null);
						Control.SetImageDrawable(null);
						//Control.Dispose();
						bitmap.Recycle();
						bitmap.Dispose();
					}
				}
			};
			if (e.NewElement != null)
			{
				(e.NewElement as MutableImage).ClearMemory = clearMemory;
			}
			if (e.OldElement != null)
			{
				(e.OldElement as MutableImage).ClearMemory = clearMemory;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}

	public class FixedImageRenderer : ViewRenderer<Image, ImageView>
	{
		private bool isDisposed;

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);
			if (e.OldElement == null)
				this.SetNativeControl((ImageView)new FormsImageView(this.Context));
			this.UpdateBitmap(e.OldElement);
			this.UpdateAspect();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Image.SourceProperty.PropertyName)
			{
				this.UpdateBitmap((Image)null);
			}
			else
			{
				if (!(e.PropertyName == Image.AspectProperty.PropertyName))
					return;
				this.UpdateAspect();
			}
		}

		private async void UpdateBitmap(Image previous = null)
		{
			var imageType = typeof(Image);
			var imageLoadingProperty = imageType.GetProperty("IsLoading");

			Bitmap bitmap = (Bitmap)null;
			Xamarin.Forms.ImageSource source = this.Element.Source;
			if (previous == null || !object.Equals((object)previous.Source, (object)this.Element.Source))
			{
				//this.Element.IsLoading = true;
				imageLoadingProperty.SetValue(Element, true, null);
				((FormsImageView)this.Control).SkipInvalidate();
				this.Control.SetImageResource(17170445);
				if (source != null)
				{
					IImageSourceHandler handler;

					const string registrarTypeName = "Xamarin.Forms.Registrar, Xamarin.Forms.Core, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null";
					var registrarType = Type.GetType(registrarTypeName);
					var registered = registrarType.GetTypeInfo()
						.DeclaredProperties
						.Single(p => p.Name == "Registered")
						.GetValue(null);
					var sourceHandler = (IImageSourceHandler)registered.GetType()
						.GetTypeInfo()
						.DeclaredMethods
						.Single(m => m.Name == "GetHandler" && m.IsGenericMethod == true)
						.MakeGenericMethod(typeof(IImageSourceHandler))
						.Invoke(registered, new[] { source.GetType() })
						;
					//if ((handler = (IImageSourceHandler)Registrar.Registered.GetHandler<IImageSourceHandler>(((object)source).GetType())) != null)
					if ((handler = sourceHandler) != null)
					{
						try
						{
							//// ISSUE: explicit reference operation
							//// ISSUE: reference to a compiler-generated field
							//if ((^ this).\u003C\u003E1__state == 0)
							//       {
							//	TaskAwaiter<Bitmap> taskAwaiter = new TaskAwaiter<Bitmap>();
							//	// ISSUE: explicit reference operation
							//	// ISSUE: reference to a compiler-generated field
							//	(^ this).\u003C\u003E1__state = -1;
							//}
							bitmap = await handler.LoadImageAsync(source, this.Context, new CancellationToken());
						}
						catch (TaskCanceledException ex)
						{
						}
						catch (IOException ex)
						{
						}
					}
				}
				if (!this.isDisposed)
				{
					this.Control.SetImageBitmap(bitmap);
					if (bitmap != null)
					{
						bitmap.Dispose();
					}
					//this.Element.IsLoading = false;
					imageLoadingProperty
						.SetValue(Element,
							false,
							null);
					//this.Element.NativeSizeChanged();
					imageType.GetTypeInfo()
						.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
						.Single(m => m.Name == "NativeSizeChanged")
						.Invoke(Element,
							null);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (this.isDisposed)
				return;
			this.isDisposed = true;
			BitmapDrawable bitmapDrawable;
			if (disposing && this.Control != null && (bitmapDrawable = this.Control.Drawable as BitmapDrawable) != null)
			{
				Bitmap bitmap = bitmapDrawable.Bitmap;
				if (bitmap != null)
				{
					//Control.SetImageResource(0);
					Control.SetImageBitmap(null);
					Control.SetImageDrawable(null);
					//Control.Dispose();
					bitmap.Recycle();
					bitmap.Dispose();
					bitmap = null;
					//Control.Drawable = null;
					Java.Lang.JavaSystem.Gc();
				}
			}
			base.Dispose(disposing);
		}

		private void UpdateAspect()
		{
			using (ImageView.ScaleType scaleType = ImageExtensions.ToScaleType(this.Element.Aspect))
				this.Control.SetScaleType(scaleType);
		}
	}
	internal class FormsImageView : ImageView
	{
		private bool skipInvalidate;

		public FormsImageView(Context context)
		  : base(context)
		{
		}

		public void SkipInvalidate()
		{
			this.skipInvalidate = true;
		}

		public override void Invalidate()
		{
			if (this.skipInvalidate)
				this.skipInvalidate = false;
			else
				base.Invalidate();
		}
	}
	internal static class ImageExtensions
	{
		public static ImageView.ScaleType ToScaleType(this Aspect aspect)
		{
			switch (aspect)
			{
				case Aspect.AspectFill:
					return ImageView.ScaleType.CenterCrop;
				case Aspect.Fill:
					return ImageView.ScaleType.FitXy;
				default:
					return ImageView.ScaleType.FitCenter;
			}
		}
	}
}