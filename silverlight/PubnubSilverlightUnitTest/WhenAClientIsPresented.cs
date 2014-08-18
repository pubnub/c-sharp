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
        static bool receivedWhereNowMessage = false;
        static bool receivedGlobalHereNowMessage = false;
        static bool receivedCustomUUID = false;

        ManualResetEvent mrePresence = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);

        string customUUID = "mylocalmachine.mydomain.com";
        bool receivedGrantMessage = false;
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

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "GrantRequestUnitTest";
                    unitTest.TestCaseName = "Init2";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel,hello_my_channel-pnpres";
                    EnqueueCallback(() => pubnub.GrantAccess<string>(channel, true, true, 20, ThenPresenceInitializeShouldReturnGrantMessage, DummyErrorCallback));
                    mreGrant.WaitOne(310 * 1000);

                    EnqueueCallback(() => Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed."));
                    EnqueueTestComplete();
                });
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
                mreGrant.Set();
            }
        }
        
        [TestMethod, Asynchronous]
        public void ThenPresenceShouldReturnReceivedMessage()
        {
            receivedPresenceMessage = false;
            mrePresence = new ManualResetEvent(false);
            
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

                    mrePresence.WaitOne(310 * 1000);
                    EnqueueCallback(() => pubnub.EndPendingRequests());
                    EnqueueCallback(() => Assert.IsTrue(receivedPresenceMessage, "Presence message not received"));
                    EnqueueTestComplete();
                    
                });
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

            mrePresence.Set();
        }

        [Asynchronous]
        void PresenceDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        public void DummyMethodForSubscribe(string receivedMessage)
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

            mrePresence.Set();
            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        [Asynchronous]
        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        public void DummyMethodForUnSubscribe(string receivedMessage)
        {
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
            mrePresence = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "ThenPresenceShouldReturnCustomUUID";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.SessionUUID = customUUID;

                    string channel = "hello_my_channel";

                    EnqueueCallback(() => pubnub.Presence<string>(channel, ThenPresenceWithCustomUUIDShouldReturnMessage, PresenceUUIDDummyMethodForConnectCallback, DummyErrorCallback));
                    mrePresence.WaitOne(310 * 1000);

                    mrePresence = new ManualResetEvent(false);
                    //since presence expects from stimulus from sub/unsub...
                    EnqueueCallback(() =>
                    {
                        pubnub.Subscribe<string>(channel, DummyMethodForSubscribeUUID, SubscribeUUIDDummyMethodForConnectCallback, DummyErrorCallback);
                    });
                    mrePresence.WaitOne(310 * 1000);
                    EnqueueCallback(() => pubnub.EndPendingRequests());
                    EnqueueCallback(() => Assert.IsTrue(receivedCustomUUID, "Custom UUID not received"));
                    EnqueueTestComplete();
                });
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

            mrePresence.Set();
        }

        [Asynchronous]
        void PresenceUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
            mrePresence.Set();
        }

        [Asynchronous]
        void DummyMethodForSubscribeUUID(string receivedMessage)
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

            mrePresence.Set();
            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        [Asynchronous]
        void SubscribeUUIDDummyMethodForConnectCallback(string receivedMessage)
        {

        }

        [Asynchronous]
        void DummyMethodForUnSubscribeUUID(string receivedMessage)
        {
            //Dummary callback method for unsubscribe to test presence
        }

        [Asynchronous]
        void UnsubscribeUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        void UnsubscribeUUIDDummyMethodForDisconnectCallback(string receivedMessage)
        {

        }

        [TestMethod, Asynchronous]
        public void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            mrePresence = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback));
                    mrePresence.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(receivedHereNowMessage, "here_now message not received"));
                    EnqueueTestComplete();
                });
        }

        [TestMethod, Asynchronous]
        public void IfGlobalHereNowIsCalledThenItShouldReturnInfo()
        {
            mrePresence = new ManualResetEvent(false);
            
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "IfGlobalHereNowIsCalledThenItShouldReturnInfo";
                    pubnub.PubnubUnitTest = unitTest;
                
                    EnqueueCallback(() => pubnub.GlobalHereNow<string>(true, true, ThenGlobalHereNowShouldReturnMessage, DummyErrorCallback));
                    mrePresence.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(receivedGlobalHereNowMessage, "global_here_now message not received"));
                    EnqueueTestComplete();
                });
        }

        [TestMethod, Asynchronous]
        public void IfWhereNowIsCalledThenItShouldReturnInfo()
        {
            mrePresence = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string uuid = "hello_my_uuid";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "IfWhereNowIsCalledThenItShouldReturnInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.WhereNow<string>(uuid, ThenWhereNowShouldReturnMessage, DummyErrorCallback));
                    mrePresence.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(receivedWhereNowMessage, "where_now message not received"));
                    EnqueueTestComplete();
                });
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

            mrePresence.Set();
        }

        [Asynchronous]
        void ThenGlobalHereNowShouldReturnMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var payload = dictionary.Value<JContainer>("payload");
                    if (payload != null)
                    {
                        var channels = payload.Value<JContainer>("channels");
                        if (channels != null && channels.Count >= 0)
                        {
                            receivedGlobalHereNowMessage = true;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                mrePresence.Set();
            }
        }

        [Asynchronous]
        public void ThenWhereNowShouldReturnMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var payload = dictionary.Value<JContainer>("payload");
                    if (payload != null)
                    {
                        var channels = payload.Value<JContainer>("channels");
                        if (channels != null && channels.Count >= 0)
                        {
                            receivedWhereNowMessage = true;
                        }
                    }
                }
            }
            catch { }

            mrePresence.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
