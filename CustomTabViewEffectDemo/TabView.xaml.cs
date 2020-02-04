using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                if (view is Button btn && btn != floater)
                {
                    var col = Grid.GetColumn(btn);
                    btn.CommandParameter = col;
                    btn.Command = selectionCommand;
                    btn.BackgroundColor = (col == currentIndex ? Color.Red : Color.Gray); 
                }
            }
            floater.Command = new Command(() =>
            {
                System.Diagnostics.Debug.WriteLine("Tapped");
            });
        }

        private async void SelectTab(int index)
        {
            UpdateTabIndex(index, true);
            if (index == 2)
            {
                if (floater.Opacity == 0)
                {
                    floater.Opacity = 1;
                    await floater.TranslateTo(0, floater.TranslationY - 200);
                }
                else
                {
                    await floater.TranslateTo(0, floater.TranslationY + 200);
                    floater.Opacity = 0;
                }
            }
        }

        void UpdateTabIndex(int index, bool shouldNotify)
        {
            if (currentIndex != index)
            {
                if (Children.FirstOrDefault((v) => v is Button btn && btn != floater && (int)btn.CommandParameter == currentIndex) is Button oldTab)
                {
                    oldTab.BackgroundColor = Color.Gray;
                }
                if (Children.FirstOrDefault((v) => v is Button btn && btn != floater && (int)btn.CommandParameter == index) is Button newTab)
                {
                    newTab.BackgroundColor = Color.Red;
                }
                currentIndex = index;
                if (shouldNotify)
                {
                    IndexChanged?.Invoke(this, index);
                }
                this.HandleNewTabIndex(index);
            }
        }

        public void OnTabSelected(int index)
        {
            UpdateTabIndex(index, false);
        }
    }
}
