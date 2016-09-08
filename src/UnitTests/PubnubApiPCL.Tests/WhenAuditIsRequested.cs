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
        private ManualResetEvent auditManualEvent = new ManualResetEvent(false);
        private bool receivedAuditMessage = false;
        string currentUnitTestCase = "";
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

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

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
                pubnub.Audit().Async(new PNCallback<PNAccessManagerAuditResult>() { Result = AccessToSubKeyLevelCallback, Error = DummyErrorCallback });
                Thread.Sleep(1000);

                auditManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
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
            string channel = "hello_my_channel";

            currentUnitTestCase = "ThenChannelLevelShouldReturnSuccess";

            receivedAuditMessage = false;

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
                pubnub.Audit().Channel(channel).Async(new PNCallback<PNAccessManagerAuditResult>() { Result = AccessToChannelLevelCallback, Error = DummyErrorCallback });
                Thread.Sleep(1000);

                auditManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
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
            string channelgroup = "hello_my_group";

            currentUnitTestCase = "ThenChannelGroupLevelShouldReturnSuccess";

            receivedAuditMessage = false;

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
                pubnub.Audit().ChannelGroup(channelgroup).Async(new PNCallback<PNAccessManagerAuditResult>() { Result = AccessToChannelLevelCallback, Error = DummyErrorCallback });
                Thread.Sleep(1000);

                auditManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedAuditMessage, "WhenAuditIsRequested -> ThenChannelGroupLevelShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenAuditIsRequested -> ThenChannelGroupLevelShouldReturnSuccess");
            }
        }

        void AccessToSubKeyLevelCallback(PNAccessManagerAuditResult receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "success")
                    {
                        if (receivedMessage.Payload != null)
                        {
                            Dictionary<string, PNAccessManagerAuditResult.Data.ChannelData> channels = receivedMessage.Payload.Channels;
                            if (channels != null && channels.Count >= 0)
                            {
                                Console.WriteLine("{0} - AccessToSubKeyLevelCallback - Audit Count = {1}", currentUnitTestCase, channels.Count);
                            }
                            string level = receivedMessage.Payload.Level;
                            if (level == "subkey")
                            {
                                receivedAuditMessage = true;
                            }
                        }
                    }

                }
            }
            catch { }
            finally
            {
                auditManualEvent.Set();
            }
        }

        void AccessToChannelLevelCallback(PNAccessManagerAuditResult receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "success")
                    {
                        if (receivedMessage.Payload != null)
                        {
                            string level = receivedMessage.Payload.Level;
                            if (currentUnitTestCase == "ThenChannelLevelShouldReturnSuccess")
                            {
                                Dictionary<string, PNAccessManagerAuditResult.Data.ChannelData> channels = receivedMessage.Payload.Channels;
                                if (channels != null && channels.Count >= 0)
                                {
                                    Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit Channel Count = {1}", currentUnitTestCase, channels.Count);
                                }
                                if (level.Contains("channel"))
                                {
                                    receivedAuditMessage = true;
                                }
                            }
                            else if (currentUnitTestCase == "ThenChannelGroupLevelShouldReturnSuccess")
                            {
                                Dictionary<string, PNAccessManagerAuditResult.Data.ChannelGroupData> channelgroups = receivedMessage.Payload.Channelgroups;
                                if (channelgroups != null && channelgroups.Count >= 0)
                                {
                                    Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit ChannelGroup Count = {1}", currentUnitTestCase, channelgroups.Count);
                                }
                                if (level.Contains("channel-group"))
                                {
                                    receivedAuditMessage = true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                auditManualEvent.Set();
            }
        }

        private void DummyErrorCallback(PubnubClientError result)
        {
            Console.WriteLine(result.Description);
        }
    }
}
