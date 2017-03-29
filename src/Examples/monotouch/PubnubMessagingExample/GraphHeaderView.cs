
using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
//using MonoTouch.Dialog;
using PubnubApi;
using System.Threading;
using System.Diagnostics;
using CoreGraphics;

namespace PubnubMessagingExample
{
    class GraphHeaderView: UITableViewCell
    {
        DrawingView dv;
        int chartHeight = 210;
        int chartWidth = 210;

        public GraphHeaderView ()
        {
            dv = new DrawingView (new CGRect (0, 0, chartWidth, chartHeight), 0, 0, 0);
            
            this.AddSubview (dv);
        }

        public void Update (int total, double min, double max, double avg, double lag)
        {
            dv.Update (lag, max, min);
        }

        public override void LayoutSubviews ()
        {
            dv.Frame = new CGRect ((Bounds.Width - chartWidth) / 2, 10, chartWidth, chartHeight);
        }
    }

    public class DrawingView : UIView
    {
        UIFont font10 = UIFont.SystemFontOfSize (10);
        UIFont font18b = UIFont.BoldSystemFontOfSize (18);
        
        double lag;
        
        double max;

        double min;

        public DrawingView (CGRect p, double lag, double max, double min) : base (p)
        {
            BackgroundColor = UIColor.White;
            this.lag = lag;
            this.max = max;
            this.min = min;
        }

        public void Update (double lag, double max, double min)
        {
            this.min = min;
            this.lag = lag;
            this.max = max;
            SetNeedsDisplay ();
        }

        private CGPoint GetCenterPoint (CGPoint p1, CGPoint p2)
        {
            return new CGPoint ((p2.X + p1.X) / 2, (p2.Y + p1.Y) / 2);
        }

        private double GetSlope (CGPoint p1, CGPoint p2)
        {
            if ((p2.Y - p1.Y) != 0)
                return (p1.X - p2.X) / (p2.Y - p1.Y);
            else
                return double.PositiveInfinity;
        }

        private double GetIntersect (CGPoint p1, CGPoint p2)
        {
            double slope = GetSlope (p1, p2);
            CGPoint center = GetCenterPoint (p1, p2);
            if (double.IsPositiveInfinity (slope))
                return 0;
            return center.Y - (slope * center.X);
        }

        public override void Draw (CGRect rect)
        {
            float x = 105;
            float y = 105;
            float r = 100;
            float twopi = (2f * (float)Math.PI) * -1f;
            
            CGContext ctx = UIGraphics.GetCurrentContext ();
            
            //base circle
            UIColor.FromRGB (137, 136, 133).SetColor ();
            ctx.AddArc (x, y, r + 3, 0, twopi, true);
            ctx.FillPath ();
            
            //border circle
            UIColor.FromRGB (231, 231, 231).SetColor ();
            ctx.AddArc (x, y, r, 0, twopi, true);
            ctx.FillPath ();

            //Center circle
            UIColor.White.SetColor ();
            ctx.AddArc (x, y, r / 1.2f, 0, twopi, true);
            ctx.FillPath ();

            UIColor.Black.SetFill ();
            //fast
            NSString text = new NSString("Fast");
            CGSize stringSize = text.GetSizeUsingAttributes(new UIStringAttributes { Font = font10 });
            text.DrawString (new CGPoint (105 - r + 7, 105 + r / 2 - 28), stringSize.Width, font10, UILineBreakMode.TailTruncation);
            
            //Slow
            text = new NSString("Slow");
            stringSize = text.GetSizeUsingAttributes(new UIStringAttributes { Font = font10 });
            text.DrawString (new CGPoint (105 + r - 25, 105 - r / 2 + 20), stringSize.Width, font10, UILineBreakMode.TailTruncation);

            //pubnub
            UIColor.Red.SetFill ();
            text = new NSString("PubNub");
            stringSize = text.GetSizeUsingAttributes(new UIStringAttributes { Font = font18b });
            text.DrawString (new CGPoint ((r * 2 - stringSize.Width) / 2 + 5, y - r / 2f), stringSize.Width, font18b, UILineBreakMode.TailTruncation);


            //needle
            //double percentFromMaxValue = max / 100.0d;
            max = 1000;
            double percentFromMaxValue = max / 100.0d;

            if (lag > max) {
                lag = max;
            }

            //angle
            double invertLag = ((max - min) / 2 - lag) * 2 + lag;
            //Debug.WriteLine("lag: "+ lag.ToString() + " invlag:" + invLag.ToString());
            double angle = 360 - Math.Round ((double)invertLag / percentFromMaxValue * (90 / 100.0f)) * Math.PI / 180.0;
            //double angle2  = 360 - Math.Round((double)lag / percentFromMaxValue* (90 / 100.0f)) * Math.PI / 180.0;;
            //Debug.WriteLine("lagangle: "+ angle.ToString() + " invLagangle" + angle2.ToString());
            //double angle = WrapValue(lag, max);
            
            float distance = 80;
            CGPoint p = new CGPoint (distance * (float)Math.Cos (angle), distance * (float)Math.Sin (angle));
            
            UIColor.Brown.SetStroke ();
            CGPath path1 = new CGPath ();
            ctx.SetLineWidth (3);
            
            CGPoint newPoint = new CGPoint (105 - p.X, 105 - p.Y);
            
            CGPoint[] linePoints = new CGPoint[] { 
                newPoint,
                new CGPoint (105, 105)
            };
            
            path1.AddLines (linePoints);
            path1.CloseSubpath ();
            
            ctx.AddPath (path1);
            ctx.DrawPath (CGPathDrawingMode.FillStroke);

            //caliberate
            UIColor.Brown.SetColor ();
            double theta = 0.0;
            for (int i = 0; i < 360; i++) {
                float bx4 = (float)(x - 4 + (r - 10) * (Math.Cos (theta * Math.PI / 180)));
                float by4 = (float)(y - 15 + (r - 10) * (Math.Sin (theta * Math.PI / 180)));

                NSString dotText = new NSString(".");
                if ((theta > 160) && (theta < 350)) {
                    UIColor.Black.SetColor ();
                    dotText.DrawString (new CGPoint (bx4, by4), (dotText.GetSizeUsingAttributes(new UIStringAttributes { Font = font18b })).Width, font18b, UILineBreakMode.TailTruncation);
                } else if (((theta >= 0) && (theta < 40)) || ((theta >= 350) && (theta <= 360))) {
                    //redline
                    UIColor.Red.SetColor ();
                    dotText.DrawString (new CGPoint (bx4, by4), (dotText.GetSizeUsingAttributes(new UIStringAttributes { Font = font18b })).Width, font18b, UILineBreakMode.TailTruncation);
                }
                theta += 10.0;

            }

            //small circle
            UIColor.FromRGB (220, 214, 194).SetColor ();
            //ctx.AddArc (x, y+y*.33f, r/1.5f, 0, twopi, true );
            ctx.AddArc (x, y + r / 2f, r / 2f, 0, twopi, true);
            ctx.FillPath ();
            
            //speed in small circle
            UIColor.Black.SetFill ();
            NSString lagText = new NSString (Convert.ToInt32 (lag).ToString ());
            stringSize = lagText.GetSizeUsingAttributes(new UIStringAttributes { Font = font18b });
            lagText.DrawString (new CGPoint ((r * 2 - stringSize.Width) / 2 + 4, y + r / 2f - 15), stringSize.Width, font18b, UILineBreakMode.TailTruncation);

            //ms
            UIColor.Black.SetFill ();
            NSString msText = new NSString ("MS");
            stringSize = msText.GetSizeUsingAttributes(new UIStringAttributes { Font = font18b });
            msText.DrawString (new CGPoint ((r - stringSize.Width) / 2 + 55, y + r / 2f + 10), stringSize.Width, font18b, UILineBreakMode.TailTruncation);
        }
    }
}

