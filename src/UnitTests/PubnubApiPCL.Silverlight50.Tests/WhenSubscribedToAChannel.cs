using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubnubApi;
using System;
using System.Threading;
using System.Windows;

namespace PubnubApiPCL.Silverlight50.Tests
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
        string publishedMessage = "";
        Pubnub pubnub = null;

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
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "GrantRequestUnitTest";
                unitTest.TestCaseName = "Init3";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel,hello_my_channel1,hello_my_channel2";

                mreGrant = new ManualResetEvent(false);

                pubnub.GrantAccess(channel, true, true, 20, ThenSubscriberInitializeShouldReturnGrantMessage, DummyErrorCallback);

                mreGrant.WaitOne(60 * 1000); //1 minute

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed.");
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
                    TestComplete();
                });
            });
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
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                string channel = "hello_my_channel";

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenSubscribedToAChannel";
                unitTest.TestCaseName = "ThenSubscribeShouldReturnReceivedMessage";
                pubnub.PubnubUnitTest = unitTest;

                pubnub.Subscribe<string>(channel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                mreConnect.WaitOne(310 * 1000);
                Thread.Sleep(PubnubCommon.TIMEOUT);

                publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";
                pubnub.Publish(channel, publishedMessage, dummyPublishCallback, DummyErrorCallback);
                mrePublish.WaitOne(310 * 1000);

                mreSubscribe.WaitOne(310 * 1000);
                pubnub.EndPendingRequests();
                Thread.Sleep(PubnubCommon.TIMEOUT);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenItShouldReturnReceivedMessage Failed");
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
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
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenSubscribedToAChannel";
                unitTest.TestCaseName = "ThenSubscribeShouldReturnConnectStatus";

                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";
                pubnub.Subscribe<string>(channel, ReceivedMessageCallbackYesConnect, ConnectStatusCallback, ConnectStatusCallback, DummyErrorCallback);
                mreConnect.WaitOne(310 * 1000);
                pubnub.EndPendingRequests();
                Thread.Sleep(PubnubCommon.TIMEOUT);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
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
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenSubscribedToAChannel";
                unitTest.TestCaseName = "ThenMultiSubscribeShouldReturnConnectStatus";

                pubnub.PubnubUnitTest = unitTest;


                string channel1 = "hello_my_channel1";
                pubnub.Subscribe<string>(channel1, ReceivedChannelUserCallback, ReceivedChannel1ConnectCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback);
                mreConnect.WaitOne(310 * 1000);
                Thread.Sleep(PubnubCommon.TIMEOUT);
                mreConnect = new ManualResetEvent(false);
                string channel2 = "hello_my_channel2";
                pubnub.Subscribe<string>(channel2, ReceivedChannelUserCallback, ReceivedChannel2ConnectCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback);
                mreConnect.WaitOne(310 * 1000);
                pubnub.EndPendingRequests();
                Thread.Sleep(PubnubCommon.TIMEOUT);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedChannel1ConnectMessage && receivedChannel2ConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
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
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenSubscribedToAChannel";
                unitTest.TestCaseName = "ThenDuplicateChannelShouldReturnAlreadySubscribed";

                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";

                pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback1, DummyMethodDuplicateChannelConnectCallback, DummyMethodDuplicateChannelDisconnectCallback, DummyErrorCallback);
                mreConnect.WaitOne(310 * 1000);
                Thread.Sleep(PubnubCommon.TIMEOUT);
                mreConnect = new ManualResetEvent(false);
                pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback2, DummyMethodDuplicateChannelConnectCallback, DummyMethodDuplicateChannelDisconnectCallback, DuplicateChannelErrorCallback);
                mreConnect.WaitOne(310 * 1000);
                pubnub.EndPendingRequests();
                Thread.Sleep(PubnubCommon.TIMEOUT);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedAlreadySubscribedMessage, "WhenSubscribedToAChannel --> ThenDuplicateChannelShouldReturnAlreadySubscribed Failed");
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
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
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenSubscribedToAChannel";
                unitTest.TestCaseName = "ThenSubscriberShouldBeAbleToReceiveManyMessages";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";

                pubnub.Subscribe<string>(channel, SubscriberDummyMethodForManyMessagesUserCallback, SubscribeDummyMethodForManyMessagesConnectCallback, SubscribeDummyMethodForManyMessagesDisconnectCallback, DummyErrorCallback);
                Thread.Sleep(PubnubCommon.TIMEOUT);

                if (!PubnubCommon.EnableStubTest)
                {
                    for (int index = 0; index < 10; index++)
                    {
                        mrePublish = new ManualResetEvent(false);
                        pubnub.Publish(channel, "Message:" + index.ToString(), SubscribeManyMessagesPublishCallback, DummyErrorCallback);
                        mrePublish.WaitOne(310 * 1000);
                    }
                }
                mreSubscribe.WaitOne(310 * 1000);
                pubnub.EndPendingRequests();
                Thread.Sleep(PubnubCommon.TIMEOUT);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedManyMessages, "WhenSubscribedToAChannel --> ThenSubscriberShouldBeAbleToReceiveManyMessages Failed");
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
                    TestComplete();
                });
            });
        }

        [Asynchronous]
        void ThenSubscriberInitializeShouldReturnGrantMessage(GrantAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null && receivedMessage.StatusCode == 200)
                {
                    receivedGrantMessage = true;
                }
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }


        [Asynchronous]
        private void SubscriberDummyMethodForManyMessagesUserCallback(Message<string> result)
        {
            numberOfReceivedMessages = numberOfReceivedMessages + 1;
            if (numberOfReceivedMessages >= 10)
            {
                receivedManyMessages = true;
                mreSubscribe.Set();
            }
        }

        [Asynchronous]
        private void SubscribeDummyMethodForManyMessagesConnectCallback(ConnectOrDisconnectAck result)
        {
            Console.WriteLine(result.ChannelName);
        }

        [Asynchronous]
        private void SubscribeDummyMethodForManyMessagesDisconnectCallback(ConnectOrDisconnectAck result)
        {
            Console.WriteLine(result.ChannelName);
        }

        [Asynchronous]
        public void SubscribeManyMessagesPublishCallback(PublishAck result)
        {
            if (result != null)
            {
                if (result.StatusCode == 1 && result.StatusMessage.ToLower() == "sent")
                {
                    //good
                }
            }
            mrePublish.Set();
        }

        [Asynchronous]
        private void ReceivedChannelUserCallback(Message<string> result)
        {
        }

        [Asynchronous]
        private void ReceivedChannel1ConnectCallback(ConnectOrDisconnectAck result)
        {
            if (result != null)
            {
                long statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "connected")
                {
                    receivedChannel1ConnectMessage = true;
                }
            }
            mreConnect.Set();
        }

        [Asynchronous]
        private void ReceivedChannel2ConnectCallback(ConnectOrDisconnectAck result)
        {
            if (result != null)
            {
                long statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "connected")
                {
                    receivedChannel2ConnectMessage = true;
                }
            }
            mreConnect.Set();
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelUserCallback1(Message<string> result)
        {
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelUserCallback2(Message<string> result)
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
        private void DummyMethodDuplicateChannelConnectCallback(ConnectOrDisconnectAck result)
        {
            mreConnect.Set();
        }

        [Asynchronous]
        private void DummyMethodDuplicateChannelDisconnectCallback(ConnectOrDisconnectAck result)
        {
        }

        [Asynchronous]
        private void ReceivedMessageCallbackWhenSubscribed(Message<string> result)
        {
            if (result != null && result.Data != null)
            {
                if (result.Data == publishedMessage)
                {
                    receivedMessage = true;
                }
            }
            mreSubscribe.Set();
        }


        [Asynchronous]
        private void ReceivedMessageCallbackYesConnect(Message<string> result)
        {
            //dummy method provided as part of subscribe connect status check.
        }

        [Asynchronous]
        private void ConnectStatusCallback(ConnectOrDisconnectAck result)
        {
            if (result != null)
            {
                long statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "connected")
                {
                    receivedConnectMessage = true;
                }
            }
            mreConnect.Set();
        }

        [Asynchronous]
        private void dummyPublishCallback(PublishAck result)
        {
            mrePublish.Set();
        }

        [Asynchronous]
        private void dummyUnsubscribeCallback(string result)
        {

        }

        [Asynchronous]
        void SubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreConnect.Set();
        }

        [Asynchronous]
        void UnsubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            meUnsubscribe.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
            Console.WriteLine(result.Description);
        }
    }
}
