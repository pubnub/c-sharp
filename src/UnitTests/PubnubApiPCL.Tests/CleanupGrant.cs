using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class CleanupGrant : TestHarness
    {
        private static ManualResetEvent auditManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent revokeManualEvent = new ManualResetEvent(false);
        private static bool receivedAuditMessage = false;
        private static string currentUnitTestCase;

        private static Pubnub pubnub = null;
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
            currentUnitTestCase = "AtUserLevel";

            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM not enabled; CleanupGrant -> AtUserLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedAuditMessage = false;
                auditManualEvent = new ManualResetEvent(false);

                PNConfiguration config = new PNConfiguration()
                {
                    PublishKey = PubnubCommon.PublishKey,
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    SecretKey = PubnubCommon.SecretKey,
                    Uuid = "mytestuuid",
                };

                pubnub = this.createPubNubInstance(config);

                string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/auth/audit/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("signature", "Yxvw2lCm7HL0tB9kj8qFA0YCC3KbxyTKkUcrwti9PKQ=")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Audit().Async(new AuditResult());
                auditManualEvent.WaitOne();
                pubnub.EndPendingRequests();
                pubnub = null;
                Assert.IsTrue(receivedAuditMessage, "CleanupGrant -> AtUserLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; CleanupGrant -> AtUserLevel.");
            }
        }

        [Test]
        public void AtChannelLevel()
        {
            currentUnitTestCase = "AtChannelLevel";

            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM not enabled; CleanupGrant -> AtChannelLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedAuditMessage = false;
                auditManualEvent = new ManualResetEvent(false);

                PNConfiguration config = new PNConfiguration()
                {
                    PublishKey = PubnubCommon.PublishKey,
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    SecretKey = PubnubCommon.SecretKey,
                    Uuid = "mytestuuid",
                };

                pubnub = this.createPubNubInstance(config);

                string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v1/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Audit().Async(new AuditResult());
                auditManualEvent.WaitOne();

                pubnub.EndPendingRequests();
                pubnub = null;
                Assert.IsTrue(receivedAuditMessage, "CleanupGrant -> AtChannelLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; CleanupGrant -> AtChannelLevel.");
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
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                    }
                }
                catch { }
                finally
                {
                    revokeManualEvent.Set();
                }
            }
        }

        private class AuditResult : PNCallback<PNAccessManagerAuditResult>
        {
            public override void OnResponse(PNAccessManagerAuditResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            if (!String.IsNullOrEmpty(result.Channel))
                            {
                                var channels = result.Channel.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                Console.WriteLine("CleanupGrant / AtUserLevel / UserCallbackForCleanUpAccess - Channel Count = {0}", channels.Length);
                                foreach (string channelName in channels)
                                {
                                    if (result.AuthKeys != null)
                                    {
                                        foreach (string authKey in result.AuthKeys.Keys)
                                        {
                                            Console.WriteLine("Auth Key = " + authKey);
                                            revokeManualEvent = new ManualResetEvent(false);
                                            pubnub.Grant().Channels(new string[] { channelName }).AuthKeys(new string[] { authKey }).Read(false).Write(false).Manage(false).Async(new GrantResult());
                                            revokeManualEvent.WaitOne();
                                        }
                                    }
                                }
                            }

                            if (result.Level == "subkey")
                            {
                                receivedAuditMessage = true;
                            }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    auditManualEvent.Set();
                }
            }
        }
    }
}
