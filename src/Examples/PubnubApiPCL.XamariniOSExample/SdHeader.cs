
using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using MonoTouch.Dialog;
using PubnubApi;
using System.Threading;
using System.Diagnostics;
using CoreGraphics;

namespace PubnubMessagingExample
{
    public class SdHeader: UIViewController
    {
        SdHeaderView sdHeaderView;

        public SdHeader (string[] speedTestNames, string[] speedTestSorted)
        {
            this.View.Frame = new CGRect (0, 2, this.View.Bounds.Width, 400);
            this.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            sdHeaderView = new SdHeaderView (speedTestNames, speedTestSorted);
            sdHeaderView.Frame = new CGRect (0, 20, this.View.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
            sdHeaderView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            this.View.AddSubviews (sdHeaderView);
        }

        public void Update (string[] speedTestNames, string[] speedTestSorted)
        {
            sdHeaderView.Update (speedTestNames, speedTestSorted);
        }
    }

}
