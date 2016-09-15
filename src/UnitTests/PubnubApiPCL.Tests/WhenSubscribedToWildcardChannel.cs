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
    public class WhenSubscribedToWildcardChannel : TestHarness
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string channel = "foo.*";
            mreGrant = new ManualResetEvent(false);
            pubnub.Grant().Channels(new string[] { channel }).Read(true).Write(true).Manage(false).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenSubscribeInitializeShouldReturnGrantMessage, Error = DummyErrorCallback });
            Thread.Sleep(1000);
            mreGrant.WaitOne();

            channel = "foo.bar";
            mreGrant = new ManualResetEvent(false);
            pubnub.Grant().Channels(new string[] { channel }).Read(true).Write(true).Manage(false).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenSubscribeInitializeShouldReturnGrantMessage, Error = DummyErrorCallback });
            Thread.Sleep(1000);
            mreGrant.WaitOne();

            if (receivedGrantMessage)
            {
                channel = "hello_my_channel";
                mreGrant = new ManualResetEvent(false);
                pubnub.Grant().Channels(new string[] { channel }).Read(true).Write(true).Manage(false).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenSubscribeInitializeShouldReturnGrantMessage, Error = DummyErrorCallback });
                Thread.Sleep(1000);
                mreGrant.WaitOne();
            }

            if (receivedGrantMessage)
            {
                channel = "hello_my_channel1";
                mreGrant = new ManualResetEvent(false);
                pubnub.Grant().Channels(new string[] { channel }).Read(true).Write(true).Manage(false).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenSubscribeInitializeShouldReturnGrantMessage, Error = DummyErrorCallback });
                Thread.Sleep(1000);
                mreGrant.WaitOne();
            }

            if (receivedMessage)
            {
                channelGroupName = "hello_my_group";
                mreGrant = new ManualResetEvent(false);
                pubnub.Grant().ChannelGroups(new string[] { channelGroupName }).Read(true).Write(true).Manage(true).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenSubscribeInitializeShouldReturnGrantMessage, Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = secretKey,
                CiperKey = cipherKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute(new SubscribeCallback<string>() { Message = ReceivedMessageCallbackWhenSubscribed, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = UnsubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";
            pubnub.Publish().Channel(publishChannel).Message(publishedMessage).Async(new PNCallback<PNPublishResult>() { Result = dummyPublishCallback, Error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = secretKey,
                CiperKey = cipherKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute(new SubscribeCallback<string>() { Message = ReceivedMessageCallbackWhenSubscribed, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = UnsubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Text with 😜 emoji 🎉.";
            pubnub.Publish().Channel(publishChannel).Message(publishedMessage).Async(new PNCallback<PNPublishResult>() { Result = dummyPublishCallback, Error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            pubnub.SessionUUID = "myuuid";

            string wildCardSubscribeChannel = "foo.*";
            string subChannelName = "hello_my_channel";
            string[] commaDelimitedChannel = new string[] { subChannelName, wildCardSubscribeChannel };
            channelGroupName = "hello_my_group";
            string channelAddForGroup = "hello_my_channel1";
            string pubWildChannelName = "foo.a";

            mreSubscribe = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup().Channels(new string[] { channelAddForGroup }).ChannelGroup(channelGroupName).Async(new PNCallback<PNChannelGroupsAddChannelResult>() { Result = ChannelGroupAddCallback, Error = DummyErrorCallback });
            mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(commaDelimitedChannel).ChannelGroups(new string[] { channelGroupName }).Execute(new SubscribeCallback<string>() { Message = ReceivedMessageCallbackWhenSubscribed, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Presence = null, Error = DummyErrorCallback });
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Test for cg";
            pubnub.Publish().Channel(channelAddForGroup).Message(publishedMessage).Async(new PNCallback<PNPublishResult>() { Result = dummyPublishCallback, Error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (isPublished)
            {
                Thread.Sleep(1000);
                mrePublish = new ManualResetEvent(false);
                publishedMessage = "Test for wc";
                pubnub.Publish().Channel(pubWildChannelName).Message(publishedMessage).Async(new PNCallback<PNPublishResult>() { Result = dummyPublishCallback, Error = DummyErrorCallback });
                manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
            }

            if (isPublished)
            {
                Thread.Sleep(1000);
                mrePublish = new ManualResetEvent(false);
                publishedMessage = "Test for normal ch";
                pubnub.Publish().Channel(subChannelName).Message(publishedMessage).Async(new PNCallback<PNPublishResult>() { Result = dummyPublishCallback, Error = DummyErrorCallback });
                manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
            }

            endOfPublish = true;
            if (isPublished)
            {
                Thread.Sleep(1000);
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>().Channels(commaDelimitedChannel).ChannelGroups(new string[] { channelGroupName }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            pubnub.SessionUUID = "myuuid";

            string wildCardSubscribeChannel = "foo.*";

            mreSubscribe = new ManualResetEvent(false);
            mreSubscribeConnect = new ManualResetEvent(false);

            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).ChannelGroups(new string[] { channelGroupName }).Execute(new SubscribeCallback<string>() { Message = ReceivedMessageCallbackWhenSubscribed, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Presence = ReceivedWildcardPresenceCallbackWhenSubscribed, Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            pubnub.SessionUUID = "myuuid";

            string wildCardSubscribeChannel = "foo.*";

            mreSubscribe = new ManualResetEvent(false);
            mreSubscribeConnect = new ManualResetEvent(false);

            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).ChannelGroups(new string[] { channelGroupName }).Execute(new SubscribeCallback<string>() { Message = ReceivedMessageCallbackWhenSubscribed, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Presence = null, Error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 10 * 1000;
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout);

            mreSubscribe.WaitOne(manualResetEventsWaitTimeout);

            Assert.IsTrue(!receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldNotReturnWildCardPresenceEventWhenNoCallback Failed");
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

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

        private void ReceivedMessageCallbackWhenSubscribed(PNMessageResult<string> result)
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

        private void ReceivedWildcardPresenceCallbackWhenSubscribed(PNPresenceEventResult result)
        {
            Console.WriteLine(string.Format("ReceivedWildcardPresenceCallbackWhenSubscribed  = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
            if (result != null)
            {
                receivedMessage = true;
            }
            mreSubscribe.Set();
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

        void ChannelGroupAddCallback(PNChannelGroupsAddChannelResult receivedMessage)
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
