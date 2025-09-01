using System;
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

            string channel = "hello_my_channel ~!@#$%^&()+=[]{}|;\"<>?-_.aA1©®€™₹😜🎉";
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
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config, authToken);

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

            string channel = "hello_my_channel ~!@#$%^&()+=[]{}|;\"<>?-_.aA1©®€™₹😜🎉";
            string message = " ~`!@#$%^&*()+=[]\\{}|;':\",/<>?-_.aA1©®€™₹😜🎉";

            PNConfiguration config = new PNConfiguration(new UserId("my ~`!@#$%^&*()+=[]\\{}|;':\",/<>?-_.aA1©®€™₹😜🎉uuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                SecretKey = PubnubCommon.SecretKey,
                IncludeRequestIdentifier = false,
            };
            pubnub = createPubNubInstance(config, authToken);
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
            pushTypeCustomData.Add(PNPushType.MPNS, new Dictionary<string, object> 
                                {
                                    {"type", "flip" },
                                    {"back_title", "Back Tile" },
                                    {"back_content", "Back message" }
                                });

            Dictionary<string, object> payload =
                new MobilePushHelper()
                .PushTypeSupport(new PNPushType[] { PNPushType.APNS2, PNPushType.FCM, PNPushType.MPNS })
                .Title("Game update 49ers touchdown")
                .Badge(2)
                .Apns2Data(new List<Apns2Data>() { apns2Data })
                .Custom(pushTypeCustomData)
                .GetPayload();

            PNConfiguration config = new PNConfiguration(new UserId("testuuid"));
            pubnub = new Pubnub(config);
            System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(payload));

            Assert.IsTrue(payload != null, "FAILED - IfMobilePayloadThenPublishReturnSuccess");
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
            string channel = "hello_my_channel";
            string message = "some_message_lalala";
            string customType = "customtype";

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
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
}
