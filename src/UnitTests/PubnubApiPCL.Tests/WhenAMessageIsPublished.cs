using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using HttpMock;
using System.Collections.Generic;
using MockServer;

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
        private ManualResetEvent grantManualEvent = new ManualResetEvent(false);

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
        private bool receivedGrantMessage = false;

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

        private int manualResetEventsWaitTimeout = 310 * 1000;

        private Pubnub pubnub = null;
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
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string channel = "hello_my_channel";

            string expected = "[1,\"Sent\",\"14715278266153304\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("signature", "84PyodrGvHlEnC1leH-HLSvy_P6Hd_PGLpLeDFrEads=")
                    .WithParameter("channel", channel)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.grant().channels(new string[] { channel }).read(true).write(true).manage(false).ttl(20).async(new PNCallback<PNAccessManagerGrantResult>() { result = ThenPublishInitializeShouldReturnGrantMessage, error = DummyErrorCallback });
            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.EndPendingRequests();
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
            string channel = "hello_my_channel";
            object message = null;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, message))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = { }, error = { } });

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenUnencryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isUnencryptPublished = false;

            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[1,\"Sent\",\"14715278266153304\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20API%201%22"))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnSuccessUnencryptPublishCodeCallback, error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mreUnencryptedPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isUnencryptPublished)
            {
                Assert.IsTrue(isUnencryptPublished, "Unencrypt Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);

                expected = "[[\"Pubnub Messaging API 1\"],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715278266153304")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.history().channel(channel)
                    .end(PubnubCommon.EnableStubTest ? 14715278266153304 : unEncryptPublishTimetoken)
                    .reverse(false)
                    .includeTimetoken(false)
                    .async(new PNCallback<PNHistoryResult>() { result = CaptureUnencryptDetailedHistoryCallback, error = DummyErrorCallback });
                mreUnencryptDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

                Assert.IsTrue(isUnencryptDetailedHistory, "Unable to match the successful unencrypt Publish");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            isUnencryptObjectPublished = false;

            string channel = "hello_my_channel";
            object message = new CustomClass();

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[1,\"Sent\",\"14715286132003364\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D"))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            messageObjectForUnencryptPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnSuccessUnencryptObjectPublishCodeCallback, error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mreUnencryptObjectPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isUnencryptObjectPublished)
            {
                Assert.IsTrue(isUnencryptObjectPublished, "Unencrypt Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);

                expected = "[[{\"foo\":\"hi!\",\"bar\":[1,2,3,4,5]}],14715286132003364,14715286132003364]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715286132003364")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.history().channel(channel).end(PubnubCommon.EnableStubTest ? 14715286132003364 : unEncryptObjectPublishTimetoken)
                    .reverse(false)
                    .includeTimetoken(false)
                    .async(new PNCallback<PNHistoryResult>() { result = CaptureUnencryptObjectDetailedHistoryCallback, error = DummyErrorCallback });
                mreUnencryptObjectDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

                Assert.IsTrue(isUnencryptObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            string channel = "hello_my_channel";
            object message = new SecretCustomClass();

            isEncryptObjectPublished = false;
            isEncryptObjectDetailedHistory = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CiperKey = "enigma",
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2%22"))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            messageObjectForEncryptPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);

            mreEncryptObjectPublish = new ManualResetEvent(false);
            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnSuccessEncryptObjectPublishCodeCallback, error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mreEncryptObjectPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isEncryptObjectPublished)
            {
                Assert.IsTrue(isEncryptObjectPublished, "Encrypt Object Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);

                expected = "[[\"nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2\"],14715325858469956,14715325858469956]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715325228931129")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                mreEncryptObjectDetailedHistory = new ManualResetEvent(false);
                pubnub.history().channel(channel)
                    .end(PubnubCommon.EnableStubTest ? 14715325228931129 : encryptObjectPublishTimetoken)
                    .count(100)
                    .reverse(false)
                    .includeTimetoken(false)
                    .async(new PNCallback<PNHistoryResult>() { result = CaptureEncryptObjectDetailedHistoryCallback, error = DummyErrorCallback });
                mreEncryptObjectDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

                Assert.IsTrue(isEncryptObjectDetailedHistory, "Unable to match the successful encrypt object Publish");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfoWithSSL()
        {
            string channel = "hello_my_channel";
            object message = new SecretCustomClass();

            isEncryptObjectPublished = false;
            isEncryptObjectDetailedHistory = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CiperKey = "enigma",
                Uuid = "mytestuuid2",
                Secure = true
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
                config.Secure = true;
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[1,\"Sent\",\"14722490854460542\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2%22"))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            messageObjectForEncryptPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);

            mreEncryptObjectPublish = new ManualResetEvent(false);
            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnSuccessEncryptObjectPublishCodeCallback, error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mreEncryptObjectPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isEncryptObjectPublished)
            {
                Assert.IsTrue(isEncryptObjectPublished, "Encrypt Object Publish with SSL Failed");
            }
            else
            {
                Thread.Sleep(1000);

                expected = "[[\"nQTUCOeyWWgWh5NRLhSlhIingu92WIQ6RFloD9rOZsTUjAhD7AkMaZJVgU7l28e2\"],14715335320075032,14715335320075032]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715335320075032")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                mreEncryptObjectDetailedHistory = new ManualResetEvent(false);
                pubnub.history().channel(channel)
                    .end(PubnubCommon.EnableStubTest ? 14715335320075032 : encryptObjectPublishTimetoken)
                    .reverse(false)
                    .includeTimetoken(false)
                    .async(new PNCallback<PNHistoryResult>() { result = CaptureEncryptObjectDetailedHistoryCallback, error = DummyErrorCallback });
                mreEncryptObjectDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

                Assert.IsTrue(isEncryptObjectDetailedHistory, "Unable to match the successful encrypt object Publish with SSL");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isEncryptPublished = false;
            string channel = "hello_my_channel";
            string message = messageForEncryptPublish;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CiperKey = "enigma",
                Uuid = "mytestuuid3",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[1,\"Sent\",\"14715426119520817\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22%2BBY5%2FmiAA8aeuhVl4d13Kg%3D%3D%22"))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnSuccessEncryptPublishCodeCallback, error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mreEncryptPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isEncryptPublished)
            {
                Assert.IsTrue(isEncryptPublished, "Encrypt Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);

                expected = "[[\"+BY5/miAA8aeuhVl4d13Kg==\"],14715426119520817,14715426119520817]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715426119520817")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.history().channel(channel)
                    .end(PubnubCommon.EnableStubTest ? 14715426119520817 : encryptPublishTimetoken)
                    .reverse(false)
                    .includeTimetoken(false)
                    .async(new PNCallback<PNHistoryResult>() { result = CaptureEncryptDetailedHistoryCallback, error = DummyErrorCallback });
                mreEncryptDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

                Assert.IsTrue(isEncryptDetailedHistory, "Unable to decrypt the successful Publish");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isSecretEncryptPublished = false;

            string channel = "hello_my_channel";
            string message = messageForSecretEncryptPublish;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = "key",
                CiperKey = "enigma",
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string url = String.Format("/publish/{0}/{1}/{2}/{3}/0/{4}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "e462eda69685ce9ddfd5be20c7e13cab", channel, "%22f42pIQcWZ9zbTbH8cyLwB%2FtdvRxjFLOYcBNMVKeHS54%3D%22");

            string expected = "[1, \"Sent\", \"14715438956854374\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/{2}/{3}/0/{4}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "e462eda69685ce9ddfd5be20c7e13cab", channel, "%22f42pIQcWZ9zbTbH8cyLwB%2FtdvRxjFLOYcBNMVKeHS54%3D%22"))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnSuccessSecretEncryptPublishCodeCallback, error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mreSecretEncryptPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isSecretEncryptPublished)
            {
                Assert.IsTrue(isSecretEncryptPublished, "Secret Encrypt Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);

                url = String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel);

                expected = "[[\"f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=\"],14715438956854374,14715438956854374]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715438956854374")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.history().channel(channel)
                    .end(PubnubCommon.EnableStubTest ? 14715438956854374 : secretEncryptPublishTimetoken)
                    .reverse(false)
                    .includeTimetoken(false)
                    .async(new PNCallback<PNHistoryResult>() { result = CaptureSecretEncryptDetailedHistoryCallback, error = DummyErrorCallback });
                mreSecretEncryptDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

                Assert.IsTrue(isSecretEncryptDetailedHistory, "Unable to decrypt the successful Secret key Publish");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        {
            isComplexObjectPublished = false;

            string channel = "hello_my_channel";
            object message = new PubnubDemoObject();

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid4",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[1, \"Sent\", \"14715459088445832\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%7B%22VersionID%22:3.4%2C%22Timetoken%22:%2213601488652764619%22%2C%22OperationName%22:%22Publish%22%2C%22Channels%22:%5B%22ch1%22%5D%2C%22DemoMessage%22:%7B%22DefaultMessage%22:%22~!%40%23%24%25%5E%26*()_%2B%20%601234567890-%3D%20qwertyuiop%5B%5D%5C%5C%20%7B%7D%7C%20asdfghjkl%3B'%20:%5C%22%20zxcvbnm%2C.%2F%20%3C%3E%3F%20%22%7D%2C%22CustomMessage%22:%7B%22DefaultMessage%22:%22Welcome%20to%20the%20world%20of%20Pubnub%20for%20Publish%20and%20Subscribe.%20Hah!%22%7D%2C%22SampleXml%22:%5B%7B%22Name%22:%7B%22First%22:%22John%22%2C%22Middle%22:%22P.%22%2C%22Last%22:%22Doe%22%7D%2C%22Address%22:%7B%22Street%22:%22123%20Duck%20Street%22%2C%22City%22:%22New%20City%22%2C%22State%22:%22New%20York%22%2C%22Country%22:%22United%20States%22%7D%2C%22ID%22:%22ABCD123%22%7D%2C%7B%22Name%22:%7B%22First%22:%22Peter%22%2C%22Middle%22:%22Z.%22%2C%22Last%22:%22Smith%22%7D%2C%22Address%22:%7B%22Street%22:%2212%20Hollow%20Street%22%2C%22City%22:%22Philadelphia%22%2C%22State%22:%22Pennsylvania%22%2C%22Country%22:%22United%20States%22%7D%2C%22ID%22:%22ABCD456%22%7D%5D%7D", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            messageComplexObjectForPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnSuccessComplexObjectPublishCodeCallback, error = DummyErrorCallback });
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 310 * 1000 : 310 * 1000;
            mreComplexObjectPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isComplexObjectPublished)
            {
                Assert.IsTrue(isComplexObjectPublished, "Complex Object Publish Failed");
            }
            else
            {
                Thread.Sleep(500);

                expected = "[[{\"VersionID\":3.4,\"Timetoken\":\"13601488652764619\",\"OperationName\":\"Publish\",\"Channels\":[\"ch1\"],\"DemoMessage\":{\"DefaultMessage\":\"~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\\\ {}| asdfghjkl;' :\\\" zxcvbnm,./ <>? \"},\"CustomMessage\":{\"DefaultMessage\":\"Welcome to the world of Pubnub for Publish and Subscribe. Hah!\"},\"SampleXml\":[{\"Name\":{\"First\":\"John\",\"Middle\":\"P.\",\"Last\":\"Doe\"},\"Address\":{\"Street\":\"123 Duck Street\",\"City\":\"New City\",\"State\":\"New York\",\"Country\":\"United States\"},\"ID\":\"ABCD123\"},{\"Name\":{\"First\":\"Peter\",\"Middle\":\"Z.\",\"Last\":\"Smith\"},\"Address\":{\"Street\":\"12 Hollow Street\",\"City\":\"Philadelphia\",\"State\":\"Pennsylvania\",\"Country\":\"United States\"},\"ID\":\"ABCD456\"}]}],14720854311688200,14720854311688200]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14715459088445832")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Console.WriteLine("WhenAMessageIsPublished-ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo - Publish OK. Now checking detailed history");
                pubnub.history().channel(channel)
                    .end(PubnubCommon.EnableStubTest ? 14715459088445832 : complexObjectPublishTimetoken)
                    .reverse(false)
                    .includeTimetoken(false)
                    .async(new PNCallback<PNHistoryResult>() { result = CaptureComplexObjectDetailedHistoryCallback, error = DummyErrorCallback });
                mreComplexObjectDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

                Assert.IsTrue(isComplexObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenDisableJsonEncodeShouldSendSerializedObjectMessage()
        {
            isSerializedObjectMessagePublished = false;

            string channel = "hello_my_channel";
            object message = "{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                EnableJsonEncodingForPublish = false
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[1,\"Sent\",\"14721410674316172\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%7B%22operation%22%3A%22ReturnData%22%2C%22channel%22%3A%22Mobile1%22%2C%22sequenceNumber%22%3A0%2C%22data%22%3A%5B%22ping%201.0.0.1%22%5D%7D", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            serializedObjectMessageForPublish = message.ToString();

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnSuccessSerializedObjectMessageForPublishCallback, error = DummyErrorCallback });

            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            mreSerializedObjectMessageForPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isSerializedObjectMessagePublished)
            {
                Assert.IsTrue(isSerializedObjectMessagePublished, "Serialized Object Message Publish Failed");
            }
            else
            {
                Thread.Sleep(500);

                expected = "[[{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}],14721411498132384,14721411498132384]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("end", "14721411498132384")
                        .WithParameter("uuid", config.Uuid)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.history().channel(channel)
                    .end(PubnubCommon.EnableStubTest ? 14721411498132384 : serializedMessagePublishTimetoken)
                    .reverse(false)
                    .includeTimetoken(false)
                    .async(new PNCallback<PNHistoryResult>() { result = CaptureSerializedMessagePublishDetailedHistoryCallback, error = DummyErrorCallback });
                mreSerializedMessagePublishDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
                Assert.IsTrue(isSerializedObjectMessageDetailedHistory, "Unable to match the successful serialized object message Publish");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void ThenLargeMessageShoudFailWithMessageTooLargeInfo()
        {
            isLargeMessagePublished = false;

            string channel = "hello_my_channel";
            string message = Resource.LargeMessage32K;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[0,\"Message Too Large\",\"14714489901553535\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, Resource.LargeMessage32KStatic))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.BadRequest));

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = DummyPublishMessageTooLargeInfoCallback, error = ReturnPublishMessageTooLargeErrorCallback });
            manualResetEventsWaitTimeout = 310 * 1000;
            mreLaregMessagePublish.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(isLargeMessagePublished, "Message Too Large is not failing as expected.");
        }

        [Test]
        public void ThenPubnubShouldGenerateUniqueIdentifier()
        {
            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

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

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = null, error = DummyErrorCallback });
            pubnub = null;

        }

        [Test]
        public void ThenOptionalSecretKeyShouldBeProvidedInConstructor()
        {
            isPublished2 = false;
            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = "key"
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            string expected = "[1,\"Sent\",\"14722277738126309\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/154de00ed4a7a76b4dc4a83906d05bab/{2}/0/%22Pubnub%20API%20Usage%20Example%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnSecretKeyPublishCallback, error = DummyErrorCallback });

            mreOptionalSecretKeyPublish.WaitOne(310 * 1000);

            pubnub.EndPendingRequests();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(isPublished2, "Publish Failed with secret key");
        }

        [Test]
        public void IfSSLNotProvidedThenDefaultShouldBeTrue()
        {
            isPublished3 = false;

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Pubnub%20API%20Usage%20Example%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.publish().channel(channel).message(message).async(new PNCallback<PNPublishResult>() { result = ReturnNoSSLDefaultTrueCallback, error = DummyErrorCallback });
            mreNoSslPublish.WaitOne(310 * 1000);

            pubnub.EndPendingRequests();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(isPublished3, "Publish Failed with no SSL");
        }

        void ThenPublishInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    var status = receivedMessage.StatusCode;
                    if (status == 200)
                    {
                        receivedGrantMessage = true;
                    }
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        private void ReturnSuccessUnencryptPublishCodeCallback(PNPublishResult result)
        {
            if (result != null)
            {
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    isUnencryptPublished = true;
                    unEncryptPublishTimetoken = result.Timetoken;
                }
            }

            mreUnencryptedPublish.Set();
        }

        private void ReturnSuccessUnencryptObjectPublishCodeCallback(PNPublishResult result)
        {
            if (result != null)
            {
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    isUnencryptObjectPublished = true;
                    unEncryptObjectPublishTimetoken = result.Timetoken;
                }
            }

            mreUnencryptObjectPublish.Set();
        }

        private void ReturnSuccessEncryptObjectPublishCodeCallback(PNPublishResult result)
        {
            if (result != null)
            {
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    isEncryptObjectPublished = true;
                    encryptObjectPublishTimetoken = result.Timetoken;
                }
            }

            mreEncryptObjectPublish.Set();
        }

        private void ReturnSuccessEncryptPublishCodeCallback(PNPublishResult result)
        {
            if (result != null)
            {
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    isEncryptPublished = true;
                    encryptPublishTimetoken = result.Timetoken;
                }
            }

            mreEncryptPublish.Set();
        }

        private void ReturnSuccessSecretEncryptPublishCodeCallback(PNPublishResult result)
        {
            if (result != null)
            {
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    isSecretEncryptPublished = true;
                    secretEncryptPublishTimetoken = result.Timetoken;
                }
            }

            mreSecretEncryptPublish.Set();
        }

        private void CaptureUnencryptDetailedHistoryCallback(PNHistoryResult result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null && message.Length > 0 && message[0].ToString() == messageForUnencryptPublish)
                {
                    isUnencryptDetailedHistory = true;
                }
            }

            mreUnencryptDetailedHistory.Set();
        }

        private void CaptureUnencryptObjectDetailedHistoryCallback(PNHistoryResult result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null)
                {
                    string publishedMesaage = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
                    if (publishedMesaage == messageObjectForUnencryptPublish)
                    {
                        isUnencryptObjectDetailedHistory = true;
                    }
                }
            }

            mreUnencryptObjectDetailedHistory.Set();
        }

        private void CaptureEncryptObjectDetailedHistoryCallback(PNHistoryResult result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null && message.Length > 0)
                {
                    string publishedMesaage = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
                    if (publishedMesaage == messageObjectForEncryptPublish)
                    {
                        isEncryptObjectDetailedHistory = true;
                    }
                }

            }

            mreEncryptObjectDetailedHistory.Set();
        }

        private void CaptureEncryptDetailedHistoryCallback(PNHistoryResult result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null && message.Length > 0)
                {
                    string publishedMesaage = message[0].ToString();
                    if (publishedMesaage == messageForEncryptPublish)
                    {
                        isEncryptDetailedHistory = true;
                    }
                }
            }

            mreEncryptDetailedHistory.Set();
        }

        private void CaptureSecretEncryptDetailedHistoryCallback(PNHistoryResult result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null && message.Length > 0)
                {
                    string publishedMesaage = message[0].ToString();
                    if (publishedMesaage == messageForSecretEncryptPublish)
                    {
                        isSecretEncryptDetailedHistory = true;
                    }
                }

            }

            mreSecretEncryptDetailedHistory.Set();
        }

        private void ReturnSuccessComplexObjectPublishCodeCallback(PNPublishResult result)
        {
            if (result != null)
            {
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    isComplexObjectPublished = true;
                    complexObjectPublishTimetoken = result.Timetoken;
                }
            }

            mreComplexObjectPublish.Set();

        }

        private void CaptureComplexObjectDetailedHistoryCallback(PNHistoryResult result)
        {
            Console.WriteLine("CaptureComplexObjectDetailedHistoryCallback = \n" + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));

            if (result != null)
            {
                object[] message = result.Message;
                if (message != null && message.Length > 0)
                {
                    string publishedMesaage = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
                    if (publishedMesaage == messageComplexObjectForPublish)
                    {
                        isComplexObjectDetailedHistory = true;
                    }
                }
            }

            mreComplexObjectDetailedHistory.Set();
        }

        private void ReturnPublishMessageTooLargeErrorCallback(PubnubClientError pubnubError)
        {
            Console.WriteLine(pubnubError);
            if (pubnubError != null)
            {
                if (pubnubError.Message.ToLower().IndexOf("message too large") >= 0)
                {
                    isLargeMessagePublished = true;
                }
            }

            mreLaregMessagePublish.Set();
        }

        private void DummyPublishMessageTooLargeInfoCallback(PNPublishResult result)
        {
            if (result != null)
            {
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 0 && statusMessage.ToLower().IndexOf("message too large") >= 0)
                {
                    isLargeMessagePublished = true;
                }
            }

            mreLaregMessagePublish.Set();
        }

        private void ReturnSecretKeyPublishCallback(PNPublishResult result)
        {
            if (result != null)
            {
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    isPublished2 = true;
                }
            }
            mreOptionalSecretKeyPublish.Set();
        }

        private void ReturnNoSSLDefaultTrueCallback(PNPublishResult result)
        {
            if (result != null && result.StatusCode == 1 && result.StatusMessage.ToLower() == "sent")
            {
                isPublished3 = true;
            }
            mreNoSslPublish.Set();
        }

        private void ReturnSuccessSerializedObjectMessageForPublishCallback(PNPublishResult result)
        {
            if (result != null)
            {
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    isSerializedObjectMessagePublished = true;
                    serializedMessagePublishTimetoken = result.Timetoken;
                }
            }

            mreSerializedObjectMessageForPublish.Set();
        }

        private void CaptureSerializedMessagePublishDetailedHistoryCallback(PNHistoryResult result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null && message.Length > 0)
                {
                    string publishedMesaage = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
                    if (publishedMesaage == serializedObjectMessageForPublish)
                    {
                        isSerializedObjectMessageDetailedHistory = true;
                    }
                }
            }

            mreSerializedMessagePublishDetailedHistory.Set();
        }


        private void DummyErrorCallback(PubnubClientError result)
        {
            if (result != null)
            {
                Console.WriteLine(result.Message);
            }
        }

    }
}
