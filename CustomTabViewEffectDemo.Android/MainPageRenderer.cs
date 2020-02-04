using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using CustomTabViewEffectDemo;
using CustomTabViewEffectDemo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;
using SignKeys.Effects;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;

[assembly: ExportRenderer(typeof(MainPage), typeof(MainPageRenderer))]
namespace CustomTabViewEffectDemo.Droid
{
    public class MainPageRenderer : TabbedPageRenderer
    {
        WeakReference<TabViewRenderer> tabViewRendererRef;
        public MainPageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                UpdateTabView();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == TabEffect.TabViewProperty.PropertyName)
            {
                UpdateTabView();
            }
        }

        async void UpdateTabView()
        {
            var view = TabEffect.GetTabView(Element) as VisualElement;
            if (view is null)
            {
                tabViewRendererRef = null;
                return;
            }
            await Task.Delay(100);
            var renderer = Platform.GetRenderer(view) as TabViewRenderer;
            if (null != renderer)
            {
                tabViewRendererRef = new WeakReference<TabViewRenderer>(renderer);
            }
            else
            {
                tabViewRendererRef = null;
            }
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (null != tabViewRendererRef && tabViewRendererRef.TryGetTarget(out TabViewRenderer renderer))
            {
                var handled = false;
                float offsetX = ScrollX - renderer.Left;
                float offsetY = ScrollY - renderer.Top;
                if (renderer.Matrix.IsIdentity)
                {
                    e.OffsetLocation(offsetX, offsetY);
                    handled = renderer.DispatchTouchEvent(e);
                    e.OffsetLocation(-offsetX, -offsetY);
                }
                else
                {
                    var transformedEvent = MotionEvent.Obtain(e);
                    transformedEvent.OffsetLocation(offsetX, offsetY);
                    var  mTempMatrix = new Matrix();
                    renderer.Matrix.Invert(mTempMatrix);
                    transformedEvent.Transform(mTempMatrix);
                    handled = renderer.DispatchTouchEvent(transformedEvent);
                    transformedEvent.Recycle();
                }
                if (handled) return true;
            }
            return base.DispatchTouchEvent(e);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                tabViewRendererRef = null;
            }
            base.Dispose(disposing);
        }
    }
}
