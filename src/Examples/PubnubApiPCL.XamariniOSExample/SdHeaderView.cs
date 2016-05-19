
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
    public class SdHeaderView: UITableViewCell
    {
        UIFont font13b = UIFont.BoldSystemFontOfSize (13);

        string[] SpeedTestNames {
            get;
            set;
        }

        string[] SpeedTestSorted {
            get;
            set;
        }

        static SdHeaderView ()
        {
        }

        public SdHeaderView (string[] speedTestNames, string[] speedTestSorted)
        {
            BackgroundColor = UIColor.White;
            this.SpeedTestNames = speedTestNames;
            this.SpeedTestSorted = speedTestSorted;
        }


        public void Update (string[] speedTestNames, string[] speedTestSorted)
        {
            this.SpeedTestNames = speedTestNames;
            this.SpeedTestSorted = speedTestSorted;
            SetNeedsDisplay ();
        }

        public override void Draw (CGRect rect)
        {
            const int padright = 10;
            const int padtop = 10;
            float boxWidth = 60;
            CGSize ssize;

            nfloat fWidth = UIScreen.MainScreen.Bounds.Width;
            this.Frame = new CGRect (0, 0, fWidth, UIScreen.MainScreen.Bounds.Width);

            CGContext ctx = UIGraphics.GetCurrentContext ();

            const int offset = 5;
            nfloat bw = Bounds.Width - offset;

            int cols = (int)(bw / boxWidth);
            int rows = (int)(SpeedTestNames.Count () / cols);
            int height = 23;
            
            UIColor.Black.SetColor ();

            int counter = 0;
            int counterProg = 0;
            float x = offset, y = 0;
            for (int i = 0; i <= rows; i++) {
                y += height;
                counterProg = counter;
                for (int j = 0; j < cols; j++) {
                    x = offset + j * boxWidth;
                    if (counter < SpeedTestNames.Count ()) {
                        UIColor.White.SetFill ();
                        ctx.SetLineWidth (1f);
                        ctx.StrokeRect (new CGRect (x - 1, y, boxWidth, height));
                        UIColor.FromRGB (235, 231, 213).SetFill ();
                        ctx.FillRect (new CGRect (x, y + 1, boxWidth - 2, height - 2));
                        
                        UIColor.Black.SetFill ();
                        NSString text = new NSString ((SpeedTestSorted [counter] == null) ? "" : SpeedTestSorted [counter] + " MS");
                        text.DrawString(new CGPoint (x + offset, y + 2), boxWidth - offset - 2, font13b, UILineBreakMode.TailTruncation);
                        counter++;
                    } else {
                        break;
                    }
                }
                counter = counterProg;
                y += height;
                for (int j = 0; j < cols; j++) {
                    x = offset + j * boxWidth;
                    
                    if (counter < SpeedTestNames.Count ()) {
                        UIColor.White.SetFill ();
                        ctx.SetLineWidth (1f);
                        ctx.StrokeRect (new CGRect (x - 1, y, boxWidth, height));
                        UIColor.FromRGB (207, 197, 161).SetFill ();
                        ctx.FillRect (new CGRect (x, y + 1, boxWidth - 2, height - 2));

                        UIColor.Black.SetFill ();
                        NSString text = new NSString (SpeedTestNames [counter]);
                        text.DrawString(new CGPoint (x + offset, y + 2), boxWidth - offset, font13b, UILineBreakMode.TailTruncation);
                        counter++;
                    } else {
                        break;
                    }
                }
            }

            base.Draw (rect);
        }
    }
}
