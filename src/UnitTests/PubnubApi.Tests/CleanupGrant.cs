using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Diagnostics;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class CleanupGrant : TestHarness
    {
        private static Pubnub pubnub;
        private static int manualResetEventWaitTimeout = 20 * 1000;

        [Test]
        public static void AtUserLevel()
        {
            bool receivedAuditMessage = false;

            if (!PubnubCommon.PAMServerSideGrant)
            {
                Assert.Ignore("PAM not enabled; CleanupGrant -> AtUserLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedAuditMessage = false;
                PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
                {
                    PublishKey = PubnubCommon.PublishKey,
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    SecretKey = PubnubCommon.SecretKey,
                    Secure = false
                };

                pubnub = createPubNubInstance(config);
                ManualResetEvent auditManualEvent = new ManualResetEvent(false);
                pubnub.Audit().Execute(new PNAccessManagerAuditResultExt((r,s)=> {
                    try
                    {
                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));

                        if (r != null)
                        {
                            Debug.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                            if (s.StatusCode == 200 && s.Error == false)
                            {
                                if (!String.IsNullOrEmpty(r.Channel))
                                {
                                    var channels = r.Channel.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    Debug.WriteLine("CleanupGrant / AtUserLevel / UserCallbackForCleanUpAccess - Channel Count = {0}", channels.Length.ToString());
                                    foreach (string channelName in channels)
                                    {
                                        if (r.AuthKeys != null)
                                        {
                                            foreach (string authKey in r.AuthKeys.Keys)
                                            {
                                                Debug.WriteLine("Auth Key = " + authKey);
                                                ManualResetEvent revokeManualEvent = new ManualResetEvent(false);
                                                pubnub.Grant().Channels(new[] { channelName }).AuthKeys(new[] { authKey }).Read(false).Write(false).Manage(false)
                                                .Execute(new PNAccessManagerGrantResultExt((r1,s1)=> 
                                                {
                                                    try
                                                    {
                                                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s1));

                                                        if (r1 != null)
                                                        {
                                                            Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r1));
                                                        }
                                                    }
                                                    catch {  /* ignore */ }
                                                    finally
                                                    {
                                                        revokeManualEvent.Set();
                                                    }
                                                }));
                                                revokeManualEvent.WaitOne();
                                            }
                                        }
                                    }
                                }

                                if (r.Level == "subkey")
                                {
                                    receivedAuditMessage = true;
                                }
                            }
                        }
                    }
                    catch {  /* ignore */  }
                    finally
                    {
                        auditManualEvent.Set();
                    }
                }));
                auditManualEvent.WaitOne(manualResetEventWaitTimeout);
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
        public static void AtChannelLevel()
        {
            bool receivedAuditMessage = false;

            if (!PubnubCommon.PAMServerSideGrant)
            {
                Assert.Ignore("PAM not enabled; CleanupGrant -> AtChannelLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedAuditMessage = false;

                PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
                {
                    PublishKey = PubnubCommon.PublishKey,
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    SecretKey = PubnubCommon.SecretKey,
                    Secure = false,
                };

                pubnub = createPubNubInstance(config);

                ManualResetEvent auditManualEvent = new ManualResetEvent(false);
                pubnub.Audit().Execute(new PNAccessManagerAuditResultExt((r, s) => {
                    try
                    {
                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));

                        if (r != null)
                        {
                            Debug.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                            if (s.StatusCode == 200 && s.Error == false)
                            {
                                if (!String.IsNullOrEmpty(r.Channel))
                                {
                                    var channels = r.Channel.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    Debug.WriteLine("CleanupGrant / AtUserLevel / UserCallbackForCleanUpAccess - Channel Count = {0}", channels.Length.ToString());
                                    foreach (string channelName in channels)
                                    {
                                        if (r.AuthKeys != null)
                                        {
                                            foreach (string authKey in r.AuthKeys.Keys)
                                            {
                                                Debug.WriteLine("Auth Key = " + authKey);
                                                ManualResetEvent revokeManualEvent = new ManualResetEvent(false);
                                                pubnub.Grant().Channels(new[] { channelName }).AuthKeys(new[] { authKey }).Read(false).Write(false).Manage(false)
                                                .Execute(new PNAccessManagerGrantResultExt((r1, s1) =>
                                                {
                                                    try
                                                    {
                                                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s1));

                                                        if (r1 != null)
                                                        {
                                                            Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r1));
                                                        }
                                                    }
                                                    catch {  /* ignore */ }
                                                    finally
                                                    {
                                                        revokeManualEvent.Set();
                                                    }
                                                }));
                                                revokeManualEvent.WaitOne();
                                            }
                                        }
                                    }
                                }

                                if (r.Level == "subkey")
                                {
                                    receivedAuditMessage = true;
                                }
                            }
                        }
                    }
                    catch {  /* ignore */  }
                    finally
                    {
                        auditManualEvent.Set();
                    }
                }));
                auditManualEvent.WaitOne(manualResetEventWaitTimeout);

                pubnub.Destroy();
                pubnub = null;
                Assert.IsTrue(receivedAuditMessage, "CleanupGrant -> AtChannelLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; CleanupGrant -> AtChannelLevel.");
            }
        }
    }
}
