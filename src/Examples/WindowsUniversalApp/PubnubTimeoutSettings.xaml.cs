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
        PubnubConfigData data = null;

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
                Int32.TryParse(txtSubscribeTimeout.Text.Trim(), out data.subscribeTimeout);
                Int32.TryParse(txtNonSubscribeTimeout.Text.Trim(), out data.nonSubscribeTimeout);
                Int32.TryParse(txtNetworkMaxRetries.Text.Trim(), out data.maxRetries);
                Int32.TryParse(txtRetryInterval.Text.Trim(), out data.retryInterval);
                Int32.TryParse(txtLocalClientHeartbeatInterval.Text.Trim(), out data.localClientHeartbeatInterval);
                Int32.TryParse(txtPresenceHeartbeat.Text.Trim(), out data.presenceHeartbeat);
                Int32.TryParse(txtPresenceHeartbeatInterval.Text.Trim(), out data.presenceHeartbeatInterval);

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
