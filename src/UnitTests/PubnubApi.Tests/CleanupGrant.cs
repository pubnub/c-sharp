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
        }

        [TestFixtureTearDown]
        public void Exit()
        {
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

                pubnub.Audit().Async(new AuditResult());
                auditManualEvent.WaitOne();
                pubnub.Destroy();
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

                pubnub.Audit().Async(new AuditResult());
                auditManualEvent.WaitOne();

                pubnub.Destroy();
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
