using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToWildcardChannel : TestHarness
    {
        private static ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static object publishedMessage = null;
        private static long publishTimetoken = 0;
        private static bool receivedGrantMessage = false;

        private static string channelGroupName = "";

        int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string authKey = "myAuth";
        private static string currentTestCase = "";

        private static Pubnub pubnub = null;

        private Server server;
        private UnitTestLog unitLog;

        [TestFixtureSetUp]
        public void Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = new Server(new Uri("https://" + PubnubCommon.StubOrign));
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();

            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;
            currentTestCase = "Init";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            pubnub = this.createPubNubInstance(config);
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            channel = "foo.*";
            grantManualEvent = new ManualResetEvent(false);
            pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
            Thread.Sleep(1000);
            grantManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedGrantMessage)
            {
                receivedGrantMessage = false;

                channel = "foo.bar";
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedGrantMessage)
            {
                receivedGrantMessage = false;

                channel = "hello_my_channel";
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedGrantMessage)
            {
                receivedGrantMessage = false;

                channel = "hello_my_channel1";
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedGrantMessage)
            {
                receivedGrantMessage = false;

                channelGroupName = "hello_my_group";
                grantManualEvent = new ManualResetEvent(false);
                //pubnub.Grant().ChannelGroups(new string[] { channelGroupName, string.Format("{0}-pnpres", channelGroupName) }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
                pubnub.Grant().ChannelGroups(new string[] { channelGroupName }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToWildcardChannel Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessage";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenItShouldReturnReceivedMessage Failed");
        }

        private void CommonSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            Console.WriteLine("Running currentTestCase = " + currentTestCase);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = secretKey,
                CipherKey = cipherKey,
                Uuid = "mytestuuid",
                Secure = ssl
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            publishManualEvent = new ManualResetEvent(false);
            publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";
            pubnub.Publish().Channel(publishChannel).Message(publishedMessage).Async(new UTPublishResult());

            subscribeManualEvent = new ManualResetEvent(false);
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for message

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedMessage)
            {
                Thread.Sleep(1000);
                pubnub.Unsubscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute();
                Thread.Sleep(2000);
            }

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageCipherSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageCipherSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecret()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecret";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecretSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretCipher()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecretCipher";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageCipher()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageCipher";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessage()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessage";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessage Failed");
        }

        private void CommonSubscribeShouldReturnEmojiMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            Console.WriteLine("Running currentTestCase = " + currentTestCase);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = secretKey,
                CipherKey = cipherKey,
                Uuid = "mytestuuid",
                Secure = ssl
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            publishManualEvent = new ManualResetEvent(false);
            publishedMessage = "Text with 😜 emoji 🎉.";
            pubnub.Publish().Channel(publishChannel).Message(publishedMessage).Async(new UTPublishResult());

            subscribeManualEvent = new ManualResetEvent(false);
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for message
            Console.WriteLine("subscribeManualEvent.WaitOne DONE");

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);
            Console.WriteLine("publishManualEvent.WaitOne DONE");

            Thread.Sleep(1000);
            pubnub.Unsubscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageSSL";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSecret()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageSecret";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageCipherSecret()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageCipherSecret";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSecretSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageSecretSSL";
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
                AuthKey = authKey,
                Secure = false
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";
            string subChannelName = "hello_my_channel";
            string[] commaDelimitedChannel = new string[] { subChannelName, wildCardSubscribeChannel };
            channelGroupName = "hello_my_group";
            string channelAddForGroup = "hello_my_channel1";
            string pubWildChannelName = "foo.a";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            channelGroupManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup().Channels(new string[] { channelAddForGroup }).ChannelGroup(channelGroupName).Async(new ChannelGroupAddChannelResult());
            channelGroupManualEvent.WaitOne();

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(commaDelimitedChannel).ChannelGroups(new string[] { channelGroupName }).Execute();

            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            publishManualEvent = new ManualResetEvent(false);
            publishedMessage = "Test for cg";
            pubnub.Publish().Channel(channelAddForGroup).Message(publishedMessage).Async(new UTPublishResult());

            subscribeManualEvent = new ManualResetEvent(false);
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for message
            Console.WriteLine("subscribeManualEvent.WaitOne DONE");

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);
            Console.WriteLine("publishManualEvent.WaitOne DONE");


            if (receivedMessage)
            {
                receivedMessage = false;

                subscribeManualEvent = new ManualResetEvent(false);

                Thread.Sleep(1000);
                publishManualEvent = new ManualResetEvent(false);
                publishedMessage = "Test for wc";
                pubnub.Publish().Channel(pubWildChannelName).Message(publishedMessage).Async(new UTPublishResult());

                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for message
                Console.WriteLine("subscribeManualEvent.WaitOne DONE");

                publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                Console.WriteLine("publishManualEvent.WaitOne DONE");
            }

            if (receivedMessage)
            {
                receivedMessage = false;

                Thread.Sleep(1000);
                publishManualEvent = new ManualResetEvent(false);
                publishedMessage = "Test for normal ch";
                pubnub.Publish().Channel(subChannelName).Message(publishedMessage).Async(new UTPublishResult());

                subscribeManualEvent = new ManualResetEvent(false);
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for message
                Console.WriteLine("subscribeManualEvent.WaitOne DONE");

                publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                Console.WriteLine("publishManualEvent.WaitOne DONE");
            }

            Thread.Sleep(1000);
            pubnub.Unsubscribe<string>().Channels(commaDelimitedChannel).ChannelGroups(new string[] { channelGroupName }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage Failed");
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
                AuthKey = authKey,
                Secure = false
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).ChannelGroups(new string[] { channelGroupName }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(1000); 
            pubnub.Unsubscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).ChannelGroups(new string[] { channelGroupName }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback Failed");
        }

        private class UTGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    if (result != null && status.StatusCode == 200 && !status.Error)
                    {
                        Console.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (result.Channels != null && result.Channels.Count > 0)
                        {
                            var read = result.Channels[channel][authKey].ReadEnabled;
                            var write = result.Channels[channel][authKey].WriteEnabled;
                            if (read && write)
                            {
                                receivedGrantMessage = true;
                            }
                        }
                        else if (result.ChannelGroups != null && result.ChannelGroups.Count > 0)
                        {
                            var read = result.ChannelGroups[channelGroupName][authKey].ReadEnabled;
                            var write = result.ChannelGroups[channelGroupName][authKey].WriteEnabled;
                            var manage = result.ChannelGroups[channelGroupName][authKey].ManageEnabled;
                            if (read && write && manage)
                            {
                                receivedGrantMessage = true;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    }
                }
                catch
                {
                }
                finally
                {
                    grantManualEvent.Set();
                }
            }
        }

        public class UTSubscribeCallback : SubscribeCallback
        {
            public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
            {
                if (message != null)
                {
                    Console.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(message.Message));
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnReceivedMessage":
                        case "ThenSubscribeShouldReturnReceivedMessageSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageCipherSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageSecret":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretCipher":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageCipher":
                        case "ThenSubscribeShouldReturnEmojiMessage":
                        case "ThenSubscribeShouldReturnEmojiMessageSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecretSSL":
                        case "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage":
                            if (publishedMessage.ToString() == message.Message.ToString())
                            {
                                receivedMessage = true;
                            }
                            subscribeManualEvent.Set();
                            break;
                        //case "ThenSubscriberShouldBeAbleToReceiveManyMessages":
                        //    numberOfReceivedMessages++;
                        //    if (numberOfReceivedMessages >= 10)
                        //    {
                        //        receivedMessage = true;
                        //        subscribeManualEvent.Set();
                        //    }
                        //    break;
                        default:
                            break;
                    }
                }
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
                switch(currentTestCase)
                {
                    case "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback":
                        receivedMessage = true;
                        subscribeManualEvent.Set();
                        break;
                    default:
                        break;
                }
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                //Console.WriteLine("SubscribeCallback: PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                Console.WriteLine("SubscribeCallback: PNStatus: " + status.StatusCode.ToString());
                if (status.StatusCode != 200 || status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnReceivedMessage":
                        case "ThenSubscribeShouldReturnReceivedMessageSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageCipherSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageSecret":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretCipher":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageCipher":
                        case "ThenSubscribeShouldReturnEmojiMessage":
                        case "ThenSubscribeShouldReturnEmojiMessageSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecretSSL":
                        case "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage":
                        case "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback":
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    if (status.ErrorData != null)
                    {
                        Console.WriteLine(status.ErrorData.Information);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNConnectedCategory)
                {
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnReceivedMessage":
                        case "ThenSubscribeShouldReturnReceivedMessageSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageCipherSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageSecret":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretCipher":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageCipher":
                        case "ThenSubscribeShouldReturnEmojiMessage":
                        case "ThenSubscribeShouldReturnEmojiMessageSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecretSSL":
                        case "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage":
                            subscribeManualEvent.Set();
                            break;
                        //case "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback":
                        //    receivedMessage = true;
                        //    subscribeManualEvent.Set();
                        //    break;
                        default:
                            break;
                    }
                }


            }
        }

        public class UTPublishResult : PNCallback<PNPublishResult>
        {
            public override void OnResponse(PNPublishResult result, PNStatus status)
            {
                Console.WriteLine("Publish Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("Publish PNStatus => Status = : " + status.StatusCode.ToString());
                if (result != null && status.StatusCode == 200 && !status.Error)
                {
                    publishTimetoken = result.Timetoken;
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnReceivedMessage":
                        case "ThenSubscribeShouldReturnReceivedMessageSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageCipherSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageSecret":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretCipher":
                        case "ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL":
                        case "ThenSubscribeShouldReturnReceivedMessageCipher":
                        case "ThenSubscribeShouldReturnEmojiMessage":
                        case "ThenSubscribeShouldReturnEmojiMessageSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecretSSL":
                        case "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage":
                            receivedMessage = true;
                            break;
                        default:
                            break;
                    }
                }

                publishManualEvent.Set();
            }
        }

        public class ChannelGroupAddChannelResult : PNCallback<PNChannelGroupsAddChannelResult>
        {
            public override void OnResponse(PNChannelGroupsAddChannelResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        //if (status.StatusCode == 200 && result.Message.ToLower() == "ok" && result.Service == "channel-registry"&& status.Error == false && result.ChannelGroup.Substring(1) == channelGroupName)
                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            receivedMessage = true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    channelGroupManualEvent.Set();
                }
            }
        }

    }
}
