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

using PubnubApi;

namespace PubNubMessaging.Example
{
    [Activity (Label = "PubNubMessaging", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FragmentActivity
    {
        Pubnub pubnub;
        PNConfiguration config;
        SubscribeCallbackExt listener;
        public class LocalLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog (string logText)
            {
                System.Diagnostics.Debug.WriteLine (logText);
            }
        }

        string channel {
            get;
            set;
        }

        string channelGroup {
            get;
            set;
        }

        private MyActionBarDrawerToggle m_DrawerToggle;
        private string m_DrawerTitle;
        private string m_Title;
        private DrawerLayout m_Drawer;
        private ListView m_DrawerList;
        private static readonly string[] Sections = new[] {
            "Subscribe", //0
            "Publish",  //1
            "History", //2
            "Here Now", //3
            "Unsubscribe", //4
            "Time", //5
            "Grant", //8
            "Subscribe Audit", //9
            "Subscribe Revoke",
            "Set User State",
            "Del User State",
            "Get User State",
            "Where Now",
            "Global Here Now",
            "Change UUID", //23
            "Add to ChannelGrp",
            "Get from ChannelGrp",
            "Remove from ChannelGrp",
            ""
        };
        
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
            if (this.m_DrawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            return base.OnOptionsItemSelected (item);
        }

        private void ListItemClicked (int position)
        {
            TextView txtSubscribedChannel = FindViewById<TextView> (Resource.Id.newChannels);
            channel = txtSubscribedChannel.Text;
            TextView txtSubscribedChannelGroup = FindViewById<TextView> (Resource.Id.newChannelGroups);
            channelGroup = txtSubscribedChannelGroup.Text;

            switch (position) {
            case 0:
                Subscribe ();
                break;
            case 1:
                Publish ();
                break;
            case 2:
                DetailedHistory ();
                break; 
            case 3:
                HereNow ();
                break; 
            case 4:
                Unsub ();
                break; 
            case 5:
                GetTime ();
                break;
            case 6:
                SubscribeGrant ();
                break;
            case 7:
                SubscribeAudit ();
                break;
            case 8:
                SubscribeRevoke ();
                break;
            case 9:
                AddUserStateKeyPair ();
                break;
            case 10:
                DeleteUserState ();
                break;
            case 11:
                GetUsetState ();
                break;
            case 12:
                WhereNow ();
                break;
            case 13:
                GlobalHereNow ();
                break;
            case 14:
                ChangeUuid ();
                break;
            case 15:
                AddChannelToChannelGroup ();
                break;
            case 16:
                GetChannelListFromChannelGroup ();
                break;
            case 17:
                RemoveChannelFromChannelGroup ();
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
            {
                menu.GetItem(i).SetVisible(!drawerOpen);
            }

            return base.OnPrepareOptionsMenu (menu);
        }

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
                
            SetContentView (Resource.Layout.PageHomeView);

            string channelName = Intent.GetStringExtra ("Channel");
            string channelGroupName = Intent.GetStringExtra ("ChannelGroup");

            bool enableSSL = Convert.ToBoolean ((Intent.GetStringExtra ("SslOn")));
            string cipher = (Intent.GetStringExtra ("Cipher"));

            string ssl = "";
            if (enableSSL)
            {
                ssl = "SSL,";
            }

            if (!String.IsNullOrWhiteSpace (cipher)) {
                cipher = " Cipher";
            }

            string head = String.Format ("{0}{1}", ssl, cipher);

            pubnub = LaunchScreen.pubnub;

            config = pubnub.PNConfig;
            config.PubnubLog = new LocalLog ();
            config.LogVerbosity = PNLogVerbosity.BODY;
            config.ReconnectionPolicy = PNReconnectionPolicy.LINEAR;

            listener = new SubscribeCallbackExt (
                (o, m) => 
                {
                    if (m != null) { Display(pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message)); }
                }, 
                (o, p) => 
                {
                    if (p != null) { Display(p.Event); }
                }, 
                (o, s) => 
                {
                    if (s != null) { Display(s.Category + " " + s.Operation + " " + s.StatusCode); }
                });

            Title = head; 
            this.m_Title = this.m_DrawerTitle = this.Title;

            TextView txtSubscribedChannel = FindViewById<TextView> (Resource.Id.newChannels);
            txtSubscribedChannel.Text = channelName;
            channel = txtSubscribedChannel.Text;

            TextView txtSubscribedChannelGroup = FindViewById<TextView> (Resource.Id.newChannelGroups);
            txtSubscribedChannelGroup.Text = channelGroupName;
            channelGroup = txtSubscribedChannelGroup.Text;

            TextView txtViewLog = FindViewById<TextView> (Resource.Id.txtViewLog);
            txtViewLog.Text = "";
            try {
                this.m_Drawer = this.FindViewById<DrawerLayout> (Resource.Id.drawer_layout);
                this.m_DrawerList = this.FindViewById<ListView> (Resource.Id.left_drawer);
                this.m_DrawerList.Adapter = new ArrayAdapter<string> (this, Resource.Layout.ItemMenu, Sections);
                this.m_DrawerList.ItemClick += (sender, args) => ListItemClicked (args.Position);

                this.m_Drawer.SetDrawerShadow (Resource.Drawable.drawer_shadow_dark, (int)GravityFlags.Start);
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
                System.Diagnostics.Debug.WriteLine (ex.ToString ());
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
            var dialog = new GrantDialogFragment (CommonDialogStates.GlobalHereNow, this);
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

        public void Subscribe ()
        {
            Display ("Running Subscribe");
            ThreadPool.QueueUserWorkItem (o => {
                pubnub.AddListener (listener);

                pubnub.Subscribe<object> ()
                        .Channels (new [] { channel })
                        .ChannelGroups (new [] { channelGroup })
                      .WithPresence ().Execute ();
            }
            );
        }

        public void Publish ()
        {
            var dialog = new GrantDialogFragment (CommonDialogStates.Publish, this);
            dialog.GrantPerms += HandleGrantPerms;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        public void DetailedHistory ()
        {
            Display ("Running Detailed History");
            string[] channels = channel.Split (',');
            foreach (string channelToCall in channels) {
                ThreadPool.QueueUserWorkItem (o =>
                      pubnub.History ()
                              .Channel (channelToCall.Trim ()).Count (100).Async (new DemoHistoryResult(Display))
                );
            }
        }

        public void HereNow ()
        {
            var dialog = new GrantDialogFragment (CommonDialogStates.HereNow, this);
            dialog.GrantPerms += HandleGrantPerms;
            dialog.Show (SupportFragmentManager, "dialog");

        }

        public void Unsub ()
        {
            Display ("Running unsubscribe");
            ThreadPool.QueueUserWorkItem (o => {
                pubnub.Unsubscribe<string> ()
                   .Channels (new [] { channel })
                   .ChannelGroups (new [] { channelGroup }).Execute ();
                
                pubnub.RemoveListener (listener);

            }
            );
        }

        public void GetTime ()
        {
            Display ("Running Time");
            ThreadPool.QueueUserWorkItem (o =>
                                          pubnub.Time ().Async (new DemoTimeResult(Display))
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

        public void AddChannelToChannelGroup()
        {
            var dialog = new AddRemoveFromChannelGroupDialogFragment (CommonDialogStates.AddToChannelGroup, this);
            dialog.AddRemoveFromCgPerms += HandleAddRemoveFromCgPerms;
            dialog.Show (SupportFragmentManager, "dialog");

        }

        public void GetChannelListFromChannelGroup()
        {
            var dialog = new AddRemoveFromChannelGroupDialogFragment (CommonDialogStates.GetChannelGroup, this);
            dialog.AddRemoveFromCgPerms += HandleAddRemoveFromCgPerms;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        public void RemoveChannelFromChannelGroup()
        {
            var dialog = new AddRemoveFromChannelGroupDialogFragment (CommonDialogStates.RemoveFromChannelGroup, this);
            dialog.AddRemoveFromCgPerms += HandleAddRemoveFromCgPerms;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void HandleSetValues (object sender, EventArgs ea)
        {
            try {
                SetEventArgs cea = ea as SetEventArgs;

                if (cea.cds == CommonDialogStates.AuditSubscribe) {

                    Display ("Running Channel Subscribe Audit");
                    ThreadPool.QueueUserWorkItem (o =>
                                                  pubnub.Audit ()
                                                  .Channel (cea.channel)
                                                  .AuthKeys (new [] { cea.valueToSet })
                                                  .Async (new DemoAuditResult (Display))
                    );
                } else if (cea.cds == CommonDialogStates.RevokeSubscribe) {

                    Display ("Running Subscribe Revoke");
                    ThreadPool.QueueUserWorkItem (o =>
                                                  pubnub.Grant ().Channels (new [] { cea.channel })
                                                  .ChannelGroups (new [] { cea.valueToSet })
                                                  .Read (false).Write (false)
                                                  .Async (new DemoGrantResult (Display))
                    );
                } else if (cea.cds == CommonDialogStates.ChangeUuid) {
                    Display ("Setting UUID");
                    pubnub.ChangeUUID (cea.valueToSet);
                    Display (string.Format ("UUID set to {0}", config.Uuid));
                } else if (cea.cds == CommonDialogStates.WhereNow) {
                    Display ("Running where now");
                    ThreadPool.QueueUserWorkItem (o =>
                          pubnub.WhereNow ().Uuid (cea.valueToSet)
                          .Async (new DemoWhereNowResult (Display))
                    );
                } else if (cea.cds == CommonDialogStates.GetUserState) {
                    Display ("Running get user state");
                    ThreadPool.QueueUserWorkItem (o =>
                          pubnub.GetPresenceState ()
                          .Channels (new [] { cea.channel })
                          .ChannelGroups (new [] { cea.channelGroup })
                          .Uuid (cea.valueToSet)
                          .Async (new DemoPNGetStateResult (Display))
                    );
                } else if (cea.cds == CommonDialogStates.DeleteUserState) {
                    Display ("Running delete user state");
                    ThreadPool.QueueUserWorkItem (o => {
                        Dictionary<string, object> delDic = new Dictionary<string, object> ();
                        delDic.Add (cea.valueToSet, null);
                        pubnub.SetPresenceState ()
                        .Channels (new [] { cea.channel })
                        .ChannelGroups (new [] { cea.channelGroup })
                        .State (delDic)
                        .Async (new DemoPNSetStateResult (Display));
                    }
                    );
                } else if (cea.cds == CommonDialogStates.AddUserStateKeyValue) {
                    int valueInt;
                    double valueDouble;

                    if (Int32.TryParse (cea.valueToSet2, out valueInt)) {
                        ThreadPool.QueueUserWorkItem (o => {
                            Dictionary<string, object> dicInt = new Dictionary<string, object> ();
                            dicInt.Add (cea.valueToSet, valueInt);

                            pubnub.SetPresenceState ()
                            .Channels (new [] { cea.channel })
                            .ChannelGroups (new [] { cea.channelGroup })
                            .State (dicInt)
                                  .Async (new DemoPNSetStateResult (Display));
                        }
                        );
                    } else if (Double.TryParse (cea.valueToSet2, out valueDouble)) {
                        ThreadPool.QueueUserWorkItem (o => {
                            Dictionary<string, object> dicDouble = new Dictionary<string, object> ();
                            dicDouble.Add (cea.valueToSet, valueDouble);

                            pubnub.SetPresenceState ()
                            .Channels (new [] { cea.channel })
                            .ChannelGroups (new [] { cea.channelGroup })
                            .State (dicDouble)
                                  .Async (new DemoPNSetStateResult (Display));
                        }
                        );
                    } else {
                        ThreadPool.QueueUserWorkItem (o => {
                            Dictionary<string, object> dicStr = new Dictionary<string, object> ();
                            dicStr.Add (cea.valueToSet, cea.valueToSet2);

                            pubnub.SetPresenceState ()
                            .Channels (new [] { cea.channel })
                            .ChannelGroups (new [] { cea.channelGroup })
                            .State (dicStr)
                                  .Async (new DemoPNSetStateResult (Display));
                        });
                    }
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
            var dialog = new GrantDialogFragment (CommonDialogStates.Grant, this);
            dialog.IsPresenceGrant = isPresenceGrant;
            dialog.GrantPerms += HandleGrantPerms;
            dialog.Show (SupportFragmentManager, "dialog");
        }

        void HandleAddRemoveFromCgPerms(object sender, EventArgs ea)
        {
            try {
                AddRemoveFromCgEventArgs cea = ea as AddRemoveFromCgEventArgs;
                if (cea.cds == CommonDialogStates.AddToChannelGroup) {
                    Display ("Running AddChannelsToChannelGroup");
                    ThreadPool.QueueUserWorkItem (o => 
                          pubnub.AddChannelsToChannelGroup().Channels(new [] { cea.channel })
                          .ChannelGroup(cea.channelGroup)
                          .Async(new DemoChannelGroupAddChannel(Display))
                    );
                } else if (cea.cds == CommonDialogStates.RemoveFromChannelGroup) {
                    Display ("Running RemoveChannelsFromChannelGroup");
                    ThreadPool.QueueUserWorkItem (o => 
                          pubnub.RemoveChannelsFromChannelGroup()
                          .Channels(new [] { cea.channel })
                          .ChannelGroup(cea.channelGroup)
                          .Async(new DemoChannelGroupRemoveChannel(Display))
                    );
                } else if (cea.cds == CommonDialogStates.GetChannelGroup) {
                    Display ("Running GetChannelsForChannelGroup");
                    ThreadPool.QueueUserWorkItem (o => 
                          pubnub.ListChannelsForChannelGroup()
                          .ChannelGroup(cea.channelGroup)
                          .Async(new DemoChannelGroupAllChannels(Display))
                    );
                } 
            }
            catch (Exception ex) {
                Display (ex.Message);
            } finally {
                AddRemoveFromChannelGroupDialogFragment coroutine = sender as AddRemoveFromChannelGroupDialogFragment;
                coroutine.AddRemoveFromCgPerms -= HandleAddRemoveFromCgPerms;
            }

        }

        void HandleGrantPerms (object sender, EventArgs ea)
        {
            try {
                GrantEventArgs cea = ea as GrantEventArgs;
                if (cea.cds == CommonDialogStates.Grant) {
                    Display ("Running Subscribe Grant");
                    ThreadPool.QueueUserWorkItem (o =>
                          pubnub.Grant()
                          .Channels(new [] { cea.channel })
                          .AuthKeys(new [] { cea.authKey })
                          .Read(cea.valToSet2)
                          .Write(cea.valToSet1)
                          .TTL(cea.ttl)
                          .Async(new DemoGrantResult(Display))
                    );
                } else if (cea.cds == CommonDialogStates.HereNow) {
                    Display ("Running HereNow");
                    ThreadPool.QueueUserWorkItem (o => 
                          pubnub.HereNow()
                          .Channels(new [] { cea.channel })
                          .IncludeUUIDs(cea.valToSet2)
                          .IncludeState(cea.valToSet1)
                          .Async(new DemoHereNowResult(Display))
                    );
                } else if (cea.cds == CommonDialogStates.GlobalHereNow) {
                    Display ("Running GlobalHereNow");
                    ThreadPool.QueueUserWorkItem (o => 
                          pubnub.HereNow().IncludeUUIDs(cea.valToSet2)
                          .IncludeState(cea.valToSet1)
                          .Async(new DemoHereNowResult(Display))
                    );
                } else if (cea.cds == CommonDialogStates.Publish){
                    Display ("Running Publish");
                    string[] channels = cea.channel.Split (',');
                    string mess = cea.message;
                    foreach (string channelToCall in channels) {
                        ThreadPool.QueueUserWorkItem (o => 
                              pubnub.Publish()
                              .Channel(channelToCall.Trim ())
                              .Message(mess)
                              .ShouldStore(cea.valToSet2)
                              .Async(new DemoPublishResult(Display))
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

        public void Display (string strText)
        {
            this.RunOnUiThread (() => {
                TextView txtViewLog = FindViewById<TextView> (Resource.Id.txtViewLog);
                txtViewLog.Append ("\n\n");
                txtViewLog.Append (strText);
            }
            );
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
            {
                this.DrawerClosed(this, new ActionBarDrawerEventArgs { DrawerView = drawerView });
            }
            base.OnDrawerClosed (drawerView);
        }

        public override void OnDrawerOpened (View drawerView)
        {
            if (null != this.DrawerOpened)
            {
                this.DrawerOpened(this, new ActionBarDrawerEventArgs { DrawerView = drawerView });
            }
            base.OnDrawerOpened (drawerView);
        }

        public override void OnDrawerSlide (View drawerView, float slideOffset)
        {
            if (null != this.DrawerSlide)
            {
                this.DrawerSlide(this, new ActionBarDrawerEventArgs
                {
                    DrawerView = drawerView,
                    SlideOffset = slideOffset
                });
            }
            base.OnDrawerSlide (drawerView, slideOffset);
        }

        public override void OnDrawerStateChanged (int newState)
        {
            if (null != this.DrawerStateChanged)
            {
                this.DrawerStateChanged(this, new ActionBarDrawerEventArgs
                {
                    NewState = newState
                });
            }
            base.OnDrawerStateChanged (newState);
        }
    }

    public class DemoTimeResult : PNCallback<PNTimeResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoTimeResult (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }

        public override void OnResponse (PNTimeResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    };

    public class DemoPublishResult : PNCallback<PNPublishResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoPublishResult (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }

        public override void OnResponse (PNPublishResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    };

    public class DemoHistoryResult : PNCallback<PNHistoryResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoHistoryResult (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNHistoryResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    };

    public class DemoHereNowResult : PNCallback<PNHereNowResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoHereNowResult (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNHereNowResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    };

    public class DemoWhereNowResult : PNCallback<PNWhereNowResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoWhereNowResult (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNWhereNowResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    };

    public class DemoPNGetStateResult : PNCallback<PNGetStateResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoPNGetStateResult (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNGetStateResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    };

    public class DemoPNSetStateResult : PNCallback<PNSetStateResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoPNSetStateResult (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNSetStateResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    };

    public class DemoSubscribeCallback : SubscribeCallback
    {
        readonly Action<string> callback;
        public DemoSubscribeCallback (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }

        public override void File(Pubnub pubnub, PNFileEventResult fileEvent)
        {
            throw new NotImplementedException();
        }

        public override void Message<T> (Pubnub pubnub, PNMessageResult<T> messageResult)
        {
            if (messageResult != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (messageResult));
            }
        }

        public override void MessageAction(Pubnub pubnub, PNMessageActionEventResult messageAction)
        {
            throw new NotImplementedException();
        }

        public override void ObjectEvent(Pubnub pubnub, PNObjectEventResult objectEvent)
        {
            throw new NotImplementedException();
        }

        public override void Presence (Pubnub pubnub, PNPresenceEventResult presenceResult)
        {
            if (presenceResult != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (presenceResult));
            }
        }

        public override void Signal<T>(Pubnub pubnub, PNSignalResult<T> signal)
        {
            throw new NotImplementedException();
        }

        public override void Status (Pubnub pubnub, PNStatus statusObj)
        {
            string msg = string.Format ("Operation: {0}; Category: {1};  StatusCode: {2}", statusObj.Operation, statusObj.Category, statusObj.StatusCode);
            this.callback (msg);

            if (statusObj.Category == PNStatusCategory.PNUnexpectedDisconnectCategory) {
                // This event happens when radio / connectivity is lost
                System.Diagnostics.Debug.WriteLine("PNUnexpectedDisconnectCategory");
            } else if (statusObj.Category == PNStatusCategory.PNConnectedCategory) {
                // Connect event. You can do stuff like publish, and know you'll get it.
                // Or just use the connected event to confirm you are subscribed for
                // UI / internal notifications, etc
                System.Diagnostics.Debug.WriteLine("PNConnectedCategory");
            } else if (statusObj.Category == PNStatusCategory.PNReconnectedCategory) {
                // Happens as part of our regular operation. This event happens when
                // radio / connectivity is lost, then regained.
                System.Diagnostics.Debug.WriteLine("PNReconnectedCategory");
            } else if (statusObj.Category == PNStatusCategory.PNDecryptionErrorCategory) {
                // Handle messsage decryption error. Probably client configured to
                // encrypt messages and on live data feed it received plain text.
                System.Diagnostics.Debug.WriteLine("PNDecryptionErrorCategory");
            }
        }
    }

    public class DemoGrantResult : PNCallback<PNAccessManagerGrantResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoGrantResult (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNAccessManagerGrantResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    };

    public class DemoAuditResult : PNCallback<PNAccessManagerAuditResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoAuditResult (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNAccessManagerAuditResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    };

    public class DemoPushAddChannel : PNCallback<PNPushAddChannelResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoPushAddChannel (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNPushAddChannelResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    }

    public class DemoPushRemoveChannel : PNCallback<PNPushRemoveChannelResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoPushRemoveChannel (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNPushRemoveChannelResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    }

    public class DemoPushListProvisionChannel : PNCallback<PNPushListProvisionsResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoPushListProvisionChannel (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNPushListProvisionsResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    }

    public class DemoChannelGroupAddChannel : PNCallback<PNChannelGroupsAddChannelResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoChannelGroupAddChannel (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNChannelGroupsAddChannelResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    }

    public class DemoChannelGroupRemoveChannel : PNCallback<PNChannelGroupsRemoveChannelResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoChannelGroupRemoveChannel (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNChannelGroupsRemoveChannelResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    }

    public class DemoChannelGroupDeleteGroup : PNCallback<PNChannelGroupsDeleteGroupResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoChannelGroupDeleteGroup (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNChannelGroupsDeleteGroupResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    }

    public class DemoChannelGroupAll : PNCallback<PNChannelGroupsListAllResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoChannelGroupAll (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNChannelGroupsListAllResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    }

    public class DemoChannelGroupAllChannels : PNCallback<PNChannelGroupsAllChannelsResult>
    {
        readonly Action<string> callback;
        readonly Pubnub pubnub = new Pubnub (null);
        public DemoChannelGroupAllChannels (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void OnResponse (PNChannelGroupsAllChannelsResult result, PNStatus status)
        {
            if (result != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (result));
            } else if (status != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (status));
            }
        }
    }

}


