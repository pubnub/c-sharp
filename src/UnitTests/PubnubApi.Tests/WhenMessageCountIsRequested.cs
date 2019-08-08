using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System;
using System.Diagnostics;

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
        private static string channelName1 = "hello_my_channel1";
        private static string channelName2 = "hello_my_channel2";
        private static string authKey = "myAuth";

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

            if (!PubnubCommon.PAMEnabled) { return; }

            if (PubnubCommon.PAMEnabled && !string.IsNullOrEmpty(PubnubCommon.SecretKey))
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
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channels\":{\"hello_my_channel1\":{\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1,\"d\":0}}},\"hello_my_channel2\":{\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1,\"d\":0}}}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", string.Format("{0},{1}",channelName1, channelName2))
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "MhmxFFbUb_HlzWqTuvJAMRjAb3fgP9dbykaiPsSZuUc=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().Channels(new[] { channelName1, channelName2 }).AuthKeys(new[] { authKey }).Read(true).Write(true).Manage(true).TTL(20)
                .Execute(new PNAccessManagerGrantResultExt((r,s)=> 
                {
                    if (r != null)
                    {
                        receivedGrantMessage = true;
                    }
                    grantManualEvent.Set();
                }));

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenMessageCountIsRequested Grant access failed.");
        }

        [TestFixtureTearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenChannel1Timetoken1ShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannel1Timetoken1ShouldReturnSuccess";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = true
            };
            if (PubnubCommon.PAMEnabled)
            {
                config.AuthKey = authKey;
            }

            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName1)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
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
        public static void ThenChannel2Timetoken2ShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannel1Timetoken1ShouldReturnSuccess";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                //SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                Secure = true
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName1)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
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
    }
}
