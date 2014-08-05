using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;
using MonoTouch.CoreLocation;
using System.Threading;

namespace Traffic
{
    public partial class TrafficDialogViewController : DialogViewController
    {
        public enum GetLocationActions{
            UpdateTraffic,
            SetOrigin,
            SetDestination,
            None
        }

        CLLocationManager iPhoneLocationManager = null;
        public static string pnChannel = "";
        UIFont font12 = UIFont.SystemFontOfSize (12);
        UIFont font10 = UIFont.SystemFontOfSize (10);
        TrafficUpdates tu; 
        UITextField tfUpdateLocation;
        UITextField tfOrigin;
        UITextField tfDestination;
        UITextField tfNextStreet;

        GlassButton gbBlocked;
        GlassButton gbHeavy; 
        GlassButton gbNormal; 
        GlassButton gbLow; 
        GlassButton gbViewTraffic;

        public static event LocationChangedHandler OnLocationChanged; 
        public delegate void LocationChangedHandler(UITextField textField, GetLocationActions locActions, string lat, string lng);

        public override void ViewDidAppear (bool animated)
        {
            AppDelegate.navigation.ToolbarHidden = true;
            base.ViewDidAppear (animated);
        }

        public void GetLocation (GetLocationActions locActions, UITextField textField) 
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0)) {
                iPhoneLocationManager.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e) => {
                    UpdateLocation (e.Locations [e.Locations.Length - 1], iPhoneLocationManager, locActions, textField);
                };
            } else {
                // this won't be called on iOS 6 (deprecated)
                iPhoneLocationManager.UpdatedLocation += (object sender, CLLocationUpdatedEventArgs e) => {
                    UpdateLocation (e.NewLocation, iPhoneLocationManager, locActions, textField);
                };
            }

            // handle the updated heading method and update the UI
            iPhoneLocationManager.UpdatedHeading += (object sender, CLHeadingUpdatedEventArgs e) => {
                Console.WriteLine(e.NewHeading.MagneticHeading.ToString () + "º");
                Console.WriteLine(e.NewHeading.TrueHeading.ToString () + "º");
            };

            // start updating our location, et. al.
            if (CLLocationManager.LocationServicesEnabled)
                iPhoneLocationManager.StartUpdatingLocation ();
            if (CLLocationManager.HeadingAvailable)
                iPhoneLocationManager.StartUpdatingHeading ();
        }

        static public void UpdateLocation (CLLocation newLocation, CLLocationManager locManager, GetLocationActions locActions, UITextField textField)
        {
            Console.WriteLine(newLocation.Coordinate.Longitude.ToString () + "º");
            Console.WriteLine(newLocation.Coordinate.Latitude.ToString () + "º");

            //FireEvent
            OnLocationChanged (textField, locActions, newLocation.Coordinate.Latitude.ToString (), newLocation.Coordinate.Longitude.ToString ());

            if (CLLocationManager.LocationServicesEnabled)
                locManager.StopUpdatingLocation ();
            if (CLLocationManager.HeadingAvailable)
                locManager.StopUpdatingHeading ();
        }

        Dictionary<string, RectangleF> GetRectanglesForIphone ()
        {
            Dictionary<string, RectangleF> dicRect = new Dictionary<string, RectangleF>();
            int buttonHeight = 30;
            int spacingX = 5;

            int row1Y = 5;
            int row2Y = row1Y + buttonHeight + 10;

            //row1
            int subsX = spacingX + 50;
            int subsWidth = 100;
            dicRect.Add("blocked", new RectangleF (subsX, row1Y, subsWidth, buttonHeight));

            int pubX = subsX + subsWidth + spacingX;
            int pubWidth = 100;
            dicRect.Add("heavy", new RectangleF (pubX, row1Y, pubWidth, buttonHeight));

            //row2
            int histX = spacingX + 50;
            int histWidth = 100;
            dicRect.Add("normal", new RectangleF (histX, row2Y, histWidth, buttonHeight));

            int presX = histX + histWidth + spacingX;
            int presWidth = 100;
            dicRect.Add("low", new RectangleF (presX, row2Y, presWidth, buttonHeight));


            return dicRect;
        }

        void CheckChannel (TrafficUpdates.TrafficMessage trafficMessage)
        {
            gbBlocked.Enabled = false;
            gbHeavy.Enabled = false;
            gbNormal.Enabled = false;
            gbLow.Enabled = false;

            tu.GetStreetAddress (tfUpdateLocation.Text, trafficMessage);
        }

        UIView CreateHeaderView (Dictionary<string, RectangleF> dicRect, int iViewHeight)
        {
            UIView uiView = new UIView (new RectangleF (0, 0, this.View.Bounds.Width, iViewHeight));
            uiView.MultipleTouchEnabled = true;

            gbBlocked = new GlassButton (dicRect["blocked"]);
            gbBlocked.Font = font12;
            gbBlocked.SetTitle ("Blocked", UIControlState.Normal);
            gbBlocked.NormalColor = UIColor.Red;
            gbBlocked.Enabled = true;
            gbBlocked.Tapped += delegate{
                CheckChannel(TrafficUpdates.TrafficMessage.Blocked);
            };
            uiView.AddSubview (gbBlocked);

            gbHeavy = new GlassButton (dicRect["heavy"]);
            gbHeavy.Font = font12;
            gbHeavy.SetTitle ("Heavy", UIControlState.Normal);
            gbHeavy.Enabled = true;
            gbHeavy.NormalColor = UIColor.Orange;
            gbHeavy.Tapped += delegate{
                CheckChannel(TrafficUpdates.TrafficMessage.Heavy);
            };
            uiView.AddSubview (gbHeavy);

            gbNormal = new GlassButton (dicRect["normal"]);
            gbNormal.Font = font12;
            gbNormal.NormalColor = UIColor.FromRGB(0, 255, 0);
            gbNormal.SetTitle ("Normal", UIControlState.Normal);
            gbNormal.Enabled = true;
            gbNormal.Tapped += delegate{
                CheckChannel(TrafficUpdates.TrafficMessage.Normal);
            };

            uiView.AddSubview (gbNormal);

            gbLow = new GlassButton (dicRect["low"]);
            gbLow.Font = font12;
            gbLow.NormalColor = UIColor.Purple;
            gbLow.SetTitle ("Low", UIControlState.Normal);
            gbLow.Enabled = true;
            gbLow.Tapped += delegate{
                CheckChannel(TrafficUpdates.TrafficMessage.Low);
            };
            uiView.AddSubview (gbLow);

            return uiView;
        }

        UIView CreateButtonView(RectangleF rectF){
            UIView uiView = new UIView (rectF);
            uiView.MultipleTouchEnabled = true;

            gbViewTraffic = new GlassButton (new RectangleF ((this.View.Bounds.Width-100)/2, 0, 100, 35));
            gbViewTraffic.Font = font12;
            gbViewTraffic.NormalColor = UIColor.Blue;
            gbViewTraffic.SetTitle ("View Traffic", UIControlState.Normal);
            gbViewTraffic.Enabled = true;
            gbViewTraffic.Tapped += delegate{new TrafficViewDialogViewController(tu, tfOrigin.Text, tfDestination.Text);};
            uiView.AddSubview (gbViewTraffic);

            return uiView;
        }

        UIView CreateTextView(UITextField textField, string placeholderText, string text, RectangleF rectF, GetLocationActions locActions){
            UIView uiView = new UIView (rectF);
            uiView.MultipleTouchEnabled = true;

            textField.Frame = new RectangleF (20, 0, this.View.Bounds.Width-110, 30);
            textField.Font = font12;
            textField.Placeholder = placeholderText;
            textField.Text = text;
            textField.BorderStyle = UITextBorderStyle.RoundedRect;
            textField.Enabled = true;
            uiView.AddSubview (textField);


            GlassButton gbLoc = new GlassButton (new RectangleF (textField.Bounds.Width + 20 + 5, 0, 80, 30));
            if (locActions != GetLocationActions.None) {
                gbLoc.Font = font10;
                gbLoc.NormalColor = UIColor.Brown;
                gbLoc.SetTitle ("Use Location", UIControlState.Normal);
                gbLoc.Enabled = true;
                gbLoc.Tapped += delegate {
                    textField.Enabled = false;

                    if (locActions == GetLocationActions.UpdateTraffic) {
                        gbBlocked.Enabled = false;
                        gbHeavy.Enabled = false;
                        gbNormal.Enabled = false;
                        gbLow.Enabled = false;
                    } else if (locActions == GetLocationActions.SetOrigin) {
                        gbViewTraffic.Enabled = false;
                    } else if (locActions == GetLocationActions.SetDestination) {
                        gbViewTraffic.Enabled = false;
                    }
                    GetLocation (locActions, textField);
                };

                uiView.AddSubview (gbLoc);
            }

            return uiView;
        }

        public TrafficDialogViewController () : base (UITableViewStyle.Grouped, null)
        {
            tu = new TrafficUpdates();  
            OnLocationChanged += delegate(UITextField textField, GetLocationActions locActions, string lat, string lng) {
                textField.Enabled = true;

                if (locActions == GetLocationActions.UpdateTraffic) {
                    tu.ValidateLocation(string.Format ("{0},{1}", lat, lng), GetLocationActions.UpdateTraffic);
                } else if (locActions == GetLocationActions.SetOrigin) {
                    tu.ValidateLocation(string.Format ("{0},{1}", lat, lng), GetLocationActions.SetOrigin);
                    gbViewTraffic.Enabled = true;
                } else if (locActions == GetLocationActions.SetDestination) {
                    tu.ValidateLocation(string.Format ("{0},{1}", lat, lng), GetLocationActions.SetDestination);
                    gbViewTraffic.Enabled = true;
                }
            };
            tu.PostToChannel += delegate(string channel, TrafficUpdates.TrafficMessage trafficMessage) {
                AppDelegate.navigation.BeginInvokeOnMainThread(delegate {
                    pnChannel = channel;
                    if (string.IsNullOrWhiteSpace (pnChannel)) {
                        AppDelegate.navigation.BeginInvokeOnMainThread (delegate {
                            new UIAlertView ("Error!", "Please enter valid location", null, "OK").Show ();
                        });
                    } else {
                        tu.PublishMessage (pnChannel, trafficMessage);
                        new UIAlertView ("Success!", "Message published", null, "OK").Show ();
                    }

                    gbBlocked.Enabled = true;
                    gbHeavy.Enabled = true;
                    gbNormal.Enabled = true;
                    gbLow.Enabled = true;
                });
            };
            tu.AddressUpdate += delegate(string address, string lat, string lng, bool fromAddress, string polylinePoints, GetLocationActions locActions) {
                AppDelegate.navigation.BeginInvokeOnMainThread(delegate {


                    if (locActions == GetLocationActions.UpdateTraffic) {
                        tfUpdateLocation.Text = address;
                        gbBlocked.Enabled = true;
                        gbHeavy.Enabled = true;
                        gbNormal.Enabled = true;
                        gbLow.Enabled = true;
                    } else if (locActions == GetLocationActions.SetOrigin) {
                        tfOrigin.Text = address;
                        gbViewTraffic.Enabled = true;
                    } else if (locActions == GetLocationActions.SetDestination) {
                        tfDestination.Text = address;
                        gbViewTraffic.Enabled = true;
                    }
                });
            };

            UIView labelView = new UIView(new RectangleF (0, 0, this.View.Bounds.Width, 16));
            int left = 20;

            iPhoneLocationManager = new CLLocationManager ();
            iPhoneLocationManager.DesiredAccuracy = CLLocation.AccuracyHundredMeters; 

            labelView.AddSubview(new UILabel (new RectangleF (left, 0, this.View.Bounds.Width - left, 16)){
                Font = UIFont.BoldSystemFontOfSize(16),
                BackgroundColor = UIColor.Clear,
                TextColor = UIColor.FromRGB(76, 86, 108),
                Text = "Update traffic"
            });

            var dictRect = GetRectanglesForIphone();
            int viewHeight = 60;

            Section secAction = new Section ();
            secAction.HeaderView = CreateHeaderView(dictRect, viewHeight);
             

            tfUpdateLocation = new UITextField ();
            Section secUpdateLocation = new Section ();
            RectangleF rectFUpdateLocation = new RectangleF (left,  25, this.View.Bounds.Width - left, 26);
            secUpdateLocation.HeaderView = CreateTextView (tfUpdateLocation, "Enter Location address", "1100 16th Street, San Francisco, CA 94107, USA", rectFUpdateLocation, GetLocationActions.UpdateTraffic);

            tfNextStreet = new UITextField ();
            Section secNextStreet = new Section ();
            RectangleF rectFNextStreet = new RectangleF (left,  25, this.View.Bounds.Width - left, 26);
            secNextStreet.HeaderView = CreateTextView (tfNextStreet, "Next Street (direction of traffic)", "339 Arkansas Street, San Francisco, CA 94107, USA", rectFUpdateLocation, GetLocationActions.None);

            tfOrigin = new UITextField ();
            Section secOrigin = new Section ();
            RectangleF rectFOrigin = new RectangleF (left,  25, this.View.Bounds.Width - left, 26);
            secOrigin.HeaderView = CreateTextView (tfOrigin, "Enter Origin (can be lat, lng)", "Downtown, san francisco", rectFOrigin, GetLocationActions.SetOrigin);

            tfDestination = new UITextField ();
            Section secDestination = new Section ();
            RectangleF rectFDestination = new RectangleF (left,  25, this.View.Bounds.Width - left, 26);
            secDestination.HeaderView = CreateTextView (tfDestination, "Enter Destination (can be lat, lng)", "725 Folsom • San Francisco, CA 94107", rectFDestination, GetLocationActions.SetDestination);

            Section secViewTraffic = new Section ();
            RectangleF rectFViewTraffic = new RectangleF (left, 25, this.View.Bounds.Width - left, 26);
            secViewTraffic.HeaderView = CreateButtonView (rectFViewTraffic);


            Root = new RootElement ("Traffic") {
                new Section ("Update traffic") {
                },
                secUpdateLocation,
                secAction,

                new Section ("View traffic") {
                },
                secOrigin,
                secDestination,
                secViewTraffic,
            };
        }
    }
}