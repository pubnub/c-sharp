using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;
using PubnubApi;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAuditIsRequested
    {
        ManualResetEvent auditManualEvent = new ManualResetEvent(false);
        bool receivedAuditMessage = false;
        string currentUnitTestCase = "";

        Pubnub pubnub = null;

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
                CiperKey = "",
                Secure = false
            };

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            unitTest.StubRequestResponse(string.Format("http{0}://{1}/v1/auth/audit/sub-key/{2}?signature=oAd8oWxgsIx7DEpHu8aa6I874sdjD5U_vI8wxGWhV0E=&pnsdk={3}&timestamp=1356998400&uuid={4}", config.Secure ? "s" : "", config.Origin, PubnubCommon.SubscribeKey, config.SdkVersion, config.Uuid),
                    "{\"message\":\"Success\",\"payload\":{\"level\":\"subkey\",\"subscribe_key\":\"pam\",\"channels\":{\"csharp-pam-cl-channel-38\":{\"r\":1,\"m\":0,\"w\":1,\"ttl\":453},\"csharp-pam-cl-channel-39\":{\"r\":1,\"m\":0,\"w\":1,\"ttl\":453}}},\"service\":\"Access Manager\",\"status\":200}"
                );
            pubnub = new Pubnub(config, unitTest);

            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.AuditAccess(AccessToSubKeyLevelCallback, DummyErrorCallback);
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
                CiperKey = "",
                Secure = false
            };

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            unitTest.StubRequestResponse(string.Format("http{0}://{1}/v1/auth/audit/sub-key/{2}?signature=H1FkVTgGZap7-jqXnJQ77PLLIx86RD0Pk3HzDgOFvoA=&channel={3}&pnsdk={4}&timestamp=1356998400&uuid={5}", config.Secure ? "s" : "", config.Origin, PubnubCommon.SubscribeKey, channel, config.SdkVersion, config.Uuid),
                    "{\"message\":\"Success\",\"payload\":{\"level\":\"channel\",\"subscribe_key\":\"pam\",\"channels\":{}},\"service\":\"Access Manager\",\"status\":200}"
                );
            pubnub = new Pubnub(config, unitTest);

            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.AuditAccess(channel, "", new string[] { }, AccessToChannelLevelCallback, DummyErrorCallback);
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
                CiperKey = "",
                Secure = false
            };

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            unitTest.StubRequestResponse(string.Format("http{0}://{1}/v1/auth/audit/sub-key/{2}?signature=bqQC8-ovOMVy1OrBKtvVipUOuDJUa35rj_HSm6VZ6pw=&channel-group={3}&pnsdk={4}&timestamp=1356998400&uuid={5}", config.Secure ? "s" : "", config.Origin, PubnubCommon.SubscribeKey, channelgroup, System.Net.WebUtility.HtmlEncode(config.SdkVersion), config.Uuid),
                    "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"channel-groups\":{}},\"service\":\"Access Manager\",\"status\":200}"
                );
            pubnub = new Pubnub(config, unitTest);

            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.AuditAccess("", channelgroup, new string[] { }, AccessToChannelLevelCallback, DummyErrorCallback);
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

        void AccessToSubKeyLevelCallback(AuditAck receivedMessage)
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
                            Dictionary<string, AuditAck.Data.ChannelData> channels = receivedMessage.Payload.channels;
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

        void AccessToChannelLevelCallback(AuditAck receivedMessage)
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
                                Dictionary<string, AuditAck.Data.ChannelData> channels = receivedMessage.Payload.channels;
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
                                Dictionary<string, AuditAck.Data.ChannelGroupData> channelgroups = receivedMessage.Payload.channelgroups;
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
