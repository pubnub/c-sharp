using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Threading;

namespace Traffic
{
    public partial class TrafficViewDialogViewController : DialogViewController
    {
        DialogViewController dvc;
        RootElement root;
        Section secOutput;
        UIFont font12 = UIFont.SystemFontOfSize (12);

        public string Destination {
            get;
            set;
        }

        public string Origin {
            get;
            set;
        }

        public TrafficViewDialogViewController (TrafficUpdates tu, string origin, string destination ) : base (UITableViewStyle.Grouped, null)
        {
            string head = "Traffic Update";

            secOutput = new Section("Output");

            root = new RootElement (head) {
                secOutput
            };

            Origin = origin;
            Destination = destination;

            Root = root;
            dvc = new DialogViewController (root, true);
            var tap = new UITapGestureRecognizer ();
            tap.AddTarget (() =>{
                dvc.View.EndEditing (true);
            });
            dvc.View.AddGestureRecognizer (tap);

            tu.TrafficUpdate += Display;

            tap.CancelsTouchesInView = false;
            dvc.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Refresh, delegate {
                secOutput.RemoveRange(0, secOutput.Count);
                tu.Origin = Origin;
                tu.Destination = Destination;

                tu.RunSearch("", false, false);
            });

            AppDelegate.navigation.PushViewController (dvc, true);

            tu.Origin = Origin;
            tu.Destination = Destination;

            tu.RunSearch("", false, false);

        }

        public void Display (string strText, TrafficUpdates.TrafficMessage trafficMessage)
        {
            UIColor uiColor = UIColor.Black;
            if (trafficMessage == TrafficUpdates.TrafficMessage.Blocked) {
                uiColor = UIColor.Red;
            } else if (trafficMessage == TrafficUpdates.TrafficMessage.Heavy) {
                uiColor = UIColor.Orange;
            } else if (trafficMessage == TrafficUpdates.TrafficMessage.Normal) {
                uiColor = UIColor.Green;
            } else if (trafficMessage == TrafficUpdates.TrafficMessage.Low) {
                uiColor = UIColor.Purple;
            }
            StyledMultilineElement sme = new StyledMultilineElement (strText)
            {
                Font = font12,
                TextColor = uiColor,
            };
            //sme.

            AppDelegate.navigation.BeginInvokeOnMainThread(delegate {
                secOutput.Add (sme);
               
            });
        }
    }
}
