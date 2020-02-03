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
    internal class RendererContainer : UIView
    {
        public UIView Renderer { get; private set; }
        UIView bottomSpace;
        public RendererContainer(Xamarin.Forms.VisualElement element)
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            AccessibilityIdentifier = "SignKeys.Effects.Platform.iOS.RendererContainer";
            BackgroundColor = UIColor.Clear;
            Renderer = (UIView)Xamarin.Forms.Platform.iOS.Platform.CreateRenderer(element);
            Renderer.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(Renderer);
            bottomSpace = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            AddSubview(bottomSpace);
            var views = new NSDictionary<NSString, NSObject>(new NSString[] {
                (NSString)"renderer",
                (NSString)"space"
            }, new NSObject[] {
                Renderer,
                bottomSpace
            });
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-0-[renderer]-0-|", NSLayoutFormatOptions.AlignAllTop, null, views));
            AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-0-[renderer({element.HeightRequest})]-0-[space]-0-|", NSLayoutFormatOptions.AlignAllLeading | NSLayoutFormatOptions.AlignAllTrailing, null, views));
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ((IVisualElementRenderer)Renderer).Element.Layout(Renderer.Frame.ToRectangle());
        }

        public void UpdateBackgroundColor(UIColor color)
        {
            if (null != bottomSpace)
            {
                bottomSpace.BackgroundColor = color;
            }
        }

        public VisualElement FormsView => null == Renderer ? null : ((IVisualElementRenderer)Renderer).Element;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Renderer?.RemoveFromSuperview();
                Renderer?.Dispose();
                Renderer = null;
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
        UIImage originalShadowImage;
        UIImage originalBackgroundImage;

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
                    var heightConfig = TabEffect.GetCustomTabHeight(me.Element);
                    var tabBar = me.Container.Subviews?.FirstOrDefault((v) => v is UITabBar) as UITabBar;
                    if (null != tabBar)
                    {
                        // Remove bar's shadow & top line
                        me.originalBackgroundImage = tabBar.BackgroundImage;
                        tabBar.BackgroundImage = new UIImage();
                        me.originalShadowImage = tabBar.ShadowImage;
                        tabBar.ShadowImage = new UIImage();
                        tabBar.ClipsToBounds = true;
                    }
                    if (heightConfig == null)
                    {
                        var constraint = me.rendererContainer.TopAnchor.ConstraintEqualTo(container.LayoutMarginsGuide.BottomAnchor, -(nfloat)xfView.HeightRequest);
                        constraint.SetIdentifier("top_to_bottom");
                        constraint.Active = true;
                    }
                    else
                    {
                        if (heightConfig.Mode == TabHeightMode.Absolute)
                        {
                            var constraint = me.rendererContainer.TopAnchor.ConstraintEqualTo(container.LayoutMarginsGuide.BottomAnchor, -(nfloat)heightConfig.Value);
                            constraint.SetIdentifier("top_to_bottom");
                            constraint.Active = true;
                        }
                        else if (null != tabBar)
                        {
                            if (heightConfig.Mode == TabHeightMode.RelativeToNativeTabBar)
                            {
                                var constraint = me.rendererContainer.HeightAnchor.ConstraintEqualTo(tabBar.HeightAnchor, 1.0f, (nfloat)heightConfig.Value);
                                constraint.SetIdentifier("relative_height");
                                constraint.Active = true;
                            }
                            else
                            {
                                var constraint = me.rendererContainer.HeightAnchor.ConstraintEqualTo(tabBar.HeightAnchor, (nfloat)heightConfig.Value, 0.0f);
                                constraint.SetIdentifier("proportional_height");
                                constraint.Active = true;
                            }
                        }
                    }
                    me.rendererContainer.UpdateBackgroundColor(TabEffect.GetTabBarColor(me.Element).ToUIColor());
                }
            });
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);
            if (null == rendererContainer || null == Element || null == Container) return;
            if (args.PropertyName == VisualElement.HeightProperty.PropertyName)
            {
                var heightConfig = TabEffect.GetCustomTabHeight(Element);
                if (null == heightConfig)
                {
                    var contraints = rendererContainer.Constraints;
                    if (contraints.FirstOrDefault((c) => string.Equals(c.GetIdentifier(), "top_to_bottom")) is NSLayoutConstraint constraint)
                    {
                        constraint.Constant = -(nfloat)((VisualElement)Element).HeightRequest;
                        rendererContainer.UpdateConstraints();
                    }
                }
            }
            if (args.PropertyName == TabEffect.TabBarColorProperty.PropertyName)
            {
                rendererContainer.UpdateBackgroundColor(TabEffect.GetTabBarColor(Element).ToUIColor());
            }
            else if (args.PropertyName == TabEffect.CustomTabHeightProperty.PropertyName)
            {
                var heightConfig = TabEffect.GetCustomTabHeight(Element);
                var contraints = rendererContainer.Constraints;
                if (null == heightConfig || heightConfig.Mode == TabHeightMode.Absolute)
                {
                    var value = -(nfloat)(null == heightConfig ? (rendererContainer.FormsView?.HeightRequest ?? 0) : heightConfig.Value);
                    var found = false;
                    var shouldStop = false;
                    foreach (var c in contraints)
                    {
                        var id = c.GetIdentifier();
                        switch (id)
                        {
                            case "top_to_bottom":
                                c.Constant = value;
                                found = true;
                                break;
                            case "relative_height":
                            case "proportional_height":
                                rendererContainer.RemoveConstraint(c);
                                shouldStop = true;
                                break;
                            default: break;
                        }
                        if (found || shouldStop)
                        {
                            break;
                        }
                    }
                    if (false == found)
                    {
                        var constraint = rendererContainer.TopAnchor.ConstraintEqualTo(Container.LayoutMarginsGuide.BottomAnchor, value);
                        constraint.SetIdentifier("top_to_bottom");
                        constraint.Active = true;
                    }
                }
                else
                {
                    var value = (nfloat)heightConfig.Value;
                    var shouldStop = false;
                    var needNewConstraint = true;
                    foreach (var c in contraints)
                    {
                        var id = c.GetIdentifier();
                        switch (id)
                        {
                            case "top_to_bottom":
                            case "proportional_height":
                                rendererContainer.RemoveConstraint(c);
                                shouldStop = true;
                                break;
                            case "relative_height":
                                if (heightConfig.Mode == TabHeightMode.RelativeToNativeTabBar)
                                {
                                    c.Constant = value;
                                    needNewConstraint = false;
                                }
                                else
                                {
                                    rendererContainer.RemoveConstraint(c);
                                    shouldStop = true;
                                }
                                break;
                            default: break;
                        }
                        if (!needNewConstraint || shouldStop)
                        {
                            break;
                        }
                    }
                    if (needNewConstraint && Container.Subviews?.FirstOrDefault((v) => v is UITabBar) is UITabBar tabBar)
                    {
                        if (heightConfig.Mode == TabHeightMode.RelativeToNativeTabBar)
                        {
                            var constraint = rendererContainer.HeightAnchor.ConstraintEqualTo(tabBar.HeightAnchor, 1.0f, value);
                            constraint.SetIdentifier("relative_height");
                            constraint.Active = true;
                        }
                        else
                        {
                            var constraint = rendererContainer.HeightAnchor.ConstraintEqualTo(tabBar.HeightAnchor, value, 0.0f);
                            constraint.SetIdentifier("proportional_height");
                            constraint.Active = true;
                        }
                    }
                }
            }
        }

        protected override void OnDetached()
        {
            rendererContainer?.RemoveFromSuperview();
            rendererContainer?.Dispose();
            rendererContainer = null;
            if (Container.Subviews?.FirstOrDefault((v) => v is UITabBar) is UITabBar tabBar)
            {
                tabBar.ShadowImage = originalShadowImage;
                tabBar.BackgroundImage = originalBackgroundImage;
                tabBar.ClipsToBounds = false;
            }
        }
    }
}
