using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenUnsubscribedToAChannel : TestHarness
    {
        private static ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage;
        private static bool receivedGrantMessage;

        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string authKey = "myauth";
        private static string currentTestCase = "";

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

            if (!PubnubCommon.PAMServerSideGrant) { return; }

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"foo.*\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

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

            pubnub.Grant().Channels(new [] { channel }).AuthKeys(new [] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Execute(new UTGrantResult());

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenUnsubscribedToAChannel Grant access failed.");
        }

        [TestFixtureTearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenShouldReturnUnsubscribedMessage()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenShouldReturnUnsubscribedMessage";

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
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

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string currentChannelName = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, currentChannelName))
                    .WithParameter("auth", config.AuthKey)
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
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, currentChannelName))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { currentChannelName }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            if (receivedMessage)
            {
                receivedMessage = false;

                Thread.Sleep(1000);
                subscribeManualEvent = new ManualResetEvent(false);

                expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, currentChannelName))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Unsubscribe<string>().Channels(new [] { currentChannelName }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenUnsubscribedToAChannel --> ThenShouldReturnUnsubscribedMessage Failed");
        }

        private class UTGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (result.Channels != null && result.Channels.Count > 0)
                        {
                            foreach (KeyValuePair<string, Dictionary<string, PNAccessManagerKeyData>> channelKP in result.Channels)
                            {
                                string channel = channelKP.Key;
                                var read = result.Channels[channel][authKey].ReadEnabled;
                                var write = result.Channels[channel][authKey].WriteEnabled;
                                if (read && write)
                                {
                                    receivedGrantMessage = true;
                                }
                                else
                                {
                                    receivedGrantMessage = false;
                                }
                            }
                        }
                    }
                }
                catch { /* empty */  }
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
                    Debug.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(message.Message));
                }
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
            }

            public override void Signal<T>(Pubnub pubnub, PNSignalResult<T> signal)
            {
                throw new NotImplementedException();
            }

            public override void ObjectEvent(Pubnub pubnub, PNObjectApiEventResult objectEvent)
            {
                throw new NotImplementedException();
            }

            public override void MessageAction(Pubnub pubnub, PNMessageActionEventResult messageAction)
            {
                throw new NotImplementedException();
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                Debug.WriteLine("SubscribeCallback: PNStatus: " + status.StatusCode.ToString());
                if (status.StatusCode != 200 || status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "ThenShouldReturnUnsubscribedMessage":
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }

                    if (status.ErrorData != null)
                    {
                        Debug.WriteLine(status.ErrorData.Information);
                    }
                }
                else if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNConnectedCategory)
                {
                    switch (currentTestCase)
                    {
                        case "ThenShouldReturnUnsubscribedMessage":
                            receivedMessage = true;
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }
                else if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNDisconnectedCategory)
                {
                    switch (currentTestCase)
                    {
                        case "ThenShouldReturnUnsubscribedMessage":
                            receivedMessage = true;
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }


            }
        }

    }
}
