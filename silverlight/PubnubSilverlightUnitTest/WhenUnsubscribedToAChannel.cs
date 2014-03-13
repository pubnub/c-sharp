using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Silverlight.Testing;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace PubnubSilverlight.UnitTest
{
    [TestClass]
    public class WhenUnsubscribedToAChannel : SilverlightTest
    {
        ManualResetEvent meNotSubscribed = new ManualResetEvent(false);
        ManualResetEvent meChannelSubscribed = new ManualResetEvent(false);
        ManualResetEvent meChannelUnsubscribed = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool receivedNotSubscribedMessage = false;
        bool receivedUnsubscribedMessage = false;
        bool receivedChannelConnectedMessage = false;
        bool receivedGrantMessage = false;

        bool grantInitInvoked = false;
        bool nochannelUnsubscribeInvoked = false;
        bool unsubscribeDisconnectInvoked = false;
        bool subscribeConnectInvoked = false;

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                EnqueueTestComplete();
                return;
            }

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            EnqueueCallback(() => pubnub.GrantAccess<string>(channel, true, true, 20, ThenUnsubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback));
            EnqueueConditional(() => grantInitInvoked);
            EnqueueCallback(() => Assert.IsTrue(receivedGrantMessage, "WhenUnsubscribedToAChannel Grant access failed."));
            EnqueueTestComplete();
        }

        [TestMethod, Asynchronous]
        public void ThenNoExistChannelShouldReturnNotSubscribed()
        {
            receivedNotSubscribedMessage = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenUnsubscribedToAChannel";
            unitTest.TestCaseName = "ThenNoExistChannelShouldReturnNotSubscribed";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            EnqueueCallback(() => pubnub.Unsubscribe<string>(channel, DummyMethodNoExistChannelUnsubscribeChannelUserCallback, DummyMethodNoExistChannelUnsubscribeChannelConnectCallback, DummyMethodNoExistChannelUnsubscribeChannelDisconnectCallback1, NoExistChannelErrorCallback));
            EnqueueConditional(() => nochannelUnsubscribeInvoked);
            EnqueueCallback(() => pubnub.EndPendingRequests());
            EnqueueCallback(() => Assert.IsTrue(receivedNotSubscribedMessage, "WhenUnsubscribedToAChannel --> ThenNoExistChannelShouldReturnNotSubscribed Failed"));
            EnqueueTestComplete();
        }

        [TestMethod, Asynchronous]
        public void ThenShouldReturnUnsubscribedMessage()
        {
            receivedChannelConnectedMessage = false;
            receivedUnsubscribedMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenUnsubscribedToAChannel";
            unitTest.TestCaseName = "ThenShouldReturnUnsubscribedMessage";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            EnqueueCallback(() => pubnub.Subscribe<string>(channel, DummyMethodChannelSubscribeUserCallback, DummyMethodChannelSubscribeConnectCallback, DummyErrorCallback));
            EnqueueConditional(() => subscribeConnectInvoked);

            EnqueueConditional(() => receivedChannelConnectedMessage);
            EnqueueCallback(() => pubnub.Unsubscribe<string>(channel, DummyMethodUnsubscribeChannelUserCallback, DummyMethodUnsubscribeChannelConnectCallback, DummyMethodUnsubscribeChannelDisconnectCallback, DummyErrorCallback));
            EnqueueConditional(() => unsubscribeDisconnectInvoked);

            EnqueueCallback(() => pubnub.EndPendingRequests());
            EnqueueCallback(() => Assert.IsTrue(receivedUnsubscribedMessage, "WhenUnsubscribedToAChannel --> ThenShouldReturnUnsubscribedMessage Failed"));
            EnqueueTestComplete();
        }

        [Asynchronous]
        void ThenUnsubscribeInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var status = dictionary["status"].ToString();
                    if (status == "200")
                    {
                        receivedGrantMessage = true;
                    }
                }
            }
            catch { }

            grantInitInvoked = true;
        }

        [Asynchronous]
        private void DummyMethodChannelSubscribeUserCallback(string result)
        {
        }

        [Asynchronous]
        private void DummyMethodChannelSubscribeConnectCallback(string result)
        {
            if (result.Contains("Connected"))
            {
                receivedChannelConnectedMessage = true;
            }
            subscribeConnectInvoked = true;
        }


        [Asynchronous]
        private void DummyMethodUnsubscribeChannelUserCallback(string result)
        {
        }

        [Asynchronous]
        private void DummyMethodUnsubscribeChannelConnectCallback(string result)
        {
        }

        [Asynchronous]
        private void DummyMethodUnsubscribeChannelDisconnectCallback(string result)
        {
            if (result.Contains("Unsubscribed from"))
            {
                receivedUnsubscribedMessage = true;
            }
            unsubscribeDisconnectInvoked = true;
        }

        [Asynchronous]
        private void DummyMethodNoExistChannelUnsubscribeChannelUserCallback(string result)
        {
        }

        [Asynchronous]
        private void DummyMethodNoExistChannelUnsubscribeChannelConnectCallback(string result)
        {
        }

        [Asynchronous]
        private void DummyMethodNoExistChannelUnsubscribeChannelDisconnectCallback1(string result)
        {
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

        [Asynchronous]
        private void NoExistChannelErrorCallback(PubnubClientError result)
        {
            if (result != null && result.Message.ToLower().Contains("not subscribed"))
            {
                receivedNotSubscribedMessage = true;
            }
            nochannelUnsubscribeInvoked = true;
        }
    }
}
