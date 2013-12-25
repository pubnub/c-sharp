using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class WhenAClientIsPresented: UUnitTestCase
    {
        ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        ManualResetEvent presenceManualEvent = new ManualResetEvent(false);
        ManualResetEvent unsubscribeManualEvent = new ManualResetEvent(false);

        ManualResetEvent subscribeUUIDManualEvent = new ManualResetEvent(false);
        ManualResetEvent presenceUUIDManualEvent = new ManualResetEvent(false);
        ManualResetEvent unsubscribeUUIDManualEvent = new ManualResetEvent(false);

        ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
        //ManualResetEvent presenceUnsubscribeEvent = new ManualResetEvent(false);
        //ManualResetEvent presenceUnsubscribeUUIDEvent = new ManualResetEvent(false);

        static bool receivedPresenceMessage = false;
        static bool receivedHereNowMessage = false;
        static bool receivedCustomUUID = false;

        string customUUID = "mylocalmachine.mydomain.com";

        [UUnitTest]
        public void ThenPresenceShouldReturnReceivedMessage()
        {
            Debug.Log("Running ThenPresenceShouldReturnReceivedMessage()");
            receivedPresenceMessage = false;

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "ThenPresenceShouldReturnReceivedMessage";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";

            pubnub.Presence<string>(channel, ThenPresenceShouldReturnMessage, PresenceDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            
            //since presence expects from stimulus from sub/unsub...
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(310 * 1000);

            presenceManualEvent.WaitOne(310 * 1000);

            pubnub.EndPendingRequests();
            
            UUnitAssert.True(receivedPresenceMessage, "Presence message not received");
        }

        [UUnitTest]
        public void ThenPresenceShouldReturnCustomUUID()
        {
            Debug.Log("Running ThenPresenceShouldReturnCustomUUID()");
            receivedCustomUUID = false;

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "ThenPresenceShouldReturnCustomUUID";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.Presence<string>(channel, ThenPresenceWithCustomUUIDShouldReturnMessage, PresenceUUIDDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            
            //since presence expects from stimulus from sub/unsub...
            pubnub.SessionUUID = customUUID;
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribeUUID, SubscribeUUIDDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            subscribeUUIDManualEvent.WaitOne(310 * 1000);

            presenceUUIDManualEvent.WaitOne(310 * 1000);

            pubnub.EndPendingRequests();

            UUnitAssert.True(receivedCustomUUID, "Custom UUID not received");
        }

        [UUnitTest]
        public void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            Debug.Log("Running IfHereNowIsCalledThenItShouldReturnInfo()");
            receivedHereNowMessage = false;

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(310 * 1000);
            UUnitAssert.True(receivedHereNowMessage, "here_now message not received");
        }

        void ThenPresenceShouldReturnMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                    object[] serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage).ToArray();
                    Dictionary<string,object> dictionary = serializedMessage[0] as Dictionary<string,object>;
                    var uuid = dictionary["uuid"].ToString();
                    if (uuid != null)
                    {
                        receivedPresenceMessage = true;
                    }
                }
            }
            catch{}
            finally
            {
                presenceManualEvent.Set();
            }
        }

        void ThenPresenceWithCustomUUIDShouldReturnMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                    object[] serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage).ToArray();
                    Dictionary<string,object> dictionary = serializedMessage[0] as Dictionary<string,object>;
                    var uuid = dictionary["uuid"].ToString();
                    if (uuid != null && uuid.Contains(customUUID))
                    {
                        receivedCustomUUID = true;
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
                    Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                    object[] serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage).ToArray();
                    Dictionary<string,object> dictionary = serializedMessage[0] as Dictionary<string,object>;
                    if (dictionary != null)
                    {
                        receivedHereNowMessage = true;
                    }
                }
            }
            catch { }
            finally
            {
                hereNowManualEvent.Set();
            }
        }

        void DummyMethodForSubscribe(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                    object[] serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage).ToArray();
                    Dictionary<string,object> dictionary = serializedMessage[0] as Dictionary<string,object>;
                    if (dictionary != null)
                    {
                        var uuid = dictionary["uuid"].ToString();
                        if (uuid != null)
                        {
                            receivedPresenceMessage = true;
                        }
                    }
                }
            }
            catch(Exception ex) 
            { 
                Debug.Log("DummyMethodForSubscribe exception = " + ex.ToString());
            }
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
                    Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                    object[] serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage).ToArray();
                    Dictionary<string,object> dictionary = serializedMessage[0] as Dictionary<string,object>;
                    if (dictionary != null)
                    {
                        var uuid = dictionary["uuid"].ToString();
                        if (uuid != null)
                        {
                            receivedCustomUUID = true;
                        }
                    }
                }
            }
            catch(Exception ex) 
            { 
                Debug.Log("DummyMethodForSubscribeUUID exception = " + ex.ToString());
            }
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

        void DummyErrorCallback(string result)
        {
            Debug.Log("WhenAClientIsPresented ErrorCallback : " + result);
        }
    }
}
