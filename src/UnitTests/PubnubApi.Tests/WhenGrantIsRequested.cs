using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
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
        private static string authKey = "hello_my_authkey";
        private static string[] channelBuilder;
        private static string[] authKeyBuilder;

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
        }

        [TestFixtureTearDown]
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
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

            if (PubnubCommon.PAMEnabled)
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
        public static void ThenUserLevelWithReadShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUserLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
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

            if (PubnubCommon.PAMEnabled)
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
        public static void ThenUserLevelWithWriteShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUserLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
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


            if (PubnubCommon.PAMEnabled)
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
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

            if (PubnubCommon.PAMEnabled)
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
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

            if (PubnubCommon.PAMEnabled)
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
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

            if (PubnubCommon.PAMEnabled)
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
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

            if (PubnubCommon.PAMEnabled)
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
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

            if (PubnubCommon.PAMEnabled)
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
        public static void ThenPAMv3ChannelShouldReturnTokenSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenPAMv3ChannelShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false,
                EnableTelemetry = false,
                IncludeInstanceIdentifier = false,
                IncludeRequestIdentifier = false
            };

            pubnub = createPubNubInstance(config);

            server.RunOnHttps(config.Secure);
            string expected = "";

            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();
            List<string> authList = new List<string>();

            List<string> userList = new List<string>();
            List<string> spaceList = new List<string>();

            Dictionary<string, int> chBitmaskPermDic = new Dictionary<string, int>();
            for (int chIndex = 0; chIndex < channelList.Count; chIndex++)
            {
                if (!chBitmaskPermDic.ContainsKey(channelList[chIndex]))
                {
                    chBitmaskPermDic.Add(channelList[chIndex], 3);
                }
            }

            Dictionary<string, int> cgBitmaskPermDic = new Dictionary<string, int>();

            Dictionary<string, int> userBitmaskPermDic = new Dictionary<string, int>();

            Dictionary<string, int> spaceBitmaskPermDic = new Dictionary<string, int>();

            Dictionary<string, object> resourcesDic = new Dictionary<string, object>();
            resourcesDic.Add("channels", chBitmaskPermDic);
            resourcesDic.Add("groups", cgBitmaskPermDic);
            resourcesDic.Add("users", userBitmaskPermDic);
            resourcesDic.Add("spaces", spaceBitmaskPermDic);


            Dictionary<string, int> dummyBitmaskPermDic = new Dictionary<string, int>();

            Dictionary<string, object> patternsDic = new Dictionary<string, object>();
            patternsDic.Add("channels", dummyBitmaskPermDic);
            patternsDic.Add("groups", dummyBitmaskPermDic);
            patternsDic.Add("users", dummyBitmaskPermDic);
            patternsDic.Add("spaces", dummyBitmaskPermDic);

            Dictionary<string, object> meta = new Dictionary<string, object>();
            meta.Add("user-id", "jay@example.com");
            meta.Add("contains-unicode", "The 💩 test.");

            Dictionary<string, object> permissionDic = new Dictionary<string, object>();
            permissionDic.Add("resources", resourcesDic);
            permissionDic.Add("patterns", patternsDic);
            permissionDic.Add("meta", meta);

            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            messageEnvelope.Add("ttl", 1440);
            messageEnvelope.Add("permissions", permissionDic);
            string postMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messageEnvelope);

            server.AddRequest(new Request()
                    .WithMethod("POST")
                    .WithPath(string.Format("/v3/pam/{0}/grant", PubnubCommon.SubscribeKey))
                    .WithContent(postMessage)
                    .WithParameter("PoundsSterling", "£13.37")
                    .WithParameter("timestamp", "123456789")
                    .WithParameter("signature", "v2.k80LsDMD-sImA8rCBj-ntRKhZ8mSjHY8Ivngt9W3Yc4")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMEnabled)
            {
                
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantToken()
                    .Channels(new Dictionary<string, PNResourcePermission>() {
                                { "inbox-jay", new PNResourcePermission() { Read = true, Write = true } } })
                    .TTL(1440)
                    .QueryParam(new System.Collections.Generic.Dictionary<string, object>() { { "PoundsSterling", "£13.37" } })
                    .Meta(new System.Collections.Generic.Dictionary<string, object>() { { "user-id", "jay@example.com" }, { "contains-unicode", "The 💩 test." } })
                    .Execute(new PNAccessManagerTokenResultExt((result, status)=> 
                    {
                        if (result != null)
                        {
                            System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                            string token = result.Token;
                            //string paddedToken = string.Format("{0}==", token);
                            //token = token.TrimEnd(new char[] { '=' });
                            try
                            {
                                token = token.Replace('_', '/').Replace('-','+').PadRight(4 * ((token.Length + 3) / 4), '=');
                                byte[] tokenByteArray = Convert.FromBase64String(token);
                                var jsonPermission = System.Text.Encoding.UTF8.GetString(tokenByteArray);
                                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(jsonPermission));
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(ex.ToString());
                            }
                            receivedGrantMessage = true;
                        }
                        grantManualEvent.Set();
                    }));

                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenPAMv3ChannelShouldReturnSuccess failed.");

            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenPAMv3ChannelShouldReturnSuccess.");
            }
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
                                    if (result.Channels.Count == multipleAuthGrantCount)
                                    {
                                        if (result.Channels.ToList()[0].Value.Count == multipleAuthGrantCount)
                                        {
                                            receivedGrantMessage = true;
                                        }
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
                catch
                {
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
                                    if (result.Channels.Count == multipleAuthGrantCount)
                                    {
                                        if (result.Channels.ToList()[0].Value.Count == multipleAuthGrantCount)
                                        {
                                            receivedGrantMessage = true;
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    revokeManualEvent.Set();
                }
            }
        }

    }
}
