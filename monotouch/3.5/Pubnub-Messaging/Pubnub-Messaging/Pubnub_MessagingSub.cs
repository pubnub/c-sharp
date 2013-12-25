using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Threading;
using System.Drawing;
using MonoTouch.ObjCRuntime;
using PubNubMessaging.Core;
using MonoTouch.SlideoutNavigation;

namespace PubnubMessaging
{
    public class LeftNavController : DialogViewController
    {
        Pubnub_MessagingSub pubnub_MessagingSub;
        SlideoutNavigationController menu;
        public LeftNavController (SlideoutNavigationController menu, Pubnub_MessagingSub pubnub_MessagingSub) 
            : base(UITableViewStyle.Plain,new RootElement(""))
        {
            this.pubnub_MessagingSub = pubnub_MessagingSub;
            this.menu = menu;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            Root.Add (new Section () {
                new StyledStringElement("Subscribe", () => {menu.Hide(true); pubnub_MessagingSub.Subscribe();}),
                new StyledStringElement("Publish", () => {menu.Hide(true); pubnub_MessagingSub.Publish();}),
                new StyledStringElement("Presence", () => {menu.Hide(true); pubnub_MessagingSub.Presence();}),
                new StyledStringElement("Detailed History", () => {menu.Hide(true); pubnub_MessagingSub.DetailedHistory();}),
                new StyledStringElement("Here Now", () => {menu.Hide(true); pubnub_MessagingSub.HereNow();}),
                new StyledStringElement("Time", () => {menu.Hide(true); pubnub_MessagingSub.GetTime();}),
                new StyledStringElement("Unsubscribe", () => {menu.Hide(true); pubnub_MessagingSub.Unsub();}),
                new StyledStringElement("Presence-Unubscribe", () => {menu.Hide(true); pubnub_MessagingSub.UnsubPresence();}),
                new StyledStringElement("Subscribe Grant", () => {menu.Hide(true); pubnub_MessagingSub.SubscribeGrant();}),
                new StyledStringElement("Subscribe Audit", () => {menu.Hide(true); pubnub_MessagingSub.SubscribeAudit();}),
                new StyledStringElement("Subscribe Revoke", () => {menu.Hide(true); pubnub_MessagingSub.SubscribeRevoke();}),
                new StyledStringElement("Presence Grant", () => {menu.Hide(true); pubnub_MessagingSub.PresenceGrant();}),
                new StyledStringElement("Presence Audit", () => {menu.Hide(true); pubnub_MessagingSub.PresenceAudit();}),
                new StyledStringElement("Presence Revoke", () => {menu.Hide(true); pubnub_MessagingSub.PresenceRevoke();}),
                new StyledStringElement("Auth Key", () => {menu.Hide(true); pubnub_MessagingSub.AuthKey();}),

            });
        }
    }
    public partial class Pubnub_MessagingSub : DialogViewController
    {
        Pubnub pubnub;
        public SlideoutNavigationController Menu { get; private set; }

        string Channel {
            get;set;
        }

        string Cipher {
            get;set;    
        }

        bool Ssl {
            get;set;
        }

        DialogViewController dvc;
        RootElement root;
        Section secOutput;
        UIFont font12 = UIFont.SystemFontOfSize (12);
        UIFont font13 = UIFont.SystemFontOfSize (13);
        public bool showErrorMessageSegments = true;
        UITextField tfChannels;
        UITextField newChannels;

        public Pubnub_MessagingSub (string channelName, string cipher, bool enableSSL, Pubnub pubnub) 
            : base (UITableViewStyle.Grouped, null)
        {
            Channel = channelName;
            Ssl = enableSSL;
            Cipher = cipher;
            this.pubnub = pubnub;

            string strSsl = "";
            if (Ssl) {
                strSsl = "SSL,";
            }
            
            string strCip = "";
            if (!String.IsNullOrWhiteSpace (Cipher)) {
                strCip = "Cipher";
            }
            
            string head = String.Format ("{0} {1}", strSsl, strCip);

            Section secAction = new Section ();
            
            bool bIphone = true;
            
            int viewHeight = 70;
            
            secAction.HeaderView = CreateHeaderView(viewHeight);
            
            secOutput = new Section("Output");
            
            root = new RootElement (head) {
                secAction,
                secOutput
            };

            Root = root;
            dvc = new DialogViewController (root, true);
            var tap = new UITapGestureRecognizer ();
            tap.AddTarget (() =>{
                dvc.View.EndEditing (true);
            });
            dvc.View.AddGestureRecognizer (tap);

            tap.CancelsTouchesInView = false;
            dvc.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, delegate {
                pubnub.EndPendingRequests ();
                AppDelegate.navigation.PopToRootViewController(true);
            });
            Menu = new SlideoutNavigationController ();
            Menu.TopView = dvc;

            Menu.MenuViewLeft = new LeftNavController (Menu, this);

            AppDelegate.navigation.PushViewController (Menu, true);
            Menu.ShowMenuLeft ();
            newChannels.Text = Channel;
        }

        UIView CreateHeaderView (int iViewHeight)
        {
            UIView uiView = new UIView (new RectangleF (0, 0, this.View.Bounds.Width, iViewHeight));
            uiView.MultipleTouchEnabled = true;

            /*UILabel lblChannel = new UILabel(new RectangleF (10, 2, 80, 25));
            lblChannel.Font = font13;
            lblChannel.Text = "Channel(s):";
            uiView.Add (lblChannel);

            tfChannels = new UITextField (new RectangleF (90, 2, 200, 25));
            tfChannels.Enabled = false;
            tfChannels.BackgroundColor = UIColor.FromRGB(239, 239, 244);
            tfChannels.Font = font12;
            uiView.Add (tfChannels);*/

            UILabel lblInfo = new UILabel(new RectangleF (10, 2, 300, 25));
            lblInfo.Font = font12;
            lblInfo.Text = "Enter new channel(s) and/or use the menu for actions";
            uiView.Add (lblInfo);

            UILabel lblNewChannel = new UILabel(new RectangleF (10, 32, 100, 25));
            lblNewChannel.Font = font13;
            lblNewChannel.Text = "New Channel(s):";
            uiView.Add (lblNewChannel);

            newChannels = new UITextField (new RectangleF (120, 32, 185, 25));
            newChannels.AutocorrectionType = UITextAutocorrectionType.No;
            newChannels.BackgroundColor = UIColor.White;
            newChannels.Font = font12;
            uiView.Add (newChannels);

            return uiView;
        }

        public void Subscribe()
        {
            Display("Running Subscribe");
            pubnub.Subscribe<string>(Channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);
        }

        public void Publish()
        {
            UIAlertView alert = new UIAlertView ();
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            alert.Title = "Publish";
            alert.Message = "Enter message to publish";
            alert.AddButton ("Publish");
            alert.AddButton ("Cancel");
            alert.Clicked += delegate(object sender, UIButtonEventArgs e) {
                if(e.ButtonIndex == 0)
                {
                    if(alert.GetTextField(0) != null){
                        string input = alert.GetTextField(0).Text;
                        Display("Running Publish");
                        string[] channels = Channel.Split(',');
                        foreach (string channel in channels)
                        {
                            pubnub.Publish<string> (channel.Trim(), input, DisplayReturnMessage, DisplayErrorMessage);
                        }
                    }
                }           
            };
            alert.Show();
        }

        void PublishAlertDismissed (object sender, UIButtonEventArgs e)
        {
            InputAlertView iav = (InputAlertView)sender;
            if ((iav != null) && (!String.IsNullOrWhiteSpace (iav.EnteredText))) {
                Display("Running Publish");
                string[] channels = Channel.Split(',');
                foreach (string channel in channels)
                {
                    pubnub.Publish<string> (channel.Trim(), iav.EnteredText, DisplayReturnMessage, DisplayErrorMessage);
                }
            }
        }
        
        public void Presence()
        {
            Display("Running Presence");
            pubnub.Presence<string>(Channel, DisplayReturnMessage, null, DisplayErrorMessage);
        }
        
        public void DetailedHistory ()
        {
            Display ("Running Detailed History");
            string[] channels = Channel.Split (',');
            foreach (string channel in channels) {
                pubnub.DetailedHistory<string> (channel.Trim(), 100, DisplayReturnMessage, DisplayErrorMessage);
            }
        }
        
        public void HereNow ()
        {
            Display ("Running Here Now");
            string[] channels = Channel.Split (',');
            foreach (string channel in channels) {
                pubnub.HereNow<string> (channel.Trim(), DisplayReturnMessage, DisplayErrorMessage);
            }
        }
        
        public void Unsub()
        {
            Display("Running unsubscribe");
            pubnub.Unsubscribe<string>(Channel, DisplayReturnMessage, 
                                       DisplayConnectStatusMessage, DisplayConnectStatusMessage, 
                                       DisplayErrorMessage);
        }

        public void UnsubPresence()
        {
            Display("Running presence-unsubscribe");
            pubnub.PresenceUnsubscribe<string>(Channel, DisplayReturnMessage, 
                                               DisplayConnectStatusMessage, DisplayConnectStatusMessage, 
                                               DisplayErrorMessage);
        }

        public void GetTime()
        {
            Display("Running Time");
            pubnub.Time<string>(DisplayReturnMessage, DisplayErrorMessage);
        }

        public void SubscribeGrant()
        {
            RunGrant (false);
        }

        public void SubscribeAudit()
        {
            Display("Running Subscribe Audit");
            pubnub.AuditAccess<string>(Channel,DisplayReturnMessage, DisplayErrorMessage);
        }

        public void SubscribeRevoke()
        {
            Display("Running Subscribe Revoke");
            pubnub.GrantAccess<string>(Channel, false,false, DisplayReturnMessage, DisplayErrorMessage);
        }

        public void PresenceGrant()
        {
            RunGrant (true);
        }

        public void PresenceAudit()
        {
            Display("Running Presence Audit");
            pubnub.AuditPresenceAccess<string>(Channel, DisplayReturnMessage, DisplayErrorMessage);
        }

        public void PresenceRevoke()
        {
            Display("Running Presence Revoke");
            pubnub.GrantPresenceAccess<string>(Channel, false, false, DisplayReturnMessage, DisplayErrorMessage);
        }

        public void AuthKey()
        {
            UIAlertView alert = new UIAlertView ();
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            alert.Title = "Auth Key";
            alert.Message = "Enter Auth Key";
            alert.AddButton ("Set");
            alert.AddButton ("Cancel");
            alert.Clicked += delegate(object sender, UIButtonEventArgs e) {
                if(e.ButtonIndex == 0)
                {
                    if(alert.GetTextField(0) != null){
                        string input = alert.GetTextField(0).Text;
                        pubnub.AuthenticationKey = input;
                        Display("Auth Key Set");
                    }
                }           
            };
            alert.Show();
        }

        void RunGrant (bool isPresenceGrant)
        {
            BooleanElement canRead = new BooleanElement ("Read", false);
            BooleanElement canWrite = new BooleanElement ("Write", false);
            EntryElement entryTtl = new EntryElement("TTL", "Enter TTL (default 1440)", "");
            entryTtl.KeyboardType = UIKeyboardType.NumberPad;

            string strHead = "Subscribe Grant";
            if (isPresenceGrant) {
                strHead = "Presence Grant";
            }

            var newroot = new RootElement (strHead, 0, 0){
                new Section (){
                    canRead,
                    canWrite,
                    entryTtl
                },
                new Section (""){
                    new StyledStringElement ("Grant", () => {
                        int iTtl;

                        Int32.TryParse(entryTtl.Value, out iTtl);
                        if (iTtl == 0) 
                        {
                            iTtl = 1440;
                            entryTtl.Value = "1440";
                        }

                        if (isPresenceGrant) {
                            Display("Running Presence Grant");
                            pubnub.GrantPresenceAccess<string>(Channel, canRead.Value, canWrite.Value, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                        }else{
                            Display("Running Subscribe Grant");
                            pubnub.GrantAccess<string>(Channel, canRead.Value, canWrite.Value, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        AppDelegate.navigation.PopViewControllerAnimated(true);
                    })
                    {
                        BackgroundColor = UIColor.Blue,
                        TextColor = UIColor.White,
                        Alignment = UITextAlignment.Center
                    },
                },
            };
            dvc = new DialogViewController (newroot, true);
            AppDelegate.navigation.PushViewController (dvc, true);
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayErrorMessage(string result)
        {
            Display(String.Format("Error Callback - {0}", result));
        }


        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayConnectStatusMessage(string result)
        {
            Display(String.Format("Connect Callback - {0}", result));
        }
        
        public void Display (string strText)
        {
            StyledMultilineElement sme = new StyledMultilineElement (strText)
            {
                Font = font12
            };
            ThreadPool.QueueUserWorkItem (delegate {
                
                System.Threading.Thread.Sleep(2000);
                
                AppDelegate.navigation.BeginInvokeOnMainThread(delegate {
                    if(secOutput.Count > 20)
                    {
                        secOutput.RemoveRange(0, 10);
                    }
                    if (secOutput.Count > 0) {
                        secOutput.Insert (secOutput.Count, sme);                    }
                    else
                    {
                        secOutput.Add (sme);
                    }
                    this.TableView.ReloadData();
                    var lastIndexPath = this.root.Last()[this.root.Last().Count-1].IndexPath;
                    this.TableView.ScrollToRow(lastIndexPath, UITableViewScrollPosition.Middle, true);    
                });
            });
        }

        void DisplayReturnMessage(string result)
        {
            Display (result);
        }

        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="result"></param>
        void DisplayErrorMessage(PubnubClientError result)
        {
            Console.WriteLine();
            Console.WriteLine(result.Description);
            Console.WriteLine();

            switch (result.StatusCode)
            {
            case 103:
                //Warning: Verify origin host name and internet connectivity
                break;
            case 104:
                //Critical: Verify your cipher key
                break;
            case 106:
                //Warning: Check network/internet connection
                break;
            case 108:
                //Warning: Check network/internet connection
                break;
            case 109:
                //Warning: No network/internet connection. Please check network/internet connection
                break;
            case 110:
                //Informational: Network/internet connection is back. Active subscriber/presence channels will be restored.
                break;
            case 111:
                //Informational: Duplicate channel subscription is not allowed. Internally Pubnub API removes the duplicates before processing.
                break;
            case 112:
                //Informational: Channel Already Subscribed/Presence Subscribed. Duplicate channel subscription not allowed
                break;
            case 113:
                //Informational: Channel Already Presence-Subscribed. Duplicate channel presence-subscription not allowed
                break;
            case 114:
                //Warning: Please verify your cipher key
                break;
            case 115:
                //Warning: Protocol Error. Please contact PubNub with error details.
                break;
            case 116:
                //Warning: ServerProtocolViolation. Please contact PubNub with error details.
                break;
            case 117:
                //Informational: Input contains invalid channel name
                break;
            case 118:
                //Informational: Channel not subscribed yet
                break;
            case 119:
                //Informational: Channel not subscribed for presence yet
                break;
            case 120:
                //Informational: Incomplete unsubscribe. Try again for unsubscribe.
                break;
            case 121:
                //Informational: Incomplete presence-unsubscribe. Try again for presence-unsubscribe.
                break;
            case 122:
                //Informational: Network/Internet connection not available. C# client retrying again to verify connection. No action is needed from your side.
                break;
            case 123:
                //Informational: During non-availability of network/internet, max retries for connection were attempted. So unsubscribed the channel.
                break;
            case 124:
                //Informational: During non-availability of network/internet, max retries for connection were attempted. So presence-unsubscribed the channel.
                break;
            case 125:
                //Informational: Publish operation timeout occured.
                break;
            case 126:
                //Informational: HereNow operation timeout occured
                break;
            case 127:
                //Informational: Detailed History operation timeout occured
                break;
            case 128:
                //Informational: Time operation timeout occured
                break;
            case 4000:
                //Warning: Message too large. Your message was not sent. Try to send this again smaller sized
                break;
            case 4001:
                //Warning: Bad Request. Please check the entered inputs or web request URL
                break;
            case 4002:
                //Warning: Invalid Key. Please verify the publish key
                break;
            case 4010:
                //Critical: Please provide correct subscribe key. This corresponds to a 401 on the server due to a bad sub key
                break;
            case 4020:
                // PAM is not enabled. Please contact PubNub support
                break;
            case 4030:
                //Warning: Not authorized. Check the permimissions on the channel. Also verify authentication key, to check access.
                break;
            case 4031:
                //Warning: Incorrect public key or secret key.
                break;
            case 4140:
                //Warning: Length of the URL is too long. Reduce the length by reducing subscription/presence channels or grant/revoke/audit channels/auth key list
                break;
            case 5000:
                //Critical: Internal Server Error. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                break;
            case 5020:
                //Critical: Bad Gateway. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                break;
            case 5040:
                //Critical: Gateway Timeout. No response from server due to PubNub server timeout. Please try again. If same problem persists, please contact PubNub support
                break;
            case 0:
                //Undocumented error. Please contact PubNub support with full error object details for further investigation
                break;
            default:
                break;
            }
            if (showErrorMessageSegments)
            {
                DisplayErrorMessageSegments(result);
                Console.WriteLine();
            }
        }

        void DisplayErrorMessageSegments(PubnubClientError pubnubError)
        {
            Console.WriteLine("<STATUS CODE>: {0}", pubnubError.StatusCode); // Unique ID of Error
            Display(String.Format("Error: {0}", pubnubError.Message));
            Console.WriteLine("<MESSAGE>: {0}", pubnubError.Message); // Message received from server/clent or from .NET exception

            Console.WriteLine("<SEVERITY>: {0}", pubnubError.Severity); // Info can be ignored, Warning and Error should be handled

            if (pubnubError.DetailedDotNetException != null)
            {
                Console.WriteLine(pubnubError.IsDotNetException); // Boolean flag to check .NET exception
                Console.WriteLine("<DETAILED DOT.NET EXCEPTION>: {0}", pubnubError.DetailedDotNetException.ToString()); // Full Details of .NET exception
            }
            Console.WriteLine("<MESSAGE SOURCE>: {0}", pubnubError.MessageSource); // Did this originate from Server or Client-side logic
            if (pubnubError.PubnubWebRequest != null)
            {
                //Captured Web Request details
                Console.WriteLine("<HTTP WEB REQUEST>: {0}", pubnubError.PubnubWebRequest.RequestUri.ToString()); 
                Console.WriteLine("<HTTP WEB REQUEST - HEADERS>: {0}", pubnubError.PubnubWebRequest.Headers.ToString()); 
            }
            if (pubnubError.PubnubWebResponse != null)
            {
                //Captured Web Response details
                Console.WriteLine("<HTTP WEB RESPONSE - HEADERS>: {0}", pubnubError.PubnubWebResponse.Headers.ToString());
            }
            Console.WriteLine("<DESCRIPTION>: {0}", pubnubError.Description); // Useful for logging and troubleshooting and support
            Console.WriteLine("<CHANNEL>: {0}", pubnubError.Channel); //Channel name(s) at the time of error
            Console.WriteLine("<DATETIME>: {0}", pubnubError.ErrorDateTimeGMT); //GMT time of error

        }
    }
}
