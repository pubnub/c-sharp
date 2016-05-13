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
using PubnubApi;

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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = PubnubCommon.SecretKey;
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init3";
            pubnub.PubnubUnitTest = unitTest;

            grantManualEvent = new ManualResetEvent(false);
            pubnub.ChannelGroupGrantAccess(channelGroupName, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummySubscribeErrorCallback);
            Thread.Sleep(1000);
            grantManualEvent.WaitOne(310*1000);

            grantManualEvent = new ManualResetEvent(false);
            pubnub.ChannelGroupGrantAccess(channelGroupName1, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummySubscribeErrorCallback);
            Thread.Sleep(1000);
            grantManualEvent.WaitOne(310 * 1000);

            grantManualEvent = new ManualResetEvent(false);
            pubnub.ChannelGroupGrantAccess(channelGroupName2, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummySubscribeErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = "";
            config.CiperKey = "";
            config.Secure = false;

            config.Uuid = "myuuid";
            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannelGroup";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnReceivedMessage";

            pubnub.PubnubUnitTest = unitTest;

            channelGroupName = "hello_my_group";
            string channelName = "hello_my_channel";

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup(new string[] { channelName }, channelGroupName, ChannelGroupAddCallback, DummySubscribeErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
            if (receivedChannelGroupMessage)
            {
                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Subscribe<string>("", channelGroupName, ReceivedMessageCallbackWhenSubscribed, SubscribeConnectCallback, SubscribeDisconnectCallback, DummySubscribeErrorCallback);
                Thread.Sleep(1000);
                pubnub.Publish(channelName, "Test for WhenSubscribedToAChannelGroup ThenItShouldReturnReceivedMessage", dummyPublishCallback, DummyPublishErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mePublish.WaitOne(manualResetEventsWaitTimeout);

                subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>("", channelGroupName, DummySubscribeErrorCallback);

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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = "";
            config.CiperKey = "";
            config.Secure = false;

            config.Uuid = "myuuid";

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToAChannelGroup";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnConnectStatus";

            pubnub.PubnubUnitTest = unitTest;


            channelGroupName = "hello_my_group";
            string channelName = "hello_my_channel";

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup(new string[] { channelName }, channelGroupName, ChannelGroupAddCallback, DummySubscribeErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            if (receivedChannelGroupMessage)
            {
                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Subscribe<string>("", channelGroupName, ReceivedMessageCallbackWhenSubscribed, SubscribeConnectCallback, SubscribeDisconnectCallback, DummySubscribeErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = "";
            config.CiperKey = "";
            config.Secure = false;

            config.Uuid = "myuuid";
            pubnub = new Pubnub(config);

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
            pubnub.AddChannelsToChannelGroup(new string[] { channelName1 }, channelGroupName1, ChannelGroupAddCallback, DummySubscribeErrorCallback);
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup(new string[] { channelName2 }, channelGroupName2, ChannelGroupAddCallback, DummySubscribeErrorCallback);
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            if (receivedChannelGroupMessage1 && receivedChannelGroupMessage2)
            {
                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Subscribe<string>("", string.Format("{0},{1}", channelGroupName1, channelGroupName2), ReceivedMessageCallbackWhenSubscribed, SubscribeConnectCallback, SubscribeDisconnectCallback, DummySubscribeErrorCallback);
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

        private void ReceivedMessageCallbackWhenSubscribed(Message<string> result)
        {
            if (currentUnitTestCase == "ThenMultiSubscribeShouldReturnConnectStatus")
            {
                return;
            }
            if (result != null && result.Data != null)
            {
                receivedMessage = true;
            }
            subscribeManualEvent.Set();
        }

        void ChannelGroupAddCallback(AddChannelToChannelGroupAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string serviceType = receivedMessage.Service;
                    bool errorStatus = receivedMessage.Error;
                    string currentChannelGroup = receivedMessage.ChannelGroupName.Substring(1); //assuming no namespace for channel group
                    string statusMessage = receivedMessage.StatusMessage;

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
            catch { }
            finally
            {
                subscribeManualEvent.Set();
            }

        }
        
        void SubscribeConnectCallback(ConnectOrDisconnectAck result)
        {
            if (currentUnitTestCase == "ThenSubscribeShouldReturnConnectStatus")
            {
                if (result != null)
                    {
                        long statusCode = result.StatusCode;
                        string statusMessage = result.StatusMessage;
                        if (statusCode == 1 && statusMessage.ToLower() == "connected")
                        {
                            receivedMessage = true;
                        }
                    }
                subscribeManualEvent.Set();
            }
            else if (currentUnitTestCase == "ThenMultiSubscribeShouldReturnConnectStatus")
            {
                if (result != null)
                {
                    long statusCode = result.StatusCode;
                    string statusMessage = result.StatusMessage;
                    if (statusCode == 1 && statusMessage.ToLower() == "connected")
                    {
                        currentCallbackResponses = currentCallbackResponses + 1;
                        if (expectedCallbackResponses == currentCallbackResponses)
                        {
                            receivedMessage = true;
                        }
                    }
                }
                if (expectedCallbackResponses == currentCallbackResponses)
                {
                    subscribeManualEvent.Set();
                }
            }
        }

        void SubscribeDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            subscribeManualEvent.Set();
        }

        void ThenChannelGroupInitializeShouldReturnGrantMessage(GrantAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    var status = receivedMessage.StatusCode;
                    if (status == 200)
                    {
                        receivedGrantMessage = true;
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

        private void dummyPublishCallback(PublishAck result)
        {
            Console.WriteLine("dummyPublishCallback -> result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
            mePublish.Set();
        }

        private void DummyPublishErrorCallback(PubnubClientError result)
        {
            mePublish.Set();
        }

        void UnsubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            subscribeManualEvent.Set();
        }

    }
}

