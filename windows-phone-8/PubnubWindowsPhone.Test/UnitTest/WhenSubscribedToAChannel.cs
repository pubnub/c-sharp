using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Phone.Testing;
using PubNubMessaging.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;


namespace PubnubWindowsPhone.Test.UnitTest
{
    [TestClass]
    public class WhenSubscribedToAChannel : WorkItemTest
    {
        ManualResetEvent meSubscribeNoConnect = new ManualResetEvent(false);
        ManualResetEvent meSubscribeYesConnect = new ManualResetEvent(false);
        ManualResetEvent mePublish = new ManualResetEvent(false);
        ManualResetEvent meUnsubscribe = new ManualResetEvent(false);
        ManualResetEvent meAlreadySubscribed = new ManualResetEvent(false);
        ManualResetEvent meChannel1SubscribeConnect = new ManualResetEvent(false);
        ManualResetEvent meChannel2SubscribeConnect = new ManualResetEvent(false);
        ManualResetEvent meSubscriberManyMessages = new ManualResetEvent(false);
        ManualResetEvent subscribeEvent = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool receivedMessage = false;
        bool receivedConnectMessage = false;
        bool receivedAlreadySubscribedMessage = false;
        bool receivedChannel1ConnectMessage = false;
        bool receivedChannel2ConnectMessage = false;
        bool receivedManyMessages = false;
        bool receivedGrantMessage = false;

        int numberOfReceivedMessages = 0;

        [ClassInitialize]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Inconclusive("WhenAClientIsPresent Grant access failed.");
                return;
            }

            receivedGrantMessage = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.GrantAccess<string>(channel, true, true, 20, ThenSubscriberInitializeShouldReturnGrantMessage, DummyErrorCallback);
            //Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed.");
        }

        [Asynchronous]
        void ThenSubscriberInitializeShouldReturnGrantMessage(string receivedMessage)
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
            finally
            {
                grantManualEvent.Set();
            }
        }

        [TestMethod]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            string channel = "hello_my_channel";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnReceivedMessage";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Subscribe<string>(channel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeEvent.WaitOne(30 * 1000);
            //Thread.Sleep(500);
            pubnub.Publish<string>(channel, "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage", dummyPublishCallback, DummyErrorCallback);
            mePublish.WaitOne(310 * 1000);
            //Thread.Sleep(500);
            meSubscribeNoConnect.WaitOne(310 * 1000);
            pubnub.Unsubscribe<string>(channel, dummyUnsubscribeCallback, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(500);
            meUnsubscribe.WaitOne(310 * 1000);

            Thread.Sleep(1000);
            pubnub.EndPendingRequests();
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenItShouldReturnReceivedMessage Failed");
        }

        [TestMethod]
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            receivedConnectMessage = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnConnectStatus";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.Subscribe<string>(channel, ReceivedMessageCallbackYesConnect, ConnectStatusCallback, DummyErrorCallback);
            meSubscribeYesConnect.WaitOne(310 * 1000);

            pubnub.EndPendingRequests();
            Thread.Sleep(200);
            Assert.IsTrue(receivedConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [TestMethod]
        public void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            receivedChannel1ConnectMessage = false;
            receivedChannel2ConnectMessage = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenMultiSubscribeShouldReturnConnectStatus";

            pubnub.PubnubUnitTest = unitTest;


            string channel1 = "hello_my_channel1";
            pubnub.Subscribe<string>(channel1, ReceivedChannelUserCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback);
            meChannel1SubscribeConnect.WaitOne(310 * 1000);

            string channel2 = "hello_my_channel2";
            pubnub.Subscribe<string>(channel2, ReceivedChannelUserCallback, ReceivedChannel2ConnectCallback, DummyErrorCallback);
            meChannel2SubscribeConnect.WaitOne(310 * 1000);

            Thread.Sleep(500);

            pubnub.EndPendingRequests();

            Assert.IsTrue(receivedChannel1ConnectMessage && receivedChannel2ConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [TestMethod]
        public void ThenDuplicateChannelShouldReturnAlreadySubscribed()
        {
            receivedAlreadySubscribedMessage = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenDuplicateChannelShouldReturnAlreadySubscribed";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback1, DummyMethodDuplicateChannelConnectCallback, DummyErrorCallback);
            Thread.Sleep(100);

            pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback2, DummyMethodDuplicateChannelConnectCallback, DuplicateChannelErrorCallback);
            meAlreadySubscribed.WaitOne();

            pubnub.EndPendingRequests();

            Assert.IsTrue(receivedAlreadySubscribedMessage, "WhenSubscribedToAChannel --> ThenDuplicateChannelShouldReturnAlreadySubscribed Failed");
        }

        [TestMethod]
        public void ThenSubscriberShouldBeAbleToReceiveManyMessages()
        {
            receivedManyMessages = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenSubscriberShouldBeAbleToReceiveManyMessages";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.Subscribe<string>(channel, SubscriberDummyMethodForManyMessagesUserCallback, SubscribeDummyMethodForManyMessagesConnectCallback, DummyErrorCallback);
            subscribeEvent.WaitOne(310 * 1000);
            if (!unitTest.EnableStubTest)
            {
                for (int index = 0; index < 10; index++)
                {
                    pubnub.Publish<string>(channel, index.ToString(), dummyPublishCallback, DummyErrorCallback);
                    mePublish.WaitOne(10 * 1000);
                }
            }

            meSubscriberManyMessages.WaitOne(310 * 1000);

            pubnub.EndPendingRequests();

            Assert.IsTrue(receivedManyMessages, "WhenSubscribedToAChannel --> ThenSubscriberShouldBeAbleToReceiveManyMessages Failed");
        }

        [Asynchronous]
        private void SubscriberDummyMethodForManyMessagesUserCallback(string result)
        {
            numberOfReceivedMessages = numberOfReceivedMessages + 1;
            if (numberOfReceivedMessages >= 10)
            {
                receivedManyMessages = true;
                meSubscriberManyMessages.Set();
            }
        }

        [Asynchronous]
        private void SubscribeDummyMethodForManyMessagesConnectCallback(string result)
        {
            subscribeEvent.Set();
        }

        [Asynchronous]
        private void ReceivedChannelUserCallback(string result)
        {
        }

        [Asynchronous]
        private void ReceivedChannel1ConnectCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "connected")
                    {
                        receivedChannel1ConnectMessage = true;
                    }
                }
            }
            meChannel1SubscribeConnect.Set();
        }

        [Asynchronous]
        private void ReceivedChannel2ConnectCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "connected")
                    {
                        receivedChannel2ConnectMessage = true;
                    }
                }
            }
            meChannel2SubscribeConnect.Set();
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelUserCallback1(string result)
        {
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelUserCallback2(string result)
        {

        }

        private void DuplicateChannelErrorCallback(PubnubClientError result)
        {
            if (result != null && result.Message.ToLower().Contains("already subscribed"))
            {
                receivedAlreadySubscribedMessage = true;
            }
            meAlreadySubscribed.Set();
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelConnectCallback(string result)
        {
        }

        [Asynchronous]
        private void ReceivedMessageCallbackWhenSubscribed(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    object subscribedObject = (object)deserializedMessage[0];
                    if (subscribedObject != null)
                    {
                        receivedMessage = true;
                    }
                }
            }
            meSubscribeNoConnect.Set();
        }


        [Asynchronous]  
        private void ReceivedMessageCallbackYesConnect(string result)
        {
            //dummy method provided as part of subscribe connect status check.
        }

        [Asynchronous]
        private void ConnectStatusCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "connected")
                    {
                        receivedConnectMessage = true;
                    }
                }
            }
            meSubscribeYesConnect.Set();
        }

        [Asynchronous]
        private void dummyPublishCallback(string result)
        {
            mePublish.Set();
        }

        [Asynchronous]
        private void dummyUnsubscribeCallback(string result)
        {

        }

        [Asynchronous]
        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
            subscribeEvent.Set();
        }

        [Asynchronous]
        void UnsubscribeDummyMethodForDisconnectCallback(string receivedMessage)
        {
            meUnsubscribe.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
