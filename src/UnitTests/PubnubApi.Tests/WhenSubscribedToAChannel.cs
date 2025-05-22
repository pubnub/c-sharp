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
    public class WhenSubscribedToAChannel : TestHarness
    {
        static int manualResetEventWaitTimeout = 310 * 1000;
        private static string authKey = "myauth";
        private static string channel = "hello_my_channel";
        private static string authToken;

        private static Pubnub pubnub;
        private static Server server;

        public class TestLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine(logText);
            }
        }

        [SetUp]
        public static async Task Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            if (PubnubCommon.EnableStubTest)
            {
                server.Start();   
            }

            if (!PubnubCommon.PAMServerSideGrant) { return; }

            bool receivedGrantMessage = false;
            
            
            string[] channelsGrant = { "hello_my_channel", "hello_my_channel1", "hello_my_channel2" };

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Secure = true
            };
            server.RunOnHttps(true);

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

        [TearDown]
        public void Cleanup()
        {

        }

        [Test]
        public static void ThenMissingSubscribeKeyShouldReturnException()
        {
            server.ClearRequests();

            string channel = "hello_my_channel";
            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
            };
            server.RunOnHttps(true);

            pubnub = createPubNubInstance(config, authToken);

            Assert.Throws<MissingMemberException>(() =>
            {
                pubnub.Subscribe<string>()
                        .Channels(new string[] {channel })
                        .WithPresence()
                        .Execute();
            });

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenComplexMessageSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;

            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public static async Task ThenNonEEMultipleSubscribeCallShouldReturnMessage()
        {
            bool receivedSubscribedMessage = false;
            bool internalReceivedMessage = false;
            bool receivedErrorMessage = false;
            var message = "hello_channel4";
            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            ManualResetEvent subscribeMessageManualEvent = new ManualResetEvent(false);
            
            PNConfiguration pubnubConfiguration = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                EnableEventEngine = false
            };
            
            var listener = new SubscribeCallbackExt(
                (_, messageEvent) =>
                {
                    if ((string)messageEvent.Message == message)
                        subscribeMessageManualEvent.Set();
                }
                , (_, _) =>
                {}
                , (_, status) => { subscribeManualEvent.Set(); }
            );
            
            pubnub = createPubNubInstance(pubnubConfiguration, authToken);
            pubnub.AddListener(listener);
            
            string channel = "hello_my_channel";
            string channel1 = "hello_my_channel_1";
            string channel2 = "hello_my_channel_2";
            string channel3 = "hello_my_channel_3";
            string channel4 = "hello_my_channel_4";
            
            pubnub.Subscribe<object>().Channels(new string[]{"channel"}).WithPresence().Execute();
        
            pubnub.Subscribe<object>().Channels(new string[]{"channel1","channel2"}).Execute();
            
            pubnub.Subscribe<object>().Channels(new string[] {"channel3","channel4"}).Execute();

            await Task.Delay(3000);

            await pubnub.Publish().Channel("channel4").Message(message).ExecuteAsync();
            
            manualResetEventWaitTimeout = 310 * 1000;
            
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status
            
            subscribeMessageManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.UnsubscribeAll<object>();
            pubnub.Destroy();
        }
        
        private static void CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl, out bool receivedMessage)
        {
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }
            bool receivedSubscribedMessage = false;
            bool internalReceivedMessage = false;
            bool receivedErrorMessage = false;
            CustomClass publishedMessage = new CustomClass();

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

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            ManualResetEvent subscribeMessageManualEvent = new ManualResetEvent(false);

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => 
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message));
                        if (pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage) == pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message))
                        {
                            internalReceivedMessage = true;
                        }
                        subscribeMessageManualEvent.Set();
                    }
                },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        internalReceivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;


            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

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


            expected = "[1,\"Sent\",\"14836234233392078\"]";

            server.AddRequest(new Request()
                .WithMethod("GET")
                .WithPath(string.Format("/publish/demo-36/{0}/0/{1}/0/{2}", PubnubCommon.SubscribeKey, channel, "%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D"))
                .WithResponse(expected)
                .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                .WithMethod("GET")
                .WithPath(string.Format("/publish/demo-36/{0}/0/{1}/0/{2}", PubnubCommon.SubscribeKey, channel, "%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22"))
                .WithResponse(expected)
                .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            if (!receivedErrorMessage)
            {
                internalReceivedMessage = false;
                ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(channel).Message(publishedMessage)
                        .Execute(new PNPublishResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                internalReceivedMessage = true;
                            }
                            publishManualEvent.Set();
                        }));

                publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                receivedMessage = internalReceivedMessage;

                subscribeMessageManualEvent.WaitOne(manualResetEventWaitTimeout);

                Thread.Sleep(1000);

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                Thread.Sleep(1000);
            }

            receivedMessage = internalReceivedMessage;
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }

        [Test]
        public static void ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;

            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public static void ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;

            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public static void ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;

            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public static void ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;

            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public static void ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;

            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public static void ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;

            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public static void ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;

            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnConnectStatus()
        {
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog(),
                NonSubscribeRequestTimeout = 120,
                //EnableEventEngine = true, //default enabled
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { },
                (o, p) => { },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        receivedMessage = true;
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
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

            Thread.Sleep(1000);
            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            Thread.Sleep(1000);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [Test]
        public static void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { },
                (o, p) => { },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        receivedMessage = true;
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = 310 * 1000;

            string channel1 = "hello_my_channel_1";

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
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

            pubnub.Subscribe<string>().Channels(new [] { channel1 }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            string channel2 = "hello_my_channel2";
            if (receivedMessage)
            {
                // Reason:  As per eventEngine specification, Connected Status won't be emitted on Subscription entity changes
                // receivedMessage = false;

                // subscribeManualEvent = new ManualResetEvent(false);

                expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1 + "," + channel2))
                        .WithParameter("heartbeat", "300")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("tt", "0")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                expected = "{}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1 + "," + channel2))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);
                pubnub.Subscribe<string>().Channels(new [] { channel2 }).Execute();
                // subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status
            }

            Thread.Sleep(1000);
            
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Unsubscribe<string>().Channels(new [] { channel1, channel2 }).Execute();
            pubnub.Disconnect<string>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [Test]
        public static void ThenMultiSubscribeShouldReturnConnectStatusSSL()
        {
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = true,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog(),
                NonSubscribeRequestTimeout = 120
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(true);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { },
                (o, p) => { },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        receivedMessage = true;
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = 310 * 1000;

            string channel1 = "hello_my_channel_1";

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
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

            pubnub.Subscribe<string>().Channels(new [] { channel1 }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            string channel2 = "hello_my_channel2";
            if (receivedMessage)
            {
                // Reason: EventEngine will not emit connected status when a new channel entity is added.
                // receivedMessage = false;
                // subscribeManualEvent = new ManualResetEvent(false);

                expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1 + "," + channel2))
                        .WithParameter("heartbeat", "300")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("tt", "0")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                expected = "{}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1 + "," + channel2))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);

                pubnub.Subscribe<string>().Channels(new [] { channel2 }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status
            }

            Thread.Sleep(1000);

            
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Unsubscribe<string>().Channels(new [] { channel1, channel2 }).Execute();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenMultiSubscribeShouldReturnConnectStatusSSL Failed");
        }

        [Test]
        public static void ThenSubscriberShouldBeAbleToReceiveManyMessages()
        {
            server.ClearRequests();

            bool receivedMessage = false;
            bool receivedErrorMessage = false;
            int numberOfReceivedMessages = 0;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                // EnableEventEngine = true, TODO: Event engine fails those tests...
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) =>
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message));
                        numberOfReceivedMessages++;
                        if (numberOfReceivedMessages >= 10)
                        {
                            receivedMessage = true;
                            subscribeManualEvent.Set();
                        }
                    }
                },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        receivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = 310 * 1000;

            string channel = "hello_my_channel_1";
            numberOfReceivedMessages = 0;

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
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

            if (!receivedErrorMessage){
                if (receivedMessage && !PubnubCommon.EnableStubTest)
                {
                    for (int index = 0; index < 10; index++)
                    {
                        Debug.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Publishing " + index.ToString());
                        ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                        pubnub.Publish().Channel(channel).Message(index.ToString())
                            .Execute(new PNPublishResultExt((r, s) =>
                            {
                                if (r != null && s.StatusCode == 200 && !s.Error)
                                {
                                    receivedMessage = true;
                                }
                                publishManualEvent.Set();
                            }));
                        publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                    }

                }

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                Thread.Sleep(1000);
            }

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscriberShouldBeAbleToReceiveManyMessages Failed");
        }
        
        [Test]
        public static async Task ThenJoinEventReceived()
        {
            int randomNumber = new Random().Next(1000, 10000);
            string userId = $"testUser{randomNumber}";
            bool receivedJoinEvent = false;
            string randomChannel = $"testChannel{randomNumber}";

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
            };

            ManualResetEvent joinEvent = new ManualResetEvent(false);
            var listener = new SubscribeCallbackExt(
                (pn, messageEvent) => { }
                , (pn, presenceEvent) =>
                {
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    } 
                }
                , (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    }
                }
            );
            pubnub = createPubNubInstance(config);
            pubnub.AddListener(listener);
            manualResetEventWaitTimeout = 310 * 1000;
            pubnub.Subscribe<string>().Channels(new [] { randomChannel }).Execute();
            joinEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.RemoveListener(listener);
            pubnub.UnsubscribeAll<string>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            await Task.Delay(2000).ConfigureAwait(false);
            Assert.IsTrue(receivedJoinEvent, "WhenSubscribedToAChannel, Then JoinEvent should be received");
            
        }
        
        [Test]
        public static async Task ThenJoinEventReceivedForSubsequentSubscribe()
        {
            int randomNumber = new Random().Next(1000, 10000);
            string userId = $"testUser{randomNumber}";
            bool receivedJoinEvent = false;
            bool receivedSecondJoinEvent = false;
            string channel = $"testChannel{randomNumber}";
            string channel2 = $"testChannel{randomNumber}_2";

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey =PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            ManualResetEvent joinEvent = new ManualResetEvent(false);
            ManualResetEvent secondJoinEvent = new ManualResetEvent(false);
            var listener = new SubscribeCallbackExt(
                (pn, messageEvent) => { }
                , (pn, presenceEvent) =>
                {
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId && presenceEvent.Channel == channel)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    }
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId && presenceEvent.Channel == channel2)
                    {
                        secondJoinEvent.Set();
                        receivedSecondJoinEvent = true;
                    }
                    
                }
                , (_, status) =>
                {
                }
            );
            pubnub = new Pubnub(config);
            pubnub.AddListener(listener);
            manualResetEventWaitTimeout = 310 * 1000;
            pubnub.Subscribe<string>().Channels(new [] { channel }).WithPresence().Execute();
            joinEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.Subscribe<string>().Channels(new [] { channel2 }).WithPresence().Execute();
            secondJoinEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.RemoveListener(listener);
            pubnub.UnsubscribeAll<string>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            await Task.Delay(2000).ConfigureAwait(false);
            Assert.IsTrue(receivedJoinEvent&& receivedSecondJoinEvent, "WhenSubscribedToAChannel, Then JoinEvent should be received for subsequent Subscribe");
        }
        
        [Test]
        public static async Task ThenJoinEventReceivedForSubsequentSubscribeWithPresenceEventEngine()
        {
            int randomNumber = new Random().Next(1000, 10000);
            string userId = $"testUser{randomNumber}";
            bool receivedJoinEvent = false;
            bool receivedSecondJoinEvent = false;
            string channel = $"testChannel{randomNumber}";
            string channel2 = $"testChannel2{randomNumber}";

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                PresenceTimeout = 310
            };

            ManualResetEvent joinEvent = new ManualResetEvent(false);
            ManualResetEvent secondJoinEvent = new ManualResetEvent(false);
            var listener = new SubscribeCallbackExt(
                (pn, messageEvent) => { }
                , (pn, presenceEvent) =>
                {
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId && presenceEvent.Channel == channel)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    }
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId && presenceEvent.Channel == channel2)
                    {
                        secondJoinEvent.Set();
                        receivedSecondJoinEvent = true;
                    }
                }
                , (_, status) =>
                {
                }
            );
            pubnub = createPubNubInstance(config);
            pubnub.AddListener(listener);
            manualResetEventWaitTimeout = 310 * 1000;
            pubnub.Subscribe<string>().Channels(new [] { channel }).WithPresence().Execute();
            joinEvent.WaitOne(manualResetEventWaitTimeout);
            await Task.Delay(2000);
            pubnub.Subscribe<string>().Channels(new [] { channel2 }).WithPresence().Execute();
            secondJoinEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.RemoveListener(listener);
            pubnub.UnsubscribeAll<string>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            await Task.Delay(2000).ConfigureAwait(false);
            Assert.IsTrue(receivedJoinEvent&& receivedSecondJoinEvent, "WhenSubscribedToAChannel, Then JoinEvent should be received when presence timeout is set");
        }
        
        [Test]
        public static async Task ThenJoinEventReceivedForSubsequentSubscribeWithZeroInterval()
        {
            int randomNumber = new Random().Next(1000, 10000);
            string userId = $"testUser{randomNumber}";
            bool receivedJoinEvent = false;
            bool receivedSecondJoinEvent = false;
            string channel = $"testChannel{randomNumber}";
            string channel2 = $"testChannel{randomNumber}_3";

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
            };
            config.SetPresenceTimeoutWithCustomInterval(300, 0);
            ManualResetEvent joinEvent = new ManualResetEvent(false);
            ManualResetEvent secondJoinEvent = new ManualResetEvent(false);
            var listener = new SubscribeCallbackExt(
                (pn, messageEvent) => { }
                , (pn, presenceEvent) =>
                {
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId && presenceEvent.Channel == channel)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    }
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId && presenceEvent.Channel == channel2)
                    {
                        secondJoinEvent.Set();
                        receivedSecondJoinEvent = true;
                    }
                    
                }
                , (_, status) =>
                {
                }
            );
            pubnub = new Pubnub(config);
            pubnub.AddListener(listener);
            manualResetEventWaitTimeout = 310 * 1000;

            pubnub.Subscribe<string>().Channels(new [] { channel }).WithPresence().Execute();
            joinEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.Subscribe<string>().Channels(new [] { channel2 }).WithPresence().Execute();
            secondJoinEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.RemoveListener(listener);
            pubnub.UnsubscribeAll<string>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            await Task.Delay(2000).ConfigureAwait(false);
            Assert.IsTrue(receivedJoinEvent&& receivedSecondJoinEvent, "WhenSubscribedToAChannel, Then JoinEvent should be received when presenceTimeout is set to 0");
        }

    }
}
