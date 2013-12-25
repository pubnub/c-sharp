using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Content.Res;
using Android.Support.V4.Widget;
using Android.Support.V4.App;

//TODO
//layout-large

using PubNubMessaging.Core;

namespace PubNubMessaging.Example
{
    [Activity (Label = "PubNubMessaging")]
    public class MainActivity : FragmentActivity
    {
        Pubnub pubnub;

        string channel {
            get;set;
        }

        public bool showErrorMessageSegments = true;

        private MyActionBarDrawerToggle m_DrawerToggle;
        private string m_DrawerTitle;
        private string m_Title;

        private DrawerLayout m_Drawer;
        private ListView m_DrawerList;
        private static readonly string[] Sections = new[]
        {
            "Subscribe", 
            "Publish", 
            "Presence", 
            "Detailed History", 
            "Here Now", 
            "Unsubscribe",
            "Unsubscribe-Presence",
            "Time",
            "Subscribe Grant",
            "Subscribe Audit",
            "Subscribe Revoke",
            "Presence Grant",
            "Presence Audit",
            "Presence Revoke",
            "Auth Key"    
        };


        /*public override bool OnCreateOptionsMenu(IMenu menu)
        {
            menu.Add(0,0,0,"Item 0");
            menu.Add(0,1,1,"Item 1");
            menu.Add(0,2,2,"Item 0");
            menu.Add(0,3,3,"Item 1");
            menu.Add(1,0,0,"Item 0");
            menu.Add(1,1,1,"Item 1");
            menu.Add(1,2,2,"Item 0"); 
            menu.Add(1,3,3,"Item 1");
            menu.Add(2,0,0,"Item 0");
            menu.Add(2,1,1,"Item 1");
            menu.Add(2,2,2,"Item 0");
            menu.Add(2,3,3,"Item 1");
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case 0: //Do stuff for button 0
                return true;
                case 1: //Do stuff for button 1
                return true;
                default:
                return base.OnOptionsItemSelected(item);
            }
        }*/

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
                base.OnPostCreate(savedInstanceState);
                this.m_DrawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
                base.OnConfigurationChanged(newConfig);
                this.m_DrawerToggle.OnConfigurationChanged(newConfig);
        }

        // Pass the event to ActionBarDrawerToggle, if it returns
        // true, then it has handled the app icon touch event
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
                if (this.m_DrawerToggle.OnOptionsItemSelected(item))
                        return true;

                return base.OnOptionsItemSelected(item);
        }

        private void ListItemClicked(int position)
        {
            TextView txtSubscribedChannel = FindViewById<TextView> (Resource.Id.newChannels);
            channel = txtSubscribedChannel.Text;

            switch (position)
            {
                case 0:
                    Subscribe();
                    break;
                case 1:
                    Publish();
                    break;
                case 2:
                    Presence();
                    break; 
                case 3:
                    DetailedHistory();
                    break; 
                case 4:
                    HereNow();
                    break; 
                case 5:
                    Unsub();
                    break; 
                case 6:
                    UnsubPresence();
                    break; 
                case 7:
                    GetTime();
                    break;
                case 8:
                    SubscribeGrant();
                    break;
                case 9:
                    SubscribeAudit();
                    break;
                case 10:
                    SubscribeRevoke();
                    break;
                case 11:
                    PresenceGrant();
                    break;
                case 12:
                    PresenceAudit();
                    break;
                case 13:
                    PresenceRevoke();
                    break;
                case 14:
                    AuthKey();
                    break;
            }
            this.m_DrawerList.SetItemChecked(position, false);
            this.m_Drawer.CloseDrawer(this.m_DrawerList);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {

            var drawerOpen = this.m_Drawer.IsDrawerOpen(this.m_DrawerList);
            //when open don't show anything
            for (int i = 0; i < menu.Size(); i++)
                    menu.GetItem(i).SetVisible(!drawerOpen);


            return base.OnPrepareOptionsMenu(menu);
        }


        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            /*if (ApplicationContext.Resources.Configuration.ScreenLayout = Android.Content.Res.ScreenLayout.SizeLarge) {
                SetContentView (Resource.Layout.Mainlarge);
            } else {*/
                
            //}
            //Build.VERSION.Sdk

            //SetContentView (Resource.Layout.Main);
            SetContentView (Resource.Layout.PageHomeView);

            string channelName = Intent.GetStringExtra("Channel");

            bool enableSSL = Convert.ToBoolean((Intent.GetStringExtra("SslOn")));
            string cipher = (Intent.GetStringExtra("Cipher"));

            string ssl= "";
            if (enableSSL)
                ssl = "SSL,";

            if (!String.IsNullOrWhiteSpace (cipher)) {
                cipher = " Cipher";
            }

            string head = String.Format ("{0}{1}", ssl, cipher);

            pubnub = LaunchScreen.pubnub;

            Title = head; 
            this.m_Title = this.m_DrawerTitle = this.Title;

            TextView txtSubscribedChannel = FindViewById<TextView> (Resource.Id.newChannels);
            txtSubscribedChannel.Text = channelName;
            channel = txtSubscribedChannel.Text;

            TextView txtViewLog = FindViewById<TextView> (Resource.Id.txtViewLog);
            txtViewLog.Text = "";
            try{
                this.m_Drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
                this.m_DrawerList = this.FindViewById<ListView>(Resource.Id.left_drawer);
                this.m_DrawerList.Adapter = new ArrayAdapter<string>(this, Resource.Layout.ItemMenu, Sections);
                this.m_DrawerList.ItemClick += (sender, args) => ListItemClicked(args.Position);

                this.m_Drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow_dark, (int)GravityFlags.Start);
                //DrawerToggle is the animation that happens with the indicator next to the actionbar
                this.m_DrawerToggle = new MyActionBarDrawerToggle(this, this.m_Drawer,
                                                              Resource.Drawable.ic_drawer_light,
                                                              Resource.String.drawer_open,
                                                              Resource.String.drawer_close);
                //Display the current fragments title and update the options menu
                this.m_DrawerToggle.DrawerClosed += (o, args) => 
                {
                        this.ActionBar.Title = this.m_Title;
                        this.InvalidateOptionsMenu();
                };
                //Display the drawer title and update the options menu
                this.m_DrawerToggle.DrawerOpened += (o, args) => 
                {
                        this.ActionBar.Title = this.m_DrawerTitle;
                        this.InvalidateOptionsMenu();
                };
                //Set the drawer lister to be the toggle.
                this.m_Drawer.SetDrawerListener(this.m_DrawerToggle);

                //if first time you will want to go ahead and click first item.
                this.ActionBar.SetDisplayHomeAsUpEnabled(true);
                this.ActionBar.SetHomeButtonEnabled(true);


            }catch (Exception ex){
                Console.WriteLine (ex.ToString ());
            }
        }

        public void Subscribe()
        {
            Display("Running Subscribe");
            pubnub.Subscribe<string>(channel, DisplayReturnMessage, 
                                     DisplayConnectStatusMessage, DisplayErrorMessage);
        }

        public void Publish()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            
            alert.SetTitle("Publish");
            alert.SetMessage("Enter message to publish");
            
            // Set an EditText view to get user input 
            EditText input = new EditText(this);
            alert.SetView(input);
            
            alert.SetPositiveButton("OK", (sender, e) =>
                                    {
                Display("Running Publish");
                string[] channels = channel.Split (',');
                foreach (string channelToCall in channels) {
                    pubnub.Publish<string> (channelToCall.Trim(), input.Text, 
                                            DisplayReturnMessage, DisplayErrorMessage);
                }
            });
            
            alert.SetNegativeButton("Cancel", (sender, e) =>
                                    {
            });
            alert.Show();
            //this.RunOnUiThread(() => alert.Show());
        }
        
        public void Presence()
        {
            Display("Running Presence");
            pubnub.Presence<string>(channel, DisplayReturnMessage, null, DisplayErrorMessage);
        }
        
        public void DetailedHistory ()
        {
            Display ("Running Detailed History");
            string[] channels = channel.Split (',');
            foreach (string channelToCall in channels) {
                pubnub.DetailedHistory<string> (channelToCall.Trim(), 100, DisplayReturnMessage, DisplayErrorMessage);
            }
        }
        
        public void HereNow ()
        {
            Display ("Running Here Now");
            string[] channels = channel.Split (',');
            foreach (string channelToCall in channels) {
                pubnub.HereNow<string> (channelToCall.Trim(), DisplayReturnMessage, DisplayErrorMessage);
            }
        }
        
        public void Unsub()
        {
            Display("Running unsubscribe");
            pubnub.Unsubscribe<string>(channel, DisplayReturnMessage, DisplayReturnMessage, 
                                       DisplayReturnMessage, DisplayErrorMessage);
        }
        
        public void UnsubPresence()
        {
            Display("Running presence-unsubscribe");
            pubnub.PresenceUnsubscribe<string>(channel, DisplayReturnMessage, DisplayReturnMessage, 
                                               DisplayReturnMessage, DisplayErrorMessage);
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
            pubnub.AuditAccess<string>(channel,DisplayReturnMessage, DisplayErrorMessage);
        }

        public void SubscribeRevoke()
        {
            Display("Running Subscribe Revoke");
            pubnub.GrantAccess<string>(channel, false,false, DisplayReturnMessage, DisplayErrorMessage);
        }

        public void PresenceGrant()
        {
            RunGrant (true);
        }

        public void PresenceAudit()
        {
            Display("Running Presence Audit");
            pubnub.AuditPresenceAccess<string>(channel, DisplayReturnMessage, DisplayErrorMessage);
        }

        public void PresenceRevoke()
        {
            Display("Running Presence Revoke");
            pubnub.GrantPresenceAccess<string>(channel, false, false, DisplayReturnMessage, DisplayErrorMessage);
        }

        public void AuthKey()
        {
            var dialog = new AuthDialogFragment();
            dialog.SetAuth += HandleSetAuth;
            dialog.Show(SupportFragmentManager, "dialog");
        }

        void HandleSetAuth (object sender, EventArgs ea)
        {
            try
            {
                Display("Setting Auth Key");
                SetAuthEventArgs cea = ea as SetAuthEventArgs;
                pubnub.AuthenticationKey = cea.authKey;
            }
            catch (Exception ex)
            {
                Display (ex.Message);
            }
            finally
            {
                AuthDialogFragment coroutine = sender as AuthDialogFragment;
                coroutine.SetAuth -= HandleSetAuth;
            }            
        }

        void RunGrant (bool isPresenceGrant)
        {
            var dialog = new GrantDialogFragment();
            dialog.IsPresenceGrant = isPresenceGrant;
            dialog.GrantPerms += HandleGrantPerms;
            dialog.Show(SupportFragmentManager, "dialog");
        }

        void HandleGrantPerms (object sender, EventArgs ea)
        {
            try
            {
                GrantEventArgs cea = ea as GrantEventArgs;
                if (cea.isPresence) {
                    Display("Running Presence Grant");
                    pubnub.GrantPresenceAccess<string>(channel, cea.canRead, cea.canWrite, cea.ttl, DisplayReturnMessage, DisplayErrorMessage);
                }else{
                    Display("Running Subscribe Grant");
                    pubnub.GrantAccess<string>(channel, cea.canRead, cea.canWrite, cea.ttl, DisplayReturnMessage, DisplayErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Display (ex.Message);
            }
            finally
            {
                GrantDialogFragment coroutine = sender as GrantDialogFragment;
                coroutine.GrantPerms -= HandleGrantPerms;
            }
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayConnectStatusMessage(string result)
        {
            Display(String.Format("Connect Callback - {0}", result));
        }        
        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayErrorMessage(string result)
        {
            Display(String.Format("Error Callback - {0}", result));
        }
        
        public void Display (string strText)
        {
            this.RunOnUiThread(() =>
                               {
                TextView txtViewLog = FindViewById<TextView> (Resource.Id.txtViewLog);
                txtViewLog.Append("\n");
                txtViewLog.Append(strText);            }
                               );
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
            // These are all the attributes you may be interested in logging, switchiing on etc:

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
            Display(String.Format("DESCRIPTION: {0}", pubnubError.Description));
            Console.WriteLine("<CHANNEL>: {0}", pubnubError.Channel); //Channel name(s) at the time of error
            Console.WriteLine("<DATETIME>: {0}", pubnubError.ErrorDateTimeGMT); //GMT time of error

        }
    }

    public class ActionBarDrawerEventArgs : EventArgs
    {
        public View DrawerView { get; set; }
        public float SlideOffset { get; set; }
        public int NewState { get; set; }
    }

    public delegate void ActionBarDrawerChangedEventHandler(object s, ActionBarDrawerEventArgs e);

    public class MyActionBarDrawerToggle : ActionBarDrawerToggle
    {
        public MyActionBarDrawerToggle(Activity activity,
                                       DrawerLayout drawerLayout,
                                       int drawerImageRes,
                                       int openDrawerContentDescRes,
                                       int closeDrawerContentDescRes)
            : base(activity,
                   drawerLayout,
                   drawerImageRes,
                   openDrawerContentDescRes,
                   closeDrawerContentDescRes)
        {

        }

        public event ActionBarDrawerChangedEventHandler DrawerClosed;
        public event ActionBarDrawerChangedEventHandler DrawerOpened;
        public event ActionBarDrawerChangedEventHandler DrawerSlide;
        public event ActionBarDrawerChangedEventHandler DrawerStateChanged;

        public override void OnDrawerClosed(View drawerView)
        {
            if (null != this.DrawerClosed)
                this.DrawerClosed(this, new ActionBarDrawerEventArgs { DrawerView = drawerView });
            base.OnDrawerClosed(drawerView);
        }

        public override void OnDrawerOpened(View drawerView)
        {
            if (null != this.DrawerOpened)
                this.DrawerOpened(this, new ActionBarDrawerEventArgs { DrawerView = drawerView });
            base.OnDrawerOpened(drawerView);
        }

        public override void OnDrawerSlide(View drawerView, float slideOffset)
        {
            if (null != this.DrawerSlide)
                this.DrawerSlide(this, new ActionBarDrawerEventArgs
                                 {
                    DrawerView = drawerView,
                    SlideOffset = slideOffset
                });
            base.OnDrawerSlide(drawerView, slideOffset);
        }

        public override void OnDrawerStateChanged(int newState)
        {
            if (null != this.DrawerStateChanged)
                this.DrawerStateChanged(this, new ActionBarDrawerEventArgs
                                        {
                    NewState = newState
                });
            base.OnDrawerStateChanged(newState);
        }
    }
}


