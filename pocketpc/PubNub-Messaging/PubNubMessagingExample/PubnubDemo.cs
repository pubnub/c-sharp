using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PubNubMessaging.Core;
using System.Threading;

namespace PubNubMessagingExample
{
    public partial class PubnubDemo : Form
    {
        private static Pubnub pubnub;
        private delegate void UpdateErrorResultDelegate(PubnubClientError message);
        private delegate void UpdateCallbackResultDelegate(string message);
        private string _channelName = "";
        private string _channelGroupName = "";

        public string Origin = "";
        public string PublishKey = "";
        public string SubscribeKey = "";
        public string SecretKey = "";
        public string CipherKey = "";
        public bool EnableSSL = false;
        public string SessionUUID = "";
        public bool EnableResumeOnReconnect = true;
        public int PresenceHeartbeatInSec = 0;
        public int PresenceHeartbeatIntervalInSec = 0;

        public PubnubPAM pamForm = null;
        PubnubUserState userStateForm = null;
        PubnubPush pushForm = null;
        PubnubChannelGroup channelGroupForm = null;
        PubnubChangeAuthKey authKeyForm = null;
        PubnubChangeUUID changeUUIDForm = null;

        public PubnubDemo()
        {
          
            InitializeComponent();

            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            bool result = ThreadPool.SetMaxThreads(100, completionPortThreads);
            if (!result)
            {
                //int j = 0;
            }
        }

        public void PubnubInitialize()
        {
            pubnub = new Pubnub(this.PublishKey, this.SubscribeKey, this.SecretKey, this.CipherKey, this.EnableSSL);
            pubnub.Origin = this.Origin;
            pubnub.SessionUUID = this.SessionUUID;
            pubnub.EnableResumeOnReconnect = this.EnableResumeOnReconnect;
            pubnub.PresenceHeartbeat = this.PresenceHeartbeatInSec;
            pubnub.PresenceHeartbeatInterval = this.PresenceHeartbeatIntervalInSec;
            pubnub.NetworkCheckMaxRetries = Int32.MaxValue;
        }

        private void CaptureInputData()
        {
            _channelName = txtChannel.Text;
            _channelGroupName = txtChannelGroup.Text;
        }

        private void btnSubscribe_Click(object sender, EventArgs e)
        {
            CaptureInputData();
            UpdateListBoxForDisplay("Running Subscribe");
            pubnub.Subscribe<string>(_channelName, _channelGroupName, DisplayReturnMessage, DisplayReturnMessage, null, DisplayErrorMessage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPresence_Click(object sender, EventArgs e)
        {
            // Check if presense channel value is present
            if (string.IsNullOrEmpty(txtChannel.Text.Trim()))
            {
                MessageBox.Show("Please enter channel name");
                return;
            }
            CaptureInputData();
            UpdateListBoxForDisplay("Running Presence");
            pubnub.Presence<string>(_channelName, _channelGroupName, DisplayReturnMessage, DisplayReturnMessage, DisplayErrorMessage);
        }

        private void btnPresenceUnsub_Click(object sender, EventArgs e)
        {
            CaptureInputData();
            UpdateListBoxForDisplay("Running Presence Unsubscribe");
            pubnub.PresenceUnsubscribe<string>(_channelName, _channelGroupName, DisplayReturnMessage, DisplayReturnMessage, DisplayReturnMessage, DisplayErrorMessage);
        }

        private void btnUnsubscribe_Click(object sender, EventArgs e)
        {
            CaptureInputData();
            UpdateListBoxForDisplay("Running Unsubscribe");
            pubnub.Unsubscribe<string>(_channelName, _channelGroupName, DisplayReturnMessage, DisplayReturnMessage, DisplayReturnMessage, DisplayErrorMessage);
        }
       
        private void btnPublish_Click(object sender, EventArgs e)
        {
            PublishForm publishForm = new PublishForm();
            publishForm.ShowDialog();
            string publishMessage = publishForm.Message;
            bool storeInHistory = publishForm.StoreInHistory;

            CaptureInputData();
            UpdateListBoxForDisplay("Running Publish");
            pubnub.Publish<string>(_channelName, publishMessage, storeInHistory, DisplayReturnMessage, DisplayErrorMessage);
        }

        private void btnTime_Click(object sender, EventArgs e)
        {
            UpdateListBoxForDisplay("Running Time");
            pubnub.Time<string>(DisplayReturnMessage, DisplayErrorMessage);
        }

        private void btnHereNow_Click(object sender, EventArgs e)
        {
            CaptureInputData();
            UpdateListBoxForDisplay("Running HereNow");
            pubnub.HereNow<string>(_channelName, DisplayReturnMessage, DisplayErrorMessage);
        }

        private void btnDetailedHistory_Click(object sender, EventArgs e)
        {
            CaptureInputData();
            UpdateListBoxForDisplay("Running DetailedHistory");
            pubnub.DetailedHistory<string>(_channelName, 10, DisplayReturnMessage, DisplayErrorMessage);
        }


        private void btnGlobalHereNow_Click(object sender, EventArgs e)
        {
            UpdateListBoxForDisplay("Running GlobalHereNow");
            pubnub.GlobalHereNow<string>(false,false,DisplayReturnMessage, DisplayErrorMessage);
        }

        private void btnWhereNow_Click(object sender, EventArgs e)
        {
            UpdateListBoxForDisplay("Running WhereNow");
            pubnub.WhereNow<string>(this.SessionUUID, DisplayReturnMessage, DisplayErrorMessage);
        }

        private void btnUserState_Click(object sender, EventArgs e)
        {
            userStateForm = new PubnubUserState();
            userStateForm.UserStateRequestSubmitted += new EventHandler(userStateForm_UserStateRequestSubmitted);
            userStateForm.Show();

        }



        private void menuItemGrant_Click(object sender, EventArgs e)
        {
            pamForm = new PubnubPAM();
            pamForm.PamRequestSubmitted += new EventHandler(pamForm_PamRequestSubmitted);
            pamForm.Text = "Grant";
            pamForm.PamRequest = "Grant";
            pamForm.Show();

        }

        private void menuItemAudit_Click(object sender, EventArgs e)
        {
            pamForm = new PubnubPAM();
            pamForm.PamRequestSubmitted += new EventHandler(pamForm_PamRequestSubmitted);
            pamForm.Text = "Audit";
            pamForm.PamRequest = "Audit";
            pamForm.Show();

        }

        private void menuItemRevoke_Click(object sender, EventArgs e)
        {
            pamForm = new PubnubPAM();
            pamForm.PamRequestSubmitted += new EventHandler(pamForm_PamRequestSubmitted);
            pamForm.Text = "Revoke";
            pamForm.PamRequest = "Revoke";
            pamForm.Show();
        }

        void pamForm_PamRequestSubmitted(object sender, EventArgs e)
        {
            switch (pamForm.PamRequest)
            {
                case "Grant":
                    if (!string.IsNullOrEmpty(pamForm.Channel))
                    {
                        if (pamForm.IsPresence)
                        {
                            UpdateListBoxForDisplay("Running GrantPresenceAccess()");
                            pubnub.GrantPresenceAccess<string>(pamForm.Channel, pamForm.AuthKey, true, true, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            UpdateListBoxForDisplay("Running GrantAccess()");
                            pubnub.GrantAccess<string>(pamForm.Channel, pamForm.AuthKey, true, true, DisplayReturnMessage, DisplayErrorMessage);
                        }
                    }
                    else if (!string.IsNullOrEmpty(pamForm.ChannelGroup))
                    {
                        if (pamForm.IsPresence)
                        {
                            UpdateListBoxForDisplay("Running ChannelGroupGrantPresenceAccess()");
                            pubnub.ChannelGroupGrantPresenceAccess<string>(pamForm.Channel, pamForm.AuthKey, true, true, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            UpdateListBoxForDisplay("Running ChannelGroupGrantAccess()");
                            pubnub.ChannelGroupGrantAccess<string>(pamForm.Channel, pamForm.AuthKey, true, true, DisplayReturnMessage, DisplayErrorMessage);
                        }
                    }
                    break;
                case "Audit":
                    if (!string.IsNullOrEmpty(pamForm.Channel))
                    {
                        if (pamForm.IsPresence)
                        {
                            UpdateListBoxForDisplay("Running AuditPresenceAccess()");
                            pubnub.AuditPresenceAccess<string>(pamForm.Channel, pamForm.AuthKey, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            UpdateListBoxForDisplay("Running AuditAccess()");
                            pubnub.AuditAccess<string>(pamForm.Channel, pamForm.AuthKey, DisplayReturnMessage, DisplayErrorMessage);
                        }
                    }
                    else if (!string.IsNullOrEmpty(pamForm.ChannelGroup))
                    {
                        if (pamForm.IsPresence)
                        {
                            UpdateListBoxForDisplay("Running ChannelGroupAuditPresenceAccess()");
                            pubnub.ChannelGroupAuditPresenceAccess<string>(pamForm.ChannelGroup, pamForm.AuthKey, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            UpdateListBoxForDisplay("Running ChannelGroupAuditAccess()");
                            pubnub.ChannelGroupAuditAccess<string>(pamForm.ChannelGroup, pamForm.AuthKey, DisplayReturnMessage, DisplayErrorMessage);
                        }
                    }
                    break;
                case "Revoke":
                    if (!string.IsNullOrEmpty(pamForm.Channel))
                    {
                        if (pamForm.IsPresence)
                        {
                            UpdateListBoxForDisplay("Running GrantPresenceAccess()");
                            pubnub.GrantPresenceAccess<string>(pamForm.Channel, pamForm.AuthKey, false, false, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            UpdateListBoxForDisplay("Running GrantAccess()");
                            pubnub.GrantAccess<string>(pamForm.Channel, pamForm.AuthKey, false, false, DisplayReturnMessage, DisplayErrorMessage);
                        }
                    }
                    else if (!string.IsNullOrEmpty(pamForm.ChannelGroup))
                    {
                        if (pamForm.IsPresence)
                        {
                            UpdateListBoxForDisplay("Running ChannelGroupGrantPresenceAccess()");
                            pubnub.ChannelGroupGrantPresenceAccess<string>(pamForm.Channel, pamForm.AuthKey, false, false, DisplayReturnMessage, DisplayErrorMessage);
                        }
                        else
                        {
                            UpdateListBoxForDisplay("Running ChannelGroupGrantAccess()");
                            pubnub.ChannelGroupGrantAccess<string>(pamForm.Channel, pamForm.AuthKey, false, false, DisplayReturnMessage, DisplayErrorMessage);
                        }
                    }
                    break;
            }
        }

        void userStateForm_UserStateRequestSubmitted(object sender, EventArgs e)
        {
            string channelName = userStateForm.ChannelName;
            string channelGroupName = userStateForm.ChannelGroupName;

            switch (userStateForm.UserStateRequest)
            {
                case "GetUserState":
                    pubnub.GetUserState<string>(channelName, channelGroupName, DisplayReturnMessage, DisplayErrorMessage);
                    break;
                case "SetUserState":
                    pubnub.SetUserState<string>(channelName, channelGroupName, new KeyValuePair<string,object>(userStateForm.StateKey, userStateForm.StateValue), DisplayReturnMessage, DisplayErrorMessage);
                    break;
            }
            userStateForm = null;
        }

        void DisplayReturnMessage(string result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateCallbackResultDelegate(DisplayReturnMessage), new object[] { result });
                return;
            }
            else
            {
                UpdateListBoxForDisplay(result);
            }
        }

        void DisplayErrorMessage(PubnubClientError result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateErrorResultDelegate(DisplayErrorMessage), new object[] { result });
                return;
            }
            else
            {
                UpdateListBoxForDisplay(result.Description);
            }
            Console.WriteLine();
            Console.WriteLine(result.Description);
            System.Diagnostics.Debug.WriteLine(result.Description);
            System.Diagnostics.Debug.WriteLine(result.Message);
            Console.WriteLine();

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
            bool showErrorMessageSegments = false;
            if (showErrorMessageSegments)
            {
                DisplayErrorMessageSegments(result);
                Console.WriteLine();
            }
        }

        static void DisplayErrorMessageSegments(PubnubClientError pubnubError)
        {
            // These are all the attributes you may be interested in logging, switchiing on etc:

            Console.WriteLine("<STATUS CODE>: {0}", pubnubError.StatusCode); // Unique ID of Error

            Console.WriteLine("<MESSAGE>: {0}", pubnubError.Message); // Message received from server/clent or from .NET exception

            Console.WriteLine("<SEVERITY>: {0}", pubnubError.Severity); // Info can be ignored, Warning and Error should be handled

            if (pubnubError.DetailedDotNetException != null)
            {
                Console.WriteLine(pubnubError.IsDotNetException); // Boolean flag to check .NET exception
                Console.WriteLine("<DETAILED DOT.NET EXCEPTION>: {0}", pubnubError.DetailedDotNetException.ToString()); // Full Details of .NET exception
            }
            Console.WriteLine("<MESSAGE SOURCE>: {0}", pubnubError.MessageSource); // Did this originate from Server or Client-side logic
            if (pubnubError.PubnubWebRequest != null)
            {
                //Captured Web Request details
                Console.WriteLine("<HTTP WEB REQUEST>: {0}", pubnubError.PubnubWebRequest.RequestUri.ToString());
                Console.WriteLine("<HTTP WEB REQUEST - HEADERS>: {0}", pubnubError.PubnubWebRequest.Headers.ToString());
            }
            if (pubnubError.PubnubWebResponse != null)
            {
                //Captured Web Response details
                Console.WriteLine("<HTTP WEB RESPONSE - HEADERS>: {0}", pubnubError.PubnubWebResponse.Headers.ToString());
            }
            Console.WriteLine("<DESCRIPTION>: {0}", pubnubError.Description); // Useful for logging and troubleshooting and support
            Console.WriteLine("<CHANNEL>: {0}", pubnubError.Channel); //Channel name(s) at the time of error
            Console.WriteLine("<DATETIME>: {0}", pubnubError.ErrorDateTimeGMT); //GMT time of error

        }

        void UpdateListBoxForDisplay(string message)
        {
            if (message.Length > 100) message = message.Substring(0, 100);

            lbResult.Items.Insert(0, message);
            while (lbResult.Items.Count > 20)
            {
                lbResult.Items.RemoveAt(lbResult.Items.Count - 1);
            }
        }

        private void menuItemSettings_Click(object sender, EventArgs e)
        {
            PubnubInitForm initForm = new PubnubInitForm();
            initForm.Show();
            this.Close();
        }

        private void menuItemNetworkDisconnect_Click(object sender, EventArgs e)
        {
            menuItemNetworkDisconnect.Checked = true;
            menuItemNetworkConnect.Checked = false;
            pubnub.EnableSimulateNetworkFailForTestingOnly();
        }

        private void menuItemNetworkConnect_Click(object sender, EventArgs e)
        {
            menuItemNetworkDisconnect.Checked = false;
            menuItemNetworkConnect.Checked = true;
            pubnub.DisableSimulateNetworkFailForTestingOnly();
        }

        private void menuItemDisconAutoReconnect_Click(object sender, EventArgs e)
        {
            pubnub.TerminateCurrentSubscriberRequest();
        }

        private void btnPush_Click(object sender, EventArgs e)
        {
            pushForm = new PubnubPush();
            pushForm.PushRequestSubmitted += new EventHandler(pushForm_PushRequestSubmitted);
            pushForm.Show();

        }


        void pushForm_PushRequestSubmitted(object sender, EventArgs e)
        {
            string channelName = pushForm.ChannelName;
            string pushToken = pushForm.PushToken;

            switch (pushForm.PushTypeRequest)
            {
                case "RegisterDevice":
                    UpdateListBoxForDisplay("Running RegisterDeviceForPush()");
                    pubnub.RegisterDeviceForPush<string>(channelName, PushTypeService.MPNS, pushToken, DisplayReturnMessage, DisplayErrorMessage);
                    break;
                case "UnregisterDevice":
                    UpdateListBoxForDisplay("Running UnregisterDeviceForPush()");
                    pubnub.UnregisterDeviceForPush<string>(PushTypeService.MPNS, pushToken, DisplayReturnMessage, DisplayErrorMessage);
                    break;
                case "RemoveChannel":
                    UpdateListBoxForDisplay("Running RegisterDeviceForPush()");
                    pubnub.RemoveChannelForDevicePush<string>(channelName, PushTypeService.MPNS, pushToken, DisplayReturnMessage, DisplayErrorMessage);
                    break;
                case "GetChannels":
                    UpdateListBoxForDisplay("Running RegisterDeviceForPush()");
                    pubnub.GetChannelsForDevicePush<string>(PushTypeService.MPNS, pushToken, DisplayReturnMessage, DisplayErrorMessage);
                    break;
            }
            pushForm = null;
        }

        private void menuItemCGAddRemoveChannel_Click(object sender, EventArgs e)
        {
            channelGroupForm = new PubnubChannelGroup();
            channelGroupForm.ChannelGroupRequestSubmitted += new EventHandler(channelGroupForm_ChannelGroupRequestSubmitted);
            channelGroupForm.Show();

        }

        void channelGroupForm_ChannelGroupRequestSubmitted(object sender, EventArgs e)
        {
            string channelName = channelGroupForm.ChannelName;
            string channelGroupName = channelGroupForm.ChannelGroupName;
            string nameSpace = "";

            switch (channelGroupForm.ChannelGroupRequestType)
            {
                case "AddChannel":
                    UpdateListBoxForDisplay("Running AddChannelsToChannelGroup()");
                    pubnub.AddChannelsToChannelGroup<string>(channelName.Split(','), nameSpace, channelGroupName, DisplayReturnMessage, DisplayErrorMessage);
                    break;
                case "RemoveChannel":
                    UpdateListBoxForDisplay("Running RemoveChannelsFromChannelGroup()");
                    pubnub.RemoveChannelsFromChannelGroup<string>(channelName.Split(','), nameSpace, channelGroupName, DisplayReturnMessage, DisplayErrorMessage);
                    break;
                case "GetChannelList":
                    UpdateListBoxForDisplay("Running GetChannelsForChannelGroup()");
                    pubnub.GetChannelsForChannelGroup<string>(nameSpace, channelGroupName, DisplayReturnMessage, DisplayErrorMessage);
                    break;
            }

            channelGroupForm = null;
        }

        private void menuItemChangeAuthKey_Click(object sender, EventArgs e)
        {
            authKeyForm = new PubnubChangeAuthKey();
            authKeyForm.OldAuthKey = pubnub.AuthenticationKey;
            authKeyForm.ChangeAuthKeySubmitted += new EventHandler(authKeyForm_ChangeAuthKeySubmitted);
            authKeyForm.Show();
        }

        void authKeyForm_ChangeAuthKeySubmitted(object sender, EventArgs e)
        {
            string newAuthKey = authKeyForm.NewAuthKey;
            pubnub.AuthenticationKey = newAuthKey;
            UpdateListBoxForDisplay("Updated AuthKey");
        }

        private void menuItemChangeUUID_Click(object sender, EventArgs e)
        {
            changeUUIDForm = new PubnubChangeUUID();
            changeUUIDForm.OldUUID = pubnub.SessionUUID;
            changeUUIDForm.ChangeUUIDSubmitted += new EventHandler(changeUUIDForm_ChangeUUIDSubmitted);
            changeUUIDForm.Show();
        }

        void changeUUIDForm_ChangeUUIDSubmitted(object sender, EventArgs e)
        {
            string newUUID = changeUUIDForm.NewUUID;
            this.SessionUUID = newUUID;
            UpdateListBoxForDisplay("Updating UUID");
            pubnub.ChangeUUID(newUUID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosed(object sender, EventArgs e)
        {
            try
            {
               Application.Exit();
            }
            catch( Exception ex)
            {
               string errMsg = ex.Message;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickClose(object sender, EventArgs e)
        {
            try
            {
                if (pubnub != null)
                {
                    pubnub.EndPendingRequests();
               }
                this.Close();
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
            }
        }

        
    }
}