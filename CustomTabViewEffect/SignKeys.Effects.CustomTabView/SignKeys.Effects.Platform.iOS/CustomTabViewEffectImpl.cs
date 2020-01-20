using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("SignKeys.Effects")]
[assembly: ExportEffect(typeof(SignKeys.Effects.Platform.iOS.CustomTabViewEffectImpl), "CustomTabViewEffect")]
namespace SignKeys.Effects.Platform.iOS
{
    internal class RendererContainer: UIView
    {
        UIView renderer;
        UIView bottomSpace;
        public RendererContainer(Xamarin.Forms.VisualElement element)
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            BackgroundColor = UIColor.Clear;
            renderer = (UIView)Xamarin.Forms.Platform.iOS.Platform.CreateRenderer(element);
            renderer.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(renderer);
            bottomSpace = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            AddSubview(bottomSpace);
            var views = new NSDictionary<NSString, NSObject>(new NSString[] {
                (NSString)"renderer",
                (NSString)"space"
            }, new NSObject[] {
                renderer,
                bottomSpace
            });
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-0-[renderer]-0-|", NSLayoutFormatOptions.AlignAllTop, null, views));
            AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-0-[renderer({element.HeightRequest})]-0-[space]-0-|", NSLayoutFormatOptions.AlignAllLeading | NSLayoutFormatOptions.AlignAllTrailing, null, views));
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ((IVisualElementRenderer)renderer).Element.Layout(renderer.Frame.ToRectangle());
        }

        public void UpdateBackgroundColor(UIColor color)
        {
            if (null != bottomSpace)
            {
                bottomSpace.BackgroundColor = color;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                renderer?.RemoveFromSuperview();
                renderer?.Dispose();
                renderer = null;
                bottomSpace?.RemoveFromSuperview();
                bottomSpace?.Dispose();
                bottomSpace = null;
            }
            base.Dispose(disposing);
        }
    }
    public class CustomTabViewEffectImpl : PlatformEffect
    {
        RendererContainer rendererContainer;

        public static void Preserve()
        {
            var now = DateTime.Now;
        }
        protected override void OnAttached()
        {
            var effect = Element.Effects.FirstOrDefault(x => x is SignKeys.Effects.CustomTabViewEffect);
            
            if (effect == null || Container == null) return;

            var containerRef = new WeakReference<UIView>(Container);
            var meRef = new WeakReference<CustomTabViewEffectImpl>(this);
            Device.InvokeOnMainThreadAsync(() =>
            {
                if (containerRef.TryGetTarget(out UIView container)
                && meRef.TryGetTarget(out CustomTabViewEffectImpl me))
                {
                    if (!(TabEffect.GetTabView(me.Element) is VisualElement xfView)) return;
                    me.rendererContainer = new RendererContainer(xfView);
                    container.AddSubview(me.rendererContainer);
                    me.rendererContainer.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor).Active = true;
                    me.rendererContainer.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor).Active = true;
                    me.rendererContainer.BottomAnchor.ConstraintEqualTo(container.BottomAnchor).Active = true;
                    //me.rendererContainer.HeightAnchor.ConstraintEqualTo((nfloat)xfView.HeightRequest + container.SafeAreaInsets.Bottom).Active = true;
                    me.rendererContainer.TopAnchor.ConstraintEqualTo(container.LayoutMarginsGuide.BottomAnchor, -(nfloat)xfView.HeightRequest).Active = true;
                    me.rendererContainer.UpdateBackgroundColor(TabEffect.GetTabbarColor(me.Element).ToUIColor());
                }
            });
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);
            if (null != rendererContainer && args.PropertyName == TabEffect.TabbarColorProperty.PropertyName)
            {
                rendererContainer.UpdateBackgroundColor(TabEffect.GetTabbarColor(Element).ToUIColor());
            }
        }

        protected override void OnDetached()
        {
            rendererContainer?.RemoveFromSuperview();
            rendererContainer?.Dispose();
            rendererContainer = null;
        }
    }
}
