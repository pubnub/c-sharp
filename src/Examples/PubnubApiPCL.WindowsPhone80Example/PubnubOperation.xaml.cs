using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using PubnubApi;
using System.Windows.Controls.Primitives;

using Microsoft.Phone.Notification;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PubnubWindowsPhone
{
    public partial class PubnubOperation : PhoneApplicationPage
    {
        Pubnub pubnub;
        string channel = "";
        string channelGroup = "";
        bool ssl = false;
        string origin = "";
        string publishKey = "";
        string subscribeKey = "";
        string secretKey = "";
        string cipherKey = "";
        string uuid = "";
        bool resumeOnReconnect = false;
        bool hideErrorCallbackMsg = true;
        string authKey = "";

        int subscribeTimeoutInSeconds;
        int operationTimeoutInSeconds;
        int networkMaxRetries;
        int networkRetryIntervalInSeconds;
        int localClientHeartbeatIntervalInSeconds;
        int presenceHeartbeat;
        int presenceHeartbeatInterval;

        Popup publishPopup = null;
        Popup hereNowPopup = null;
        Popup whereNowPopup = null;
        Popup globalHereNowPopup = null;
        Popup userStatePopup = null;
        Popup changeUUIDPopup = null;
        Popup pushNotificationPopup = null;

        string microsoftChannelName = "pushSampleChannel";
        ResponseType currentPushReqType;

        public PubnubOperation()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            pubnub = new Pubnub(publishKey, subscribeKey, secretKey, cipherKey, ssl);
            pubnub.Origin = origin;
            pubnub.SessionUUID = uuid;
            pubnub.SubscribeTimeout = subscribeTimeoutInSeconds;
            pubnub.NonSubscribeTimeout = operationTimeoutInSeconds;
            pubnub.NetworkCheckMaxRetries = networkMaxRetries;
            pubnub.NetworkCheckRetryInterval = networkRetryIntervalInSeconds;
            pubnub.LocalClientHeartbeatInterval = localClientHeartbeatIntervalInSeconds;
            pubnub.EnableResumeOnReconnect = resumeOnReconnect;
            pubnub.AuthenticationKey = authKey;
            pubnub.PresenceHeartbeat = presenceHeartbeat;
            pubnub.PresenceHeartbeatInterval = presenceHeartbeatInterval;
            pubnub.PushServiceName = ""; //overriding default push service name to use non-ssl for push
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ssl = Boolean.Parse(NavigationContext.QueryString["ssl"].ToString());
            origin = NavigationContext.QueryString["origin"].ToString();
            publishKey = NavigationContext.QueryString["publishKey"].ToString();
            subscribeKey = NavigationContext.QueryString["subscribeKey"].ToString();
            cipherKey = NavigationContext.QueryString["cipherkey"].ToString();
            secretKey = NavigationContext.QueryString["secretkey"].ToString();
            uuid = NavigationContext.QueryString["uuid"].ToString();
            authKey = NavigationContext.QueryString["authKey"].ToString();

            subscribeTimeoutInSeconds = Convert.ToInt32(NavigationContext.QueryString["subtimeout"]);
            operationTimeoutInSeconds = Convert.ToInt32(NavigationContext.QueryString["optimeout"]);
            networkMaxRetries = Convert.ToInt32(NavigationContext.QueryString["retries"]);
            networkRetryIntervalInSeconds = Convert.ToInt32(NavigationContext.QueryString["retryinterval"]);
            localClientHeartbeatIntervalInSeconds = Convert.ToInt32(NavigationContext.QueryString["localbeatinterval"]);
            resumeOnReconnect = Boolean.Parse(NavigationContext.QueryString["resumeOnReconnect"].ToString());
            hideErrorCallbackMsg = Boolean.Parse(NavigationContext.QueryString["hideErrCallbackMsg"].ToString());
            presenceHeartbeat = Convert.ToInt32(NavigationContext.QueryString["prebeat"]);
            presenceHeartbeatInterval = Convert.ToInt32(NavigationContext.QueryString["prebeatinterval"]);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (publishPopup != null && publishPopup.IsOpen)
            {
                publishPopup.IsOpen = false;
                this.IsEnabled = true;
                e.Cancel = true;
            }
            pubnub.EndPendingRequests();
            base.OnBackKeyPress(e);
            
        }

        private void btnTime_Click(object sender, RoutedEventArgs e)
        {
            pubnub.Time(DisplayTimeReturnMessage, PubnubDisplayErrorMessage);
        }

        private void PubnubCallbackResult(string result)
        {
            Deployment.Current.Dispatcher.BeginInvoke(
                () =>
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.TextWrapping = TextWrapping.Wrap;
                        if (result.Length > 3000)
                        {
                            result = string.Format("{0}..LONG TEXT DATA TRUNCATED",result.Substring(0, 3000));
                        }
                        textBlock.Text = string.Format("REGULAR CALLBACK: {0}", result);
                        messageStackPanel.Children.Add(textBlock);
                        scrollViewerResult.UpdateLayout();
                        scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                    }
                );
        }

        private void PubnubConnectCallbackResult(string result)
        {
            Deployment.Current.Dispatcher.BeginInvoke(
                () =>
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Text = string.Format("CONNECT CALLBACK: {0}", result);
                    messageStackPanel.Children.Add(textBlock);
                    scrollViewerResult.UpdateLayout();
                    scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                }
                );
        }

        private void PubnubDisconnectCallbackResult(string result)
        {
            Deployment.Current.Dispatcher.BeginInvoke(
                () =>
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Text = string.Format("DISCONNECT CALLBACK: {0}",result);
                    messageStackPanel.Children.Add(textBlock);
                    scrollViewerResult.UpdateLayout();
                    scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                }
                );
        }

        private async void DisplayMessageInTextBox(string result)
        {
            PubnubCallbackResult(result);
        }

        void DisplayRemoveChannelGroupNamespaceReturnMessage(RemoveNamespaceAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayRemoveChannelGroupReturnMessage(RemoveChannelGroupAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayRemoveChannelFromChannelGroupReturnMessage(RemoveChannelFromChannelGroupAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayAddChannelToChannelGroupReturnMessage(AddChannelToChannelGroupAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayAllNamespacesReturnMessage(GetAllNamespacesAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayGetAllChannelGroupsReturnMessage(GetAllChannelGroupsAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayGetChannelsForChannelGroupReturnMessage(GetChannelGroupChannelsAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayTimeReturnMessage(long result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayGetUserStateReturnMessage(GetUserStateAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplaySetUserStateReturnMessage(SetUserStateAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayWhereNowReturnMessage(WhereNowAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayGlobalHereNowReturnMessage(GlobalHereNowAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayHereNowReturnMessage(HereNowAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayDetailedHistoryReturnMessage(DetailedHistoryAck result)
        {
            for (int index = 0; index < result.Message.Length; index++)
            {
                DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result.Message[index]));
            }
        }

        void DisplayPublishReturnMessage(PublishAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayPresenceAckMessage(PresenceAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        /// <summary>
        /// Callback method captures the response for Grant requests
        /// </summary>
        /// <param name="result"></param>
        void DisplayGrantReturnMessage(GrantAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        /// <summary>
        /// Callback method captures the response for Audit requests
        /// </summary>
        /// <param name="result"></param>
        void DisplayAuditReturnMessage(AuditAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Subscribe
        /// </summary>
        /// <param name="result"></param>
        void DisplaySubscribeReturnMessage(Message<string> result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }
        void DisplaySubscribeReturnMessage(Message<object> result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }
        void DisplaySubscribeReturnMessage(Message<PubnubDemoObject> result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }
        void DisplaySubscribeReturnMessage(Message<UserCreated> result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }


        /// <summary>
        /// Callback method captures wildcard presence events for Subscribe
        /// </summary>
        /// <param name="result"></param>
        void DisplayWildCardPresenceReturnMessage(PresenceAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Presence
        /// </summary>
        /// <param name="result"></param>
        void DisplayPresenceReturnMessage(PresenceAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        void DisplayConnectDisconnectStatusMessage(ConnectOrDisconnectAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        /// <summary>
        /// Callback method to provide the connect status of Presence call
        /// </summary>
        /// <param name="result"></param>
        void DisplayPresenceConnectStatusMessage(ConnectOrDisconnectAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplaySubscribeDisconnectStatusMessage(ConnectOrDisconnectAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        void DisplayPresenceDisconnectStatusMessage(ConnectOrDisconnectAck result)
        {
            DisplayMessageInTextBox(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
        }

        private void PubnubDisplayErrorMessage(PubnubClientError result)
        {
            if (!hideErrorCallbackMsg)
            {
                Deployment.Current.Dispatcher.BeginInvoke(
                    () =>
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.TextWrapping = TextWrapping.Wrap;
                        textBlock.Text = string.Format("ERROR CALLBACK: {0}", result.Description);
                        //textBlock.Text = string.Format("ERROR CALLBACK: {0}; {1}; {2}", result.Description, result.DetailedDotNetException, result.Message);
                        if (result.PubnubWebRequest != null)
                        {
                            LoggingMethod.WriteToLog(result.PubnubWebRequest.RequestUri.ToString(), true);
                        }
                        messageStackPanel.Children.Add(textBlock);
                        scrollViewerResult.UpdateLayout();
                        scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                    }
                    );
            }
        }

        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel publishStackPanel = new StackPanel();
            publishStackPanel.Background = new SolidColorBrush(Colors.Blue);
            publishStackPanel.Width = 300;
            publishStackPanel.Height = 600;

            publishPopup = new Popup();
            publishPopup.Height = 600;
            publishPopup.Width = 550;
            publishPopup.VerticalOffset = 100;
            publishPopup.HorizontalOffset = 100;
            PublishMessageUserControl control = new PublishMessageUserControl();
            publishStackPanel.Children.Add(control);
            border.Child = publishStackPanel;

            publishPopup.Child = border;
            publishPopup.IsOpen = true;
            control.btnOK.Click += (s, args) =>
            {
                publishPopup.IsOpen = false;
                string publishedMessage = "";
                if (control.radNormalPublish.IsChecked.Value)
                {
                    publishedMessage = control.txtPublish.Text;
                    bool storeInHistory = control.chkStoreInHistory.IsChecked.Value;
                    pubnub.Publish(channel, publishedMessage, storeInHistory, "", DisplayPublishReturnMessage, PubnubDisplayErrorMessage);
                }
                else if (control.radToastPublish.IsChecked.Value)
                {
                    MpnsToastNotification toast = new MpnsToastNotification();
                    //toast.type = "toast";
                    toast.text1 = "hardcode message";
                    Dictionary<string, object> dicToast = new Dictionary<string, object>();
                    dicToast.Add("pn_mpns", toast);
                    
                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish(channel, dicToast, DisplayPublishReturnMessage, PubnubDisplayErrorMessage);
                }
                else if (control.radFlipTilePublish.IsChecked.Value)
                {
                    pubnub.PushRemoteImageDomainUri.Add(new Uri("http://cdn.flaticon.com"));

                    MpnsFlipTileNotification tile = new MpnsFlipTileNotification();
                    tile.title = "front title";
                    tile.count = 6;
                    tile.back_title = "back title";
                    tile.back_content = "back message";
                    tile.back_background_image = "Assets/Tiles/pubnub3.png";
                    tile.background_image = "http://cdn.flaticon.com/png/256/37985.png";
                    Dictionary<string, object> dicTile = new Dictionary<string, object>();
                    dicTile.Add("pn_mpns", tile);
                    
                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish(channel, dicTile, DisplayPublishReturnMessage, PubnubDisplayErrorMessage);
                }
                else if (control.radCycleTilePublish.IsChecked.Value)
                {
                    MpnsCycleTileNotification tile = new MpnsCycleTileNotification();
                    tile.title = "front title";
                    tile.count = 2;
                    tile.images = new string[] { "Assets/Tiles/pubnub1.png", "Assets/Tiles/pubnub2.png", "Assets/Tiles/pubnub3.png", "Assets/Tiles/pubnub4.png" };
                    
                    Dictionary<string, object> dicTile = new Dictionary<string, object>();
                    dicTile.Add("pn_mpns", tile);
                    
                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish(channel, dicTile, DisplayPublishReturnMessage, PubnubDisplayErrorMessage);
                }
                else if (control.radIconicTilePublish.IsChecked.Value)
                {
                    MpnsIconicTileNotification tile = new MpnsIconicTileNotification();
                    tile.title = "front title";
                    tile.count = 2;
                    tile.wide_content_1 = "my wide content";
                    
                    Dictionary<string, object> dicTile = new Dictionary<string, object>();
                    dicTile.Add("pn_mpns", tile);

                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish(channel, dicTile, DisplayPublishReturnMessage, PubnubDisplayErrorMessage);
                }

                TextBlock textBlock = new TextBlock();
                textBlock.Text = string.Format("Publishing {0}\n", publishedMessage);
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                publishPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                publishPopup.IsOpen = false;
                publishPopup = null;
                this.IsEnabled = true;
            };

        }

        private void btnHereNow_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel herenowStackPanel = new StackPanel();
            herenowStackPanel.Background = new SolidColorBrush(Colors.Blue);
            herenowStackPanel.Width = 300;
            herenowStackPanel.Height = 300;

            hereNowPopup = new Popup();
            hereNowPopup.Height = 300;
            hereNowPopup.Width = 300;
            hereNowPopup.VerticalOffset = 100;
            hereNowPopup.HorizontalOffset = 100;
            HereNowOptionsUserControl control = new HereNowOptionsUserControl();
            herenowStackPanel.Children.Add(control);
            border.Child = herenowStackPanel;

            hereNowPopup.Child = border;
            hereNowPopup.IsOpen = true;
            control.btnOK.Click += (s, args) =>
            {
                hereNowPopup.IsOpen = false;
                bool showUUID = control.chkHereNowShowUUID.IsChecked.Value;
                bool includeState = control.chkHereIncludeUserState.IsChecked.Value;

                pubnub.HereNow(channel.Split(','), showUUID, includeState, DisplayHereNowReturnMessage, PubnubDisplayErrorMessage);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = string.Format("Running HereNow\n");
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                hereNowPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                hereNowPopup.IsOpen = false;
                hereNowPopup = null;
                this.IsEnabled = true;
            };

        }

        private void btnWhereNow_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel whereNowStackPanel = new StackPanel();
            whereNowStackPanel.Background = new SolidColorBrush(Colors.Blue);
            whereNowStackPanel.Width = 300;
            whereNowStackPanel.Height = 300;

            whereNowPopup = new Popup();
            whereNowPopup.Height = 300;
            whereNowPopup.Width = 300;
            whereNowPopup.VerticalOffset = 100;
            whereNowPopup.HorizontalOffset = 100;
            WhereNowUserControl control = new WhereNowUserControl();
            control.txtWhereNowUUID.Text = uuid;
            whereNowStackPanel.Children.Add(control);
            border.Child = whereNowStackPanel;

            whereNowPopup.Child = border;
            whereNowPopup.IsOpen = true;
            control.btnOK.Click += (s, args) =>
            {
                whereNowPopup.IsOpen = false;
                string whereNowUUID = control.txtWhereNowUUID.Text;
                pubnub.WhereNow(whereNowUUID, DisplayWhereNowReturnMessage, PubnubDisplayErrorMessage);
                TextBlock textBlock = new TextBlock();
                textBlock.Text = string.Format("Running WhereNow \n");
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                whereNowPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                whereNowPopup.IsOpen = false;
                whereNowPopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnChangeUUID_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel changeUUIDStackPanel = new StackPanel();
            changeUUIDStackPanel.Background = new SolidColorBrush(Colors.Blue);
            changeUUIDStackPanel.Width = 400;
            changeUUIDStackPanel.Height = 300;

            changeUUIDPopup = new Popup();
            changeUUIDPopup.Height = 300;
            changeUUIDPopup.Width = 300;
            changeUUIDPopup.VerticalOffset = 10;
            changeUUIDPopup.HorizontalOffset = 10;
            ChangeUUIDUserControl control = new ChangeUUIDUserControl();
            control.lblCurrentUUID.Text = uuid;
            changeUUIDStackPanel.Children.Add(control);
            border.Child = changeUUIDStackPanel;

            changeUUIDPopup.Child = border;
            changeUUIDPopup.IsOpen = true;
            control.btnOK.Click += (s, args) =>
            {
                changeUUIDPopup.IsOpen = false;
                string newUUID = control.txtNewUUID.Text;
                if (!string.IsNullOrEmpty(newUUID) && newUUID.Trim().Length > 0)
                {
                    uuid = newUUID;
                    pubnub.ChangeUUID(uuid);

                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = string.Format("Change UUID done.\n");
                    messageStackPanel.Children.Add(textBlock);
                    scrollViewerResult.UpdateLayout();
                    scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                }
                changeUUIDPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                changeUUIDPopup.IsOpen = false;
                changeUUIDPopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnGlobalHereNow_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel globalHerenowStackPanel = new StackPanel();
            globalHerenowStackPanel.Background = new SolidColorBrush(Colors.Blue);
            globalHerenowStackPanel.Width = 300;
            globalHerenowStackPanel.Height = 300;

            globalHereNowPopup = new Popup();
            globalHereNowPopup.Height = 300;
            globalHereNowPopup.Width = 300;
            globalHereNowPopup.VerticalOffset = 100;
            globalHereNowPopup.HorizontalOffset = 100;
            HereNowOptionsUserControl control = new HereNowOptionsUserControl();
            globalHerenowStackPanel.Children.Add(control);
            border.Child = globalHerenowStackPanel;

            globalHereNowPopup.Child = border;
            globalHereNowPopup.IsOpen = true;
            control.btnOK.Click += (s, args) =>
            {
                globalHereNowPopup.IsOpen = false;
                bool showUUID = control.chkHereNowShowUUID.IsChecked.Value;
                bool includeState = control.chkHereIncludeUserState.IsChecked.Value;

                pubnub.GlobalHereNow(showUUID, includeState, DisplayGlobalHereNowReturnMessage, PubnubDisplayErrorMessage);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = string.Format("Running Global HereNow\n");
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                globalHereNowPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                globalHereNowPopup.IsOpen = false;
                globalHereNowPopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnUserState_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text;
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel userStateStackPanel = new StackPanel();
            userStateStackPanel.Background = new SolidColorBrush(Colors.Blue);
            userStateStackPanel.Width = 600;
            userStateStackPanel.Height = 600;

            userStatePopup = new Popup();
            userStatePopup.Height = 300;
            userStatePopup.Width = 300;
            userStatePopup.VerticalOffset = 20;
            userStatePopup.HorizontalOffset = 20;
            UserStateUserControl control = new UserStateUserControl();
            control.txtGetUserStateUUID.Text = uuid;
            userStateStackPanel.Children.Add(control);
            border.Child = userStateStackPanel;

            userStatePopup.Child = border;
            userStatePopup.IsOpen = true;
            control.btnOK.Click += (s, args) =>
            {
                string msg = "";
                userStatePopup.IsOpen = false;
                if (control.radSetUserState.IsChecked.Value)
                {
                    string userStateKey1 = control.txtKey1.Text;
                    string userStateValue1 = control.txtValue1.Text;

                    int valueInt;
                    double valueDouble;
                    if (Int32.TryParse(userStateValue1, out valueInt))
                    {
                        pubnub.SetUserState(channel, channelGroup, new KeyValuePair<string, object>(userStateKey1, valueInt), DisplaySetUserStateReturnMessage, PubnubDisplayErrorMessage);
                    }
                    else if (Double.TryParse(userStateValue1, out valueDouble))
                    {
                        pubnub.SetUserState(channel, channelGroup, new KeyValuePair<string, object>(userStateKey1, valueDouble), DisplaySetUserStateReturnMessage, PubnubDisplayErrorMessage);
                    }
                    else
                    {
                        pubnub.SetUserState(channel, channelGroup, new KeyValuePair<string, object>(userStateKey1, userStateValue1), DisplaySetUserStateReturnMessage, PubnubDisplayErrorMessage);
                    }

                    msg = "Running Set User State";
                }
                else
                {
                    string userStateUUID = control.txtGetUserStateUUID.Text;
                    pubnub.GetUserState(channel, channelGroup, userStateUUID, DisplayGetUserStateReturnMessage, PubnubDisplayErrorMessage);
                    msg = "Running Get User State";
                }

                TextBlock textBlock = new TextBlock();
                textBlock.Text = msg;
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                userStatePopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                userStatePopup.IsOpen = false;
                userStatePopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnPresence_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text;
            if (channelGroup != "")
            {
                pubnub.Presence(channel, channelGroup, DisplayPresenceAckMessage, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, PubnubDisplayErrorMessage);
            }
            else
            {
                pubnub.Presence(channel, DisplayPresenceAckMessage, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, PubnubDisplayErrorMessage);
            }
        }

        private void btnDetailedHistory_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            pubnub.DetailedHistory(channel, 10, false, DisplayDetailedHistoryReturnMessage, PubnubDisplayErrorMessage);
        }

        private void scrollViewerResult_DoubleTap(object sender, GestureEventArgs e)
        {
            MessageBoxResult action = MessageBox.Show("Delete?", "Confirm", MessageBoxButton.OKCancel);
            if (action == MessageBoxResult.OK)
            {
                messageStackPanel.Children.Clear();
            }
        }

        private void btnUnsubscribe_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text;
            if (channelGroup != "")
            {
                pubnub.Unsubscribe<string>(channel, channelGroup, PubnubDisplayErrorMessage);
            }
            else
            {
                pubnub.Unsubscribe<string>(channel, PubnubDisplayErrorMessage);
            }
        }

        private void btnPresenceUnsub_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text;
            if (channelGroup != "")
            {
                pubnub.PresenceUnsubscribe(channel, channelGroup, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, PubnubDisplayErrorMessage);
            }
            else
            {
                pubnub.PresenceUnsubscribe(channel, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, PubnubDisplayErrorMessage);
            }
        }

        private void btnSubscribe_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text;
            if (channelGroup != "")
            {
                pubnub.Subscribe<string>(channel, channelGroup, DisplaySubscribeReturnMessage, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, PubnubDisplayErrorMessage);
            }
            else
            {
                pubnub.Subscribe<string>(channel, DisplaySubscribeReturnMessage, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, PubnubDisplayErrorMessage);
            }
        }

        private void btnDisableNetwork_Click(object sender, RoutedEventArgs e)
        {
            pubnub.EnableSimulateNetworkFailForTestingOnly();
        }

        private void btnEnableNetwork_Click(object sender, RoutedEventArgs e)
        {
            pubnub.DisableSimulateNetworkFailForTestingOnly();
        }

        private void btnDisconnectRetry_Click(object sender, RoutedEventArgs e)
        {
            pubnub.TerminateCurrentSubscriberRequest();
        }

        private void btnChannelGroup_Click(object sender, RoutedEventArgs e)
        {
            channelGroup = txtChannelGroup.Text;

            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel channelGroupStackPanel = new StackPanel();
            channelGroupStackPanel.Background = new SolidColorBrush(Colors.Blue);
            channelGroupStackPanel.Width = 400;
            channelGroupStackPanel.Height = 440;

            Popup channelGroupPopup = new Popup();
            channelGroupPopup.Height = 420;
            channelGroupPopup.Width = 300;

            channelGroupPopup.HorizontalOffset = 10;
            channelGroupPopup.VerticalOffset = 100;

            ChannelGroupUserControl control = new ChannelGroupUserControl();

            control.txtChannelGroup.Text = channelGroup;
            control.txtChannelName.Text = "ch1";

            channelGroupStackPanel.Children.Add(control);
            border.Child = channelGroupStackPanel;

            channelGroupPopup.Child = border;
            channelGroupPopup.IsOpen = true;

            control.btnOK.Click += (s, args) =>
            {
                channelGroupPopup.IsOpen = false;
                string msg = "";
                string userChannelGroup = control.txtChannelGroup.Text; ;
                string userChannelName = control.txtChannelName.Text;

                if (control.radGetChannelsOfChannelGroup.IsChecked.Value)
                {
                     msg = "Running GetChannelsForChannelGroup";
                     pubnub.GetChannelsForChannelGroup(userChannelGroup, DisplayGetChannelsForChannelGroupReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radAddChannelToChannelGroup.IsChecked.Value)
                {
                     msg = "Running AddChannelsToChannelGroup";
                     pubnub.AddChannelsToChannelGroup(new string[] { userChannelName }, userChannelGroup, DisplayAddChannelToChannelGroupReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radRemoveChannelFromChannelGroup.IsChecked.Value)
                {
                     msg = "Running RemoveChannelsFromChannelGroup";
                     pubnub.RemoveChannelsFromChannelGroup(new string[] { userChannelName }, userChannelGroup, DisplayRemoveChannelFromChannelGroupReturnMessage, PubnubDisplayErrorMessage);
                }

                TextBlock textBlock = new TextBlock();
                textBlock.Text = msg;
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                channelGroupPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                channelGroupPopup.IsOpen = false;
                channelGroupPopup = null;
                this.IsEnabled = true;
            };

        }

        private void btnPAMChannel_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;

            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pamChannelStackPanel = new StackPanel();
            pamChannelStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pamChannelStackPanel.Width = 400;
            pamChannelStackPanel.Height = 650;

            Popup pamChannelPopup = new Popup();
            pamChannelPopup.Height = 650;
            pamChannelPopup.Width = 400;

            pamChannelPopup.HorizontalOffset = 10;
            pamChannelPopup.VerticalOffset = 10;

            PAMChannelUserControl control = new PAMChannelUserControl();

            control.txtChannelName.Text = channel;

            pamChannelStackPanel.Children.Add(control);
            border.Child = pamChannelStackPanel;

            pamChannelPopup.Child = border;
            pamChannelPopup.IsOpen = true;

            control.btnOK.Click += (s, args) =>
            {
                pamChannelPopup.IsOpen = false;
                string msg = "";
                string pamUserChannelName = control.txtChannelName.Text;
                string pamAuthKey = control.txtAuthKey.Text;

                if (control.radGrantChannel.IsChecked.Value)
                {
                    msg = "Running GrantAccess";
                    int ttlInMinutes = 1440;
                    pubnub.GrantAccess(pamUserChannelName, pamAuthKey, true, true, ttlInMinutes, DisplayGrantReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radAuditChannel.IsChecked.Value)
                {
                    msg = "Running AuditAccess";
                    pubnub.AuditAccess(pamUserChannelName, pamAuthKey, DisplayAuditReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radRevokeChannel.IsChecked.Value)
                {
                    msg = "Running Revoke Access";
                    pubnub.GrantAccess(pamUserChannelName, pamAuthKey, false, false, DisplayGrantReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radGrantPresenceChannel.IsChecked.Value)
                {
                    msg = "Running GrantPresenceAccess";
                    int ttlInMinutes = 1440;
                    pubnub.GrantPresenceAccess(pamUserChannelName, pamAuthKey, true, true, ttlInMinutes, DisplayGrantReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radAuditPresenceChannel.IsChecked.Value)
                {
                    msg = "Running AuditPresenceAccess";
                    pubnub.AuditPresenceAccess(pamUserChannelName, pamAuthKey, DisplayAuditReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radRevokePresenceChannel.IsChecked.Value)
                {
                    msg = "Running Presence Revoke Access";
                    pubnub.GrantPresenceAccess(pamUserChannelName, pamAuthKey, false, false, DisplayGrantReturnMessage, PubnubDisplayErrorMessage);
                }
                
                TextBlock textBlock = new TextBlock();
                textBlock.Text = msg;
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                pamChannelPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                pamChannelPopup.IsOpen = false;
                pamChannelPopup = null;
                this.IsEnabled = true;
            };

        }

        private void btnPAMChannelGroup_Click(object sender, RoutedEventArgs e)
        {
            channelGroup = txtChannelGroup.Text;

            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pamChannelGroupStackPanel = new StackPanel();
            pamChannelGroupStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pamChannelGroupStackPanel.Width = 400;
            pamChannelGroupStackPanel.Height = 650;

            Popup pamChannelGroupPopup = new Popup();
            pamChannelGroupPopup.Height = 650;
            pamChannelGroupPopup.Width = 300;

            pamChannelGroupPopup.HorizontalOffset = 10;
            pamChannelGroupPopup.VerticalOffset = 10;

            PAMChannelGroupUserControl control = new PAMChannelGroupUserControl();

            control.txtChannelGroup.Text = channelGroup;

            pamChannelGroupStackPanel.Children.Add(control);
            border.Child = pamChannelGroupStackPanel;

            pamChannelGroupPopup.Child = border;
            pamChannelGroupPopup.IsOpen = true;

            control.btnOK.Click += (s, args) =>
            {
                pamChannelGroupPopup.IsOpen = false;
                string msg = "";
                string pamUserChannelGroup = control.txtChannelGroup.Text;
                string pamAuthKey = control.txtAuthKey.Text;
                int ttlInMinutes = 1440;

                if (control.radGrantPresenceChannelGroup.IsChecked.Value)
                {
                    msg = "Running ChannelGroupGrantAccess";
                    pubnub.ChannelGroupGrantAccess(pamUserChannelGroup, pamAuthKey, true, true, ttlInMinutes, DisplayGrantReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radAuditChannelGroup.IsChecked.Value)
                {
                    msg = "Running ChannelGroupAuditAccess";
                    pubnub.ChannelGroupAuditAccess(pamUserChannelGroup, pamAuthKey, DisplayAuditReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radRevokeChannelGroup.IsChecked.Value)
                {
                    msg = "Running ChannelGroup Revoke Access";
                    pubnub.ChannelGroupGrantAccess(pamUserChannelGroup, pamAuthKey, false, false, DisplayGrantReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radGrantPresenceChannelGroup.IsChecked.Value)
                {
                    msg = "Running ChannelGroupGrantPresenceAccess";
                    pubnub.ChannelGroupGrantPresenceAccess(pamUserChannelGroup, pamAuthKey, true, true, ttlInMinutes, DisplayGrantReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radAuditPresenceChannelGroup.IsChecked.Value)
                {
                    msg = "Running ChannelGroupAuditAccess";
                    pubnub.ChannelGroupAuditPresenceAccess(pamUserChannelGroup, pamAuthKey, DisplayAuditReturnMessage, PubnubDisplayErrorMessage);
                }

                if (control.radRevokePresenceChannelGroup.IsChecked.Value)
                {
                    msg = "Running ChannelGroup Revoke Access";
                    pubnub.ChannelGroupGrantPresenceAccess(pamUserChannelGroup, pamAuthKey, false, false, DisplayGrantReturnMessage, PubnubDisplayErrorMessage);
                }
                
                TextBlock textBlock = new TextBlock();
                textBlock.Text = msg;
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                pamChannelGroupPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                pamChannelGroupPopup.IsOpen = false;
                pamChannelGroupPopup = null;
                this.IsEnabled = true;
            };

        }

        //private void btnGrant_Click(object sender, RoutedEventArgs e)
        //{
        //    this.IsEnabled = false;
        //    Border border = new Border();
        //    border.BorderBrush = new SolidColorBrush(Colors.Black);
        //    border.BorderThickness = new Thickness(5.0);

        //    StackPanel pamAuthKeyStackPanel = new StackPanel();
        //    pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
        //    pamAuthKeyStackPanel.Width = 400;
        //    pamAuthKeyStackPanel.Height = 300;

        //    Popup pamAuthKeyPopup = new Popup();
        //    pamAuthKeyPopup.Height = 300;
        //    pamAuthKeyPopup.Width = 300;
        //    pamAuthKeyPopup.VerticalOffset = 10;
        //    pamAuthKeyPopup.HorizontalOffset = 10;
        //    PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
        //    control.txtPAMAuthKey.Text = "";
        //    pamAuthKeyStackPanel.Children.Add(control);
        //    border.Child = pamAuthKeyStackPanel;

        //    pamAuthKeyPopup.Child = border;
        //    pamAuthKeyPopup.IsOpen = true;

        //    control.btnOK.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
        //        channel = txtChannel.Text;
        //        pubnub.GrantAccess<string>(channel, pamAuthKey, true, true, PubnubCallbackResult, PubnubDisplayErrorMessage);

        //        TextBlock textBlock = new TextBlock();
        //        textBlock.Text = string.Format("Grant request ...\n");
        //        messageStackPanel.Children.Add(textBlock);
        //        scrollViewerResult.UpdateLayout();
        //        scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };
        //    control.btnCancel.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };

        //}

        //private void btnRevoke_Click(object sender, RoutedEventArgs e)
        //{
        //    this.IsEnabled = false;
        //    Border border = new Border();
        //    border.BorderBrush = new SolidColorBrush(Colors.Black);
        //    border.BorderThickness = new Thickness(5.0);

        //    StackPanel pamAuthKeyStackPanel = new StackPanel();
        //    pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
        //    pamAuthKeyStackPanel.Width = 400;
        //    pamAuthKeyStackPanel.Height = 300;

        //    Popup pamAuthKeyPopup = new Popup();
        //    pamAuthKeyPopup.Height = 300;
        //    pamAuthKeyPopup.Width = 300;
        //    pamAuthKeyPopup.VerticalOffset = 10;
        //    pamAuthKeyPopup.HorizontalOffset = 10;
        //    PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
        //    control.txtPAMAuthKey.Text = "";
        //    pamAuthKeyStackPanel.Children.Add(control);
        //    border.Child = pamAuthKeyStackPanel;

        //    pamAuthKeyPopup.Child = border;
        //    pamAuthKeyPopup.IsOpen = true;

        //    control.btnOK.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
        //        channel = txtChannel.Text;
        //        pubnub.GrantAccess<string>(channel, pamAuthKey, false, false, PubnubCallbackResult, PubnubDisplayErrorMessage);

        //        TextBlock textBlock = new TextBlock();
        //        textBlock.Text = string.Format("Revoke Grant request ....\n");
        //        messageStackPanel.Children.Add(textBlock);
        //        scrollViewerResult.UpdateLayout();
        //        scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };
        //    control.btnCancel.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };
            
        //}

        //private void btnAudit_Click(object sender, RoutedEventArgs e)
        //{
        //    this.IsEnabled = false;
        //    Border border = new Border();
        //    border.BorderBrush = new SolidColorBrush(Colors.Black);
        //    border.BorderThickness = new Thickness(5.0);

        //    StackPanel pamAuthKeyStackPanel = new StackPanel();
        //    pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
        //    pamAuthKeyStackPanel.Width = 400;
        //    pamAuthKeyStackPanel.Height = 300;

        //    Popup pamAuthKeyPopup = new Popup();
        //    pamAuthKeyPopup.Height = 300;
        //    pamAuthKeyPopup.Width = 300;
        //    pamAuthKeyPopup.VerticalOffset = 10;
        //    pamAuthKeyPopup.HorizontalOffset = 10;
        //    PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
        //    control.txtPAMAuthKey.Text = "";
        //    pamAuthKeyStackPanel.Children.Add(control);
        //    border.Child = pamAuthKeyStackPanel;

        //    pamAuthKeyPopup.Child = border;
        //    pamAuthKeyPopup.IsOpen = true;

        //    control.btnOK.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
        //        channel = txtChannel.Text;
        //        pubnub.AuditAccess<string>(channel, pamAuthKey, PubnubCallbackResult, PubnubDisplayErrorMessage);

        //        TextBlock textBlock = new TextBlock();
        //        textBlock.Text = string.Format("Audit request ...\n");
        //        messageStackPanel.Children.Add(textBlock);
        //        scrollViewerResult.UpdateLayout();
        //        scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };
        //    control.btnCancel.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };
        //}

        //private void btnGrantPresence_Click(object sender, RoutedEventArgs e)
        //{
        //    this.IsEnabled = false;
        //    Border border = new Border();
        //    border.BorderBrush = new SolidColorBrush(Colors.Black);
        //    border.BorderThickness = new Thickness(5.0);

        //    StackPanel pamAuthKeyStackPanel = new StackPanel();
        //    pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
        //    pamAuthKeyStackPanel.Width = 400;
        //    pamAuthKeyStackPanel.Height = 300;

        //    Popup pamAuthKeyPopup = new Popup();
        //    pamAuthKeyPopup.Height = 300;
        //    pamAuthKeyPopup.Width = 300;
        //    pamAuthKeyPopup.VerticalOffset = 10;
        //    pamAuthKeyPopup.HorizontalOffset = 10;
        //    PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
        //    control.txtPAMAuthKey.Text = "";
        //    pamAuthKeyStackPanel.Children.Add(control);
        //    border.Child = pamAuthKeyStackPanel;

        //    pamAuthKeyPopup.Child = border;
        //    pamAuthKeyPopup.IsOpen = true;

        //    control.btnOK.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
        //        channel = txtChannel.Text;
        //        pubnub.GrantPresenceAccess<string>(channel, pamAuthKey, true, true, PubnubCallbackResult, PubnubDisplayErrorMessage);

        //        TextBlock textBlock = new TextBlock();
        //        textBlock.Text = string.Format("Grant request ...\n");
        //        messageStackPanel.Children.Add(textBlock);
        //        scrollViewerResult.UpdateLayout();
        //        scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };
        //    control.btnCancel.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };
        //}

        //private void btnAuditPresence_Click(object sender, RoutedEventArgs e)
        //{
        //    this.IsEnabled = false;
        //    Border border = new Border();
        //    border.BorderBrush = new SolidColorBrush(Colors.Black);
        //    border.BorderThickness = new Thickness(5.0);

        //    StackPanel pamAuthKeyStackPanel = new StackPanel();
        //    pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
        //    pamAuthKeyStackPanel.Width = 400;
        //    pamAuthKeyStackPanel.Height = 300;

        //    Popup pamAuthKeyPopup = new Popup();
        //    pamAuthKeyPopup.Height = 300;
        //    pamAuthKeyPopup.Width = 300;
        //    pamAuthKeyPopup.VerticalOffset = 10;
        //    pamAuthKeyPopup.HorizontalOffset = 10;
        //    PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
        //    control.txtPAMAuthKey.Text = "";
        //    pamAuthKeyStackPanel.Children.Add(control);
        //    border.Child = pamAuthKeyStackPanel;

        //    pamAuthKeyPopup.Child = border;
        //    pamAuthKeyPopup.IsOpen = true;

        //    control.btnOK.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
        //        channel = txtChannel.Text;
        //        pubnub.AuditPresenceAccess<string>(channel, pamAuthKey, PubnubCallbackResult, PubnubDisplayErrorMessage);

        //        TextBlock textBlock = new TextBlock();
        //        textBlock.Text = string.Format("Audit request...\n");
        //        messageStackPanel.Children.Add(textBlock);
        //        scrollViewerResult.UpdateLayout();
        //        scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };
        //    control.btnCancel.Click += (s, args) =>
        //    {
        //        pamAuthKeyPopup.IsOpen = false;
        //        pamAuthKeyPopup = null;
        //        this.IsEnabled = true;
        //    };
        //}

        private void btnPush_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pushNotificationStackPanel = new StackPanel();
            pushNotificationStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pushNotificationStackPanel.Width = 400;
            pushNotificationStackPanel.Height = 400;

            pushNotificationPopup = new Popup();
            pushNotificationPopup.Height = 300;
            pushNotificationPopup.Width = 300;
            pushNotificationPopup.VerticalOffset = 20;
            pushNotificationPopup.HorizontalOffset = 20;
            PushNotificationUserControl control = new PushNotificationUserControl();
            pushNotificationStackPanel.Children.Add(control);
            border.Child = pushNotificationStackPanel;

            pushNotificationPopup.Child = border;
            pushNotificationPopup.IsOpen = true;
            control.btnOK.Click += (s, args) =>
            {
                pushNotificationPopup.IsOpen = false;
                ResponseType type;
                //currentPushReqType
                if (control.radRegisterDevice.IsChecked.Value)
                {
                    type = ResponseType.PushRegister;
                    currentPushReqType = type;
                    pubnub.PushRemoteImageDomainUri.Add(new Uri("http://cdn.flaticon.com")); 
                    ProcessPushRequestType(type, channel);
                }
                else if (control.radRUnegisterDevice.IsChecked.Value)
                {
                    type = ResponseType.PushUnregister;
                    currentPushReqType = type;
                    ProcessPushRequestType(type, channel);
                }
                else if (control.radRemoveChannel.IsChecked.Value)
                {
                    type = ResponseType.PushRemove;
                    currentPushReqType = type;
                    ProcessPushRequestType(type, channel);
                }
                else if (control.radGetChannels.IsChecked.Value)
                {
                    type = ResponseType.PushGet;
                    currentPushReqType = type;
                    ProcessPushRequestType(type, channel);

                }

                pushNotificationPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                pushNotificationPopup.IsOpen = false;
                pushNotificationPopup = null;
                this.IsEnabled = true;
            };

        }

        void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {

            Dispatcher.BeginInvoke(() =>
            {
                //HttpNotificationChannel microsoftPushChannel;
                //microsoftPushChannel = HttpNotificationChannel.Find(microsoftChannelName);
                //if (!microsoftPushChannel.IsShellTileBound)
                //{
                //    //microsoftPushChannel.UnbindToShellTile();

                //    if (pubnub.PushRemoteImageDomainUri != null && pubnub.PushRemoteImageDomainUri.Count > 0)
                //    {
                //        microsoftPushChannel.BindToShellTile(pubnub.PushRemoteImageDomainUri);
                //    }
                //    else
                //    {
                //        microsoftPushChannel.BindToShellTile();
                //    }
                //}

                //if (!microsoftPushChannel.IsShellToastBound)
                //{
                //    microsoftPushChannel.BindToShellToast();

                //    if (pubnub.PushRemoteImageDomainUri != null && pubnub.PushRemoteImageDomainUri.Count > 0)
                //    {
                //        microsoftPushChannel.BindToShellTile(pubnub.PushRemoteImageDomainUri);
                //    }
                //    else
                //    {
                //        microsoftPushChannel.BindToShellTile();
                //    }

                //}

                string pubnubChannel = txtChannel.Text;
                string msg = "";
                // Display the new URI for testing purposes.   Normally, the URI would be passed back to your web service at this point.
                System.Diagnostics.Debug.WriteLine(e.ChannelUri.ToString());
                ResponseType type = currentPushReqType;
                switch (type)
                {
                    case ResponseType.PushRegister:
                        pubnub.RegisterDeviceForPush<string>(pubnubChannel, PushTypeService.MPNS, e.ChannelUri.ToString(), PubnubCallbackResult, PubnubDisplayErrorMessage);
                        msg = "Running Register Device";
                        break;
                    case ResponseType.PushUnregister:
                        pubnub.UnregisterDeviceForPush<string>(PushTypeService.MPNS, e.ChannelUri.ToString(), PubnubCallbackResult, PubnubDisplayErrorMessage);
                        msg = "Running Unregister Device";
                        break;
                    case ResponseType.PushRemove:
                        pubnub.RemoveChannelForDevicePush<string>(pubnubChannel, PushTypeService.MPNS, e.ChannelUri.ToString(), PubnubCallbackResult, PubnubDisplayErrorMessage);
                        msg = "Running remove channel from push";
                        break;
                    case ResponseType.PushGet:
                        pubnub.GetChannelsForDevicePush<string>(PushTypeService.MPNS, e.ChannelUri.ToString(), PubnubCallbackResult, PubnubDisplayErrorMessage);
                        msg = "Running get channels for push";
                        break;
                    default:
                        break;
                }

                TextBlock textBlock = new TextBlock();
                textBlock.Text = msg;
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                //MessageBox.Show(String.Format("Channel Uri is {0}", e.ChannelUri.ToString()));
            });
        }

        void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            // Error handling logic for your particular application would be here.
            Dispatcher.BeginInvoke(() =>
                MessageBox.Show(String.Format("A push notification {0} error occurred.  {1} ({2}) {3}",
                    e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData))
                    );
        }

        void PushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            StringBuilder message = new StringBuilder();
            string relativeUri = string.Empty;

            message.AppendFormat("Received Toast {0}:\n", DateTime.Now.ToShortTimeString());

            // Parse out the information that was part of the message.
            foreach (string key in e.Collection.Keys)
            {
                message.AppendFormat("{0}: {1}\n", key, e.Collection[key]);

                if (string.Compare(
                    key,
                    "wp:Param",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    relativeUri = e.Collection[key];
                }
            }

            // Display a dialog of all the fields in the toast.
            Dispatcher.BeginInvoke(() => MessageBox.Show(message.ToString()));

        }

        void ProcessPushRequestType(ResponseType type, string pubnubChannel)
        {
            HttpNotificationChannel microsoftPushChannel;

            microsoftPushChannel = HttpNotificationChannel.Find(microsoftChannelName);

            if (microsoftPushChannel == null)
            {
                if (!string.IsNullOrEmpty(pubnub.PushServiceName))
                {
                    microsoftPushChannel = new HttpNotificationChannel(microsoftChannelName, pubnub.PushServiceName);
                }
                else
                {
                    microsoftPushChannel = new HttpNotificationChannel(microsoftChannelName);
                }

                // Register for all the events before attempting to open the channel.
                microsoftPushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                microsoftPushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

                // Register for this notification only if you need to receive the notifications while your application is running.
                microsoftPushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

                microsoftPushChannel.Open();

                // Bind this new channel for toast events.
                microsoftPushChannel.BindToShellToast();

                if (pubnub.PushRemoteImageDomainUri != null && pubnub.PushRemoteImageDomainUri.Count > 0)
                {
                    microsoftPushChannel.BindToShellTile(pubnub.PushRemoteImageDomainUri);
                }
                else
                {
                    microsoftPushChannel.BindToShellTile();
                }


            }
            else
            {
                // The channel was already open, so just register for all the events.
                microsoftPushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                microsoftPushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

                // Register for this notification only if you need to receive the notifications while your application is running.
                microsoftPushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

                if (microsoftPushChannel.ChannelUri != null)
                {
                    // Display the URI for testing purposes. Normally, the URI would be passed back to your web service at this point.
                    System.Diagnostics.Debug.WriteLine(microsoftPushChannel.ChannelUri.ToString());
                    string msg = "";
                    switch (type)
                    {
                        case ResponseType.PushRegister:
                            pubnub.RegisterDeviceForPush<string>(pubnubChannel, PushTypeService.MPNS, microsoftPushChannel.ChannelUri.ToString(), PubnubCallbackResult, PubnubDisplayErrorMessage);
                            msg = "Running Register Device";
                            break;
                        case ResponseType.PushUnregister:
                            pubnub.UnregisterDeviceForPush<string>(PushTypeService.MPNS, microsoftPushChannel.ChannelUri.ToString(), PubnubCallbackResult, PubnubDisplayErrorMessage);
                            msg = "Running Unregister Device";
                            break;
                        case ResponseType.PushRemove:
                            pubnub.RemoveChannelForDevicePush<string>(pubnubChannel, PushTypeService.MPNS, microsoftPushChannel.ChannelUri.ToString(), PubnubCallbackResult, PubnubDisplayErrorMessage);
                            msg = "Running remove channel from push";
                            break;
                        case ResponseType.PushGet:
                            pubnub.GetChannelsForDevicePush<string>(PushTypeService.MPNS, microsoftPushChannel.ChannelUri.ToString(), PubnubCallbackResult, PubnubDisplayErrorMessage);
                            msg = "Running get channels for push";
                            break;
                        default:
                            break;
                    }
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = msg;
                    messageStackPanel.Children.Add(textBlock);
                    scrollViewerResult.UpdateLayout();
                    scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);
                    //MessageBox.Show(String.Format("Channel Uri is {0}", microsoftPushChannel.ChannelUri.ToString()));
                }

            }

        }
    }

    public class PubnubDemoObject
    {
        public double VersionID = 3.4;
        public string Timetoken = "13601488652764619";
        public string OperationName = "Publish";
        public string[] Channels = { "ch1" };
        public PubnubDemoMessage DemoMessage = new PubnubDemoMessage();
        public PubnubDemoMessage CustomMessage = new PubnubDemoMessage("Welcome to the world of Pubnub for Publish and Subscribe. Hah!");
        public Person[] SampleXml = new DemoRoot().Person.ToArray();
    }

    public class PubnubDemoMessage
    {
        public string DefaultMessage = "~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\ {}| asdfghjkl;' :\" zxcvbnm,./ <>? ";

        public PubnubDemoMessage()
        {
        }

        public PubnubDemoMessage(string message)
        {
            DefaultMessage = message;
        }

    }

    public class DemoRoot
    {
        public List<Person> Person
        {
            get
            {
                List<Person> ret = new List<Person>();
                Person p1 = new Person();
                p1.ID = "ABCD123";
                //PersonID id1 = new PersonID(); id1.ID = "ABCD123" ;
                //p1.ID = id1;
                Name n1 = new Name();
                n1.First = "John";
                n1.Middle = "P.";
                n1.Last = "Doe";
                p1.Name = n1;

                Address a1 = new Address();
                a1.Street = "123 Duck Street";
                a1.City = "New City";
                a1.State = "New York";
                a1.Country = "United States";
                p1.Address = a1;

                ret.Add(p1);

                Person p2 = new Person();
                p2.ID = "ABCD456";
                //PersonID id2 = new PersonID(); id2.ID = "ABCD123" ;
                //p2.ID = id2;
                Name n2 = new Name();
                n2.First = "Peter";
                n2.Middle = "Z.";
                n2.Last = "Smith";
                p2.Name = n2;

                Address a2 = new Address();
                a2.Street = "12 Hollow Street";
                a2.City = "Philadelphia";
                a2.State = "Pennsylvania";
                a2.Country = "United States";
                p2.Address = a2;

                ret.Add(p2);

                return ret;

            }
        }
    }

    public class Person
    {
        public string ID { get; set; }

        public Name Name;

        public Address Address;
    }

    public class Name
    {
        public string First { get; set; }
        public string Middle { get; set; }
        public string Last { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }


    public class UserCreated
    {
        public DateTime TimeStamp { get; set; }
        public User User { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Addressee Addressee { get; set; }
        public List<Phone> Phones { get; set; }
    }

    public class Addressee
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
    }

    public class Phone
    {
        public string Number { get; set; }
        public string Extenion { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PhoneType PhoneType { get; set; }
    }

    public enum PhoneType
    {
        Home,
        Mobile,
        Work
    }

}