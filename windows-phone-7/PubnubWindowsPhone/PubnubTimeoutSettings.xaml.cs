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

namespace PubnubWindowsPhone
{
    public partial class PubnubTimeoutSettings : PhoneApplicationPage
    {
        bool ssl = false;
        string origin = "";
        string publishKey = "";
        string subscribeKey = "";
        string secretKey = "";
        string cipherKey = "";
        string sessionUUID = "";
        bool resumeOnReconnect = false;
        bool hideErrCallbackMsg = true;
        string authKey = "";
        
        int subscribeTimeoutInSeconds = 0;
        int operationTimeoutInSeconds = 0;
        int networkMaxRetries = 0;
        int networkRetryIntervalInSeconds = 0;
        int localClientHeartbeatIntervalInSeconds = 0;
        int presenceHeartbeat = 0;
        int presenceHeartbeatInterval = 0;
        
        public PubnubTimeoutSettings()
        {
            InitializeComponent();
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
            sessionUUID = NavigationContext.QueryString["uuid"].ToString();
            resumeOnReconnect = Boolean.Parse(NavigationContext.QueryString["resumeOnReconnect"].ToString());
            hideErrCallbackMsg = Boolean.Parse(NavigationContext.QueryString["hideErrCallbackMsg"].ToString());
            authKey = NavigationContext.QueryString["authKey"].ToString();
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            Int32.TryParse(txtSubscribeTimeout.Text, out subscribeTimeoutInSeconds);
            subscribeTimeoutInSeconds = (subscribeTimeoutInSeconds <= 0) ? 310 : subscribeTimeoutInSeconds;

            Int32.TryParse(txtNonSubscribeTimeout.Text, out operationTimeoutInSeconds);
            operationTimeoutInSeconds = (operationTimeoutInSeconds <= 0) ? 15 : operationTimeoutInSeconds;

            Int32.TryParse(txtNetworkMaxRetries.Text, out networkMaxRetries);
            networkMaxRetries = (networkMaxRetries <= 0) ? 50 : networkMaxRetries;

            Int32.TryParse(txtRetryInterval.Text, out networkRetryIntervalInSeconds);
            networkRetryIntervalInSeconds = (networkRetryIntervalInSeconds <= 0) ? 10 : networkRetryIntervalInSeconds;

            Int32.TryParse(txtLocalClientHeartbeatInterval.Text, out localClientHeartbeatIntervalInSeconds);
            localClientHeartbeatIntervalInSeconds = (localClientHeartbeatIntervalInSeconds <= 0) ? 10 : localClientHeartbeatIntervalInSeconds;

            Int32.TryParse(txtPresenceHeartbeat.Text, out presenceHeartbeat);
            presenceHeartbeat = (presenceHeartbeat <= 0 || presenceHeartbeat > 300) ? 60 : presenceHeartbeat;

            Int32.TryParse(txtPresenceHeartbeatInterval.Text, out presenceHeartbeatInterval);
            presenceHeartbeatInterval = (presenceHeartbeatInterval <= 0) ? 10 : localClientHeartbeatIntervalInSeconds;

            Uri nextPage = new Uri(string.Format("/PubnubOperation.xaml?ssl={0}&origin={1}&publishKey={2}&subscribeKey={3}&cipherkey={4}&secretkey={5}&uuid={6}&subtimeout={7}&optimeout={8}&retries={9}&retryinterval={10}&localbeatinterval={11}&resumeOnReconnect={12}&hideErrCallbackMsg={13}&authKey={14}&prebeat={15}&prebeatinterval={16}", ssl, origin, publishKey, subscribeKey, cipherKey, secretKey, sessionUUID, subscribeTimeoutInSeconds, operationTimeoutInSeconds, networkMaxRetries, networkRetryIntervalInSeconds, localClientHeartbeatIntervalInSeconds, resumeOnReconnect, hideErrCallbackMsg, authKey, presenceHeartbeat, presenceHeartbeatInterval), UriKind.Relative);
            NavigationService.Navigate(nextPage);
        }

    }
}