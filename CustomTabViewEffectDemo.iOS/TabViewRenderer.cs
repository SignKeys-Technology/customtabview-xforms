using System;
using CoreGraphics;
using CustomTabViewEffectDemo;
using CustomTabViewEffectDemo.iOS;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TabView), typeof(TabViewRenderer))]
namespace CustomTabViewEffectDemo.iOS
{
    public class TabViewRenderer: VisualElementRenderer<Grid>
    {
        public TabViewRenderer()
        {
        }

        public override bool PointInside(CGPoint point, UIEvent uievent)
        {
            foreach (var child in Element.Children)
            {
                var renderer = Platform.GetRenderer(child) as UIView;
                if (null != renderer && renderer.Frame.Contains(point)) {
                    return true;
                }
            }
            return base.PointInside(point, uievent);
        }
    }
}
