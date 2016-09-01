using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class GenerateSampleGrant : TestHarness
    {
        private ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private bool receivedGrantMessage = false;
        private int sampleCount = 100;

        private Pubnub pubnub = null;
        private Server server;
        private UnitTestLog unitLog;

        [TestFixtureSetUp]
        public void Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = new Server(new Uri("https://" + PubnubCommon.StubOrign));
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void AtUserLevel()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM not enabled; GenerateSampleGrant -> AtUserLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedGrantMessage = false;

                PNConfiguration config = new PNConfiguration()
                {
                    PublishKey = PubnubCommon.PublishKey,
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    SecretKey = PubnubCommon.SecretKey,
                    Uuid = "mytestuuid",
                };

                if (PubnubCommon.EnableStubTest)
                {
                    pubnub = this.createPubNubInstance(config);
                }
                else
                {
                    pubnub = new Pubnub(config);
                }

                string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v1/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                for (int index = 0; index < sampleCount; index++)
                {
                    grantManualEvent = new ManualResetEvent(false);
                    string channelName = string.Format("csharp-pam-ul-channel-{0}", index);
                    string authKey = string.Format("csharp-pam-authkey-0-{0},csharp-pam-authkey-1-{1}", index, index);
                    pubnub.grant().channels(new string[] { channelName }).authKeys(new string[] { authKey }).read(true).write(true).manage(false).async(new PNCallback<GrantAck>() { result = UserCallbackForSampleGrantAtUserLevel, error = ErrorCallbackForSampleGrantAtUserLevel });
                    grantManualEvent.WaitOne();
                }

                pubnub.EndPendingRequests();
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "GenerateSampleGrant -> AtUserLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; GenerateSampleGrant -> AtUserLevel.");
            }
        }

        [Test]
        public void AtChannelLevel()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM not enabled; GenerateSampleGrant -> AtChannelLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedGrantMessage = false;

                PNConfiguration config = new PNConfiguration()
                {
                    PublishKey = PubnubCommon.PublishKey,
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    SecretKey = PubnubCommon.SecretKey,
                    Uuid = "mytestuuid",
                };

                if (PubnubCommon.EnableStubTest)
                {
                    pubnub = this.createPubNubInstance(config);
                }
                else
                {
                    pubnub = new Pubnub(config);
                }

                string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v1/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                for (int index = 0; index < sampleCount; index++)
                {
                    grantManualEvent = new ManualResetEvent(false);
                    string channelName = string.Format("csharp-pam-cl-channel-{0}", index);
                    pubnub.grant().channels(new string[] { channelName }).read(true).write(true).manage(false).async(new PNCallback<GrantAck>() { result = UserCallbackForSampleGrantAtChannelLevel, error = ErrorCallbackForSampleGrantAtChannelLevel });
                    grantManualEvent.WaitOne();
                }

                pubnub.EndPendingRequests();
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "GenerateSampleGrant -> AtChannelLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; GenerateSampleGrant -> AtChannelLevel.");
            }
        }

        void UserCallbackForSampleGrantAtUserLevel(GrantAck receivedMessage)
        {
            receivedGrantMessage = true;
            Console.WriteLine(receivedMessage);
            grantManualEvent.Set();
        }

        void ErrorCallbackForSampleGrantAtUserLevel(PubnubClientError receivedMessage)
        {
            if (receivedMessage != null)
            {
                Console.WriteLine(receivedMessage);
            }
            grantManualEvent.Set();
        }

        void UserCallbackForSampleGrantAtChannelLevel(GrantAck receivedMessage)
        {
            receivedGrantMessage = true;
            Console.WriteLine(receivedMessage);
            grantManualEvent.Set();
        }

        void ErrorCallbackForSampleGrantAtChannelLevel(PubnubClientError receivedMessage)
        {
            if (receivedMessage != null)
            {
                Console.WriteLine(receivedMessage);
            }
            grantManualEvent.Set();
        }

    }
}
