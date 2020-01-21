using System;
using Xamarin.Forms;

namespace SignKeys.Effects
{
    public interface ICustomTabView
    {
        /// <summary>
        /// Notify that the current tab index in the view has been changed
        /// </summary>
        event EventHandler<int> IndexChanged;

        /// <summary>
        /// The TabbedPage has changed the current page
        /// </summary>
        /// <param name="index">Index of the current page</param>
        void OnTabSelected(int index);
    }

    public static class ICustomTabViewExtensions {
        public static void HandleNewTabIndex(this ICustomTabView view, int index)
        {
            //Update the current page of the TabbedPage
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