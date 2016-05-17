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
    public partial class DemoStart : PhoneApplicationPage
    {
        public DemoStart()
        {
            InitializeComponent();
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            bool ssl = chkSSL.IsChecked.Value;
            string secretKey = txtSecret.Text;
            string cipherKey = txtCipher.Text;
            string sessionUUID = txtUUID.Text;
            bool resumeOnReconnect = chkResumeOnReconnect.IsChecked.Value;
            bool hideErrorCallbackMsg = chkHideErrCallbackMsg.IsChecked.Value;
            string publishKey = txtPubKey.Text;
            string subscribeKey = txtSubKey.Text;
            string origin = txtOrigin.Text;
            string authKey = txtAuthKey.Text;

            //Uri nextPage = new Uri(string.Format("/PubnubOperation.xaml?ssl={0}&cipherkey={1}&secretkey={2}&uuid={3}", ssl, cipherKey, secretKey,sessionUUID), UriKind.Relative);
            Uri nextPage = new Uri(string.Format("/PubnubTimeoutSettings.xaml?ssl={0}&origin={1}&publishKey={2}&subscribeKey={3}&cipherkey={4}&secretkey={5}&uuid={6}&resumeOnReconnect={7}&hideErrCallbackMsg={8}&authKey={9}", ssl, origin, publishKey, subscribeKey, cipherKey, secretKey, sessionUUID, resumeOnReconnect, hideErrorCallbackMsg, authKey), UriKind.Relative);
            NavigationService.Navigate(nextPage);
        }

    }
}