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
    public class WhenAuditIsRequested : TestHarness
    {
        private static ManualResetEvent auditManualEvent = new ManualResetEvent(false);
        private static bool receivedAuditMessage = false;
        private static string currentUnitTestCase = "";
        private static Pubnub pubnub;
        private static Server server;
        private static int manualResetEventWaitTimeout = 20 * 1000;

        [SetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenSubKeyLevelShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenSubKeyLevelShouldReturnSuccess";
            receivedAuditMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"subkey\",\"subscribe_key\":\"demo-36\",\"channels\":{},\"objects\":{},\"channel-groups\":{}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/audit/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithParameter("signature", "zNRxtKgMlbcnwlamcfesFaXmG9DK2wirgob3a37Xyo0=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideRun)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.Audit().Execute(new AuditResult());
                Thread.Sleep(1000);

                auditManualEvent.WaitOne(manualResetEventWaitTimeout);

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedAuditMessage, "WhenAuditIsRequested -> ThenSubKeyLevelShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenAuditIsRequested -> ThenSubKeyLevelShouldReturnSuccess");
            }

        }

        [Test]
        public static void ThenChannelLevelShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannelLevelShouldReturnSuccess";
            string channel = "hello_my_channel";

            receivedAuditMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel\",\"subscribe_key\":\"demo-36\",\"channels\":{}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/audit/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("channel", channel)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithParameter("signature", "EdYHODFD_SaOGUkN8_QT3GpcjYdAzA71xvJfFXq2sUU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));


            if (PubnubCommon.PAMServerSideRun)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.Audit().Channel(channel).Execute(new AuditResult());
                Thread.Sleep(1000);

                auditManualEvent.WaitOne(manualResetEventWaitTimeout);

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedAuditMessage, "WhenAuditIsRequested -> ThenChannelLevelShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenAuditIsRequested -> ThenChannelLevelShouldReturnSuccess");
            }
        }

        [Test]
        public static void ThenChannelGroupLevelShouldReturnSuccess()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenChannelGroupLevelShouldReturnSuccess";
            string channelgroup = "hello_my_group";
            receivedAuditMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"demo-36\",\"channel-groups\":{}},\"service\":\"Access Manager\",\"status\":200}";

            server.RunOnHttps(config.Secure);
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/audit/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("channel-group", channelgroup)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithParameter("signature", "0xTp9FTQGMLn9cKlxqhTagtzc1r2BfaOlOym0XJ9qiQ=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMServerSideRun)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.Audit().ChannelGroup(channelgroup).Execute(new AuditResult());
                Thread.Sleep(1000);

                auditManualEvent.WaitOne();

                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedAuditMessage, "WhenAuditIsRequested -> ThenChannelGroupLevelShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenAuditIsRequested -> ThenChannelGroupLevelShouldReturnSuccess");
            }
        }

        private class AuditResult : PNCallback<PNAccessManagerAuditResult>
        {
            public override void OnResponse(PNAccessManagerAuditResult result, PNStatus status)
            {
                Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                try
                {
                    if (result != null)
                    {
                        Debug.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            if (currentUnitTestCase == "ThenSubKeyLevelShouldReturnSuccess")
                            {
                                if (!String.IsNullOrEmpty(result.Channel))
                                {
                                    var channels = result.Channel.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    Debug.WriteLine(string.Format("{0} - AccessToSubKeyLevelCallback - Audit Count = {1}", currentUnitTestCase, channels.Length));
                                }

                                if (result.Level == "subkey")
                                {
                                    receivedAuditMessage = true;
                                }
                            }
                            else if (currentUnitTestCase == "ThenChannelLevelShouldReturnSuccess")
                            {
                                if (!String.IsNullOrEmpty(result.Channel))
                                {
                                    var channels = result.Channel.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    Debug.WriteLine(string.Format("{0} - AccessToChannelLevelCallback - Audit Channel Count = {1}", currentUnitTestCase, channels.Length));
                                }
                                if (result.Level.Contains("channel"))
                                {
                                    receivedAuditMessage = true;
                                }
                            }
                            else if (currentUnitTestCase == "ThenChannelGroupLevelShouldReturnSuccess")
                            {
                                if (!String.IsNullOrEmpty(result.ChannelGroup))
                                {
                                    var channelgroups = result.ChannelGroup.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    Debug.WriteLine(string.Format("{0} - AccessToChannelLevelCallback - Audit ChannelGroup Count = {1}", currentUnitTestCase, channelgroups.Length));
                                }
                                if (result.Level.Contains("channel-group"))
                                {
                                    receivedAuditMessage = true;
                                }
                            }
                        }
                    }
                }
                catch { /* empty */ }
                finally
                {
                    auditManualEvent.Set();
                }
            }
        }
    }
}
