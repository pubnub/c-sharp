using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PubNubMessaging.Core;
using System.Threading;
using System.Web.Caching;
using System.Collections.Concurrent;

namespace PubNubMessaging
{
    public partial class PubnubExampleNET40 : System.Web.UI.Page
    {
        protected static Pubnub pubnub;

        static string channel = "";
        static string channelGroup = "";
        static bool ssl = false;
        static string origin = "";
        static string publishKey = "";
        static string subscriberKey = "";
        static string secretKey = "";
        static string cipherKey = "";
        static string uuid = "";
        static string authKey = "";
        static bool resumeOnReconnect = false;

        static int subscribeTimeoutInSeconds = 0;
        static int operationTimeoutInSeconds = 0;
        static int networkMaxRetries = 0;
        static int networkRetryIntervalInSeconds = 0;
        static int localClientheartbeatIntervalInSeconds = 0;
        static int presenceHeartbeat = 0;
        static int presenceHeartbeatInterval = 0;


        ManualResetEvent mre = new ManualResetEvent(false);
        private object _lockObject = new object();
        private static List<string> _pubnubResult = new List<string>();

        internal class RecordStatusHolder
        {
            public string Record
            {
                get;
                set;
            }

            public bool Status
            {
                get;
                set;
            }
        }

        private static ConcurrentQueue<string> _recordQueue = new ConcurrentQueue<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            txtMessage.Attributes.Add("onmouseover", "javascript:MessageDisplayHolderMouseOver();");
            txtMessage.Attributes.Add("onmouseout", "javascript:MessageDisplayHolderMouseOut();");
            if (!Page.IsPostBack)
            {
                txtUUID.Text = "myuuid";
            }
        }

        private void CheckUserInputs()
        {
            if (ssl != chkSSL.Checked
                || origin != txtOrigin.Text
                || publishKey != txtPubKey.Text
                || subscriberKey != txtSubKey.Text
                || secretKey != txtSecret.Text
                || cipherKey != txtCipher.Text)
            {
                //pubnub.EndPendingRequests();
                pubnub = null;
            }
            ssl = chkSSL.Checked;
            origin = txtOrigin.Text;
            publishKey = txtPubKey.Text;
            subscriberKey = txtSubKey.Text;
            secretKey = txtSecret.Text;
            cipherKey = txtCipher.Text;
            uuid = txtUUID.Text;
            authKey = txtAuthKey.Text;
            resumeOnReconnect = chkResumeOnReconnect.Checked;

            Int32.TryParse(txtSubscribeTimeout.Text, out subscribeTimeoutInSeconds);
            subscribeTimeoutInSeconds = (subscribeTimeoutInSeconds <= 0) ? 310 : subscribeTimeoutInSeconds;

            Int32.TryParse(txtNonSubscribeTimeout.Text, out operationTimeoutInSeconds);
            operationTimeoutInSeconds = (operationTimeoutInSeconds <= 0) ? 15 : operationTimeoutInSeconds;

            Int32.TryParse(txtNetworkMaxRetries.Text, out networkMaxRetries);
            networkMaxRetries = (networkMaxRetries <= 0) ? 50 : networkMaxRetries;

            Int32.TryParse(txtRetryInterval.Text, out networkRetryIntervalInSeconds);
            networkRetryIntervalInSeconds = (networkRetryIntervalInSeconds <= 0) ? 10 : networkRetryIntervalInSeconds;

            Int32.TryParse(txtLocalClientHeartbeatInterval.Text, out localClientheartbeatIntervalInSeconds);
            localClientheartbeatIntervalInSeconds = (localClientheartbeatIntervalInSeconds <= 0) ? 10 : localClientheartbeatIntervalInSeconds;

            Int32.TryParse(txtPresenceHeartbeat.Text, out presenceHeartbeat);

            Int32.TryParse(txtPresenceHeartbeatInterval.Text, out presenceHeartbeatInterval);

            if (pubnub == null)
            {
                pubnub = new Pubnub(publishKey, subscriberKey, secretKey, cipherKey, ssl);
                pubnub.Origin = origin;
                txtOrigin.Enabled = false;
                txtPubKey.Enabled = false;
                txtSubKey.Enabled = false;
                txtSecret.Enabled = false;
                txtCipher.Enabled = false;
                //txtUUID.Enabled = false;
                chkSSL.Enabled = false;
                chkResumeOnReconnect.Enabled = false;

                txtSubscribeTimeout.Enabled = false;
                txtNonSubscribeTimeout.Enabled = false;
                txtNetworkMaxRetries.Enabled = false;
                txtRetryInterval.Enabled = false;
                txtLocalClientHeartbeatInterval.Enabled = false;
                txtPresenceHeartbeat.Enabled = false;
                txtPresenceHeartbeatInterval.Enabled = false;

                btnReset.Enabled = true;
            }
            pubnub.SessionUUID = uuid;
            pubnub.AuthenticationKey = authKey;
            pubnub.SubscribeTimeout = subscribeTimeoutInSeconds;
            pubnub.NonSubscribeTimeout = operationTimeoutInSeconds;
            pubnub.NetworkCheckMaxRetries = networkMaxRetries;
            pubnub.NetworkCheckRetryInterval = networkRetryIntervalInSeconds;
            pubnub.LocalClientHeartbeatInterval = localClientheartbeatIntervalInSeconds;
            pubnub.EnableResumeOnReconnect = resumeOnReconnect;
            pubnub.PresenceHeartbeat = presenceHeartbeat;
            pubnub.PresenceHeartbeatInterval = presenceHeartbeatInterval;
        }

        private void AddToPubnubResultContainer(string result)
        {
            _recordQueue.Enqueue(result);
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for all operations
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayUserCallbackMessage(string result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer("REGULAR CALLBACK");
            AddToPubnubResultContainer(result);
            AddToPubnubResultContainer("");
        }


        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayConnectCallbackMessage(string result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer("CONNECT CALLBACK");
            AddToPubnubResultContainer(result);
            AddToPubnubResultContainer("");
        }

        protected void DisplayDisconnectCallbackMessage(string result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer("DISCONNECT CALLBACK");
            AddToPubnubResultContainer(result);
            AddToPubnubResultContainer("");
        }

        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayErrorMessage(string result)
        {
        }
        /// <summary>
        /// Callback method for error messages
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayErrorMessage(PubnubClientError result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer("ERROR CALLBACK");
            AddToPubnubResultContainer(result.Description);
            AddToPubnubResultContainer("");

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
                case 4010:
                    //Critical: Please provide correct subscribe key. This corresponds to a 401 on the server due to a bad sub key
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
        }

        protected void btnTime_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnSubscribe_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnPresence_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnDetailedHistory_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnPublish_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnUnsubscribe_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnHereNow_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnPresenceUnsub_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnDisableNetwork_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnEnableNetwork_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnDisconnectAndRetry_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnGrant_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnRevoke_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnAudit_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnWhereNow_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnGlobalHereNow_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnSetUserState_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnGetUserState_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        protected void btnChannelGroup_Command(object sender, CommandEventArgs e)
        {
            ProcessPubnubRequest(e.CommandName);
        }

        void ProcessPubnubRequest(string requestType)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            channelGroup = txtChannelGroup.Text.Trim();

            bool storeInHistory = chkStoreInHistory.Checked;
            string pamAuthKey = txtPAMAuthKey.Text.Trim();
            UpdateTimer.Enabled = true;

            switch (requestType.ToLower())
            {
                case "presence":
                    if (channelGroup == "")
                    {
                        pubnub.Presence<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
                    }
                    else
                    {
                        pubnub.Presence<string>(channel, channelGroup, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
                    }
                    break;
                case "subscribe":
                    if (channelGroup == "")
                    {
                        pubnub.Subscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
                    }
                    else
                    {
                        pubnub.Subscribe<string>(channel, channelGroup, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayErrorMessage);
                    }
                    break;
                case "detailedhistory":
                    pubnub.DetailedHistory<string>(channel, 10, DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "publish":
                    if (radNormalPublish.Checked)
                    {
                        string messageToBePublished = txtPublishMessage.Text;
                        pubnub.Publish<string>(channel, messageToBePublished, storeInHistory, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (radToastPublish.Checked)
                    {
                        MpnsToastNotification toast = new MpnsToastNotification();
                        //toast.type = "toast";
                        toast.text1 = "hardcode message";
                        Dictionary<string, object> dicToast = new Dictionary<string, object>();
                        dicToast.Add("pn_mpns", toast);

                        pubnub.EnableDebugForPushPublish = true;
                        pubnub.Publish<string>(channel, dicToast, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (radFlipTilePublish.Checked)
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
                        pubnub.Publish<string>(channel, dicTile, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (radCycleTilePublish.Checked)
                    {
                        MpnsCycleTileNotification tile = new MpnsCycleTileNotification();
                        tile.title = "front title";
                        tile.count = 2;
                        tile.images = new string[] { "Assets/Tiles/pubnub1.png", "Assets/Tiles/pubnub2.png", "Assets/Tiles/pubnub3.png", "Assets/Tiles/pubnub4.png" };

                        Dictionary<string, object> dicTile = new Dictionary<string, object>();
                        dicTile.Add("pn_mpns", tile);

                        pubnub.EnableDebugForPushPublish = true;
                        pubnub.Publish<string>(channel, dicTile, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (radIconicTilePublish.Checked)
                    {
                        MpnsIconicTileNotification tile = new MpnsIconicTileNotification();
                        tile.title = "front title";
                        tile.count = 2;
                        tile.wide_content_1 = "my wide content";

                        Dictionary<string, object> dicTile = new Dictionary<string, object>();
                        dicTile.Add("pn_mpns", tile);

                        pubnub.EnableDebugForPushPublish = true;
                        pubnub.Publish<string>(channel, dicTile, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    txtPublishMessage.Text = "";
                    lblErrorMessage.Text = "";
                    break;
                case "unsubscribe":
                    if (channelGroup == "")
                    {
                        pubnub.Unsubscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayDisconnectCallbackMessage, DisplayErrorMessage);
                    }
                    else
                    {
                        pubnub.Unsubscribe<string>(channel, channelGroup, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayDisconnectCallbackMessage, DisplayErrorMessage);
                    }
                    break;
                case "presenceunsubscribe":
                    if (channelGroup == "")
                    {
                        pubnub.PresenceUnsubscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayDisconnectCallbackMessage, DisplayErrorMessage);
                    }
                    else
                    {
                        pubnub.PresenceUnsubscribe<string>(channel, channelGroup, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayDisconnectCallbackMessage, DisplayErrorMessage);
                    }
                    break;
                case "herenow":
                    bool showUUID2 = chbShowUUIDList2.Checked;
                    bool includeUserState2 = chbShowUserState2.Checked;
                    pubnub.HereNow<string>(channel, showUUID2, includeUserState2, DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "globalherenow":
                    bool showUUID = chbShowUUIDList.Checked;
                    bool includeUserState = chbShowUserState.Checked;
                    pubnub.GlobalHereNow<string>(showUUID, includeUserState, DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "wherenow":
                    pubnub.WhereNow<string>(uuid, DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "time":
                    pubnub.Time<string>(DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "grantaccess":
                    pubnub.GrantAccess<string>(channel, pamAuthKey, true, true, 60, DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "revokeaccess":
                    pubnub.GrantAccess<string>(channel, pamAuthKey, false, false, DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "auditaccess":
                    pubnub.AuditAccess<string>(channel, pamAuthKey, DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "disablenetwork":
                    pubnub.EnableSimulateNetworkFailForTestingOnly();
                    break;
                case "enablenetwork":
                    pubnub.DisableSimulateNetworkFailForTestingOnly();
                    break;
                case "disconnectandretry":
                    pubnub.TerminateCurrentSubscriberRequest();
                    break;
                case "getuserstate":
                    string getUserStateUUID = txtGetUserStateUUID.Text;
                    pubnub.GetUserState<string>(channel, getUserStateUUID,  DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "jsonsetuserstate":
                    string jsonUserState = txtJsonUserState.Text;
                    pubnub.SetUserState<string>(channel, jsonUserState, DisplayUserCallbackMessage, DisplayErrorMessage);
                    break;
                case "setuserstate":
                    string key1 = txtKey1.Text;
                    string value1 = txtValue1.Text;
                    
                    int valueInt;
                    double valueDouble;

                    if (Int32.TryParse(value1, out valueInt))
                    {
                        pubnub.SetUserState<string>(channel, new KeyValuePair<string, object>(key1, valueInt), DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (Double.TryParse(value1, out valueDouble))
                    {
                        pubnub.SetUserState<string>(channel, new KeyValuePair<string, object>(key1, valueDouble), DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else
                    {
                        pubnub.SetUserState<string>(channel, new KeyValuePair<string, object>(key1, value1), DisplayUserCallbackMessage, DisplayErrorMessage);
                    }

                    break;
                case "channelgroup":
                    string userChannelGroupName = txtChannelGroupName.Text;
                    if (radChannelGroupGet.Checked)
                    {
                        pubnub.GetChannelsForChannelGroup<string>(userChannelGroupName, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (radChannelGroupAdd.Checked)
                    {
                        string userChannelGroupAddChannel = txtChannelGroupAddChannels.Text;
                        pubnub.AddChannelsToChannelGroup<string>(new string[] { userChannelGroupAddChannel }, userChannelGroupName, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (radChannelGroupRemove.Checked)
                    {
                        string userChannelGroupRemoveChannel = txtChannelGroupRemoveChannels.Text;
                        pubnub.RemoveChannelsFromChannelGroup<string>(new string[] { userChannelGroupRemoveChannel }, userChannelGroupName, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (radChannelGroupGrant.Checked)
                    {
                        pubnub.ChannelGroupGrantAccess<string>(userChannelGroupName, true, true, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (radChannelGroupAudit.Checked)
                    {
                        pubnub.ChannelGroupAuditAccess<string>(userChannelGroupName, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    else if (radChannelGroupRevoke.Checked)
                    {
                        pubnub.ChannelGroupGrantAccess<string>(userChannelGroupName, false, false, DisplayUserCallbackMessage, DisplayErrorMessage);
                    }
                    break;
                default:
                    break;
            }
        }

        protected void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateResultView();
        }

        private void UpdateResultView()
        {
            string recordTest;
            if (_recordQueue.TryPeek(out recordTest))
            {
                if (txtUUID.Text.Trim() == "")
                {
                    txtUUID.Text = pubnub.SessionUUID;
                }

                if (txtMessage.Text.Length > 1000)
                {
                    string trucatedMessage = "..(truncated)..." + txtMessage.Text.Substring(txtMessage.Text.Length - 300);
                    txtMessage.Text = trucatedMessage;
                }

                string currentRecord;
                while (_recordQueue.TryDequeue(out currentRecord))
                {
                    txtMessage.Text += string.Format("{0}{1}", currentRecord, Environment.NewLine);
                    System.Diagnostics.Debug.WriteLine(currentRecord);
                }
            }

            UpdatePanelRight.Update();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (pubnub != null)
            {
                pubnub.EndPendingRequests();
                pubnub = null;
            }
            txtOrigin.Enabled = true;
            txtPubKey.Enabled = true;
            txtSubKey.Enabled = true;
            txtSecret.Enabled = true;
            txtCipher.Enabled = true;
            //txtUUID.Enabled = true;
            chkSSL.Enabled = true;
            chkResumeOnReconnect.Enabled = true;

            txtSubscribeTimeout.Enabled = true;
            txtNonSubscribeTimeout.Enabled = true;
            txtNetworkMaxRetries.Enabled = true;
            txtRetryInterval.Enabled = true;
            txtLocalClientHeartbeatInterval.Enabled = true;
            txtPresenceHeartbeat.Enabled = true;
            txtPresenceHeartbeatInterval.Enabled = true;

            btnReset.Enabled = false;

        }

        protected void btnOkay_OnClick(object sender, EventArgs e)
        {
            btnReset.Enabled = true;

            lblErrorMessage.Text = "";

            publishModalPopupExtender.Hide();

            ProcessPubnubRequest("publish");

            UpdatePanelLeft.Update();
        }

        protected void btnOkayGlobalHereNow_OnClick(object sender, EventArgs e)
        {
            btnReset.Enabled = true;

            lblErrorMessage.Text = "";

            globalHereNowPopupExtender.Hide();

            ProcessPubnubRequest("globalherenow");
            UpdatePanelLeft.Update();
        }

        protected void btnOkayHereNow_OnClick(object sender, EventArgs e)
        {
            btnReset.Enabled = true;

            lblErrorMessage.Text = "";

            hereNowPopupExtender.Hide();

            ProcessPubnubRequest("herenow");
            UpdatePanelLeft.Update();
        }

        protected void btnOkayJsonSetUserState_OnClick(object sender, EventArgs e)
        {
            btnReset.Enabled = true;
            lblErrorMessage.Text = "";

            setUserStatePopupExtender.Hide();

            ProcessPubnubRequest("jsonsetuserstate");

            UpdatePanelLeft.Update();
        }

        protected void btnOkaySetUserState_OnClick(object sender, EventArgs e)
        {
            btnReset.Enabled = true;
            lblErrorMessage.Text = "";

            setUserStatePopupExtender.Hide();

            ProcessPubnubRequest("setuserstate");
            
            UpdatePanelLeft.Update();
        }

        protected void btnOkayGetUserState_OnClick(object sender, EventArgs e)
        {
            btnReset.Enabled = true;
            lblErrorMessage.Text = "";

            getUserStatePopupExtender.Hide();

            ProcessPubnubRequest("getuserstate");

            UpdatePanelLeft.Update();
        }

        protected void btnOkayChannelGroup_OnClick(object sender, EventArgs e)
        {
            btnReset.Enabled = true;

            lblErrorMessage.Text = "";

            channelGroupPopupExtender.Hide();

            ProcessPubnubRequest("channelgroup");

            UpdatePanelLeft.Update();
        }
    }
}

