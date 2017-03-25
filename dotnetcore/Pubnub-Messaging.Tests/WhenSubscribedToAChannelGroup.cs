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
    public class WhenSubscribedToAChannelGroup
    {
        ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        ManualResetEvent mePublish = new ManualResetEvent(false);

        bool receivedMessage = false;
        bool receivedGrantMessage = false;
        bool receivedChannelGroupMessage = false;

        bool receivedMessage1 = false;
        bool receivedMessage2 = false;
        bool receivedChannelGroupMessage1 = false;
        bool receivedChannelGroupMessage2 = false;

        string currentUnitTestCase = "";
        string channelGroupName = "hello_my_group";

        string channelGroupName1 = "hello_my_group1";
        string channelGroupName2 = "hello_my_group2";
        int expectedCallbackResponses = 2;
        int currentCallbackResponses = 0;

        int manualResetEventsWaitTimeout = 310 * 1000;

        Pubnub pubnub = null;

        [TestFixtureSetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            currentUnitTestCase = "Init";
            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init3";
            pubnub.PubnubUnitTest = unitTest;

            grantManualEvent = new ManualResetEvent(false);
            pubnub.ChannelGroupGrantAccess<string>(channelGroupName, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummySubscribeErrorCallback);
            Thread.Sleep(1000);
            grantManualEvent.WaitOne(310*1000);

            grantManualEvent = new ManualResetEvent(false);
            pubnub.ChannelGroupGrantAccess<string>(channelGroupName1, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummySubscribeErrorCallback);
            Thread.Sleep(1000);
            grantManualEvent.WaitOne(310 * 1000);

            grantManualEvent = new ManualResetEvent(false);
            pubnub.ChannelGroupGrantAccess<string>(channelGroupName2, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummySubscribeErrorCallback);
            Thread.Sleep(1000);
            grantManualEvent.WaitOne(310 * 1000);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannelGroup Grant access failed.");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            currentUnitTestCase = "ThenSubscribeShouldReturnReceivedMessage";
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannelGroup";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnReceivedMessage";

            pubnub.PubnubUnitTest = unitTest;

            channelGroupName = "hello_my_group";
            string channelName = "hello_my_channel";

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup<string>(new string[] { channelName }, channelGroupName, ChannelGroupAddCallback, DummySubscribeErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
            if (receivedChannelGroupMessage)
            {
                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Subscribe<string>("", channelGroupName, ReceivedMessageCallbackWhenSubscribed, SubscribeConnectCallback, DummySubscribeErrorCallback);
                Thread.Sleep(1000);
                pubnub.Publish<string>(channelName, "Test for WhenSubscribedToAChannelGroup ThenItShouldReturnReceivedMessage", dummyPublishCallback, DummyPublishErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mePublish.WaitOne(manualResetEventsWaitTimeout);

                subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>("", channelGroupName, dummyUnsubscribeCallback, SubscribeConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummySubscribeErrorCallback);

                subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                
                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenItShouldReturnReceivedMessage Failed");
            }
            else
            {
                Assert.IsTrue(receivedChannelGroupMessage, "WhenSubscribedToAChannelGroup --> ThenItShouldReturnReceivedMessage Failed");
            }

        }

        [Test]
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            currentUnitTestCase = "ThenSubscribeShouldReturnConnectStatus";
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannelGroup";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnConnectStatus";

            pubnub.PubnubUnitTest = unitTest;


            channelGroupName = "hello_my_group";
            string channelName = "hello_my_channel";

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup<string>(new string[] { channelName }, channelGroupName, ChannelGroupAddCallback, DummySubscribeErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            if (receivedChannelGroupMessage)
            {
                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Subscribe<string>("", channelGroupName, ReceivedMessageCallbackWhenSubscribed, SubscribeConnectCallback, DummySubscribeErrorCallback);
                Thread.Sleep(1000);

                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;

                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenSubscribeShouldReturnConnectStatus Failed");
            }
            else
            {
                Assert.IsTrue(receivedChannelGroupMessage, "WhenSubscribedToAChannelGroup --> ThenSubscribeShouldReturnConnectStatus Failed");
            }
        }

        [Test]
        public void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            currentUnitTestCase = "ThenMultiSubscribeShouldReturnConnectStatus";
            receivedMessage = false;
            receivedChannelGroupMessage1 = false;
            receivedChannelGroupMessage2 = false;
            expectedCallbackResponses = 2;
            currentCallbackResponses = 0;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannelGroup";
            unitTest.TestCaseName = "ThenMultiSubscribeShouldReturnConnectStatus";

            pubnub.PubnubUnitTest = unitTest;
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 6000 : 310 * 1000;

            channelGroupName1 = "hello_my_group1";
            channelGroupName2 = "hello_my_group2";

            string channelName1 = "hello_my_channel1";
            string channelName2 = "hello_my_channel2";
            string channel1 = "hello_my_channel1";

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup<string>(new string[] { channelName1 }, channelGroupName1, ChannelGroupAddCallback, DummySubscribeErrorCallback);
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup<string>(new string[] { channelName2 }, channelGroupName2, ChannelGroupAddCallback, DummySubscribeErrorCallback);
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            if (receivedChannelGroupMessage1 && receivedChannelGroupMessage2)
            {
                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Subscribe<string>("", string.Format("{0},{1}", channelGroupName1, channelGroupName2), ReceivedMessageCallbackWhenSubscribed, SubscribeConnectCallback, DummySubscribeErrorCallback);
                subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;

                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenMultiSubscribeShouldReturnConnectStatusFailed");
            }
            else
            {
                Assert.IsTrue(receivedChannelGroupMessage1 && receivedChannelGroupMessage2, "WhenSubscribedToAChannelGroup --> ThenMultiSubscribeShouldReturnConnectStatusFailed");
            }
        }

        private void ReceivedMessageCallbackWhenSubscribed(string result)
        {
            if (currentUnitTestCase == "ThenMultiSubscribeShouldReturnConnectStatus")
            {
                return;
            }
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object subscribedObject = (object)deserializedMessage[0];
                    if (subscribedObject != null)
                    {
                        receivedMessage = true;
                    }
                }
            }
            subscribeManualEvent.Set();
        }

        void ChannelGroupAddCallback(string receivedMessage)
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
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string serviceType = dictionary["service"].ToString();
                            bool errorStatus = (bool)dictionary["error"];
                            string currentChannelGroup = serializedMessage[1].ToString().Substring(1); //assuming no namespace for channel group
                            string statusMessage = dictionary["message"].ToString();

                            if (statusCode == 200 && statusMessage.ToLower() == "ok" && serviceType == "channel-registry" && !errorStatus)
                            {
                                if (currentUnitTestCase == "ThenMultiSubscribeShouldReturnConnectStatus")
                                {
                                    if (currentChannelGroup == channelGroupName1)
                                    {
                                        receivedChannelGroupMessage1 = true;
                                    }
                                    else if (currentChannelGroup == channelGroupName2)
                                    {
                                        receivedChannelGroupMessage2 = true;
                                    }
                                }
                                else
                                {
                                    if (currentChannelGroup == channelGroupName)
                                    {
                                        receivedChannelGroupMessage = true;
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
                subscribeManualEvent.Set();
            }

        }
        
        void SubscribeConnectCallback(string result)
        {
            if (currentUnitTestCase == "ThenSubscribeShouldReturnConnectStatus")
            {
                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
                {
                    List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                    if (deserializedMessage != null && deserializedMessage.Count > 0)
                    {
                        long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                        string statusMessage = (string)deserializedMessage[1];
                        if (statusCode == 1 && statusMessage.ToLower() == "connected")
                        {
                            receivedMessage = true;
                        }
                    }
                }
                subscribeManualEvent.Set();
            }
            else if (currentUnitTestCase == "ThenMultiSubscribeShouldReturnConnectStatus")
            {
                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
                {
                    List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                    if (deserializedMessage != null && deserializedMessage.Count > 0)
                    {
                        long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                        string statusMessage = (string)deserializedMessage[1];
                        if (statusCode == 1 && statusMessage.ToLower() == "connected")
                        {
                            currentCallbackResponses = currentCallbackResponses + 1;
                            if (expectedCallbackResponses == currentCallbackResponses)
                            {
                                receivedMessage = true;
                            }
                        }
                    }
                }
                if (expectedCallbackResponses == currentCallbackResponses)
                {
                    subscribeManualEvent.Set();
                }
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

        private void DummySubscribeErrorCallback(PubnubClientError result)
        {
            Console.WriteLine(result.ToString());
            if (currentUnitTestCase == "Init")
            {
                grantManualEvent.Set();
            }
            else
            {
                subscribeManualEvent.Set();
            }
        }

        private void dummyPublishCallback(string result)
        {
            Console.WriteLine("dummyPublishCallback -> result = " + result);
            mePublish.Set();
        }

        private void DummyPublishErrorCallback(PubnubClientError result)
        {
            mePublish.Set();
        }

        private void dummyUnsubscribeCallback(string result)
        {

        }
        
        void UnsubscribeDummyMethodForDisconnectCallback(string receivedMessage)
        {
            subscribeManualEvent.Set();
        }

    }
}

