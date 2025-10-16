using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenPushIsRequested : TestHarness
    {
        private static bool receivedMessage;
        private static bool receivedGrantMessage;

        private static ManualResetEvent mrePush = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);

        private static string channel = "hello_my_channel";
        private static string authKey = "myauth";
        private static long publishTimetoken = 0;
        private static string currentTestCase = "";
        private static int manualResetEventWaitTimeout = 310 * 1000;
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
        public static void ThenPublishMpnsToastShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsToastShouldReturnSuccess";

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
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config, authToken);

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
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(dicToast)
                .Execute(new PNPublishResultExt((result, status)=> 
                {
                    if (result != null)
                    {
                        receivedMessage = true;
                    }
                    publishManualEvent.Set();
                }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "Toast Publish Failed");
        }

        [Test]
        public static void ThenPublishMpnsFlipTileShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsFlipTileShouldReturnSuccess";

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
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config, authToken);

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
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish().Channel(channel).Message(dicTile)
                .Execute(new PNPublishResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        receivedMessage = true;
                    }
                    publishManualEvent.Set();
                }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Flip Tile Publish Failed");
        }

        [Test]
        public static void ThenPublishMpnsCycleTileShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsCycleTileShouldReturnSuccess";

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
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config, authToken);

            string channel = "hello_my_channel";

            MpnsCycleTileNotification tile = new MpnsCycleTileNotification();
            tile.title = "front title";
            tile.count = 2;
            tile.images = new [] { "Assets/Tiles/pubnub1.png", "Assets/Tiles/pubnub2.png", "Assets/Tiles/pubnub3.png", "Assets/Tiles/pubnub4.png" };

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
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish().Channel(channel).Message(dicTile)
                .Execute(new PNPublishResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        receivedMessage = true;
                    }
                    publishManualEvent.Set();
                }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Cycle Tile Publish Failed");
        }

        [Test]
        public static void ThenPublishMpnsIconicTileShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            publishTimetoken = 0;
            currentTestCase = "ThenPublishMpnsIconicTileShouldReturnSuccess";

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
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

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
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish().Channel(channel).Message(dicTile)
                .Execute(new PNPublishResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        receivedMessage = true;
                    }
                    publishManualEvent.Set();
                }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Iconic Tile Publish Failed");
        }

        [Test]
        public static void ThenAuditPushChannelProvisionsShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenAuditPushChannelProvisionsShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on AuditPushChannelProvisions");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            mrePush = new ManualResetEvent(false);

            pubnub.AuditPushChannelProvisions().DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b").PushType(PNPushType.APNS)
                .Execute(new PNPushListProvisionsResultExt((result, status)=> 
                {
                    if (result != null)
                    {
                        receivedMessage = true;
                    }
                    mrePush.Set();
                }));
            mrePush.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "AuditPushChannelProvisions Failed");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncAuditPushChannelProvisionsShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncAuditPushChannelProvisionsShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenWithAsyncAuditPushChannelProvisionsShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on AuditPushChannelProvisions");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            mrePush = new ManualResetEvent(false);

#if NET40
            PNResult<PNPushListProvisionsResult> resp = Task.Factory.StartNew(async () => await pubnub.AuditPushChannelProvisions().DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b").PushType(PNPushType.APNS)
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNPushListProvisionsResult> resp = await pubnub.AuditPushChannelProvisions().DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b").PushType(PNPushType.APNS)
                .ExecuteAsync();
#endif
            if (resp.Result != null)
            {
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "With Async AuditPushChannelProvisions Failed");
        }

        [Test]
        public static void ThenAPNS2AddDeviceToPushChannelShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenAPNS2AddDeviceToPushChannelShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on AddPushNotificationsOnChannels");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            mrePush = new ManualResetEvent(false);

            pubnub.AddPushNotificationsOnChannels()
                .Channels(new string[] { channel })
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .Execute(new PNPushAddChannelResultExt((result, status) => 
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        receivedMessage = true;
                    }
                    mrePush.Set();
                }));
            mrePush.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenAPNS2AddDeviceToPushChannelShouldReturnSuccess Failed");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncAPNS2AddDeviceToPushChannelShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncAPNS2AddDeviceToPushChannelShouldReturnSuccess()
#endif        
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenWithAsyncAPNS2AddDeviceToPushChannelShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on AddPushNotificationsOnChannels");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            mrePush = new ManualResetEvent(false);

#if NET40
            PNResult<PNPushAddChannelResult> resp = Task.Factory.StartNew(async () => await pubnub.AddPushNotificationsOnChannels()
                .Channels(new string[] { channel })
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNPushAddChannelResult> resp = await pubnub.AddPushNotificationsOnChannels()
                .Channels(new string[] { channel })
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .ExecuteAsync();
#endif
            if (resp.Result != null)
            {
                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(resp.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenWithAsyncAPNS2AddDeviceToPushChannelShouldReturnSuccess Failed");
        }

        [Test]
        public static void ThenAPNS2RemovePushChannelFromDeviceShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenAPNS2RemovePushChannelFromDeviceShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on RemovePushNotificationsFromChannels");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            mrePush = new ManualResetEvent(false);

            pubnub.RemovePushNotificationsFromChannels()
                .Channels(new string[] { channel })
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .Execute(new PNPushRemoveChannelResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        receivedMessage = true;
                    }
                    mrePush.Set();
                }));
            mrePush.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenAPNS2RemovePushChannelFromDeviceShouldReturnSuccess Failed");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncAPNS2RemovePushChannelFromDeviceShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncAPNS2RemovePushChannelFromDeviceShouldReturnSuccess()
#endif        
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenWithAsyncAPNS2RemovePushChannelFromDeviceShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on RemovePushNotificationsFromChannels");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            mrePush = new ManualResetEvent(false);

#if NET40
            PNResult<PNPushRemoveChannelResult> resp = Task.Factory.StartNew(async () => await pubnub.RemovePushNotificationsFromChannels()
                .Channels(new string[] { channel })
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNPushRemoveChannelResult> resp = await pubnub.RemovePushNotificationsFromChannels()
                .Channels(new string[] { channel })
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .ExecuteAsync();
#endif
            if (resp.Result != null)
            {
                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(resp.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenWithAsyncAPNS2RemovePushChannelFromDeviceShouldReturnSuccess Failed");
        }

        [Test]
        public static void ThenAPNS2ListPushChannelsFromDeviceShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenAPNS2ListPushChannelsFromDeviceShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on AuditPushChannelProvisions");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            mrePush = new ManualResetEvent(false);

            pubnub.AuditPushChannelProvisions()
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .Execute(new PNPushListProvisionsResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        receivedMessage = true;
                    }
                    mrePush.Set();
                }));
            mrePush.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenAPNS2ListPushChannelsFromDeviceShouldReturnSuccess Failed");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncAPNS2ListPushChannelsFromDeviceShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncAPNS2ListPushChannelsFromDeviceShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenWithAsyncAPNS2ListPushChannelsFromDeviceShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on AuditPushChannelProvisions");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

#if NET40
            PNResult<PNPushListProvisionsResult> resp = Task.Factory.StartNew(async () => await pubnub.AuditPushChannelProvisions()
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNPushListProvisionsResult> resp = await pubnub.AuditPushChannelProvisions()
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .ExecuteAsync();
#endif
            if (resp.Result != null)
            {
                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(resp.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenAPNS2ListPushChannelsFromDeviceShouldReturnSuccess Failed");
        }

        [Test]
        public static void ThenAPNS2RemoveDeviceFromPushShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenAPNS2RemoveDeviceFromPushShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on RemoveAllPushNotificationsFromDeviceWithPushToken");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            mrePush = new ManualResetEvent(false);

            pubnub.RemoveAllPushNotificationsFromDeviceWithPushToken()
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .Execute(new PNPushRemoveAllChannelsResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        receivedMessage = true;
                    }
                    mrePush.Set();
                }));
            mrePush.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenAPNS2RemoveDeviceFromPushShouldReturnSuccess Failed");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncAPNS2RemoveDeviceFromPushShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncAPNS2RemoveDeviceFromPushShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenAPNS2RemoveDeviceFromPushShouldReturnSuccess";

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Cannot run static unit test on ThenWithAsyncAPNS2RemoveDeviceFromPushShouldReturnSuccess");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.PublishKey = PubnubCommon.PublishKey;
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

#if NET40
            PNResult<PNPushRemoveAllChannelsResult> resp = Task.Factory.StartNew(async () => await pubnub.RemoveAllPushNotificationsFromDeviceWithPushToken()
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNPushRemoveAllChannelsResult> resp = await pubnub.RemoveAllPushNotificationsFromDeviceWithPushToken()
                .DeviceId("4e71acc275a8eeb400654d923724c073956661455697c92ca6c5438f2c19aa7b")
                .PushType(PNPushType.APNS2)
                .Environment(PushEnvironment.Development)
                .Topic("My Sample Topic")
                .ExecuteAsync();
#endif
            if (resp.Result != null)
            {
                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(resp.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenWithAsyncAPNS2RemoveDeviceFromPushShouldReturnSuccess Failed");
        }

        [Test]
        public static void ThenGCMToUrlStringShouldReturnFcm()
        {
            // Test that GCM enum converts to 'fcm' string for URL compatibility
            string gcmUrlString = PNPushType.GCM.ToUrlString();
            Assert.AreEqual("fcm", gcmUrlString, "GCM should convert to 'fcm' for URL compatibility");
            
            // Test that FCM also returns 'fcm'
            string fcmUrlString = PNPushType.FCM.ToUrlString();
            Assert.AreEqual("fcm", fcmUrlString, "FCM should return 'fcm'");
            
            // Test other push types return correct lowercase values
            Assert.AreEqual("apns", PNPushType.APNS.ToUrlString(), "APNS should return 'apns'");
            Assert.AreEqual("apns2", PNPushType.APNS2.ToUrlString(), "APNS2 should return 'apns2'");
        }
    }
}
