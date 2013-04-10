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
        static bool ssl = false;
        static string secretKey = "";
        static string cipherKey = "";
        static string uuid = "";
        static bool resumeOnReconnect = false;

        static int subscribeTimeoutInSeconds = 0;
        static int operationTimeoutInSeconds = 0;
        static int networkMaxRetries = 0;
        static int networkRetryIntervalInSeconds = 0;
        static int heartbeatIntervalInSeconds = 0;


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
        }

        private void CheckUserInputs()
        {
            ssl = chkSSL.Checked;
            secretKey = txtSecret.Text;
            cipherKey = txtCipher.Text;
            uuid = txtUUID.Text;
            resumeOnReconnect = chkResumeOnReconnect.Checked;

            Int32.TryParse(txtSubscribeTimeout.Text, out subscribeTimeoutInSeconds);
            subscribeTimeoutInSeconds = (subscribeTimeoutInSeconds <= 0) ? 310 : subscribeTimeoutInSeconds;

            Int32.TryParse(txtNonSubscribeTimeout.Text, out operationTimeoutInSeconds);
            operationTimeoutInSeconds = (operationTimeoutInSeconds <= 0) ? 15 : operationTimeoutInSeconds;

            Int32.TryParse(txtNetworkMaxRetries.Text, out networkMaxRetries);
            networkMaxRetries = (networkMaxRetries <= 0) ? 50 : networkMaxRetries;

            Int32.TryParse(txtRetryInterval.Text, out networkRetryIntervalInSeconds);
            networkRetryIntervalInSeconds = (networkRetryIntervalInSeconds <= 0) ? 10 : networkRetryIntervalInSeconds;

            Int32.TryParse(txtHeartbeatInterval.Text, out heartbeatIntervalInSeconds);
            heartbeatIntervalInSeconds = (heartbeatIntervalInSeconds <= 0) ? 10 : heartbeatIntervalInSeconds;

            if (pubnub == null)
            {
                pubnub = new Pubnub("demo", "demo", secretKey, cipherKey, ssl);
                txtSecret.Enabled = false;
                txtCipher.Enabled = false;
                txtUUID.Enabled = false;
                chkSSL.Enabled = false;
                chkResumeOnReconnect.Enabled = false;

                txtSubscribeTimeout.Enabled = false;
                txtNonSubscribeTimeout.Enabled = false;
                txtNetworkMaxRetries.Enabled = false;
                txtRetryInterval.Enabled = false;
                txtHeartbeatInterval.Enabled = false;

                btnReset.Enabled = true;
            }
            pubnub.SessionUUID = uuid;
            pubnub.SubscribeTimeout = subscribeTimeoutInSeconds;
            pubnub.NonSubscribeTimeout = operationTimeoutInSeconds;
            pubnub.NetworkCheckMaxRetries = networkMaxRetries;
            pubnub.NetworkCheckRetryInterval = networkRetryIntervalInSeconds;
            pubnub.HeartbeatInterval = heartbeatIntervalInSeconds;
            pubnub.EnableResumeOnReconnect = resumeOnReconnect;
        }

        private void AddToPubnubResultContainer(string result)
        {
            _recordQueue.Enqueue(result);
            //lock (_lockObject)
            //{
            //    RecordStatusHolder recordHolder = new RecordStatusHolder();
            //    recordHolder.Record = result;
            //    recordHolder.Status = false;
            //    _pubnubRecordHolder.Add(
            //    int recordsCount = _pubnubResult.Count;
            //    if (recordsCount > 50)
            //    {
            //        _pubnubResult.RemoveRange(0, recordsCount - 50);
            //    }
            //    _pubnubResult.Add(result);
            //}
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for all operations
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayUserCallbackMessage(string result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer(result);
        }


        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        protected void DisplayConnectCallbackMessage(string result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer(result);
        }

        protected void DisplayDisconnectCallbackMessage(string result)
        {
            UpdateTimer.Enabled = true;
            AddToPubnubResultContainer(result);
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

        void ProcessPubnubRequest(string requestType)
        {
            CheckUserInputs();
            channel = txtChannel.Text;
            string messageToBePublished = txtPublishMessage.Text;
            UpdateTimer.Enabled = true;

            switch (requestType.ToLower())
            {
                case "presence":
                    pubnub.Presence<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage);
                    break;
                case "subscribe":
                    pubnub.Subscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage);
                    break;
                case "detailedhistory":
                    pubnub.DetailedHistory<string>(channel, 10, DisplayUserCallbackMessage);
                    break;
                case "publish":
                    pubnub.Publish<string>(channel, messageToBePublished, DisplayUserCallbackMessage);
                    txtPublishMessage.Text = "";
                    lblErrorMessage.Text = "";
                    break;
                case "unsubscribe":
                    pubnub.Unsubscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayDisconnectCallbackMessage);
                    break;
                case "presenceunsubscribe":
                    pubnub.PresenceUnsubscribe<string>(channel, DisplayUserCallbackMessage, DisplayConnectCallbackMessage, DisplayDisconnectCallbackMessage);
                    break;
                case "herenow":
                    pubnub.HereNow<string>(channel, DisplayUserCallbackMessage);
                    break;
                case "time":
                    pubnub.Time<string>(DisplayUserCallbackMessage);
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
                string currentRecord;
                while (_recordQueue.TryDequeue(out currentRecord))
                {
                    txtMessage.Text += string.Format("{0}{1}", currentRecord, Environment.NewLine);
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
            txtSecret.Enabled = true;
            txtCipher.Enabled = true;
            txtUUID.Enabled = true;
            chkSSL.Enabled = true;
            chkResumeOnReconnect.Enabled = true;

            txtSubscribeTimeout.Enabled = true;
            txtNonSubscribeTimeout.Enabled = true;
            txtNetworkMaxRetries.Enabled = true;
            txtRetryInterval.Enabled = true;
            txtHeartbeatInterval.Enabled = true;

            btnReset.Enabled = false;

        }

        protected void btnOkay_OnClick(object sender, EventArgs e)
        {
            if (txtPublishMessage.Text.Trim() != "")
            {
                btnReset.Enabled = true;

                lblErrorMessage.Text = "";

                publishModalPopupExtender.Hide();

                ProcessPubnubRequest("publish");
            }
            else
            {
                lblErrorMessage.Text = "* Message cannot be blank.";
            }
            UpdatePanelLeft.Update();
        }



    }
}

