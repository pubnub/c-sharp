using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToAChannelGroup : TestHarness
    {
        private static ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static bool receivedGrantMessage = false;

        private static string channelGroupName = "hello_my_group";

        private static string channelGroupName1 = "hello_my_group1";
        private static string channelGroupName2 = "hello_my_group2";
        private static string channelName = "hello_my_channel";

        private static object publishedMessage = null;
        private static long publishTimetoken = 0;

        int manualResetEventWaitTimeout = 310 * 1000;
        private static string authKey = "myAuth";
        private static string currentTestCase = "";

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

            currentTestCase = "Init";
            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            grantManualEvent = new ManualResetEvent(false);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", "hello_my_channel")
                    .WithParameter("channel-group", "hello_my_group%2Chello_my_group1%2Chello_my_group2")
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "oiUrVMZSf7NEGk6M9JrvpnffmMEy7wWLgYGHwMztIlU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().AuthKeys(new string[] { authKey }).ChannelGroups(new string[] { channelGroupName, channelGroupName1, channelGroupName2 }).Channels(new string[] { channelName }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());
            Thread.Sleep(1000);
            grantManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannelGroup Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnReceivedMessage";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                AuthKey = authKey,
                Secure = false
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            channelGroupManualEvent = new ManualResetEvent(false);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName)
                    .WithParameter("auth", config.AuthKey)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.AddChannelsToChannelGroup().Channels(new string[] { channelName }).ChannelGroup(channelGroupName).Async(new ChannelGroupAddChannelResult());
            channelGroupManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedMessage)
            {
                receivedMessage = false;
                subscribeManualEvent = new ManualResetEvent(false);

                expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithParameter("auth", authKey)
                        .WithParameter("channel-group", "hello_my_group")
                        .WithParameter("heartbeat", "300")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("tt", "0")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                expected = "{}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Subscribe<string>().ChannelGroups(new string[] { channelGroupName }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

                publishManualEvent = new ManualResetEvent(false);
                subscribeManualEvent = new ManualResetEvent(false);

                publishedMessage = "Test for WhenSubscribedToAChannelGroup ThenItShouldReturnReceivedMessage";

                expected = "[1,\"Sent\",\"14722484585147754\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20WhenSubscribedToAChannelGroup%20ThenItShouldReturnReceivedMessage%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channelName))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);

                pubnub.Publish().Channel(channelName).Message(publishedMessage).Async(new UTPublishResult());

                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                Thread.Sleep(1000);
                pubnub.Unsubscribe<string>().ChannelGroups(new string[] { channelGroupName }).Execute();

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
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            server.ClearRequests();

            currentTestCase = "ThenSubscribeShouldReturnConnectStatus";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                AuthKey = authKey,
                Secure = false
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            channelGroupManualEvent = new ManualResetEvent(false);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName)
                    .WithParameter("auth", config.AuthKey)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.AddChannelsToChannelGroup().Channels(new string[] { channelName }).ChannelGroup(channelGroupName).Async(new ChannelGroupAddChannelResult());
            channelGroupManualEvent.WaitOne();

            if (receivedMessage)
            {
                receivedMessage = false;
                subscribeManualEvent = new ManualResetEvent(false);

                expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithParameter("auth", authKey)
                        .WithParameter("channel-group", "hello_my_group")
                        .WithParameter("heartbeat", "300")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("tt", "0")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                expected = "{}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Subscribe<string>().ChannelGroups(new string[] { channelGroupName }).Execute();

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
        public void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            server.ClearRequests();

            currentTestCase = "ThenMultiSubscribeShouldReturnConnectStatus";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                AuthKey = authKey,
                Secure = false
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = 310 * 1000;

            channelGroupName1 = "hello_my_group1";
            channelGroupName2 = "hello_my_group2";

            string channelName1 = "hello_my_channel1";
            string channelName2 = "hello_my_channel2";

            channelGroupManualEvent = new ManualResetEvent(false);

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName1))
                    .WithParameter("add", channelName1)
                    .WithParameter("auth", config.AuthKey)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.AddChannelsToChannelGroup().Channels(new string[] { channelName1 }).ChannelGroup(channelGroupName1).Async(new ChannelGroupAddChannelResult());
            channelGroupManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedMessage)
            {
                receivedMessage = false;

                channelGroupManualEvent = new ManualResetEvent(false);

                expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName2))
                        .WithParameter("add", channelName2)
                        .WithParameter("auth", config.AuthKey)
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.AddChannelsToChannelGroup().Channels(new string[] { channelName2 }).ChannelGroup(channelGroupName2).Async(new ChannelGroupAddChannelResult());
                channelGroupManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                subscribeManualEvent = new ManualResetEvent(false);

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
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                expected = "{}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, ","))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));


                pubnub.Subscribe<string>().ChannelGroups(new string[] { channelGroupName1, channelGroupName2 }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status
            }

            Thread.Sleep(1000);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenMultiSubscribeShouldReturnConnectStatusFailed");

        }


        private class UTGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    if (result != null && status.StatusCode == 200 && !status.Error)
                    {
                        Console.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));

                        if (result.ChannelGroups != null && result.ChannelGroups.Count > 0)
                        {
                            foreach (KeyValuePair<string, Dictionary<string, PNAccessManagerKeyData>> channelGroupKP in result.ChannelGroups)
                            {
                                receivedGrantMessage = false;

                                string channelGroup = channelGroupKP.Key;
                                var read = result.ChannelGroups[channelGroup][authKey].ReadEnabled;
                                var write = result.ChannelGroups[channelGroup][authKey].WriteEnabled;
                                var manage = result.ChannelGroups[channelGroup][authKey].ManageEnabled;

                                if (read && write && manage)
                                {
                                    receivedGrantMessage = true;
                                }
                                else
                                {
                                    receivedGrantMessage = false;
                                    break;
                                }
                            }
                        }
                        if (result.Channels != null && result.Channels.Count > 0)
                        {
                            foreach (KeyValuePair<string, Dictionary<string, PNAccessManagerKeyData>> channelKP in result.Channels)
                            {
                                receivedGrantMessage = false;

                                string channel = channelKP.Key;
                                var read = result.Channels[channel][authKey].ReadEnabled;
                                var write = result.Channels[channel][authKey].WriteEnabled;
                                var manage = result.Channels[channel][authKey].ManageEnabled;

                                if (read && write && manage)
                                {
                                    receivedGrantMessage = true;
                                }
                                else
                                {
                                    receivedGrantMessage = false;
                                    break;
                                }
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
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

        public class UTSubscribeCallback : SubscribeCallback
        {
            public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
            {
                if (message != null)
                {
                    Console.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(message.Message));
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnReceivedMessage":
                            if (publishedMessage.ToString() == message.Message.ToString())
                            {
                                receivedMessage = true;
                            }
                            subscribeManualEvent.Set();
                            break;
                        //case "ThenSubscriberShouldBeAbleToReceiveManyMessages":
                        //    numberOfReceivedMessages++;
                        //    if (numberOfReceivedMessages >= 10)
                        //    {
                        //        receivedMessage = true;
                        //        subscribeManualEvent.Set();
                        //    }
                        //    break;
                        default:
                            break;
                    }
                }
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                //Console.WriteLine("SubscribeCallback: PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                Console.WriteLine("SubscribeCallback: PNStatus: " + status.StatusCode.ToString());
                if (status.StatusCode != 200 || status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnReceivedMessage":
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    if (status.ErrorData != null)
                    {
                        Console.WriteLine(status.ErrorData.Information);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNConnectedCategory)
                {
                    switch (currentTestCase)
                    {
                        case "ThenSubscribeShouldReturnReceivedMessage":
                            subscribeManualEvent.Set();
                            break;
                        case "ThenSubscribeShouldReturnConnectStatus":
                            receivedMessage = true;
                            subscribeManualEvent.Set();
                            break;
                        case "ThenMultiSubscribeShouldReturnConnectStatus":
                            if (status.AffectedChannelGroups != null && status.AffectedChannelGroups.Contains(channelGroupName1) && status.AffectedChannelGroups.Contains(channelGroupName2))
                            {
                                receivedMessage = true;
                                subscribeManualEvent.Set();
                            }
                            break;
                        //case "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback":
                        //    receivedMessage = true;
                        //    subscribeManualEvent.Set();
                        //    break;
                        default:
                            break;
                    }
                }


            }
        }

        public class ChannelGroupAddChannelResult : PNCallback<PNChannelGroupsAddChannelResult>
        {
            public override void OnResponse(PNChannelGroupsAddChannelResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        //if (status.StatusCode == 200 && result.Message.ToLower() == "ok" && result.Service == "channel-registry"&& status.Error == false && result.ChannelGroup.Substring(1) == channelGroupName)
                        switch(currentTestCase)
                        {
                            case "ThenMultiSubscribeShouldReturnConnectStatus":
                                if (status.StatusCode == 200 && status.Error == false && (status.AffectedChannelGroups.Contains(channelGroupName1) || status.AffectedChannelGroups.Contains(channelGroupName2)))
                                {
                                    receivedMessage = true;
                                }
                                break;
                            default:
                                if (status.StatusCode == 200 && status.Error == false && status.AffectedChannelGroups.Contains(channelGroupName))
                                {
                                    receivedMessage = true;
                                }
                                break;
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
                        case "ThenSubscribeShouldReturnReceivedMessage":
                            receivedMessage = true;
                            break;
                        default:
                            break;
                    }
                }

                publishManualEvent.Set();
            }
        }

    }
}

