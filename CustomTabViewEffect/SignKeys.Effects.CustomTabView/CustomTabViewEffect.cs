using System;
using System.Linq;
using Xamarin.Forms;

namespace SignKeys.Effects
{
    public static class TabEffect
    {
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

        public static BindableProperty TabbarColorProperty = BindableProperty.CreateAttached(
            "TabbarColor",
            typeof(Color),
            typeof(TabEffect),
            default(Color),
            BindingMode.OneWay
        );
        public static Color GetTabbarColor(BindableObject obj)
        {
            return (Color)obj.GetValue(TabbarColorProperty);
        }
        public static void SetTabbarColor(BindableObject obj, Color value)
        {
            obj.SetValue(TabbarColorProperty, value);
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

            var effect = (CustomTabViewEffect)element.Effects.FirstOrDefault(x => x is CustomTabViewEffect);

            if (effect != null)
            {
                if (!(newValue is ICustomTabView))
                {
                    element.Effects.Remove(effect);
                }

                return;
            }

            if (newValue is ICustomTabView newView)
            {
                element.CurrentPageChanged += CurrentPageChanged;
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

        private static void CurrentPageChanged(object sender, EventArgs e)
        {
            if (sender is TabbedPage page && TabEffect.GetTabView(page) is ICustomTabView view)
            {
                view.OnTabSelected(page.Children.IndexOf(page.CurrentPage));
            }
        }
    }

    public class CustomTabViewEffect : RoutingEffect
    {
        const string NAME = "SignKeys.Effects.CustomTabViewEffect";
        public CustomTabViewEffect() : base(NAME)
        {
            
        }
    }
}
