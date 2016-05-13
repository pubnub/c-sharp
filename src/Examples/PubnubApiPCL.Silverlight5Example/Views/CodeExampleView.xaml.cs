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
using PubnubApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PubnubSilverlight
{
    public partial class CodeExampleView : Page
    {

        #region "Properties and Members"

        static public Pubnub pubnub = null;

        static public string channel = "";
        static public string channelGroup = "";
        static bool ssl = false;
        static string subscribeKey = "";
        static string publishKey = "";
        static string secretKey = "";
        static string cipherKey = "";
        static string authKey = "";
        static string uuid = "";
        static string origin = "";
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
            origin = txtOrigin.Text;

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

            //PNConfiguration config = new PNConfiguration();
            //config.Origin = origin;
            //config.PublishKey = publishKey;
            //config.SubscribeKey = subscribeKey;
            //config.SecretKey = secretKey;
            //config.CiperKey = cipherKey;
            //config.Secure = ssl;
            //config.Uuid = uuid;
            //config.SubscribeTimeout = subscribeTimeoutInSeconds;
            //config.NonSubscribeRequestTimeout = operationTimeoutInSeconds;
            //config.SetPresenceHeartbeatTimeout(presenceHeartbeat);
            //config.SetPresenceHeartbeatTimeoutWithCustomInterval(presenceHeartbeat, presenceHeartbeatInterval);
            //config.AuthKey = authKey;

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
                txtOrigin.IsEnabled = false;

                txtSubscribeTimeout.IsEnabled = false;
                txtNonSubscribeTimeout.IsEnabled = false;
                txtNetworkMaxRetries.IsEnabled = false;
                txtRetryInterval.IsEnabled = false;
                txtLocalClientHeartbeatInterval.IsEnabled = false;
                txtPresenceHeartbeat.IsEnabled = false;
                txtPresenceHeartbeatInterval.IsEnabled = false;

                btnReset.IsEnabled = true;
            }
            pubnub.NetworkCheckMaxRetries = networkMaxRetries;
            pubnub.NetworkCheckRetryInterval = networkRetryIntervalInSeconds;
            pubnub.LocalClientHeartbeatInterval = localClientHeartbeatIntervalInSeconds;
            pubnub.EnableResumeOnReconnect = resumeOnReconnect;
        }

        private void Subscribe_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text.Trim();
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running subscribe()");
            if (channelGroup != "")
            {
                pubnub.Subscribe<string>(channel, channelGroup, DisplaySubscribeReturnMessage, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, DisplayErrorMessage);
            }
            else
            {
                pubnub.Subscribe<string>(channel, DisplaySubscribeReturnMessage, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, DisplayErrorMessage);
            }
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;

            PublishMessageDialog publishView = new PublishMessageDialog();

            publishView.Show();

            publishView.Closed += (obj, args) => 
            {
                if (publishView.radNormalPublish.IsChecked.Value)
                {
                    if (publishView.DialogResult == true && publishView.Message.Text.Length > 0)
                    {
                        Console.WriteLine("Running publish()");

                        string publishedMessage = publishView.Message.Text;
                        bool storeInHistory = publishView.chkStoreInHistory.IsChecked.Value;
                        if (authKey.Trim() != "")
                        {
                            pubnub.AuthenticationKey = authKey;
                        }
                        pubnub.Publish(channel, publishedMessage, storeInHistory, "", DisplayPublishReturnMessage, DisplayErrorMessage);
                    }
                }
                else if (publishView.radToastPublish.IsChecked.Value)
                {
                    MpnsToastNotification toast = new MpnsToastNotification();
                    //toast.type = "toast";
                    toast.text1 = "hardcode message";
                    Dictionary<string, object> dicToast = new Dictionary<string, object>();
                    dicToast.Add("pn_mpns", toast);

                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish(channel, dicToast, DisplayPublishReturnMessage, DisplayErrorMessage);
                }
                else if (publishView.radFlipTilePublish.IsChecked.Value)
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
                    pubnub.Publish(channel, dicTile, DisplayPublishReturnMessage, DisplayErrorMessage);
                }
                else if (publishView.radCycleTilePublish.IsChecked.Value)
                {
                    MpnsCycleTileNotification tile = new MpnsCycleTileNotification();
                    tile.title = "front title";
                    tile.count = 2;
                    tile.images = new string[] { "Assets/Tiles/pubnub1.png", "Assets/Tiles/pubnub2.png", "Assets/Tiles/pubnub3.png", "Assets/Tiles/pubnub4.png" };

                    Dictionary<string, object> dicTile = new Dictionary<string, object>();
                    dicTile.Add("pn_mpns", tile);

                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish(channel, dicTile, DisplayPublishReturnMessage, DisplayErrorMessage);
                }
                else if (publishView.radIconicTilePublish.IsChecked.Value)
                {
                    MpnsIconicTileNotification tile = new MpnsIconicTileNotification();
                    tile.title = "front title";
                    tile.count = 2;
                    tile.wide_content_1 = "my wide content";

                    Dictionary<string, object> dicTile = new Dictionary<string, object>();
                    dicTile.Add("pn_mpns", tile);

                    pubnub.EnableDebugForPushPublish = true;
                    pubnub.Publish(channel, dicTile, DisplayPublishReturnMessage, DisplayErrorMessage);
                }

            };
        }

        private void Presence_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text.Trim();
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running presence()");
            if (channelGroup != "")
            {
                pubnub.Presence(channel, channelGroup, DisplayPresenceAckMessage, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, DisplayErrorMessage);
            }
            else
            {
                pubnub.Presence(channel, DisplayPresenceAckMessage, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, DisplayErrorMessage);
            }
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
            pubnub.DetailedHistory(channel, 10, false, DisplayDetailedHistoryReturnMessage, DisplayErrorMessage);
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

                    pubnub.HereNow(channel.Split(','), hereNowShowUUIDList, hereNowIncludeState, DisplayHereNowReturnMessage, DisplayErrorMessage);
                }
            };

        }

        private void Unsubscribe_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text.Trim();
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running unsubscribe()");
            if (channelGroup != "")
            {
                pubnub.Unsubscribe<string>(channel, channelGroup, DisplayErrorMessage);
            }
            else
            {
                pubnub.Unsubscribe<string>(channel, DisplayErrorMessage);
            }
        }

        private void PresenceUnsubscrib_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text.Trim();
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running presence-unsubscribe()");
            if (channelGroup != "")
            {
                pubnub.PresenceUnsubscribe(channel, channelGroup, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, DisplayErrorMessage);
            }
            else
            {
                pubnub.PresenceUnsubscribe(channel, DisplayConnectDisconnectStatusMessage, DisplayConnectDisconnectStatusMessage, DisplayErrorMessage);
            }
        }

        private void Time_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }
            Console.WriteLine("Running time()");
            pubnub.Time(DisplayTimeReturnMessage, DisplayErrorMessage);
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
            pubnub.GrantAccess(channel, authKeyPAM, read, write, grantTimeLimitMinutes, DisplayGrantReturnMessage, DisplayErrorMessage);
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
            pubnub.GrantAccess(channel, authKeyPAM, read, write, DisplayGrantReturnMessage, DisplayErrorMessage);
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
            pubnub.AuditAccess(channel, authKeyPAM, DisplayAuditReturnMessage, DisplayErrorMessage);
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
                    pubnub.WhereNow(whereNowUUID, DisplayWhereNowReturnMessage, DisplayErrorMessage);
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

                    pubnub.GlobalHereNow(globalHereNowShowUUIDList, globalHereNowIncludeState, DisplayGlobalHereNowReturnMessage, DisplayErrorMessage);
                }
            };
        }

        private void btnUserState_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text.Trim();

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
                        pubnub.GetUserState(channel, getUserStateUUID, DisplayGetUserStateReturnMessage, DisplayErrorMessage);
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
                            pubnub.SetUserState(channel, new KeyValuePair<string, object>(userStateKey1, valueInt), DisplaySetUserStateReturnMessage, DisplayErrorMessage);
                        }
                        else if (Double.TryParse(userStateValue1, out valueDouble))
                        {
                            pubnub.SetUserState(channel, new KeyValuePair<string, object>(userStateKey1, valueDouble), DisplaySetUserStateReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            pubnub.SetUserState(channel, new KeyValuePair<string, object>(userStateKey1, userStateValue1), DisplaySetUserStateReturnMessage, DisplayErrorMessage);
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

        private void btnChannelGroup_Click(object sender, RoutedEventArgs e)
        {
            CheckUserInputs();
            channelGroup = txtChannelGroup.Text.Trim();

            if (authKey.Trim() != "")
            {
                pubnub.AuthenticationKey = authKey;
            }

            ChannelGroupDialog channelGroupPopup = new ChannelGroupDialog();
            channelGroupPopup.txtChannelGroupName.Text = channelGroup;
            channelGroupPopup.Show();

            channelGroupPopup.Closed += (obj, args) =>
            {
                if (channelGroupPopup.DialogResult == true)
                {
                    string userChannelGroupName = channelGroupPopup.txtChannelGroupName.Text.Trim();
                    if (channelGroupPopup.radChannelGroupGet.IsChecked.Value)
                    {
                        Console.WriteLine("Running GetChannelsForChannelGroup()");
                        pubnub.GetChannelsForChannelGroup(userChannelGroupName, DisplayGetChannelsForChannelGroupReturnMessage, DisplayErrorMessage);
                    }
                    else if (channelGroupPopup.radChannelGroupAdd.IsChecked.Value)
                    {
                        Console.WriteLine("Running AddChannelsToChannelGroup()");
                        string userChannelGroupAddChannel = channelGroupPopup.txtChannelGroupAddChannels.Text;
                        pubnub.AddChannelsToChannelGroup(new string[] { userChannelGroupAddChannel }, userChannelGroupName, DisplayAddChannelToChannelGroupReturnMessage, DisplayErrorMessage);
                    }
                    else if (channelGroupPopup.radChannelGroupRemove.IsChecked.Value)
                    {
                        Console.WriteLine("Running RemoveChannelsFromChannelGroup()");
                        string userChannelGroupRemoveChannel = channelGroupPopup.txtChannelGroupRemoveChannels.Text;
                        pubnub.RemoveChannelsFromChannelGroup(new string[] { userChannelGroupRemoveChannel }, userChannelGroupName, DisplayRemoveChannelFromChannelGroupReturnMessage, DisplayErrorMessage);
                    }
                    else if (channelGroupPopup.radChannelGroupGrant.IsChecked.Value)
                    {
                        Console.WriteLine("Running ChannelGroupGrantAccess()");
                        pubnub.ChannelGroupGrantAccess(userChannelGroupName, true, true, DisplayGrantReturnMessage, DisplayErrorMessage);
                    }
                    else if (channelGroupPopup.radChannelGroupAudit.IsChecked.Value)
                    {
                        Console.WriteLine("Running ChannelGroupAuditAccess()");
                        pubnub.ChannelGroupAuditAccess(userChannelGroupName, DisplayAuditReturnMessage, DisplayErrorMessage);
                    }
                    else if (channelGroupPopup.radChannelGroupRevoke.IsChecked.Value)
                    {
                        Console.WriteLine("Running ChannelGroupRevokeAccess()");
                        pubnub.ChannelGroupGrantAccess(userChannelGroupName, false, false, DisplayGrantReturnMessage, DisplayErrorMessage);
                    }

                    else if (channelGroupPopup.radPresenceChannelGroupGrant.IsChecked.Value)
                    {
                        Console.WriteLine("Running ChannelGroupGrantPresenceAccess()");
                        pubnub.ChannelGroupGrantPresenceAccess(userChannelGroupName, true, true, DisplayGrantReturnMessage, DisplayErrorMessage);
                    }
                    else if (channelGroupPopup.radPresenceChannelGroupAudit.IsChecked.Value)
                    {
                        Console.WriteLine("Running ChannelGroupAuditPresenceAccess()");
                        pubnub.ChannelGroupAuditPresenceAccess(userChannelGroupName, DisplayAuditReturnMessage, DisplayErrorMessage);
                    }
                    else if (channelGroupPopup.radPresenceChannelGroupRevoke.IsChecked.Value)
                    {
                        Console.WriteLine("Running ChannelGroup Revoke PresenceAccess()");
                        pubnub.ChannelGroupGrantPresenceAccess(userChannelGroupName, false, false, DisplayGrantReturnMessage, DisplayErrorMessage);
                    }
                
                }
            };

        }

        static void DisplayRemoveChannelGroupNamespaceReturnMessage(RemoveNamespaceAck result)
        {
            Console.WriteLine("RemoveNamespaceAck CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayRemoveChannelGroupReturnMessage(RemoveChannelGroupAck result)
        {
            Console.WriteLine("RemoveChannelFromChannelGroupAck CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayRemoveChannelFromChannelGroupReturnMessage(RemoveChannelFromChannelGroupAck result)
        {
            Console.WriteLine("RemoveChannelFromChannelGroupAck CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayAddChannelToChannelGroupReturnMessage(AddChannelToChannelGroupAck result)
        {
            Console.WriteLine("AddChannelToChannelGroupAck CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayAllNamespacesReturnMessage(GetAllNamespacesAck result)
        {
            Console.WriteLine("GetAllNamespacesAck CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayGetAllChannelGroupsReturnMessage(GetAllChannelGroupsAck result)
        {
            Console.WriteLine("GetAllChannelGroupsAck CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayGetChannelsForChannelGroupReturnMessage(GetChannelGroupChannelsAck result)
        {
            Console.WriteLine("GetChannelGroupChannelsAck CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayTimeReturnMessage(long result)
        {
            Console.WriteLine("TIME CALLBACK:");
            Console.WriteLine(result.ToString());
            Console.WriteLine("");
        }

        static void DisplayGetUserStateReturnMessage(GetUserStateAck result)
        {
            Console.WriteLine("GET USERSTATE CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplaySetUserStateReturnMessage(SetUserStateAck result)
        {
            Console.WriteLine("SET USERSTATE CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayWhereNowReturnMessage(WhereNowAck result)
        {
            Console.WriteLine("WHERENOW CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayGlobalHereNowReturnMessage(GlobalHereNowAck result)
        {
            Console.WriteLine("GlobalHereNow CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayHereNowReturnMessage(HereNowAck result)
        {
            Console.WriteLine("HERENOW CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayDetailedHistoryReturnMessage(DetailedHistoryAck result)
        {
            Console.WriteLine("DETAILED HISTORY CALLBACK:");
            Console.WriteLine("Total Records = " + result.Message.Length.ToString());
            for (int index = 0; index < result.Message.Length; index++)
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result.Message[index]));
            }

            Console.WriteLine("");
        }

        static void DisplayPublishReturnMessage(PublishAck result)
        {
            Console.WriteLine("PUBLISH CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayPresenceAckMessage(PresenceAck result)
        {
            Console.WriteLine("PRESENCE CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        /// <summary>
        /// Callback method captures the response for Grant requests
        /// </summary>
        /// <param name="result"></param>
        static void DisplayGrantReturnMessage(GrantAck result)
        {
            Console.WriteLine("GRANT CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        /// <summary>
        /// Callback method captures the response for Audit requests
        /// </summary>
        /// <param name="result"></param>
        static void DisplayAuditReturnMessage(AuditAck result)
        {
            Console.WriteLine("AUDIT CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Subscribe
        /// </summary>
        /// <param name="result"></param>
        static void DisplaySubscribeReturnMessage(Message<string> result)
        {
            Console.WriteLine("SUBSCRIBE REGULAR CALLBACK for string:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }
        static void DisplaySubscribeReturnMessage(Message<object> result)
        {
            Console.WriteLine("SUBSCRIBE REGULAR CALLBACK for object:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }
        static void DisplaySubscribeReturnMessage(Message<PubnubDemoObject> result)
        {
            Console.WriteLine("SUBSCRIBE REGULAR CALLBACK for PubnubDemoObject:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }
        static void DisplaySubscribeReturnMessage(Message<UserCreated> result)
        {
            Console.WriteLine("SUBSCRIBE REGULAR CALLBACK for UserCreated:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }


        /// <summary>
        /// Callback method captures wildcard presence events for Subscribe
        /// </summary>
        /// <param name="result"></param>
        static void DisplayWildCardPresenceReturnMessage(PresenceAck result)
        {
            Console.WriteLine("WILDCARD PRESENCE CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Presence
        /// </summary>
        /// <param name="result"></param>
        static void DisplayPresenceReturnMessage(PresenceAck result)
        {
            Console.WriteLine("PRESENCE REGULAR CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        static void DisplayConnectDisconnectStatusMessage(ConnectOrDisconnectAck result)
        {
            Console.WriteLine("CONNECT OR DISCONNECT CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        /// <summary>
        /// Callback method to provide the connect status of Presence call
        /// </summary>
        /// <param name="result"></param>
        static void DisplayPresenceConnectStatusMessage(ConnectOrDisconnectAck result)
        {
            Console.WriteLine("PRESENCE CONNECT CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplaySubscribeDisconnectStatusMessage(ConnectOrDisconnectAck result)
        {
            Console.WriteLine("SUBSCRIBE DISCONNECT CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            Console.WriteLine("");
        }

        static void DisplayPresenceDisconnectStatusMessage(ConnectOrDisconnectAck result)
        {
            Console.WriteLine("PRESENCE DISCONNECT CALLBACK:");
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
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


    #endregion

}
