﻿using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenUnsubscribedToAChannelGroup : TestHarness
    {
        private static string channelGroupName = "hello_my_group";
        private static string authKey = "myauth";
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

            bool receivedGrantMessage = false;

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

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel-group\":\"hello_my_group\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

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
        public static void ThenShouldReturnUnsubscribedMessage()
        {
            server.RunOnHttps(false);

            bool receivedMessage = false;

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

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { if (m != null) { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); } },
                (o, p) => { /* Catch the presence events */ },
                (o, s) => {
                    Debug.WriteLine("SubscribeCallback: PNStatus: " + s.StatusCode.ToString());
                    // Reason: No Status code information emitted on Disconnected Event
                    if (s.Category == PNStatusCategory.PNConnectedCategory || s.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        receivedMessage = true;
                        subscribeManualEvent.Set();
                    } else if (s.StatusCode != 200 || s.Error)
                    {
                        subscribeManualEvent.Set();
                        if (s.ErrorData != null)
                        {
                            Debug.WriteLine(s.ErrorData.Information);
                        }
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            channelGroupName = "hello_my_group";
            string channelName = "hello_my_channel";

            int manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName)
                    .WithParameter("auth", config.AuthKey)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent cgManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup().Channels(new [] { channelName }).ChannelGroup(channelGroupName)
                .Execute(new PNChannelGroupsAddChannelResultExt((r,s)=> {
                    try
                    {
                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                        if (r != null)
                        {
                            Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                            if (s.StatusCode == 200 && s.Error == false && s.AffectedChannelGroups.Contains(channelGroupName))
                            {
                                receivedMessage = true;
                            }
                        }
                    }
                    catch { /* ignore */ }
                    finally { cgManualEvent.Set(); }
                }));
            cgManualEvent.WaitOne(manualResetEventWaitTimeout); 

            if (receivedMessage)
            {
                receivedMessage = false;
                expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithParameter("auth", authKey)
                        .WithParameter("channel-group", channelGroupName)
                        .WithParameter("heartbeat", "300")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("tt", "0")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                expected = "{}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "," ))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Subscribe<string>().ChannelGroups(new [] { channelGroupName }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

                if (receivedMessage)
                {
                    receivedMessage = false;
                    subscribeManualEvent = new ManualResetEvent(false);

                    expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

                    server.AddRequest(new Request()
                            .WithMethod("GET")
                            .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channelGroupName))
                            .WithResponse(expected)
                            .WithStatusCode(System.Net.HttpStatusCode.OK));

                    pubnub.Unsubscribe<string>().ChannelGroups(new [] { channelGroupName }).Execute();
                    subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
                }

                pubnub.RemoveListener(listenerSubCallack);
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;

                Assert.IsTrue(receivedMessage, "WhenUnsubscribedToAChannelGroup --> ThenShouldReturnUnsubscribedMessage Failed");
            }
            else
            {
                Assert.IsTrue(receivedMessage, "WhenUnsubscribedToAChannelGroup --> ThenShouldReturnUnsubscribedMessage Failed");
            }
        }

    }
}
