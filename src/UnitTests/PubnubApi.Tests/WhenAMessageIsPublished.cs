using System;
using System.Linq;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;
#if NETSTANDARD20 || NET60
using PubnubApiPCL.Tests;
#else
using PubnubApi.Tests;
#endif
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using WireMock.Server;
using WireMock.Matchers;
using WireMockRequest = WireMock.RequestBuilders.Request;
using WireMockResponse = WireMock.ResponseBuilders.Response;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAMessageIsPublished : TestHarness
    {
        private const string messageForUnencryptPublish = "Pubnub Messaging API 1";
        private const string messageForEncryptPublish = "漢語";
        private const string messageForSecretEncryptPublish = "Pubnub Messaging API 2";

        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;
        private static Server server;
        private static string authKey = "myauth";
        private static string authToken;

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
            string channel = "hello_my_channel";

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

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
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "JMQKzXgfqNo-HaHuabC0gq0X6IkVMHa9AWBCg6BGN1Q=")
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
        public static void ThenNullMessageShouldReturnException()
        {
            server.ClearRequests();

            string channel = "hello_my_channel";
            object message = null;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }

            server.RunOnHttps(true);

            pubnub = createPubNubInstance(config, authToken);

            string expected = "";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, message))
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Assert.Throws<ArgumentException>(() =>
            {
                pubnub.Publish()
                        .Channel(channel)
                        .Message(message)
                        .Execute(new PNPublishResultExt((r, s) => { }));
            });
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenUnencryptPublishGETShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;

            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14715278266153304\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedPublishMessage)
            {
                Assert.IsTrue(receivedPublishMessage, "Unencrypt Publish Failed");
            }
            else
            {
                receivedPublishMessage = false;

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

                expected = "[[\"Pubnub Messaging API 1\"],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715278266153304")
                        .WithParameter("include_token", "true")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715278266153304 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Execute(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedPublishMessage = true;
                                    historyManualEvent.Set();
                                }));

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful unencrypt Publish");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncUnencryptPublishGETShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncUnencryptPublishGETShouldReturnSuccessCodeAndInfo()
#endif
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;

            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14715278266153304\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNPublishResult> respPub = Task.Factory.StartNew(async () => await pubnub.Publish().Channel(channel).Message(message).ExecuteAsync()).Result.Result;
#else
            PNResult<PNPublishResult> respPub = await pubnub.Publish().Channel(channel).Message(message).ExecuteAsync();
#endif
            if (respPub.Result != null && respPub.Status.StatusCode == 200 && !respPub.Status.Error)
            {
                publishTimetoken = respPub.Result.Timetoken;
                receivedPublishMessage = true;
            }

            if (!receivedPublishMessage)
            {
                Assert.IsTrue(receivedPublishMessage, "WithAsync Unencrypt Publish Failed");
            }
            else
            {
                receivedPublishMessage = false;

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

                expected = "[[\"Pubnub Messaging API 1\"],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715278266153304")
                        .WithParameter("include_token", "true")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
                PNResult<PNHistoryResult> respHist = Task.Factory.StartNew(async () => await pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715278266153304 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNHistoryResult> respHist = await pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715278266153304 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .ExecuteAsync();
#endif
                if (respHist.Result != null)
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(respHist.Result));
                    receivedPublishMessage = true;
                }

                Assert.IsTrue(receivedPublishMessage, "WithAsync Unable to match the successful unencrypt Publish");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenUnencryptFireGETShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;

            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14715278266153304\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Fire().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedPublishMessage, "Unencrypt Fire Failed");
        }
        
        public class MockObject
        {
            public string Text;
            public int Number;
            public bool Flag;
        }

        [Test]
        public static async Task ThenSendAndReceiveCustomObject()
        {
            var config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            pubnub = createPubNubInstance(config, authToken);

            var objectMessage = new MockObject()
            {
                Text = "some_text",
                Flag = true,
                Number = 2137
            };

            var receivedObject = new ManualResetEvent(false);
            var listener = new SubscribeCallbackExt(
                (o, m) =>
                {
                    if (m is { Message: MockObject { Text: "some_text", Flag: true, Number: 2137 } })
                    {
                        receivedObject.Set();
                    }
                },
                (o, p) => {
                },
                (o, s) => {
                });
            pubnub.AddListener(listener);
            
            var channel = "hello_my_channel";
            
            pubnub.Subscribe<MockObject>().Channels(new []{channel}).Execute();

            await Task.Delay(1000);
            
            await pubnub.Publish().Channel(channel).Message(objectMessage).ExecuteAsync();

            var received = receivedObject.WaitOne(10000);
            Assert.True(received);
            pubnub.Unsubscribe<MockObject>().Channels(new []{channel}).Execute();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenUnencryptPublishPOSTShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;

            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

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
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14715278266153304\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).UsePOST(true)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedPublishMessage)
            {
                Assert.IsTrue(receivedPublishMessage, "Unencrypt Publish Failed");
            }
            else
            {
                receivedPublishMessage = false;

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

                expected = "[[\"Pubnub Messaging API 1\"],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715278266153304")
                        .WithParameter("include_token", "true")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715278266153304 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Execute(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedPublishMessage = true;
                                    historyManualEvent.Set();
                                }));

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful unencrypt Publish");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;

            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            object message = new CustomClass();

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
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
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14715286132003364\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedPublishMessage)
            {
                Assert.IsTrue(receivedPublishMessage, "Unencrypt Publish Failed");
            }
            else
            {
                receivedPublishMessage = false;

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

                expected = "[[{\"foo\":\"hi!\",\"bar\":[1,2,3,4,5]}],14715286132003364,14715286132003364]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715286132003364")
                        .WithParameter("include_token", "true")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715286132003364 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Execute(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedPublishMessage = true;
                                    historyManualEvent.Set();
                                }));

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful unencrypt object Publish");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;

            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            object message = new SecretCustomClass();

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = false
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
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedPublishMessage)
            {
                Assert.IsTrue(receivedPublishMessage, "Encrypt Object Publish Failed");
            }
            else
            {
                receivedPublishMessage = false;

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

                expected = "[[\"nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2\"],14715325858469956,14715325858469956]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715325228931129")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715325228931129 : publishTimetoken)
                    .Count(100)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Execute(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedPublishMessage = true;
                                    historyManualEvent.Set();
                                }));

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful encrypt object Publish");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfoWithSSL()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            object message = new SecretCustomClass();

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = true
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
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedPublishMessage)
            {
                Assert.IsTrue(receivedPublishMessage, "Encrypt Object Publish Failed with SSL");
            }
            else
            {
                receivedPublishMessage = false;

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

                expected = "[[\"nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2\"],14715325858469956,14715325858469956]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715325228931129")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715325228931129 : publishTimetoken)
                    .Count(100)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Execute(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedPublishMessage = true;
                                    historyManualEvent.Set();
                                }));

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful encrypt object Publish with SSL");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForEncryptPublish;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = false
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
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14715426119520817\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22%2BBY5%2FmiAA8aeuhVl4d13Kg%3D%3D%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                            publishManualEvent.Set();
                        }
                        else
                        {
                            publishManualEvent.Set();
                        }
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedPublishMessage)
            {
                Assert.IsTrue(receivedPublishMessage, "Encrypt Publish Failed");
            }
            else
            {
                receivedPublishMessage = false;

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

                expected = "[[\"+BY5/miAA8aeuhVl4d13Kg==\"],14715426119520817,14715426119520817]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715426119520817")
                        .WithParameter("include_token","true")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715426119520817 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Execute(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedPublishMessage = true;
                                    historyManualEvent.Set();
                                }));

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to decrypt the successful Publish");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();
            if (!PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("Ignored due to no secret key at client side");
            }

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForSecretEncryptPublish;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = false
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

            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1, \"Sent\", \"14715438956854374\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22f42pIQcWZ9zbTbH8cyLwB%2FtdvRxjFLOYcBNMVKeHS54%3D%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "tcFpCYsp1uiqyWCZxvdJp7KXEXjyvCFnH6F4UjJ6mds=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedPublishMessage)
            {
                Assert.IsTrue(receivedPublishMessage, "Secret Encrypt Publish Failed");
            }
            else
            {
                receivedPublishMessage = false;

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

                expected = "[[\"f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=\"],14715438956854374,14715438956854374]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715438956854374")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("timestamp", "1356998400")
                        .WithParameter("uuid", config.UserId)
                        .WithParameter("signature", "WyHIBPHildY1gtERK5uDGqX8RyKnrqQFegoOoHizsV4=")
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715438956854374 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Execute(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedPublishMessage = true;
                                    historyManualEvent.Set();
                                }));

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to decrypt the successful Secret key Publish");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        // [Test]
        // public static void ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        // {
        //     server.ClearRequests();
        //
        //     bool receivedPublishMessage = false;
        //     long publishTimetoken = 0;
        //
        //     string channel = "hello_my_channel";
        //     object message = new PubnubDemoObject();
        //
        //     PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
        //     {
        //         PublishKey = PubnubCommon.PublishKey,
        //         SubscribeKey = PubnubCommon.SubscribeKey,
        //         Secure = false
        //     };
        //     if (PubnubCommon.PAMServerSideRun)
        //     {
        //         config.SecretKey = PubnubCommon.SecretKey;
        //     }
        //     else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
        //     {
        //         config.AuthKey = authKey;
        //     }
        //     server.RunOnHttps(false);
        //
        // pubnub = createPubNubInstance(config, authToken);
        //
        //     string expected = "[1, \"Sent\", \"14715459088445832\"]";
        //
        //     server.AddRequest(new Request()
        //             .WithMethod("GET")
        //             .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%7B%22VersionID%22:3.4%2C%22Timetoken%22:%2213601488652764619%22%2C%22OperationName%22:%22Publish%22%2C%22Channels%22:%5B%22ch1%22%5D%2C%22DemoMessage%22:%7B%22DefaultMessage%22:%22~!%40%23%24%25%5E%26*()_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D%5C%5C%20%7B%7D%7C%20asdfghjkl%3B'%20:%5C%22%20zxcvbnm%2C.%2F%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22:%7B%22DefaultMessage%22:%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah!%22%7D%2C%22SampleXml%22:%5B%7B%22ID%22:%22ABCD123%22%2C%22Name%22:%7B%22First%22:%22John%22%2C%22Middle%22:%22P.%22%2C%22Last%22:%22Doe%22%7D%2C%22Address%22:%7B%22Street%22:%22123%20Duck%20Street%22%2C%22City%22:%22New%20City%22%2C%22State%22:%22New%20York%22%2C%22Country%22:%22United%20States%22%7D%7D%2C%7B%22ID%22:%22ABCD456%22%2C%22Name%22:%7B%22First%22:%22Peter%22%2C%22Middle%22:%22Z.%22%2C%22Last%22:%22Smith%22%7D%2C%22Address%22:%7B%22Street%22:%2212%20Hollow%20Street%22%2C%22City%22:%22Philadelphia%22%2C%22State%22:%22Pennsylvania%22%2C%22Country%22:%22United%20States%22%7D%7D%5D%7D", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
        //             .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
        //             .WithParameter("requestid", "myRequestId")
        //             .WithParameter("uuid", config.UserId)
        //             .WithResponse(expected)
        //             .WithStatusCode(System.Net.HttpStatusCode.OK));
        //
        //     manualResetEventWaitTimeout = 310 * 1000;
        //
        //     ManualResetEvent publishManualEvent = new ManualResetEvent(false);
        //     pubnub.Publish().Channel(channel).Message(message)
        //             .Execute(new PNPublishResultExt((r, s) =>
        //             {
        //                 if (r != null && s.StatusCode == 200 && !s.Error)
        //                 {
        //                     publishTimetoken = r.Timetoken;
        //                     receivedPublishMessage = true;
        //                 }
        //                 publishManualEvent.Set();
        //             }));
        //     publishManualEvent.WaitOne(manualResetEventWaitTimeout);
        //
        //     if (!receivedPublishMessage)
        //     {
        //         Assert.IsTrue(receivedPublishMessage, "Complex Object Publish Failed");
        //     }
        //     else
        //     {
        //         receivedPublishMessage = false;
        //
        //         if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
        //
        //         expected = Resource.ComplexMessage;
        //
        //         server.AddRequest(new Request()
        //                 .WithMethod("GET")
        //                 .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
        //                 .WithParameter("count", "100")
        //                 .WithParameter("end", "14715459088445832")
        //                 .WithParameter("include_token", "true")
        //                 .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
        //                 .WithParameter("requestid", "myRequestId")
        //                 .WithParameter("uuid", config.UserId)
        //                 .WithResponse(expected)
        //                 .WithStatusCode(System.Net.HttpStatusCode.OK));
        //
        //         Debug.WriteLine("WhenAMessageIsPublished-ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo - Publish OK. Now checking detailed history");
        //
        //         ManualResetEvent historyManualEvent = new ManualResetEvent(false);
        //
        //         pubnub.History().Channel(channel)
        //             .End(PubnubCommon.EnableStubTest ? 14715459088445832 : publishTimetoken)
        //             .Reverse(false)
        //             .IncludeTimetoken(true)
        //             .Execute(new PNHistoryResultExt(
        //                         (r, s) =>
        //                         {
        //                             Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
        //                             receivedPublishMessage = true;
        //                             historyManualEvent.Set();
        //                         }));
        //
        //         historyManualEvent.WaitOne(manualResetEventWaitTimeout);
        //
        //         Assert.IsTrue(receivedPublishMessage, "Unable to match the successful unencrypt object Publish");
        //     }
        //     pubnub.Destroy();
        //     pubnub.PubnubUnitTest = null;
        //     pubnub = null;
        // }

        [Test]
        public static void ThenPubnubShouldFailOnWithoutSettingUuid()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                PNConfiguration config = new PNConfiguration(new UserId(""))
                {
                    PublishKey = PubnubCommon.PublishKey,
                    SubscribeKey = PubnubCommon.SubscribeKey,
                };

                pubnub = createPubNubInstance(config, authToken);
            });

            pubnub = null;
        }

        [Test]
        public static void ThenPublishKeyShouldNotBeEmpty()
        {
            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = "",
                SubscribeKey = PubnubCommon.SubscribeKey,
            };

            pubnub = createPubNubInstance(config, authToken);

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            Assert.Throws<MissingMemberException>(() =>
            {
                pubnub.Publish().Channel(channel).Message(message)
                        .Execute(new PNPublishResultExt((r, s) => { }));

            });
            pubnub = null;

        }

        [Test]
        public static void ThenOptionalSecretKeyShouldBeProvidedInConfig()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

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

            pubnub = createPubNubInstance(config, authToken);

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            string expected = "[1,\"Sent\",\"14722277738126309\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20API%20Usage%20Example%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "CkHf9ur70OxnkkPvzc9PCPbSbq_SHq2hfYbfDHXh90Q=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish Failed with secret key");
        }

        [Test]
        public static void IfSSLNotProvidedThenDefaultShouldBeTrue()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
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
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Pubnub%20API%20Usage%20Example%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish Failed with no SSL");
        }

        [Test]
        public static void IfSample1SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            string message = " !~`@#$%^&*()+=[]\\{}|;':\",/<>?-_.aA1©®€™₹😜🎉";
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample1SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSample2SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            string message = " !~";
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample2SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSample3SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            string message = "{a:\"!\"}";
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample3SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }


        [Test]
        public static void IfSample4SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            string message = "{a:6}";
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample4SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSample5SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            string message = "!";
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample5SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSample6SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            string message = "~";
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample6SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSample7SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            string message = "Its me (Pandu)";
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample7SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSample8SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            string message = "Its me (Pandu) Ok ç!~:)@";
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample8SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSample9SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            object message = new
            {
                id = "c670e9a9-fe5b-436c-8c0e-785a9201a5ef",
                senderId = "yigit@armut.com",
                quoteId = 2208445,
                shouldImport = false,
                type = "TEXT",
                text = @"
            Merhabalar :)

            Kare ya da çember arka fon tercihinize göre kullanılabilir.

            Jardinyer Takım
            Şamdanlar ve mumlar
            Çember ya da Kare arka fon
            Çiçekli vazo
            Lokumluklar
            Kolonyalık
            Led ışıklar
            Led mumlar
            Maket pasta
            Damat fincanı
            Yüzük tepsisi
            Kıyafet renginize göre hafif süsleme.



            Nakliye Kurulum dahildir.
            Kırılan ya da eksilen malzeme size aittir, ödemesi alınır.

            Çiçek karışık renk ya da beyaz seçilebilir. Ya da mor seçilebilir






            "
            };
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample9SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSample10SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            object message = new
            {
                id = "c670e9a9-fe5b-436c-8c0e-785a9201a5ef",
                senderId = "yigit@armut.com",
                quoteId = 2208445,
                shouldImport = false,
                type = "TEXT Its me (Pandu) Ok ç!~:)@",
                text = @"
            Merhabalar :)

            Kare ya da çember arka fon tercihinize göre kullanılabilir.

            Jardinyer Takım
            Şamdanlar ve mumlar
            Çember ya da Kare arka fon
            Çiçekli vazo
            Lokumluklar
            Kolonyalık
            Led ışıklar
            Led mumlar
            Maket pasta
            Damat fincanı
            Yüzük tepsisi
            Kıyafet renginize göre hafif süsleme.



            Nakliye Kurulum dahildir.
            Kırılan ya da eksilen malzeme size aittir, ödemesi alınır.

            Çiçek karışık renk ya da beyaz seçilebilir. Ya da mor seçilebilir






            "
            };
            bool receivedPublishMessage = SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(message);
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSample10SecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }


        private static bool SampleXSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess(object message)
        {
            server.ClearRequests();
            if (!PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("Ignored due to no secret key at client side");
            }
            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";

            PNConfiguration config = new PNConfiguration(new UserId("myuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                IncludeRequestIdentifier = false,
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

            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22%21%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            Dictionary<string, object> cp = new Dictionary<string, object>();
            cp.Add("seqn", "1");
            pubnub.Publish().Channel(channel).Message(message).QueryParam(cp)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(s.ErrorData));
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            return receivedPublishMessage;
        }

        [Test]
        public static void IfSecretKeyCipherKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess()
        {
            server.ClearRequests();
            if (!PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("Ignored due to no secret key at client side");
            }
            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel ~!@#$%^&()+=[]{}|;\"<>?-_.aA1©®€™₹😜🎉";
            string message = " ~`!@#$%^&*()+=[]\\{}|;':\",/<>?-_.aA1©®€™₹😜🎉";

            PNConfiguration config = new PNConfiguration(new UserId("my ~`!@#$%^&*()+=[]\\{}|;':\",/<>?-_.aA1©®€™₹😜🎉uuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = false,
                IncludeRequestIdentifier = false,
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

            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22%21%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            Dictionary<string, object> cp = new Dictionary<string, object>();
            cp.Add("seqn", "1");
            pubnub.Publish().Channel(channel).Message(message).QueryParam(cp)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(s.ErrorData));
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSecretKeyWithoutAuthThenPostMessageWithSpecialCharsReturnSuccess()
        {
            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel!@#$%^&()+=[]{}|;\"<>?-_.aA1©®€™₹😜🎉";
            //string channel = "hello_my_channel";
            string message = " !~`@#$%^&*()+=[]\\{}|;':\",/<>?-_.aA1©®€™₹😜🎉";
            //string message = " !~";
            //string message = "{a:\"!\"}";
            //string message = "{a:6}";
            //string message = "!";
            //var message = new { a="!" };
            //":";

            PNConfiguration config = new PNConfiguration(new UserId("my ~`!@#$%^&*()+=[]\\{}|;':\",/<>?-_.aA1©®€™₹😜🎉uuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                IncludeRequestIdentifier = false,
            };
            pubnub = createPubNubInstance(config);

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            Dictionary<string, object> cp = new Dictionary<string, object>();
            cp.Add("seqn", "1");
            pubnub.Publish().Channel(channel).Message(message).QueryParam(cp).UsePOST(true)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(s.ErrorData));
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfSecretKeyCipherKeyWithoutAuthThenPostMessageWithSpecialCharsReturnSuccess()
        {
            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel!@#$%^&()+=[]{}|;\"<>?-_.aA1©®€™₹😜🎉";
            string message = " ~`!@#$%^&*()+=[]\\{}|;':\",/<>?-_.aA1©®€™₹😜🎉";

            PNConfiguration config = new PNConfiguration(new UserId("my ~`!@#$%^&*()+=[]\\{}|;':\",/<>?-_.aA1©®€™₹😜🎉uuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                SecretKey = PubnubCommon.SecretKey,
                IncludeRequestIdentifier = false,
            };
            pubnub = createPubNubInstance(config);
            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            Dictionary<string, object> cp = new Dictionary<string, object>();
            cp.Add("seqn", "1");
            pubnub.Publish().Channel(channel).Message(message).QueryParam(cp).UsePOST(true)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(s.ErrorData));
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "FAILED - IfSecretKeyWithoutAuthThenGetMessageWithSpecialCharsReturnSuccess");
        }

        [Test]
        public static void IfMobilePayloadThenPublishReturnSuccess()
        {
            Apns2Data apns2Data = new Apns2Data
            {
                collapseId = "sample collapse id",
                expiration = "xyzexpiration",
                authMethod = APNS2AuthMethod.TOKEN,
                targets = new List<PushTarget>()
                    {
                        new PushTarget()
                        {
                            environment= PubnubApi.Environment.Development,
                            exclude_devices = new List<string>(){ "excl_d1", "excl_d2" },
                            topic = "sample dev topic"
                        },
                        new PushTarget()
                        {
                            environment= PubnubApi.Environment.Production,
                            exclude_devices = new List<string>(){ "excl_d3", "excl_d4" },
                            topic = "sample prod topic"
                        }
                    }
            };

            Dictionary<PNPushType, Dictionary<string, object>> pushTypeCustomData = new Dictionary<PNPushType, Dictionary<string, object>>();
            pushTypeCustomData.Add(PNPushType.APNS2, new Dictionary<string, object>
                                {
                                    {"teams", new string[] { "49ers", "raiders" } },
                                    {"score", new int[] { 7, 0 } }
                                });
            pushTypeCustomData.Add(PNPushType.FCM, new Dictionary<string, object>
                                {
                                    {"teams", new string[] { "49ers", "raiders" } },
                                    {"score", new int[] { 7, 0 } },
                                    {"lastplay", "5yd run up the middle" }
                                });

            Dictionary<string, object> payload =
                new MobilePushHelper()
                .PushTypeSupport(new PNPushType[] { PNPushType.APNS2, PNPushType.FCM })
                .Title("Game update 49ers touchdown")
                .Badge(2)
                .Apns2Data(new List<Apns2Data>() { apns2Data })
                .Custom(pushTypeCustomData)
                .GetPayload();

            PNConfiguration config = new PNConfiguration(new UserId("testuuid"));
            pubnub = new Pubnub(config);
            System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(payload));

            Assert.IsTrue(payload != null, "FAILED - IfMobilePayloadThenPublishReturnSuccess");
            Assert.True(payload.ContainsKey("pn_apns"), "FAILED - Push Payload should contain apns key");
            Assert.True(payload.ContainsKey("pn_fcm"), "FAILED - Push Payload should contain fcm key");
            Assert.False(payload.ContainsKey("pn_gcm"), "FAILED - Push Payload should NOT contain gcm key");
        }

        [Test]
        public static void ThenPublishWithTtlShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            // The request should include a ttl parameter
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("ttl", "24")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Ttl(24)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish with TTL Failed");
        }

        [Test]
        public static void ThenPublishWithShouldStoreFalseShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            // The request should include a store parameter with value 0
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("store", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).ShouldStore(false)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish with ShouldStore false Failed");
        }

        [Test]
        public static void ThenPublishWithMetaShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;
            Dictionary<string, object> metaData = new Dictionary<string, object>
            {
                { "sender", "unit-test" },
                { "timestamp", 12345 }
            };

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            // The request should include a meta parameter
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("meta", "%7B%22sender%22%3A%22unit-test%22%2C%22timestamp%22%3A12345%7D")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Meta(metaData)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish with Meta Failed");
        }
        
        [Test]
        public static async Task ThenPublishWithMetaCallbackShouldHaveData()
        {
            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;
            Dictionary<string, object> metaData = new Dictionary<string, object>
            {
                { "sender", "unit-test" },
                { "timestamp", 12345 }
            };

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            pubnub = createPubNubInstance(config, authToken);
            
            PNMessageResult<object> messageResult = null;
            var callbackReset = new ManualResetEvent(false);
            SubscribeCallbackExt listener = new SubscribeCallbackExt(
                delegate(Pubnub pn, PNMessageResult<object> m)
                {
                    messageResult = m;
                    callbackReset.Set();
                }, delegate(Pubnub pn, PNStatus status)
                {
                });

            pubnub.AddListener(listener);
            pubnub.Subscribe<object>().Channels(new []{channel}).Execute();

            await Task.Delay(3000);
            
            var result = await pubnub.Publish().Channel(channel).Message(message).Meta(metaData)
                .ExecuteAsync();

            if (result.Status.Error)
            {
                Assert.Fail($"Publish with meta failed with error: {result.Status.ErrorData.Information}");
            }
            
            manualResetEventWaitTimeout = 310 * 1000;
            var gotCallback = callbackReset.WaitOne(manualResetEventWaitTimeout);

            Assert.IsTrue(gotCallback, "Never received message callback for publish with metadata");
            if (gotCallback)
            {
                Assert.IsNotNull(messageResult.UserMetadata, "PNMessageResult.UserMetadata should not be null");
                Assert.IsTrue(messageResult.UserMetadata is Dictionary<string, object>, "PNMessageResult.UserMetadata should be a Dictionary<string, object>");
                Assert.IsTrue(messageResult.UserMetadata.ContainsKey("sender"), "PNMessageResult.UserMetadata doesn't contain expected key");
                Assert.IsTrue(messageResult.UserMetadata["sender"].ToString() == "unit-test", "PNMessageResult.UserMetadata has unexpected value");
                Assert.IsTrue(messageResult.UserMetadata.ContainsKey("timestamp"), "PNMessageResult.UserMetadata doesn't contain expected key");
                Assert.IsTrue(Convert.ToInt32(messageResult.UserMetadata["timestamp"]) == 12345, "PNMessageResult.UserMetadata has unexpected value");
            }
            
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenPublishWithCustomMessageTypeShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;
            string customType = "custom-type";

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            // The request should include a custom_message_type parameter
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("custom_message_type", customType)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).CustomMessageType(customType)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish with CustomMessageType Failed");
        }

        [Test]
        public static void ThenPublishWithAllOptionsShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;
            string customType = "notification";
            Dictionary<string, object> metaData = new Dictionary<string, object>
            {
                { "sender", "unit-test" },
                { "priority", "high" }
            };

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            // The request should include all parameters
            server.AddRequest(new Request()
                    .WithMethod("POST")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("store", "0")
                    .WithParameter("ttl", "10")
                    .WithParameter("meta", "%7B%22sender%22%3A%22unit-test%22%2C%22priority%22%3A%22high%22%7D")
                    .WithParameter("custom_message_type", customType)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .ShouldStore(false)
                    .Ttl(10)
                    .Meta(metaData)
                    .CustomMessageType(customType)
                    .UsePOST(true)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish with all options Failed");
        }

        [Test]
        public static void ThenPublishWithCustomQueryParamsShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;
            Dictionary<string, object> queryParams = new Dictionary<string, object>
            {
                { "custom_param", "custom_value" },
                { "numeric_param", 42 }
            };

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            // The request should include the custom query parameters
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("custom_param", "custom_value")
                    .WithParameter("numeric_param", "42")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).QueryParam(queryParams)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedPublishMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish with custom query parameters Failed");
        }

        [Test]
#if NET40
        public static void ThenExecuteAsyncWithAllParametersShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenExecuteAsyncWithAllParametersShouldReturnSuccessCodeAndInfo()
#endif
        {
            server.ClearRequests();

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;
            string customType = "notification";
            Dictionary<string, object> metaData = new Dictionary<string, object>
            {
                { "sender", "unit-test" },
                { "priority", "high" }
            };
            Dictionary<string, object> queryParams = new Dictionary<string, object>
            {
                { "custom_param", "custom_value" }
            };

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
            else if (!string.IsNullOrEmpty(authToken) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authToken;
            }
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            // The request should include all parameters
            server.AddRequest(new Request()
                    .WithMethod("POST")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("store", "0")
                    .WithParameter("ttl", "10")
                    .WithParameter("meta", "%7B%22sender%22%3A%22unit-test%22%2C%22priority%22%3A%22high%22%7D")
                    .WithParameter("custom_message_type", customType)
                    .WithParameter("custom_param", "custom_value")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNPublishResult> result = Task.Factory.StartNew(async () => await pubnub.Publish().Channel(channel).Message(message)
                .ShouldStore(false)
                .Ttl(10)
                .Meta(metaData)
                .CustomMessageType(customType)
                .QueryParam(queryParams)
                .UsePOST(true)
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNPublishResult> result = await pubnub.Publish().Channel(channel).Message(message)
                .ShouldStore(false)
                .Ttl(10)
                .Meta(metaData)
                .CustomMessageType(customType)
                .QueryParam(queryParams)
                .UsePOST(true)
                .ExecuteAsync();
#endif

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.Result, "Result.Result should not be null");
            Assert.AreEqual(200, result.Status.StatusCode, "StatusCode should be 200");
            Assert.IsFalse(result.Status.Error, "Error should be false");
        }

        [Test]
        public static void ThenEmptyChannelNameShouldThrowException()
        {
            server.ClearRequests();

            string channel = "";
            string message = messageForUnencryptPublish;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            Assert.Throws<ArgumentException>(() =>
            {
                pubnub.Publish()
                      .Channel(channel)
                      .Message(message)
                      .Execute(new PNPublishResultExt((r, s) => { }));
            });

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenNullCallbackShouldThrowException()
        {
            server.ClearRequests();

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config, authToken);

            Assert.Throws<ArgumentException>(() =>
            {
                pubnub.Publish()
                      .Channel(channel)
                      .Message(message)
                      .Execute(null);
            });

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenPublishWithCustomMessageTypeAndSubscribeShouldReceiveCorrectMessageType()
        {
            var randomiser = new Random();
            string channel = $"hello_my_channel_{randomiser.Next(1000, 10000)}";
            string message = $"some_message_{randomiser.Next(1000, 10000)}";
            string customType = "customtype";

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
            };
            pubnub = createPubNubInstance(config);

            manualResetEventWaitTimeout = 310 * 1000;

            // Subscribe to the channel
            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>()
                .Channels(new string[] { channel })
                .WithPresence()
                .Execute();

            // Add message listener
            string receivedCustomMessageType = null;
            pubnub.AddListener(new SubscribeCallbackExt(
                (p, m) => {
                    if (m.Channel.Equals(channel) && m.Message.ToString().Equals(message))
                    {
                        receivedCustomMessageType = m.CustomMessageType;
                        subscribeManualEvent.Set();
                    }
                },
                (p, pnPresenceEventResult) => { },
                (p, pnSignalResult) => { },
                (p, pnObjectEventResult) => { },
                (p, pnMessageActionEventResult) => { },
                (p, pnFileEventResult) => { },
                (p, pnStatus) => { }
            ));

            // Publish the message
            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).CustomMessageType(customType)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishManualEvent.Set();
                        }
                        
                    }));
            var receivedPublishMessage = publishManualEvent.WaitOne(manualResetEventWaitTimeout);
            Assert.IsTrue(receivedPublishMessage, "Publish with CustomMessageType Failed");
            
            // Wait for the subscribe message
            var receivedSubscribeMessage = subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
            Assert.IsTrue(receivedSubscribeMessage, "Subscribe message not received");
            Assert.AreEqual(customType, receivedCustomMessageType, "Custom message type mismatch");
            
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
        
        [Test]
        public static async Task ThenAwaitedPublishPostWithSecretKeySuccess()
        {
            var id = $"test_{new Random().Next(1000, 10000)}";
            string channel = $"channel_{id}";
            string message =$"message_{id}";
            string customType = $"customtype_{id}";

            PNConfiguration config = new PNConfiguration(new UserId($"user_{id}"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };
            pubnub = createPubNubInstance(config, authToken);
            manualResetEventWaitTimeout = 310 * 1000;
            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>()
                .Channels(new string[] { channel })
                .WithPresence()
                .Execute();
            string receivedMessage = null;
            pubnub.AddListener(new SubscribeCallbackExt(
                (p, m) => {
                    if (m.Channel.Equals(channel) )
                    {
                        receivedMessage = $"{m.Message}";
                        subscribeManualEvent.Set();
                    }
                },
                (p, pnPresenceEventResult) => { },
                (p, pnSignalResult) => { },
                (p, pnObjectEventResult) => { },
                (p, pnMessageActionEventResult) => { },
                (p, pnFileEventResult) => { },
                (p, pnStatus) => { }
            ));

            // Publish the message
            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
             await pubnub.Publish().Channel(channel).Message(message).UsePOST(true).CustomMessageType(customType)
                .ExecuteAsync();
            var receivedSubscribeMessage = subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
            Assert.IsTrue(receivedSubscribeMessage, "Subscribe message not received");
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
        
        [Test]
        public static async Task ThenAwaitedPublishPostWithTokenSuccess()
        {
            var id = $"test_{new Random().Next(1000, 10000)}";
            string channel = $"channel_{id}";
            string message =$"message_{id}";
            string customType = $"customtype_{id}";

            PNConfiguration config = new PNConfiguration(new UserId($"user_{id}"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };
            pubnub = createPubNubInstance(config, authToken);
            PNResult<PNAccessManagerTokenResult> grantTokenResponse = await pubnub.GrantToken()
                .TTL(15)
                .AuthorizedUuid($"user_{id}")
                .Resources(new PNTokenResources
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>
                    {
                        { channel, new PNTokenAuthValues { Read = true, Write = true, Join = true, Manage = true} }
                    }
                })
                .ExecuteAsync();
            var token =  grantTokenResponse.Result.Token;
            Assert.NotNull(token, "token should not be null for publisher");
            // wait after grant token to be effective
            await Task.Delay(1000);
            PNConfiguration publsherConfiguration = new PNConfiguration(new UserId($"user_{id}"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            var publisher = createPubNubInstance(publsherConfiguration, token);
            manualResetEventWaitTimeout = 310 * 1000;
            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>()
                .Channels(new string[] { channel })
                .WithPresence()
                .Execute();
            string receivedMessage = null;
            pubnub.AddListener(new SubscribeCallbackExt(
                (p, m) => {
                    if (m.Channel.Equals(channel) )
                    {
                        receivedMessage = $"{m.Message}";
                        subscribeManualEvent.Set();
                    }
                },
                (p, pnPresenceEventResult) => { },
                (p, pnSignalResult) => { },
                (p, pnObjectEventResult) => { },
                (p, pnMessageActionEventResult) => { },
                (p, pnFileEventResult) => { },
                (p, pnStatus) => { }
            ));

            // Publish the message
            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
             await publisher.Publish().Channel(channel).Message(message).UsePOST(true).CustomMessageType(customType)
                .ExecuteAsync();
            var receivedSubscribeMessage = subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
            Assert.IsTrue(receivedSubscribeMessage, "Subscribe message not received");
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            publisher.Destroy();
            publisher.PubnubUnitTest = null;
            publisher = null;
        }
    }

    /// <summary>
    /// Tests for the v2/publish endpoint selection logic based on message payload size.
    /// Verifies that the SDK correctly chooses between the regular publish endpoint and
    /// the v2/publish endpoint depending on serialized message size and the UsePOST flag.
    ///
    /// Boundary rules under test:
    /// <list type="bullet">
    ///   <item>Regular publish (GET): message in URL path, total URL must stay under 32 KB.</item>
    ///   <item>Regular publish (POST): body must be less than 32*1024 − 2 = 32 766 bytes.</item>
    ///   <item>v2/publish (POST): used when regular limits are exceeded; server rejects bodies ≥ 2*1024*1024 − 2 bytes with 413.</item>
    ///   <item>Client-side validation: prepared message content (including encryption overhead)
    ///         exceeding 2*1024*1024 − 2 bytes throws <see cref="ArgumentException"/> before
    ///         any HTTP request is made.</item>
    /// </list>
    /// </summary>
    [TestFixture]
    public class WhenLargeMessageIsPublished
    {
        private WireMockServer _server;
        private Pubnub _pubnub;

        private const string Channel = "test_channel";
        private const string PubKey = "demo";
        private const string SubKey = "demo";
        private const string PublishSuccessResponse = "[1,\"Sent\",\"14715278266153304\"]";
        private const string Publish413Response =
            "{\"status\":413,\"service\":\"Balancer\",\"error\":true,\"message\":\"Request Entity Too Large\"}";

        /// <summary>
        /// POST body boundary for the regular publish endpoint.
        /// Matches <c>PublishOperation.MaxPublishRequestSizeBytes − PostBodyFramingOverheadBytes</c>.
        /// At or above this size the SDK must switch to v2/publish.
        /// </summary>
        private const int PostBodyBoundaryBytes = 32 * 1024 - 2; // 32 766

        /// <summary>
        /// Maximum permitted size for the prepared message content (2 MiB − 2 bytes).
        /// The SDK throws <see cref="ArgumentException"/> client-side for payloads
        /// strictly exceeding this value. Payloads exactly at this size are sent to
        /// the server, which returns HTTP 413.
        /// </summary>
        private const int TwoMbBoundaryBytes = 2 * 1024 * 1024 - 2; // 2 097 150

        [SetUp]
        public void Setup()
        {
            _server = WireMockServer.Start();
            SetupDefaultPublishSuccessMock();
        }

        [TearDown]
        public void TearDown()
        {
            _pubnub?.Destroy();
            _server?.Stop();
            _server?.Dispose();
        }

        /// <summary>
        /// Registers a catch-all mock that responds with a standard publish success
        /// regardless of path or HTTP method. Individual tests override as needed.
        /// </summary>
        private void SetupDefaultPublishSuccessMock()
        {
            _server
                .Given(WireMockRequest.Create().UsingAnyMethod())
                .RespondWith(WireMockResponse.Create()
                    .WithStatusCode(200)
                    .WithBody(PublishSuccessResponse));
        }

        private Pubnub CreatePubnub()
        {
            var config = new PNConfiguration(new UserId("test-uuid"))
            {
                PublishKey = PubKey,
                SubscribeKey = SubKey,
                Origin = $"localhost:{_server.Port}",
                Secure = false
            };
            _pubnub = new Pubnub(config);
            return _pubnub;
        }

        /// <summary>
        /// Creates a Pubnub instance with CryptoModule configured using LegacyCryptor.
        /// Encryption adds overhead (AES-CBC IV, PKCS7 padding, Base64 encoding, JSON wrapping),
        /// so the final serialized payload will be larger than the original plaintext.
        /// </summary>
        private Pubnub CreatePubnubWithCrypto()
        {
            var config = new PNConfiguration(new UserId("test-uuid"))
            {
                PublishKey = PubKey,
                SubscribeKey = SubKey,
                Origin = $"localhost:{_server.Port}",
                Secure = false,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null)
            };
            _pubnub = new Pubnub(config);
            return _pubnub;
        }

        /// <summary>
        /// Creates a plain ASCII string that, when JSON-serialized by Newtonsoft.Json,
        /// produces exactly <paramref name="targetSerializedBytes"/> bytes.
        /// JSON serialization of a simple string adds 2 bytes for the surrounding double-quotes.
        /// </summary>
        private static string CreateMessageOfSerializedSize(int targetSerializedBytes)
        {
            // 'a' is a single UTF-8 byte that requires no JSON escaping.
            // Serialized form: "aaa...a" → (targetSerializedBytes − 2) chars + 2 quote bytes.
            return new string('a', targetSerializedBytes - 2);
        }

        /// <summary>
        /// Expected URL path prefix for the regular publish endpoint.
        /// Format: <c>/publish/{pubKey}/{subKey}/0/{channel}/0</c>.
        /// For GET, the encoded message is appended after this prefix.
        /// </summary>
        private static readonly string RegularPublishPathBase =
            $"/publish/{PubKey}/{SubKey}/0/{Channel}/0";

        /// <summary>
        /// Expected URL path for the v2/publish endpoint (no message in path).
        /// Format: <c>/v2/publish/{pubKey}/{subKey}/0/{channel}/0</c>.
        /// </summary>
        private static readonly string V2PublishPath =
            $"/v2/publish/{PubKey}/{SubKey}/0/{Channel}/0";

        /// <summary>
        /// Asserts that the last HTTP request used the regular publish endpoint
        /// with the expected method, correct path structure, and no v2 prefix.
        /// </summary>
        private void AssertRegularPublishEndpoint(string expectedMethod)
        {
            var entry = _server.LogEntries.Last();
            Assert.That(entry.RequestMessage.Method, Is.EqualTo(expectedMethod),
                $"Expected HTTP method {expectedMethod}.");
            Assert.That(entry.RequestMessage.Path, Does.StartWith(RegularPublishPathBase),
                $"Path should follow the regular publish structure: {RegularPublishPathBase}");
            Assert.That(entry.RequestMessage.Path, Does.Not.StartWith("/v2/"),
                "Should NOT use the /v2/publish/ endpoint.");
        }

        /// <summary>
        /// Asserts that the last HTTP request used the v2/publish endpoint
        /// with POST method and the correct path structure.
        /// </summary>
        private void AssertV2PublishEndpoint()
        {
            var entry = _server.LogEntries.Last();
            Assert.That(entry.RequestMessage.Method, Is.EqualTo("POST"),
                "v2/publish always uses POST.");
            Assert.That(entry.RequestMessage.Path, Is.EqualTo(V2PublishPath),
                $"Path should exactly match the v2/publish structure: {V2PublishPath}");
        }

        #region Small payload — regular publish endpoint respects UsePOST flag

        [Test]
        public async Task ThenSmallPayload_GetMode_UsesRegularPublishGetEndpoint()
        {
            // Arrange
            var pubnub = CreatePubnub();

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message("Hello")
                .ExecuteAsync();

            // Assert — publish succeeded
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False, "Publish should succeed.");
            Assert.That(result.Result.Timetoken, Is.GreaterThan(0), "Timetoken should be set.");
            AssertRegularPublishEndpoint("GET");

            // Assert — for GET, message is in the URL path, not in the body
            var entry = _server.LogEntries.Last();
            Assert.That(entry.RequestMessage.Path, Does.Contain("Hello"),
                "GET publish should include the message in the URL path.");
            Assert.That(entry.RequestMessage.Body, Is.Null.Or.Empty,
                "GET publish should not have a request body.");
        }

        [Test]
        public async Task ThenSmallPayload_PostMode_UsesRegularPublishPostEndpoint()
        {
            // Arrange
            var pubnub = CreatePubnub();

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message("Hello")
                .UsePOST(true)
                .ExecuteAsync();

            // Assert — publish succeeded
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False, "Publish should succeed.");
            Assert.That(result.Result.Timetoken, Is.GreaterThan(0), "Timetoken should be set.");
            AssertRegularPublishEndpoint("POST");

            // Assert — for POST, message is in the body and the path ends at /0
            var entry = _server.LogEntries.Last();
            Assert.That(entry.RequestMessage.Body, Does.Contain("Hello"),
                "POST publish should include the message in the request body.");
            Assert.That(entry.RequestMessage.Path, Is.EqualTo(RegularPublishPathBase),
                "Regular POST path should not contain the message.");
        }

        #endregion

        #region 32 KB POST-body boundary — v2/publish activation

        [Test]
        public async Task ThenPostBody_MoreThanBoundary_32766Bytes_UsesV2PublishEndpoint()
        {
            // Arrange — serialized message is more than at the 32 766-byte boundary
            var pubnub = CreatePubnub();
            var message = CreateMessageOfSerializedSize(PostBodyBoundaryBytes +1);

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(message)
                .UsePOST(true)
                .ExecuteAsync();

            // Assert
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False, "Publish should succeed.");
            AssertV2PublishEndpoint();
        }
        
        [Test]
        public async Task ThenPostBody_ExactlyAtBoundary_32766Bytes_UsesV2PublishEndpoint()
        {
            // Arrange — serialized message is exactly at the 32 766-byte boundary
            var pubnub = CreatePubnub();
            var message = CreateMessageOfSerializedSize(PostBodyBoundaryBytes);

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(message)
                .UsePOST(true)
                .ExecuteAsync();

            // Assert
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False, "Publish should succeed.");
            AssertRegularPublishEndpoint("POST");
        }

        [Test]
        public async Task ThenPostBody_OneBelowBoundary_32765Bytes_UsesRegularPublishEndpoint()
        {
            // Arrange — serialized message is one byte below the boundary
            var pubnub = CreatePubnub();
            var message = CreateMessageOfSerializedSize(PostBodyBoundaryBytes - 1);

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(message)
                .UsePOST(true)
                .ExecuteAsync();

            // Assert
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False, "Publish should succeed.");
            AssertRegularPublishEndpoint("POST");
        }

        #endregion

        #region Large payload — v2/publish forced regardless of UsePOST flag

        [Test]
        public async Task ThenLargePayload_GetMode_FallsBackToV2PublishPostEndpoint()
        {
            // Arrange — 40 KB message, well above the 32 KB URL limit; UsePOST not set
            var pubnub = CreatePubnub();
            var message = CreateMessageOfSerializedSize(40_000);

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(message)
                .ExecuteAsync();

            // Assert — SDK should auto-switch to v2/publish with POST
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False, "Publish should succeed.");
            AssertV2PublishEndpoint();
        }

        [Test]
        public async Task ThenLargePayload_PostMode_AboveBoundary_UsesV2PublishEndpoint()
        {
            // Arrange — 100 bytes above the POST-body boundary, with UsePOST(true)
            var pubnub = CreatePubnub();
            var message = CreateMessageOfSerializedSize(PostBodyBoundaryBytes + 100);

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(message)
                .UsePOST(true)
                .ExecuteAsync();

            // Assert
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False, "Publish should succeed.");
            AssertV2PublishEndpoint();
        }

        [Test]
        public async Task ThenLargePayload_ExplicitGetMode_StillForcedToV2PublishPost()
        {
            // Arrange — explicitly set UsePOST(false) with a large message.
            // The SDK must override this and use v2/publish with POST.
            var pubnub = CreatePubnub();
            var message = CreateMessageOfSerializedSize(40_000);

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(message)
                .UsePOST(false)
                .ExecuteAsync();

            // Assert — UsePOST(false) should be overridden for large payloads
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False, "Publish should succeed.");
            AssertV2PublishEndpoint();

            // Assert — message is in the body, not in the URL path
            var entry = _server.LogEntries.Last();
            Assert.That(entry.RequestMessage.Body, Is.Not.Null.And.Not.Empty,
                "v2/publish should carry the message in the request body.");
        }

        #endregion

        #region Non-string payload — JSON object endpoint selection

        [Test]
        public async Task ThenLargeJsonObjectPayload_GetMode_UsesV2PublishEndpoint()
        {
            // Arrange — a large dictionary that serializes to > 32 KB of JSON
            var pubnub = CreatePubnub();
            var largeObject = new Dictionary<string, string>();
            for (int i = 0; i < 500; i++)
            {
                largeObject[$"key_{i:D4}"] = new string('x', 100);
            }

            // Act — no UsePOST, so GET mode is attempted first
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(largeObject)
                .ExecuteAsync();

            // Assert — large JSON object should trigger v2/publish with POST
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False, "Publish should succeed.");
            AssertV2PublishEndpoint();
        }

        #endregion

        #region 2 MB boundary — client-side ArgumentException and server-side HTTP 413

        [Test]
        public async Task ThenPayload_AtTwoMBBoundary_UsesV2PublishAndServerReturns413()
        {
            // Arrange — message is exactly at TwoMbBoundaryBytes (2,097,150 bytes).
            // This passes the client-side validation (> check, not >=) but the server
            // rejects it with 413.
            _server.Reset();
            _server
                .Given(WireMockRequest.Create()
                    .WithPath(new WildcardMatcher("/v2/publish/*"))
                    .UsingPost())
                .RespondWith(WireMockResponse.Create()
                    .WithStatusCode(413)
                    .WithBody(Publish413Response));

            var pubnub = CreatePubnub();
            var message = CreateMessageOfSerializedSize(TwoMbBoundaryBytes);

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(message)
                .UsePOST(true)
                .ExecuteAsync();

            // Assert — verify the request went to v2/publish
            Assert.That(_server.LogEntries.Count(), Is.GreaterThanOrEqualTo(1),
                "At least one request should have been made.");
            var entry = _server.LogEntries.Last();
            Assert.That(entry.RequestMessage.Method, Is.EqualTo("POST"));
            Assert.That(entry.RequestMessage.Path, Does.StartWith("/v2/publish/"),
                "Payload at the 2 MB boundary should use v2/publish.");

            // Assert — verify the 413 error was surfaced through the SDK
            Assert.That(result.Result, Is.Null,
                "Publish should not return a result when the server rejects with 413.");
            Assert.That(result.Status, Is.Not.Null, "Status should always be set.");
            Assert.That(result.Status.Error, Is.True,
                "Server 413 rejection should be reported as an error.");
        }

        [Test]
        public async Task ThenPayload_JustBelow2MBBoundary_UsesV2PublishAndSucceeds()
        {
            // Arrange — one byte below the 2 MB boundary; default success mock handles it
            var pubnub = CreatePubnub();
            var message = CreateMessageOfSerializedSize(TwoMbBoundaryBytes - 1);

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(message)
                .UsePOST(true)
                .ExecuteAsync();

            // Assert
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False,
                "Payload one byte below the 2 MB boundary should succeed.");
            AssertV2PublishEndpoint();
        }

        [Test]
        public void ThenPayload_AboveTwoMBBoundary_ThrowsArgumentException()
        {
            // Arrange — message is one byte above the max permitted content size.
            // The SDK must throw ArgumentException before making any HTTP request.
            var pubnub = CreatePubnub();
            var message = CreateMessageOfSerializedSize(TwoMbBoundaryBytes + 4);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await pubnub.Publish()
                    .Channel(Channel)
                    .Message(message)
                    .UsePOST(true)
                    .ExecuteAsync();
            });

            Assert.That(ex!.Message, Does.Contain("Message content size exceeds"),
                "Exception message should describe the size violation.");
            Assert.That(_server.LogEntries.Count(), Is.EqualTo(0),
                "No HTTP request should be made when the message exceeds the size limit.");
        }

        #endregion

        #region 2 MB server-side boundary with CryptoModule — encrypted payload near limit

        [Test]
        public async Task ThenEncryptedPayload_JustBelowTwoMBBoundary_WithCryptoModule_UsesV2PublishAndSucceeds()
        {
            // Arrange
            //
            // When CryptoModule is configured, the SDK encrypts the message before publishing.
            // The encryption pipeline in PrepareContent is:
            //   1. JSON-serialize the plaintext         → adds 2 bytes (surrounding quotes)
            //   2. AES-256-CBC encrypt with PKCS7 pad   → rounds up to next 16-byte block
            //   3. Prepend 16-byte random IV             → +16 bytes
            //   4. Base64-encode the encrypted bytes     → ~33 % expansion (4/3 ratio)
            //   5. JSON-serialize the Base64 string      → adds 2 bytes (surrounding quotes)
            //
            // For a plaintext of 1,572,829 ASCII characters the sizes are:
            //   Step 1 → 1,572,831 bytes   (1,572,829 + 2)
            //   Step 2 → 1,572,832 bytes   (1,572,831 + 1 byte PKCS7 padding)
            //   Step 3 → 1,572,848 bytes   (1,572,832 + 16)
            //   Step 4 → 2,097,132 bytes   (ceil(1,572,848 / 3) × 4 = 524,283 × 4)
            //   Step 5 → 2,097,134 bytes   (2,097,132 + 2)
            //
            // 2,097,134 < 2,097,150 (TwoMbBoundaryBytes), so this payload fits just under
            // the 2 MB − 2 server-side limit.  Due to AES block alignment and Base64 step
            // quantization, this is the largest achievable encrypted payload below the limit.
            const int plaintextLength = 1_572_829;

            var pubnub = CreatePubnubWithCrypto();
            var message = new string('a', plaintextLength);

            // Act
            var result = await pubnub.Publish()
                .Channel(Channel)
                .Message(message)
                .UsePOST(true)
                .ExecuteAsync();

            // Assert — publish succeeded via v2/publish
            Assert.That(result.Result, Is.Not.Null, "Publish result should not be null.");
            Assert.That(result.Status.Error, Is.False,
                "Encrypted payload just below the 2 MB − 2 boundary should publish successfully.");
            AssertV2PublishEndpoint();

            // Assert — verify the message body is encrypted (no plaintext leakage)
            var entry = _server.LogEntries.Last();
            var bodyString = entry.RequestMessage.Body;
            Assert.That(bodyString, Is.Not.Null.And.Not.Empty,
                "v2/publish POST body should contain the encrypted message.");
            Assert.That(bodyString, Does.Not.Contain(new string('a', 100)),
                "POST body should contain encrypted data, not the original plaintext.");

            // Assert — verify the encrypted payload is under the 2 MB − 2 server limit
            Assert.That(System.Text.Encoding.UTF8.GetByteCount(bodyString!),
                Is.LessThan(TwoMbBoundaryBytes),
                "Encrypted payload byte size should be below the 2 MB − 2 server-side limit.");
        }

        [Test]
        public void ThenEncryptedPayload_AboveTwoMBBoundary_WithCryptoModule_ThrowsArgumentException()
        {
            // Arrange
            //
            // A plaintext of 1,572,830 ASCII chars encrypts to 2,097,154 bytes — 4 bytes
            // above MaxMessageContentSizeBytes (2,097,150).  The client-side validation
            // must reject this before any HTTP request is made.
            //
            // Encryption size breakdown for 1,572,830 chars:
            //   JSON serialize       → 1,572,832 bytes  (plaintext + 2 quote bytes)
            //   AES-CBC + PKCS7 pad  → 1,572,848 bytes  (1,572,832 is a 16-byte multiple,
            //                                             so PKCS7 appends a full 16-byte block)
            //   Prepend 16-byte IV   → 1,572,864 bytes
            //   Base64 encode        → 2,097,152 bytes  (1,572,864 / 3 × 4 = 524,288 × 4)
            //   JSON wrap (quotes)   → 2,097,154 bytes  (> 2,097,150 = TwoMbBoundaryBytes)
            const int plaintextLength = 1_572_830;

            var pubnub = CreatePubnubWithCrypto();
            var message = new string('a', plaintextLength);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await pubnub.Publish()
                    .Channel(Channel)
                    .Message(message)
                    .UsePOST(true)
                    .ExecuteAsync();
            });

            Assert.That(ex!.Message, Does.Contain("Message content size exceeds"),
                "Exception message should describe the size violation.");
            Assert.That(_server.LogEntries.Count(), Is.EqualTo(0),
                "No HTTP request should be made when the encrypted message exceeds the size limit.");
        }

        #endregion

        #region Publish 2 MB size message using prod server 

        [Test]
        public async Task Then_2MB_Size_Message_Published_to_Prod_server()
        {
            // Arrange — message is one byte above the max permitted content size.
            // The SDK must throw ArgumentException before making any HTTP request.
            var pubnub = new Pubnub(new PNConfiguration(new UserId("test-user"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey
            });
            var message = CreateMessageOfSerializedSize(TwoMbBoundaryBytes - 4);
            var publishResponseFor2MbMessage = await pubnub.Publish()
                    .Channel(Channel)
                    .Message(message)
                    .ExecuteAsync();

            Assert.That(publishResponseFor2MbMessage.Status.Error, Is.False ,
                "2MB size message publish response should not have error");
            Assert.That(publishResponseFor2MbMessage.Result, Is.Not.Null,
                "2MB size message publish result should not be null.");
            Assert.That(publishResponseFor2MbMessage.Result.Timetoken, Is.Not.Null, 
                "2MB size message publish response should contain valid time token");
        }

        #endregion
    }
}
