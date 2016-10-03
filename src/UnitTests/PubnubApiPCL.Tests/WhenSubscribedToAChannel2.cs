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
//    public class WhenSubscribedToAChannel2 : TestHarness
//    {
//        ManualResetEvent mreSubscribeConnect = new ManualResetEvent(false);
//        ManualResetEvent mrePublish = new ManualResetEvent(false);
//        ManualResetEvent mreUnsubscribe = new ManualResetEvent(false);
//        ManualResetEvent mreGrant = new ManualResetEvent(false);
//        ManualResetEvent mreSubscribe = new ManualResetEvent(false);

//        bool receivedMessage = false;
//        bool receivedGrantMessage = false;

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

//            string channel = "hello_my_channel,hello_my_channel1,hello_my_channel2";

//            pubnub.Grant().Channels(new string[] { channel }).Read(true).Write(true).Manage(false).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenSubscribeInitializeShouldReturnGrantMessage, Error = DummyErrorCallback });
//            Thread.Sleep(1000);

//            mreGrant.WaitOne();

//            pubnub.EndPendingRequests(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;
//            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannel Grant access failed.");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnReceivedMessage()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenItShouldReturnReceivedMessage Failed");
//        }

//        private void CommonSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
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
//            publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";
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
//            pubnub.EndPendingRequests(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;

//        }

//        [Test]
//        public void ThenSubscribeShouldReturnReceivedMessageSSL()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSSL Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnReceivedMessageCipherSSL()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageCipherSSL Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnReceivedMessageSecret()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecret Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnReceivedMessageSecretSSL()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecretSSL Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnReceivedMessageSecretCipher()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipher Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnReceivedMessageCipher()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageCipher Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnEmojiMessage()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessage Failed");
//        }

//        private void CommonSubscribeShouldReturnEmojiMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
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
//            publishedMessage = "Text with 😜 emoji 🎉.";
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
//            pubnub.EndPendingRequests(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;

//        }

//        [Test]
//        public void ThenSubscribeShouldReturnEmojiMessageSSL()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageSSL Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnEmojiMessageSecret()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageSecret Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnEmojiMessageCipherSecret()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecret Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL Failed");
//        }

//        [Test]
//        public void ThenSubscribeShouldReturnEmojiMessageSecretSSL()
//        {
//            receivedMessage = false;
//            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
//            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageSecretSSL Failed");
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

//        private void ReceivedMessageCallbackWhenSubscribed(PNMessageResult<string> result)
//        {
//            if (result != null && result.Data != null)
//            {
//                string serializedResultMessage = pubnub.JsonPluggableLibrary.SerializeToJsonString(result.Data);
//                string serializedPublishMesage = pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage);
//                if (serializedResultMessage == serializedPublishMesage)
//                {
//                    receivedMessage = true;
//                }
//            }
//            mreSubscribe.Set();
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
