﻿using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Diagnostics;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class GenerateSampleGrant : TestHarness
    {
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static bool receivedGrantMessage = false;
        private static int sampleCount = 100;
        private static string currentUnitTestCase;

        private static Pubnub pubnub;
        private static Server server;

        [SetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            if (PubnubCommon.EnableStubTest)
            {
                server.Start();   
            }
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
        public static void AtUserLevel()
        {
            server.ClearRequests();

            currentUnitTestCase = "AtUserLevel";

            if (!PubnubCommon.PAMServerSideGrant)
            {
                Assert.Ignore("PAM not enabled; GenerateSampleGrant -> AtUserLevel.");
                return;
            }

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false,
            };

            pubnub = createPubNubInstance(config);

            grantManualEvent = new ManualResetEvent(false);
            string channelName = "csharp-pam-ul-channel";
            string authKey = "csharp-pam-authkey-0-0,csharp-pam-authkey-1-0";

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                .WithMethod("GET")
                .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                .WithParameter("auth", "csharp-pam-authkey-0-0%2Ccsharp-pam-authkey-1-0")
                .WithParameter("channel", channelName)
                .WithParameter("m", "0")
                .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                .WithParameter("r", "1")
                .WithParameter("requestid", "myRequestId")
                .WithParameter("uuid", config.UserId)
                .WithParameter("w", "1")
                .WithResponse(expected)
                .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            pubnub.Grant().Channels(new [] { channelName }).AuthKeys(new [] { authKey }).Read(true).Write(true).Manage(false).Execute(new GrantResult());
            grantManualEvent.WaitOne(5000);

            pubnub.Destroy();
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "GenerateSampleGrant -> AtUserLevel failed.");
        }

        [Test]
        public static void AtChannelLevel()
        {
            server.ClearRequests();
            // TODO: Reviewers, I understand that it helps to debug the testing suite - question is if maybe there are better ways?
            currentUnitTestCase = "AtChannelLevel";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false,
            };

            pubnub = createPubNubInstance(config);

            grantManualEvent = new ManualResetEvent(false);
            string channelName = "csharp-pam-cl-channel";

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("channel", channelName)
                    .WithParameter("m", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("w", "1")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().Channels(new [] { channelName }).Read(true).Write(true).Manage(false)
                .Execute(new GrantResult());

            grantManualEvent.WaitOne(5000);
            pubnub.Destroy();
            pubnub = null;

            Assert.IsTrue(receivedGrantMessage, "GenerateSampleGrant -> AtChannelLevel failed.");
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
                        receivedGrantMessage = true;
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
    }
}