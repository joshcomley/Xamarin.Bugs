// ReSharper disable once CheckNamespace
namespace Xamarin.Forms
{
	public static class ElementExtensions
	{
		public static T FindNearestAncestorOfType<T>(this Element element)
			where T : Element
		{
			var parent = element.Parent;
			if (parent == null) return null;
			while (parent.Parent != null)
			{
				parent = parent.Parent;
				if (parent is T)
				{
					return parent as T;
				}
			}
			return null;
		}
	}
}