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
using System.Threading;

//TODO
//layout-large
using PubNubMessaging.Core;

namespace PubNubMessaging.Example
{
    [Activity (Label = "PubNubMessaging", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FragmentActivity
    {
        Pubnub pubnub;

        string channel {
            get;
            set;
        }

        public bool showErrorMessageSegments = true;
        private MyActionBarDrawerToggle m_DrawerToggle;
        private string m_DrawerTitle;
        private string m_Title;
        private DrawerLayout m_Drawer;
        private ListView m_DrawerList;
        private static readonly string[] Sections = new[] {
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
            "Auth Key",
            "Presence Heartbeat",
            "Presence Interval",
            "Set User State",
            "Del User State",
            "Set User State Json",
            "Get User State",
            "Where Now",
            "Global Here Now",
            "Change UUID"
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
        protected override void OnPostCreate (Bundle savedInstanceState)
        {
            base.OnPostCreate (savedInstanceState);
            this.m_DrawerToggle.SyncState ();
        }

        public override void OnConfigurationChanged (Configuration newConfig)
        {
            base.OnConfigurationChanged (newConfig);
            this.m_DrawerToggle.OnConfigurationChanged (newConfig);
        }
        // Pass the event to ActionBarDrawerToggle, if it returns
        // true, then it has handled the app icon touch event
        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            if (this.m_DrawerToggle.OnOptionsItemSelected (item))
                return true;

            return base.OnOptionsItemSelected (item);
        }

        private void ListItemClicked (int position)
        {
            TextView txtSubscribedChannel = FindViewById<TextView> (Resource.Id.newChannels);
            channel = txtSubscribedChannel.Text;

            switch (position) {
            case 0:
                Subscribe ();
                break;
            case 1:
                Publish ();
                break;
            case 2:
                Presence ();
                break; 
            case 3:
                DetailedHistory ();
                break; 
            case 4:
                HereNow ();
                break; 
            case 5:
                Unsub ();
                break; 
            case 6:
                UnsubPresence ();
                break; 
            case 7:
                GetTime ();
                break;
            case 8:
                SubscribeGrant ();
                break;
            case 9:
                SubscribeAudit ();
                break;
            case 10:
                SubscribeRevoke ();
                break;
            case 11:
                PresenceGrant ();
                break;
            case 12:
                PresenceAudit ();
                break;
            case 13:
                PresenceRevoke ();
                break;
            case 14:
                AuthKey ();
                break;
            case 15:
                //"Presence Heartbeat",
                SetPresenceHeartbeat ();
                break;
            case 16:
                //"Presence Interval",
                SetPresenceInterval ();
                break;
            case 17:
                //"Add/Mod Local User State",
                AddUserStateKeyPair ();
                break;
            case 18:
                //"Del Local User State",
                DeleteUserState ();
                break;
            /*case 19:
                //"View Local User State",
                ViewLocalUserState ();
                break;*/
            case 19:
                //"Set User State",
                SetUserStateJson ();
                break;
            case 20:
                //"Get User State",
                GetUsetState ();
                break;
            case 21:
                //"Where Now",
                WhereNow ();
                break;
            case 22:
                //"Global Here Now",
                GlobalHereNow ();
                break;
            case 23:
                //"Change UUID"
                ChangeUuid ();
                break;
            }
            this.m_DrawerList.SetItemChecked (position, false);
            this.m_Drawer.CloseDrawer (this.m_DrawerList);
        }

        public override bool OnPrepareOptionsMenu (IMenu menu)
        {

            var drawerOpen = this.m_Drawer.IsDrawerOpen (this.m_DrawerList);
            //when open don't show anything
            for (int i = 0; i < menu.Size (); i++)
                menu.GetItem (i).SetVisible (!drawerOpen);


            return base.OnPrepareOptionsMenu (menu);
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

            string channelName = Intent.GetStringExtra ("Channel");

            bool enableSSL = Convert.ToBoolean ((Intent.GetStringExtra ("SslOn")));
            string cipher = (Intent.GetStringExtra ("Cipher"));

            string ssl = "";
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
            try {
                this.m_Drawer = this.FindViewById<DrawerLayout> (Resource.Id.drawer_layout);
                this.m_DrawerList = this.FindViewById<ListView> (Resource.Id.left_drawer);
                this.m_DrawerList.Adapter = new ArrayAdapter<string> (this, Resource.Layout.ItemMenu, Sections);
                this.m_DrawerList.ItemClick += (sender, args) => ListItemClicked (args.Position);

                this.m_Drawer.SetDrawerShadow (Resource.Drawable.drawer_shadow_dark, GravityFlags.Start);
                //DrawerToggle is the animation that happens with the indicator next to the actionbar
                this.m_DrawerToggle = new MyActionBarDrawerToggle (this, this.m_Drawer,
                    Resource.Drawable.ic_drawer_light,
                    Resource.String.drawer_open,
                    Resource.String.drawer_close);
                //Display the current fragments title and update the options menu
                this.m_DrawerToggle.DrawerClosed += (o, args) => {
                    this.ActionBar.Title = this.m_Title;
                    this.InvalidateOptionsMenu ();
                };
                //Display the drawer title and update the options menu
                this.m_DrawerToggle.DrawerOpened += (o, args) => {
                    this.ActionBar.Title = this.m_DrawerTitle;
                    this.InvalidateOptionsMenu ();
                };
                //Set the drawer lister to be the toggle.
                this.m_Drawer.SetDrawerListener (this.m_DrawerToggle);

                //if first time you will want to go ahead and click first item.
                this.ActionBar.SetDisplayHomeAsUpEnabled (true);
                this.ActionBar.SetHomeButtonEnabled (true);


            } catch (Exception ex) {
                Console.WriteLine (ex.ToString ());
            }
        }

        void ChangeUuid ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.ChangeUuid, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void GlobalHereNow ()
        {
            var dialog = new GrantDialogFragment (CommonDialogStates.GlobalHereNow);
            dialog.GrantPerms += HandleGrantPerms;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void WhereNow ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.WhereNow, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void GetUsetState ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.GetUserState, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void SetUserStateJson ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.SetUserStateJson, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        /*void ViewLocalUserState ()
        {
            string[] channels = channel.Split (',');
            foreach (string channelToCall in channels) {
                string currentUserStateView = pubnub.GetLocalUserState (channelToCall);
                if (!string.IsNullOrEmpty (currentUserStateView)) {
                    Display (string.Format("User state for channel {0}:{1}", channelToCall, currentUserStateView));
                } else {
                    Display (string.Format("No User State Exists for channel {0}", channelToCall));
                }
            }
        }*/

        void DeleteUserState ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.DeleteUserState, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void AddUserStateKeyPair ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.AddUserStateKeyValue, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void SetPresenceInterval ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.PresenceHeartbeatInterval, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void SetPresenceHeartbeat ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.PresenceHeartbeat, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        public void Subscribe ()
        {
            Display ("Running Subscribe");
            ThreadPool.QueueUserWorkItem (o => 
                pubnub.Subscribe<string> (channel, DisplayReturnMessage, 
                    DisplayConnectStatusMessage, DisplayErrorMessage)
            );
        }

        public void Publish ()
        {
            var dialog = new GrantDialogFragment (CommonDialogStates.Publish);
            dialog.GrantPerms += HandleGrantPerms;
            dialog.Show (SupportFragmentManager, "dialog");
        }
        /*public void Publish ()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder (this);
            
            alert.SetTitle ("Publish");
            alert.SetMessage ("Enter message to publish");
            
            // Set an EditText view to get user input 
            EditText input = new EditText (this);
            alert.SetView (input);
            
            alert.SetPositiveButton ("OK", (sender, e) => {
                Display ("Running Publish");
                string[] channels = channel.Split (',');
                string mess = input.Text;
                foreach (string channelToCall in channels) {
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.Publish<string> (channelToCall.Trim (), mess, 
                            DisplayReturnMessage, DisplayErrorMessage)
                    );
                }
            });
            
            alert.SetNegativeButton ("Cancel", (sender, e) => {
            });
            alert.Show ();
            //this.RunOnUiThread(() => alert.Show());
        }*/

        public void Presence ()
        {
            Display ("Running Presence");
            ThreadPool.QueueUserWorkItem (o => 
                pubnub.Presence<string> (channel, DisplayReturnMessage, null, DisplayErrorMessage)
            );
        }

        public void DetailedHistory ()
        {
            Display ("Running Detailed History");
            string[] channels = channel.Split (',');
            foreach (string channelToCall in channels) {
                ThreadPool.QueueUserWorkItem (o => 
                    pubnub.DetailedHistory<string> (channelToCall.Trim (), 100, DisplayReturnMessage, DisplayErrorMessage)
                );
            }
        }

        public void HereNow ()
        {
            Display ("Running Here Now");
            /*string[] channels = channel.Split (',');
            foreach (string channelToCall in channels) {
                pubnub.HereNow<string> (channelToCall.Trim (), DisplayReturnMessage, DisplayErrorMessage);
            }*/
            var dialog = new GrantDialogFragment (CommonDialogStates.HereNow);
            dialog.GrantPerms += HandleGrantPerms;
            dialog.Show (SupportFragmentManager, "dialog");

        }

        public void Unsub ()
        {
            Display ("Running unsubscribe");
            ThreadPool.QueueUserWorkItem (o => 
                pubnub.Unsubscribe<string> (channel, DisplayReturnMessage, DisplayReturnMessage, 
                    DisplayReturnMessage, DisplayErrorMessage)
            );
        }

        public void UnsubPresence ()
        {
            Display ("Running presence-unsubscribe");
            ThreadPool.QueueUserWorkItem (o => 
                pubnub.PresenceUnsubscribe<string> (channel, DisplayReturnMessage, DisplayReturnMessage, 
                    DisplayReturnMessage, DisplayErrorMessage)
            );
        }

        public void GetTime ()
        {
            Display ("Running Time");
            ThreadPool.QueueUserWorkItem (o => 
                pubnub.Time<string> (DisplayReturnMessage, DisplayErrorMessage)
            );
        }

        public void SubscribeGrant ()
        {
            RunGrant (false);
        }

        public void SubscribeAudit ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.AuditSubscribe, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        public void SubscribeRevoke ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.RevokeSubscribe, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        public void PresenceGrant ()
        {
            RunGrant (true);
        }

        public void PresenceAudit ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.AuditPresence, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        public void PresenceRevoke ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.RevokePresence, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        public void AuthKey ()
        {
            var dialog = new CommonDialogFragment (CommonDialogStates.Auth, this);
            dialog.SetValues += HandleSetValues;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void HandleSetValues (object sender, EventArgs ea)
        {
            try {
                SetEventArgs cea = ea as SetEventArgs;

                if (cea.cds == CommonDialogStates.Auth) {
                    Display ("Setting Auth Key");
                    pubnub.AuthenticationKey = cea.valueToSet;
                    Display ("Auth Key set");
                } else if (cea.cds == CommonDialogStates.AuditSubscribe) {

                    Display ("Running Subscribe Audit");
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.AuditAccess<string> (channel, cea.valueToSet, DisplayReturnMessage, DisplayErrorMessage)
                    );
                } else if (cea.cds == CommonDialogStates.AuditPresence) {

                    Display ("Running Presence Audit");
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.AuditPresenceAccess<string> (channel, cea.valueToSet, DisplayReturnMessage, DisplayErrorMessage)
                    );

                } else if (cea.cds == CommonDialogStates.RevokePresence) {
                    Display ("Running Presence Revoke");
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.GrantPresenceAccess<string> (channel, cea.valueToSet, false, false, DisplayReturnMessage, DisplayErrorMessage)
                    );

                } else if (cea.cds == CommonDialogStates.RevokeSubscribe) {

                    Display ("Running Subscribe Revoke");
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.GrantAccess<string> (channel, cea.valueToSet, false, false, DisplayReturnMessage, DisplayErrorMessage)
                    );

                } else if (cea.cds == CommonDialogStates.ChangeUuid) {
                    Display ("Setting UUID");
                    pubnub.ChangeUUID (cea.valueToSet);
                    Display (string.Format ("UUID set to {0}", pubnub.SessionUUID));
                } else if (cea.cds == CommonDialogStates.WhereNow) {
                    Display ("Running where now");
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.WhereNow<string> (cea.valueToSet, DisplayReturnMessage, DisplayErrorMessage)
                    );
                } else if (cea.cds == CommonDialogStates.GetUserState) {
                    Display ("Running get user state");
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.GetUserState<string> (cea.channel, cea.valueToSet, DisplayReturnMessage, DisplayErrorMessage)
                    );
                } else if (cea.cds == CommonDialogStates.DeleteUserState) {
                    Display ("Running delete user state");
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.SetUserState<string> (cea.channel, new KeyValuePair<string, object> (cea.valueToSet, null), DisplayReturnMessage, DisplayErrorMessage)
                    );
                } else if (cea.cds == CommonDialogStates.PresenceHeartbeat) {
                    Display ("Setting presence heartbeat");
                    //int check done in CommonDialogFragment
                    pubnub.PresenceHeartbeat = int.Parse (cea.valueToSet);

                    Display (string.Format ("PresenceHeartbeat set to {0}", pubnub.PresenceHeartbeat));
                } else if (cea.cds == CommonDialogStates.PresenceHeartbeatInterval) {
                    Display ("Setting presence interval");
                    //int check done in CommonDialogFragment
                    pubnub.PresenceHeartbeatInterval = int.Parse (cea.valueToSet);
                    Display (string.Format ("PresenceHeartbeatInterval set to {0}", pubnub.PresenceHeartbeatInterval));
                } else if (cea.cds == CommonDialogStates.AddUserStateKeyValue) {
                    int valueInt;
                    double valueDouble;

                    if (Int32.TryParse (cea.valueToSet2, out valueInt)) {
                        ThreadPool.QueueUserWorkItem (o => 
                            pubnub.SetUserState<string> (cea.channel, new KeyValuePair<string, object> (cea.valueToSet, valueInt), DisplayReturnMessage, DisplayErrorMessage)
                        );
                    } else if (Double.TryParse (cea.valueToSet2, out valueDouble)) {
                        ThreadPool.QueueUserWorkItem (o => 
                            pubnub.SetUserState<string> (cea.channel, new KeyValuePair<string, object> (cea.valueToSet, valueDouble), DisplayReturnMessage, DisplayErrorMessage)
                        );
                    } else {
                        ThreadPool.QueueUserWorkItem (o => 
                            pubnub.SetUserState<string> (cea.channel, new KeyValuePair<string, object> (cea.valueToSet, cea.valueToSet2), DisplayReturnMessage, DisplayErrorMessage)
                        );
                    }
                } else if (cea.cds == CommonDialogStates.SetUserStateJson) {
                    string jsonUserState = "";
                    if (string.IsNullOrEmpty (cea.valueToSet2)) {
                        //jsonUserState = ;
                    } else {
                        jsonUserState = cea.valueToSet2;
                    }
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.SetUserState<string> (cea.channel, cea.valueToSet, jsonUserState, DisplayReturnMessage, DisplayErrorMessage)
                    );
                }
            } catch (Exception ex) {
                Display (ex.Message);
            } finally {
                CommonDialogFragment coroutine = sender as CommonDialogFragment;
                coroutine.SetValues -= HandleSetValues;
            }            
        }

        void RunGrant (bool isPresenceGrant)
        {
            var dialog = new GrantDialogFragment (CommonDialogStates.Grant);
            dialog.IsPresenceGrant = isPresenceGrant;
            dialog.GrantPerms += HandleGrantPerms;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void HandleGrantPerms (object sender, EventArgs ea)
        {
            try {
                GrantEventArgs cea = ea as GrantEventArgs;
                if (cea.cds == CommonDialogStates.Grant) {
                    if (cea.isPresence) {
                        Display ("Running Presence Grant");
                        ThreadPool.QueueUserWorkItem (o => 
                            pubnub.GrantPresenceAccess<string> (channel, cea.channel, cea.valToSet2, cea.valToSet1, cea.ttl, DisplayReturnMessage, DisplayErrorMessage)
                        );
                    } else {
                        Display ("Running Subscribe Grant");
                        ThreadPool.QueueUserWorkItem (o => 
                            pubnub.GrantAccess<string> (channel, cea.channel, cea.valToSet2, cea.valToSet1, cea.ttl, DisplayReturnMessage, DisplayErrorMessage)
                        );
                    }
                } else if (cea.cds == CommonDialogStates.HereNow) {
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.HereNow<string> (cea.channel, cea.valToSet2, cea.valToSet1, DisplayReturnMessage, DisplayErrorMessage)
                    );
                } else if (cea.cds == CommonDialogStates.GlobalHereNow) {
                    ThreadPool.QueueUserWorkItem (o => 
                        pubnub.GlobalHereNow<string> (cea.valToSet2, cea.valToSet1, DisplayReturnMessage, DisplayErrorMessage)
                    );
                } else if (cea.cds == CommonDialogStates.Publish){
                    Display ("Running Publish");
                    string[] channels = cea.channel.Split (',');
                    string mess = cea.message;
                    foreach (string channelToCall in channels) {
                        ThreadPool.QueueUserWorkItem (o => 
                            pubnub.Publish<string> (channelToCall.Trim (), mess, cea.valToSet2,
                                DisplayReturnMessage, DisplayErrorMessage)
                        );
                    }
                }
            } catch (Exception ex) {
                Display (ex.Message);
            } finally {
                GrantDialogFragment coroutine = sender as GrantDialogFragment;
                coroutine.GrantPerms -= HandleGrantPerms;
            }
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayConnectStatusMessage (string result)
        {
            Display (String.Format ("Connect Callback - {0}", result));
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayErrorMessage (string result)
        {
            Display (String.Format ("Error Callback - {0}", result));
        }

        public void Display (string strText)
        {
            this.RunOnUiThread (() => {
                TextView txtViewLog = FindViewById<TextView> (Resource.Id.txtViewLog);
                txtViewLog.Append ("\n\n");
                txtViewLog.Append (strText);
            }
            );
        }

        void DisplayReturnMessage (string result)
        {
            Display (result);
        }

        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="result"></param>
        void DisplayErrorMessage (PubnubClientError result)
        {
            Console.WriteLine ();
            Console.WriteLine (result.Description);
            Console.WriteLine ();

            switch (result.StatusCode) {
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
            if (showErrorMessageSegments) {
                DisplayErrorMessageSegments (result);
                Console.WriteLine ();
            }
        }

        void DisplayErrorMessageSegments (PubnubClientError pubnubError)
        {
            // These are all the attributes you may be interested in logging, switchiing on etc:

            Console.WriteLine ("<STATUS CODE>: {0}", pubnubError.StatusCode); // Unique ID of Error
            Display (String.Format ("Error: {0}", pubnubError.Message));
            Console.WriteLine ("<MESSAGE>: {0}", pubnubError.Message); // Message received from server/clent or from .NET exception

            Console.WriteLine ("<SEVERITY>: {0}", pubnubError.Severity); // Info can be ignored, Warning and Error should be handled

            if (pubnubError.DetailedDotNetException != null) {
                Console.WriteLine (pubnubError.IsDotNetException); // Boolean flag to check .NET exception
                Console.WriteLine ("<DETAILED DOT.NET EXCEPTION>: {0}", pubnubError.DetailedDotNetException.ToString ()); // Full Details of .NET exception
            }
            Console.WriteLine ("<MESSAGE SOURCE>: {0}", pubnubError.MessageSource); // Did this originate from Server or Client-side logic
            if (pubnubError.PubnubWebRequest != null) {
                //Captured Web Request details
                Console.WriteLine ("<HTTP WEB REQUEST>: {0}", pubnubError.PubnubWebRequest.RequestUri.ToString ()); 
                Console.WriteLine ("<HTTP WEB REQUEST - HEADERS>: {0}", pubnubError.PubnubWebRequest.Headers.ToString ()); 
            }
            if (pubnubError.PubnubWebResponse != null) {
                //Captured Web Response details
                Console.WriteLine ("<HTTP WEB RESPONSE - HEADERS>: {0}", pubnubError.PubnubWebResponse.Headers.ToString ());
            }
            Console.WriteLine ("<DESCRIPTION>: {0}", pubnubError.Description); // Useful for logging and troubleshooting and support
            Display (String.Format ("DESCRIPTION: {0}", pubnubError.Description));
            Console.WriteLine ("<CHANNEL>: {0}", pubnubError.Channel); //Channel name(s) at the time of error
            Console.WriteLine ("<DATETIME>: {0}", pubnubError.ErrorDateTimeGMT); //GMT time of error

        }
    }

    public class ActionBarDrawerEventArgs : EventArgs
    {
        public View DrawerView { get; set; }

        public float SlideOffset { get; set; }

        public int NewState { get; set; }
    }
    public delegate void ActionBarDrawerChangedEventHandler (object s, ActionBarDrawerEventArgs e);
    public class MyActionBarDrawerToggle : ActionBarDrawerToggle
    {
        public MyActionBarDrawerToggle (Activity activity,
                                        DrawerLayout drawerLayout,
                                        int drawerImageRes,
                                        int openDrawerContentDescRes,
                                        int closeDrawerContentDescRes)
            : base (activity,
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

        public override void OnDrawerClosed (View drawerView)
        {
            if (null != this.DrawerClosed)
                this.DrawerClosed (this, new ActionBarDrawerEventArgs { DrawerView = drawerView });
            base.OnDrawerClosed (drawerView);
        }

        public override void OnDrawerOpened (View drawerView)
        {
            if (null != this.DrawerOpened)
                this.DrawerOpened (this, new ActionBarDrawerEventArgs { DrawerView = drawerView });
            base.OnDrawerOpened (drawerView);
        }

        public override void OnDrawerSlide (View drawerView, float slideOffset)
        {
            if (null != this.DrawerSlide)
                this.DrawerSlide (this, new ActionBarDrawerEventArgs {
                    DrawerView = drawerView,
                    SlideOffset = slideOffset
                });
            base.OnDrawerSlide (drawerView, slideOffset);
        }

        public override void OnDrawerStateChanged (int newState)
        {
            if (null != this.DrawerStateChanged)
                this.DrawerStateChanged (this, new ActionBarDrawerEventArgs {
                    NewState = newState
                });
            base.OnDrawerStateChanged (newState);
        }
    }
}


