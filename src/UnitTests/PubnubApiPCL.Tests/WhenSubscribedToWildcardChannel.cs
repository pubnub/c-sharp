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
    public class WhenSubscribedToWildcardChannel
    {
        ManualResetEvent mreSubscribeConnect = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent mreUnsubscribe = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);
        ManualResetEvent mreSubscribe = new ManualResetEvent(false);

        bool receivedMessage = false;
        bool receivedGrantMessage = false;
        
        //3 channels for multiplex subscribe
        bool receivedChannelGroupMessage = false;
        bool receivedChannelMessage = false;
        bool receivedWildcardChannelMessage = false;

        int manualResetEventsWaitTimeout = 310 * 1000;
        object publishedMessage = null;
        bool isPublished = false;

        string channelGroupName = "";

        Pubnub pubnub = null;
        string currentTestCase = "";
        bool endOfPublish = false;

        [TestFixtureSetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;
            currentTestCase = "Init";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "foo.*";
            mreGrant = new ManualResetEvent(false);
            pubnub.GrantAccess(new string[] { channel }, null, true, true, false, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Thread.Sleep(1000);
            mreGrant.WaitOne();

            channel = "foo.bar";
            mreGrant = new ManualResetEvent(false);
            pubnub.GrantAccess(new string[] { channel }, null, true, true, false, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Thread.Sleep(1000);
            mreGrant.WaitOne();

            if (receivedGrantMessage)
            {
                channel = "hello_my_channel";
                mreGrant = new ManualResetEvent(false);
                pubnub.GrantAccess(new string[] { channel }, null, true, true, false, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
                Thread.Sleep(1000);
                mreGrant.WaitOne();
            }

            if (receivedGrantMessage)
            {
                channel = "hello_my_channel1";
                mreGrant = new ManualResetEvent(false);
                pubnub.GrantAccess(new string[] { channel }, null, true, true, false, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
                Thread.Sleep(1000);
                mreGrant.WaitOne();
            }

            if (receivedMessage)
            {
                channelGroupName = "hello_my_group";
                mreGrant = new ManualResetEvent(false);
                pubnub.GrantAccess(null, new string[] { channelGroupName }, true, true, true, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
                Thread.Sleep(1000);
                mreGrant.WaitOne();
            }
            pubnub.EndPendingRequests();
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToWildcardChannel Grant access failed.");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenItShouldReturnReceivedMessage Failed");
        }

        private void CommonSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = (string.IsNullOrEmpty(cipherKey)) ? "ThenSubscribeShouldReturnReceivedMessage" : "ThenSubscribeShouldReturnReceivedCipherMessage";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel:wildCardSubscribeChannel, subscribeCallback:ReceivedMessageCallbackWhenSubscribed, connectCallback:SubscribeDummyMethodForConnectCallback, disconnectCallback: UnsubscribeDummyMethodForDisconnectCallback, errorCallback:DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";
            pubnub.Publish(channel:publishChannel, message:publishedMessage, userCallback:dummyPublishCallback, errorCallback:DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(wildCardSubscribeChannel, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout);
            }
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageCipherSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecret()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretCipher()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageCipher()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessage()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessage Failed");
        }

        private void CommonSubscribeShouldReturnEmojiMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = (string.IsNullOrEmpty(cipherKey)) ? "ThenSubscribeShouldReturnReceivedEmojiMessage" : "ThenSubscribeShouldReturnReceivedCipherEmojiMessage";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(wildCardSubscribeChannel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Text with 😜 emoji 🎉.";
            pubnub.Publish(publishChannel, publishedMessage, dummyPublishCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(wildCardSubscribeChannel, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout);
            }
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSecret()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageCipherSecret()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSecretSSL Failed");
        }

        [Test]
        public void ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            currentTestCase = "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";
            string subChannelName = "hello_my_channel";
            string commaDelimitedChannel = string.Format("{0},{1}", subChannelName, wildCardSubscribeChannel);
            channelGroupName = "hello_my_group";
            string channelAddForGroup = "hello_my_channel1";
            string pubWildChannelName = "foo.a";

            mreSubscribe = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup(new string[] { channelAddForGroup }, channelGroupName, ChannelGroupAddCallback, DummyErrorCallback);
            mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel: commaDelimitedChannel, channelGroup: channelGroupName, subscribeCallback: ReceivedMessageCallbackWhenSubscribed, connectCallback: SubscribeDummyMethodForConnectCallback, disconnectCallback: SubscribeDummyMethodForDisconnectCallback, wildcardPresenceCallback: null, errorCallback: DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Test for cg";
            pubnub.Publish(channel: channelAddForGroup, message: publishedMessage, userCallback: dummyPublishCallback, errorCallback: DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                Thread.Sleep(1000);
                mrePublish = new ManualResetEvent(false);
                publishedMessage = "Test for wc";
                pubnub.Publish(channel: pubWildChannelName, message: publishedMessage, userCallback: dummyPublishCallback, errorCallback: DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
            }

            if (isPublished)
            {
                Thread.Sleep(1000);
                mrePublish = new ManualResetEvent(false);
                publishedMessage = "Test for normal ch";
                pubnub.Publish(channel: subChannelName, message: publishedMessage, userCallback: dummyPublishCallback, errorCallback: DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
            }

            endOfPublish = true;
            if (isPublished)
            {
                Thread.Sleep(1000);
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(commaDelimitedChannel, channelGroupName, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout);
            }
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage Failed");
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

        }

        [Test]
        public void ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback()
        {
            receivedMessage = false;
            currentTestCase = "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";

            mreSubscribe = new ManualResetEvent(false);
            mreSubscribeConnect = new ManualResetEvent(false);

            pubnub.Subscribe<string>(channel: wildCardSubscribeChannel, channelGroup: channelGroupName, subscribeCallback: ReceivedMessageCallbackWhenSubscribed, connectCallback: SubscribeDummyMethodForConnectCallback, disconnectCallback: SubscribeDummyMethodForDisconnectCallback, wildcardPresenceCallback: ReceivedWildcardPresenceCallbackWhenSubscribed, errorCallback: DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback Failed");
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

        }

        [Test]
        public void ThenSubscribeShouldNotReturnWildCardPresenceEventWhenNoCallback()
        {
            receivedMessage = false;
            currentTestCase = "ThenSubscribeShouldNotReturnWildCardPresenceEventWhenNoCallback";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";

            mreSubscribe = new ManualResetEvent(false);
            mreSubscribeConnect = new ManualResetEvent(false);

            pubnub.Subscribe<string>(channel: wildCardSubscribeChannel, channelGroup: channelGroupName, subscribeCallback: ReceivedMessageCallbackWhenSubscribed, connectCallback: SubscribeDummyMethodForConnectCallback,  disconnectCallback:SubscribeDummyMethodForDisconnectCallback, wildcardPresenceCallback: null, errorCallback: DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 10 * 1000;
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

            Assert.IsTrue(!receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldNotReturnWildCardPresenceEventWhenNoCallback Failed");
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

        }

        void ThenSubscribeInitializeShouldReturnGrantMessage(GrantAck receivedMessage)
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

        private void ReceivedMessageCallbackWhenSubscribed(Message<string> result)
        {
            if (result != null && result.Data != null)
            {
                Console.WriteLine("ReceivedMessageCallbackWhenSubscribed -> result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                if (result.Data.ToString() == publishedMessage.ToString())
                {
                    receivedMessage = true;
                }
            }
            if (currentTestCase == "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessageBased")
            {
                if (endOfPublish)
                {
                    mreSubscribe.Set();
                }
            }
            else
            {
                mreSubscribe.Set();
            }
        }

        private void ReceivedWildcardPresenceCallbackWhenSubscribed(PresenceAck result)
        {
            Console.WriteLine(string.Format("ReceivedWildcardPresenceCallbackWhenSubscribed  = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
            if (result != null)
            {
                receivedMessage = true;
            }
            mreSubscribe.Set();
        }

        private void dummyPublishCallback(PublishAck result)
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
            if (currentTestCase == "Init")
            {
                mreGrant.Set();
            }
        }

        void SubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreSubscribeConnect.Set();
        }

        void SubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreUnsubscribe.Set();
        }

        void UnsubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreUnsubscribe.Set();
        }

        void ChannelGroupAddCallback(AddChannelToChannelGroupAck receivedMessage)
        {
            Console.WriteLine(string.Format("ChannelGroupAddCallback = {0}", receivedMessage));
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string serviceType = receivedMessage.Service;
                    bool errorStatus = receivedMessage.Error;
                    string currentChannelGroup = receivedMessage.ChannelGroupName;// serializedMessage[1].ToString().Substring(1); //assuming no namespace for channel group
                    string statusMessage = receivedMessage.StatusMessage;

                    if (statusCode == 200 && statusMessage.ToLower() == "ok" && serviceType == "channel-registry" && !errorStatus)
                    {
                        if (currentChannelGroup == channelGroupName)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                mreSubscribe.Set();
            }

        }

    }
}
