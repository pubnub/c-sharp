using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubnubApi;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToAChannel
    {
        ManualResetEvent mreSubscribeConnect = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent mreUnsubscribe = new ManualResetEvent(false);
        ManualResetEvent mreAlreadySubscribed = new ManualResetEvent(false);
        ManualResetEvent mreChannel1SubscribeConnect = new ManualResetEvent(false);
        ManualResetEvent mreChannel2SubscribeConnect = new ManualResetEvent(false);
        ManualResetEvent mreSubscriberManyMessages = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);
        ManualResetEvent mreSubscribe = new ManualResetEvent(false);

        bool receivedMessage = false;
        bool receivedConnectMessage = false;
        bool receivedAlreadySubscribedMessage = false;
        bool receivedChannel1ConnectMessage = false;
        bool receivedChannel2ConnectMessage = false;
        bool receivedManyMessages = false;
        bool receivedGrantMessage = false;

        int numberOfReceivedMessages = 0;
        int manualResetEventsWaitTimeout = 310 * 1000;
        object publishedMessage = null;
        bool isPublished = false;

        Pubnub pubnub = null;

        [TestFixtureSetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channelList = "hello_my_channel,hello_my_channel1,hello_my_channel2";
            string[] channel = channelList.Split(',');

            pubnub.grant().channels(channel).read(true).write(true).manage(false).ttl(20).async(new PNCallback<PNAccessManagerGrantResult>() { result = ThenSubscribeInitializeShouldReturnGrantMessage, error = DummyErrorCallback });
            Thread.Sleep(1000);

            mreGrant.WaitOne();

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannel Grant access failed.");
        }

        [Test]
        public void ThenComplexMessageSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSubscribeShouldReturnReceivedMessage Failed");
        }

        private void CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = (string.IsNullOrEmpty(cipherKey)) ? "ThenSubscribeShouldReturnReceivedComplexMessage" : "ThenSubscribeShouldReturnReceivedCipherComplexMessage";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = new CustomClass();
            pubnub.publish().channel(channel).message(publishedMessage).async(new PNCallback<PNPublishResult>() { result = dummyPublishCallback, error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(channel, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout);
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }

        [Test]
        public void ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage Failed");
        }
        
        [Test]
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            receivedConnectMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnConnectStatus";

            pubnub.PubnubUnitTest = unitTest;


            string channel = "hello_my_channel";

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, ReceivedMessageCallbackYesConnect, ConnectStatusCallback, ConnectStatusCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [Test]
        public void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            receivedChannel1ConnectMessage = false;
            receivedChannel2ConnectMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenMultiSubscribeShouldReturnConnectStatus";

            pubnub.PubnubUnitTest = unitTest;
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            string channel1 = "hello_my_channel1";
            mreChannel1SubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel1, ReceivedChannelUserCallback, ReceivedChannel1ConnectCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback);
            mreChannel1SubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            string channel2 = "hello_my_channel2";
            mreChannel2SubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel2, ReceivedChannelUserCallback, ReceivedChannel2ConnectCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback);
            mreChannel2SubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedChannel1ConnectMessage && receivedChannel2ConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [Test]
        public void ThenMultiSubscribeShouldReturnConnectStatusSSL()
        {
            receivedChannel1ConnectMessage = false;
            receivedChannel2ConnectMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", true);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenMultiSubscribeShouldReturnConnectStatus";

            pubnub.PubnubUnitTest = unitTest;
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            string channel1 = "hello_my_channel1";
            mreChannel1SubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel1, ReceivedChannelUserCallback, ReceivedChannel1ConnectCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback);
            mreChannel1SubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            string channel2 = "hello_my_channel2";
            mreChannel2SubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel2, ReceivedChannelUserCallback, ReceivedChannel2ConnectCallback, ReceivedChannel1ConnectCallback, DummyErrorCallback);
            mreChannel2SubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedChannel1ConnectMessage && receivedChannel2ConnectMessage, "WhenSubscribedToAChannel --> ThenMultiSubscribeShouldReturnConnectStatusSSL Failed");
        }

        [Test]
        public void ThenDuplicateChannelShouldReturnAlreadySubscribed()
        {
            receivedAlreadySubscribedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenDuplicateChannelShouldReturnAlreadySubscribed";

            pubnub.PubnubUnitTest = unitTest;
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            string channel = "hello_my_channel";

            pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback1, DummyMethodDuplicateChannelConnectCallback, DummyMethodDuplicateChannelDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(100);

            pubnub.Subscribe<string>(channel, DummyMethodDuplicateChannelUserCallback2, DummyMethodDuplicateChannelConnectCallback, DummyMethodDuplicateChannelDisconnectCallback, DuplicateChannelErrorCallback);
            mreAlreadySubscribed.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedAlreadySubscribedMessage, "WhenSubscribedToAChannel --> ThenDuplicateChannelShouldReturnAlreadySubscribed Failed");
        }

        [Test]
        public void ThenSubscriberShouldBeAbleToReceiveManyMessages()
        {
            receivedManyMessages = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = "ThenSubscriberShouldBeAbleToReceiveManyMessages";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            //Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Iniatiating Subscribe");
            mreSubscribe = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, SubscriberDummyMethodForManyMessagesUserCallback, SubscribeDummyMethodForManyMessagesConnectCallback, SubscribeDummyMethodForManyMessagesDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            
            mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

            if (!unitTest.EnableStubTest)
            {
                for (int index = 0; index < 10; index++)
                {
                    //Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Publishing " + index.ToString());
                    pubnub.publish().channel(channel).message(index.ToString()).async(new PNCallback<PNPublishResult>() { result = dummyPublishCallback, error = DummyErrorCallback });
                    //Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Publishing..waiting for confirmation " + index.ToString());
                    //mePublish.WaitOne(10*1000);
                }
            }
            mreSubscriberManyMessages.WaitOne(manualResetEventsWaitTimeout);
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;


            Assert.IsTrue(receivedManyMessages, "WhenSubscribedToAChannel --> ThenSubscriberShouldBeAbleToReceiveManyMessages Failed");
        }

        void ThenSubscribeInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    var status = receivedMessage.StatusCode;
                    if (status == 200)
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

        private void SubscriberDummyMethodForManyMessagesUserCallback(Message<string> result)
        {
            //Console.WriteLine("WhenSubscribedToChannel -> \n ThenSubscriberShouldBeAbleToReceiveManyMessages -> \n SubscriberDummyMethodForManyMessagesUserCallback -> result = " + result);
            numberOfReceivedMessages = numberOfReceivedMessages + 1;
            if (numberOfReceivedMessages >= 10)
            {
                receivedManyMessages = true;
                mreSubscriberManyMessages.Set();
            }
            
        }

        private void SubscribeDummyMethodForManyMessagesConnectCallback(ConnectOrDisconnectAck result)
        {
            //Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Subscribe Connected");
            mreSubscribe.Set();
        }

        private void SubscribeDummyMethodForManyMessagesDisconnectCallback(ConnectOrDisconnectAck result)
        {
        }

        private void ReceivedChannelUserCallback(Message<string> result)
        {
        }

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
            mreChannel1SubscribeConnect.Set();
        }

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
            mreChannel2SubscribeConnect.Set();
        }

        private void DummyMethodDuplicateChannelUserCallback1(Message<string> result)
        {
        }

        private void DummyMethodDuplicateChannelUserCallback2(Message<string> result)
        {
        }

        private void DummyMethodDuplicateChannelConnectCallback(ConnectOrDisconnectAck result)
        {
        }

        private void DummyMethodDuplicateChannelDisconnectCallback(ConnectOrDisconnectAck result)
        {
        }

        private void ReceivedMessageCallbackWhenSubscribed(Message<string> result)
        {
            if (result != null && result.Data != null)
            {
                string serializedPublishMesage = pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage);
                if (result.Data == serializedPublishMesage)
                {
                    receivedMessage = true;
                }
            }
            mreSubscribe.Set(); 
        }

        private void ReceivedMessageCallbackYesConnect(Message<string> result)
        {
            //dummy method provided as part of subscribe connect status check.
        }

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
            mreSubscribeConnect.Set();
        }

        private void dummyPublishCallback(PNPublishResult result)
        {
            //Console.WriteLine("dummyPublishCallback -> result = " + result);
            if (result != null)
            {
                long statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    isPublished = true;
                }
            }

            mrePublish.Set();
        }

        private void DummyErrorCallback(PubnubClientError result)
        {
            if (result != null)
            {
                Console.WriteLine("DummyErrorCallback result = " + result.Message);
            }
        }

        private void DuplicateChannelErrorCallback(PubnubClientError result)
        {
            if (result != null && result.Message.ToLower().Contains("already subscribed"))
            {
                receivedAlreadySubscribedMessage = true;
            }
            mreAlreadySubscribed.Set();
        }

        private void dummyUnsubscribeCallback(string result)
        {
            
        }

        void SubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreSubscribeConnect.Set();
        }

        void UnsubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreUnsubscribe.Set();
        }

    }
}
