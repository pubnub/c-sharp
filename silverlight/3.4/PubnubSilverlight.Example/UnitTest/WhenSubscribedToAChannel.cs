using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Microsoft.Silverlight.Testing;
using PubNubMessaging.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PubnubSilverlight.UnitTest
{
    [TestClass]
    public class WhenSubscribedToAChannel : SilverlightTest
    {
        bool receivedMessageInCallback = false;
        bool publishInCallback = false;

        bool receivedSubscribeMessage = false;
        bool receivedConnectMessage = false;
        bool receivedAlreadySubscribedMessage = false;
        bool receivedChannel1ConnectMessage = false;
        bool receivedChannel2ConnectMessage = false;
        bool receivedManyMessages = false;

        int numberOfReceivedMessages = 0;

        //bool subscribeConnected = false;
        bool subscribeConnectedBeforeDuplicate = false;

        [TestMethod, Asynchronous]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            receivedSubscribeMessage = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            string channel = "my/channel";
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnReceivedMessage";
            pubnub.PubnubUnitTest = unitTest;

            EnqueueCallback(() => pubnub.Subscribe<string>(channel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, DummyErrorCallback));
            EnqueueConditional(() => receivedSubscribeMessage);
            //EnqueueConditional(() => subscribeConnected);
            EnqueueCallback(() => pubnub.Publish<string>(channel, "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage", dummyPublishCallback, DummyErrorCallback));
            EnqueueConditional(() => publishInCallback);
            //EnqueueConditional(() => receivedMessageInCallback);
            EnqueueCallback(() => pubnub.EndPendingRequests());
            EnqueueCallback(() => Assert.IsTrue(receivedSubscribeMessage, "WhenSubscribedToAChannel --> ThenItShouldReturnReceivedMessage Failed"));

            EnqueueTestComplete();
        }

        // START

        [TestMethod, Asynchronous]
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            receivedConnectMessage = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnConnectStatus";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "my/channel";

            EnqueueCallback(() => pubnub.Subscribe<string>(channel, ReceivedMessageCallbackYesConnect, ConnectStatusCallback, DummyErrorCallback));
            EnqueueConditional(() => receivedConnectMessage);

            EnqueueCallback(() => pubnub.EndPendingRequests());
            EnqueueCallback(() => Thread.Sleep(200));

            EnqueueCallback(() => Assert.IsTrue(receivedConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed"));
            EnqueueTestComplete();
        }

        [TestMethod, Asynchronous]
        public void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            receivedChannel1ConnectMessage = false;
            receivedChannel2ConnectMessage = false;
            ThreadPool.QueueUserWorkItem((s) =>
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenSubscribedToAChannel";
                unitTest.TestCaseName = "ThenMultiSubscribeShouldReturnConnectStatus";

                pubnub.PubnubUnitTest = unitTest;

                string channel1 = "my/channel1";
                EnqueueCallback(() => pubnub.Subscribe<string>(channel1, ReceivedChannelUserCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback));
                EnqueueConditional(() => receivedChannel1ConnectMessage);

                string channel2 = "my/channel2";
                EnqueueCallback(() => pubnub.Subscribe<string>(channel2, ReceivedChannelUserCallback, ReceivedChannel2ConnectCallback, DummyErrorCallback));
                EnqueueConditional(() => receivedChannel2ConnectMessage);

                EnqueueCallback(() => pubnub.EndPendingRequests());
                EnqueueCallback(() => Thread.Sleep(100));


                EnqueueCallback(() => Assert.IsTrue(receivedChannel1ConnectMessage && receivedChannel2ConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed"));
                EnqueueTestComplete();
                
            });
        }

        [TestMethod, Asynchronous]
        public void ThenDuplicateChannelShouldReturnAlreadySubscribed()
        {
            subscribeConnectedBeforeDuplicate = false;
            receivedAlreadySubscribedMessage = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenDuplicateChannelShouldReturnAlreadySubscribed";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "my/channel";

            EnqueueCallback(() => pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback1, DummyMethodDuplicateChannelConnectCallback, DummyErrorCallback));
            EnqueueConditional(() => subscribeConnectedBeforeDuplicate);
            EnqueueCallback(() => Thread.Sleep(100));

            EnqueueCallback(() => pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback2, DummyMethodDuplicateChannelConnectCallback, DummyErrorCallback));
            //receivedAlreadySubscribedMessage = true;
            EnqueueConditional(() => receivedAlreadySubscribedMessage);
            EnqueueCallback(() => Thread.Sleep(100));

            EnqueueCallback(() => pubnub.EndPendingRequests());

            EnqueueCallback(() => Assert.IsTrue(receivedAlreadySubscribedMessage, "WhenSubscribedToAChannel --> ThenDuplicateChannelShouldReturnAlreadySubscribed Failed"));
            EnqueueTestComplete();
        }

        [TestMethod, Asynchronous]
        public void ThenSubscriberShouldBeAbleToReceiveManyMessages()
        {
            receivedManyMessages = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenSubscribedToAChannel";
                unitTest.TestCaseName = "ThenSubscriberShouldBeAbleToReceiveManyMessages";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "my/channel";

                EnqueueCallback(() => pubnub.Subscribe<string>(channel, SubscriberDummyMethodForManyMessagesUserCallback, SubscribeDummyMethodForManyMessagesConnectCallback, DummyErrorCallback));
                //EnqueueCallback(() => Thread.Sleep(1000));
                Thread.Sleep(1000);
                //meSubscriberManyMessages.WaitOne();
                EnqueueConditional(() => receivedManyMessages);

                EnqueueCallback(() => pubnub.EndPendingRequests());

                Assert.IsTrue(receivedManyMessages, "WhenSubscribedToAChannel --> ThenSubscriberShouldBeAbleToReceiveManyMessages Failed");
                EnqueueTestComplete();
            });
        }

        [Asynchronous]
        private void SubscriberDummyMethodForManyMessagesUserCallback(string result)
        {
            numberOfReceivedMessages = numberOfReceivedMessages + 1;
            if (numberOfReceivedMessages >= 10)
            {
                receivedManyMessages = true;
            }
        }

        [Asynchronous]
        private void SubscribeDummyMethodForManyMessagesConnectCallback(string result)
        {
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
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelUserCallback1(string result)
        {
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelUserCallback2(string result)
        {
            if (result.Contains("already subscribed"))
            {
                receivedAlreadySubscribedMessage = true;
            }
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelConnectCallback(string result)
        {
            subscribeConnectedBeforeDuplicate = true;
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
                        receivedSubscribeMessage = true;
                    }
                }
            }
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
        }

        [Asynchronous]
        private void dummyUnsubscribeCallback(string result)
        {

        }

        [Asynchronous]
        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        void UnsubscribeDummyMethodForDisconnectCallback(string receivedMessage)
        {

        }

        // LAST
        
        //[Asynchronous]
        //private void ReceivedMessageCallback(string result)
        //{
        //    if (!string.IsNullOrWhiteSpace(result))
        //    {
        //        object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
        //        if (deserializedMessage is object[])
        //        {
        //            object subscribedObj = (object)deserializedMessage[0];
        //            if (subscribedObj != null)
        //            {
        //                receivedMessage = true;
        //            }
        //        }
        //    }
        //    receivedMessageInCallback = true;
        //}

        [Asynchronous]
        private void dummyPublishCallback(string result)
        {
            publishInCallback = true;
        }

        [Asynchronous]
        private void DummyErrorCallback(string result)
        {
        }

    }
}
