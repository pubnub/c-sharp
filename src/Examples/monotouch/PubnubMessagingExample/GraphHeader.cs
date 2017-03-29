
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
    public class GraphHeader: UIViewController
    {
        GraphHeaderView graphHeaderView;

        public GraphHeader ()
        {
            this.View.Frame = new CGRect (0, 2, this.View.Bounds.Width, 400);
            graphHeaderView = new GraphHeaderView ();
            graphHeaderView.Frame = new CGRect (0, 40, this.View.Bounds.Width, this.View.Bounds.Height);
            graphHeaderView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            this.View.AddSubviews (graphHeaderView);
        }

        public void Update (int total, double min, double max, double avg, double lag)
        {
            
            graphHeaderView.Update (total, min, max, avg, lag);
        }
    }


    

}
