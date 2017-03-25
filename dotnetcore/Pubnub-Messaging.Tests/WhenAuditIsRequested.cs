using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

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

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAuditIsRequested";
            unitTest.TestCaseName = "ThenSubKeyLevelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.AuditAccess<string>(AccessToSubKeyLevelCallback, DummyErrorCallback);
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
            currentUnitTestCase = "ThenChannelLevelShouldReturnSuccess";

            receivedAuditMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAuditIsRequested";
            unitTest.TestCaseName = "ThenChannelLevelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.AuditAccess<string>(channel, AccessToChannelLevelCallback, DummyErrorCallback);
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
            currentUnitTestCase = "ThenChannelGroupLevelShouldReturnSuccess";

            receivedAuditMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAuditIsRequested";
            unitTest.TestCaseName = "ThenChannelGroupLevelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channelgroup = "hello_my_group";

            if (PubnubCommon.PAMEnabled)
            {
                auditManualEvent = new ManualResetEvent(false);
                pubnub.ChannelGroupAuditAccess<string>(channelgroup, AccessToChannelLevelCallback, DummyErrorCallback);
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

        void AccessToSubKeyLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);

                        if (dictionary != null && dictionary.Count > 0)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    Dictionary<string, object> channels = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channels"]);
                                    if (channels != null && channels.Count >= 0)
                                    {
                                        Console.WriteLine("{0} - AccessToSubKeyLevelCallback - Audit Count = {1}", currentUnitTestCase, channels.Count);
                                    }
                                    string level = payload["level"].ToString();
                                    if (level == "subkey")
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

        void AccessToChannelLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        string currentChannel = serializedMessage[1].ToString();
                        
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    string level = payload["level"].ToString();
                                    if (currentUnitTestCase == "ThenChannelLevelShouldReturnSuccess")
                                    {
                                        Dictionary<string, object> channels = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channels"]);
                                        if (channels != null && channels.Count >= 0)
                                        {
                                            Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit Channel Count = {1}", currentUnitTestCase, channels.Count);
                                        }
                                        if (level == "channel")
                                        {
                                            receivedAuditMessage = true;
                                        }
                                    }
                                    else if (currentUnitTestCase == "ThenChannelGroupLevelShouldReturnSuccess")
                                    {
                                        Dictionary<string, object> channelgroups = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channel-groups"]);
                                        if (channelgroups != null && channelgroups.Count >= 0)
                                        {
                                            Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit ChannelGroup Count = {1}", currentUnitTestCase, channelgroups.Count);
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
