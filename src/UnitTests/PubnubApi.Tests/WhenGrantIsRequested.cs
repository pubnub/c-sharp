using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    public class PNRes
    {
        public Dictionary<string, object> usr { get; set; }
        public Dictionary<string, object> chan { get; set; }
        public Dictionary<string, object> spc { get; set; }
        public Dictionary<string, object> grp { get; set; }
    }
    public class PNCbor
    {
        public int v { get; set; }
        public long t { get; set; }
        public int ttl { get; set; }
        public PNRes res { get; set; }
        public PNRes pat { get; set; }
        public byte[] sig { get; set; }
        public Dictionary<string, object> meta { get; set; }
    }
    [TestFixture]
    public class WhenGrantIsRequested : TestHarness
    {
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent revokeManualEvent = new ManualResetEvent(false);
        private static bool receivedGrantMessage = false;
        private static bool receivedRevokeMessage = false;
        private static int multipleChannelGrantCount = 5;
        private static int multipleAuthGrantCount = 5;
        private static string currentUnitTestCase = "";
        private static string channel = "hello_my_channel";
        private static string channelGroup = "myChannelGroup";
        private static string targetUuid = "my_target_uuid";
        private static string authKey = "hello_my_authkey";
        private static string[] channelBuilder;
        private static string[] authKeyBuilder;

        private static Pubnub pubnub;

        private static Server server;

        [SetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenUserLevelWithReadWriteShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUserLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channel\":\"hello_my_channel\",\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1,\"m\":0}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", channel)
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "V6C3eRs_YSP7njOW1f9xAFpmCx5do_7D3oUGXxDClXw=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new [] { channel }).AuthKeys(new [] { authKey }).Read(true).Write(true).Manage(false).TTL(5).Execute(new GrantResult());
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess.");
            }
        }

        [Test]
#if NET40
        public static void ThenWithAsyncUserLevelWithReadWriteShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncUserLevelWithReadWriteShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenWithAsyncUserLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channel\":\"hello_my_channel\",\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1,\"m\":0}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", channel)
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "V6C3eRs_YSP7njOW1f9xAFpmCx5do_7D3oUGXxDClXw=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideGrant)
            {
#if NET40
                PNResult<PNAccessManagerGrantResult> result = Task.Factory.StartNew(async () => await pubnub.Grant().Channels(new[] { channel }).AuthKeys(new[] { authKey }).Read(true).Write(true).Manage(false).TTL(5).ExecuteAsync()).Result.Result;
#else
                PNResult<PNAccessManagerGrantResult> result = await pubnub.Grant().Channels(new[] { channel }).AuthKeys(new[] { authKey }).Read(true).Write(true).Manage(false).TTL(5).ExecuteAsync();
#endif
                if (result.Result != null && result.Result.Channels != null && result.Result.Channels.Count > 0)
                {
                    var read = result.Result.Channels[channel][authKey].ReadEnabled;
                    var write = result.Result.Channels[channel][authKey].WriteEnabled;
                    if (read && write)
                    {
                        receivedGrantMessage = true;
                    }
                }

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenWithAsyncUserLevelWithReadWriteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenWithAsyncUserLevelWithReadWriteShouldReturnSuccess.");
            }
        }


        [Test]
        public static void ThenUserLevelWithReadShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUserLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channel\":\"hello_my_channel\",\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":0,\"m\":0}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", channel)
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "0")
                    .WithParameter("signature", "ViiLvN22MUr36vgEINkwrLTa2kMOH9ztmM4Dg-bqoaE=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new [] { channel }).AuthKeys(new [] { authKey }).Read(true).Write(false).Manage(false).TTL(5).Execute(new GrantResult());
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess.");
            }
        }

        [Test]
#if NET40
        public static void ThenWithAsyncUserLevelWithReadShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncUserLevelWithReadShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenWithAsyncUserLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channel\":\"hello_my_channel\",\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":0,\"m\":0}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", channel)
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "0")
                    .WithParameter("signature", "ViiLvN22MUr36vgEINkwrLTa2kMOH9ztmM4Dg-bqoaE=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
#if NET40
                PNResult<PNAccessManagerGrantResult> result = Task.Factory.StartNew(async () => await pubnub.Grant().Channels(new[] { channel }).AuthKeys(new[] { authKey }).Read(true).Write(false).Manage(false).TTL(5).ExecuteAsync()).Result.Result;
#else
                PNResult<PNAccessManagerGrantResult> result = await pubnub.Grant().Channels(new[] { channel }).AuthKeys(new[] { authKey }).Read(true).Write(false).Manage(false).TTL(5).ExecuteAsync();
#endif
                if (result.Result != null && result.Result.Channels != null && result.Result.Channels.Count > 0)
                {
                    var read = result.Result.Channels[channel][authKey].ReadEnabled;
                    var write = result.Result.Channels[channel][authKey].WriteEnabled;
                    if (read && !write)
                    {
                        receivedGrantMessage = true;
                    }
                }

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenWithAsyncUserLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenWithAsyncUserLevelWithReadShouldReturnSuccess.");
            }
        }

        [Test]
        public static void ThenUserLevelWithWriteShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUserLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channel\":\"hello_my_channel\",\"auths\":{\"hello_my_authkey\":{\"r\":0,\"w\":1,\"m\":0}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", channel)
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "0")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "qIMvcoTYVOIhZ4oaVJtv_jcb4n_1YGtRwmxotAVI2eQ=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));


            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new [] { channel }).AuthKeys(new [] { authKey }).Read(false).Write(true).Manage(false).TTL(5).Execute(new GrantResult());
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess.");
            }
        }

        [Test]
        public static void ThenMultipleChannelGrantShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenMultipleChannelGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            channelBuilder = new string[multipleChannelGrantCount];

            for (int index = 0; index < multipleChannelGrantCount; index++)
            {
                channelBuilder[index] = String.Format("csharp-hello_my_channel-{0}", index);
            }

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channels\":{\"csharp-hello_my_channel-0\":{\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1,\"m\":0}}},\"csharp-hello_my_channel-1\":{\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1,\"m\":0}}},\"csharp-hello_my_channel-2\":{\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1,\"m\":0}}},\"csharp-hello_my_channel-3\":{\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1,\"m\":0}}},\"csharp-hello_my_channel-4\":{\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1,\"m\":0}}}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", "csharp-hello_my_channel-0%2Ccsharp-hello_my_channel-1%2Ccsharp-hello_my_channel-2%2Ccsharp-hello_my_channel-3%2Ccsharp-hello_my_channel-4")
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "aRUz_KM2JmPWMoblHezJsSJCrIjZcjhTy0UYTi_Nru0=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().AuthKeys(new [] { authKey }).Channels(channelBuilder).Read(true).Write(true).Manage(false).TTL(5).Execute(new GrantResult());
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess.");
            }
        }

        [Test]
        public static void ThenMultipleAuthGrantShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenMultipleAuthGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            channelBuilder = new string[multipleChannelGrantCount];
            authKeyBuilder = new string[multipleChannelGrantCount];

            for (int index = 0; index < multipleChannelGrantCount; index++)
            {
                channelBuilder[index] = String.Format("csharp-hello_my_channel-{0}", index);
                authKeyBuilder[index] = String.Format("AuthKey-csharp-hello_my_channel-{0}", index);
            }

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channels\":{\"csharp-hello_my_channel-0\":{\"auths\":{\"AuthKey-csharp-hello_my_channel-0\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-1\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-2\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-3\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-4\":{\"r\":1,\"w\":1,\"m\":0}}},\"csharp-hello_my_channel-1\":{\"auths\":{\"AuthKey-csharp-hello_my_channel-0\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-1\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-2\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-3\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-4\":{\"r\":1,\"w\":1,\"m\":0}}},\"csharp-hello_my_channel-2\":{\"auths\":{\"AuthKey-csharp-hello_my_channel-0\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-1\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-2\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-3\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-4\":{\"r\":1,\"w\":1,\"m\":0}}},\"csharp-hello_my_channel-3\":{\"auths\":{\"AuthKey-csharp-hello_my_channel-0\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-1\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-2\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-3\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-4\":{\"r\":1,\"w\":1,\"m\":0}}},\"csharp-hello_my_channel-4\":{\"auths\":{\"AuthKey-csharp-hello_my_channel-0\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-1\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-2\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-3\":{\"r\":1,\"w\":1,\"m\":0},\"AuthKey-csharp-hello_my_channel-4\":{\"r\":1,\"w\":1,\"m\":0}}}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", "AuthKey-csharp-hello_my_channel-0%2CAuthKey-csharp-hello_my_channel-1%2CAuthKey-csharp-hello_my_channel-2%2CAuthKey-csharp-hello_my_channel-3%2CAuthKey-csharp-hello_my_channel-4")
                    .WithParameter("channel", "csharp-hello_my_channel-0%2Ccsharp-hello_my_channel-1%2Ccsharp-hello_my_channel-2%2Ccsharp-hello_my_channel-3%2Ccsharp-hello_my_channel-4")
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "9_2ZxQiQeXroQqt3x728ebRN6f4hMPk7QtebCKSZl7Q=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(channelBuilder).AuthKeys(authKeyBuilder).Read(true).Write(true).Manage(false).TTL(5).Execute(new GrantResult());
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess.");
            }
        }

        [Test]
        public static void ThenRevokeAtUserLevelReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenRevokeAtUserLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expectedGrant = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channel\":\"hello_my_channel\",\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1,\"m\":0}}},\"service\":\"Access Manager\",\"status\":200}";
            string expectedRevoke = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":1,\"channel\":\"hello_my_channel\",\"auths\":{\"hello_my_authkey\":{\"r\":0,\"w\":0,\"m\":0}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", channel)
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "V6C3eRs_YSP7njOW1f9xAFpmCx5do_7D3oUGXxDClXw=")
                    .WithResponse(expectedGrant)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", channel)
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "0")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "0")
                    .WithParameter("signature", "MJVPMrBFTV7jo8jMp_DSn9OhIi8aikd7wru8x0sz3io=")
                    .WithResponse(expectedRevoke)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new [] { channel }).AuthKeys(new [] { authKey }).Read(true).Write(true).Manage(false).TTL(5).Execute(new GrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne();

                if (receivedGrantMessage)
                {
                    revokeManualEvent = new ManualResetEvent(false);
                    pubnub.Grant().Channels(new [] { channel }).AuthKeys(new [] { authKey }).Read(false).Write(false).Manage(false).TTL(0).Execute(new RevokeGrantResult());
                    Thread.Sleep(1000);
                    revokeManualEvent.WaitOne();

                    pubnub.Destroy();
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
                    Assert.IsTrue(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess -> Grant success but revoke failed.");
                }
                else
                {
                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                }
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess.");
            }
        }

        [Test]
        public static void ThenChannelGroupLevelWithReadManageShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannelGroupLevelWithReadManageShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group+auth\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channel-groups\":\"myChannelGroup\",\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel-group", channelGroup)
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "9TH9YpQydj1FaCJVoHqaL9YbPOEOhpVVv17FMrrz89U=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().ChannelGroups(new [] { channelGroup }).AuthKeys(new [] { authKey }).Read(true).Write(true).Manage(true).TTL(5).Execute(new GrantResult());
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelGroupLevelWithReadManageShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelGroupLevelWithReadManageShouldReturnSuccess.");
            }
        }

        [Test]
        public static void ThenChannelGroupLevelWithReadShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannelGroupLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group+auth\",\"subscribe_key\":\"demo-36\",\"ttl\":5,\"channel-groups\":\"myChannelGroup\",\"auths\":{\"hello_my_authkey\":{\"r\":1,\"w\":0,\"m\":0}}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel-group", channelGroup)
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "5")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "0")
                    .WithParameter("signature", "wrMx4t1Zh-2h_gIXQFJsKbTKsSHAIr8dK0Rn9KNrqp0=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().ChannelGroups(new [] { channelGroup }).AuthKeys(new [] { authKey }).Read(true).Write(false).Manage(false).TTL(5).Execute(new GrantResult());
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelGroupLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelGroupLevelWithReadShouldReturnSuccess.");
            }
        }

        [Test]
        public static void ThenUuidWithGetUpdateDeleteShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUuidWithGetUpdateDeleteShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);
            server.RunOnHttps(config.Secure);

            if (PubnubCommon.PAMServerSideGrant)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Uuids(new[] { targetUuid }).AuthKeys(new[] { authKey }).Get(true).Update(true).Delete(true).TTL(5)
                    .Execute(new PNAccessManagerGrantResultExt((r, s) => 
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
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUuidWithGetUpdateDeleteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUuidWithGetUpdateDeleteShouldReturnSuccess.");
            }
        }

        [Test]
        public static void ThenUuidWithReadPermisionShouldReturnError()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUuidWithReadPermisionShouldReturnError";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);
            server.RunOnHttps(config.Secure);

            if (PubnubCommon.PAMServerSideGrant)
            {
                try
                {
                    grantManualEvent = new ManualResetEvent(false);
                    pubnub.Grant().Uuids(new[] { targetUuid }).AuthKeys(new[] { authKey }).Read(true).TTL(5)
                        .Execute(new PNAccessManagerGrantResultExt((r, s) =>
                        {
                            grantManualEvent.Set();
                        }));
                    grantManualEvent.WaitOne();
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    receivedGrantMessage = true;
                    grantManualEvent.Set();
                }


                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUuidWithReadPermisionShouldReturnError failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUuidWithReadPermisionShouldReturnError.");
            }
        }

        [Test]
        public static void ThenUuidAndChannelShouldReturnError()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUuidAndChannelShouldReturnError";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);
            server.RunOnHttps(config.Secure);

            if (PubnubCommon.PAMServerSideGrant)
            {
                try
                {
                    grantManualEvent = new ManualResetEvent(false);
                    pubnub.Grant().Uuids(new[] { targetUuid }).Channels(new[] { channel }).AuthKeys(new[] { authKey }).Read(true).TTL(5)
                        .Execute(new PNAccessManagerGrantResultExt((r, s) =>
                        {
                            grantManualEvent.Set();
                        }));
                    grantManualEvent.WaitOne();
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    receivedGrantMessage = true;
                    grantManualEvent.Set();
                }


                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUuidAndChannelShouldReturnError failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUuidAndChannelShouldReturnError.");
            }
        }

        [Test]
        public static void ThenUuidAndChannelGroupShouldReturnError()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUuidAndChannelGroupShouldReturnError";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);
            server.RunOnHttps(config.Secure);

            if (PubnubCommon.PAMServerSideGrant)
            {
                try
                {
                    grantManualEvent = new ManualResetEvent(false);
                    pubnub.Grant().Uuids(new[] { targetUuid }).ChannelGroups(new[] { channelGroup }).AuthKeys(new[] { authKey }).Read(true).TTL(5)
                        .Execute(new PNAccessManagerGrantResultExt((r, s) =>
                        {
                            grantManualEvent.Set();
                        }));
                    grantManualEvent.WaitOne();
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    receivedGrantMessage = true;
                    grantManualEvent.Set();
                }


                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUuidAndChannelGroupShouldReturnError failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUuidAndChannelGroupShouldReturnError.");
            }
        }

        [Test]
        public static void ThenGrantTokenShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenGrantTokenShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false
            };

            pubnub = createPubNubInstance(config);
            server.RunOnHttps(config.Secure);

            try
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantToken()
                    .Resources(new PNTokenResources() 
                        { 
                            Channels=new Dictionary<string, PNTokenAuthValues>() {
                                            { "ch1", new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } } },
                            ChannelGroups = new Dictionary<string, PNTokenAuthValues>() {
                                            { "cg1", new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } } },
                            Uuids = new Dictionary<string, PNTokenAuthValues>() {
                                            { "uuid1", new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } } },
                    }
                    )
                    .TTL(10)
                    .Execute(new PNAccessManagerTokenResultExt((result, status) =>
                    {
                        if (result != null)
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        }
                        else
                        {
                            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                        }
                        receivedGrantMessage = true;
                        grantManualEvent.Set();
                    }));
                grantManualEvent.WaitOne();
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                receivedGrantMessage = true;
                grantManualEvent.Set();
            }

        }


        [Test]
#if NET40
        public static void ThenWithAsyncGrantTokenShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncGrantTokenShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenWithAsyncGrantTokenShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);
            server.RunOnHttps(config.Secure);

#if NET40
#else
            PNResult<PNAccessManagerTokenResult> result = await pubnub.GrantToken()
                    .Resources(new PNTokenResources()
                    {
                        Channels = new Dictionary<string, PNTokenAuthValues>() {
                                            { "ch1", new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } } },
                        ChannelGroups = new Dictionary<string, PNTokenAuthValues>() {
                                            { "cg1", new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } } },
                        Uuids = new Dictionary<string, PNTokenAuthValues>() {
                                            { "uuid1", new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } } },
                    }
                    )
                .TTL(10)
                .ExecuteAsync();
#endif

        }

        [Test]
        public static void ThenRevokeTokenShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenRevokeTokenShouldReturnSuccess";

            receivedRevokeMessage = false;

            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false
            };

            pubnub = createPubNubInstance(config);
            server.RunOnHttps(config.Secure);

            try
            {
                PNResult<PNAccessManagerTokenResult> grantResult = pubnub.GrantToken().TTL(5).Resources(new PNTokenResources() { Channels = new Dictionary<string, PNTokenAuthValues>() { { "ch1", new PNTokenAuthValues() { Read = true } } } }).ExecuteAsync().Result;
                if (grantResult.Result != null && !string.IsNullOrEmpty(grantResult.Result.Token))
                {
                    revokeManualEvent = new ManualResetEvent(false);
                    pubnub.RevokeToken()
                        .Token(grantResult.Result.Token)
                        .Execute(new PNAccessManagerRevokeTokenResultExt((result, status) =>
                        {
                            if (result != null)
                            {
                                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                            }
                            else
                            {
                                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                            }
                            receivedRevokeMessage = true;
                            revokeManualEvent.Set();
                        }));
                    revokeManualEvent.WaitOne();
                    Thread.Sleep(1000);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                receivedRevokeMessage = true;
                revokeManualEvent.Set();
            }

        }


        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private class GrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        switch (currentUnitTestCase)
                        {
                            case "ThenUserLevelWithReadWriteShouldReturnSuccess":
                            case "ThenRevokeAtUserLevelReturnSuccess":
                                {
                                    if (result.Channels != null && result.Channels.Count > 0)
                                    {
                                        var read = result.Channels[channel][authKey].ReadEnabled;
                                        var write = result.Channels[channel][authKey].WriteEnabled;
                                        if (read && write)
                                        {
                                            receivedGrantMessage = true;
                                        }
                                    }
                                    break;
                                }
                            case "ThenUserLevelWithReadShouldReturnSuccess":
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (read && !write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenUserLevelWithWriteShouldReturnSuccess":
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (!read && write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenMultipleChannelGrantShouldReturnSuccess":
                                {
                                    if (result.Channels.Count==multipleAuthGrantCount)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenMultipleAuthGrantShouldReturnSuccess":
                                {
                                    if (result.Channels.Count == multipleAuthGrantCount && result.Channels.ToList()[0].Value.Count == multipleAuthGrantCount)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenChannelGroupLevelWithReadManageShouldReturnSuccess":
                                {
                                    var read = result.ChannelGroups[channelGroup][authKey].ReadEnabled;
                                    var write = result.ChannelGroups[channelGroup][authKey].WriteEnabled;
                                    var manage = result.ChannelGroups[channelGroup][authKey].ManageEnabled;
                                    if (read && write && manage)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenChannelGroupLevelWithReadShouldReturnSuccess":
                                {
                                    var read = result.ChannelGroups[channelGroup][authKey].ReadEnabled;
                                    var write = result.ChannelGroups[channelGroup][authKey].WriteEnabled;
                                    var manage = result.ChannelGroups[channelGroup][authKey].ManageEnabled;
                                    if (read && !write && !manage)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                        }
                    }
                }
                finally
                {
                    grantManualEvent.Set();
                }
            }
        }


        private class RevokeGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        switch (currentUnitTestCase)
                        {
                            case "ThenRevokeAtUserLevelReturnSuccess":
                                {
                                    if (result.Channels != null && result.Channels.Count > 0)
                                    {
                                        var read = result.Channels[channel][authKey].ReadEnabled;
                                        var write = result.Channels[channel][authKey].WriteEnabled;
                                        if (!read && !write)
                                        {
                                            receivedRevokeMessage = true;
                                        }
                                    }
                                    break;
                                }
                            case "ThenUserLevelWithReadShouldReturnSuccess":
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (read && !write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenUserLevelWithWriteShouldReturnSuccess":
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (!read && write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenMultipleChannelGrantShouldReturnSuccess":
                                {
                                    if (result.Channels.Count == multipleAuthGrantCount)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenMultipleAuthGrantShouldReturnSuccess":
                                {
                                    if (result.Channels.Count == multipleAuthGrantCount && result.Channels.ToList()[0].Value.Count == multipleAuthGrantCount)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                        }
                    }
                }
                finally
                {
                    revokeManualEvent.Set();
                }
            }
        }

    }
}
