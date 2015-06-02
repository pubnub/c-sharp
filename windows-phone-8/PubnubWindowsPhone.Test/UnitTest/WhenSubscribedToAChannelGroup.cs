using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Phone.Testing;
using PubNubMessaging.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace PubnubWindowsPhone.Test.UnitTest
{
    [TestClass]
    public class WhenSubscribedToAChannelGroup : WorkItemTest
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

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "GrantRequestUnitTest";
                    unitTest.TestCaseName = "Init3";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.ChannelGroupGrantAccess<string>(channelGroupName, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummySubscribeErrorCallback);
                    Thread.Sleep(1000);

                    grantManualEvent.WaitOne();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannelGroup Grant access failed.");
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                            TestComplete();
                        });
                });
        }

        [TestMethod, Asynchronous]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            currentUnitTestCase = "ThenSubscribeShouldReturnReceivedMessage";
            receivedMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
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

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenItShouldReturnReceivedMessage Failed");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(receivedChannelGroupMessage, "WhenSubscribedToAChannelGroup --> ThenItShouldReturnReceivedMessage Failed");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                });
        }

        [TestMethod, Asynchronous]
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            currentUnitTestCase = "ThenSubscribeShouldReturnConnectStatus";
            receivedMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
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

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenSubscribeShouldReturnConnectStatus Failed");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(receivedChannelGroupMessage, "WhenSubscribedToAChannelGroup --> ThenSubscribeShouldReturnConnectStatus Failed");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                });
        }

        [TestMethod, Asynchronous]
        public void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            currentUnitTestCase = "ThenMultiSubscribeShouldReturnConnectStatus";
            receivedMessage = false;
            receivedChannelGroupMessage1 = false;
            receivedChannelGroupMessage2 = false;
            expectedCallbackResponses = 2;
            currentCallbackResponses = 0;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
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

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannelGroup --> ThenMultiSubscribeShouldReturnConnectStatusFailed");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(receivedChannelGroupMessage1 && receivedChannelGroupMessage2, "WhenSubscribedToAChannelGroup --> ThenMultiSubscribeShouldReturnConnectStatusFailed");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                });
        }

        [Asynchronous]
        private void ReceivedMessageCallbackWhenSubscribed(string result)
        {
            if (currentUnitTestCase == "ThenMultiSubscribeShouldReturnConnectStatus")
            {
                return;
            }
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
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

        [Asynchronous]
        void ChannelGroupAddCallback(string receivedMessage)
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
                        string serviceType = dictionary.Value<string>("service");
                        bool errorStatus = dictionary.Value<bool>("error");
                        string currentChannelGroup = serializedMessage[1].ToString().Substring(1); //assuming no namespace for channel group
                        string statusMessage = dictionary.Value<string>("message");
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
            catch { }
            finally
            {
                subscribeManualEvent.Set();
            }

        }

        [Asynchronous]
        void SubscribeConnectCallback(string result)
        {
            if (currentUnitTestCase == "ThenSubscribeShouldReturnConnectStatus")
            {
                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
                {
                    object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                    if (deserializedMessage is object[])
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
                    object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                    if (deserializedMessage is object[])
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

        [Asynchronous]
        void ThenChannelGroupInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var status = dictionary["status"].ToString();
                    if (status == "200")
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

        [Asynchronous]
        private void DummySubscribeErrorCallback(PubnubClientError result)
        {
            subscribeManualEvent.Set();
        }

        [Asynchronous]
        private void dummyPublishCallback(string result)
        {
            Console.WriteLine("dummyPublishCallback -> result = " + result);
            mePublish.Set();
        }

        [Asynchronous]
        private void DummyPublishErrorCallback(PubnubClientError result)
        {
            mePublish.Set();
        }

        [Asynchronous]
        private void dummyUnsubscribeCallback(string result)
        {

        }

        [Asynchronous]
        void UnsubscribeDummyMethodForDisconnectCallback(string receivedMessage)
        {
            subscribeManualEvent.Set();
        }

    }
}
