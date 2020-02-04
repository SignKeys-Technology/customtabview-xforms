using System;
using System.Linq;
using CoreGraphics;
using CustomTabViewEffectDemo;
using CustomTabViewEffectDemo.iOS;
using Foundation;
using SignKeys.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MainPage), typeof(MainPageRenderer))]
namespace CustomTabViewEffectDemo.iOS
{
    public class MainPageRenderer: TabbedRenderer
    {
        MainContainer touchView;
        public MainPageRenderer()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            touchView = new MainContainer();
            View.Add(touchView);
            touchView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            touchView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            touchView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor).Active = true;
            touchView.RightAnchor.ConstraintEqualTo(View.RightAnchor).Active = true;
        }

        public override void ViewDidLayoutSubviews()
        {
            //if (null == TabBar) return;
            base.ViewDidLayoutSubviews();
            View.BringSubviewToFront(touchView);
        }
    }

    public class MainContainer : UIView
    {
        public MainContainer()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
        }

        public MainContainer(CGRect frame) : base(frame)
        {
        }

        public MainContainer(NSCoder coder) : base(coder)
        {
        }

        protected MainContainer(NSObjectFlag t) : base(t)
        {
        }

        protected internal MainContainer(IntPtr handle) : base(handle)
        {
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            if (Superview?.Subviews?.LastOrDefault((v) => string.Equals(v.AccessibilityIdentifier, CustomTabViewEffect.RendererContainerIdentifier))
                is UIView rendererContainer
                && rendererContainer.Subviews?.FirstOrDefault((v) => v is TabViewRenderer) is TabViewRenderer aRenderer)
            {
                var p = ConvertPointToView(point, aRenderer);
                var hitTestResult = aRenderer.HitTest(p, uievent);
                if (null != hitTestResult)
                {
                    return hitTestResult;
                }
            }
            if (Superview?.Subviews?.FirstOrDefault((v)
                => !(v is UITabBar) &&
                !string.Equals(v.AccessibilityIdentifier, CustomTabViewEffect.RendererContainerIdentifier))
                is UIView childrenContainerView)
            {
                return childrenContainerView.HitTest(point, uievent);
            }
            return base.HitTest(point, uievent);
        }
    }
}
