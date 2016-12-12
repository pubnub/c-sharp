using System;
using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToAChannel2 : TestHarness
    {
        private static ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static object publishedMessage = null;
        private static long publishTimetoken = 0;
        private static bool receivedGrantMessage = false;

        int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string[] channelsGrant = { "hello_my_channel", "hello_my_channel1", "hello_my_channel2" };
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

            pubnub.Grant().Channels(channelsGrant).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannel2 Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {

        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessage";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenItShouldReturnReceivedMessage Failed");
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
                AuthKey = authKey,
                Secure = ssl
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            publishManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);

            publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";
            pubnub.Publish().Channel(channel).Message(publishedMessage).Async(new UTPublishResult());

            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for message

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

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
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageCipherSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageCipherSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecret()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecret";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecretSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretCipher()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecretCipher";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageCipher()
        {
            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageCipher";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessage()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessage";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessage Failed");
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
                AuthKey = authKey,
                Secure = ssl
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            publishManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);

            publishedMessage = "Text with 😜 emoji 🎉.";
            pubnub.Publish().Channel(channel).Message(publishedMessage).Async(new UTPublishResult());

            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for message

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
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
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSecret()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageSecret";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageCipherSecret()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageCipherSecret";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSecretSSL()
        {
            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageSecretSSL";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageSecretSSL Failed");
        }

        private class UTGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (result.Channels != null && result.Channels.Count > 0)
                        {
                            foreach (KeyValuePair<string, Dictionary<string, PNAccessManagerKeyData>> channelKP in result.Channels)
                            {
                                string channel = channelKP.Key;
                                if (Array.IndexOf(channelsGrant, channel) > -1)
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (read && write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    else
                                    {
                                        receivedGrantMessage = false;
                                    }
                                }
                                else
                                {
                                    receivedGrantMessage = false;
                                    break;
                                }
                            }
                        }
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
                            if (publishedMessage.ToString() == message.Message.ToString())
                            {
                                receivedMessage = true;
                            }
                            subscribeManualEvent.Set();
                            break;
                        case "ThenSubscribeShouldReturnEmojiMessage":
                        case "ThenSubscribeShouldReturnEmojiMessageSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecret":
                        case "ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL":
                        case "ThenSubscribeShouldReturnEmojiMessageSecretSSL":
                            if (publishedMessage.ToString() == message.Message.ToString())
                            {
                                receivedMessage = true;
                            }
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                //Console.WriteLine("SubscribeCallback: PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                Console.WriteLine("SubscribeCallback: PNStatus: " + status.StatusCode.ToString());
                if (status.StatusCode != 200 || status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "ThenPresenceShouldReturnReceivedMessage":
                            //presenceManualEvent.Set();
                            break;
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
                            subscribeManualEvent.Set();
                            break;
                        //case "ThenSubscribeShouldReturnConnectStatus":
                        //case "ThenMultiSubscribeShouldReturnConnectStatus":
                        //case "ThenMultiSubscribeShouldReturnConnectStatusSSL":
                        //case "ThenSubscriberShouldBeAbleToReceiveManyMessages":
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
                            receivedMessage = true;
                            publishManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

    }
}
