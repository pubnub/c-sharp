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
        private ManualResetEvent mreUnencryptedPublish = new ManualResetEvent(false);
        private ManualResetEvent mreOptionalSecretKeyPublish = new ManualResetEvent(false);
        private ManualResetEvent mreNoSslPublish = new ManualResetEvent(false);

        private ManualResetEvent mreUnencryptObjectPublish = new ManualResetEvent(false);
        private ManualResetEvent mreEncryptObjectPublish = new ManualResetEvent(false);
        private ManualResetEvent mreEncryptPublish = new ManualResetEvent(false);
        private ManualResetEvent mreSecretEncryptPublish = new ManualResetEvent(false);
        private ManualResetEvent mreComplexObjectPublish = new ManualResetEvent(false);
        private ManualResetEvent mreLaregMessagePublish = new ManualResetEvent(false);

        private ManualResetEvent mreEncryptDetailedHistory = new ManualResetEvent(false);
        private ManualResetEvent mreSecretEncryptDetailedHistory = new ManualResetEvent(false);
        private ManualResetEvent mreUnencryptDetailedHistory = new ManualResetEvent(false);
        private ManualResetEvent mreUnencryptObjectDetailedHistory = new ManualResetEvent(false);
        private ManualResetEvent mreEncryptObjectDetailedHistory = new ManualResetEvent(false);
        private ManualResetEvent mreComplexObjectDetailedHistory = new ManualResetEvent(false);
        private ManualResetEvent mreSerializedObjectMessageForPublish = new ManualResetEvent(false);
        private ManualResetEvent mreSerializedMessagePublishDetailedHistory = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent historyManualEvent = new ManualResetEvent(false);

        private bool isPublished2 = false;
        private bool isPublished3 = false;

        private bool isUnencryptPublished = false;
        private bool isUnencryptObjectPublished = false;
        private bool isEncryptObjectPublished = false;
        private bool isUnencryptDetailedHistory = false;
        private bool isUnencryptObjectDetailedHistory = false;
        private bool isEncryptObjectDetailedHistory = false;
        private bool isEncryptPublished = false;
        private bool isSecretEncryptPublished = false;
        private bool isEncryptDetailedHistory = false;
        private bool isSecretEncryptDetailedHistory = false;
        private bool isComplexObjectPublished = false;
        private bool isComplexObjectDetailedHistory = false;
        private bool isSerializedObjectMessagePublished = false;
        private bool isSerializedObjectMessageDetailedHistory = false;
        private bool isLargeMessagePublished = false;
        private static bool receivedGrantMessage = false;
        private static bool receivedPublishMessage = false;

        private static long publishTimetoken = 0;
        private long unEncryptPublishTimetoken = 0;
        private long unEncryptObjectPublishTimetoken = 0;
        private long encryptObjectPublishTimetoken = 0;
        private long encryptPublishTimetoken = 0;
        private long secretEncryptPublishTimetoken = 0;
        private long complexObjectPublishTimetoken = 0;
        private long serializedMessagePublishTimetoken = 0;

        private const string messageForUnencryptPublish = "Pubnub Messaging API 1";
        private const string messageForEncryptPublish = "漢語";
        private const string messageForSecretEncryptPublish = "Pubnub Messaging API 2";
        private string messageObjectForUnencryptPublish = "";
        private string messageObjectForEncryptPublish = "";
        private string messageComplexObjectForPublish = "";
        private string serializedObjectMessageForPublish;

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

            pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenAMessageIsPublished Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ThenNullMessageShouldReturnException()
        {
            server.ClearRequests();

            string channel = "hello_my_channel";
            object message = null;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };
            server.RunOnHttps(true);

            pubnub = this.createPubNubInstance(config);

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
                    .Async(new UTPublishResult());

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenUnencryptPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenUnencryptPublishShouldReturnSuccessCodeAndInfo";

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid"
            };
            server.RunOnHttps(true);

            pubnub = this.createPubNubInstance(config);

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

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
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

                historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715278266153304 : publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful unencrypt Publish");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo";

            string channel = "hello_my_channel";
            object message = new CustomClass();

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };
            server.RunOnHttps(true);

            pubnub = this.createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715286132003364\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            messageObjectForUnencryptPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);

            manualResetEventWaitTimeout = 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
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

                historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715286132003364 : unEncryptObjectPublishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful unencrypt object Publish");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo";

            string channel = "hello_my_channel";
            object message = new SecretCustomClass();

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            messageObjectForEncryptPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);

            manualResetEventWaitTimeout = 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
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

                historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715325228931129 : encryptObjectPublishTimetoken)
                    .Count(100)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful encrypt object Publish");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfoWithSSL()
        {
            server.ClearRequests();

            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfoWithSSL";

            string channel = "hello_my_channel";
            object message = new SecretCustomClass();

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = true
            };
            server.RunOnHttps(true);

            pubnub = this.createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            messageObjectForEncryptPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);

            manualResetEventWaitTimeout = 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
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

                historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715325228931129 : encryptObjectPublishTimetoken)
                    .Count(100)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful encrypt object Publish with SSL");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenEncryptPublishShouldReturnSuccessCodeAndInfo";

            string channel = "hello_my_channel";
            string message = messageForEncryptPublish;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

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

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
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

                historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715426119520817 : encryptPublishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to decrypt the successful Publish");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo";

            string channel = "hello_my_channel";
            string message = messageForSecretEncryptPublish;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

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

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
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

                historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715438956854374 : secretEncryptPublishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to decrypt the successful Secret key Publish");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo";

            string channel = "hello_my_channel";
            object message = new PubnubDemoObject();

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid4",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            string expected = "[1, \"Sent\", \"14715459088445832\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%7B%22VersionID%22:3.4%2C%22Timetoken%22:%2213601488652764619%22%2C%22OperationName%22:%22Publish%22%2C%22Channels%22:%5B%22ch1%22%5D%2C%22DemoMessage%22:%7B%22DefaultMessage%22:%22~!%40%23%24%25%5E%26*()_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D%5C%5C%20%7B%7D%7C%20asdfghjkl%3B'%20:%5C%22%20zxcvbnm%2C.%2F%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22:%7B%22DefaultMessage%22:%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah!%22%7D%2C%22SampleXml%22:%5B%7B%22ID%22:%22ABCD123%22%2C%22Name%22:%7B%22First%22:%22John%22%2C%22Middle%22:%22P.%22%2C%22Last%22:%22Doe%22%7D%2C%22Address%22:%7B%22Street%22:%22123%20Duck%20Street%22%2C%22City%22:%22New%20City%22%2C%22State%22:%22New%20York%22%2C%22Country%22:%22United%20States%22%7D%7D%2C%7B%22ID%22:%22ABCD456%22%2C%22Name%22:%7B%22First%22:%22Peter%22%2C%22Middle%22:%22Z.%22%2C%22Last%22:%22Smith%22%7D%2C%22Address%22:%7B%22Street%22:%2212%20Hollow%20Street%22%2C%22City%22:%22Philadelphia%22%2C%22State%22:%22Pennsylvania%22%2C%22Country%22:%22United%20States%22%7D%7D%5D%7D", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            messageComplexObjectForPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);

            manualResetEventWaitTimeout = 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
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

                historyManualEvent = new ManualResetEvent(false);

                pubnub.History().Channel(channel)
                    .End(PubnubCommon.EnableStubTest ? 14715459088445832 : complexObjectPublishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedPublishMessage, "Unable to match the successful unencrypt object Publish");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        //[Test]
        //public void ThenDisableJsonEncodeShouldSendSerializedObjectMessage()
        //{
        //    receivedPublishMessage = false;
        //    publishTimetoken = 0;
        //    currentTestCase = "ThenDisableJsonEncodeShouldSendSerializedObjectMessage";

        //    string channel = "hello_my_channel";
        //    object message = "{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}";

        //    PNConfiguration config = new PNConfiguration()
        //    {
        //        PublishKey = PubnubCommon.PublishKey,
        //        SubscribeKey = PubnubCommon.SubscribeKey,
        //        Uuid = "mytestuuid",
        //        EnableJsonEncodingForPublish = false,
        //        Secure = false
        //    };

        //    pubnub = this.createPubNubInstance(config);

        //    string expected = "[1,\"Sent\",\"14721410674316172\"]";

        //    server.AddRequest(new Request()
        //            .WithMethod("GET")
        //            .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%7B%22operation%22%3A%22ReturnData%22%2C%22channel%22%3A%22Mobile1%22%2C%22sequenceNumber%22%3A0%2C%22data%22%3A%5B%22ping%201.0.0.1%22%5D%7D", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
        //            .WithParameter("uuid", config.Uuid)
        //            .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
        //            .WithResponse(expected)
        //            .WithStatusCode(System.Net.HttpStatusCode.OK));

        //    serializedObjectMessageForPublish = message.ToString();

        //    manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

        //    publishManualEvent = new ManualResetEvent(false);
        //    pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
        //    publishManualEvent.WaitOne(manualResetEventWaitTimeout);

        //    if (!receivedPublishMessage)
        //    {
        //        Assert.IsTrue(receivedPublishMessage, "Serialized Object Message Publish Failed");
        //    }
        //    else
        //    {
        //        receivedPublishMessage = false;

        //        Thread.Sleep(1000);

        //        expected = "[[{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}],14721411498132384,14721411498132384]";

        //        server.AddRequest(new Request()
        //                .WithMethod("GET")
        //                .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
        //                .WithParameter("count", "100")
        //                .WithParameter("end", "14721411498132384")
        //                .WithParameter("uuid", config.Uuid)
        //                .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
        //                .WithResponse(expected)
        //                .WithStatusCode(System.Net.HttpStatusCode.OK));

        //        historyManualEvent = new ManualResetEvent(false);

        //        pubnub.History().Channel(channel)
        //            .End(PubnubCommon.EnableStubTest ? 14721411498132384 : serializedMessagePublishTimetoken)
        //            .Reverse(false)
        //            .IncludeTimetoken(false)
        //            .Async(new UTHistoryResult());

        //        historyManualEvent.WaitOne(manualResetEventWaitTimeout);

        //        Assert.IsTrue(receivedPublishMessage, "Unable to match the successful serialized object message Publish");
        //    }

        //    pubnub.Destroy();
        //    pubnub.PubnubUnitTest = null;
        //    pubnub = null;
        //}

        //[Test]
        //public void ThenLargeMessageShoudFailWithMessageTooLargeInfo()
        //{
        //    receivedPublishMessage = false;
        //    publishTimetoken = 0;
        //    currentTestCase = "ThenLargeMessageShoudFailWithMessageTooLargeInfo";

        //    string channel = "hello_my_channel";
        //    string message = Resource.LargeMessage32K;

        //    PNConfiguration config = new PNConfiguration()
        //    {
        //        PublishKey = PubnubCommon.PublishKey,
        //        SubscribeKey = PubnubCommon.SubscribeKey,
        //        Uuid = "mytestuuid",
        //    };

        //    pubnub = this.createPubNubInstance(config);

        //    string expected = "[0,\"Message Too Large\",\"14714489901553535\"]";

        //    server.AddRequest(new Request()
        //            .WithMethod("GET")
        //            .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, Resource.LargeMessage32KStatic))
        //            .WithParameter("uuid", config.Uuid)
        //            .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
        //            .WithResponse(expected)
        //            .WithStatusCode(System.Net.HttpStatusCode.BadRequest));

        //    manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

        //    publishManualEvent = new ManualResetEvent(false);
        //    pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
        //    publishManualEvent.WaitOne(manualResetEventWaitTimeout);

        //    pubnub.Destroy();
        //    pubnub.PubnubUnitTest = null;
        //    pubnub = null;
        //    Assert.IsTrue(isLargeMessagePublished, "Message Too Large is not failing as expected.");
        //}

        [Test]
        public void ThenPubnubShouldGenerateUniqueIdentifier()
        {
            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };

            pubnub = this.createPubNubInstance(config);

            Assert.IsNotNull(pubnub.GenerateGuid());
            pubnub = null;
        }

        [Test]
        [ExpectedException(typeof(MissingMemberException))]
        public void ThenPublishKeyShouldNotBeEmpty()
        {
            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = "",
                SubscribeKey = PubnubCommon.SubscribeKey,
            };

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
            pubnub = null;

        }

        [Test]
        public void ThenOptionalSecretKeyShouldBeProvidedInConfig()
        {
            server.ClearRequests();

            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenOptionalSecretKeyShouldBeProvidedInConfig";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false,
                Uuid = "mytestuuid"
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

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

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish Failed with secret key");
        }

        [Test]
        public void IfSSLNotProvidedThenDefaultShouldBeTrue()
        {
            server.ClearRequests();

            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "IfSSLNotProvidedThenDefaultShouldBeTrue";

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            server.RunOnHttps(true);

            pubnub = this.createPubNubInstance(config);

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

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Publish Failed with no SSL");
        }

        //void ThenPublishInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
        //{
        //    try
        //    {
        //        if (receivedMessage != null)
        //        {
        //            var status = receivedMessage.StatusCode;
        //            if (status == 200)
        //            {
        //                receivedGrantMessage = true;
        //            }
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        grantManualEvent.Set();
        //    }
        //}

        //private void ReturnSuccessUnencryptPublishCodeCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            isUnencryptPublished = true;
        //            unEncryptPublishTimetoken = result.Timetoken;
        //        }
        //    }

        //    mreUnencryptedPublish.Set();
        //}

        //private void ReturnSuccessUnencryptObjectPublishCodeCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            isUnencryptObjectPublished = true;
        //            unEncryptObjectPublishTimetoken = result.Timetoken;
        //        }
        //    }

        //    mreUnencryptObjectPublish.Set();
        //}

        //private void ReturnSuccessEncryptObjectPublishCodeCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            isEncryptObjectPublished = true;
        //            encryptObjectPublishTimetoken = result.Timetoken;
        //        }
        //    }

        //    mreEncryptObjectPublish.Set();
        //}

        //private void ReturnSuccessEncryptPublishCodeCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            isEncryptPublished = true;
        //            encryptPublishTimetoken = result.Timetoken;
        //        }
        //    }

        //    mreEncryptPublish.Set();
        //}

        //private void ReturnSuccessSecretEncryptPublishCodeCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            isSecretEncryptPublished = true;
        //            secretEncryptPublishTimetoken = result.Timetoken;
        //        }
        //    }

        //    mreSecretEncryptPublish.Set();
        //}

        //private void CaptureUnencryptDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null && message.Length > 0 && message[0].ToString() == messageForUnencryptPublish)
        //        {
        //            isUnencryptDetailedHistory = true;
        //        }
        //    }

        //    mreUnencryptDetailedHistory.Set();
        //}

        //private void CaptureUnencryptObjectDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null)
        //        {
        //            string publishedMesaage = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
        //            if (publishedMesaage == messageObjectForUnencryptPublish)
        //            {
        //                isUnencryptObjectDetailedHistory = true;
        //            }
        //        }
        //    }

        //    mreUnencryptObjectDetailedHistory.Set();
        //}

        //private void CaptureEncryptObjectDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null && message.Length > 0)
        //        {
        //            string publishedMesaage = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
        //            if (publishedMesaage == messageObjectForEncryptPublish)
        //            {
        //                isEncryptObjectDetailedHistory = true;
        //            }
        //        }

        //    }

        //    mreEncryptObjectDetailedHistory.Set();
        //}

        //private void CaptureEncryptDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null && message.Length > 0)
        //        {
        //            string publishedMesaage = message[0].ToString();
        //            if (publishedMesaage == messageForEncryptPublish)
        //            {
        //                isEncryptDetailedHistory = true;
        //            }
        //        }
        //    }

        //    mreEncryptDetailedHistory.Set();
        //}

        //private void CaptureSecretEncryptDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null && message.Length > 0)
        //        {
        //            string publishedMesaage = message[0].ToString();
        //            if (publishedMesaage == messageForSecretEncryptPublish)
        //            {
        //                isSecretEncryptDetailedHistory = true;
        //            }
        //        }

        //    }

        //    mreSecretEncryptDetailedHistory.Set();
        //}

        //private void ReturnSuccessComplexObjectPublishCodeCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            isComplexObjectPublished = true;
        //            complexObjectPublishTimetoken = result.Timetoken;
        //        }
        //    }

        //    mreComplexObjectPublish.Set();

        //}

        //private void CaptureComplexObjectDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    Console.WriteLine("CaptureComplexObjectDetailedHistoryCallback = \n" + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));

        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null && message.Length > 0)
        //        {
        //            string publishedMesaage = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
        //            if (publishedMesaage == messageComplexObjectForPublish)
        //            {
        //                isComplexObjectDetailedHistory = true;
        //            }
        //        }
        //    }

        //    mreComplexObjectDetailedHistory.Set();
        //}

        //private void ReturnPublishMessageTooLargeErrorCallback(PubnubClientError pubnubError)
        //{
        //    Console.WriteLine(pubnubError);
        //    if (pubnubError != null)
        //    {
        //        if (pubnubError.Message.ToLower().IndexOf("message too large") >= 0)
        //        {
        //            isLargeMessagePublished = true;
        //        }
        //    }

        //    mreLaregMessagePublish.Set();
        //}

        //private void DummyPublishMessageTooLargeInfoCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 0 && statusMessage.ToLower().IndexOf("message too large") >= 0)
        //        {
        //            isLargeMessagePublished = true;
        //        }
        //    }

        //    mreLaregMessagePublish.Set();
        //}

        //private void ReturnSecretKeyPublishCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            isPublished2 = true;
        //        }
        //    }
        //    mreOptionalSecretKeyPublish.Set();
        //}

        //private void ReturnNoSSLDefaultTrueCallback(PNPublishResult result)
        //{
        //    if (result != null && result.StatusCode == 1 && result.StatusMessage.ToLower() == "sent")
        //    {
        //        isPublished3 = true;
        //    }
        //    mreNoSslPublish.Set();
        //}

        //private void ReturnSuccessSerializedObjectMessageForPublishCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            isSerializedObjectMessagePublished = true;
        //            serializedMessagePublishTimetoken = result.Timetoken;
        //        }
        //    }

        //    mreSerializedObjectMessageForPublish.Set();
        //}

        //private void CaptureSerializedMessagePublishDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null && message.Length > 0)
        //        {
        //            string publishedMesaage = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
        //            if (publishedMesaage == serializedObjectMessageForPublish)
        //            {
        //                isSerializedObjectMessageDetailedHistory = true;
        //            }
        //        }
        //    }

        //    mreSerializedMessagePublishDetailedHistory.Set();
        //}


        //private void DummyErrorCallback(PubnubClientError result)
        //{
        //    if (result != null)
        //    {
        //        Console.WriteLine(result.Message);
        //    }
        //}

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
                            var read = result.Channels[channel][authKey].ReadEnabled;
                            var write = result.Channels[channel][authKey].WriteEnabled;
                            if (read && write)
                            {
                                receivedGrantMessage = true;
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
                        case "ThenUnencryptPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfoWithSSL":
                        case "ThenEncryptPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo":
                        case "ThenDisableJsonEncodeShouldSendSerializedObjectMessage":
                        case "ThenLargeMessageShoudFailWithMessageTooLargeInfo":
                        case "ThenOptionalSecretKeyShouldBeProvidedInConfig":
                        case "IfSSLNotProvidedThenDefaultShouldBeTrue":
                            receivedPublishMessage = true;
                            publishManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }
            }
        };

        public class UTHistoryResult : PNCallback<PNHistoryResult>
        {
            public override void OnResponse(PNHistoryResult result, PNStatus status)
            {
                Console.WriteLine("History Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("History PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                if (status.StatusCode == 200 && !status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "ThenUnencryptPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfoWithSSL":
                        case "ThenEncryptPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo":
                        case "ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo":
                        case "ThenDisableJsonEncodeShouldSendSerializedObjectMessage":
                            receivedPublishMessage = true;
                            historyManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }
            }
        };
    }
}
