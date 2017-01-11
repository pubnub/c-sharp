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
    public sealed partial class PubnubDemoStart : Page
    {
        public PubnubDemoStart()
        {
            this.InitializeComponent();
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            PubnubConfigData data = new PubnubConfigData();
            data.ssl = chkSSL.IsChecked.Value;
            //data.resumeOnReconnect = chkReconnect.IsChecked.Value;
            data.publishKey = txtPublishKey.Text.Trim();
            data.subscribeKey = txtSubscribeKey.Text.Trim();
            data.cipherKey = txtCipherKey.Text.Trim();
            data.secretKey = txtSecretKey.Text.Trim();
            data.sessionUUID = txtSessionUUID.Text.Trim();
            data.origin = txtOrigin.Text.Trim();
            //data.hideErrorCallbackMessages = chkHideErrors.IsChecked.Value;

            var frame = new Frame();
            frame.Navigate(typeof(PubnubTimeoutSettings), data);
            Window.Current.Content = frame;
        }
    }
}
