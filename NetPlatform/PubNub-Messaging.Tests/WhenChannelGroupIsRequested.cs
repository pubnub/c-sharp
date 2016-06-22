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
    public class WhenChannelGroupIsRequested
    {
        ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool receivedChannelGroupMessage = false;
        bool receivedGrantMessage = false;
        
        string currentUnitTestCase = "";
        string channelGroupName = "hello_my_group";

        Pubnub pubnub = null;

        [TestFixtureSetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init3";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.ChannelGroupGrantAccess<string>(channelGroupName, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenChannelGroupIsRequested Grant access failed.");
        }

        [Test]
        public void ThenAddChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenAddChannelShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenChannelGroupIsRequested";
            unitTest.TestCaseName = "ThenAddChannelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            channelGroupManualEvent = new ManualResetEvent(false);
            string channelName = "hello_my_channel";
            
            pubnub.AddChannelsToChannelGroup<string>(new string[] { channelName }, channelGroupName, ChannelGroupCRUDCallback, DummyErrorCallback);
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenAddChannelShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenRemoveChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenRemoveChannelShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenChannelGroupIsRequested";
            unitTest.TestCaseName = "ThenRemoveChannelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            channelGroupManualEvent = new ManualResetEvent(false);
            string channelName = "hello_my_channel";

            pubnub.RemoveChannelsFromChannelGroup<string>(new string[] { channelName }, channelGroupName, ChannelGroupCRUDCallback, DummyErrorCallback);
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenRemoveChannelShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenGetChannelListShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenGetChannelListShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenChannelGroupIsRequested";
            unitTest.TestCaseName = "ThenGetChannelListShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            channelGroupManualEvent = new ManualResetEvent(false);
            //string channelName = "hello_my_channel";

            pubnub.GetChannelsForChannelGroup<string>(channelGroupName, ChannelGroupCRUDCallback, DummyErrorCallback);
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");

        }

        void ChannelGroupCRUDCallback(string receivedMessage)
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
                            string serviceType = dictionary["service"].ToString();
                            bool errorStatus = Convert.ToBoolean(dictionary["error"]);
                            string currentChannelGroup = "";
                            switch (currentUnitTestCase)
                            {
                                case "ThenAddChannelShouldReturnSuccess":
                                case "ThenRemoveChannelShouldReturnSuccess":
                                    currentChannelGroup = serializedMessage[1].ToString().Substring(1); //assuming no namespace for channel group
                                    string statusMessage = dictionary["message"].ToString();
                                    if (statusCode == 200 && statusMessage.ToLower() == "ok" && serviceType == "channel-registry" && !errorStatus)
                                    {
                                        if (currentChannelGroup == channelGroupName)
                                        {
                                            receivedChannelGroupMessage = true;
                                        }
                                    }
                                    break;
                                case "ThenGetChannelListShouldReturnSuccess":
                                    Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                    if (payload != null && payload.Count > 0)
                                    {
                                        currentChannelGroup = payload["group"].ToString();
                                        object[] channels = pubnub.JsonPluggableLibrary.ConvertToObjectArray(payload["channels"]);
                                        if (currentChannelGroup == channelGroupName && channels != null && channels.Length >= 0)
                                        {
                                            receivedChannelGroupMessage = true;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    
                }
            }
            catch { }
            finally
            {
                channelGroupManualEvent.Set();
            }

        }

        void ThenChannelGroupInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null)
                        {
                            var status = dictionary["status"].ToString();
                            if (status == "200")
                            {
                                receivedGrantMessage = true;
                            }
                        }

                    }
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        private void DummyErrorCallback(PubnubClientError result)
        {
            channelGroupManualEvent.Set();
        }
    }
}
