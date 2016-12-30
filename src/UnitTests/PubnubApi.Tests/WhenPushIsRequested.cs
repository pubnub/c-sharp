using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubnubApi;
using MockServer;


namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenPushIsRequested : TestHarness
    {
        private static bool receivedPublishMessage = false;
        private static bool receivedGrantMessage = false;

        ManualResetEvent mrePush = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);

        private static string channel = "hello_my_channel";
        private static string authKey = "myAuth";
        private static long publishTimetoken = 0;
        private static string currentTestCase = "";
        int manualResetEventWaitTimeout = 310 * 1000;

        private static Pubnub pubnub = null;

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
                AuthKey = authKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

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
                    .WithParameter("signature", "pOL-X541lXTpA8fNkJE3k7FjaZwo0qynAkPhBPANiCg=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedGrantMessage, "WhenPushIsRequested Grant access failed.");
        }

        [Test]
        public void ThenPublishMpnsToastShouldReturnSuccess()
        {
            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsToastShouldReturnSuccess";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            pubnub = this.createPubNubInstance(config);

            MpnsToastNotification toast = new MpnsToastNotification();
            toast.text1 = "hardcode message";
            Dictionary<string, object> dicToast = new Dictionary<string, object>();
            dicToast.Add("pn_mpns", toast);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(dicToast).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedPublishMessage, "Toast Publish Failed");
        }

        [Test]
        public void ThenPublishMpnsFlipTileShouldReturnSuccess()
        {
            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsFlipTileShouldReturnSuccess";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            pubnub = this.createPubNubInstance(config);

            MpnsFlipTileNotification tile = new MpnsFlipTileNotification();
            tile.title = "front title";
            tile.count = 6;
            tile.back_title = "back title";
            tile.back_content = "back message";
            tile.back_background_image = "Assets/Tiles/pubnub3.png";
            tile.background_image = "http://cdn.flaticon.com/png/256/37985.png";
            Dictionary<string, object> dicTile = new Dictionary<string, object>();
            dicTile.Add("pn_mpns", tile);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(dicTile).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Flip Tile Publish Failed");
        }

        [Test]
        public void ThenPublishMpnsCycleTileShouldReturnSuccess()
        {
            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsCycleTileShouldReturnSuccess";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";

            MpnsCycleTileNotification tile = new MpnsCycleTileNotification();
            tile.title = "front title";
            tile.count = 2;
            tile.images = new string[] { "Assets/Tiles/pubnub1.png", "Assets/Tiles/pubnub2.png", "Assets/Tiles/pubnub3.png", "Assets/Tiles/pubnub4.png" };

            Dictionary<string, object> dicTile = new Dictionary<string, object>();
            dicTile.Add("pn_mpns", tile);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(dicTile).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Cycle Tile Publish Failed");
        }

        [Test]
        public void ThenPublishMpnsIconicTileShouldReturnSuccess()
        {
            receivedPublishMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsIconicTileShouldReturnSuccess";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";

            MpnsIconicTileNotification tile = new MpnsIconicTileNotification();
            tile.title = "front title";
            tile.count = 2;
            tile.wide_content_1 = "my wide content";

            Dictionary<string, object> dicTile = new Dictionary<string, object>();
            dicTile.Add("pn_mpns", tile);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(dicTile).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPublishMessage, "Iconic Tile Publish Failed");
        }

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
                        case "ThenPublishMpnsToastShouldReturnSuccess":
                        case "ThenPublishMpnsFlipTileShouldReturnSuccess":
                        case "ThenPublishMpnsCycleTileShouldReturnSuccess":
                        case "ThenPublishMpnsIconicTileShouldReturnSuccess":
                            receivedPublishMessage = true;
                            break;
                        default:
                            break;
                    }
                }

                publishManualEvent.Set();
            }
        };
    }
}
