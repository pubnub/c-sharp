﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class GenerateSampleGrant
    {
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        bool receivedGrantMessage = false;
        int sampleCount = 100;

        Pubnub pubnub = null;

        [Test]
        public void AtUserLevel()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM not enabled; GenerateSampleGrant -> AtUserLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedGrantMessage = false;

                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                for (int index = 0; index < sampleCount; index++)
                {
                    grantManualEvent = new ManualResetEvent(false);
                    string channelName = string.Format("csharp-pam-ul-channel-{0}", index);
                    string authKey = string.Format("csharp-pam-authkey-0-{0},csharp-pam-authkey-1-{1}", index, index);
                    pubnub.GrantAccess<string>(channelName, authKey, true, true, UserCallbackForSampleGrantAtUserLevel, ErrorCallbackForSampleGrantAtUserLevel);
                    grantManualEvent.WaitOne();
                }

                pubnub.EndPendingRequests();
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "GenerateSampleGrant -> AtUserLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; GenerateSampleGrant -> AtUserLevel.");
            }
        }

        [Test]
        public void AtChannelLevel()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM not enabled; GenerateSampleGrant -> AtChannelLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedGrantMessage = false;

                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                for (int index = 0; index < sampleCount; index++)
                {
                    grantManualEvent = new ManualResetEvent(false);
                    string channelName = string.Format("csharp-pam-cl-channel-{0}", index);
                    pubnub.GrantAccess<string>(channelName, true, true, UserCallbackForSampleGrantAtChannelLevel, ErrorCallbackForSampleGrantAtChannelLevel);
                    grantManualEvent.WaitOne();
                }

                pubnub.EndPendingRequests();
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "GenerateSampleGrant -> AtChannelLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; GenerateSampleGrant -> AtChannelLevel.");
            }
        }

        void UserCallbackForSampleGrantAtUserLevel(string receivedMessage)
        {
            receivedGrantMessage = true;
            Console.WriteLine(receivedMessage);
            grantManualEvent.Set();
        }

        void ErrorCallbackForSampleGrantAtUserLevel(PubnubClientError receivedMessage)
        {
            if (receivedMessage != null)
            {
                Console.WriteLine(receivedMessage);
            }
            grantManualEvent.Set();
        }

        void UserCallbackForSampleGrantAtChannelLevel(string receivedMessage)
        {
            receivedGrantMessage = true;
            Console.WriteLine(receivedMessage);
            grantManualEvent.Set();
        }

        void ErrorCallbackForSampleGrantAtChannelLevel(PubnubClientError receivedMessage)
        {
            if (receivedMessage != null)
            {
                Console.WriteLine(receivedMessage);
            }
            grantManualEvent.Set();
        }

    }
}
