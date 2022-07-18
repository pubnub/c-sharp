using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenChannelGroupIsRequested : TestHarness
    {
        private static ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedChannelGroupMessage = false;
        private static bool receivedGrantMessage = false;

        private static string currentUnitTestCase = "";
        private static string channelGroupName = "hello_my_group";
        private static string channelName = "hello_my_channel";
        private static string authKey = "myauth";

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
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel-group", channelGroupName)
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "mnWJN7WSbajMt_LWpuiXGhcs3NUcVbU3L_MZpb9_blU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().ChannelGroups(new [] { channelGroupName }).AuthKeys(new [] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Execute(new GrantResult());

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenChannelGroupIsRequested Grant access failed.");
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenAddChannelShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenAddChannelShouldReturnSuccess";
            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "yo21VoxIksrH3Iozeaz5Zw4BX18N3vU9PLa-zVxRXsU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.AddChannelsToChannelGroup().Channels(new [] { channelName }).ChannelGroup(channelGroupName).Execute(new ChannelGroupAddChannelResult());
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenAddChannelShouldReturnSuccess failed.");

        }

        [Test]
#if NET40
        public static void ThenWithAsyncAddChannelShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncAddChannelShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenAddChannelShouldReturnSuccess";
            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "yo21VoxIksrH3Iozeaz5Zw4BX18N3vU9PLa-zVxRXsU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

#if NET40
            PNResult<PNChannelGroupsAddChannelResult> cgAddResult = Task.Factory.StartNew(async () => await pubnub.AddChannelsToChannelGroup().Channels(new[] { channelName }).ChannelGroup(channelGroupName).ExecuteAsync()).Result.Result;
#else
            PNResult<PNChannelGroupsAddChannelResult> cgAddResult = await pubnub.AddChannelsToChannelGroup().Channels(new[] { channelName }).ChannelGroup(channelGroupName).ExecuteAsync();
#endif
            Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(cgAddResult.Status));

            if (cgAddResult.Result != null)
            {
                Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(cgAddResult.Result));
                if (cgAddResult.Status.StatusCode == 200 && !cgAddResult.Status.Error)
                {
                    receivedChannelGroupMessage = true;
                }
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenWithAsyncAddChannelShouldReturnSuccess failed.");

        }

        [Test]
        public static void ThenRemoveChannelShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenRemoveChannelShouldReturnSuccess";
            string channelName = "hello_my_channel";
            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("remove", channelName)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "bTwraHYh6dEMi44y-WgHslZdKSltsMySX5cg0uHt9tE=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.RemoveChannelsFromChannelGroup().Channels(new [] { channelName }).ChannelGroup(channelGroupName).Execute(new ChannelGroupRemoveChannel());
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.Destroy();

            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenRemoveChannelShouldReturnSuccess failed.");

        }

        [Test]
#if NET40
        public static void ThenWithAsyncRemoveChannelShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncRemoveChannelShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenRemoveChannelShouldReturnSuccess";
            string channelName = "hello_my_channel";
            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("remove", channelName)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "bTwraHYh6dEMi44y-WgHslZdKSltsMySX5cg0uHt9tE=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

#if NET40
            PNResult<PNChannelGroupsRemoveChannelResult> cgRemoveChannelResult = Task.Factory.StartNew(async () => await pubnub.RemoveChannelsFromChannelGroup().Channels(new[] { channelName }).ChannelGroup(channelGroupName).ExecuteAsync()).Result.Result;
#else
            PNResult<PNChannelGroupsRemoveChannelResult> cgRemoveChannelResult = await pubnub.RemoveChannelsFromChannelGroup().Channels(new[] { channelName }).ChannelGroup(channelGroupName).ExecuteAsync();
#endif
            Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(cgRemoveChannelResult.Status));

            if (cgRemoveChannelResult.Result != null)
            {
                Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(cgRemoveChannelResult.Result));
                if (cgRemoveChannelResult.Status.StatusCode == 200 && cgRemoveChannelResult.Result.Message.ToLower() == "ok" 
                    && cgRemoveChannelResult.Result.Service == "channel-registry" && !cgRemoveChannelResult.Status.Error 
                    && cgRemoveChannelResult.Result.ChannelGroup == channelGroupName)
                {
                    receivedChannelGroupMessage = true;
                }
            }

            pubnub.Destroy();

            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenWithAsyncRemoveChannelShouldReturnSuccess failed.");

        }

        [Test]
        public static void ThenGetChannelListShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenGetChannelListShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"payload\": {\"channels\": [\"" + channelName + "\"], \"group\": \"" + channelGroupName + "\"}, \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "03YaQgvhwhQ9wMg3RLSYolTDzOpuuGoRzE5a7sEMLds=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.ListChannelsForChannelGroup().ChannelGroup(channelGroupName).Execute(new ChannelGroupAllChannels());
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");

        }

        [Test]
#if NET40
        public static void ThenWithAsyncGetChannelListShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncGetChannelListShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenGetChannelListShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"payload\": {\"channels\": [\"" + channelName + "\"], \"group\": \"" + channelGroupName + "\"}, \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "03YaQgvhwhQ9wMg3RLSYolTDzOpuuGoRzE5a7sEMLds=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

#if NET40
            PNResult<PNChannelGroupsAllChannelsResult> cgAllChannelsResult = Task.Factory.StartNew(async () => await pubnub.ListChannelsForChannelGroup().ChannelGroup(channelGroupName).ExecuteAsync()).Result.Result;
#else
            PNResult<PNChannelGroupsAllChannelsResult> cgAllChannelsResult = await pubnub.ListChannelsForChannelGroup().ChannelGroup(channelGroupName).ExecuteAsync();
#endif
            Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(cgAllChannelsResult.Status));

            if (cgAllChannelsResult.Result != null)
            {
                Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(cgAllChannelsResult.Result));
                if (cgAllChannelsResult.Status.StatusCode == 200 && !cgAllChannelsResult.Status.Error 
                    && cgAllChannelsResult.Result.ChannelGroup == channelGroupName && cgAllChannelsResult.Result.Channels.Count > 0)
                {
                    receivedChannelGroupMessage = true;
                }
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenWithAsyncGetChannelListShouldReturnSuccess failed.");

        }

        [Test]
        public static void ThenGetAllChannelGroupShouldReturnSuccess()
        {
            server.ClearRequests();
            if (!PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("Ignored due to requied secret key to get all CGs");
            }
            currentUnitTestCase = "ThenGetAllChannelGroupShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"payload\": {\"namespace\": \"\", \"groups\": [\"" + channelGroupName + "\", \"hello_my_group1\"]}, \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group", PubnubCommon.SubscribeKey))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "qnMQZkath89WEZaFGFmYaODIJqscq97l4TlvkVKHx_0=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.ListChannelGroups().Execute(new ChannelGroupAll());
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");

        }

        [Test]
#if NET40
        public static void ThenWithAsyncGetAllChannelGroupShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncGetAllChannelGroupShouldReturnSuccess()
#endif
        {
            server.ClearRequests();
            if (!PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("Ignored due to requied secret key to get all CGs");
            }
            currentUnitTestCase = "ThenGetAllChannelGroupShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            string expected = "{\"status\": 200, \"payload\": {\"namespace\": \"\", \"groups\": [\"" + channelGroupName + "\", \"hello_my_group1\"]}, \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group", PubnubCommon.SubscribeKey))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "qnMQZkath89WEZaFGFmYaODIJqscq97l4TlvkVKHx_0=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

#if NET40
            PNResult<PNChannelGroupsListAllResult> cgListAllResult = Task.Factory.StartNew(async () => await pubnub.ListChannelGroups().ExecuteAsync()).Result.Result;
#else
            PNResult<PNChannelGroupsListAllResult> cgListAllResult = await pubnub.ListChannelGroups().ExecuteAsync();
#endif
            Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(cgListAllResult.Status));

            if (cgListAllResult.Result != null)
            {
                Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(cgListAllResult.Result));
                if (cgListAllResult.Status.StatusCode == 200 && !cgListAllResult.Status.Error)
                {
                    receivedChannelGroupMessage = true;
                }
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");

        }

        private class GrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    if (status.Error == false)
                    {
                        receivedGrantMessage = true;
                    }

                    if (result != null)
                    {
                        Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                    }
                }
                catch
                { }
                finally
                {
                    grantManualEvent.Set();
                    channelGroupManualEvent.Set();
                }
            }
        }

        public class ChannelGroupAddChannelResult : PNCallback<PNChannelGroupsAddChannelResult>
        {
            public override void OnResponse(PNChannelGroupsAddChannelResult result, PNStatus status)
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    channelGroupManualEvent.Set();
                }
            }
        }

        public class ChannelGroupRemoveChannel : PNCallback<PNChannelGroupsRemoveChannelResult>
        {
            public override void OnResponse(PNChannelGroupsRemoveChannelResult result, PNStatus status)
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && result.Message.ToLower() == "ok" && result.Service == "channel-registry" && status.Error == false && result.ChannelGroup == channelGroupName)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    channelGroupManualEvent.Set();
                }
            }
        }

        public class ChannelGroupAllChannels : PNCallback<PNChannelGroupsAllChannelsResult>
        {
            public override void OnResponse(PNChannelGroupsAllChannelsResult result, PNStatus status)
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && status.Error == false && result.ChannelGroup==channelGroupName && result.Channels.Count>0)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    channelGroupManualEvent.Set();
                }
            }
        }

        public class ChannelGroupAll : PNCallback<PNChannelGroupsListAllResult>
        {
            public override void OnResponse(PNChannelGroupsListAllResult result, PNStatus status)
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    channelGroupManualEvent.Set();
                }
            }
        }
    }
}
