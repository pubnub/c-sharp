using System;
using CoreGraphics;
using UIKit;
using Foundation;

namespace MonoTouch.SlideoutNavigation
{
    /// <summary>
    /// Slideout view controller.
    /// </summary>
    public class SlideoutNavigationController : UIViewController
    {
        #region private attributes

        private readonly ProxyNavigationController _internalMenuViewLeft;
        private readonly ProxyNavigationController _internalMenuViewRight;
        private readonly UIViewController _internalTopView;
        private readonly UIPanGestureRecognizer _panGesture;
        private readonly UITapGestureRecognizer _tapGesture;
        private UIViewController _externalContentView;
        private UIViewController _externalMenuViewLeft;
        private UIViewController _externalMenuViewRight;
        private bool _ignorePan;
        private UINavigationController _internalTopNavigation;
        private nfloat _panOriginX;
        private bool _displayNavigationBarOnSideBarLeft;
        private bool _displayNavigationBarOnSideBarRight;
        private bool _shadowShown;
        private bool _leftMenuEnabled = true;
        private bool _rightMenuEnabled = false;
        private bool _leftMenuShowing = true;
        private bool _rightMenuShowing = true;
        private string _menuTextLeft = " Menu ";
        private string _menuTextRight = "Right Menu > ";

        #endregion private attributes

        #region public attributes

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>The color of the background.</value>
        public UIColor BackgroundColor {
            get {
                return _internalTopView.View.BackgroundColor;
            }
            set {
                _internalTopView.View.BackgroundColor = value;
            }
        }

        public float SlideHeight { get; set; }

        /// <summary>
        /// Gets or sets the current view.
        /// </summary>
        /// <value>
        /// The current view.
        /// </value>
        public UIViewController TopView {
            get { return _externalContentView; }
            set {
                if (_externalContentView == value)
                    return;
                SelectView (value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the left menu us enabled.
        /// If this is true then you can reach the menu. If false then all hooks to get to the menu view will be disabled.
        /// This is only necessary when you don't want the user to get to the menu.
        /// </summary>
        /// <value><c>true</c> if left menu enabled; otherwise, <c>false</c>.</value>
        public bool LeftMenuEnabled {
            get { return _leftMenuEnabled; }
            set {
                if (value == _leftMenuEnabled)
                    return;

                if (!value)
                    Hide ();

                if (_internalTopNavigation != null && _internalTopNavigation.ViewControllers.Length > 0) {
                    var view = _internalTopNavigation.ViewControllers [0];
                    view.NavigationItem.LeftBarButtonItem = value ? CreateLeftMenuButton () : null;
                }

                _leftMenuEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the right menu is enabled.
        /// If this is true then you can reach the menu. If false then all hooks to get to the menu view will be disabled.
        /// This is only necessary when you don't want the user to get to the menu.
        /// </summary>
        /// <value><c>true</c> if right menu enabled; otherwise, <c>false</c>.</value>
        public bool RightMenuEnabled {
            get { return _rightMenuEnabled; }
            set {
                if (value == _rightMenuEnabled)
                    return;

                if (!value)
                    Hide ();

                if (_internalTopNavigation != null && _internalTopNavigation.ViewControllers.Length > 0) {
                    var view = _internalTopNavigation.ViewControllers [0];
                    view.NavigationItem.RightBarButtonItem = value ? CreateRightMenuButton () : null;
                }

                _rightMenuEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the menu on the left side, also enables it, set LeftMenuEnabled to disable.
        /// </summary>
        /// <value>
        /// The list view.
        /// </value>
        public UIViewController MenuViewLeft {
            get { return _externalMenuViewLeft; }
            set {
                if (_externalMenuViewLeft == value)
                    return;
                _internalMenuViewLeft.SetController (value);
                _externalMenuViewLeft = value;
                LeftMenuEnabled = true;
            }
        }

        /// <summary>
        /// Gets or sets the menu on the right side, also enables it, set RightMenuEnabled to disable.
        /// </summary>
        /// <value>The menu view right.</value>
        public UIViewController MenuViewRight {
            get { return _externalMenuViewRight; }
            set {
                if (_externalMenuViewRight == value)
                    return;
                _internalMenuViewRight.SetController (value);
                _externalMenuViewRight = value;
                RightMenuEnabled = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether there should be shadowing effects on the top view
        /// </summary>
        /// <value>
        /// <c>true</c> if layer shadowing; otherwise, <c>false</c>.
        /// </value>
        public bool LayerShadowing { get; set; }

        /// <summary>
        /// Gets or sets the shadow opacity.
        /// </summary>
        /// <value>The shadow opacity.</value>
        public float ShadowOpacity { get; set; }

        /// <summary>
        /// Gets or sets the slide speed.
        /// </summary>
        /// <value>
        /// The slide speed.
        /// </value>
        public float SlideSpeed { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SlideoutNavigationController"/> is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public bool Visible { get; private set; }

        /// <summary>
        /// Gets or sets the width of the slide.
        /// </summary>
        /// <value>
        /// The width of the slide.
        /// </value>
        public float SlideWidth { get; set; }

        /// <summary>
        /// Gets or sets the left menu button text.
        /// </summary>
        /// <value>The left menu button text.</value>
        public string LeftMenuButtonText { get { return _menuTextLeft; } set { _menuTextLeft = value; } }

        /// <summary>
        /// Gets or sets the right menu button text.
        /// </summary>
        /// <value>The right menu button text.</value>
        public string RightMenuButtonText { get { return _menuTextRight; } set { _menuTextRight = value; } }

        /// <summary>
        /// Gets or sets a value indicating whether the navigation bar is shown on the left menu.
        /// </summary>
        /// <value><c>true</c> if display navigation bar on left menu; otherwise, <c>false</c>.</value>
        public bool DisplayNavigationBarOnLeftMenu { 
            get { return _displayNavigationBarOnSideBarLeft; } 
            set { 
                _displayNavigationBarOnSideBarLeft = value; 
                _internalMenuViewLeft.SetNavigationBarHidden (!value, false);
            } 
        }

        /// <summary>
        /// Gets or sets a value indicating whether the navigation bar is shown on the right menu.
        /// </summary>
        /// <value><c>true</c> if display navigation bar on right menu; otherwise, <c>false</c>.</value>
        public bool DisplayNavigationBarOnRightMenu { 
            get { return _displayNavigationBarOnSideBarRight; } 
            set { 
                _displayNavigationBarOnSideBarRight = value; 
                _internalMenuViewRight.SetNavigationBarHidden (!value, false);
            } 
        }

        #endregion public attributes

        /// <summary>
        /// Initializes a new instance of the <see cref="SlideoutNavigationController"/> class.
        /// </summary>
        public SlideoutNavigationController ()
        {
            SlideSpeed = 0.2f;
            SlideWidth = 245f;
            SlideHeight = 44f;
            LayerShadowing = false;
            ShadowOpacity = 0.5f;

            _internalMenuViewLeft = new ProxyNavigationController {
                ParentController = this,
                View = { AutoresizingMask = UIViewAutoresizing.FlexibleHeight }
            };
            _internalMenuViewRight = new ProxyNavigationController {
                ParentController = this,
                View = { AutoresizingMask = UIViewAutoresizing.FlexibleHeight }
            };

            _internalMenuViewLeft.SetNavigationBarHidden (DisplayNavigationBarOnLeftMenu, false);
            _internalMenuViewRight.SetNavigationBarHidden (DisplayNavigationBarOnRightMenu, false);

            _internalTopView = new UIViewController { View = { UserInteractionEnabled = true } };
            _internalTopView.View.Layer.MasksToBounds = false;

            _tapGesture = new UITapGestureRecognizer ();
            _tapGesture.AddTarget (() => Hide ());
            _tapGesture.NumberOfTapsRequired = 1;

            _panGesture = new UIPanGestureRecognizer {
                Delegate = new SlideoutPanDelegate (this),
                MaximumNumberOfTouches = 1,
                MinimumNumberOfTouches = 1
            };
            _panGesture.AddTarget (() => Pan (_internalTopView.View));
            _internalTopView.View.AddGestureRecognizer (_panGesture);
        }

        /// <summary>
        /// Pan the specified view.
        /// </summary>
        /// <param name='view'>
        /// View.
        /// </param>
        private void Pan (UIView view)
        {
            if (_panGesture.State == UIGestureRecognizerState.Began) {
                _panOriginX = view.Frame.X;
                _ignorePan = false;

                if (!Visible) {
                    CGPoint touch = _panGesture.LocationOfTouch (0, view);
                    if (touch.Y > SlideHeight || _internalTopNavigation.NavigationBarHidden)
                        _ignorePan = true;
                }
            } else if (!_ignorePan && (_panGesture.State == UIGestureRecognizerState.Changed)) {
                nfloat t = _panGesture.TranslationInView (view).X;

                if (RightMenuEnabled && _panOriginX + t < 0) {
                    HideLeft ();
                    ShowRight ();
                } else if (LeftMenuEnabled && _panOriginX + t > 0) {
                    HideRight ();
                    ShowLeft ();
                }

                if (t < -SlideWidth) {
                    t = -SlideWidth;
                } else if (t > SlideWidth) {
                    t = SlideWidth;
                } else if ((Visible && _rightMenuShowing && t < 0) || (Visible && _leftMenuShowing && t > 0)) {
                    t = 0;
                }

                if ((LeftMenuEnabled && (_panOriginX + t) >= 0) || (RightMenuEnabled && (_panOriginX + t) <= 0))
                    view.Frame = new CGRect (_panOriginX + t, view.Frame.Y, view.Frame.Width, view.Frame.Height);

                ShowShadowWhileDragging ();
            } else if (!_ignorePan &&
                       (_panGesture.State == UIGestureRecognizerState.Ended ||
                       _panGesture.State == UIGestureRecognizerState.Cancelled)) {
                nfloat velocity = _panGesture.VelocityInView (view).X;

                if (Visible) {
                    if ((view.Frame.X < (view.Frame.Width / 2) && _leftMenuShowing) || (view.Frame.X > -(view.Frame.Width / 2) && _rightMenuShowing))
                        Hide ();
                    else if (_leftMenuShowing) {
                        UIView.Animate (SlideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
                            () => {
                                view.Frame = new CGRect (SlideWidth, view.Frame.Y, view.Frame.Width, view.Frame.Height);
                            }, () => {
                        });
                    } else if (_rightMenuShowing) {
                        UIView.Animate (SlideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
                            () => {
                                view.Frame = new CGRect (-SlideWidth, view.Frame.Y, view.Frame.Width, view.Frame.Height);
                            }, () => {
                        });
                    }
                } else {
                    if (velocity > 800.0f || (view.Frame.X > (view.Frame.Width / 2))) {
                        if (LeftMenuEnabled)
                            ShowMenuLeft ();
                    } else if (velocity < -800.0f || (view.Frame.X < -(view.Frame.Width / 2))) {
                        if (RightMenuEnabled)
                            ShowMenuRight ();
                    } else {
                        UIView.Animate (SlideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
                            () => {
                                view.Frame = new CGRect (0, 0, view.Frame.Width, view.Frame.Height);
                            }, () => {
                        });
                    }
                }
            }
        }

        /// <Docs>
        /// Called after the controller’s view is loaded into memory.
        /// </Docs>
        /// <summary>
        /// Views the did load.
        /// </summary>
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            _internalTopView.View.Frame = new CGRect (0, 0, View.Frame.Width, View.Frame.Height);
            _internalMenuViewLeft.View.Frame = new CGRect (0, 0, SlideWidth, View.Frame.Height);
            _internalMenuViewRight.View.Frame = new CGRect (View.Frame.Width - SlideWidth, 0, SlideWidth, View.Frame.Height);

            //Add the list View
            AddChildViewController (_internalMenuViewLeft);
            AddChildViewController (_internalMenuViewRight);
            View.AddSubview (_internalMenuViewLeft.View);
            View.AddSubview (_internalMenuViewRight.View);

            //Add the parent view
            AddChildViewController (_internalTopView);
            View.AddSubview (_internalTopView.View);
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            if (NavigationController != null)
                NavigationController.SetNavigationBarHidden (true, true);
        }

        public override void ViewWillDisappear (bool animated)
        {
            base.ViewWillDisappear (animated);
            if (NavigationController != null)
                NavigationController.SetNavigationBarHidden (false, true);
        }

        /// <summary>
        /// Shows the shadow of the left side of the top view.
        /// </summary>
        private void ShowShadowLeft ()
        {
            ShowShadow (-5);
        }

        /// <summary>
        /// Shows the shadow of the right side of the top view.
        /// </summary>
        private void ShowShadowRight ()
        {
            ShowShadow (5);
        }

        /// <summary>
        /// Shows the shadow of the top view while dragging.
        /// </summary>
        private void ShowShadowWhileDragging ()
        {
            if (!LayerShadowing)
                return;

            _internalTopView.View.Layer.ShadowPath = UIBezierPath.FromRect (_internalTopView.View.Bounds).CGPath;
            _internalTopView.View.Layer.ShadowRadius = 4.0f;
            _internalTopView.View.Layer.ShadowOpacity = ShadowOpacity;
            _internalTopView.View.Layer.ShadowColor = UIColor.Black.CGColor;
        }

        private void ShowShadow (float position)
        {
            //Dont need to call this twice if its already shown
            if (!LayerShadowing || _shadowShown)
                return;

            _internalTopView.View.Layer.ShadowOffset = new CGSize (position, 0);
            _internalTopView.View.Layer.ShadowPath = UIBezierPath.FromRect (_internalTopView.View.Bounds).CGPath;
            _internalTopView.View.Layer.ShadowRadius = 4.0f;
            _internalTopView.View.Layer.ShadowOpacity = ShadowOpacity;
            _internalTopView.View.Layer.ShadowColor = UIColor.Black.CGColor;

            _shadowShown = true;
        }

        /// <summary>
        /// Hides the shadow of the top view
        /// </summary>
        private void HideShadow ()
        {
            //Dont need to call this twice if its already hidden
            if (!LayerShadowing || !_shadowShown)
                return;

            _internalTopView.View.Layer.ShadowOffset = new CGSize (0, 0);
            _internalTopView.View.Layer.ShadowRadius = 0.0f;
            _internalTopView.View.Layer.ShadowOpacity = 0.0f;
            _internalTopView.View.Layer.ShadowColor = UIColor.Clear.CGColor;
            _shadowShown = false;
        }

        /// <summary>
        /// Open the left menu programmaticly.
        /// </summary>
        public void ShowMenuLeft ()
        {
            //Don't show if already shown
            if (Visible)
                return;
            Visible = true;

            ShowLeft ();
            HideRight ();
            //Show some shadow!
            ShowShadowLeft ();

            _internalMenuViewLeft.View.Frame = new CGRect (0, 0, SlideWidth, View.Frame.Height);
            if (MenuViewLeft != null)
                MenuViewLeft.ViewWillAppear (true);

            UIView view = _internalTopView.View;
            UIView.Animate (SlideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
                () => {
                    view.Frame = new CGRect (SlideWidth, 0, view.Frame.Width, view.Frame.Height);
                },
                () => {
                    if (view.Subviews.Length > 0)
                        view.Subviews [0].UserInteractionEnabled = false;
                    view.AddGestureRecognizer (_tapGesture);

                    if (MenuViewLeft != null)
                        MenuViewLeft.ViewDidAppear (true);
                });
        }

        /// <summary>
        /// Shows the left menu view, this is done to prevent the two menu's from being displayed at the same time.
        /// </summary>
        private void ShowLeft ()
        {
            if (_leftMenuShowing)
                return;
            _internalMenuViewLeft.View.Hidden = false;
            _leftMenuShowing = true;
        }

        /// <summary>
        /// Hides the left menu view, this is done to prevent the two menu's from being displayed at the same time.
        /// </summary>
        private void HideLeft ()
        {
            if (!_leftMenuShowing)
                return;
            _internalMenuViewLeft.View.Hidden = true;
            _leftMenuShowing = false;
        }

        /// <summary>
        /// Open the right menu programmaticly
        /// </summary>
        public void ShowMenuRight ()
        {
            if (Visible)
                return;
            Visible = true;

            ShowRight ();
            HideLeft ();

            ShowShadowRight ();

            _internalMenuViewRight.View.Frame = new CGRect (View.Frame.Width - SlideWidth, 0, SlideWidth, View.Frame.Height);
            if (MenuViewRight != null)
                MenuViewRight.ViewWillAppear (true);

            UIView view = _internalTopView.View;
            UIView.Animate (SlideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
                () => {
                    view.Frame = new CGRect (-SlideWidth, 0, view.Frame.Width, view.Frame.Height);
                },
                () => {
                    if (view.Subviews.Length > 0)
                        view.Subviews [0].UserInteractionEnabled = false;
                    view.AddGestureRecognizer (_tapGesture);
                    if (MenuViewRight != null)
                        MenuViewRight.ViewDidAppear (true);
                });
        }

        /// <summary>
        /// Shows the right menu view, this is done to prevent the two menu's from being displayed at the same time.
        /// </summary>
        private void ShowRight ()
        {
            if (_rightMenuShowing)
                return;
            _internalMenuViewRight.View.Hidden = false;
            _rightMenuShowing = true;
        }

        /// <summary>
        /// Hides the right menu view, this is done to prevent the two menu's from being displayed at the same time.
        /// </summary>
        private void HideRight ()
        {
            if (!_rightMenuShowing)
                return;
            _internalMenuViewRight.View.Hidden = true;
            _rightMenuShowing = false;
        }

        /// <summary>
        /// Creates the menu button for the left side.
        /// </summary>
        protected virtual UIBarButtonItem CreateLeftMenuButton ()
        {
            return new UIBarButtonItem (LeftMenuButtonText, UIBarButtonItemStyle.Plain, (s, e) => ShowMenuLeft ());
        }

        /// <summary>
        /// Creates the menu button for the right side.
        /// </summary>
        protected virtual UIBarButtonItem CreateRightMenuButton ()
        {
            return new UIBarButtonItem (RightMenuButtonText, UIBarButtonItemStyle.Plain, (s, e) => ShowMenuRight ());
        }

        /// <summary>
        /// Selects the view.
        /// </summary>
        /// <param name='view'>
        /// View.
        /// </param>
        public void SelectView (UIViewController view)
        {
            if (_internalTopNavigation != null) {
                _internalTopNavigation.RemoveFromParentViewController ();
                _internalTopNavigation.View.RemoveFromSuperview ();
                _internalTopNavigation.Dispose ();
            }

            _internalTopNavigation = new UINavigationController (view) {
                View = {
                    Frame = new CGRect (0, 0,
                        _internalTopView.View.Frame.Width,
                        _internalTopView.View.Frame.Height)
                }
            };
            _internalTopView.AddChildViewController (_internalTopNavigation);
            _internalTopView.View.AddSubview (_internalTopNavigation.View);

            if (LeftMenuEnabled)
                view.NavigationItem.LeftBarButtonItem = CreateLeftMenuButton ();
            if (RightMenuEnabled)
                view.NavigationItem.RightBarButtonItem = CreateRightMenuButton ();

            _externalContentView = view;

            Hide ();
        }

        /// <summary>
        /// Hide the menu's and returns the topview to the center.
        /// </summary>
        public void Hide (bool animate = true)
        {
            //Don't hide if its not visible.
            if (!Visible)
                return;
            Visible = false;

            UIView view = _internalTopView.View;

            Action animation = () => {
                view.Frame = new CGRect (0, 0, view.Frame.Width, view.Frame.Height);
            };
            Action finished = () => {
                if (view.Subviews.Length > 0)
                    view.Subviews [0].UserInteractionEnabled = true;
                view.RemoveGestureRecognizer (_tapGesture);
                //Hide the shadow when not needed to increase performance of the top layer!
                HideShadow ();
            };

            if (animate)
                UIView.Animate (SlideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut, animation, finished);
            else {
                animation ();
                finished ();
            }
        }

        /// <summary>
        /// Shoulds the autorotate to interface orientation.
        /// </summary>
        /// <returns>
        /// The autorotate to interface orientation.
        /// </returns>
        /// <param name='toInterfaceOrientation'>
        /// If set to <c>true</c> to interface orientation.
        /// </param>
        public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
        {
            return true;
        }

        /// <summary>
        /// Sets the menu navigation background image.
        /// </summary>
        /// <param name='image'>Image to be displayed as the background</param>
        /// <param name='metrics'>Metrics.</param>
        public void SetMenuNavigationBackgroundImage (UIImage image, UIBarMetrics metrics)
        {
            _internalMenuViewLeft.NavigationBar.SetBackgroundImage (image, metrics);
            _internalMenuViewRight.NavigationBar.SetBackgroundImage (image, metrics);
        }

        /// <summary>
        /// Sets the top view navigation background image.
        /// </summary>
        /// <param name='image'>Image to be displayed as the background</param>
        /// <param name='metrics'>Metrics.</param>
        public void SetTopNavigationBackgroundImage (UIImage image, UIBarMetrics metrics)
        {
            _internalTopNavigation.NavigationBar.SetBackgroundImage (image, metrics);
        }

        #region Nested type: ProxyNavigationController

        ///<summary>
        /// A proxy class for the navigation controller.
        /// This allows the menu view to make requests to the navigation controller
        /// and have them forwarded to the topview.
        ///</summary>
        private class ProxyNavigationController : UINavigationController
        {
            /// <summary>
            /// Gets or sets the parent controller.
            /// </summary>
            /// <value>
            /// The parent controller.
            /// </value>
            public SlideoutNavigationController ParentController { get; set; }

            /// <summary>
            /// Sets the controller.
            /// </summary>
            /// <param name='viewController'>
            /// View controller.
            /// </param>
            public void SetController (UIViewController viewController)
            {
                base.PopToRootViewController (false);
                base.PushViewController (viewController, false);
            }

            /// <Docs>
            /// To be added.
            /// </Docs>
            /// <summary>
            /// To be added.
            /// </summary>
            /// <param name='viewController'>
            /// View controller.
            /// </param>
            /// <param name='animated'>
            /// Animated.
            /// </param>
            public override void PushViewController (UIViewController viewController, bool animated)
            {
                ParentController.SelectView (viewController);
            }
        }

        #endregion

        #region Nested type: SlideoutPanDelegate

        ///<summary>
        /// A custom UIGestureRecognizerDelegate activated only when the controller 
        /// is visible or touch is within the 44.0f boundary.
        /// 
        /// Special thanks to Gerry High for this snippet!
        ///</summary>
        private class SlideoutPanDelegate : UIGestureRecognizerDelegate
        {
            private readonly SlideoutNavigationController _controller;

            public SlideoutPanDelegate (SlideoutNavigationController controller)
            {
                _controller = controller;
            }

            public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
            {
                return (_controller.Visible ||
                (touch.LocationInView (_controller._internalTopView.View).Y <= _controller.SlideHeight)) && (_controller.LeftMenuEnabled || _controller.RightMenuEnabled);
            }
        }

        #endregion
    }
}
