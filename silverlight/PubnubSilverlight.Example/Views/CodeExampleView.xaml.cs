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
using System.Windows.Navigation;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
//using System.Threading;
//using Microsoft.Silverlight.Testing;
using PubNubMessaging.Core;

namespace PubnubSilverlight
{
    public partial class CodeExampleView : Page
    {

        #region "Properties and Members"

        static public Pubnub pubnub = null;

        static public string channel = "";
        static bool ssl = false;
        static string subscribeKey = "";
        static string publishKey = "";
        static string secretKey = "";
        static string cipherKey = "";
        static string authKey = "";
        static string uuid = "";
        static bool resumeOnReconnect = false;

        static int subscribeTimeoutInSeconds = 0;
        static int operationTimeoutInSeconds = 0;
        static int networkMaxRetries = 0;
        static int networkRetryIntervalInSeconds = 0;
        static int localClientHeartbeatIntervalInSeconds = 0;
        static int presenceHeartbeat = 0;
        static int presenceHeartbeatInterval = 0;

        static bool showErrorMessageSegments = true;
       
        #endregion

        public CodeExampleView()
        {
            InitializeComponent();

            Console.Container = ConsoleContainer;

        }

        private void CheckUserInputs()
        {
            ssl = chkSSL.IsChecked.Value;
            subscribeKey = txtSubKey.Text;
            publishKey = txtPubKey.Text;
            secretKey = txtSecret.Text;
            cipherKey = txtCipher.Text;
            authKey = txtAuthKey.Text;
            uuid = txtUUID.Text;
            resumeOnReconnect = chkResumeOnReconnect.IsChecked.Value;

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
            presenceHeartbeat = (presenceHeartbeat <= 0) ? 63 : presenceHeartbeat;
            
            Int32.TryParse(txtPresenceHeartbeatInterval.Text, out presenceHeartbeatInterval);
            presenceHeartbeatInterval = (presenceHeartbeatInterval <= 0) ? 60 : presenceHeartbeatInterval;

            if (pubnub == null)
            {
                pubnub = new Pubnub(publishKey, subscribeKey, secretKey, cipherKey, ssl);
                txtPubKey.IsEnabled = false;
                txtSubKey.IsEnabled = false;
                txtSecret.IsEnabled = false;
                txtCipher.IsEnabled = false;
                txtAuthKey.IsEnabled = false;
                txtUUID.IsEnabled = false;
                chkSSL.IsEnabled = false;
                chkResumeOnReconnect.IsEnabled = false;

                txtSubscribeTimeout.IsEnabled = false;
                txtNonSubscribeTimeout.IsEnabled = false;
                txtNetworkMaxRetries.IsEnabled = false;
                txtRetryInterval.IsEnabled = false;
                txtLocalClientHeartbeatInterval.IsEnabled = false;
                txtPresenceHeartbeat.IsEnabled = false;
                txtPresenceHeartbeatInterval.IsEnabled = false;

                btnReset.IsEnabled = true;
            }
            pubnub.SessionUUID = uuid;
            pubnub.SubscribeTimeout = subscribeTimeoutInSeconds;
            pubnub.NonSubscribeTimeout = operationTimeoutInSeconds;
            pubnub.NetworkCheckMaxRetries = networkMaxRetries;
            pubnub.NetworkCheckRetryInterval = networkRetryIntervalInSeconds;
            pubnub.LocalClientHeartbeatInterval = localClientHeartbeatIntervalInSeconds;
            pubnub.PresenceHeartbeat = presenceHeartbeat;
            pubnub.PresenceHeartbeatInterval = presenceHeartbeatInterval;
            pubnub.EnableResumeOnReconnect = resumeOnReconnect;
        }

        private void Subscribe_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running subscribe()");
            pubnub.Subscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;

            PublishMessageDialog publishView = new PublishMessageDialog();

            publishView.Show();

            publishView.Closed += (obj, args) => 
            {
                if (publishView.DialogResult == true && publishView.Message.Text.Length > 0)
                {
                    Console.WriteLine("Running publish()");

                    string publishedMessage = publishView.Message.Text;
                    if (authKey.Trim() != "")
                    {
                        pubnub.AuthenticationKey = authKey;
                    }
                    pubnub.Publish<string>(channel, publishedMessage, DisplayUserCallbackMessage, DisplayErrorMessage);
                }
            };
        }

        private void Presence_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running presence()");
            pubnub.Presence<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running detailed history()");
            pubnub.DetailedHistory<string>(channel, 10, DisplayUserCallbackMessage, DisplayErrorMessage);
        }

        private void HereNow_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            uuid = txtUUID.Text;
            
            if (uuid.Trim() == "")
            {
                MessageBox.Show("UUID is required");
                return;
            }

            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }

            HereNowOptionsDialog hereNowPopup = new HereNowOptionsDialog();

            hereNowPopup.Show();

            hereNowPopup.Closed += (obj, args) =>
            {
                if (hereNowPopup.DialogResult == true)
                {
                    Console.WriteLine("Running Here_Now()");

                    bool hereNowShowUUIDList = hereNowPopup.chkHereNowShowUUIDList.IsChecked.Value;
                    bool hereNowIncludeState = hereNowPopup.chkHereNowIncludeState.IsChecked.Value;

                    pubnub.HereNow<string>(channel, hereNowShowUUIDList, hereNowIncludeState, DisplayUserCallbackMessage, DisplayErrorMessage);
                }
            };

        }

        private void Unsubscribe_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running unsubscribe()");
            pubnub.Unsubscribe<string>(channel, DisplayUserCallbackMessage, DisplayUserCallbackMessage, DisplayDisconnectCallbackMessage, DisplayErrorMessage);
        }

        private void PresenceUnsubscrib_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running presence-unsubscribe()");
            pubnub.PresenceUnsubscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayDisconnectCallbackMessage, DisplayErrorMessage);
        }

        private void Time_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running time()");
            pubnub.Time<string>(DisplayUserCallbackMessage, DisplayErrorMessage);
        }

        private void btnGrant_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            if (secretKey.Trim() == "")
            {
                MessageBox.Show("Secret Key is required for Grant");
                return;
            }
            channel = txtChannel.Text;
            bool read = true;
            bool write = true;
            int grantTimeLimitMinutes = 60;
            string authKeyPAM = txtAuthKeyPAM.Text.Trim();

            Console.WriteLine("Running Grant()");
            pubnub.GrantAccess<string>(channel, authKeyPAM, read, write, grantTimeLimitMinutes, DisplayGrantCallbackMessage, DisplayErrorMessage);
        }

        private void btnRevoke_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            if (secretKey.Trim() == "")
            {
                MessageBox.Show("Secret Key is required for Revoke");
                return;
            }
            channel = txtChannel.Text;
            bool read = false;
            bool write = false;
            string authKeyPAM = txtAuthKeyPAM.Text.Trim();
            
            Console.WriteLine("Running Revoke()");
            pubnub.GrantAccess<string>(channel, authKeyPAM, read, write, DisplayRevokeCallbackMessage, DisplayErrorMessage);
        }

        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            if (secretKey.Trim() == "")
            {
                MessageBox.Show("Secret Key is required for Audit");
                return;
            }
            channel = txtChannel.Text;
            string authKeyPAM = txtAuthKeyPAM.Text.Trim();
            
            Console.WriteLine("Running Audit()");
            pubnub.AuditAccess<string>(channel, authKeyPAM, DisplayAuditCallbackMessage, DisplayErrorMessage);
        }

        private void btnWhereNow_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            uuid = txtUUID.Text;
            if (uuid.Trim() == "")
            {
                MessageBox.Show("UUID is required");
                return;
            }

            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            
            WhereNowDialog whereNowPopup = new WhereNowDialog();
            whereNowPopup.txtWhereNowUUID.Text = uuid;

            whereNowPopup.Show();

            whereNowPopup.Closed += (obj, args) =>
            {
                if (whereNowPopup.DialogResult == true && whereNowPopup.txtWhereNowUUID.Text.Length > 0)
                {
                    Console.WriteLine("Running WhereNow()");

                    string whereNowUUID = whereNowPopup.txtWhereNowUUID.Text;
                    pubnub.WhereNow<string>(whereNowUUID, DisplayUserCallbackMessage, DisplayErrorMessage);
                }
            };
        }

        private void btnGlobalHereNow_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();

            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }

            GlobalHereNowDialog globalHereNowPopup = new GlobalHereNowDialog();

            globalHereNowPopup.Show();

            globalHereNowPopup.Closed += (obj, args) =>
            {
                if (globalHereNowPopup.DialogResult == true)
                {
                    Console.WriteLine("Running Global_Here_Now()");

                    bool globalHereNowShowUUIDList = globalHereNowPopup.chkHereNowShowUUIDList.IsChecked.Value;
                    bool globalHereNowIncludeState = globalHereNowPopup.chkHereNowIncludeState.IsChecked.Value;

                    pubnub.GlobalHereNow<string>(globalHereNowShowUUIDList, globalHereNowIncludeState, DisplayUserCallbackMessage, DisplayErrorMessage);
                }
            };
        }

        private void btnUserState_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;

            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }

            UserStateDialog userStatePopup = new UserStateDialog();
            userStatePopup.txtGetUserStateUUID.Text = uuid;
            userStatePopup.Show();

            userStatePopup.Closed += (obj, args) =>
            {
                if (userStatePopup.DialogResult == true)
                {
                    if (userStatePopup.radGetUserState.IsChecked.Value)
                    {
                        string getUserStateUUID = userStatePopup.txtGetUserStateUUID.Text.Trim();
                        Console.WriteLine("Running GetUserState()");
                        pubnub.GetUserState<string>(channel, getUserStateUUID, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (userStatePopup.radSetUserState.IsChecked.Value)
                    {
                        Console.WriteLine("Running SetUserState()");
                        string userStateKey1 = userStatePopup.txtKey1.Text;
                        string userStateValue1 = userStatePopup.txtValue1.Text;

                        int valueInt;
                        double valueDouble;
                        if (Int32.TryParse(userStateValue1, out valueInt))
                        {
                            pubnub.SetUserState<string>(channel, new KeyValuePair<string, object>(userStateKey1, valueInt), DisplayUserCallbackMessage, DisplayErrorMessage);
                        }
                        else if (Double.TryParse(userStateValue1, out valueDouble))
                        {
                            pubnub.SetUserState<string>(channel, new KeyValuePair<string, object>(userStateKey1, valueDouble), DisplayUserCallbackMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.SetUserState<string>(channel, new KeyValuePair<string, object>(userStateKey1, userStateValue1), DisplayUserCallbackMessage, DisplayErrorMessage);
                        }
                    }
                }
            };
        }

        private void btnChangeUUID_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            uuid = txtUUID.Text;
            if (uuid.Trim() == "")
            {
                MessageBox.Show("UUID is required");
                return;
            }

            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }

            ChangeUUIDDialog changeUUIDPopup = new ChangeUUIDDialog();
            changeUUIDPopup.txtChangeUUID.Text = uuid;

            changeUUIDPopup.Show();

            changeUUIDPopup.Closed += (obj, args) =>
            {
                if (changeUUIDPopup.DialogResult == true && changeUUIDPopup.txtChangeUUID.Text.Trim().Length > 0)
                {
                    Console.WriteLine("Running ChangeUUID()");

                    string newUUID = changeUUIDPopup.txtChangeUUID.Text;
                    pubnub.ChangeUUID(newUUID);
                    txtUUID.Text = uuid = newUUID;
                }
            };
        }

        static void DisplayUserCallbackMessage(string result)
        {
            Console.WriteLine("REGULAR CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine("");
        }

        static void DisplayConnectCallbackMessage(string result)
        {
            Console.WriteLine("CONNECT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine("");
        }

        static void DisplayDisconnectCallbackMessage(string result)
        {
            Console.WriteLine("DISCONNECT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine("");
        }

        static void DisplayGrantCallbackMessage(string result)
        {
            Console.WriteLine("GRANT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine("");
        }

        static void DisplayRevokeCallbackMessage(string result)
        {
            Console.WriteLine("REVOKE CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine("");
        }

        static void DisplayAuditCallbackMessage(string result)
        {
            Console.WriteLine("AUDIT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine("");
        }

        static void DisplayErrorMessage(PubnubClientError result)
        {
            Console.WriteLine("ERROR CALLBACK:");
            Console.WriteLine(result.Description);
            Console.WriteLine("");
            switch (result.StatusCode)
            {
                case 103:
                    //Warning: Verify origin host name and internet connectivity
                    break;
                case 104:
                    //Critical: Verify your cipher key
                    break;
                case 106:
                    //Warning: Check network/internet connection
                    break;
                case 108:
                    //Warning: Check network/internet connection
                    break;
                case 109:
                    //Warning: No network/internet connection. Please check network/internet connection
                    break;
                case 110:
                    //Informational: Network/internet connection is back. Active subscriber/presence channels will be restored.
                    break;
                case 111:
                    //Informational: Duplicate channel subscription is not allowed. Internally Pubnub API removes the duplicates before processing.
                    break;
                case 112:
                    //Informational: Channel Already Subscribed/Presence Subscribed. Duplicate channel subscription not allowed
                    break;
                case 113:
                    //Informational: Channel Already Presence-Subscribed. Duplicate channel presence-subscription not allowed
                    break;
                case 114:
                    //Warning: Please verify your cipher key
                    break;
                case 115:
                    //Warning: Protocol Error. Please contact PubNub with error details.
                    break;
                case 116:
                    //Warning: ServerProtocolViolation. Please contact PubNub with error details.
                    break;
                case 117:
                    //Informational: Input contains invalid channel name
                    break;
                case 118:
                    //Informational: Channel not subscribed yet
                    break;
                case 119:
                    //Informational: Channel not subscribed for presence yet
                    break;
                case 120:
                    //Informational: Incomplete unsubscribe. Try again for unsubscribe.
                    break;
                case 121:
                    //Informational: Incomplete presence-unsubscribe. Try again for presence-unsubscribe.
                    break;
                case 122:
                    //Informational: Network/Internet connection not available. C# client retrying again to verify connection. No action is needed from your side.
                    break;
                case 123:
                    //Informational: During non-availability of network/internet, max retries for connection were attempted. So unsubscribed the channel.
                    break;
                case 124:
                    //Informational: During non-availability of network/internet, max retries for connection were attempted. So presence-unsubscribed the channel.
                    break;
                case 125:
                    //Informational: Publish operation timeout occured.
                    break;
                case 126:
                    //Informational: HereNow operation timeout occured
                    break;
                case 127:
                    //Informational: Detailed History operation timeout occured
                    break;
                case 128:
                    //Informational: Time operation timeout occured
                    break;
                case 4000:
                    //Warning: Message too large. Your message was not sent. Try to send this again smaller sized
                    break;
                case 4001:
                    //Warning: Bad Request. Please check the entered inputs or web request URL
                    break;
                case 4002:
                    //Warning: Invalid Key. Please verify the publish key
                    break;
                case 4010:
                    //Critical: Please provide correct subscribe key. This corresponds to a 401 on the server due to a bad sub key
                    break;
                case 4020:
                    // PAM is not enabled. Please contact PubNub support
                    break;
                case 4030:
                    //Warning: Not authorized. Check the permimissions on the channel. Also verify authentication key, to check access.
                    break;
                case 4031:
                    //Warning: Incorrect public key or secret key.
                    break;
                case 4140:
                    //Warning: Length of the URL is too long. Reduce the length by reducing subscription/presence channels or grant/revoke/audit channels/auth key list
                    break;
                case 5000:
                    //Critical: Internal Server Error. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                    break;
                case 5020:
                    //Critical: Bad Gateway. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
                    break;
                case 5040:
                    //Critical: Gateway Timeout. No response from server due to PubNub server timeout. Please try again. If same problem persists, please contact PubNub support
                    break;
                case 0:
                    //Undocumented error. Please contact PubNub support with full error object details for further investigation
                    break;
                default:
                    break;
            }
            if (showErrorMessageSegments)
            {
                DisplayErrorMessageSegments(result);
                Console.WriteLine("");
            }
        }

        static void DisplayErrorMessageSegments(PubnubClientError pubnubError)
        {
            // These are all the attributes you may be interested in logging, switchiing on etc:

            Console.WriteLine(string.Format("<STATUS CODE>: {0}", pubnubError.StatusCode)); // Unique ID of Error

            Console.WriteLine(string.Format("<MESSAGE>: {0}", pubnubError.Message)); // Message received from server/clent or from .NET exception

            Console.WriteLine(string.Format("<SEVERITY>: {0}", pubnubError.Severity)); // Info can be ignored, Warning and Error should be handled

            if (pubnubError.DetailedDotNetException != null)
            {
                Console.WriteLine(pubnubError.IsDotNetException.ToString()); // Boolean flag to check .NET exception
                Console.WriteLine(string.Format("<DETAILED DOT.NET EXCEPTION>: {0}", pubnubError.DetailedDotNetException.ToString())); // Full Details of .NET exception
            }
            Console.WriteLine(string.Format("<MESSAGE SOURCE>: {0}", pubnubError.MessageSource)); // Did this originate from Server or Client-side logic
            if (pubnubError.PubnubWebRequest != null)
            {
                //Captured Web Request details
                Console.WriteLine(string.Format("<HTTP WEB REQUEST>: {0}", pubnubError.PubnubWebRequest.RequestUri.ToString()));
                Console.WriteLine(string.Format("<HTTP WEB REQUEST - HEADERS>: {0}", pubnubError.PubnubWebRequest.Headers.ToString()));
            }
            if (pubnubError.PubnubWebResponse != null)
            {
                //Captured Web Response details
                Console.WriteLine(string.Format("<HTTP WEB RESPONSE - HEADERS>: {0}", pubnubError.PubnubWebResponse.Headers.ToString()));
            }
            Console.WriteLine(string.Format("<DESCRIPTION>: {0}", pubnubError.Description)); // Useful for logging and troubleshooting and support
            Console.WriteLine(string.Format("<CHANNEL>: {0}", pubnubError.Channel)); //Channel name(s) at the time of error
            Console.WriteLine(string.Format("<DATETIME>: {0}", pubnubError.ErrorDateTimeGMT)); //GMT time of error

        }
        private void btnDisconnectRetry_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Running Disconnect/auto-Reconnect Subscriber Request Connection");
            pubnub.TerminateCurrentSubscriberRequest();
        }

        private void btnDisableNetwork_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("");
            Console.WriteLine("Disabling Network Connection (no internet)");
            Console.WriteLine("Initiating Simulation of Internet non-availability");
            Console.WriteLine("Until \"Enable Network\" is selected, no operations will occur");
            Console.WriteLine("NOTE: Publish from other pubnub clients can occur and those will be ");
            Console.WriteLine("      captured upon \"Enable Network\" is selected, provided resume on reconnect is enabled.");
            

            pubnub.EnableSimulateNetworkFailForTestingOnly();
        }

        private void btnEnableNetwork_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("");
            Console.WriteLine("Enabling Network Connection (yes internet)");
            pubnub.DisableSimulateNetworkFailForTestingOnly();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (pubnub != null)
            {
                pubnub.EndPendingRequests();
                pubnub = null;
            }
            txtSubKey.IsEnabled = true;
            txtPubKey.IsEnabled = true;
            txtSecret.IsEnabled = true;
            txtCipher.IsEnabled = true;
            txtAuthKey.IsEnabled = true;
            txtUUID.IsEnabled = true;
            chkSSL.IsEnabled = true;
            chkResumeOnReconnect.IsEnabled = true;

            txtSubscribeTimeout.IsEnabled = true;
            txtNonSubscribeTimeout.IsEnabled = true;
            txtNetworkMaxRetries.IsEnabled = true;
            txtRetryInterval.IsEnabled = true;
            txtLocalClientHeartbeatInterval.IsEnabled = true;
            txtPresenceHeartbeat.IsEnabled = true;
            txtPresenceHeartbeatInterval.IsEnabled = true;

            btnReset.IsEnabled = false;
        }

    }

    #region "Console View"

    public class Console
    {
        internal static TextBlock Container { get; set; }

        public static void WriteLine(string format)
        {
            Container.Dispatcher.BeginInvoke(() =>
            {
                if (Container != null)
                {
                    if (Container.Text == null)
                    {
                        Container.Text = "";
                    }
                    Container.Text += format + "\r\n";
                }
            });
        }

        public static void Clear()
        {
            if (Container != null)
            {
                Container.Text = string.Empty;
            }
        }

    }

    #endregion

}
