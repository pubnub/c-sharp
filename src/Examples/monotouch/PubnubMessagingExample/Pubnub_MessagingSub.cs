using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using MonoTouch.Dialog;
using System.Threading;
using CoreGraphics;
using ObjCRuntime;
using PubnubApi;
using MonoTouch.SlideoutNavigation;

namespace PubnubMessagingExample
{
    public class LeftNavController : DialogViewController
    {
        Pubnub_MessagingSub pubnub_MessagingSub;
        SlideoutNavigationController menu;

        public LeftNavController (SlideoutNavigationController menu, Pubnub_MessagingSub pubnub_MessagingSub)
            : base (UITableViewStyle.Plain, new RootElement (""))
        {
            this.pubnub_MessagingSub = pubnub_MessagingSub;
            this.menu = menu;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            Root.Add (new Section () {
                new StyledStringElement ("Subscribe", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.Subscribe ();
                }),
                new StyledStringElement ("Publish", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.Publish ();
                }),
                new StyledStringElement ("History", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.DetailedHistory ();
                }),
                new StyledStringElement ("Here Now", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.HereNow ();
                }),
                new StyledStringElement ("Time", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.GetTime ();
                }),
                new StyledStringElement ("Unsubscribe", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.Unsub ();
                }),
                new StyledStringElement ("Subscribe Grant", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.SubscribeGrant ();
                }),
                new StyledStringElement ("Subscribe Audit", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.SubscribeAudit ();
                }),
                new StyledStringElement ("Subscribe Revoke", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.SubscribeRevoke ();
                }),
                new StyledStringElement ("Presence Heartbeat", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.SetPresenceHeartbeat ();
                }),
                new StyledStringElement ("Presence Interval", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.SetPresenceInterval ();
                }),
                new StyledStringElement ("Set User State Key-Val", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.SetUserStateKeyVal ();
                }),
                new StyledStringElement ("Del User State", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.DelUserState ();
                }),
                new StyledStringElement ("Get User State", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.GetUserState ();
                }),
                new StyledStringElement ("Where Now", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.WhereNow ();
                }),
                new StyledStringElement ("Global Here Now", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.GlobalHereNow ();
                }),
                new StyledStringElement ("Change UUID", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.ChangeUuid ();
                }),
                new StyledStringElement ("Add Channel To ChannelGroup", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.AddChannelToChannelGroup();
                }),
                new StyledStringElement ("Remove Channel From ChannelGroup", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.RemoveChannelFromChannelGroup();
                }),
                new StyledStringElement ("Get ChannelList From ChannelGroup", () => {
                    menu.Hide (true);
                    pubnub_MessagingSub.GetChannelFromChannelGroup();
                }),
            });
        }
    }

    public partial class Pubnub_MessagingSub : DialogViewController
    {
        Pubnub pubnub;
        PNConfiguration config;
        static Pubnub_MessagingSub instance = null;

        DemoSubscribeCallback listener = null;

        public enum CommonDialogStates
        {
            Publish,
            HereNow,
            PresenceHeartbeat,
            PresenceHeartbeatInterval,
            SetUserStateKeyPair,
            ViewLocalUserState,
            DeleteUserState,
            GetUserState,
            WhereNow,
            GlobalHereNow,
            ChangeUuid,
            SubscribeGrant,
            AuditSubscribe,
            RevokeSubscribe,
            AddChannelToChannelGroup,
            RemoveChannelFromChannelGroup,
            GetChannelsFromChannelGroup
        }

        public SlideoutNavigationController Menu { get; private set; }

        string Channel {
            get;
            set;
            //get { return newChannels.Text; }
            //set { newChannels.Text = value; }
        }

        string ChannelGroup {
            get;
            set;
        }

        string Cipher {
            get;
            set;    
        }

        bool Ssl {
            get;
            set;
        }

        DialogViewController dvc;
        RootElement root;
        Section secOutput;
        UIFont font12 = UIFont.SystemFontOfSize (12);
        UIFont font13 = UIFont.SystemFontOfSize (13);
        public bool showErrorMessageSegments = true;
        UITextField tfChannels;
        UITextField newChannels;
        UITextField newChannelGroups;

        public Pubnub_MessagingSub (string channelName, string channelGroupName, string cipher, bool enableSSL, Pubnub pubnub, PNConfiguration pubnubConfig)
            : base (UITableViewStyle.Grouped, null)
        {
            instance = this;

            listener = new DemoSubscribeCallback (instance.Display);

            Channel = channelName;
            ChannelGroup = channelGroupName;
            Ssl = enableSSL;
            Cipher = cipher;
            this.pubnub = pubnub;
            this.config = pubnubConfig;

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
            
            secAction.HeaderView = CreateHeaderView (viewHeight);
            
            secOutput = new Section ("Output");
            
            root = new RootElement (head) {
                secAction,
                secOutput
            };

            Root = root;
            dvc = new DialogViewController (root, true);
            var tap = new UITapGestureRecognizer ();
            tap.AddTarget (() => {
                dvc.View.EndEditing (true);
            });
            dvc.View.AddGestureRecognizer (tap);

            tap.CancelsTouchesInView = false;
            dvc.NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, delegate {
                InvokeInBackground(() => {
                    pubnub.Destroy();
                });
                AppDelegate.navigation.PopToRootViewController (true);
            });
            Menu = new SlideoutNavigationController ();
            Menu.TopView = dvc;

            Menu.MenuViewLeft = new LeftNavController (Menu, this);

            AppDelegate.navigation.PushViewController (Menu, true);
            Menu.ShowMenuLeft ();
            newChannels.Text = Channel;
            newChannelGroups.Text = ChannelGroup;

        }

        UIView CreateHeaderView (int iViewHeight)
        {
            UIView uiView = new UIView (new CGRect (0, 0, this.View.Bounds.Width, iViewHeight));
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

            UILabel lblInfo = new UILabel (new CGRect (10, 2, 300, 25));
            lblInfo.Font = font12;
            lblInfo.Text = "Enter new channel(s)/channelgroup(s) and/or use the menu for actions";
            uiView.Add (lblInfo);

            UILabel lblNewChannel = new UILabel (new CGRect (10, 32, 100, 25));
            lblNewChannel.Font = font13;
            lblNewChannel.Text = "New Channel(s):";
            uiView.Add (lblNewChannel);

            newChannels = new UITextField (new CGRect (120, 32, 185, 25));
            newChannels.AutocorrectionType = UITextAutocorrectionType.No;
            newChannels.BackgroundColor = UIColor.White;
            newChannels.Font = font12;
            uiView.Add (newChannels);

            UILabel lblNewChannelGroup = new UILabel (new CGRect (10, 62, 140, 25));
            lblNewChannelGroup.Font = font13;
            lblNewChannelGroup.Text = "New ChannelGroup(s):";
            uiView.Add (lblNewChannelGroup);

            newChannelGroups = new UITextField (new CGRect (160, 62, 145, 25));
            newChannelGroups.AutocorrectionType = UITextAutocorrectionType.No;
            newChannelGroups.BackgroundColor = UIColor.White;
            newChannelGroups.Font = font12;
            uiView.Add (newChannelGroups);

            return uiView;
        }

        public void Subscribe ()
        {
            Display ("Running Subscribe");
            Channel = newChannels.Text;
            InvokeInBackground(() => {
                pubnub.AddListener (listener);
                pubnub.Subscribe<string>()
                      .Channels(new string [] { Channel })
                      .ChannelGroups(new string [] { ChannelGroup})
                      .WithPresence()
                      .Execute();
            });
        }

        /*public void Publish ()
        {
            UIAlertView alert = new UIAlertView ();
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            alert.Title = "Publish";
            alert.Message = "Enter message to publish";
            alert.AddButton ("Publish");
            alert.AddButton ("Cancel");
            alert.Clicked += delegate(object sender, UIButtonEventArgs e) {
                if (e.ButtonIndex == 0) {
                    if (alert.GetTextField (0) != null) {
                        string input = alert.GetTextField (0).Text;
                        Display ("Running Publish");
                        Channel = newChannels.Text;
                        string[] channels = Channel.Split (',');
                        foreach (string channel in channels) {
                            InvokeInBackground(() => {
                                pubnub.Publish<string> (channel.Trim (), input, DisplayReturnMessage, DisplayErrorMessage);
                            });
                        }
                    }
                }           
            };
            alert.Show ();
        }

        void PublishAlertDismissed (object sender, UIButtonEventArgs e)
        {
            InputAlertView iav = (InputAlertView)sender;
            if ((iav != null) && (!String.IsNullOrWhiteSpace (iav.EnteredText))) {
                Display ("Running Publish");
                Channel = newChannels.Text;
                string[] channels = Channel.Split (',');

                foreach (string channel in channels) {
                    InvokeInBackground(() => {
                        pubnub.Publish<string> (channel.Trim (), iav.EnteredText, DisplayReturnMessage, DisplayErrorMessage);
                    });
                }
            }
        }*/

        public void Publish (){
            ShowAlertType2 (CommonDialogStates.Publish);
        }


        public void DetailedHistory ()
        {
            Display ("Running Detailed History");
            Channel = newChannels.Text;
            string[] channels = Channel.Split (',');
            foreach (string channel in channels) {
                InvokeInBackground (() => {
                    pubnub.History()
                          .Channel(channel.Trim ())
                          .Count(100)
                          .Async(new DemoHistoryResult(Display));
                });
            }
        }

        public void HereNow ()
        {
            ShowAlertType2 (CommonDialogStates.HereNow);
        }

        public void Unsub ()
        {
            Display ("Running unsubscribe");
            Channel = newChannels.Text;
            InvokeInBackground (() => {
                pubnub.Unsubscribe<string>().Channels(new string [] { Channel })
                      .ChannelGroups(new string [] { ChannelGroup })
                      .Execute();
                pubnub.RemoveListener (listener);
            });
        }

        public void GetTime ()
        {
            Display ("Running Time");
            InvokeInBackground (() => {
                pubnub.Time().Async(new DemoTimeResult(instance.Display));
            });
        }

        public void SubscribeGrant ()
        {
            ShowAlertType2 (CommonDialogStates.SubscribeGrant);
        }

        public void SubscribeAudit ()
        {
            //Display("Running Subscribe Audit");
            //pubnub.AuditAccess<string>(Channel,DisplayReturnMessage, DisplayErrorMessage);
            ShowAlertType1 (CommonDialogStates.AuditSubscribe);
        }

        public void SubscribeRevoke ()
        {
            //Display("Running Subscribe Revoke");
            //pubnub.GrantAccess<string>(Channel, false,false, DisplayReturnMessage, DisplayErrorMessage);
            ShowAlertType1 (CommonDialogStates.RevokeSubscribe);
        }

        public void SetPresenceHeartbeat ()
        {
            ShowAlertType1 (CommonDialogStates.PresenceHeartbeat);
        }

        public void SetPresenceInterval ()
        {
            ShowAlertType1 (CommonDialogStates.PresenceHeartbeatInterval);
        }

        public void ChangeUuid ()
        {
            ShowAlertType1 (CommonDialogStates.ChangeUuid);
        }

        public void GlobalHereNow ()
        {
            ShowAlertType2 (CommonDialogStates.GlobalHereNow);
        }

        public void WhereNow ()
        {
            ShowAlertType1 (CommonDialogStates.WhereNow);
        }

        public void GetUserState ()
        {
            ShowAlertType3 (CommonDialogStates.GetUserState);
        }

        public void AddChannelToChannelGroup()
        {
            EntryElement elementChannel = new EntryElement ("Channel(s)", "Enter Channel(s)", "");
            elementChannel.AutocapitalizationType = UITextAutocapitalizationType.None;
            elementChannel.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement elementChannelGroup = new EntryElement ("ChannelGroup", "Enter ChannelGrop", "");
            elementChannelGroup.AutocapitalizationType = UITextAutocapitalizationType.None;
            elementChannelGroup.AutocorrectionType = UITextAutocorrectionType.No;

            var newroot = new RootElement ("Add Channel To ChannelGroup", 0, 0) {
                new Section () {
                    elementChannelGroup,
                    elementChannel,
                },
                new Section ("") {
                    new StyledStringElement ("Add Channel", () => {
                        string entryChannel = elementChannel.Value;
                        string entryChannelGroup = elementChannelGroup.Value;

                        Display ("Adding Channel To ChannelGroup");
                        pubnub.AddChannelsToChannelGroup()
                              .Channels(new string[]{entryChannel})
                              .ChannelGroup(entryChannelGroup)
                              .Async(new DemoChannelGroupAddChannel(Display));

                        AppDelegate.navigation.PopViewController (true);
                    }) {
                        BackgroundColor = UIColor.Blue,
                        TextColor = UIColor.White,
                        Alignment = UITextAlignment.Center
                    },
                },
            };
            dvc = new DialogViewController (newroot, true);
            AppDelegate.navigation.PushViewController (dvc, true);
            
        }

        public void RemoveChannelFromChannelGroup()
        {
            EntryElement elementChannel = new EntryElement ("Channel(s)", "Enter Channel(s)", "");
            elementChannel.AutocapitalizationType = UITextAutocapitalizationType.None;
            elementChannel.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement elementChannelGroup = new EntryElement ("ChannelGroup", "Enter ChannelGrop", "");
            elementChannelGroup.AutocapitalizationType = UITextAutocapitalizationType.None;
            elementChannelGroup.AutocorrectionType = UITextAutocorrectionType.No;

            var newroot = new RootElement ("Remove Channel From ChannelGroup", 0, 0) {
                new Section () {
                    elementChannelGroup,
                    elementChannel,
                },
                new Section ("") {
                    new StyledStringElement ("Remove Channel", () => {
                        string entryChannel = elementChannel.Value;
                        string entryChannelGroup = elementChannelGroup.Value;

                        Display ("Removing Channel From ChannelGroup");
                        pubnub.RemoveChannelsFromChannelGroup()
                              .Channels(new string[]{entryChannel})
                              .ChannelGroup(entryChannelGroup)
                              .Async(new DemoChannelGroupRemoveChannel(Display));

                        AppDelegate.navigation.PopViewController (true);
                    }) {
                        BackgroundColor = UIColor.Blue,
                        TextColor = UIColor.White,
                        Alignment = UITextAlignment.Center
                    },
                },
            };
            dvc = new DialogViewController (newroot, true);
            AppDelegate.navigation.PushViewController (dvc, true);
        }

        public void GetChannelFromChannelGroup()
        {
            EntryElement elementChannelGroup = new EntryElement ("ChannelGroup", "Enter ChannelGrop", "");
            elementChannelGroup.AutocapitalizationType = UITextAutocapitalizationType.None;
            elementChannelGroup.AutocorrectionType = UITextAutocorrectionType.No;

            var newroot = new RootElement ("Get Channel List From ChannelGroup", 0, 0) {
                new Section () {
                    elementChannelGroup,
                },
                new Section ("") {
                    new StyledStringElement ("Get ChannelList", () => {
                        string entryChannelGroup = elementChannelGroup.Value;

                        Display ("Getting ChannelList From ChannelGroup");
                        pubnub.ListChannelsForChannelGroup()
                              .ChannelGroup(entryChannelGroup)
                              .Async(new DemoChannelGroupAllChannels(instance.Display));

                        AppDelegate.navigation.PopViewController (true);
                    }) {
                        BackgroundColor = UIColor.Blue,
                        TextColor = UIColor.White,
                        Alignment = UITextAlignment.Center
                    },
                },
            };
            dvc = new DialogViewController (newroot, true);
            AppDelegate.navigation.PushViewController (dvc, true);
        }

        /*public void ViewUserState ()
        {
            string[] channels = Channel.Split (',');
            foreach (string channelToCall in channels) {
                string currentUserStateView = pubnub.GetUserState (channelToCall);
                if (!string.IsNullOrEmpty (currentUserStateView)) {
                    Display (string.Format("User state for channel {0}:{1}", channelToCall, currentUserStateView));
                } else {
                    Display (string.Format("No User State Exists for channel {0}", channelToCall));
                }
            }

        }*/

        public void DelUserState ()
        {
            ShowAlertType3 (CommonDialogStates.DeleteUserState);
        }

        public void SetUserStateKeyVal ()
        {
            ShowAlertType3 (CommonDialogStates.SetUserStateKeyPair);
        }

        void ShowAlertType3 (CommonDialogStates cds)
        {
            bool showEntryText3 = true;

            string strHead = "", elementText1 = "", elementText2 = "", elementText3 = "";
            string elementSubText1 = "", elementSubText2 = "", elementSubText3 = "", buttonTitle = "";
            string elementTextChannelGroup = "", elementSubTextChannelGroup = "";
            if (cds == CommonDialogStates.SetUserStateKeyPair) {
                strHead = "Add Local User State";
                elementText1 = "Channel";
                elementTextChannelGroup = "ChannelGroup";
                elementText2 = "Key";
                elementText3 = "Value";
                elementSubText1 = "Enter Channel";
                elementSubTextChannelGroup = "Enter ChannelGroup";
                elementSubText2 = "Enter Key";
                elementSubText3 = "Enter Value";
                buttonTitle = "Add";
            } else if (cds == CommonDialogStates.DeleteUserState) {
                strHead = "Delete Local User State";
                elementText1 = "Channel";
                elementTextChannelGroup = "ChannelGroup";
                elementText2 = "Key";
                elementSubText1 = "Enter Channel";
                elementSubTextChannelGroup = "Enter ChannelGroup";
                elementSubText2 = "Key to delete";
                buttonTitle = "Delete";
                showEntryText3 = false;
            } else if (cds == CommonDialogStates.GetUserState) {
                strHead = "Get User State";
                elementText1 = "Channel";
                elementTextChannelGroup = "ChannelGroup";
                elementText2 = "UUID";
                elementSubText1 = "Enter Channel";
                elementSubTextChannelGroup = "Enter ChannelGroup";
                elementSubText2 = "Enter UUID";
                buttonTitle = "Get";
                showEntryText3 = false;
            }

            EntryElement entryText3 = null;

            if (showEntryText3) {
                entryText3 = new EntryElement (elementText3, elementSubText3, "");
                entryText3.AutocapitalizationType = UITextAutocapitalizationType.None;
                entryText3.AutocorrectionType = UITextAutocorrectionType.No;
            }

            EntryElement entryText1 = new EntryElement (elementText1, elementSubText1, "");
            entryText1.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryText1.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryText2 = new EntryElement (elementText2, elementSubText2, "");
            entryText2.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryText2.AutocorrectionType = UITextAutocorrectionType.No;

            EntryElement entryChannelGroup = new EntryElement (elementTextChannelGroup, elementSubTextChannelGroup, "");
            entryChannelGroup.AutocapitalizationType = UITextAutocapitalizationType.None;
            entryChannelGroup.AutocorrectionType = UITextAutocorrectionType.No;

            var newroot = new RootElement (strHead, 0, 0) {
                new Section () {
                    entryText1,
                    entryChannelGroup,
                    entryText2,
                    entryText3
                },
                new Section ("") {
                    new StyledStringElement (buttonTitle, () => {
                        string entryText1Val = entryText1.Value;
                        string entryText2Val = entryText2.Value;
                        string entryChannelGroupValue = entryChannelGroup.Value;

                        if (cds == CommonDialogStates.SetUserStateKeyPair) {
                            string entryText3Val = entryText3.Value;
                            Display ("Setting user state");
                            int valueInt;
                            double valueDouble;
                            if (Int32.TryParse (entryText3Val, out valueInt)) {
                                InvokeInBackground(() => {
                                    Dictionary<string, object> intDic = new Dictionary<string, object>();
                                    intDic.Add(entryText2Val, valueInt);
                                    
                                    pubnub.SetPresenceState()
                                          .Channels(new string[] { entryText1Val })
                                          .ChannelGroups(new string[] { entryChannelGroupValue })
                                          .State(intDic)
                                          .Async(new DemoPNSetStateResult(instance.Display));
                                });
                            } else if (Double.TryParse (entryText3Val, out valueDouble)) {
                                InvokeInBackground(() => {
                                    Dictionary<string, object> doubleDic = new Dictionary<string, object>();
                                    doubleDic.Add(entryText2Val, valueDouble);
                                    
                                    pubnub.SetPresenceState()
                                          .Channels(new string[] { entryText1Val })
                                          .ChannelGroups( new string[] { entryChannelGroupValue })
                                          .State(doubleDic)
                                          .Async(new DemoPNSetStateResult(instance.Display));
                                });
                            } else {
                                InvokeInBackground(() => {
                                    Dictionary<string, object> strDic = new Dictionary<string, object>();
                                    strDic.Add(entryText2Val, entryText3Val);
                                    pubnub.SetPresenceState()
                                          .Channels(new string[] { entryText1Val })
                                          .ChannelGroups( new string[] { entryChannelGroupValue })
                                          .State(strDic)
                                          .Async(new DemoPNSetStateResult(instance.Display));
                                });
                            }
                        } else if (cds == CommonDialogStates.DeleteUserState) {
                            InvokeInBackground(() => {
                                Dictionary<string, object> delDic = new Dictionary<string, object>();
                                delDic.Add(entryText2Val, null);
                                pubnub.SetPresenceState()
                                      .Channels(new string[] { entryText1Val })
                                      .ChannelGroups( new string[] { entryChannelGroupValue })
                                      .State(delDic)
                                      .Async(new DemoPNSetStateResult(instance.Display));
                            });
                        } else if (cds == CommonDialogStates.GetUserState) {
                            Display ("Running get user state");
                            InvokeInBackground(() => {
                                pubnub.GetPresenceState()
                                      .Channels(new string[] { entryText1Val })
                                      .ChannelGroups(new string[] { entryChannelGroupValue })
                                      .Uuid(entryText2Val)
                                      .Async(new DemoPNGetStateResult(instance.Display));
                            });
                        }

                        AppDelegate.navigation.PopViewController (true);
                    }) {
                        BackgroundColor = UIColor.Blue,
                        TextColor = UIColor.White,
                        Alignment = UITextAlignment.Center
                    },
                },
            };
            dvc = new DialogViewController (newroot, true);
            AppDelegate.navigation.PushViewController (dvc, true);
        }

        void ShowAlertType1 (CommonDialogStates cds)
        {
            UIAlertView alert = new UIAlertView ();
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            bool isHeartbeatTimeout = false;
            string messageBoxTitle = "Set";

            if ((cds == CommonDialogStates.AuditSubscribe)
                     || (cds == CommonDialogStates.RevokeSubscribe)) {
                alert.Title = "Auth Key";
                alert.Message = "Enter Auth Key (Optional)";
                messageBoxTitle = cds.ToString ();
            } else if (cds == CommonDialogStates.PresenceHeartbeat) {
                alert.GetTextField (0).KeyboardType = UIKeyboardType.NumberPad;
                alert.Title = "Presence Heartbeat";
                alert.Message = "Enter Presence Heartbeat";
            //} else if (cds == CommonDialogStates.PresenceHeartbeatInterval) {
            //    isHeartbeatInterval = true;
            //    alert.GetTextField (0).KeyboardType = UIKeyboardType.NumberPad;
            //    alert.Title = "Presence Heartbeat Interval";
            //    alert.Message = "Enter Presence Heartbeat Interval";
            } else if (cds == CommonDialogStates.ChangeUuid) {
                alert.Title = "Change UUID";
                alert.Message = "Enter UUID";
            } else if (cds == CommonDialogStates.WhereNow) {
                alert.Title = "Where Now";
                alert.Message = "Enter UUID (optional)";
                messageBoxTitle = "Where Now";
            }

            alert.AddButton (messageBoxTitle);
            alert.AddButton ("Cancel");
            alert.Clicked += delegate(object sender, UIButtonEventArgs e) {
                if (e.ButtonIndex == 0) {
                    if (alert.GetTextField (0) != null) {
                        string input = alert.GetTextField (0).Text;
                        Channel = newChannels.Text;
                        if (cds == CommonDialogStates.AuditSubscribe) {
                            Display ("Running Subscribe Audit");
                            InvokeInBackground(() => {
                                pubnub.Audit()
                                      .Channel(Channel)
                                      .AuthKeys(new string [] { input })
                                      .Async(new DemoAuditResult(instance.Display));
                            });
                        } else if (cds == CommonDialogStates.RevokeSubscribe) {
                            Display ("Running Subscribe Revoke");
                            InvokeInBackground(() => {
                                pubnub.Grant()
                                      .Channels(new string [] { Channel })
                                      .AuthKeys(new string [] { input })
                                      .Read(false)
                                      .Write(false)
                                      .Async(new DemoGrantResult(instance.Display));
                            });
                        } else if (cds == CommonDialogStates.PresenceHeartbeat) {
                            int iVal;
                            Int32.TryParse (input, out iVal);
                            if (iVal != 0) {
                                InvokeInBackground (() => {
                                    config.PresenceTimeout = iVal;
                                    pubnub = new Pubnub (config);
                                });
                                Display (string.Format ("Presence Heartbeat Timeout set to {0}", config.PresenceTimeout));
                            } else {
                                Display (string.Format ("Value not numeric"));
                            }
                        } else if (cds == CommonDialogStates.ChangeUuid) {
                            InvokeInBackground(() => {
                                pubnub.ChangeUUID (input);
                            });
                            Display (string.Format ("UUID set to {0}", config.Uuid));
                        } else if (cds == CommonDialogStates.WhereNow) {
                            Display ("Running where now");
                            InvokeInBackground(() => {
                                pubnub.WhereNow().Uuid(input).Async(new DemoWhereNowResult(instance.Display));
                            });
                        }
                    }
                }           
            };
            alert.Show ();
        }

        void ShowAlertType2 (CommonDialogStates cds)
        {
            bool showEntryText = true;
            bool showEntryText2 = false;
            bool boolval1 = false;
            bool showBool = true;
            UIKeyboardType keyboardType = UIKeyboardType.Default;

            string strHead = "", elementText1 = "", elementText2 = "", 
            elementText3 = "", elementSubText = "", buttonTitle = "", elementText4 = "", elementSubText2 = "";
            if (cds == CommonDialogStates.SubscribeGrant) {
                elementText1 = "Read";
                elementText2 = "Write";
                elementText3 = "TTL";
                elementSubText = "Enter TTL (default 1440)";
                elementText4 = "Auth key";
                elementSubText2 = "optional";
                strHead = "Subscribe Grant";
                buttonTitle = "Grant";
                keyboardType = UIKeyboardType.NumberPad;
                showEntryText2 = true;
            } else if (cds == CommonDialogStates.HereNow) {
                elementText1 = "Show UUID";
                elementText2 = "Include User State";
                elementText3 = "Channel";
                elementSubText = "Enter channel name";
                strHead = "Here now";
                buttonTitle = "Here Now";

                boolval1 = true;
            } else if (cds == CommonDialogStates.GlobalHereNow) {
                elementText1 = "Show UUID";
                elementText2 = "Include User State";
                strHead = "Global Here Now";
                buttonTitle = "Global Here Now";
                showEntryText = false;
                boolval1 = true;
            } else if (cds == CommonDialogStates.Publish) {
                elementText1 = "Store in history";
                elementText4 = "Channel";
                elementSubText2 = "Enter channel name";
                elementText3 = "Message";
                elementSubText = "Enter message";
                strHead = "Publish";
                buttonTitle = "Publish";
                showEntryText = true;
                showEntryText2 = true;
                boolval1 = true;
                showBool = false;
            }

            BooleanElement be1 = new BooleanElement (elementText1, boolval1);
            BooleanElement be2 = null;
            if (showBool) {
                be2 = new BooleanElement (elementText2, false);
            }

            EntryElement entryText = null;
            EntryElement entryText2 = null;

            if (showEntryText) {
                entryText = new EntryElement (elementText3, elementSubText, "");
                entryText.KeyboardType = keyboardType;
                entryText.AutocapitalizationType = UITextAutocapitalizationType.None;
                entryText.AutocorrectionType = UITextAutocorrectionType.No;
            }
            if (showEntryText2) {
                entryText2 = new EntryElement (elementText4, elementSubText2, "");
                entryText2.AutocapitalizationType = UITextAutocapitalizationType.None;
                entryText2.AutocorrectionType = UITextAutocorrectionType.No;
            }

            var newroot = new RootElement (strHead, 0, 0) {
                new Section () {
                    be1,
                    be2,
                    entryText2,
                    entryText
                },
                new Section ("") {
                    new StyledStringElement (buttonTitle, () => {
                        bool be1Val = be1.Value;

                        Channel = newChannels.Text;
                        if (cds == CommonDialogStates.SubscribeGrant) {
                            string entryTextVal = entryText.Value;
                            string entryText2Val = entryText2.Value;
                            bool be2Val = be2.Value;
                            int iTtl;
                            Int32.TryParse (entryTextVal, out iTtl);
                            if (iTtl < 0) {
                                iTtl = 1440;
                                entryTextVal = "1440";
                            }

                            Display ("Running Subscribe Grant");
                            InvokeInBackground(() => {
                                pubnub.Grant()
                                      .Channels(new string[] { Channel })
                                      .AuthKeys(new string[] { entryText2Val })
                                      .Read(be1Val)
                                      .Write(be2Val)
                                      .TTL(iTtl)
                                      .Async(new DemoGrantResult(instance.Display));
                            });
                        } else if (cds == CommonDialogStates.HereNow) {
                            Display ("Running Here Now");
                            string entryTextVal = entryText.Value;
                            bool be2Val = be2.Value;
                            if(entryTextVal.Trim() != ""){
                                string[] channels = entryTextVal.Split (',');//Channel.Split (',');
                                foreach (string channel in channels) {
                                    InvokeInBackground(() => {
                                        pubnub.HereNow()
                                              .Channels(new string[] { channel.Trim () })
                                              .IncludeState(be1Val)
                                              .IncludeUUIDs(be2Val)
                                              .Async(new DemoHereNowResult(instance.Display));
                                    });
                                }
                            } else {
                                Display ("Channel empty");
                            }
                        } else if (cds == CommonDialogStates.GlobalHereNow) {
                            bool be2Val = be2.Value;
                            InvokeInBackground(() => {
                                pubnub.HereNow()
                                      .IncludeState(be1Val)
                                      .IncludeUUIDs(be2Val)
                                      .Async(new DemoHereNowResult(instance.Display));
                            });
                        } else if (cds == CommonDialogStates.Publish) {
                            Display ("Running Publish");
                            string entryTextVal = entryText.Value;
                            string entryText2Val = entryText2.Value;

                            string[] channels = entryText2Val.Split (',');

                            foreach (string channel in channels) {
                                InvokeInBackground(() => {
                                    pubnub.Publish()
                                          .Channel(channel.Trim ())
                                          .Message(entryTextVal)
                                          .ShouldStore(be1Val)
                                          .Async(new DemoPublishResult(instance.Display));
                                });
                            }
                        }

                        AppDelegate.navigation.PopViewController (true);
                    }) {
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
        void DisplayErrorMessage (string result)
        {
            Display (String.Format ("Error Callback - {0}", result));
        }


        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayConnectStatusMessage (string result)
        {
            Display (String.Format ("Connect Callback - {0}", result));
        }

        public void Display (string strText)
        {
            StyledMultilineElement sme = new StyledMultilineElement (strText) {
                Font = font12
            };
            ThreadPool.QueueUserWorkItem (delegate {
                
                System.Threading.Thread.Sleep (500);
                
                AppDelegate.navigation.BeginInvokeOnMainThread (delegate {
                    if (secOutput.Count > 20) {
                        secOutput.RemoveRange (20, secOutput.Count-1);
                    }
                    if (secOutput.Count > 0) {
                        secOutput.Insert (0, sme);
                    } else {
                        secOutput.Add (sme);
                    }                
                });
            });
        }

        void DisplayReturnMessage (string result)
        {
            Display (result);
        }

        void DisplayWildcardPresenceMessage(string result)
        {
            Display (result);
        }

    }

    public class PlatformPubnubLog : IPubnubLog
    {
        private string logFilePath = "";

        public PlatformPubnubLog ()
        {
            // Get folder path may vary based on environment
            //string folder = System.IO.Directory.GetCurrentDirectory (); //For console
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // For iOS
            System.Diagnostics.Debug.WriteLine (folder);
            logFilePath = System.IO.Path.Combine (folder, "pubnubmessaging.log");
            System.Diagnostics.Trace.Listeners.Add (new System.Diagnostics.TextWriterTraceListener (logFilePath));
        }

        public void WriteToLog (string log)
        {
            System.Diagnostics.Trace.WriteLine (log);
            System.Diagnostics.Trace.Flush ();
        }
    }

    public class DemoTimeResult : PNCallback<PNTimeResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
        public DemoSubscribeCallback (Action<string> displayCallback)
        {
            this.callback = displayCallback;
        }
        public override void Message<T> (Pubnub pubnub, PNMessageResult<T> message)
        {
            if (message != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (message));
            }
        }

        public override void Presence (Pubnub pubnub, PNPresenceEventResult presence)
        {
            if (presence != null) {
                this.callback (pubnub.JsonPluggableLibrary.SerializeToJsonString (presence));
            }
        }

        public override void Status (Pubnub pubnub, PNStatus status)
        {
            string msg = string.Format ("Operation: {0}; Category: {1};  StatusCode: {2}", status.Operation, status.Category, status.StatusCode);
            this.callback (msg);

            //if (status.StatusCode != 200 || status.Error)
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    if (status.ErrorData != null)
            //    {
            //        Console.WriteLine(status.ErrorData.Information);
            //    }
            //    Console.ForegroundColor = ConsoleColor.White;
            //}

            if (status.Category == PNStatusCategory.PNUnexpectedDisconnectCategory) {
                // This event happens when radio / connectivity is lost
            } else if (status.Category == PNStatusCategory.PNConnectedCategory) {
                //Console.WriteLine("CONNECTED {0} Channels = {1}, ChannelGroups = {2}", status.StatusCode, string.Join(",", status.AffectedChannels), string.Join(",", status.AffectedChannelGroups));
                // Connect event. You can do stuff like publish, and know you'll get it.
                // Or just use the connected event to confirm you are subscribed for
                // UI / internal notifications, etc

            } else if (status.Category == PNStatusCategory.PNReconnectedCategory) {
                //Console.WriteLine("RE-CONNECTED {0} Channels = {1}, ChannelGroups = {2}", status.StatusCode, string.Join(",", status.AffectedChannels), string.Join(",", status.AffectedChannelGroups));
                // Happens as part of our regular operation. This event happens when
                // radio / connectivity is lost, then regained.
            } else if (status.Category == PNStatusCategory.PNDecryptionErrorCategory) {
                // Handle messsage decryption error. Probably client configured to
                // encrypt messages and on live data feed it received plain text.
            }
        }
    }

    public class DemoGrantResult : PNCallback<PNAccessManagerGrantResult>
    {
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
        Action<string> callback = null;
        Pubnub pubnub = new Pubnub (null);
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
