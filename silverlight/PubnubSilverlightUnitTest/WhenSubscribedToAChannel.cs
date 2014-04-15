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
        bool receivedSubscribeMessage = false;
        bool receivedConnectMessage = false;
        bool receivedAlreadySubscribedMessage = false;
        bool receivedChannel1ConnectMessage = false;
        bool receivedChannel2ConnectMessage = false;
        bool receivedManyMessages = false;

        int numberOfReceivedMessages = 0;

        bool receivedGrantMessage = false;
        //bool grantInitCallbackInvoked = false;

        ManualResetEvent mreSubscribe = new ManualResetEvent(false);
        ManualResetEvent mreConnect = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                EnqueueTestComplete();
                return;
            }

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "GrantRequestUnitTest";
                    unitTest.TestCaseName = "Init";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel,hello_my_channel1,hello_my_channel2";

                    EnqueueCallback(() => pubnub.GrantAccess<string>(channel, true, true, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback));
                    mreGrant.WaitOne(310 * 1000);

                    //EnqueueConditional(() => grantInitCallbackInvoked);

                    EnqueueCallback(() => Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannel Grant access failed."));
                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        void ThenSubscribeInitializeShouldReturnGrantMessage(string receivedMessage)
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
                mreGrant.Set();
            }
        }


        [TestMethod, Asynchronous]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            receivedSubscribeMessage = false;
            mreConnect = new ManualResetEvent(false);
            mreSubscribe = new ManualResetEvent(false);
            mrePublish = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    string channel = "hello_my_channel";
                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenSubscribedToAChannel";
                    unitTest.TestCaseName = "ThenSubscribeShouldReturnReceivedMessage";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Subscribe<string>(channel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, DummyErrorCallback));
                    mreConnect.WaitOne(310 * 1000);

                    EnqueueCallback(() => pubnub.Publish<string>(channel, "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage", dummyPublishCallback, DummyErrorCallback));
                    mrePublish.WaitOne(310 * 1000);

                    mreSubscribe.WaitOne(310 * 1000);

                    EnqueueCallback(() => pubnub.EndPendingRequests());
                    EnqueueCallback(() => Assert.IsTrue(receivedSubscribeMessage, "WhenSubscribedToAChannel --> ThenItShouldReturnReceivedMessage Failed"));
                    
                    EnqueueTestComplete();
                });
        }


        [TestMethod, Asynchronous]
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            receivedConnectMessage = false;
            
            mreConnect = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenSubscribedToAChannel";
                    unitTest.TestCaseName = "ThenSubscribeShouldReturnConnectStatus";

                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";

                    EnqueueCallback(() => pubnub.Subscribe<string>(channel, ReceivedMessageCallbackYesConnect, ConnectStatusCallback, DummyErrorCallback));
                    mreConnect.WaitOne(310 * 1000);
                    //EnqueueConditional(() => receivedConnectMessage);

                    EnqueueCallback(() => pubnub.EndPendingRequests());
                    //EnqueueCallback(() => Thread.Sleep(200));

                    EnqueueCallback(() => Assert.IsTrue(receivedConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed"));
                    EnqueueTestComplete();
                });
        }

        [TestMethod, Asynchronous]
        public void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            receivedChannel1ConnectMessage = false;
            receivedChannel2ConnectMessage = false;

            mreConnect = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
            {
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenSubscribedToAChannel";
                unitTest.TestCaseName = "ThenMultiSubscribeShouldReturnConnectStatus";

                pubnub.PubnubUnitTest = unitTest;

                string channel1 = "hello_my_channel1";
                EnqueueCallback(() => pubnub.Subscribe<string>(channel1, ReceivedChannelUserCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback));
                mreConnect.WaitOne(310 * 1000);
                //EnqueueConditional(() => receivedChannel1ConnectMessage);

                mreConnect = new ManualResetEvent(false);
                string channel2 = "hello_my_channel2";
                EnqueueCallback(() => pubnub.Subscribe<string>(channel2, ReceivedChannelUserCallback, ReceivedChannel2ConnectCallback, DummyErrorCallback));
                mreConnect.WaitOne(310 * 1000);
                //EnqueueConditional(() => receivedChannel2ConnectMessage);

                EnqueueCallback(() => pubnub.EndPendingRequests());
                //EnqueueCallback(() => Thread.Sleep(100));


                EnqueueCallback(() => Assert.IsTrue(receivedChannel1ConnectMessage && receivedChannel2ConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed"));
                EnqueueTestComplete();
                
            });
        }

        [TestMethod, Asynchronous]
        public void ThenDuplicateChannelShouldReturnAlreadySubscribed()
        {
            //subscribeConnectedBeforeDuplicate = false;
            receivedAlreadySubscribedMessage = false;

            mreConnect = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenSubscribedToAChannel";
                    unitTest.TestCaseName = "ThenDuplicateChannelShouldReturnAlreadySubscribed";
                    pubnub.PubnubUnitTest = unitTest;
                    string channel = "hello_my_channel";

                    EnqueueCallback(() => pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback1, DummyMethodDuplicateChannelConnectCallback, DummyErrorCallback));
                    mreConnect.WaitOne(310 * 1000);
                    //EnqueueConditional(() => subscribeConnectedBeforeDuplicate);
                    //EnqueueCallback(() => Thread.Sleep(100));

                    mreConnect = new ManualResetEvent(false);
                    EnqueueCallback(() => pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback2, DummyMethodDuplicateChannelConnectCallback, DuplicateChannelErrorCallback2));
                    mreConnect.WaitOne(310 * 1000);
                    //receivedAlreadySubscribedMessage = true;
                    //EnqueueConditional(() => receivedAlreadySubscribedMessage);
                    //EnqueueCallback(() => Thread.Sleep(100));

                    EnqueueCallback(() => pubnub.EndPendingRequests());

                    EnqueueCallback(() => Assert.IsTrue(receivedAlreadySubscribedMessage, "WhenSubscribedToAChannel --> ThenDuplicateChannelShouldReturnAlreadySubscribed Failed"));
                    EnqueueTestComplete();
                });
        }

        [TestMethod, Asynchronous]
        public void ThenSubscriberShouldBeAbleToReceiveManyMessages()
        {
            receivedManyMessages = false;
            mreSubscribe = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenSubscribedToAChannel";
                    unitTest.TestCaseName = "ThenSubscriberShouldBeAbleToReceiveManyMessages";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";

                    EnqueueCallback(() => pubnub.Subscribe<string>(channel, SubscriberDummyMethodForManyMessagesUserCallback, SubscribeDummyMethodForManyMessagesConnectCallback, DummyErrorCallback));
                    if (!PubnubCommon.EnableStubTest)
                    {
                        for (int index = 0; index < 10; index++)
                        {
                            mrePublish = new ManualResetEvent(false);
                            EnqueueCallback(() => pubnub.Publish<string>(channel, index, SubscribeManyMessagesPublishCallback, DummyErrorCallback));
                            //EnqueueCallback(() => Thread.Sleep(100));
                            mrePublish.WaitOne(310 * 1000);
                            //EnqueueConditional(() => detailedHistoryPublishCallbackInvoked);
                        }
                    }

                    mreSubscribe.WaitOne(310 * 1000);
                    //EnqueueCallback(() => Thread.Sleep(1000));

                    //meSubscriberManyMessages.WaitOne();
                    //EnqueueConditional(() => receivedManyMessages);

                    EnqueueCallback(() => pubnub.EndPendingRequests());

                    EnqueueCallback(() => Assert.IsTrue(receivedManyMessages, "WhenSubscribedToAChannel --> ThenSubscriberShouldBeAbleToReceiveManyMessages Failed"));
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
            mreSubscribe.Set();
        }

        [Asynchronous]
        private void SubscribeDummyMethodForManyMessagesConnectCallback(string result)
        {
        }

        [Asynchronous]
        public void SubscribeManyMessagesPublishCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    int statusCode = Int32.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        //good
                    }
                }
            }
            //detailedHistoryPublishCallbackInvoked = true;
            mrePublish.Set();
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
            mreConnect.Set();
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
            mreConnect.Set();
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
        private void DuplicateChannelErrorCallback2(PubnubClientError result)
        {
            if (result != null && result.StatusCode > 0)
            {
                if (result.Message.ToLower().Contains("already subscribed"))
                {
                    receivedAlreadySubscribedMessage = true;
                }
            }
            mreConnect.Set();
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelConnectCallback(string result)
        {
            //subscribeConnectedBeforeDuplicate = true;
            mreConnect.Set();
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
            mreSubscribe.Set();
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
            mreConnect.Set();
        }

        [Asynchronous]
        private void dummyUnsubscribeCallback(string result)
        {

        }

        [Asynchronous]
        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
            mreConnect.Set();
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
            //publishInCallback = true;
            mrePublish.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
