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
    public class WhenUnsubscribedToAChannelGroup
    {
        ManualResetEvent unsubscribeManualEvent = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool receivedMessage = false;
        bool receivedGrantMessage = false;
        bool receivedChannelGroupMessage = false;
        bool receivedChannelGroupConnectedMessage = false;

        string currentUnitTestCase = "";
        string channelGroupName = "hello_my_group";

        int manualResetEventsWaitTimeout = 310 * 1000;

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

            pubnub.ChannelGroupGrantAccess<string>(channelGroupName, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummyUnsubscribeErrorCallback);
            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenUnsubscribedToAChannelGroup Grant access failed.");
        }

        [Test]
        public void ThenShouldReturnUnsubscribedMessage()
        {
            currentUnitTestCase = "ThenShouldReturnUnsubscribedMessage";
            receivedMessage = false;
            receivedChannelGroupMessage = false;
            receivedChannelGroupConnectedMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenUnsubscribedToAChannelGroup";
            unitTest.TestCaseName = "ThenShouldReturnUnsubscribedMessage";

            pubnub.PubnubUnitTest = unitTest;

            channelGroupName = "hello_my_group";
            string channelName = "hello_my_channel";

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup<string>(new string[] { channelName }, channelGroupName, ChannelGroupAddCallback, DummySubscribeErrorCallback);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
            if (receivedChannelGroupMessage)
            {
                unsubscribeManualEvent = new ManualResetEvent(false);
                pubnub.Subscribe<string>("", channelGroupName, DummyMethodChannelSubscribeUserCallback, DummyMethodChannelSubscribeConnectCallback, DummyErrorCallback);
                Thread.Sleep(1000);
                unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

                if (receivedChannelGroupConnectedMessage)
                {
                    unsubscribeManualEvent = new ManualResetEvent(false);
                    pubnub.Unsubscribe<string>("", channelGroupName, DummyMethodUnsubscribeChannelUserCallback, DummyMethodUnsubscribeChannelConnectCallback, DummyMethodUnsubscribeChannelDisconnectCallback, DummyErrorCallback);
                    unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
                }

                pubnub.EndPendingRequests();
                pubnub = null;

                Assert.IsTrue(receivedMessage, "WhenUnsubscribedToAChannelGroup --> ThenShouldReturnUnsubscribedMessage Failed");
            }
            else
            {
                Assert.IsTrue(receivedChannelGroupMessage, "WhenUnsubscribedToAChannelGroup --> ThenShouldReturnUnsubscribedMessage Failed");
            }
        }

        private void DummyMethodChannelSubscribeUserCallback(string result)
        {
        }

        private void DummyMethodChannelSubscribeConnectCallback(string result)
        {
            if (result.Contains("Connected"))
            {
                receivedChannelGroupConnectedMessage = true;
            }
            unsubscribeManualEvent.Set();
        }

        private void DummyMethodUnsubscribeChannelUserCallback(string result)
        {
        }

        private void DummyMethodUnsubscribeChannelConnectCallback(string result)
        {
        }

        private void DummyMethodUnsubscribeChannelDisconnectCallback(string result)
        {
            if (result.Contains("Unsubscribed from"))
            {
                receivedMessage = true;
            }
            unsubscribeManualEvent.Set();
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
                unsubscribeManualEvent.Set();
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

        private void DummyUnsubscribeErrorCallback(PubnubClientError result)
        {
            unsubscribeManualEvent.Set();
        }

        private void DummySubscribeErrorCallback(PubnubClientError result)
        {
            unsubscribeManualEvent.Set();
        }

        private void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
