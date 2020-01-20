using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using SignKeys.Effects;
using Xamarin.Forms;

namespace CustomTabViewEffectDemo
{
    public partial class TabView : Grid, ICustomTabView
    {
        public event EventHandler<int> IndexChanged;
        int currentIndex = 0;
        ICommand selectionCommand;
        
        public TabView()
        {
            InitializeComponent();
            selectionCommand = new Command<int>(SelectTab);
            foreach (var view in Children)
            {
                if (view is Button btn)
                {
                    var col = Grid.GetColumn(btn);
                    btn.CommandParameter = col;
                    btn.Command = selectionCommand;
                    btn.BackgroundColor = (col == currentIndex ? Color.Red : Color.Gray); 
                }
            }
        }

        private void SelectTab(int index)
        {
            UpdateTabIndex(index, true);
        }

        void UpdateTabIndex(int index, bool shouldNotify)
        {
            if (currentIndex != index)
            {
                if (Children.FirstOrDefault((v) => v is Button btn && (int)btn.CommandParameter == currentIndex) is Button oldTab)
                {
                    oldTab.BackgroundColor = Color.Gray;
                }
                if (Children.FirstOrDefault((v) => v is Button btn && (int)btn.CommandParameter == index) is Button newTab)
                {
                    newTab.BackgroundColor = Color.Red;
                }
                currentIndex = index;
                if (shouldNotify)
                {
                    IndexChanged?.Invoke(this, index);
                }
                if (Parent is TabbedPage page)
                {
                    page.CurrentPage = page.Children[index];
                }
                    //WeakReference<TabView> meRef = new WeakReference<TabView>(this);
                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                    //    if (meRef.TryGetTarget(out TabView me) && me.Parent is TabbedPage page)
                    //    {
                    //        page.CurrentPage = page.Children[me.currentIndex];
                    //    }

                    //});
            }
        }

        public void OnTabSelected(int index)
        {
            UpdateTabIndex(index, false);
        }
    }
}
