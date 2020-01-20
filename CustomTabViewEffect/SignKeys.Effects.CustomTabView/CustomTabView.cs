using System;
using Xamarin.Forms;

namespace SignKeys.Effects
{
    public interface ICustomTabView
    {
        event EventHandler<int> IndexChanged;
        void OnTabSelected(int index);
    }

    public static class ICustomTabViewExtensions {
        public static void HandleNewTabIndex(this ICustomTabView view, int index)
        {
            if (view is View v
                && v.Parent is TabbedPage page
                && 0 <= index
                && index < page.Children.Count)
            {
                page.CurrentPage = page.Children[index];
            }
        }
    }
}