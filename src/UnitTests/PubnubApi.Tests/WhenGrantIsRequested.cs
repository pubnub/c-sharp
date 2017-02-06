using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Text;
using System.Linq;

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
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void ThenUserLevelWithReadWriteShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUserLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

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
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(false).TTL(5).Async(new GrantResult());
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
        public void ThenUserLevelWithReadShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUserLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

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
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(false).Manage(false).TTL(5).Async(new GrantResult());
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
        public void ThenUserLevelWithWriteShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenUserLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

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
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(false).Write(true).Manage(false).TTL(5).Async(new GrantResult());
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
        public void ThenMultipleChannelGrantShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenMultipleChannelGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

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
                pubnub.Grant().AuthKeys(new string[] { authKey }).Channels(channelBuilder).Read(true).Write(true).Manage(false).TTL(5).Async(new GrantResult());
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
        public void ThenMultipleAuthGrantShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenMultipleAuthGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

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
                pubnub.Grant().Channels(channelBuilder).AuthKeys(authKeyBuilder).Read(true).Write(true).Manage(false).TTL(5).Async(new GrantResult());
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
        public void ThenRevokeAtUserLevelReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenRevokeAtUserLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

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
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(false).TTL(5).Async(new GrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne();

                if (receivedGrantMessage)
                {
                    revokeManualEvent = new ManualResetEvent(false);
                    Console.WriteLine("WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess -> Grant ok..Now trying Revoke");
                    pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(false).Write(false).Manage(false).TTL(0).Async(new RevokeGrantResult());
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
        public void ThenChannelGroupLevelWithReadManageShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannelGroupLevelWithReadManageShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

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
                pubnub.Grant().ChannelGroups(new string[] { channelGroup }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(5).Async(new GrantResult());
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
        public void ThenChannelGroupLevelWithReadShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannelGroupLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

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
                pubnub.Grant().ChannelGroups(new string[] { channelGroup }).AuthKeys(new string[] { authKey }).Read(true).Write(false).Manage(false).TTL(5).Async(new GrantResult());
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

        private class GrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
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
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
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
