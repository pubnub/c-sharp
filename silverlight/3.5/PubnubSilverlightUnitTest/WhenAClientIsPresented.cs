using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Microsoft.Silverlight.Testing;
using PubNubMessaging.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PubnubSilverlight.UnitTest
{
    [TestClass]
    public class WhenAClientIsPresented : SilverlightTest
    {
        static bool receivedPresenceMessage = false;
        static bool receivedHereNowMessage = false;
        static bool receivedCustomUUID = false;

        bool presenceReturnMessageCallbackInvoked = false;
        bool subscribeCallbackInvoked = false;
        bool subscribeConnectStatusCallbackInvoked = false;
        bool presenceUUIDCallbackInvoked = false;
        bool subscribeUUIDCallbackInvoked = false;
        bool subscribeUUIDConnectStatusCallbackInvoked = false;
        bool unSubscribeCallbackInvoked = false;
        bool unSubscribeUUIDCallbackInvoked = false;
        bool hereNowReturnMessageCallbackInvoked = false;

        string customUUID = "mylocalmachine.mydomain.com";
        bool receivedGrantMessage = false;
        //bool receivedGrantMessage2 = false;
        bool grantInitCallbackInvoked = false;

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                EnqueueTestComplete();
                return;
            }

            receivedGrantMessage = false;
            //receivedGrantMessage2 = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init2";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel,hello_my_channel-pnpres";
            EnqueueCallback(() => pubnub.GrantAccess<string>(channel, true, true, 20, ThenPresenceInitializeShouldReturnGrantMessage, DummyErrorCallback));
            Thread.Sleep(1000);
            EnqueueConditional(() => grantInitCallbackInvoked);
            EnqueueCallback(() => Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed."));
            EnqueueTestComplete();
        }

        [Asynchronous]
        void ThenPresenceInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    int statusCode = dictionary.Value<int>("status");
                    string statusMessage = dictionary.Value<string>("message");
                    //if (statusCode == 200)
                    //{
                    //    receivedGrantMessage = true;
                    //}
                    if (statusCode == 200 && statusMessage.ToLower() == "success")
                    {
                        var payload = dictionary.Value<JContainer>("payload");
                        if (payload != null)
                        {
                            string level = payload.Value<string>("level");
                            var channels = payload.Value<JContainer>("channels");
                            if (channels != null)
                            {
                                foreach (JToken channelToken in channels)
                                {
                                    if (channelToken is JProperty)
                                    {
                                        var channelContainer = channelToken as JProperty;
                                        if (channelContainer.Name == "hello_my_channel")
                                        {
                                            receivedGrantMessage = true;
                                        }
                                        else if (channelContainer.Name == "hello_my_channel-pnpres")
                                        {
                                            receivedGrantMessage = true;
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
                grantInitCallbackInvoked = true;
            }
        }
        
        [TestMethod, Asynchronous]
        public void ThenPresenceShouldReturnReceivedMessage()
        {
            receivedPresenceMessage = false;
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "ThenPresenceShouldReturnReceivedMessage";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Presence<string>(channel, ThenPresenceShouldReturnMessage, PresenceDummyMethodForConnectCallback, DummyErrorCallback));
                    EnqueueCallback(() => pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback));
                    EnqueueConditional(() => subscribeConnectStatusCallbackInvoked);
                    EnqueueConditional(() => subscribeCallbackInvoked);
                    EnqueueCallback(() => pubnub.EndPendingRequests());
                    EnqueueConditional(() => presenceReturnMessageCallbackInvoked);
                    EnqueueCallback(() => Assert.IsTrue(receivedPresenceMessage, "Presence message not received"));
                });
            EnqueueTestComplete();
        }

        [Asynchronous]
        public void ThenPresenceShouldReturnMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    object[] receivedObj = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dic = receivedObj[0] as JContainer;
                    var uuid = dic["uuid"].ToString();
                    if (uuid != null)
                    {
                        receivedPresenceMessage = true;
                    }
                }
            }
            catch { }

            presenceReturnMessageCallbackInvoked = true;
        }

        [Asynchronous]
        void PresenceDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        public void DummyMethodForSubscribe(string receivedMessage)
        {
            subscribeCallbackInvoked = true;

            try
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    object[] receivedObj = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dic = receivedObj[0] as JContainer;
                    var uuid = dic["uuid"].ToString();
                    if (uuid != null)
                    {
                        receivedPresenceMessage = true;
                    }
                }
            }
            catch { }

            presenceReturnMessageCallbackInvoked = true;
            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        [Asynchronous]
        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
            //subscribeManualEvent.Set();
            subscribeConnectStatusCallbackInvoked = true;
        }

        [Asynchronous]
        public void DummyMethodForUnSubscribe(string receivedMessage)
        {
            unSubscribeCallbackInvoked = true;
            //Dummary callback method for unsubscribe to test presence
        }

        [Asynchronous]
        void UnsubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        void UnsubscribeDummyMethodForDisconnectCallback(string receivedMessage)
        {
            
        }

        [TestMethod, Asynchronous]
        public void ThenPresenceShouldReturnCustomUUID()
        {
            receivedCustomUUID = false;
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "ThenPresenceShouldReturnCustomUUID";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";

                    EnqueueCallback(() => pubnub.Presence<string>(channel, ThenPresenceWithCustomUUIDShouldReturnMessage, PresenceUUIDDummyMethodForConnectCallback, DummyErrorCallback));
                    //Thread.Sleep(1000);

                    //since presence expects from stimulus from sub/unsub...
                    EnqueueCallback(() =>
                    {
                        pubnub.SessionUUID = customUUID;
                        pubnub.Subscribe<string>(channel, DummyMethodForSubscribeUUID, SubscribeUUIDDummyMethodForConnectCallback, DummyErrorCallback);
                    });
                    EnqueueConditional(() => subscribeUUIDConnectStatusCallbackInvoked);
                    EnqueueConditional(() => subscribeUUIDCallbackInvoked);
                    //Thread.Sleep(1000);

                    //presenceUUIDManualEvent.WaitOne();
                    //Thread.Sleep(1000);
                    EnqueueCallback(() => pubnub.EndPendingRequests());
                    EnqueueConditional(() => presenceUUIDCallbackInvoked);
                    EnqueueCallback(() => Assert.IsTrue(receivedCustomUUID, "Custom UUID not received"));
                });
            EnqueueTestComplete();
        }

        [Asynchronous]
        void ThenPresenceWithCustomUUIDShouldReturnMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var uuid = dictionary["uuid"].ToString();
                    if (uuid != null && uuid.Contains(customUUID))
                    {
                        receivedCustomUUID = true;
                    }
                }
            }
            catch { }

            presenceUUIDCallbackInvoked = true;
        }

        [Asynchronous]
        void PresenceUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        void DummyMethodForSubscribeUUID(string receivedMessage)
        {
            subscribeUUIDCallbackInvoked = true;

            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var uuid = dictionary["uuid"].ToString();
                    if (uuid != null && uuid.Contains(customUUID))
                    {
                        receivedCustomUUID = true;
                    }
                }
            }
            catch { }

            presenceUUIDCallbackInvoked = true;

            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        [Asynchronous]
        void SubscribeUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
            subscribeUUIDConnectStatusCallbackInvoked = true;
        }

        [Asynchronous]
        void DummyMethodForUnSubscribeUUID(string receivedMessage)
        {
            unSubscribeUUIDCallbackInvoked = true;
            //Dummary callback method for unsubscribe to test presence
        }

        [Asynchronous]
        void UnsubscribeUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        void UnsubscribeUUIDDummyMethodForDisconnectCallback(string receivedMessage)
        {
            //unsubscribeUUIDManualEvent.Set();
        }

        [TestMethod, Asynchronous]
        public void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            string channel = "hello_my_channel";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;

            EnqueueCallback(() => pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback));
            EnqueueConditional(() => hereNowReturnMessageCallbackInvoked);
            EnqueueCallback(() => Assert.IsTrue(receivedHereNowMessage, "here_now message not received"));
            EnqueueTestComplete();
        }

        [Asynchronous]
        public void ThenHereNowShouldReturnMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    var dictionary = ((JContainer)serializedMessage[0])["uuids"];
                    if (dictionary != null)
                    {
                        receivedHereNowMessage = true;
                    }
                }
            }
            catch { }

            hereNowReturnMessageCallbackInvoked = true;
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
