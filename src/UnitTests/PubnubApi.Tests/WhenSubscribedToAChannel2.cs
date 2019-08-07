using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToAChannel2 : TestHarness
    {
        private static ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static object publishedMessage;
        private static long publishTimetoken = 0;
        private static bool receivedGrantMessage = false;

        static int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string[] channelsGrant = { "hello_my_channel", "hello_my_channel1", "hello_my_channel2" };
        private static string authKey = "myAuth";
        private static string currentTestCase = "";

        private static Pubnub pubnub;

        private static Server server;

        public class TestLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine(logText);
            }
        }

        [TestFixtureSetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();

            if (!PubnubCommon.PAMEnabled)
            {
                return;
            }

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Uuid = "mytestuuid",
                Secure = false
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
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "hc7IKhEB7tyL6ENR3ndOOlHqPIG3RmzxwJMSGpofE6Q=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().Channels(channelsGrant).AuthKeys(new [] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Execute(new UTGrantResult());

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannel2 Grant access failed.");
        }

        [TestFixtureTearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnReceivedMessage";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenItShouldReturnReceivedMessage Failed");
        }

        private static void CommonSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = secretKey,
                CipherKey = cipherKey,
                Uuid = "mytestuuid",
                AuthKey = authKey,
                Secure = ssl,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog(),
                NonSubscribeRequestTimeout = 120
            };
            server.RunOnHttps(ssl);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = createPubNubInstance(config);
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
                .WithParameter("uuid", config.Uuid)
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
                .WithParameter("uuid", config.Uuid)
                .WithParameter("signature", "zJpO1HpSZxGkOr3EALbOk-vQgjIZTZ6AU5svzNU9l_A=")
                .WithResponse(expected)
                .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).QueryParam(new Dictionary<string, object> { { "ut", currentTestCase } }).Execute();

            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            publishManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);

            publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20WhenSubscribedToAChannel%20ThenItShouldReturnReceivedMessage%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22QoHwTga0QtOCtJRQ6sqtyateB%2FVotNt%2F50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5%2FlXSCw%3D%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            pubnub.Publish().Channel(channel).Message(publishedMessage).QueryParam(new Dictionary<string, object> { { "ut", currentTestCase } }).Execute(new UTPublishResult());

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(1000);

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).QueryParam(new Dictionary<string, object> { { "ut", currentTestCase } }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Thread.Sleep(1000);
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageCipherSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageCipherSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageCipherSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSecret()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecret";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSecretSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecretSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecretSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSecretCipher()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecretCipher";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipher Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageCipher()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnReceivedMessageCipher";
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnReceivedMessageCipher Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnEmojiMessage";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessage Failed");
        }

        private static void CommonSubscribeShouldReturnEmojiMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = secretKey,
                CipherKey = cipherKey,
                Uuid = "mytestuuid",
                AuthKey = authKey,
                Secure = ssl,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog(),
                NonSubscribeRequestTimeout = 120
            };
            server.RunOnHttps(ssl);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = createPubNubInstance(config);
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
                .WithParameter("uuid", config.Uuid)
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
                .WithParameter("uuid", config.Uuid)
                .WithParameter("signature", "zJpO1HpSZxGkOr3EALbOk-vQgjIZTZ6AU5svzNU9l_A=")
                .WithResponse(expected)
                .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).QueryParam(new Dictionary<string, object>() { {"ut", currentTestCase } }).Execute();

            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            publishManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);

            publishedMessage = "Text with 😜 emoji 🎉.";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Text%20with%20%F0%9F%98%9C%20emoji%20%F0%9F%8E%89.%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22vaD98V5XDtEvByw6RrxT9Ya76GKQLhyrEZw9Otrsu1KBVDIqGgWkrAD8X6TM%2FXC6%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            pubnub.Publish().Channel(channel).Message(publishedMessage).QueryParam(new Dictionary<string, object>() { { "ut", currentTestCase } }).Execute(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).QueryParam(new Dictionary<string, object>() { { "ut", currentTestCase } }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            Thread.Sleep(1000);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Thread.Sleep(1000);

        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageSSL";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageSecret()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageSecret";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageCipherSecret()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageCipherSecret";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL";
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageSecretSSL()
        {
            server.ClearRequests();

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
                    Debug.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(message.Message));
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
                            break;
                        default:
                            break;
                    }
                }
                subscribeManualEvent.Set();
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
            }

            public override void Signal<T>(Pubnub pubnub, PNMessageResult<T> signal)
            {
                throw new NotImplementedException();
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                Debug.WriteLine("SubscribeCallback: PNStatus: " + status.StatusCode.ToString());
                if (status.StatusCode != 200 || status.Error)
                {
                    Debug.WriteLine("Subsccribe ErrorData: " + status.ErrorData?.Information);
                    switch (currentTestCase)
                    {
                        case "ThenPresenceShouldReturnReceivedMessage":
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
                }


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
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Debug.WriteLine("Publish ErrorData: " + status.ErrorData?.Information);
                }
                publishManualEvent.Set();
            }
        }

    }
}
