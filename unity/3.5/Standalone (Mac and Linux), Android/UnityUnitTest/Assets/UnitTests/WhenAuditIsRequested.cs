using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class WhenAuditIsRequested: UUnitTestCase
    {
        ManualResetEvent auditManualEvent = new ManualResetEvent(false);
        bool receivedAuditMessage = false;
        string currentUnitTestCase = "";

        [UUnitTest]
        public void ThenSubKeyLevelShouldReturnSuccess()
        {
			Debug.Log("Running ThenSubKeyLevelShouldReturnSuccess()");
            currentUnitTestCase = "ThenSubKeyLevelShouldReturnSuccess";

            receivedAuditMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAuditIsRequested";
            unitTest.TestCaseName = "ThenSubKeyLevelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                pubnub.AuditAccess<string>(AccessToSubKeyLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                auditManualEvent.WaitOne();

                UUnitAssert.True(receivedAuditMessage, "WhenAuditIsRequested -> ThenSubKeyLevelShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true, "PAM Not Enabled for WhenAuditIsRequested -> ThenSubKeyLevelShouldReturnSuccess");
            }

        }

        [UUnitTest]
        public void ThenChannelLevelShouldReturnSuccess()
        {
			Debug.Log("Running ThenChannelLevelShouldReturnSuccess()");
            currentUnitTestCase = "ThenChannelLevelShouldReturnSuccess";

            receivedAuditMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAuditIsRequested";
            unitTest.TestCaseName = "ThenChannelLevelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            if (PubnubCommon.PAMEnabled)
            {
                pubnub.AuditAccess<string>(channel, AccessToChannelLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                auditManualEvent.WaitOne();

                UUnitAssert.True(receivedAuditMessage, "WhenAuditIsRequested -> ThenChannelLevelShouldReturnSuccess failed.");
            }
            else
            {
                UUnitAssert.False(true,"PAM Not Enabled for WhenAuditIsRequested -> ThenChannelLevelShouldReturnSuccess");
            }
        }

        void AccessToSubKeyLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    //object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
					}
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                //bool read = (bool)payload["r"];
                                //bool write = (bool)payload["w"];
                                var channels = payload["channels"];
                                if (channels != null)
                                {
                                    //Debug.Log(string.Format("{0} - AccessToSubKeyLevelCallback - Audit Count = {1}",currentUnitTestCase, channels.Count));
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
                    //object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
					object[] deserializedMessage = new JsonFXDotNet().DeserializeToListOfObject(receivedMessage).ToArray();
					Dictionary<string,object> dictionary = null;
					if (deserializedMessage is object[])
					{
						dictionary = deserializedMessage[0] as Dictionary<string, object>;
					}
					
                    //JContainer dictionary = serializedMessage[0] as JContainer;
                    string currentChannel = deserializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = (int)dictionary["status"];
                        string statusMessage = dictionary["message"].ToString();
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary["payload"] as Dictionary<string, object>;
                            if (payload != null)
                            {
                                string level = payload["level"].ToString();
                                var channels = payload["channels"];
                                if (channels != null)
                                {
                                    //Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit Channel Count = {1}", currentUnitTestCase, channels.Count);
                                }
                                if (level == "channel")
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

        }
    }
}
