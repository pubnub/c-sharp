using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Threading.Tasks;
using System;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenMessageCountIsRequested : TestHarness
    {
        private static ManualResetEvent unittestManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static bool receivedGrantMessage = false;

        private static string currentUnitTestCase = "";
        private static string channelGroupName = "hello_my_group";
        private static string channelName1 = "hello_my_channel_1";
        private static string channelName2 = "hello_my_channel_2";
        private static string authToken;

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

            if (!PubnubCommon.PAMServerSideGrant) { return; }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channels\":{\"hello_my_channel1\":{\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1,\"d\":0}}},\"hello_my_channel2\":{\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1,\"d\":0}}}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    //.WithParameter("auth", authKey)
                    .WithParameter("channel", string.Format("{0},{1}",channelName1, channelName2))
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "MhmxFFbUb_HlzWqTuvJAMRjAb3fgP9dbykaiPsSZuUc=")
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
        public static void ThenChannel1Timetoken1ShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannel1Timetoken1ShouldReturnSuccess";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = true
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config, authToken);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName1)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "yo21VoxIksrH3Iozeaz5Zw4BX18N3vU9PLa-zVxRXsU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            unittestManualEvent = new ManualResetEvent(false);

            pubnub.MessageCounts()
                .Channels(new[] { channelName1 })
                .ChannelsTimetoken(new long[] { 15505396580138884 })
                .Execute(new PNMessageCountResultExt((r,s)=> 
                    {
                        if (r != null && r.Channels != null)
                        {
                            receivedMessage = true;
                        }
                        unittestManualEvent.Set();
                    }));
            Thread.Sleep(1000);

            unittestManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenMessageCountIsRequested -> ThenChannel1Timetoken1ShouldReturnSuccess failed.");

        }

        [Test]
#if NET40
        public static void ThenWithAsyncChannel1Timetoken1ShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncChannel1Timetoken1ShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenWithAsyncChannel1Timetoken1ShouldReturnSuccess";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = true
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config, authToken);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName1)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "yo21VoxIksrH3Iozeaz5Zw4BX18N3vU9PLa-zVxRXsU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            unittestManualEvent = new ManualResetEvent(false);

#if NET40
            PNResult<PNMessageCountResult> resp = Task.Factory.StartNew(async () => await pubnub.MessageCounts()
                .Channels(new[] { channelName1 })
                .ChannelsTimetoken(new long[] { 15505396580138884 })
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNMessageCountResult> resp = await pubnub.MessageCounts()
                .Channels(new[] { channelName1 })
                .ChannelsTimetoken(new long[] { 15505396580138884 })
                .ExecuteAsync();
#endif
            if (resp.Result != null && resp.Result.Channels != null)
            {
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenMessageCountIsRequested -> ThenChannel1Timetoken1ShouldReturnSuccess failed.");

        }

        [Test]
        public static void ThenChannel2Timetoken2ShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannel1Timetoken1ShouldReturnSuccess";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = true
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config, authToken);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName2)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "yo21VoxIksrH3Iozeaz5Zw4BX18N3vU9PLa-zVxRXsU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            unittestManualEvent = new ManualResetEvent(false);

            pubnub.MessageCounts()
                .Channels(new[] { channelName1, channelName2 })
                .ChannelsTimetoken(new long[] { 15505396580138884, 15505396580138884 })
                .Execute(new PNMessageCountResultExt((r, s) =>
                {
                    if (r != null && r.Channels != null)
                    {
                        receivedMessage = true;
                    }
                    unittestManualEvent.Set();
                }));
            Thread.Sleep(1000);

            unittestManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenMessageCountIsRequested -> ThenChannel1Timetoken1ShouldReturnSuccess failed.");

        }

        [Test]
#if NET40
        public static void ThenWithAsyncChannel2Timetoken2ShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncChannel2Timetoken2ShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenWithAsyncChannel2Timetoken2ShouldReturnSuccess";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = true
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config, authToken);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName2)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "yo21VoxIksrH3Iozeaz5Zw4BX18N3vU9PLa-zVxRXsU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            unittestManualEvent = new ManualResetEvent(false);

#if NET40
            PNResult<PNMessageCountResult> resp = Task.Factory.StartNew(async () => await pubnub.MessageCounts()
                .Channels(new[] { channelName1, channelName2 })
                .ChannelsTimetoken(new long[] { 15505396580138884, 15505396580138884 })
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNMessageCountResult> resp = await pubnub.MessageCounts()
                .Channels(new[] { channelName1, channelName2 })
                .ChannelsTimetoken(new long[] { 15505396580138884, 15505396580138884 })
                .ExecuteAsync();
#endif
            if (resp.Result != null && resp.Result.Channels != null)
            {
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenMessageCountIsRequested -> ThenWithAsyncChannel2Timetoken2ShouldReturnSuccess failed.");

        }

        [Test]
        public static async Task ThenMessageCountShouldReflectPublishedMessages()
        {
            string testChannel = $"foo.test_channel_{new Random().Next(1000, 9999)}";
            string customUuid = "mytestuuid";

            PNConfiguration config = new PNConfiguration(new UserId(customUuid))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config);

            // Step 1: Check initial message count (should be 0)
            PNResult<PNMessageCountResult> initialCountResult = await pubnub.MessageCounts()
                .Channels(new[] { testChannel })
                .ChannelsTimetoken(new long[] { DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 10000 })
                .ExecuteAsync();

            Assert.IsNotNull(initialCountResult, "Initial count result should not be null");
            Assert.IsNotNull(initialCountResult.Result, "Initial count result data should not be null");
            Assert.IsNotNull(initialCountResult.Result.Channels, "Initial count channels should not be null");
            Assert.IsTrue(initialCountResult.Result.Channels.ContainsKey(testChannel), "Initial count should include test channel");
            Assert.AreEqual(0, initialCountResult.Result.Channels[testChannel], "Initial message count should be 0");

            // Step 2: Publish a message
            long timeToken = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 10000;
            string testMessage = "Test message for counting";
            PNResult<PNPublishResult> publishResult = await pubnub.Publish()
                .Channel(testChannel)
                .Message(testMessage)
                .ExecuteAsync();

            Assert.IsNotNull(publishResult, "Publish result should not be null");
            Assert.IsNotNull(publishResult.Result, "Publish result data should not be null");
            Assert.IsFalse(publishResult.Status.Error, "Publish should not have errors");
            Assert.IsTrue(publishResult.Result.Timetoken > 0, "Publish should return valid timetoken");

            await Task.Delay(5000);

            // Step 3: Check message count after publishing (should be 1)
            PNResult<PNMessageCountResult> finalCountResult = await pubnub.MessageCounts()
                .Channels(new[] { testChannel })
                .ChannelsTimetoken(new long[] { timeToken })
                .ExecuteAsync();

            Assert.IsNotNull(finalCountResult, "Final count result should not be null");
            Assert.IsNotNull(finalCountResult.Result, "Final count result data should not be null");
            Assert.IsNotNull(finalCountResult.Result.Channels, "Final count channels should not be null");
            Assert.IsTrue(finalCountResult.Result.Channels.ContainsKey(testChannel), "Final count should include test channel");
            Assert.AreEqual(1, finalCountResult.Result.Channels[testChannel], "Final message count should be 1");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
    }
}
