using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
    public class WhenAClientIsPresented
    {
        ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        ManualResetEvent presenceManualEvent = new ManualResetEvent(false);
        ManualResetEvent unsubscribeManualEvent = new ManualResetEvent(false);

        ManualResetEvent subscribeUUIDManualEvent = new ManualResetEvent(false);
        ManualResetEvent presenceUUIDManualEvent = new ManualResetEvent(false);
        ManualResetEvent unsubscribeUUIDManualEvent = new ManualResetEvent(false);

        ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
        ManualResetEvent globalHereNowManualEvent = new ManualResetEvent(false);
        ManualResetEvent whereNowManualEvent = new ManualResetEvent(false);
        ManualResetEvent presenceUnsubscribeEvent = new ManualResetEvent(false);
        ManualResetEvent presenceUnsubscribeUUIDEvent = new ManualResetEvent(false);

        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        static bool receivedPresenceMessage = false;
        static bool receivedHereNowMessage = false;
        static bool receivedGlobalHereNowMessage = false;
        static bool receivedWhereNowMessage = false;
        static bool receivedCustomUUID = false;
        static bool receivedGrantMessage = false;

        string customUUID = "mylocalmachine.mydomain.com";
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
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel,hello_my_channel-pnpres";

            pubnub.GrantAccess<string>(channel, true, true, 20, ThenPresenceInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {

        }
    
        [Test]
        public void ThenPresenceShouldReturnReceivedMessage()
        {
            receivedPresenceMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "ThenPresenceShouldReturnReceivedMessage";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            pubnub.Presence<string>(channel, ThenPresenceShouldReturnMessage, PresenceDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            
            //since presence expects from stimulus from sub/unsub...
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            presenceManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests();

            pubnub.EndPendingRequests();
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "Presence message not received");
        }

        [Test]
        public void ThenPresenceShouldReturnCustomUUID()
        {
            receivedCustomUUID = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "ThenPresenceShouldReturnCustomUUID";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            pubnub.Presence<string>(channel, ThenPresenceWithCustomUUIDShouldReturnMessage, PresenceUUIDDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            
            //since presence expects from stimulus from sub/unsub...
            pubnub.SessionUUID = customUUID;
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribeUUID, SubscribeUUIDDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            subscribeUUIDManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribeUUID, UnsubscribeUUIDDummyMethodForConnectCallback, UnsubscribeUUIDDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeUUIDManualEvent.WaitOne(manualResetEventsWaitTimeout);

            presenceUUIDManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests();

            pubnub.EndPendingRequests();
            pubnub = null;
            Assert.IsTrue(receivedCustomUUID, "Custom UUID not received");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            receivedHereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            string channel = "hello_my_channel";
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received");
        }

        [Test]
        public void IfGlobalHereNowIsCalledThenItShouldReturnInfo()
        {
            receivedGlobalHereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfGlobalHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            pubnub.GlobalHereNow<string>(true, true, ThenGlobalHereNowShouldReturnMessage, DummyErrorCallback);
            globalHereNowManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub = null;
            Assert.IsTrue(receivedGlobalHereNowMessage, "global_here_now message not received");
        }

        [Test]
        public void IfWhereNowIsCalledThenItShouldReturnInfo()
        {
            receivedWhereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfWhereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            string uuid = "hello_my_uuid";
            pubnub.WhereNow<string>(uuid, ThenWhereNowShouldReturnMessage, DummyErrorCallback);
            whereNowManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub = null;
            Assert.IsTrue(receivedWhereNowMessage, "where_now message not received");
        }

        void ThenPresenceInitializeShouldReturnGrantMessage(string receivedMessage)
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
        
        void ThenPresenceShouldReturnMessage(string receivedMessage)
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
                            var uuid = dictionary["uuid"].ToString();
                            if (uuid != null)
                            {
                                receivedPresenceMessage = true;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                presenceManualEvent.Set();
            }
        }

        void ThenPresenceWithCustomUUIDShouldReturnMessage(string receivedMessage)
        {
            try
            {
                Console.WriteLine("ThenPresenceWithCustomUUIDShouldReturnMessage -> result = " + receivedMessage);
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null && dictionary.Count > 0)
                        {
                            var uuid = dictionary["uuid"].ToString();
                            if (uuid != null && uuid.Contains(customUUID))
                            {
                                receivedCustomUUID = true;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                presenceUUIDManualEvent.Set();
            }
        }

        void ThenHereNowShouldReturnMessage(string receivedMessage)
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
                            var uuids = dictionary["uuids"];
                            if (uuids != null)
                            {
                                receivedHereNowMessage = true;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                hereNowManualEvent.Set();
            }
        }

        void ThenGlobalHereNowShouldReturnMessage(string receivedMessage)
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
                            Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                            if (payload != null && payload.Count > 0)
                            {
                                Dictionary<string, object> channels = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channels"]);
                                if (channels != null && channels.Count >= 0)
                                {
                                    receivedGlobalHereNowMessage = true;
                                }
                            }
                        }
                    }
                    
                }
            }
            catch { }
            finally
            {
                globalHereNowManualEvent.Set();
            }
        }

        void ThenWhereNowShouldReturnMessage(string receivedMessage)
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
                            Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                            if (payload != null && payload.Count > 0)
                            {
                                object[] channels = pubnub.JsonPluggableLibrary.ConvertToObjectArray(payload["channels"]);
                                if (channels != null && channels.Length >= 0)
                                {
                                    receivedWhereNowMessage = true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                whereNowManualEvent.Set();
            }
        }

        void DummyMethodForSubscribe(string receivedMessage)
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
                            var uuid = dictionary["uuid"].ToString();
                            if (uuid != null)
                            {
                                receivedPresenceMessage = true;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                presenceManualEvent.Set();
            }
            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        void DummyMethodForSubscribeUUID(string receivedMessage)
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
                            var uuid = dictionary["uuid"].ToString();
                            if (uuid != null)
                            {
                                receivedCustomUUID = true;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                presenceUUIDManualEvent.Set();
            }
            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        void DummyMethodForUnSubscribe(string receivedMessage)
        {
            //Dummary callback method for unsubscribe to test presence
        }

        void DummyMethodForUnSubscribeUUID(string receivedMessage)
        {
            //Dummary callback method for unsubscribe to test presence
        }

        void PresenceDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        void PresenceUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
            subscribeManualEvent.Set();
        }

        void SubscribeUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
            subscribeUUIDManualEvent.Set();
        }


        void UnsubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        void UnsubscribeUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        void UnsubscribeDummyMethodForDisconnectCallback(string receivedMessage)
        {
            unsubscribeManualEvent.Set();
        }

        void UnsubscribeUUIDDummyMethodForDisconnectCallback(string receivedMessage)
        {
            unsubscribeUUIDManualEvent.Set();
        }

        void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
