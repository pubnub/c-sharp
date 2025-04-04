﻿using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToAChannelGroup : TestHarness
    {
        private static string channelGroupName = "hello_my_group";
        private static string channelGroupName1 = "hello_my_group1";
        private static string channelGroupName2 = "hello_my_group2";
        private static string channelName = "hello_my_channel";

        private static object publishedMessage;

        static int manualResetEventWaitTimeout = 310 * 1000;
        private static string authToken;

        private static Pubnub pubnub;
        private static Server server;

        public class TestLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine(logText);
            }
        }

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
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            
            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    //.WithParameter("auth", authKey)
                    .WithParameter("channel", "hello_my_channel")
                    .WithParameter("channel-group", "hello_my_group%2Chello_my_group1%2Chello_my_group2")
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "oiUrVMZSf7NEGk6M9JrvpnffmMEy7wWLgYGHwMztIlU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            var fullAccess = new PNTokenAuthValues()
            {
                Read = true,
                Write = true,
                Create = true,
                Get = true,
                Delete = true,
                Join = true,
                Update = true,
                Manage = true
            };
            var grantResult = await pubnub.GrantToken().TTL(20).AuthorizedUuid(config.UserId).Resources(
                new PNTokenResources()
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>()
                    {
                        {
                            channelName, fullAccess
                        }
                    },
                    ChannelGroups = new Dictionary<string, PNTokenAuthValues>()
                    {
                        {
                            channelGroupName, fullAccess
                        },
                        {
                            channelGroupName1, fullAccess
                        },
                        {
                            channelGroupName2, fullAccess
                        }
                    }
                }).ExecuteAsync();

            await Task.Delay(4000);

            authToken = grantResult.Result?.Token;
            
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(grantResult.Status.Error == false && grantResult.Result != null, 
                "WhenSubscribedToAChannelGroup Grant access failed.");
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

        // TODO: This test seems to be not stable...
        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog(),
                NonSubscribeRequestTimeout = 120
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => {
                    if (m != null) {
                        Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                        if (string.Compare(publishedMessage.ToString(), m.Message.ToString(), true) == 0)
                        {
                            receivedMessage = true;
                        }
                    }
                    subscribeManualEvent.Set();
                },
                (o, p) => { /* Catch the presence events */ },
                (o, s) => {
                    Debug.WriteLine("SubscribeCallback: PNStatus: " + s.StatusCode.ToString());
                    if (s.StatusCode != 200 || s.Error)
                    {
                        subscribeManualEvent.Set();
                        if (s.ErrorData != null)
                        {
                            Debug.WriteLine(s.ErrorData.Information);
                        }
                    }
                    else if (s.StatusCode == 200 && (s.Category == PNStatusCategory.PNConnectedCategory || s.Category == PNStatusCategory.PNDisconnectedCategory))
                    {
                        subscribeManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

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

            ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup().Channels(new [] { channelName }).ChannelGroup(channelGroupName).QueryParam(new Dictionary<string, object> { { "ut", "ThenSubscribeShouldReturnReceivedMessage" } })
                .Execute(new PNChannelGroupsAddChannelResultExt((r, s) => {
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
                    finally { channelGroupManualEvent.Set(); }
                }));
            channelGroupManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedMessage)
            {
                receivedMessage = false;

                expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        //.WithParameter("auth", authKey)
                        .WithParameter("channel-group", "hello_my_group")
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
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Subscribe<string>().ChannelGroups(new [] { channelGroupName }).QueryParam(new Dictionary<string, object> { { "ut", "ThenSubscribeShouldReturnReceivedMessage" } }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

                subscribeManualEvent = new ManualResetEvent(false);

                publishedMessage = "Test for WhenSubscribedToAChannelGroup ThenItShouldReturnReceivedMessage";

                expected = "[1,\"Sent\",\"14722484585147754\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20WhenSubscribedToAChannelGroup%20ThenItShouldReturnReceivedMessage%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channelName))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);

                ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(channelName).Message(publishedMessage).QueryParam(new Dictionary<string, object> { { "ut", "ThenSubscribeShouldReturnReceivedMessage" } })
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        Debug.WriteLine("Publish PNStatus => Status = : " + s.StatusCode.ToString());
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            Debug.WriteLine("Publish Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                            receivedMessage = true;
                        }

                        publishManualEvent.Set();
                    }));
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                Thread.Sleep(1000);
                pubnub.Unsubscribe<string>().ChannelGroups(new [] { channelGroupName }).QueryParam(new Dictionary<string, object> { { "ut", "ThenSubscribeShouldReturnReceivedMessage" } }).Execute();
                Thread.Sleep(1000);
                pubnub.RemoveListener(listenerSubCallack);
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;

                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenItShouldReturnReceivedMessage Failed");
            }
            else
            {
                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenItShouldReturnReceivedMessage Failed");
            }

        }

        [Test]
        public static void ThenSubscribeShouldReturnConnectStatus()
        {
            server.ClearRequests();

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

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { if (m != null) { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); } },
                (o, p) => { /* Catch the presence events */ },
                (o, s) => {
                    Debug.WriteLine("SubscribeCallback: PNStatus: " + s.StatusCode.ToString());
                    if (s.StatusCode != 200 || s.Error)
                    {
                        subscribeManualEvent.Set();
                        if (s.ErrorData != null)
                        {
                            Debug.WriteLine(s.ErrorData.Information);
                        }
                    }
                    else if (s.StatusCode == 200 && (s.Category == PNStatusCategory.PNConnectedCategory || s.Category == PNStatusCategory.PNDisconnectedCategory))
                    {
                        receivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

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

            ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup().Channels(new [] { channelName }).ChannelGroup(channelGroupName)
                .Execute(new PNChannelGroupsAddChannelResultExt((r, s) => {
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
                    finally { channelGroupManualEvent.Set(); }
                }));
            channelGroupManualEvent.WaitOne();

            if (receivedMessage)
            {
                receivedMessage = false;

                expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        //.WithParameter("auth", authKey)
                        .WithParameter("channel-group", "hello_my_group")
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
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Subscribe<string>().ChannelGroups(new [] { channelGroupName }).Execute();

                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

                Thread.Sleep(1000);

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;

                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenSubscribeShouldReturnConnectStatus Failed");
            }
            else
            {
                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenSubscribeShouldReturnConnectStatus Failed");
            }
        }

        [Test]
        public static void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            server.ClearRequests();

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

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { if (m != null) { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); } },
                (o, p) => { /* Catch the presence events */ },
                (o, s) => {
                    Debug.WriteLine("SubscribeCallback: PNStatus: " + s.StatusCode.ToString());
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null)
                        {
                            Debug.WriteLine(s.ErrorData.Information);
                        }
                    }
                    else if (s.StatusCode == 200 
                        && (s.AffectedChannelGroups != null && s.AffectedChannelGroups.Contains(channelGroupName1) && s.AffectedChannelGroups.Contains(channelGroupName2))
                        && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        receivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = 310 * 1000;

            channelGroupName1 = "hello_my_group1";
            channelGroupName2 = "hello_my_group2";

            string channelName1 = "hello_my_channel1";
            string channelName2 = "hello_my_channel2";

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName1))
                    .WithParameter("add", channelName1)
                    .WithParameter("auth", config.AuthKey)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup().Channels(new [] { channelName1 }).ChannelGroup(channelGroupName1)
                .Execute(new PNChannelGroupsAddChannelResultExt((r, s) => {
                    try
                    {
                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                        if (r != null)
                        {
                            Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                            if (s.StatusCode == 200 && s.Error == false && (s.AffectedChannelGroups.Contains(channelGroupName1) || s.AffectedChannelGroups.Contains(channelGroupName2)))
                            {
                                receivedMessage = true;
                            }
                        }
                    }
                    catch { /* ignore */ }
                    finally { channelGroupManualEvent.Set(); }
                }));
            channelGroupManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedMessage)
            {
                receivedMessage = false;

                expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName2))
                        .WithParameter("add", channelName2)
                        .WithParameter("auth", config.AuthKey)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                channelGroupManualEvent = new ManualResetEvent(false);
                pubnub.AddChannelsToChannelGroup().Channels(new [] { channelName2 }).ChannelGroup(channelGroupName2)
                .Execute(new PNChannelGroupsAddChannelResultExt((r, s) => {
                    try
                    {
                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                        if (r != null)
                        {
                            Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                            if (s.StatusCode == 200 && s.Error == false && (s.AffectedChannelGroups.Contains(channelGroupName1) || s.AffectedChannelGroups.Contains(channelGroupName2)))
                            {
                                receivedMessage = true;
                            }
                        }
                    }
                    catch { /* ignore */ }
                    finally { channelGroupManualEvent.Set(); }
                }));
                channelGroupManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedMessage)
            {
                receivedMessage = false;

                expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithParameter("auth", config.AuthKey)
                        .WithParameter("channel-group", "hello_my_group1,hello_my_group2")
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
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));


                pubnub.Subscribe<string>().ChannelGroups(new [] { channelGroupName1, channelGroupName2 }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status
            }

            Thread.Sleep(1000);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenMultiSubscribeShouldReturnConnectStatusFailed");

        }

    }
}

