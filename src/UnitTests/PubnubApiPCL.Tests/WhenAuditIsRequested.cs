using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAuditIsRequested : TestHarness
    {
        private static ManualResetEvent auditManualEvent = new ManualResetEvent(false);
        private static bool receivedAuditMessage = false;
        private static string currentUnitTestCase = "";
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
        public void ThenSubKeyLevelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelShouldReturnSuccess";
            receivedAuditMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"subkey\",\"subscribe_key\":\"sub-c-25bce39a-6ead-11e6-b9ce-02ee2ddab7fe\",\"channels\":{},\"objects\":{},\"channel-groups\":{}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/auth/audit/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("signature", "8OajzzQLuMQvNuc9deWo2_LFVy4FbyGZLjV5TuhybW4=")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.Audit().Async(new AuditResult());
                Thread.Sleep(1000);

                auditManualEvent.WaitOne();

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
        public void ThenChannelLevelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelLevelShouldReturnSuccess";
            string channel = "hello_my_channel";

            receivedAuditMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel\",\"subscribe_key\":\"sub-c-25bce39a-6ead-11e6-b9ce-02ee2ddab7fe\",\"channels\":{}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/auth/audit/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("signature", "kVcEpy88jSLs9YTfAGqqAuA-7HYvH6HtJFla8yNQJCg=")
                    .WithParameter("channel", channel)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.Audit().Channel(channel).Async(new AuditResult());
                Thread.Sleep(1000);

                auditManualEvent.WaitOne();

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
        public void ThenChannelGroupLevelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelGroupLevelShouldReturnSuccess";
            string channelgroup = "hello_my_group";
            receivedAuditMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"channel-groups\":{}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/auth/audit/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("signature", "Yxvw2lCm7HL0tB9kj8qFA0YCC3KbxyTKkUcrwti9PKQ=")
                    .WithParameter("channel-group", channelgroup)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.Audit().ChannelGroup(channelgroup).Async(new AuditResult());
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
                Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                try
                {
                    if (result != null)
                    {
                        Console.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            if (currentUnitTestCase == "ThenSubKeyLevelShouldReturnSuccess")
                            {
                                if (!String.IsNullOrEmpty(result.Channel))
                                {
                                    var channels = result.Channel.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    Console.WriteLine("{0} - AccessToSubKeyLevelCallback - Audit Count = {1}", currentUnitTestCase, channels.Length);
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
                                    var channels = result.Channel.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit Channel Count = {1}", currentUnitTestCase, channels.Length);
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
                                    var channelgroups = result.ChannelGroup.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit ChannelGroup Count = {1}", currentUnitTestCase, channelgroups.Length);
                                }
                                if (result.Level.Contains("channel-group"))
                                {
                                    receivedAuditMessage = true;
                                }
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
