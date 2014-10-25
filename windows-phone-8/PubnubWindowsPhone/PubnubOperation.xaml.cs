using System;
using System.Collections.Generic;
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
using PubNubMessaging.Core;
using System.Windows.Controls.Primitives;

using Microsoft.Phone.Notification;
using System.Text;

namespace PubnubWindowsPhone
{
    public partial class PubnubOperation : PhoneApplicationPage
    {
        Pubnub pubnub;
        string channel = "";
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
            pubnub.Time<string>(PubnubCallbackResult, PubnubDisplayErrorMessage);
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
                    pubnub.Publish<string>(channel, publishedMessage, storeInHistory, PubnubCallbackResult, PubnubDisplayErrorMessage);
                }
                else if (control.radToastPublish.IsChecked.Value)
                {
                    ToastNotification toast = new ToastNotification();
                    //toast.type = "toast";
                    toast.text1 = "hardcode message";
                    Dictionary<string, object> dicToast = new Dictionary<string, object>();
                    dicToast.Add("pn_mpns", toast);
                    
                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish<string>(channel, dicToast, PubnubCallbackResult, PubnubDisplayErrorMessage);
                }
                else if (control.radFlipTilePublish.IsChecked.Value)
                {
                    FlipTileNotification tile = new FlipTileNotification();
                    tile.title = "front title";
                    tile.count = 2;
                    tile.back_title = "back title";
                    tile.back_content = "back message";
                    tile.back_background_image = "Assets/Tiles/pubnub2.png";
                    tile.background_image = "Assets/Tiles/pubnub3.png";
                    Dictionary<string, object> dicTile = new Dictionary<string, object>();
                    dicTile.Add("pn_mpns", tile);
                    
                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish<string>(channel, dicTile, PubnubCallbackResult, PubnubDisplayErrorMessage);
                }
                else if (control.radCycleTilePublish.IsChecked.Value)
                {
                    CycleTileNotification tile = new CycleTileNotification();
                    tile.title = "front title";
                    tile.count = 2;
                    tile.images = new string[] { "Images/Blue.jpg", "Images/Green.jpg" };
                    
                    Dictionary<string, object> dicTile = new Dictionary<string, object>();
                    dicTile.Add("pn_mpns", tile);
                    
                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish<string>(channel, dicTile, PubnubCallbackResult, PubnubDisplayErrorMessage);
                }
                else if (control.radIconicTilePublish.IsChecked.Value)
                {
                    IconicTileNotification tile = new IconicTileNotification();
                    tile.title = "front title";
                    tile.count = 2;
                    
                    Dictionary<string, object> dicTile = new Dictionary<string, object>();
                    dicTile.Add("pn_mpns", tile);

                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish<string>(channel, dicTile, PubnubCallbackResult, PubnubDisplayErrorMessage);
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

                pubnub.HereNow<string>(channel, showUUID, includeState, PubnubCallbackResult, PubnubDisplayErrorMessage);

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
                pubnub.WhereNow<string>(whereNowUUID, PubnubCallbackResult, PubnubDisplayErrorMessage);
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

                pubnub.GlobalHereNow<string>(showUUID, includeState, PubnubCallbackResult, PubnubDisplayErrorMessage);

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
                        pubnub.SetUserState<string>(channel, new KeyValuePair<string, object>(userStateKey1, valueInt), PubnubCallbackResult, PubnubDisplayErrorMessage);
                    }
                    else if (Double.TryParse(userStateValue1, out valueDouble))
                    {
                        pubnub.SetUserState<string>(channel, new KeyValuePair<string, object>(userStateKey1, valueDouble), PubnubCallbackResult, PubnubDisplayErrorMessage);
                    }
                    else
                    {
                        pubnub.SetUserState<string>(channel, new KeyValuePair<string, object>(userStateKey1, userStateValue1), PubnubCallbackResult, PubnubDisplayErrorMessage);
                    }

                    msg = "Running Set User State";
                }
                else
                {
                    string userStateUUID = control.txtGetUserStateUUID.Text;
                    pubnub.GetUserState<string>(channel, userStateUUID, PubnubCallbackResult, PubnubDisplayErrorMessage);
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
            pubnub.Presence<string>(channel, PubnubCallbackResult, PubnubConnectCallbackResult, PubnubDisplayErrorMessage);
        }

        private void btnDetailedHistory_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            pubnub.DetailedHistory<string>(channel, 10, PubnubCallbackResult, PubnubDisplayErrorMessage);
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
            pubnub.Unsubscribe<string>(channel, PubnubCallbackResult, PubnubConnectCallbackResult, PubnubDisconnectCallbackResult, PubnubDisplayErrorMessage);
        }

        private void btnPresenceUnsub_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            pubnub.PresenceUnsubscribe<string>(channel, PubnubCallbackResult, PubnubConnectCallbackResult, PubnubDisconnectCallbackResult, PubnubDisplayErrorMessage);
        }

        private void btnSubscribe_Click(object sender, RoutedEventArgs e)
        {
            channel = txtChannel.Text;
            pubnub.Subscribe<string>(channel, PubnubCallbackResult, PubnubConnectCallbackResult, PubnubDisplayErrorMessage);
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

        private void btnGrant_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pamAuthKeyStackPanel = new StackPanel();
            pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pamAuthKeyStackPanel.Width = 400;
            pamAuthKeyStackPanel.Height = 300;

            Popup pamAuthKeyPopup = new Popup();
            pamAuthKeyPopup.Height = 300;
            pamAuthKeyPopup.Width = 300;
            pamAuthKeyPopup.VerticalOffset = 10;
            pamAuthKeyPopup.HorizontalOffset = 10;
            PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
            control.txtPAMAuthKey.Text = "";
            pamAuthKeyStackPanel.Children.Add(control);
            border.Child = pamAuthKeyStackPanel;

            pamAuthKeyPopup.Child = border;
            pamAuthKeyPopup.IsOpen = true;

            control.btnOK.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
                channel = txtChannel.Text;
                pubnub.GrantAccess<string>(channel, pamAuthKey, true, true, PubnubCallbackResult, PubnubDisplayErrorMessage);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = string.Format("Grant request ...\n");
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };

        }

        private void btnRevoke_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pamAuthKeyStackPanel = new StackPanel();
            pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pamAuthKeyStackPanel.Width = 400;
            pamAuthKeyStackPanel.Height = 300;

            Popup pamAuthKeyPopup = new Popup();
            pamAuthKeyPopup.Height = 300;
            pamAuthKeyPopup.Width = 300;
            pamAuthKeyPopup.VerticalOffset = 10;
            pamAuthKeyPopup.HorizontalOffset = 10;
            PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
            control.txtPAMAuthKey.Text = "";
            pamAuthKeyStackPanel.Children.Add(control);
            border.Child = pamAuthKeyStackPanel;

            pamAuthKeyPopup.Child = border;
            pamAuthKeyPopup.IsOpen = true;

            control.btnOK.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
                channel = txtChannel.Text;
                pubnub.GrantAccess<string>(channel, pamAuthKey, false, false, PubnubCallbackResult, PubnubDisplayErrorMessage);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = string.Format("Revoke Grant request ....\n");
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };
            
        }

        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pamAuthKeyStackPanel = new StackPanel();
            pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pamAuthKeyStackPanel.Width = 400;
            pamAuthKeyStackPanel.Height = 300;

            Popup pamAuthKeyPopup = new Popup();
            pamAuthKeyPopup.Height = 300;
            pamAuthKeyPopup.Width = 300;
            pamAuthKeyPopup.VerticalOffset = 10;
            pamAuthKeyPopup.HorizontalOffset = 10;
            PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
            control.txtPAMAuthKey.Text = "";
            pamAuthKeyStackPanel.Children.Add(control);
            border.Child = pamAuthKeyStackPanel;

            pamAuthKeyPopup.Child = border;
            pamAuthKeyPopup.IsOpen = true;

            control.btnOK.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
                channel = txtChannel.Text;
                pubnub.AuditAccess<string>(channel, pamAuthKey, PubnubCallbackResult, PubnubDisplayErrorMessage);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = string.Format("Audit request ...\n");
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnGrantPresence_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pamAuthKeyStackPanel = new StackPanel();
            pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pamAuthKeyStackPanel.Width = 400;
            pamAuthKeyStackPanel.Height = 300;

            Popup pamAuthKeyPopup = new Popup();
            pamAuthKeyPopup.Height = 300;
            pamAuthKeyPopup.Width = 300;
            pamAuthKeyPopup.VerticalOffset = 10;
            pamAuthKeyPopup.HorizontalOffset = 10;
            PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
            control.txtPAMAuthKey.Text = "";
            pamAuthKeyStackPanel.Children.Add(control);
            border.Child = pamAuthKeyStackPanel;

            pamAuthKeyPopup.Child = border;
            pamAuthKeyPopup.IsOpen = true;

            control.btnOK.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
                channel = txtChannel.Text;
                pubnub.GrantPresenceAccess<string>(channel, pamAuthKey, true, true, PubnubCallbackResult, PubnubDisplayErrorMessage);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = string.Format("Grant request ...\n");
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };
        }

        private void btnAuditPresence_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(5.0);

            StackPanel pamAuthKeyStackPanel = new StackPanel();
            pamAuthKeyStackPanel.Background = new SolidColorBrush(Colors.Blue);
            pamAuthKeyStackPanel.Width = 400;
            pamAuthKeyStackPanel.Height = 300;

            Popup pamAuthKeyPopup = new Popup();
            pamAuthKeyPopup.Height = 300;
            pamAuthKeyPopup.Width = 300;
            pamAuthKeyPopup.VerticalOffset = 10;
            pamAuthKeyPopup.HorizontalOffset = 10;
            PAMAuthKeyUserControl control = new PAMAuthKeyUserControl();
            control.txtPAMAuthKey.Text = "";
            pamAuthKeyStackPanel.Children.Add(control);
            border.Child = pamAuthKeyStackPanel;

            pamAuthKeyPopup.Child = border;
            pamAuthKeyPopup.IsOpen = true;

            control.btnOK.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                string pamAuthKey = control.txtPAMAuthKey.Text.Trim();
                channel = txtChannel.Text;
                pubnub.AuditPresenceAccess<string>(channel, pamAuthKey, PubnubCallbackResult, PubnubDisplayErrorMessage);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = string.Format("Audit request...\n");
                messageStackPanel.Children.Add(textBlock);
                scrollViewerResult.UpdateLayout();
                scrollViewerResult.ScrollToVerticalOffset(scrollViewerResult.ExtentHeight);

                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };
            control.btnCancel.Click += (s, args) =>
            {
                pamAuthKeyPopup.IsOpen = false;
                pamAuthKeyPopup = null;
                this.IsEnabled = true;
            };
        }

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
                HttpNotificationChannel microsoftPushChannel;
                microsoftPushChannel = HttpNotificationChannel.Find(microsoftChannelName);
                if (!microsoftPushChannel.IsShellTileBound)
                {
                    microsoftPushChannel.BindToShellTile();
                }

                if (!microsoftPushChannel.IsShellToastBound)
                {
                    microsoftPushChannel.BindToShellToast();
                }

                string pubnubChannel = txtChannel.Text;
                string msg = "";
                // Display the new URI for testing purposes.   Normally, the URI would be passed back to your web service at this point.
                System.Diagnostics.Debug.WriteLine(e.ChannelUri.ToString());
                ResponseType type = currentPushReqType;
                switch (type)
                {
                    case ResponseType.PushRegister:
                        pubnub.RegisterDeviceForPush<string>(pubnubChannel, PushTypeService.MPNS, e.ChannelUri, PubnubCallbackResult, PubnubDisplayErrorMessage);
                        msg = "Running Register Device";
                        break;
                    case ResponseType.PushUnregister:
                        pubnub.UnregisterDeviceForPush<string>(PushTypeService.MPNS, e.ChannelUri, PubnubCallbackResult, PubnubDisplayErrorMessage);
                        msg = "Running Unregister Device";
                        break;
                    case ResponseType.PushRemove:
                        pubnub.RemoveChannelForDevicePush<string>(pubnubChannel, PushTypeService.MPNS, e.ChannelUri, PubnubCallbackResult, PubnubDisplayErrorMessage);
                        msg = "Running remove channel from push";
                        break;
                    case ResponseType.PushGet:
                        pubnub.GetChannelsForDevicePush<string>(PushTypeService.MPNS, e.ChannelUri, PubnubCallbackResult, PubnubDisplayErrorMessage);
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
                microsoftPushChannel = new HttpNotificationChannel(microsoftChannelName);

                // Register for all the events before attempting to open the channel.
                microsoftPushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                microsoftPushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

                // Register for this notification only if you need to receive the notifications while your application is running.
                microsoftPushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

                microsoftPushChannel.Open();

                // Bind this new channel for toast events.
                microsoftPushChannel.BindToShellToast();
                microsoftPushChannel.BindToShellTile();

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
                            pubnub.RegisterDeviceForPush<string>(pubnubChannel, PushTypeService.MPNS, microsoftPushChannel.ChannelUri, PubnubCallbackResult, PubnubDisplayErrorMessage);
                            msg = "Running Register Device";
                            break;
                        case ResponseType.PushUnregister:
                            pubnub.UnregisterDeviceForPush<string>(PushTypeService.MPNS, microsoftPushChannel.ChannelUri, PubnubCallbackResult, PubnubDisplayErrorMessage);
                            msg = "Running Unregister Device";
                            break;
                        case ResponseType.PushRemove:
                            pubnub.RemoveChannelForDevicePush<string>(pubnubChannel, PushTypeService.MPNS, microsoftPushChannel.ChannelUri, PubnubCallbackResult, PubnubDisplayErrorMessage);
                            msg = "Running remove channel from push";
                            break;
                        case ResponseType.PushGet:
                            pubnub.GetChannelsForDevicePush<string>(PushTypeService.MPNS, microsoftPushChannel.ChannelUri, PubnubCallbackResult, PubnubDisplayErrorMessage);
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
}