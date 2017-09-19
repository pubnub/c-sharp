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
        private static bool receivedMessage = false;
        private static bool receivedGrantMessage = false;

        private static ManualResetEvent mrePush = new ManualResetEvent(false);
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

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedGrantMessage, "WhenPushIsRequested Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void ThenPublishMpnsToastShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsToastShouldReturnSuccess";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            MpnsToastNotification toast = new MpnsToastNotification();
            toast.text1 = "hardcode message";
            Dictionary<string, object> dicToast = new Dictionary<string, object>();
            dicToast.Add("pn_mpns", toast);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

            string expected = "[1,\"Sent\",\"14836234233392078\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/publish/demo-36/{0}/0/{1}/0/{2}", PubnubCommon.SubscribeKey, channel, "%7B%22pn_mpns%22%3A%7B%22type%22%3A%22toast%22%2C%22text1%22%3A%22hardcode%20message%22%2C%22text2%22%3A%22%22%2C%22param%22%3A%22%22%7D%7D"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(dicToast).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "Toast Publish Failed");
        }

        [Test]
        public void ThenPublishMpnsFlipTileShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsFlipTileShouldReturnSuccess";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

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

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);

            string expected = "[1,\"Sent\",\"14836234233392078\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/publish/demo-36/{0}/0/{1}/0/{2}", PubnubCommon.SubscribeKey, channel, "%7B%22pn_mpns%22%3A%7B%22type%22%3A%22flip%22%2C%22delay%22%3A0%2C%22title%22%3A%22front%20title%22%2C%22count%22%3A6%2C%22small_background_image%22%3A%22%22%2C%22background_image%22%3A%22http%3A%2F%2Fcdn.flaticon.com%2Fpng%2F256%2F37985.png%22%2C%22back_background_image%22%3A%22Assets%2FTiles%2Fpubnub3.png%22%2C%22back_content%22%3A%22back%20message%22%2C%22back_title%22%3A%22back%20title%22%2C%22wide_background_image%22%3A%22%22%2C%22wide_back_background_image%22%3A%22%22%2C%22wide_back_content%22%3A%22%22%7D%7D"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish().Channel(channel).Message(dicTile).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Flip Tile Publish Failed");
        }

        [Test]
        public void ThenPublishMpnsCycleTileShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsCycleTileShouldReturnSuccess";

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

            MpnsCycleTileNotification tile = new MpnsCycleTileNotification();
            tile.title = "front title";
            tile.count = 2;
            tile.images = new string[] { "Assets/Tiles/pubnub1.png", "Assets/Tiles/pubnub2.png", "Assets/Tiles/pubnub3.png", "Assets/Tiles/pubnub4.png" };

            Dictionary<string, object> dicTile = new Dictionary<string, object>();
            dicTile.Add("pn_mpns", tile);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);

            string expected = "[1,\"Sent\",\"14836234233392078\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/publish/demo-36/{0}/0/{1}/0/{2}", PubnubCommon.SubscribeKey, channel, "%7B%22pn_mpns%22%3A%7B%22type%22%3A%22cycle%22%2C%22delay%22%3A0%2C%22title%22%3A%22front%20title%22%2C%22count%22%3A2%2C%22small_background_image%22%3A%22%22%2C%22images%22%3A%5B%22Assets%2FTiles%2Fpubnub1.png%22%2C%22Assets%2FTiles%2Fpubnub2.png%22%2C%22Assets%2FTiles%2Fpubnub3.png%22%2C%22Assets%2FTiles%2Fpubnub4.png%22%5D%7D%7D"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish().Channel(channel).Message(dicTile).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Cycle Tile Publish Failed");
        }

        [Test]
        public void ThenPublishMpnsIconicTileShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsIconicTileShouldReturnSuccess";

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

            MpnsIconicTileNotification tile = new MpnsIconicTileNotification();
            tile.title = "front title";
            tile.count = 2;
            tile.wide_content_1 = "my wide content";

            Dictionary<string, object> dicTile = new Dictionary<string, object>();
            dicTile.Add("pn_mpns", tile);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            publishManualEvent = new ManualResetEvent(false);

            string expected = "[1,\"Sent\",\"14836234233392078\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/publish/demo-36/{0}/0/{1}/0/{2}", PubnubCommon.SubscribeKey, channel, "%7B%22pn_mpns%22%3A%7B%22type%22%3A%22iconic%22%2C%22delay%22%3A0%2C%22title%22%3A%22front%20title%22%2C%22count%22%3A2%2C%22icon_image%22%3A%22%22%2C%22small_icon_image%22%3A%22%22%2C%22background_color%22%3A%22%22%2C%22wide_content_1%22%3A%22my%20wide%20content%22%2C%22wide_content_2%22%3A%22%22%2C%22wide_content_3%22%3A%22%22%7D%7D"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish().Channel(channel).Message(dicTile).Async(new UTPublishResult());
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Iconic Tile Publish Failed");
        }

        [Test]
        public void ThenAuditPushChannelProvisionsShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenAuditPushChannelProvisionsShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on AuditPushChannelProvisions");
                return;
            }

            PNConfiguration config = new PNConfiguration()
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            mrePush = new ManualResetEvent(false);

            pubnub.AuditPushChannelProvisions().DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b").PushType(PNPushType.APNS).Async(new UTAuditPushChannel());
            mrePush.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "AuditPushChannelProvisions Failed");
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
                            receivedMessage = true;
                            break;
                        default:
                            break;
                    }
                }

                publishManualEvent.Set();
            }
        };

        public class UTAuditPushChannel : PNCallback<PNPushListProvisionsResult>
        {
            public override void OnResponse(PNPushListProvisionsResult result, PNStatus status)
            {
                receivedMessage = true;
                mrePush.Set();
            }
        }
    }
}
