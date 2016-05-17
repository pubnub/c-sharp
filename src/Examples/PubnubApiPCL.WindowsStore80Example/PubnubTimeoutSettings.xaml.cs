using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PubnubWindowsStore
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class PubnubTimeoutSettings : PubnubWindowsStore.Common.LayoutAwarePage
    {
        PubnubConfigData data = null;

        public PubnubTimeoutSettings()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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
    }
}
