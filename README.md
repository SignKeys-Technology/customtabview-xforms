# Custom tab view for Xamarin Forms
Overlay bottom tab bar with a custom view in Xamarin Forms

# Usage

1. Create a view (Grid, Stack, Frame...) that implements SignKeys.Effects.ICustomTabView

        public partial class TabView : Grid, ICustomTabView
        {
            public event EventHandler<int> IndexChanged;

            public void OnTabSelected(int index)
            {
                //Highlight the selected tab
            }
            ...
        }

2. Set it as the tab view of a TabbedPage using TabEffect.

    XAML:

        <TabbedPage 
            ...
            xmlns:effects="clr-namespace:SignKeys.Effects;assembly=SignKeys.Effects.CustomTabView"
        >
            <effects:TabEffect.TabView>
                <local:TabView />
            </effects:TabEffect.TabView>
            ...

    CS:

        TabEffect.SetTabView(this, new TabView());

3. Set the view's height.

By default, the drawn view's height will be equal to its `HeightRequest`.

To make the view's height change dynamically based on the native tab bar's height (which varies from device to device and from orientation to orientation), use `TabEffect.SetCustomTabHeight(...)`.

    TabEffect.SetCustomTabHeight(this, new TabHeight(TabHeightMode.RelativeToNativeTabBar, 16));

The custom view will be always 16px higher than the native tab bar.

    TabEffect.SetCustomTabHeight(this, new TabHeight(TabHeightMode.ProportionalToNativeTabBar, 1.2));

The custom view's height = the tab bar's height * 1.2
