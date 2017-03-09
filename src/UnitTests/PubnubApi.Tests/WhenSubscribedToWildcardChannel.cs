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
            server = Server.Instance();
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
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);
            manualResetEventWaitTimeout = 310 * 1000;

            channel = "foo.*";
            grantManualEvent = new ManualResetEvent(false);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"foo.*\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", "foo.%2A")
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "0OiQ1k5uyR4Y56XBmpCfMFtMkUiJKMf6k-OZEs5ea5c=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
            Thread.Sleep(1000);
            grantManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedGrantMessage)
            {
                receivedGrantMessage = false;

                channel = "foo.bar";
                grantManualEvent = new ManualResetEvent(false);

                expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"foo.bar\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                        .WithParameter("auth", authKey)
                        .WithParameter("channel", channel)
                        .WithParameter("m", "1")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("r", "1")
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("timestamp", "1356998400")
                        .WithParameter("ttl", "20")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("w", "1")
                        .WithParameter("signature", "aIQJHjVxSH626VLkW7ULvBcifLYGyZBWGQ-Nbpss4Qw=")
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedGrantMessage)
            {
                receivedGrantMessage = false;

                channel = "hello_my_channel";
                grantManualEvent = new ManualResetEvent(false);

                expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                        .WithParameter("auth", authKey)
                        .WithParameter("channel", channel)
                        .WithParameter("m", "1")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("r", "1")
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("timestamp", "1356998400")
                        .WithParameter("ttl", "20")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("w", "1")
                        .WithParameter("signature", "JMQKzXgfqNo-HaHuabC0gq0X6IkVMHa9AWBCg6BGN1Q=")
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedGrantMessage)
            {
                receivedGrantMessage = false;

                channel = "hello_my_channel1";
                grantManualEvent = new ManualResetEvent(false);

                expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel1\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                        .WithParameter("auth", authKey)
                        .WithParameter("channel", channel)
                        .WithParameter("m", "1")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("r", "1")
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("timestamp", "1356998400")
                        .WithParameter("ttl", "20")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("w", "1")
                        .WithParameter("signature", "FVeU4RXzcxTzOf7xvmMyEPllc388HDpDfdT5lnGcLVE=")
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedGrantMessage)
            {
                receivedGrantMessage = false;

                channelGroupName = "hello_my_group";
                grantManualEvent = new ManualResetEvent(false);

                expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel-group\":\"hello_my_group\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                        .WithParameter("auth", authKey)
                        .WithParameter("channel-group", channelGroupName)
                        .WithParameter("m", "1")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("r", "1")
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("timestamp", "1356998400")
                        .WithParameter("ttl", "20")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("w", "1")
                        .WithParameter("signature", "mnWJN7WSbajMt_LWpuiXGhcs3NUcVbU3L_MZpb9_blU=")
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

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
            server.ClearRequests();

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
            server.RunOnHttps(ssl);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("signature", "5yl-J1ci5xJzHDTctEWusTxwvwRPuZ_JNAALf_zBvJU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            publishManualEvent = new ManualResetEvent(false);
            publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20WhenSubscribedToAChannel%20ThenItShouldReturnReceivedMessage%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, publishChannel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22QoHwTga0QtOCtJRQ6sqtyateB%2FVotNt%2F50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5%2FlXSCw%3D%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, publishChannel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            pubnub.Publish().Channel(publishChannel).Message(publishedMessage).Async(new UTPublishResult());

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute();

            Thread.Sleep(1000);

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
            server.ClearRequests();

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
            server.RunOnHttps(ssl);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("auth", authKey)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("signature", "5yl-J1ci5xJzHDTctEWusTxwvwRPuZ_JNAALf_zBvJU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            publishManualEvent = new ManualResetEvent(false);
            publishedMessage = "Text with 😜 emoji 🎉.";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Text%20with%20%F0%9F%98%9C%20emoji%20%F0%9F%8E%89.%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, publishChannel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22vaD98V5XDtEvByw6RrxT9Ya76GKQLhyrEZw9Otrsu1KBVDIqGgWkrAD8X6TM%2FXC6%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, publishChannel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            pubnub.Publish().Channel(publishChannel).Message(publishedMessage).Async(new UTPublishResult());

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);
            Console.WriteLine("publishManualEvent.WaitOne DONE");

            pubnub.Unsubscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).Execute();

            Thread.Sleep(1000);

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
            server.ClearRequests();

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
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";
            string subChannelName = "hello_my_channel";
            string[] commaDelimitedChannel = new string[] { subChannelName, wildCardSubscribeChannel };
            channelGroupName = "hello_my_group";
            string channelAddForGroup = "hello_my_channel1";
            string pubWildChannelName = "foo.a";
            manualResetEventWaitTimeout = 310 * 1000;

            channelGroupManualEvent = new ManualResetEvent(false);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelAddForGroup)
                    .WithParameter("auth", config.AuthKey)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.AddChannelsToChannelGroup().Channels(new string[] { channelAddForGroup }).ChannelGroup(channelGroupName).Async(new ChannelGroupAddChannelResult());
            channelGroupManualEvent.WaitOne();

            subscribeManualEvent = new ManualResetEvent(false);

            expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A,hello_my_channel"))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel-group", channelGroupName)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A,hello_my_channel"))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(commaDelimitedChannel).ChannelGroups(new string[] { channelGroupName }).Execute();

            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            publishManualEvent = new ManualResetEvent(false);

            publishedMessage = "Test for cg";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20cg%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            pubnub.Publish().Channel(channelAddForGroup).Message(publishedMessage).Async(new UTPublishResult());

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);
            Console.WriteLine("publishManualEvent.WaitOne DONE");


            if (receivedMessage)
            {
                receivedMessage = false;

                subscribeManualEvent = new ManualResetEvent(false);

                Thread.Sleep(1000);
                publishManualEvent = new ManualResetEvent(false);
                publishedMessage = "Test for wc";

                expected = "[1,\"Sent\",\"14722484585147754\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20wc%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, pubWildChannelName))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);

                pubnub.Publish().Channel(pubWildChannelName).Message(publishedMessage).Async(new UTPublishResult());

                publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                Console.WriteLine("publishManualEvent.WaitOne DONE");
            }

            if (receivedMessage)
            {
                receivedMessage = false;

                publishManualEvent = new ManualResetEvent(false);
                publishedMessage = "Test for normal ch";

                expected = "[1,\"Sent\",\"14722484585147754\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20normal%20ch%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, subChannelName))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);

                pubnub.Publish().Channel(subChannelName).Message(publishedMessage).Async(new UTPublishResult());

                publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                Console.WriteLine("publishManualEvent.WaitOne DONE");
            }

            pubnub.Unsubscribe<string>().Channels(commaDelimitedChannel).ChannelGroups(new string[] { channelGroupName }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback()
        {
            server.ClearRequests();

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
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14843277463968146\",\"r\":7},\"m\":[{\"a\":\"4\",\"f\":512,\"p\":{\"t\":\"14843277462084783\",\"r\":1},\"k\":\"demo-36\",\"c\":\"foo.*-pnpres\",\"d\":{\"action\": \"join\", \"timestamp\": 1484327746, \"uuid\": \"mytestuuid\", \"occupancy\": 1},\"b\":\"foo.*\"}]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel-group",channelGroupName)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).ChannelGroups(new string[] { channelGroupName }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(1000);

            pubnub.Unsubscribe<string>().Channels(new string[] { wildCardSubscribeChannel }).ChannelGroups(new string[] { channelGroupName }).Execute();

            Thread.Sleep(1000);

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
