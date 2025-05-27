using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToAChannel3 : TestHarness, IDisposable
    {
        private static ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static object publishedMessage;
        private static long publishTimetoken = 0;
        private static bool receivedGrantMessage = false;

        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string channel2 = "hello_my_channel_2";
        private static string[] channelsGrant = { "hello_my_channel", "hello_my_channel1", "hello_my_channel2" };
        private static string authKey = "myauth";
        private static string currentTestCase = "";
        private static string authToken;

        private static Pubnub pubnub;

        private static Server server;
        private static UnitTestLog unitLog;

        [SetUp]
        public static async Task Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            if (PubnubCommon.EnableStubTest)
            {
                server.Start();   
            }

            if (!PubnubCommon.PAMServerSideGrant) { return; }

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Secure = false,
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", "hello_my_channel%2Chello_my_channel1%2Chello_my_channel2")
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "hc7IKhEB7tyL6ENR3ndOOlHqPIG3RmzxwJMSGpofE6Q=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));
            if (string.IsNullOrEmpty(PubnubCommon.GrantToken))
            {
                await GenerateTestGrantToken(pubnub);
            }
            authToken = PubnubCommon.GrantToken;

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [TearDown]
        public static void Exit()
        {
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }
            server.Stop();
        }

        [Test]
        public static async Task ThenStatusCallbackShouldKeepFiring()
        {
            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                LogVerbosity = PNLogVerbosity.BODY
            };
            if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            var firstChannel = new ManualResetEvent(false);
            var secondChannel = new ManualResetEvent(false);
            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                },
                delegate (Pubnub pnObj, PNStatus status)
                {
                    Debug.WriteLine("Received STATUS callback");
                    if (status.AffectedChannels != null && (status.Category == PNStatusCategory.PNConnectedCategory || status.Category == PNStatusCategory.PNSubscriptionChangedCategory))
                    {
                        if (status.Category == PNStatusCategory.PNConnectedCategory && status.AffectedChannels.Contains(channel))
                        {
                            firstChannel.Set();
                        }
                        if (status.Category == PNStatusCategory.PNSubscriptionChangedCategory && status.AffectedChannels.Contains(channel2))
                        {
                            secondChannel.Set();
                        }
                    }
                }
            );
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(eventListener);
            pubnub.Subscribe<string>().Channels(new []{channel}).Execute();
            var firstCallbackSuccess = firstChannel.WaitOne(5000);
            await Task.Delay(2000);
            pubnub.Subscribe<string>().Channels(new []{channel2}).Execute();
            var secondCallbackSuccess = secondChannel.WaitOne(5000);
            Assert.True(firstCallbackSuccess && secondCallbackSuccess);
        }

        [Test]
        public static void ThenSubscribeShouldReturnUnicodeMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnUnicodeMessage";
            CommonSubscribeShouldReturnUnicodeMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnUnicodeMessage Failed");
        }

        private static void CommonSubscribeShouldReturnUnicodeMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor(cipherKey), null),
                Secure = ssl
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = secretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(ssl);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("auth", authKey)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            publishManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);

            publishedMessage = "Text with ÜÖ漢語";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Text%20with%20%C3%9C%C3%96%E6%BC%A2%E8%AA%9E%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22QoHwTga0QtOCtJRQ6sqtyateB%2FVotNt%2F50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5%2FlXSCw%3D%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            pubnub.Publish().Channel(channel).Message(publishedMessage).Execute(new UTPublishResult());

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSubscribeShouldReturnUnicodeMessageSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnUnicodeMessageSSL";
            CommonSubscribeShouldReturnUnicodeMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnUnicodeMessageSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnForwardSlashMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnForwardSlashMessage";
            CommonSubscribeReturnForwardSlashMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessage Failed");
        }

        private static void CommonSubscribeReturnForwardSlashMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor(cipherKey), null),
                Secure = ssl,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = secretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(ssl);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout =  310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("auth", authKey)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("auth", authKey)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "zJpO1HpSZxGkOr3EALbOk-vQgjIZTZ6AU5svzNU9l_A=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            publishManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);

            publishedMessage = "Text with /";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Text%20with%20%2F%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22s98XlGoA68ypX1Z7A7mOwQ%3D%3D%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));


            pubnub.Publish().Channel(channel).Message(publishedMessage).Execute(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(1000);

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSubscribeShouldReturnForwardSlashMessageSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnForwardSlashMessageSSL";
            CommonSubscribeReturnForwardSlashMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnForwardSlashMessageCipher()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnForwardSlashMessageCipher";
            CommonSubscribeReturnForwardSlashMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageCipher Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnForwardSlashMessageCipherSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnForwardSlashMessageCipherSSL";
            CommonSubscribeReturnForwardSlashMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageCipherSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnForwardSlashMessageSecret()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnForwardSlashMessageSecret";
            CommonSubscribeReturnForwardSlashMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnForwardSlashMessageCipherSecret()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnForwardSlashMessageCipherSecret";
            CommonSubscribeReturnForwardSlashMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageCipherSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnForwardSlashMessageCipherSecretSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnForwardSlashMessageCipherSecretSSL";
            CommonSubscribeReturnForwardSlashMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageCipherSecretSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnForwardSlashMessageSecretSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnForwardSlashMessageSecretSSL";
            CommonSubscribeReturnForwardSlashMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnForwardSlashMessageSecretSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnSpecialCharMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnSpecialCharMessage";
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessage Failed");
        }

        private static void CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor(cipherKey), null),
                Secure = ssl
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = secretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(ssl);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout =  310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("auth", authKey)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("auth", authKey)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "zJpO1HpSZxGkOr3EALbOk-vQgjIZTZ6AU5svzNU9l_A=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            
            Thread.Sleep(2000);
            
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            publishManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);

            publishedMessage = "Text with '\"";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Text%20with%20%27%5C%22%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22kl7vmPUMMz6UdliN7t6XYw%3D%3D%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            pubnub.Publish().Channel(channel).Message(publishedMessage).Execute(new UTPublishResult());

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSubscribeShouldReturnSpecialCharMessageSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnSpecialCharMessageSSL";
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnSpecialCharMessageCipher()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnSpecialCharMessageCipher";
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageCipher Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnSpecialCharMessageCipherSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnSpecialCharMessageCipherSSL";
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageCipherSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnSpecialCharMessageSecret()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnSpecialCharMessageSecret";
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnSpecialCharMessageCipherSecret()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnSpecialCharMessageCipherSecret";
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageCipherSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnSpecialCharMessageCipherSecretSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnSpecialCharMessageCipherSecretSSL";
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageCipherSecretSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnSpecialCharMessageSecretSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnSpecialCharMessageSecretSSL";
            CommonSubscribeShouldReturnSpecialCharMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnSpecialCharMessageSecretSSL Failed");
        }

        private class UTGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
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
                catch { /* ignore */ }
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
                    Debug.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(message.Message));
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnUnicodeMessage":
                        case "ThenSubscribeShouldReturnUnicodeMessageSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessage":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipher":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSecret":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSecretSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSecret":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSecretSSL":
                            if (publishedMessage.ToString() == message.Message.ToString())
                            {
                                receivedMessage = true;
                            }
                            subscribeManualEvent.Set();
                            break;
                        case "ThenSubscribeShouldReturnSpecialCharMessage":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipher":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSecret":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSecretSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSecret":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSecretSSL":
                        default:
                            break;
                    }
                }
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
                if (presence != null)
                {
                    Debug.WriteLine("SubscribeCallback: Presence: " + presence.Event);
                }
            }

            public override void Signal<T>(Pubnub pubnub, PNSignalResult<T> signal)
            {
                throw new NotImplementedException();
            }

            public override void ObjectEvent(Pubnub pubnub, PNObjectEventResult objectEvent)
            {
                throw new NotImplementedException();
            }

            public override void MessageAction(Pubnub pubnub, PNMessageActionEventResult messageAction)
            {
                throw new NotImplementedException();
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                Debug.WriteLine("SubscribeCallback: PNStatus: " + status.StatusCode.ToString());
                if (status.StatusCode != 200 || status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "ThenPresenceShouldReturnReceivedMessage":
                            break;
                        case "ThenSubscribeShouldReturnUnicodeMessage":
                        case "ThenSubscribeShouldReturnUnicodeMessageSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessage":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipher":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSecret":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSecretSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSecret":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSecretSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessage":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipher":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSecret":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSecretSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSecret":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSecretSSL":
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }

                    if (status.ErrorData != null)
                    {
                        Debug.WriteLine(status.ErrorData.Information);
                    }
                }
                else if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNConnectedCategory)
                {
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnUnicodeMessage":
                        case "ThenSubscribeShouldReturnUnicodeMessageSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessage":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipher":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSecret":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSecretSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSecret":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSecretSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessage":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipher":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSecret":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSecretSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSecret":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSecretSSL":
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }


            }

            public override void File(Pubnub pubnub, PNFileEventResult fileEvent)
            {
                throw new NotImplementedException();
            }
        }

        public class UTPublishResult : PNCallback<PNPublishResult>
        {
            public override void OnResponse(PNPublishResult result, PNStatus status)
            {
                Debug.WriteLine("Publish Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Debug.WriteLine("Publish PNStatus => Status = : " + status.StatusCode.ToString());
                if (result != null && status.StatusCode == 200 && !status.Error)
                {
                    publishTimetoken = result.Timetoken;
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnUnicodeMessage":
                        case "ThenSubscribeShouldReturnUnicodeMessageSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessage":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipher":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSecret":
                        case "ThenSubscribeShouldReturnForwardSlashMessageSecretSSL":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSecret":
                        case "ThenSubscribeShouldReturnForwardSlashMessageCipherSecretSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessage":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipher":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSecret":
                        case "ThenSubscribeShouldReturnSpecialCharMessageSecretSSL":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSecret":
                        case "ThenSubscribeShouldReturnSpecialCharMessageCipherSecretSSL":
                            receivedMessage = true;
                            publishManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    subscribeManualEvent.Close();
                    publishManualEvent.Close();
                    grantManualEvent.Close();
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
