using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using PubnubApi.Tests;

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

        [TestFixtureSetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();

            if (!PubnubCommon.PAMEnabled) return;

            bool receivedGrantMessage = false;
            string channel = "hello_my_channel";
            string authKey = "myAuth";

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

            ManualResetEvent grantManualEvent = new ManualResetEvent(false);
            pubnub.Grant().Channels(new [] { channel }).AuthKeys(new [] { authKey }).Read(true).Write(true).Manage(true).TTL(20)
                .Async(new PNAccessManagerGrantResultExt(
                                (r, s) =>
                                {
                                    try
                                    {
                                        Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                                        if (r != null)
                                        {
                                            Console.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                            if (r.Channels != null && r.Channels.Count > 0)
                                            {
                                                var read = r.Channels[channel][authKey].ReadEnabled;
                                                var write = r.Channels[channel][authKey].WriteEnabled;
                                                if (read && write) { receivedGrantMessage = true; }
                                            }
                                        }
                                    }
                                    catch { }
                                    finally
                                    {
                                        grantManualEvent.Set();
                                    }
                                }));

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenAMessageIsPublished Grant access failed.");
        }

        [TestFixtureTearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public static void ThenNullMessageShouldReturnException()
        {
            server.ClearRequests();

            string channel = "hello_my_channel";
            object message = null;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };
            server.RunOnHttps(true);

            pubnub = createPubNubInstance(config);

            string expected = "";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, message))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish()
                    .Channel(channel)
                    .Message(message)
                    .Async(new PNPublishResultExt((r, s) => { }));

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenUnencryptPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;

            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid"
            };
            server.RunOnHttps(true);

            pubnub = createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715278266153304\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) =>
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
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715278266153304 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Async(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };
            server.RunOnHttps(true);

            pubnub = createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715286132003364\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) =>
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
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715286132003364 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Async(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) =>
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
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715325228931129 : publishTimetoken)
                    .Count(100)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Async(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = true
            };
            server.RunOnHttps(true);

            pubnub = createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) =>
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
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715325228931129 : publishTimetoken)
                    .Count(100)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Async(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715426119520817\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22%2BBY5%2FmiAA8aeuhVl4d13Kg%3D%3D%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) =>
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
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715426119520817 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Async(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
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

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForSecretEncryptPublish;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string expected = "[1, \"Sent\", \"14715438956854374\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22f42pIQcWZ9zbTbH8cyLwB%2FtdvRxjFLOYcBNMVKeHS54%3D%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("signature", "tcFpCYsp1uiqyWCZxvdJp7KXEXjyvCFnH6F4UjJ6mds=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) =>
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
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("signature", "WyHIBPHildY1gtERK5uDGqX8RyKnrqQFegoOoHizsV4=")
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715438956854374 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Async(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
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

        [Test]
        public static void ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            string channel = "hello_my_channel";
            object message = new PubnubDemoObject();

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid4",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string expected = "[1, \"Sent\", \"14715459088445832\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%7B%22VersionID%22:3.4%2C%22Timetoken%22:%2213601488652764619%22%2C%22OperationName%22:%22Publish%22%2C%22Channels%22:%5B%22ch1%22%5D%2C%22DemoMessage%22:%7B%22DefaultMessage%22:%22~!%40%23%24%25%5E%26*()_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D%5C%5C%20%7B%7D%7C%20asdfghjkl%3B'%20:%5C%22%20zxcvbnm%2C.%2F%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22:%7B%22DefaultMessage%22:%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah!%22%7D%2C%22SampleXml%22:%5B%7B%22ID%22:%22ABCD123%22%2C%22Name%22:%7B%22First%22:%22John%22%2C%22Middle%22:%22P.%22%2C%22Last%22:%22Doe%22%7D%2C%22Address%22:%7B%22Street%22:%22123%20Duck%20Street%22%2C%22City%22:%22New%20City%22%2C%22State%22:%22New%20York%22%2C%22Country%22:%22United%20States%22%7D%7D%2C%7B%22ID%22:%22ABCD456%22%2C%22Name%22:%7B%22First%22:%22Peter%22%2C%22Middle%22:%22Z.%22%2C%22Last%22:%22Smith%22%7D%2C%22Address%22:%7B%22Street%22:%2212%20Hollow%20Street%22%2C%22City%22:%22Philadelphia%22%2C%22State%22:%22Pennsylvania%22%2C%22Country%22:%22United%20States%22%7D%7D%5D%7D", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) =>
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
                Assert.IsTrue(receivedPublishMessage, "Complex Object Publish Failed");
            }
            else
            {
                receivedPublishMessage = false;

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

                expected = Resource.ComplexMessage;

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715459088445832")
                        .WithParameter("include_token", "true")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Console.WriteLine("WhenAMessageIsPublished-ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo - Publish OK. Now checking detailed history");

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715459088445832 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Async(new PNHistoryResultExt(
                                (r, s) =>
                                {
                                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
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
        public static void ThenPubnubShouldGenerateUniqueIdentifier()
        {
            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };

            pubnub = createPubNubInstance(config);

            Assert.IsNotNull(pubnub.GenerateGuid());
            pubnub = null;
        }

        [Test]
        [ExpectedException(typeof(MissingMemberException))]
        public static void ThenPublishKeyShouldNotBeEmpty()
        {
            PNConfiguration config = new PNConfiguration
            {
                PublishKey = "",
                SubscribeKey = PubnubCommon.SubscribeKey,
            };

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) => { }));
            pubnub = null;

        }

        [Test]
        public static void ThenOptionalSecretKeyShouldBeProvidedInConfig()
        {
            server.ClearRequests();

            bool receivedPublishMessage = false;
            long publishTimetoken = 0;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false,
                Uuid = "mytestuuid"
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            string expected = "[1,\"Sent\",\"14722277738126309\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20API%20Usage%20Example%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("signature", "CkHf9ur70OxnkkPvzc9PCPbSbq_SHq2hfYbfDHXh90Q=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) =>
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            server.RunOnHttps(true);

            pubnub = createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Pubnub%20API%20Usage%20Example%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message)
                    .Async(new PNPublishResultExt((r, s) =>
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
    }
}
