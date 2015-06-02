using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToAChannel3
    {
        ManualResetEvent mreSubscribeConnect = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent mreUnsubscribe = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);
        ManualResetEvent mreSubscribe = new ManualResetEvent(false);

        bool receivedMessage = false;
        bool receivedGrantMessage = false;

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

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel,hello_my_channel1,hello_my_channel2";

            pubnub.GrantAccess<string>(channel, true, true, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Thread.Sleep(1000);

            mreGrant.WaitOne();

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannel Grant access failed.");
        }

        [Test]
        public void ThenSubscribeShouldReturnUnicodeMessage()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnUnicodeMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnUnicodeMessage Failed");
        }

        private void CommonSubscribeShouldReturnUnicodeMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = (string.IsNullOrEmpty(cipherKey)) ? "ThenSubscribeShouldReturnUnicodeMessage" : "ThenSubscribeShouldReturnUnicodeCipherMessage";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Text with ÜÖ漢語";
            pubnub.Publish<string>(channel, publishedMessage, dummyPublishCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(channel, dummyUnsubscribeCallback, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout);
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenSubscribeShouldReturnUnicodeMessageSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnUnicodeMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnUnicodeMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnForwardSlashMessage()
        {
            receivedMessage = false;
            CommonSubscribeReturnForwardSlashMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessage Failed");
        }

        private void CommonSubscribeReturnForwardSlashMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = (string.IsNullOrEmpty(cipherKey)) ? "ThenSubscribeShouldReturnReceivedForwardSlashMessage" : "ThenSubscribeShouldReturnReceivedForwardSlashCipherMessage";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Text with /";
            pubnub.Publish<string>(channel, publishedMessage, dummyPublishCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(channel, dummyUnsubscribeCallback, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout);
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }

        [Test]
        public void ThenSubscribeShouldReturnForwardSlashMessageSSL()
        {
            receivedMessage = false;
            CommonSubscribeReturnForwardSlashMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnForwardSlashMessageCipher()
        {
            receivedMessage = false;
            CommonSubscribeReturnForwardSlashMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnForwardSlashMessageCipherSSL()
        {
            receivedMessage = false;
            CommonSubscribeReturnForwardSlashMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnForwardSlashMessageSecret()
        {
            receivedMessage = false;
            CommonSubscribeReturnForwardSlashMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnForwardSlashMessageCipherSecret()
        {
            receivedMessage = false;
            CommonSubscribeReturnForwardSlashMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageCipherSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnForwardSlashMessageCipherSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeReturnForwardSlashMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageCipherSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnForwardSlashMessageSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeReturnForwardSlashMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnSpecialCharMessage()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessage Failed");
        }

        private void CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannel";
            unitTest.TestCaseName = (string.IsNullOrEmpty(cipherKey)) ? "ThenSubscribeShouldReturnSpecialCharMessage" : "ThenSubscribeShouldReturnSpecialCharCipherMessage";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Text with '\"";
            pubnub.Publish<string>(channel, publishedMessage, dummyPublishCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(channel, dummyUnsubscribeCallback, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout);
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenSubscribeShouldReturnSpecialCharMessageSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnSpecialCharMessageCipher()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnSpecialCharMessageCipherSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnSpecialCharMessageSecret()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnSpecialCharMessageCipherSecret()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageCipherSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnSpecialCharMessageCipherSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageCipherSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnSpecialCharMessageSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageSecretSSL Failed");
        }

        void ThenSubscribeInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null)
                        {
                            var status = dictionary["status"].ToString();
                            if (status == "200")
                            {
                                receivedGrantMessage = true;
                            }
                        }

                    }
                }
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        private void ReceivedMessageCallbackWhenSubscribed(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                //Console.WriteLine("ReceivedMessageCallbackWhenSubscribed -> result = " + result);
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object subscribedObject = (object)deserializedMessage[0];
                    if (subscribedObject != null)
                    {
                        string serializedResultMessage = pubnub.JsonPluggableLibrary.SerializeToJsonString(subscribedObject);
                        string serializedPublishMesage = pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage);
                        if (serializedResultMessage == serializedPublishMesage)
                        {
                            receivedMessage = true;
                        }

                    }
                }
            }
            mreSubscribe.Set();
        }

        private void dummyPublishCallback(string result)
        {
            //Console.WriteLine("dummyPublishCallback -> result = " + result);
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isPublished = true;
                    }
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

        private void dummyUnsubscribeCallback(string result)
        {

        }

        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
            mreSubscribeConnect.Set();
        }

        void UnsubscribeDummyMethodForDisconnectCallback(string receivedMessage)
        {
            mreUnsubscribe.Set();
        }

    }
}
