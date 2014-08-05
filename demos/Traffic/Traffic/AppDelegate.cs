using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Traffic
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register ("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;
        TrafficDialogViewController viewController;
        public static UINavigationController navigation;

        //
        // This method is invoked when the application has loaded and is ready to run. In this
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching (UIApplication   app, NSDictionary options)
        {

            UITabBarController tabBarController;

            window = new UIWindow (UIScreen.MainScreen.Bounds);

            //viewController = new TrafficDialogViewController();

            var dv = new TrafficDialogViewController (){
                Autorotate = true
            };

            var tap = new UITapGestureRecognizer ();
            tap.AddTarget (() =>{
                dv.View.EndEditing (true);
            });
            dv.View.AddGestureRecognizer (tap);

            tap.CancelsTouchesInView = false;


            navigation = new UINavigationController ();
            navigation.PushViewController (dv, true);

            window = new UIWindow (UIScreen.MainScreen.Bounds);
            window.MakeKeyAndVisible ();
            window.RootViewController = navigation;  

            return true;
        }
    }
}

