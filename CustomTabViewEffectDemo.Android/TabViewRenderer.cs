using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using CustomTabViewEffectDemo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomTabViewEffectDemo.TabView), typeof(TabViewRenderer))]
namespace CustomTabViewEffectDemo.Droid
{
    public class TabViewRenderer : ViewRenderer
    {
        public TabViewRenderer(Context context) : base(context)
        {
            SetClipChildren(false);
            //SetClipToOutline(false);
            SetClipToPadding(false);
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            if (Parent is Android.Views.ViewGroup parentView)
            {
                parentView.SetClipChildren(false);
                parentView.SetClipToOutline(false);
                parentView.SetClipToPadding(false);
            }
        }
    }
}
