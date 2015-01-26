using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.Threading;

namespace PubnubWindowsStore.Test
{
    [TestFixture]
    public class WhenAuditIsRequested
    {
        ManualResetEvent auditManualEvent = new ManualResetEvent(false);
        bool receivedAuditMessage = false;
        string currentUnitTestCase = "";

        [Test]
        public void ThenSubKeyLevelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelShouldReturnSuccess";

            receivedAuditMessage = false;
            auditManualEvent = new ManualResetEvent(false);

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAuditIsRequested";
            unitTest.TestCaseName = "ThenSubKeyLevelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.AuditAccess<string>(AccessToSubKeyLevelCallback, DummyErrorCallback);
                Task.Delay(1000);

                auditManualEvent.WaitOne();

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

            receivedAuditMessage = false;
            auditManualEvent = new ManualResetEvent(false);

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAuditIsRequested";
            unitTest.TestCaseName = "ThenChannelLevelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            if (PubnubCommon.PAMEnabled)
            {
                pubnub.AuditAccess<string>(channel, AccessToChannelLevelCallback, DummyErrorCallback);
                Task.Delay(1000);

                auditManualEvent.WaitOne();

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

            receivedAuditMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAuditIsRequested";
            unitTest.TestCaseName = "ThenChannelGroupLevelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channelgroup = "hello_my_group";

            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.ChannelGroupAuditAccess<string>(channelgroup, AccessToChannelLevelCallback, DummyErrorCallback);
                Task.Delay(1000);

                auditManualEvent.WaitOne();

                Assert.IsTrue(receivedAuditMessage, "WhenAuditIsRequested -> ThenChannelGroupLevelShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenAuditIsRequested -> ThenChannelGroupLevelShouldReturnSuccess");
            }
        }


        void AccessToSubKeyLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                bool read = payload.Value<bool>("r");
                                bool write = payload.Value<bool>("w");
                                var channels = payload.Value<JContainer>("channels");
                                if (channels != null)
                                {
                                    //Console.WriteLine("{0} - AccessToSubKeyLevelCallback - Audit Count = {1}", currentUnitTestCase, channels.Count);
                                }
                                string level = payload.Value<string>("level");
                                if (level == "subkey")
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

        void AccessToChannelLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                string level = payload.Value<string>("level");
                                if (currentUnitTestCase == "ThenChannelLevelShouldReturnSuccess")
                                {
                                    var channels = payload.Value<JContainer>("channels");
                                    if (channels != null)
                                    {
                                        //Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit Channel Count = {1}", currentUnitTestCase, channels.Count);
                                    }
                                    if (level == "channel")
                                    {
                                        receivedAuditMessage = true;
                                    }
                                }
                                else if (currentUnitTestCase == "ThenChannelGroupLevelShouldReturnSuccess")
                                {
                                    var channelgroups = payload.Value<JContainer>("channel-groups");
                                    if (channelgroups != null)
                                    {
                                        //Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit ChannelGroup Count = {1}", currentUnitTestCase, channelgroups.Count);
                                    }
                                    if (level == "channel-group")
                                    {
                                        receivedAuditMessage = true;
                                    }
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
            
        }
    }
}
