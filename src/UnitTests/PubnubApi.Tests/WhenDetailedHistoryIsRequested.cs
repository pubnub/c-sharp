using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenDetailedHistoryIsRequested : TestHarness
    {
        //ManualResetEvent mreMessageCount10ReverseTrue = new ManualResetEvent(false);
        //ManualResetEvent mreMessageStartReverseTrue = new ManualResetEvent(false);

        private static ManualResetEvent historyManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent timeManualEvent = new ManualResetEvent(false);

        const string messageForNoStorePublish = "Pubnub Messaging With No Storage";
        const string messageForPublish = "Pubnub Messaging API 1";

        //bool message10ReverseTrueReceived = false;
        //bool messageStartReverseTrue = false;
        private static bool receivedGrantMessage = false;
        private static bool receivedMessage = false;

        int expectedCountAtStartTimeWithReverseTrue = 0;
        private static long currentTimetoken = 0;
        //bool isPublished = false;
        //long publishTimetokenForHistory = 0;

        //int manualResetEventsWaitTimeout = 310 * 1000;
        List<int> firstPublishSet;
        List<double> secondPublishSet;
        private static List<object> historyMessageList = new List<object>();

        long starttime = Int64.MaxValue;
        long midtime = Int64.MaxValue;
        long endtime = Int64.MaxValue;

        private static long publishTimetoken = 0;
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

            string channel = "hello_my_channel";

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

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenDetailedHistoryIsRequested Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void DetailHistoryNoStoreShouldNotGetMessage()
        {
            server.ClearRequests();

            receivedMessage = true;
            publishTimetoken = 0;
            currentTestCase = "DetailHistoryNoStoreShouldNotGetMessage";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";
            string message = messageForNoStorePublish;

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20With%20No%20Storage%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("store","0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish().Channel(channel).Message(message).ShouldStore(false).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "No Store Publish Failed");
            }
            else
            {
                receivedMessage = false;

                Thread.Sleep(1000);

                historyManualEvent = new ManualResetEvent(false);

                expected = "[[\"Pubnub Messaging API 1\"],14715432709547189,14715432709547189]";
                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("start", "14715322883933786")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.History().Channel(channel)
                    .Start(publishTimetoken)
                    .Reverse(false)
                    .IncludeTimetoken(false)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(!receivedMessage, "Message stored for Publish when no store is expected");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void DetailHistoryShouldReturnDecryptMessage()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "DetailHistoryShouldReturnDecryptMessage";

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

            string channel = "hello_my_channel";
            string message = messageForPublish;

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22f42pIQcWZ9zbTbH8cyLwByD%2FGsviOE0vcREIEVPARR0%3D%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish().Channel(channel).Message(message).ShouldStore(true).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "Encrypted message Publish Failed");
            }
            else
            {
                receivedMessage = false;

                Thread.Sleep(1000);

                historyManualEvent = new ManualResetEvent(false);

                expected = "[[{\"message\":\"f42pIQcWZ9zbTbH8cyLwByD/GsviOE0vcREIEVPARR0=\",\"timetoken\":14715322883933786}],14834460344901569,14834460344901569]";
                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "1")
                        .WithParameter("end", "14715322883933786")
                        .WithParameter("include_token","true")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("start", "14715322883933785")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.History()
                    .Channel(channel)
                    .Start(publishTimetoken-1)
                    .End(publishTimetoken)
                    .Count(1)
                    .Reverse(false)
                    .IncludeTimetoken(true)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedMessage, "Encrypted message not showed up in history");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void DetailHistoryCount10ReturnsRecords()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "DetailHistoryCount10ReturnsRecords";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            historyManualEvent = new ManualResetEvent(false);

            string expected = expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("count", "10")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.History().Channel(channel)
                .Count(10)
                .IncludeTimetoken(false)
                .Async(new UTHistoryResult());

            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Detailed History Failed");
        }

        [Test]
        public void DetailHistoryCount10ReverseTrueReturnsRecords()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "DetailHistoryCount10ReverseTrueReturnsRecords";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";

            historyManualEvent = new ManualResetEvent(false);

            string expected = expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("count", "10")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("reverse","true")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.History().Channel(channel)
                .Count(10)
                .Reverse(true)
                .IncludeTimetoken(false)
                .Async(new UTHistoryResult());

            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Detailed History Failed");
        }

        [Test]
        public void DetailedHistoryStartWithReverseTrue()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "DetailedHistoryStartWithReverseTrue";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);
            string expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Async(new TimeResult());
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            for (int index = 0; index < 10; index++)
            {
                receivedMessage = false;
                publishManualEvent = new ManualResetEvent(false);

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, String.Format("%22DetailedHistoryStartTimeWithReverseTrue%20{0}%22",index)))
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Publish().Channel(channel)
                    .Message(string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index))
                    .Async(new UTPublishResult());

                publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                if (!receivedMessage)
                {
                    break;
                }
            }

            if (receivedMessage)
            {
                Thread.Sleep(2000);

                historyManualEvent = new ManualResetEvent(false);

                expected = expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("start", "1356998400")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.History().Channel(channel)
                    .Start(currentTimetoken)
                    .Reverse(false)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Detailed History with Start and Reverse True Failed");
        }

        [Test]
        [ExpectedException(typeof(MissingMemberException))]
        public void DetailHistoryWithNullKeysReturnsError()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "DetailHistoryWithNullKeysReturnsError";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = null,
                SubscribeKey = null,
                SecretKey = null,
                CipherKey = null,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            historyManualEvent = new ManualResetEvent(false);

            pubnub.History().Channel(channel)
                .Count(10)
                .Reverse(true)
                .IncludeTimetoken(false)
                .Async(new UTHistoryResult());

            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "Detailed History With Null Keys Failed");
        }

        [Test]
        public void DetailHistoryShouldReturnUnencrypedSecretMessage()
        {
            server.ClearRequests();

            currentTestCase = "DetailHistoryShouldReturnUnencrypedSecretMessage";
            CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "DetailHistoryShouldReturnUnencrypedSecretMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnUnencrypedMessage()
        {
            server.ClearRequests();

            currentTestCase = "DetailHistoryShouldReturnUnencrypedMessage";
            CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "DetailHistoryShouldReturnUnencrypedMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnUnencrypedSecretSSLMessage()
        {
            server.ClearRequests();

            currentTestCase = "DetailHistoryShouldReturnUnencrypedSecretSSLMessage";
            CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "DetailHistoryShouldReturnUnencrypedSecretSSLMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnUnencrypedSSLMessage()
        {
            server.ClearRequests();

            currentTestCase = "DetailHistoryShouldReturnUnencrypedSSLMessage";
            CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "DetailHistoryShouldReturnUnencrypedSSLMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnEncrypedMessage()
        {
            server.ClearRequests();

            currentTestCase = "DetailHistoryShouldReturnEncrypedMessage";
            CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "DetailHistoryShouldReturnEncrypedMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnEncrypedSecretMessage()
        {
            server.ClearRequests();

            currentTestCase = "DetailHistoryShouldReturnEncrypedSecretMessage";
            CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "DetailHistoryShouldReturnEncrypedSecretMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnEncrypedSecretSSLMessage()
        {
            server.ClearRequests();

            currentTestCase = "DetailHistoryShouldReturnEncrypedSecretSSLMessage";
            CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "DetailHistoryShouldReturnEncrypedSecretSSLMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnEncrypedSSLMessage()
        {
            server.ClearRequests();

            currentTestCase = "DetailHistoryShouldReturnEncrypedSSLMessage";
            CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "DetailHistoryShouldReturnEncrypedSSLMessage - Detailed History Result not expected");
        }

        private void CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            server.ClearRequests();

            receivedMessage = false;
            int totalMessages = 10;
            starttime = 0;
            midtime = 0;
            endtime = 0;

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

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            string expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Async(new TimeResult());
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            starttime = currentTimetoken;

            Console.WriteLine(string.Format("Start Time = {0}", starttime));
            firstPublishSet = new List<int>();

            for (int index = 0; index < totalMessages / 2; index++)
            {
                object message = index;
                firstPublishSet.Add(index);

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                publishManualEvent = new ManualResetEvent(false);

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, index))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Publish().Channel(channel).Message(message).ShouldStore(true).Async(new UTPublishResult());
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Async(new TimeResult());
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            midtime = currentTimetoken;

            Console.WriteLine(string.Format("Mid Time = {0}", midtime));
            secondPublishSet = new List<double>();
            int arrayIndex = 0;

            for (int index = totalMessages / 2; index < totalMessages; index++)
            {
                receivedMessage = false;

                object message = (double)index + 0.1D;
                secondPublishSet.Add((double)index + 0.1D);
                arrayIndex++;

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                publishManualEvent = new ManualResetEvent(false);

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, message))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Publish().Channel(channel).Message(message).ShouldStore(true).Async(new UTPublishResult());
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Async(new TimeResult());
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            endtime = currentTimetoken;

            Console.WriteLine(string.Format("End Time = {0}", endtime));

            Thread.Sleep(1000);

            Console.WriteLine("Detailed History with Start & End");

            historyManualEvent = new ManualResetEvent(false);

            expected = "[[\"kvIeHmojsLyV1KMBo82DYQ==\",\"Ld0rZfbe4yN0Qj4V7o2BuQ==\",\"zNlnhYco9o6a646+Ox6ksg==\",\"mR8EEMx154BBHU3OOa+YjQ==\",\"v+viLoq0Gj2docUMAYyoYg==\"],14835539837820376,14835539843298232]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.History().Channel(channel)
                .Start(starttime)
                .End(midtime)
                .Count(totalMessages / 2)
                .Reverse(true)
                .IncludeTimetoken(false)
                .Async(new UTHistoryResult());

            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            foreach (object item in historyMessageList)
            {
                int num;
                if (int.TryParse(item.ToString(), out num))
                {
                    if (!firstPublishSet.Contains(num))
                    {
                        receivedMessage = false;
                        break;
                    }
                    receivedMessage = true;
                }
            }

            if (!receivedMessage)
            {
                Console.WriteLine("firstPublishSet did not match");
            }
            else
            {
                Console.WriteLine("DetailedHistory with start & reverse = true");

                historyManualEvent = new ManualResetEvent(false);

                expected = "[[\"F2ZPfJnzuU34VKe24ds81A==\",\"2K/TO5WADvJRhvX7Zk0IpQ==\",\"oWOYyGxkWFJ1gpJxhcyzjA==\",\"LwEzvPCHdM8Yagg6oKknvg==\",\"/jjH/PT4NrK5HHjDT2KAlQ==\"],14835549524365492,14835549537755368]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.History().Channel(channel)
                    .Start(midtime)
                    .Count(totalMessages / 2)
                    .Reverse(true)
                    .IncludeTimetoken(false)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                foreach (object item in historyMessageList)
                {
                    int num;
                    if (int.TryParse(item.ToString(), out num))
                    {
                        if (!secondPublishSet.Contains(num))
                        {
                            receivedMessage = false;
                            break;
                        }
                        receivedMessage = true;
                    }
                }

                if (!receivedMessage)
                {
                    Console.WriteLine("secondPublishSet did not match");
                }
                else
                {
                    Console.WriteLine("DetailedHistory with start & reverse = false");
                    historyManualEvent = new ManualResetEvent(false);

                    expected = expected = "[[\"kvIeHmojsLyV1KMBo82DYQ==\",\"Ld0rZfbe4yN0Qj4V7o2BuQ==\",\"zNlnhYco9o6a646+Ox6ksg==\",\"mR8EEMx154BBHU3OOa+YjQ==\",\"v+viLoq0Gj2docUMAYyoYg==\"],14835550731714499,14835550737165103]";

                    server.AddRequest(new Request()
                            .WithMethod("GET")
                            .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                            .WithResponse(expected)
                            .WithStatusCode(System.Net.HttpStatusCode.OK));

                    pubnub.History().Channel(channel)
                        .Start(midtime - 1)
                        .Count(totalMessages / 2)
                        .Reverse(false)
                        .IncludeTimetoken(false)
                        .Async(new UTHistoryResult());

                    historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                    foreach (object item in historyMessageList)
                    {
                        int num;
                        if (int.TryParse(item.ToString(), out num))
                        {
                            if (!firstPublishSet.Contains(num))
                            {
                                receivedMessage = false;
                                break;
                            }
                            receivedMessage = true;
                        }
                    }
                }
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        private void CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            int totalMessages = 10;
            starttime = 0;
            midtime = 0;
            endtime = 0;

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

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            string expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Async(new TimeResult());
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            starttime = currentTimetoken;

            Console.WriteLine(string.Format("Start Time = {0}", starttime));
            firstPublishSet = new List<int>();

            for (int index = 0; index < totalMessages / 2; index++)
            {
                receivedMessage = false;

                object message = index;
                firstPublishSet.Add(index);

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                publishManualEvent = new ManualResetEvent(false);

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, index))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Publish().Channel(channel).Message(message).ShouldStore(true).Async(new UTPublishResult());
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Async(new TimeResult());
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            midtime = currentTimetoken;

            Console.WriteLine(string.Format("Mid Time = {0}", midtime));
            secondPublishSet = new List<double>();
            int arrayIndex = 0;

            for (int index = totalMessages / 2; index < totalMessages; index++)
            {
                receivedMessage = false;

                object message = (double)index + 0.1D;
                secondPublishSet.Add((double)index + 0.1D);
                arrayIndex++;

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                publishManualEvent = new ManualResetEvent(false);

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, message))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Publish().Channel(channel).Message(message).ShouldStore(true).Async(new UTPublishResult());
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Async(new TimeResult());
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            endtime = currentTimetoken;

            Console.WriteLine(string.Format("End Time = {0}", endtime));

            Thread.Sleep(1000);

            Console.WriteLine("Detailed History with Start & End");

            historyManualEvent = new ManualResetEvent(false);

            expected = expected = "[[0,1,2,3,4],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.History().Channel(channel)
                .Start(starttime)
                .End(midtime)
                .Count(totalMessages / 2)
                .Reverse(true)
                .IncludeTimetoken(false)
                .Async(new UTHistoryResult());

            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            foreach (object item in historyMessageList)
            {
                int num;
                if (int.TryParse(item.ToString(), out num))
                {
                    if (!firstPublishSet.Contains(num))
                    {
                        receivedMessage = false;
                        break;
                    }
                    receivedMessage = true;
                }
            }

            if (!receivedMessage)
            {
                Console.WriteLine("firstPublishSet did not match");
            }
            else
            {
                Console.WriteLine("DetailedHistory with start & reverse = true");

                historyManualEvent = new ManualResetEvent(false);

                expected = expected = "[[5.1,6.1,7.1,8.1,9.1],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.History().Channel(channel)
                    .Start(midtime - 1)
                    .Count(totalMessages / 2)
                    .Reverse(true)
                    .IncludeTimetoken(false)
                    .Async(new UTHistoryResult());

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                foreach (object item in historyMessageList)
                {
                    int num;
                    if (int.TryParse(item.ToString(), out num))
                    {
                        if (!secondPublishSet.Contains(num))
                        {
                            receivedMessage = false;
                            break;
                        }
                        receivedMessage = true;
                    }
                }

                if (!receivedMessage)
                {
                    Console.WriteLine("secondPublishSet did not match");
                }
                else
                {
                    Console.WriteLine("DetailedHistory with start & reverse = false");
                    historyManualEvent = new ManualResetEvent(false);

                    expected = expected = "[[5.1,6.1,7.1,8.1,9.1],14715432709547189,14715432709547189]";

                    server.AddRequest(new Request()
                            .WithMethod("GET")
                            .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                            .WithResponse(expected)
                            .WithStatusCode(System.Net.HttpStatusCode.OK));

                    pubnub.History().Channel(channel)
                        .Start(midtime - 1)
                        .Count(totalMessages / 2)
                        .Reverse(false)
                        .IncludeTimetoken(false)
                        .Async(new UTHistoryResult());

                    historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                    foreach (object item in historyMessageList)
                    {
                        int num;
                        if (int.TryParse(item.ToString(), out num))
                        {
                            if (!firstPublishSet.Contains(num))
                            {
                                receivedMessage = false;
                                break;
                            }
                            receivedMessage = true;
                        }
                    }
                }

            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        //void ThenDetailedHistoryInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
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

        //void ReturnRegularPublishCodeCallback(PNPublishResult result)
        //{
        //    try
        //    {
        //        Console.WriteLine(string.Format("ReturnRegularPublishCodeCallback result = {0}", result));

        //        if (result != null)
        //        {
        //            int statusCode = result.StatusCode;
        //            string statusMessage = result.StatusMessage;
        //            if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //            {
        //                publishTimetokenForHistory = result.Timetoken;
        //                isPublished = true;
        //            }
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        mrePublish.Set();
        //    }
        //}

        //void CaptureNoStoreDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null && message.Length >= 0)
        //        {
        //            if (!message.Contains(messageForNoStorePublish))
        //            {
        //                messageReceived = false;
        //            }
        //        }
        //    }

        //    mreDetailedHistory.Set();
        //}

        //void CaptureRegularDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null && message.Length >= 0)
        //        {
        //            if (message.Contains(messageForPublish))
        //            {
        //                messageReceived = true;
        //            }
        //        }
        //    }

        //    mreDetailedHistory.Set();
        //}

        //void DetailedHistoryCount10Callback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null)
        //        {
        //            if (message.Length >= 0)
        //            {
        //                messageReceived = true;
        //            }
        //        }
        //    }

        //    mreDetailedHistory.Set();
        //}

        //void DetailedHistoryCount10ReverseTrueCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        object[] message = result.Message;
        //        if (message != null)
        //        {
        //            if (message.Length >= 0)
        //            {
        //                message10ReverseTrueReceived = true;
        //            }
        //        }
        //    }

        //    mreMessageCount10ReverseTrue.Set();
        //}

        //void DetailedHistoryStartWithReverseTrueCallback(PNHistoryResult result)
        //{
        //    int actualCountAtStartTimeWithReverseFalse = 0;
        //    if (result != null)
        //    {
        //        Console.WriteLine(string.Format("DetailedHistoryStartWithReverseTrueCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
        //        object[] message = result.Message;
        //        if (message != null)
        //        {
        //            if (message.Length >= expectedCountAtStartTimeWithReverseTrue)
        //            {
        //                foreach (object item in message)
        //                {
        //                    if (item.ToString().Contains("DetailedHistoryStartTimeWithReverseTrue"))
        //                    {
        //                        actualCountAtStartTimeWithReverseFalse++;
        //                    }
        //                }
        //                if (actualCountAtStartTimeWithReverseFalse >= expectedCountAtStartTimeWithReverseTrue)
        //                {
        //                    messageStartReverseTrue = true;
        //                }
        //            }
        //        }
        //    }
        //    mreMessageStartReverseTrue.Set();
        //}

        //void DetailedHistorySamplePublishCallback(PNPublishResult result)
        //{
        //    if (result != null)
        //    {
        //        Console.WriteLine(string.Format("DetailedHistorySamplePublishCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
        //        int statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            expectedCountAtStartTimeWithReverseTrue++;
        //        }
        //    }

        //    mrePublish.Set();
        //}

        //void DetailHistoryWithNullKeyseDummyCallback(PNHistoryResult result)
        //{
        //    mreDetailedHistory.Set();
        //}

        //void CaptureFirstPublishSetRegularDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        Console.WriteLine(string.Format("CaptureFirstPublishSetRegularDetailedHistoryCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
        //        object[] message = result.Message;
        //        if (message != null && message.Length >= 0 && firstPublishSet != null && firstPublishSet.Length == message.Length)
        //        {
        //            for (int index = 0; index < message.Length; index++)
        //            {
        //                if (firstPublishSet[index].ToString() != message[index].ToString())
        //                {
        //                    messageReceived = false;
        //                    break;
        //                }
        //                messageReceived = true;
        //            }
        //        }
        //    }

        //    mreDetailedHistory.Set();
        //}

        //void CaptureSecondPublishSetRegularDetailedHistoryCallback(PNHistoryResult result)
        //{
        //    if (result != null)
        //    {
        //        Console.WriteLine(string.Format("CaptureSecondPublishSetRegularDetailedHistoryCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
        //        object[] message = result.Message;
        //        if (message != null && message.Length >= 0 && firstPublishSet != null && firstPublishSet.Length == message.Length)
        //        {
        //            for (int index = 0; index < message.Length; index++)
        //            {
        //                if (secondPublishSet[index].ToString() != message[index].ToString())
        //                {
        //                    messageReceived = false;
        //                    break;
        //                }
        //                messageReceived = true;
        //            }
        //        }
        //    }

        //    mreDetailedHistory.Set();
        //}

        //void DummyErrorCallback(PubnubClientError result)
        //{
        //    if (currentTestCase == "DetailHistoryWithNullKeysReturnsError")
        //    {
        //        messageReceived = true;
        //        mreDetailedHistory.Set();
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
                        case "DetailHistoryNoStoreShouldNotGetMessage":
                        case "DetailHistoryShouldReturnDecryptMessage":
                        case "DetailedHistoryStartWithReverseTrue":
                        case "DetailHistoryShouldReturnUnencrypedSecretMessage":
                        case "DetailHistoryShouldReturnUnencrypedMessage":
                        case "DetailHistoryShouldReturnUnencrypedSecretSSLMessage":
                        case "DetailHistoryShouldReturnUnencrypedSSLMessage":
                        case "DetailHistoryShouldReturnEncrypedMessage":
                        case "DetailHistoryShouldReturnEncrypedSecretMessage":
                        case "DetailHistoryShouldReturnEncrypedSecretSSLMessage":
                        case "DetailHistoryShouldReturnEncrypedSSLMessage":
                            receivedMessage = true;
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
                if (result != null && status.StatusCode == 200 && !status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "DetailHistoryNoStoreShouldNotGetMessage":
                            if (result.Messages != null && result.Messages.Count > 0)
                            {
                                foreach(PNHistoryItemResult item in result.Messages)
                                {
                                    if (item.Entry != null && item.Entry.ToString() == messageForNoStorePublish && item.Timetoken == publishTimetoken)
                                    {
                                        receivedMessage = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        case "DetailHistoryShouldReturnDecryptMessage":
                            if (result.Messages != null && result.Messages.Count > 0)
                            {
                                foreach (PNHistoryItemResult item in result.Messages)
                                {
                                    if (item.Entry != null && item.Entry.ToString() == messageForPublish && item.Timetoken == publishTimetoken)
                                    {
                                        receivedMessage = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        case "DetailHistoryCount10ReturnsRecords":
                        case "DetailHistoryCount10ReverseTrueReturnsRecords":
                        case "DetailedHistoryStartWithReverseTrue":
                            if (result.Messages != null && result.Messages.Count >= 10)
                            {
                                receivedMessage = true;
                            }
                            break;
                        case "DetailHistoryShouldReturnUnencrypedSecretMessage":
                        case "DetailHistoryShouldReturnUnencrypedMessage":
                        case "DetailHistoryShouldReturnUnencrypedSecretSSLMessage":
                        case "DetailHistoryShouldReturnUnencrypedSSLMessage":
                        case "DetailHistoryShouldReturnEncrypedMessage":
                        case "DetailHistoryShouldReturnEncrypedSecretMessage":
                        case "DetailHistoryShouldReturnEncrypedSecretSSLMessage":
                        case "DetailHistoryShouldReturnEncrypedSSLMessage":
                            historyMessageList = new List<object>();
                            if (result.Messages != null && result.Messages.Count > 0)
                            {
                                foreach (PNHistoryItemResult item in result.Messages)
                                {
                                    if (item.Entry != null)
                                    {
                                        historyMessageList.Add(item.Entry);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                else if (result == null || status.StatusCode != 200 || status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "DetailHistoryWithNullKeysReturnsError":
                            receivedMessage = true;
                            break;
                        default:
                            break;
                    }
                }

                historyManualEvent.Set();

            }
        };

        public class TimeResult : PNCallback<PNTimeResult>
        {
            public override void OnResponse(PNTimeResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));

                    if (result != null)
                    {
                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            currentTimetoken = result.Timetoken;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    timeManualEvent.Set();
                }

            }
        };
    }
}
