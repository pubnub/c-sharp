using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenMessageDeletedFromChannel : TestHarness
    {
        private static ManualResetEvent deleteMessageManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static bool receivedGrantMessage = false;

        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string authToken;
        private static string currentTestCase = "";

        private static Pubnub pubnub;

        private static Server server;

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

            if (!PubnubCommon.PAMServerSideGrant)
            {
                return;
            }

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    //.WithParameter("auth", authKey)
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
        public static void ThenDeleteMessageShouldReturnSuccessMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenDeleteMessageShouldReturnSuccessMessage";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            pubnub = createPubNubInstance(config);
            pubnub.SetAuthToken(authToken);

            string expected = "{\"status\": 200, \"error\": false, \"error_message\": \"\"}";

            server.AddRequest(new Request()
                    .WithMethod("DELETE")
                    .WithPath(string.Format("/v3/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "AQjIVGOI59CyaHrRG18XRZmqRjgWdzbbf9icO0Yzxo4=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;
            deleteMessageManualEvent = new ManualResetEvent(false);
            pubnub.DeleteMessages().Channel(channel).Execute(new PNDeleteMessageResultExt((result, status) => 
            {
                if (result != null && !status.Error)
                {
                    receivedMessage = true;
                }
                Debug.WriteLine("DeleteMessage Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Debug.WriteLine("DeleteMessage PNStatus => Status = : " + status.StatusCode.ToString());
                deleteMessageManualEvent.Set();
            }));
            deleteMessageManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "ThenDeleteMessageShouldReturnSuccessMessage - DeleteMessages Result not expected");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncDeleteMessageShouldReturnSuccessMessage()
#else
        public static async Task ThenWithAsyncDeleteMessageShouldReturnSuccessMessage()
#endif
        {
            server.ClearRequests();

            currentTestCase = "ThenWithAsyncDeleteMessageShouldReturnSuccessMessage";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            pubnub = createPubNubInstance(config);
            pubnub.SetAuthToken(authToken);

            string expected = "{\"status\": 200, \"error\": false, \"error_message\": \"\"}";

            server.AddRequest(new Request()
                    .WithMethod("DELETE")
                    .WithPath(string.Format("/v3/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "AQjIVGOI59CyaHrRG18XRZmqRjgWdzbbf9icO0Yzxo4=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNDeleteMessageResult> resp = Task.Factory.StartNew(async () => await pubnub.DeleteMessages().Channel(channel).ExecuteAsync()).Result.Result;
#else
            PNResult<PNDeleteMessageResult> resp = await pubnub.DeleteMessages().Channel(channel).ExecuteAsync();
#endif
            if (resp.Result != null && resp.Status.StatusCode == 200 && !resp.Status.Error)
            {
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "ThenWithAsyncDeleteMessageShouldReturnSuccessMessage - DeleteMessages Result not expected");
        }
    }
}
