using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsUniversalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PubnubTimeoutSettings : Page
    {
        PubnubConfigData data;

        public PubnubTimeoutSettings()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            data = e.Parameter as PubnubConfigData;
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            if (data != null)
            {
                int subscribeTimeout = 0;
                if (Int32.TryParse(txtSubscribeTimeout.Text.Trim(), out subscribeTimeout)) { data.subscribeTimeout = subscribeTimeout; }

                int nonSubscribeTimeout = 0;
                if (Int32.TryParse(txtNonSubscribeTimeout.Text.Trim(), out nonSubscribeTimeout)) { data.nonSubscribeTimeout = nonSubscribeTimeout; }

                int maxRetries = 0;
                if (Int32.TryParse(txtNetworkMaxRetries.Text.Trim(), out maxRetries)) { data.maxRetries = maxRetries; }

                int retryInterval = 0;
                if (Int32.TryParse(txtRetryInterval.Text.Trim(), out retryInterval)) { data.retryInterval = retryInterval; }

                int localClientHeartbeatInterval = 0;
                if (Int32.TryParse(txtLocalClientHeartbeatInterval.Text.Trim(), out localClientHeartbeatInterval)) { data.localClientHeartbeatInterval = localClientHeartbeatInterval;  }

                int presenceHeartbeat = 0;
                if (Int32.TryParse(txtPresenceHeartbeat.Text.Trim(), out presenceHeartbeat)) { data.presenceHeartbeat = presenceHeartbeat; }

                int presenceHeartbeatInterval = 0;
                if (Int32.TryParse(txtPresenceHeartbeatInterval.Text.Trim(), out presenceHeartbeatInterval)) { data.presenceHeartbeatInterval = presenceHeartbeatInterval; }

                var frame = new Frame();
                frame.Navigate(typeof(PubnubOperation), data);
                Window.Current.Content = frame;

            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Frame != null && Frame.CanGoBack)
            {
                Frame.Navigate(typeof(PubnubDemoStart));
            }
        }

        
    }
}
