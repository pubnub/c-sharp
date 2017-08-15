using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenMessageDeletedFromChannel : TestHarness
    {
        private static ManualResetEvent deleteMessageManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static bool receivedGrantMessage = false;

        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
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

            string channel = "hello_my_channel";

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

            Assert.IsTrue(receivedGrantMessage, "WhenUnsubscribedToAChannelGroup Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void DeleteMessageShouldReturnSuccessMessage()
        {
            server.ClearRequests();

            currentTestCase = "DeleteMessageShouldReturnSuccessMessage";
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"status\": 200, \"error\": false, \"error_message\": \"\"}";

            server.AddRequest(new Request()
                    .WithMethod("DELETE")
                    .WithPath(string.Format("/v3/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    //.WithParameter("start", -1)
                    //.WithParameter("end", -1)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("signature", "AQjIVGOI59CyaHrRG18XRZmqRjgWdzbbf9icO0Yzxo4=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;
            deleteMessageManualEvent = new ManualResetEvent(false);
            pubnub.DeleteMessages().Channel(channel).Async(new UTDeleteMessagaeResult());
            deleteMessageManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "DeleteMessageShouldReturnSuccessMessage - DeleteMessages Result not expected");
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

        public class UTDeleteMessagaeResult : PNCallback<PNDeleteMessageResult>
        {
            public override void OnResponse(PNDeleteMessageResult result, PNStatus status)
            {
                Console.WriteLine("DeleteMessage Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("DeleteMessage PNStatus => Status = : " + status.StatusCode.ToString());
                if (result != null && status.StatusCode == 200 && !status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "DeleteMessageShouldReturnSuccessMessage":
                            receivedMessage = true;
                            deleteMessageManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }
            }
        };
    }
}
