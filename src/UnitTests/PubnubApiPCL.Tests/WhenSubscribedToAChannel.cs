//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NUnit.Framework;
//using System.ComponentModel;
//using System.Threading;
//using System.Collections;
//using PubnubApi;

//namespace PubNubMessaging.Tests
//{
//    [TestFixture]
//    public class WhenSubscribedToAChannel : TestHarness
//    {
//        ManualResetEvent mreSubscribeConnect = new ManualResetEvent(false);
//        ManualResetEvent mrePublish = new ManualResetEvent(false);
//        ManualResetEvent mreUnsubscribe = new ManualResetEvent(false);
//        ManualResetEvent mreAlreadySubscribed = new ManualResetEvent(false);
//        ManualResetEvent mreChannel1SubscribeConnect = new ManualResetEvent(false);
//        ManualResetEvent mreChannel2SubscribeConnect = new ManualResetEvent(false);
//        ManualResetEvent mreSubscriberManyMessages = new ManualResetEvent(false);
//        ManualResetEvent mreGrant = new ManualResetEvent(false);
//        ManualResetEvent mreSubscribe = new ManualResetEvent(false);

//        bool receivedMessage = false;
//        bool receivedConnectMessage = false;
//        bool receivedAlreadySubscribedMessage = false;
//        bool receivedChannel1ConnectMessage = false;
//        bool receivedChannel2ConnectMessage = false;
//        bool receivedManyMessages = false;
//        bool receivedGrantMessage = false;

//        int numberOfReceivedMessages = 0;
//        int manualResetEventsWaitTimeout = 310 * 1000;
//        object publishedMessage = null;
//        bool isPublished = false;

//        Pubnub pubnub = null;

//        [TestFixtureSetUp]
//        public void Init()
//        {
//            if (!PubnubCommon.PAMEnabled) return;

//            receivedGrantMessage = false;

//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                SecretKey = PubnubCommon.SecretKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            string channelList = "hello_my_channel,hello_my_channel1,hello_my_channel2";
//            string[] channel = channelList.Split(',');

//            pubnub.Grant().Channels(channel).Read(true).Write(true).Manage(false).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenSubscribeInitializeShouldReturnGrantMessage, Error = DummyErrorCallback });
//            Thread.Sleep(1000);

//            mreGrant.WaitOne();

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;
//            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannel Grant access failed.");
//        }

//        [Test]
//        public void ThenComplexMessageSubscribeShouldReturnReceivedMessage()
//        {
//            receivedMessage = false;
//            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSubscribeShouldReturnReceivedMessage Failed");
//        }

//        private void CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
//        {
//            receivedMessage = false;
//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                SecretKey = secretKey,
//                CiperKey = cipherKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            string channel = "hello_my_channel";

//            mreSubscribe = new ManualResetEvent(false);

//            mreSubscribeConnect = new ManualResetEvent(false);
//            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = ReceivedMessageCallbackWhenSubscribed, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = UnsubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
//            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

//            mrePublish = new ManualResetEvent(false);
//            publishedMessage = new CustomClass();
//            pubnub.Publish().Channel(channel).Message(publishedMessage).Async(new PNCallback<PNPublishResult>() { Result = dummyPublishCallback, Error = DummyErrorCallback });
//            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
//            mrePublish.WaitOne(manualResetEventsWaitTimeout);

//            if (isPublished)
//            {
//                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

//                mreUnsubscribe = new ManualResetEvent(false);
//                pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
//                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout);
//            }
//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;

//        }

//        [Test]
//        public void ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage()
//        {
//            receivedMessage = false;
//            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage Failed");
//        }

//        [Test]
//        public void ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage()
//        {
//            receivedMessage = false;
//            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage Failed");
//        }

//        [Test]
//        public void ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage()
//        {
//            receivedMessage = false;
//            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage Failed");
//        }

//        [Test]
//        public void ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage()
//        {
//            receivedMessage = false;
//            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage Failed");
//        }

//        [Test]
//        public void ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage()
//        {
//            receivedMessage = false;
//            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage Failed");
//        }

//        [Test]
//        public void ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage()
//        {
//            receivedMessage = false;
//            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage Failed");
//        }

//        [Test]
//        public void ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage()
//        {
//            receivedMessage = false;
//            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage Failed");
//        }
        
//        [Test]
//        public void ThenSubscribeShouldReturnConnectStatus()
//        {
//            receivedConnectMessage = false;
//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            string channel = "hello_my_channel";

//            mreSubscribeConnect = new ManualResetEvent(false);
//            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = ReceivedMessageCallbackYesConnect, Connect = ConnectStatusCallback, Disconnect = ConnectStatusCallback, Error = DummyErrorCallback });
//            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
//            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;

//            Assert.IsTrue(receivedConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
//        }

//        [Test]
//        public void ThenMultiSubscribeShouldReturnConnectStatus()
//        {
//            receivedChannel1ConnectMessage = false;
//            receivedChannel2ConnectMessage = false;

//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

//            string channel1 = "hello_my_channel1";
//            mreChannel1SubscribeConnect = new ManualResetEvent(false);
//            pubnub.Subscribe<string>().Channels(new string[] { channel1 }).Execute(new SubscribeCallback<string>() { Message = ReceivedChannelUserCallback, Connect = ReceivedChannel1ConnectCallback, Disconnect = ReceivedChannel1ConnectCallback, Error = DummyErrorCallback });
//            mreChannel1SubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

//            string channel2 = "hello_my_channel2";
//            mreChannel2SubscribeConnect = new ManualResetEvent(false);
//            pubnub.Subscribe<string>().Channels(new string[] { channel2 }).Execute(new SubscribeCallback<string>() { Message = ReceivedChannelUserCallback, Connect = ReceivedChannel2ConnectCallback, Disconnect = ReceivedChannel1ConnectCallback, Error = DummyErrorCallback });
//            mreChannel2SubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;

//            Assert.IsTrue(receivedChannel1ConnectMessage && receivedChannel2ConnectMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
//        }

//        [Test]
//        public void ThenMultiSubscribeShouldReturnConnectStatusSSL()
//        {
//            receivedChannel1ConnectMessage = false;
//            receivedChannel2ConnectMessage = false;

//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

//            string channel1 = "hello_my_channel1";
//            mreChannel1SubscribeConnect = new ManualResetEvent(false);
//            pubnub.Subscribe<string>().Channels(new string[] { channel1 }).Execute(new SubscribeCallback<string>() { Message = ReceivedChannelUserCallback, Connect = ReceivedChannel1ConnectCallback, Disconnect = ReceivedChannel1ConnectCallback, Error = DummyErrorCallback });
//            mreChannel1SubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

//            string channel2 = "hello_my_channel2";
//            mreChannel2SubscribeConnect = new ManualResetEvent(false);
//            pubnub.Subscribe<string>().Channels(new string[] { channel2 }).Execute(new SubscribeCallback<string>() { Message = ReceivedChannelUserCallback, Connect = ReceivedChannel2ConnectCallback, Disconnect = ReceivedChannel1ConnectCallback, Error = DummyErrorCallback });
//            mreChannel2SubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;

//            Assert.IsTrue(receivedChannel1ConnectMessage && receivedChannel2ConnectMessage, "WhenSubscribedToAChannel --> ThenMultiSubscribeShouldReturnConnectStatusSSL Failed");
//        }

//        [Test]
//        public void ThenDuplicateChannelShouldReturnAlreadySubscribed()
//        {
//            receivedAlreadySubscribedMessage = false;

//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

//            string channel = "hello_my_channel";

//            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodDuplicateChannelUserCallback1, Connect = DummyMethodDuplicateChannelConnectCallback, Disconnect = DummyMethodDuplicateChannelDisconnectCallback, Error = DummyErrorCallback });
//            Thread.Sleep(100);

//            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodDuplicateChannelUserCallback2, Connect = DummyMethodDuplicateChannelConnectCallback, Disconnect = DummyMethodDuplicateChannelDisconnectCallback, Error = DuplicateChannelErrorCallback });
//            mreAlreadySubscribed.WaitOne(manualResetEventsWaitTimeout);

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;

//            Assert.IsTrue(receivedAlreadySubscribedMessage, "WhenSubscribedToAChannel --> ThenDuplicateChannelShouldReturnAlreadySubscribed Failed");
//        }

//        [Test]
//        public void ThenSubscriberShouldBeAbleToReceiveManyMessages()
//        {
//            receivedManyMessages = false;
//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            string channel = "hello_my_channel";
            
//            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
//            //Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Iniatiating Subscribe");
//            mreSubscribe = new ManualResetEvent(false);
//            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = SubscriberDummyMethodForManyMessagesUserCallback, Connect = SubscribeDummyMethodForManyMessagesConnectCallback, Disconnect = SubscribeDummyMethodForManyMessagesDisconnectCallback, Error = DummyErrorCallback });
//            Thread.Sleep(1000);
            
//            mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

//            if (!PubnubCommon.EnableStubTest)
//            {
//                for (int index = 0; index < 10; index++)
//                {
//                    //Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Publishing " + index.ToString());
//                    pubnub.Publish().Channel(channel).Message(index.ToString()).Async(new PNCallback<PNPublishResult>() { Result = dummyPublishCallback, Error = DummyErrorCallback });
//                    //Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Publishing..waiting for confirmation " + index.ToString());
//                    //mePublish.WaitOne(10*1000);
//                }
//            }
//            mreSubscriberManyMessages.WaitOne(manualResetEventsWaitTimeout);
//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;


//            Assert.IsTrue(receivedManyMessages, "WhenSubscribedToAChannel --> ThenSubscriberShouldBeAbleToReceiveManyMessages Failed");
//        }

//        void ThenSubscribeInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
//        {
//            try
//            {
//                if (receivedMessage != null)
//                {
//                    var status = receivedMessage.StatusCode;
//                    if (status == 200)
//                    {
//                        receivedGrantMessage = true;
//                    }
//                }
//            }
//            catch { }
//            finally
//            {
//                mreGrant.Set();
//            }
//        }

//        private void SubscriberDummyMethodForManyMessagesUserCallback(PNMessageResult<string> result)
//        {
//            //Console.WriteLine("WhenSubscribedToChannel -> \n ThenSubscriberShouldBeAbleToReceiveManyMessages -> \n SubscriberDummyMethodForManyMessagesUserCallback -> result = " + result);
//            numberOfReceivedMessages = numberOfReceivedMessages + 1;
//            if (numberOfReceivedMessages >= 10)
//            {
//                receivedManyMessages = true;
//                mreSubscriberManyMessages.Set();
//            }
            
//        }

//        private void SubscribeDummyMethodForManyMessagesConnectCallback(ConnectOrDisconnectAck result)
//        {
//            //Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Subscribe Connected");
//            mreSubscribe.Set();
//        }

//        private void SubscribeDummyMethodForManyMessagesDisconnectCallback(ConnectOrDisconnectAck result)
//        {
//        }

//        private void ReceivedChannelUserCallback(PNMessageResult<string> result)
//        {
//        }

//        private void ReceivedChannel1ConnectCallback(ConnectOrDisconnectAck result)
//        {
//            if (result != null)
//            {
//                long statusCode = result.StatusCode;
//                string statusMessage = result.StatusMessage;
//                if (statusCode == 1 && statusMessage.ToLower() == "connected")
//                {
//                    receivedChannel1ConnectMessage = true;
//                }
//            }
//            mreChannel1SubscribeConnect.Set();
//        }

//        private void ReceivedChannel2ConnectCallback(ConnectOrDisconnectAck result)
//        {
//            if (result != null)
//            {
//                long statusCode = result.StatusCode;
//                string statusMessage = result.StatusMessage;
//                if (statusCode == 1 && statusMessage.ToLower() == "connected")
//                {
//                    receivedChannel2ConnectMessage = true;
//                }
//            }
//            mreChannel2SubscribeConnect.Set();
//        }

//        private void DummyMethodDuplicateChannelUserCallback1(PNMessageResult<string> result)
//        {
//        }

//        private void DummyMethodDuplicateChannelUserCallback2(PNMessageResult<string> result)
//        {
//        }

//        private void DummyMethodDuplicateChannelConnectCallback(ConnectOrDisconnectAck result)
//        {
//        }

//        private void DummyMethodDuplicateChannelDisconnectCallback(ConnectOrDisconnectAck result)
//        {
//        }

//        private void ReceivedMessageCallbackWhenSubscribed(PNMessageResult<string> result)
//        {
//            if (result != null && result.Data != null)
//            {
//                string serializedPublishMesage = pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage);
//                if (result.Data == serializedPublishMesage)
//                {
//                    receivedMessage = true;
//                }
//            }
//            mreSubscribe.Set(); 
//        }

//        private void ReceivedMessageCallbackYesConnect(PNMessageResult<string> result)
//        {
//            //dummy method provided as part of subscribe connect status check.
//        }

//        private void ConnectStatusCallback(ConnectOrDisconnectAck result)
//        {
//            if (result != null)
//            {
//                long statusCode = result.StatusCode;
//                string statusMessage = result.StatusMessage;
//                if (statusCode == 1 && statusMessage.ToLower() == "connected")
//                {
//                    receivedConnectMessage = true;
//                }
//            }
//            mreSubscribeConnect.Set();
//        }

//        private void dummyPublishCallback(PNPublishResult result)
//        {
//            //Console.WriteLine("dummyPublishCallback -> result = " + result);
//            if (result != null)
//            {
//                long statusCode = result.StatusCode;
//                string statusMessage = result.StatusMessage;
//                if (statusCode == 1 && statusMessage.ToLower() == "sent")
//                {
//                    isPublished = true;
//                }
//            }

//            mrePublish.Set();
//        }

//        private void DummyErrorCallback(PubnubClientError result)
//        {
//            if (result != null)
//            {
//                Console.WriteLine("DummyErrorCallback result = " + result.Message);
//            }
//        }

//        private void DuplicateChannelErrorCallback(PubnubClientError result)
//        {
//            if (result != null && result.Message.ToLower().Contains("already subscribed"))
//            {
//                receivedAlreadySubscribedMessage = true;
//            }
//            mreAlreadySubscribed.Set();
//        }

//        private void dummyUnsubscribeCallback(string result)
//        {
            
//        }

//        void SubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
//        {
//            mreSubscribeConnect.Set();
//        }

//        void UnsubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
//        {
//            mreUnsubscribe.Set();
//        }

//    }
//}
