using System.ComponentModel;
using SignKeys.Effects;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace CustomTabViewEffectDemo
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : Xamarin.Forms.TabbedPage
    {
        public MainPage()
        {
            On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
            TabEffect.SetCustomTabHeight(this, new TabHeight(TabHeightMode.RelativeToNativeTabBar, 16));
            InitializeComponent();
            CurrentPage = Children[1];
        }
    }
}