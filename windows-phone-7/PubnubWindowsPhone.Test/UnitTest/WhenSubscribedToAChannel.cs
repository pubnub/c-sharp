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
        ManualResetEvent mreSubscribe = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent meUnsubscribe = new ManualResetEvent(false);
        ManualResetEvent meAlreadySubscribed = new ManualResetEvent(false);
        ManualResetEvent mreConnect = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);

        bool receivedMessage = false;
        bool receivedConnectMessage = false;
        bool receivedAlreadySubscribedMessage = false;
        bool receivedChannel1ConnectMessage = false;
        bool receivedChannel2ConnectMessage = false;
        bool receivedManyMessages = false;
        bool receivedGrantMessage = false;

        int numberOfReceivedMessages = 0;

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                TestComplete();
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

                    mreGrant = new ManualResetEvent(false);
                    pubnub.GrantAccess<string>(channel, true, true, 20, ThenSubscriberInitializeShouldReturnGrantMessage, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000); //1 minute

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed.");
                            TestComplete();
                        });
                });
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
                mreGrant.Set();
            }
        }
        
        [TestMethod, Asynchronous]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            mreConnect = new ManualResetEvent(false);
            mrePublish = new ManualResetEvent(false);
            mreSubscribe = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenSubscribedToAChannel";
                    unitTest.TestCaseName = "ThenSubscribeShouldReturnReceivedMessage";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Subscribe<string>(channel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
                    mreConnect.WaitOne(310 * 1000);
                    
                    pubnub.Publish<string>(channel, "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage", dummyPublishCallback, DummyErrorCallback);
                    mrePublish.WaitOne(310 * 1000);
                    
                    mreSubscribe.WaitOne(310 * 1000);
                    //pubnub.Unsubscribe<string>(channel, dummyUnsubscribeCallback, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                    //Thread.Sleep(500);
                    //meUnsubscribe.WaitOne(60 * 1000);

                    //Thread.Sleep(1000);
                    pubnub.EndPendingRequests();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenItShouldReturnReceivedMessage Failed");
                            TestComplete();
                        });
                });
        }

        [TestMethod, Asynchronous]
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            receivedConnectMessage = false;
            mreConnect = new ManualResetEvent(false);
            mreSubscribe = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenSubscribedToAChannel";
                    unitTest.TestCaseName = "ThenSubscribeShouldReturnConnectStatus";

                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";

                    pubnub.Subscribe<string>(channel, ReceivedMessageCallbackYesConnect, ConnectStatusCallback, DummyErrorCallback);
                    mreConnect.WaitOne(310 * 1000);

                    pubnub.EndPendingRequests();
                    
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
                            TestComplete();
                        });
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
                    pubnub.Subscribe<string>(channel1, ReceivedChannelUserCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback);
                    mreConnect.WaitOne(310 * 1000);

                    mreConnect = new ManualResetEvent(false);
                    string channel2 = "hello_my_channel2";
                    pubnub.Subscribe<string>(channel2, ReceivedChannelUserCallback, ReceivedChannel2ConnectCallback, DummyErrorCallback);
                    mreConnect.WaitOne(310 * 1000);

                    pubnub.EndPendingRequests();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedChannel1ConnectMessage && receivedChannel2ConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
                            TestComplete();
                        });
                });
        }

        [TestMethod, Asynchronous]
        public void ThenDuplicateChannelShouldReturnAlreadySubscribed()
        {
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

                    pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback1, DummyMethodDuplicateChannelConnectCallback, DummyErrorCallback);
                    mreConnect.WaitOne(310 * 1000);

                    mreConnect = new ManualResetEvent(false);
                    pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback2, DummyMethodDuplicateChannelConnectCallback, DuplicateChannelErrorCallback);
                    mreConnect.WaitOne(310 * 1000);

                    pubnub.EndPendingRequests();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedAlreadySubscribedMessage, "WhenSubscribedToAChannel --> ThenDuplicateChannelShouldReturnAlreadySubscribed Failed");
                            TestComplete();
                        });
                });
        }

        [TestMethod, Asynchronous]
        public void ThenSubscriberShouldBeAbleToReceiveManyMessages()
        {
            receivedManyMessages = false;
            mreSubscribe = new ManualResetEvent(false);
            mrePublish = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenSubscribedToAChannel";
                    unitTest.TestCaseName = "ThenSubscriberShouldBeAbleToReceiveManyMessages";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";

                    pubnub.Subscribe<string>(channel, SubscriberDummyMethodForManyMessagesUserCallback, SubscribeDummyMethodForManyMessagesConnectCallback, DummyErrorCallback);
                    if (!PubnubCommon.EnableStubTest)
                    {
                        for (int index = 0; index < 10; index++)
                        {
                            mrePublish = new ManualResetEvent(false);
                            pubnub.Publish<string>(channel, index, SubscribeManyMessagesPublishCallback, DummyErrorCallback);
                            mrePublish.WaitOne(310 * 1000);
                        }
                    }

                    mreSubscribe.WaitOne(310 * 1000);

                    pubnub.EndPendingRequests();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedManyMessages, "WhenSubscribedToAChannel --> ThenSubscriberShouldBeAbleToReceiveManyMessages Failed");
                            TestComplete();
                        });
                });
        }

        [Asynchronous]
        private void SubscriberDummyMethodForManyMessagesUserCallback(string result)
        {
            numberOfReceivedMessages = numberOfReceivedMessages + 1;
            if (numberOfReceivedMessages >= 10)
            {
                receivedManyMessages = true;
                mreSubscribe.Set();
            }
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
        }

        [Asynchronous]
        private void DuplicateChannelErrorCallback(PubnubClientError result)
        {
            if (result != null && result.Message.ToLower().Contains("already subscribed"))
            {
                receivedAlreadySubscribedMessage = true;
            }
            mreConnect.Set();
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelConnectCallback(string result)
        {
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
                        receivedMessage = true;
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
        private void dummyPublishCallback(string result)
        {
            mrePublish.Set();
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
            meUnsubscribe.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
