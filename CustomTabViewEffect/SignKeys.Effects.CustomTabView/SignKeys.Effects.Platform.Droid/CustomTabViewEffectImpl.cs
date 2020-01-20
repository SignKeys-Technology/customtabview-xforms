using System;
using System.Linq;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("SignKeys.Effects")]
[assembly: ExportEffect(typeof(SignKeys.Effects.Platform.Droid.CustomTabViewEffectImpl), "CustomTabViewEffect")]
namespace SignKeys.Effects.Platform.Droid
{

    public class CustomTabViewEffectImpl : PlatformEffect
    {
        VisualElementRenderer<Xamarin.Forms.View> renderer;
        RendererLayoutChangeListener layoutChangeListener;
        TabBarLayoutChangeListener tabBarLayoutChangeListener;

        public static void Preserve()
        {
            var now = DateTime.Now;
        }

        protected override void OnAttached()
        {
            var effect = Element.Effects.FirstOrDefault(x => x is SignKeys.Effects.CustomTabViewEffect);

            if (effect == null || Container == null) return;

            var containerRef = new WeakReference<ViewGroup>(Container);
            var meRef = new WeakReference<CustomTabViewEffectImpl>(this);
            Device.InvokeOnMainThreadAsync(() =>
            {
                if (containerRef.TryGetTarget(out ViewGroup container)
                && meRef.TryGetTarget(out CustomTabViewEffectImpl me)
                && container.GetChildView<BottomNavigationView>() is BottomNavigationView bottomNavView
                && TabEffect.GetTabView(me.Element) is VisualElement xfView)
                {
                    container.SetBackgroundColor(Android.Graphics.Color.Red);
                    var renderer = (VisualElementRenderer<Xamarin.Forms.View>)Xamarin.Forms.Platform.Android.Platform.CreateRendererWithContext(xfView, container.Context);
                    Xamarin.Forms.Platform.Android.Platform.SetRenderer(xfView, renderer);
                    renderer.Elevation = bottomNavView.Elevation + 1;
                    renderer.Tracker.UpdateLayout();

                    var tabHeight = TabEffect.GetCustomTabHeight(me.Element);
                    int viewHeight = 0;
                    var formViewHeight = xfView.HeightRequest;
                    if (null == tabHeight)
                    {
                        viewHeight = (int)container.Context.ToPixels(xfView.HeightRequest);
                    }
                    else
                    {
                        switch (tabHeight.Mode)
                        {
                            case TabHeightMode.Absolute:
                                viewHeight = (int)container.Context.ToPixels(tabHeight.Value);
                                formViewHeight = tabHeight.Value;
                                break;
                            case TabHeightMode.RelativeToNativeTabBar:
                                viewHeight = (int)Math.Round((double)bottomNavView.Height + container.Context.ToPixels(tabHeight.Value));
                                formViewHeight = container.Context.FromPixels(bottomNavView.Height) + tabHeight.Value;
                                break;
                            case TabHeightMode.ProportionalToNativeTabBar:
                                viewHeight = (int)Math.Round((double)bottomNavView.Height * tabHeight.Value);
                                formViewHeight = container.Context.FromPixels((double)bottomNavView.Height * tabHeight.Value);
                                break;
                            default: break;
                        }
                    } 
                    renderer.Layout(0, 0, bottomNavView.Width, viewHeight);
                    xfView.Layout(new Rectangle(0, 0, container.Context.FromPixels(bottomNavView.Width), formViewHeight));
                    me.layoutChangeListener = new RendererLayoutChangeListener();
                    renderer.AddOnLayoutChangeListener(me.layoutChangeListener);
                    if (null != tabHeight && tabHeight.Mode != TabHeightMode.Absolute)
                    {
                        me.tabBarLayoutChangeListener = new TabBarLayoutChangeListener();
                        me.tabBarLayoutChangeListener.HeightChanged += me.HandleTabBarHeightChanged;
                        bottomNavView.AddOnLayoutChangeListener(me.tabBarLayoutChangeListener);
                    }

                    var layoutParams = new Android.Widget.RelativeLayout.LayoutParams(
                         Android.Widget.RelativeLayout.LayoutParams.MatchParent,
                         viewHeight);
                    //layoutParams.AddRule(Android.Widget.LayoutRules.AlignTop, bottomNavView.Id);
                    layoutParams.AddRule(Android.Widget.LayoutRules.AlignBottom, bottomNavView.Id);
                    var parentView = (Android.Widget.RelativeLayout)bottomNavView.Parent;
                    parentView.AddView(renderer, layoutParams);
                    me.renderer = renderer;
                }
            });
        }

        private void HandleTabBarHeightChanged(object sender, int height)
        {
            if (null == Container
                || !(Element is VisualElement element)
                || !(sender is BottomNavigationView bottomNavView)
                || !(bottomNavView.Parent is ViewGroup viewGroup)) return;
            var count = viewGroup.ChildCount;
            VisualElementRenderer<Xamarin.Forms.View> renderer = null;
            var i = count - 1;
            while (i >= 0)
            {
                renderer = viewGroup.GetChildAt(i) as VisualElementRenderer<Xamarin.Forms.View>;
                if (null != renderer) break;
                i--;
            }
            if (null == renderer) return;
            var layoutParams = renderer.LayoutParameters as Android.Widget.RelativeLayout.LayoutParams;
            var tabHeight = TabEffect.GetCustomTabHeight(Element);
            if (null == layoutParams || null == tabHeight || tabHeight.Mode == TabHeightMode.Absolute) return;
            var viewHeight = 0;
            //double formHeight = 0;
            if (tabHeight.Mode == TabHeightMode.RelativeToNativeTabBar)
            {
                //formHeight = Container.Context.FromPixels(height) + tabHeight.Value;
                viewHeight = height + (int)Container.Context.ToPixels(tabHeight.Value);
            }
            else
            {
                //formHeight = Container.Context.FromPixels(height) * tabHeight.Value;
                viewHeight = (int)((double)height * Container.Context.ToPixels(tabHeight.Value));
            }
            if (viewHeight != layoutParams.Height)
            {
                layoutParams.Height = viewHeight;
                renderer.LayoutParameters = layoutParams;
            }
        }

        protected override void OnDetached()
        {
            if (null != layoutChangeListener)
            {
                renderer?.RemoveOnLayoutChangeListener(layoutChangeListener);
                layoutChangeListener.Dispose();
                layoutChangeListener = null;
            }
            if (null != tabBarLayoutChangeListener)
            {
                if (Container?.GetChildView<BottomNavigationView>() is BottomNavigationView bottomNavView)
                {
                    bottomNavView.RemoveOnLayoutChangeListener(tabBarLayoutChangeListener);
                }
                tabBarLayoutChangeListener.HeightChanged -= HandleTabBarHeightChanged;
                tabBarLayoutChangeListener.Dispose();
                tabBarLayoutChangeListener = null;
            }
            
            renderer?.RemoveFromParent();
            renderer?.Dispose();
            renderer = null;
        }
    }

    static class ViewGroupExtentions
    {
        public static T GetChildView<T>(this ViewGroup group) where T: Android.Views.View
        {
            var count = group.ChildCount;
            var i = 0;
            while (i < count)
            {
                var view = group.GetChildAt(i);
                if (view.GetType() == typeof(T))
                {
                    return (T)view;
                }
                if (view is ViewGroup childGroup)
                {
                    if (childGroup.GetChildView<T>() is T result)
                    {
                        return result;
                    }
                }
                i++;
            }
            return default(T);
        }
    }

    internal class TabBarLayoutChangeListener : Java.Lang.Object, Android.Views.View.IOnLayoutChangeListener
    {
        public event EventHandler<int> HeightChanged;
        public TabBarLayoutChangeListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public TabBarLayoutChangeListener()
        {
        }

        public void OnLayoutChange(Android.Views.View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            HeightChanged?.Invoke(v, bottom - top);
        }
    }

    internal class RendererLayoutChangeListener : Java.Lang.Object, Android.Views.View.IOnLayoutChangeListener
    {
        public RendererLayoutChangeListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public RendererLayoutChangeListener()
        {
        }

        public void OnLayoutChange(Android.Views.View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            if (v is VisualElementRenderer<Xamarin.Forms.View> renderer)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    renderer.Element.Layout(new Rectangle(0, 0, v.Context.FromPixels(right - left), renderer.Element.HeightRequest));
                });
            }
        }
    }
}
