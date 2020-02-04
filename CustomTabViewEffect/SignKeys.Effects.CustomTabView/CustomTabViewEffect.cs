using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Xamarin.Forms;

namespace SignKeys.Effects
{
    public enum TabHeightMode
    {
        Absolute,
        RelativeToNativeTabBar,
        ProportionalToNativeTabBar
    }
    public class TabHeight
    {
        public TabHeightMode Mode { get; set; }
        public double Value { get; set; }

        public TabHeight(TabHeightMode mode, double value)
        {
            Mode = mode;
            Value = value;
        }
    }

    public static class TabEffect
    {
        public static readonly BindableProperty CustomTabHeightProperty = BindableProperty.Create(
        "CustomTabHeight",
        typeof(TabHeight),
        typeof(TabEffect),
        default(TabHeight),
        BindingMode.OneWay);

        public static TabHeight GetCustomTabHeight(BindableObject obj)
        {
            return (TabHeight)obj.GetValue(CustomTabHeightProperty);
        }
        public static void SetCustomTabHeight(BindableObject obj, TabHeight value)
        {
            obj.SetValue(CustomTabHeightProperty, value);
        }

        public static BindableProperty TabViewProperty = BindableProperty.CreateAttached(
            "TabView",
            typeof(ICustomTabView),
            typeof(TabEffect),
            null,
            BindingMode.OneWay,
            propertyChanged: HandleTabViewChanged
        );
        public static ICustomTabView GetTabView(BindableObject obj)
        {
            return (ICustomTabView)obj.GetValue(TabViewProperty);
        }
        public static void SetTabView(BindableObject obj, ICustomTabView value)
        {
            obj.SetValue(TabViewProperty, value);
        }

        public static BindableProperty TabBarColorProperty = BindableProperty.CreateAttached(
            "TabBarColor",
            typeof(Color),
            typeof(TabEffect),
            default(Color),
            BindingMode.OneWay
        );
        public static Color GetTabBarColor(BindableObject obj)
        {
            return (Color)obj.GetValue(TabBarColorProperty);
        }
        public static void SetTabBarColor(BindableObject obj, Color value)
        {
            obj.SetValue(TabBarColorProperty, value);
        }

        public static readonly BindableProperty BottomContentOffsetProperty = BindableProperty.Create(
            "BottomContentOffset",
            typeof(double),
            typeof(TabEffect),
            default(double),
            BindingMode.OneWay);
        public static double GetBottomContentOffset(BindableObject obj)
        {
            return (double)obj.GetValue(BottomContentOffsetProperty);
        }
        public static void SetBottomContentOffset(BindableObject obj, double value)
        {
            obj.SetValue(BottomContentOffsetProperty, value);
        }

        static void HandleTabViewChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (oldValue is BindableObject)
            {
                ((BindableObject)oldValue).RemoveBinding(BindableObject.BindingContextProperty);
            }
            if (oldValue is View)
            {
                ((View)oldValue).Parent = null;
            }
            if (!(bindable is TabbedPage element))
            {
                return;
            }
            element.CurrentPageChanged -= CurrentPageChanged;
            element.PropertyChanged -= HandleElementPropertyChanged;
            element.ChildAdded -= OnChildPageAdded;
            var effect = (CustomTabViewEffect)element.Effects.FirstOrDefault(x => x is CustomTabViewEffect);

            if (effect != null)
            {
                if (!(newValue is ICustomTabView))
                {
                    element.Effects.Remove(effect);
                }

                return;
            }
            UpdateBottomInsets(element);
            element.PropertyChanged += HandleElementPropertyChanged;
            element.ChildAdded += OnChildPageAdded;
            if (newValue is ICustomTabView newView)
            {
                element.CurrentPageChanged += CurrentPageChanged;
                newView.OnTabSelected(element.Children.IndexOf(element.CurrentPage));
                var newEffect = new CustomTabViewEffect();
                element.Effects.Add(newEffect);
            }
            if (newValue is BindableObject)
            {
                ((BindableObject)newValue).SetBinding(BindableObject.BindingContextProperty, new Binding("BindingContext", mode: BindingMode.OneWay, source: element));
            }
            if (newValue is View)
            {
                ((View)newValue).Parent = element;
            }
        }

        private static void OnChildPageAdded(object sender, ElementEventArgs e)
        {
            if (sender is TabbedPage page && e.Element is ContentPage child)
            {
                var inset = GetBottomContentOffset(page);
                if (child.Content is Layout layout)
                {
                    layout.Padding = new Thickness(0, 0, 0, inset);
                }
            }
        }

        private static void CurrentPageChanged(object sender, EventArgs e)
        {
            if (sender is TabbedPage page && TabEffect.GetTabView(page) is ICustomTabView view)
            {
                view.OnTabSelected(page.Children.IndexOf(page.CurrentPage));
            }
        }


        static void UpdateBottomInsets(Element element)
        {
            if (element is TabbedPage page)
            {
                var offset = TabEffect.GetBottomContentOffset(element);
                foreach (var p in page.Children)
                {
                    if (((ContentPage)p).Content is Layout layout) {
                        layout.Padding = new Thickness(0, 0, 0, offset);
                    }
                }
            }
        }

        static private void HandleElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BottomContentOffsetProperty.PropertyName)
            {
                UpdateBottomInsets(sender as Element);
            }
        }
    }

    public class CustomTabViewEffect : RoutingEffect
    {
        const string NAME = "SignKeys.Effects.CustomTabViewEffect";
        public static string RendererContainerIdentifier = "SignKeys.Effects.RendererContainer";
        public CustomTabViewEffect() : base(NAME)
        {
            
        }
    }
}
