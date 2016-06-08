using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
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
        ManualResetEvent userStateManualEvent = new ManualResetEvent(false);

        static bool receivedPresenceMessage = false;
        static bool receivedHereNowMessage = false;
        static bool receivedGlobalHereNowMessage = false;
        static bool receivedWhereNowMessage = false;
        static bool receivedCustomUUID = false;
        static bool receivedGrantMessage = false;
        static bool receivedUserStateMessage = false;

        string customUUID = "mylocalmachine.mydomain.com";
        string jsonUserState = "";
        string currentTestCase = "";
        string whereNowChannel = "";
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
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {

        }

        #if (USE_JSONFX)
        [Test]
        #else
        [Ignore("")]
        #endif
        public void UsingJsonFx()
        {
            Console.Write("UsingJsonFx");
            Assert.True(true, "UsingJsonFx");
        }

        #if (USE_JSONFX)
        [Ignore]
        #else
        [Test]
        #endif
        public void UsingNewtonSoft()
        {
            Console.Write("UsingNewtonSoft");
            Assert.True(true, "UsingNewtonSoft");
        }
    
        [Test]
        public void ThenPresenceShouldReturnReceivedMessage()
        {
            receivedPresenceMessage = false;
            currentTestCase = "ThenPresenceShouldReturnReceivedMessage";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "ThenPresenceShouldReturnReceivedMessage";
            pubnub.PubnubUnitTest = unitTest;

            presenceManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);
            unsubscribeManualEvent = new ManualResetEvent(false);

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
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "Presence message not received");
        }

        [Test]
        public void ThenPresenceShouldReturnReceivedMessageSSL()
        {
            receivedPresenceMessage = false;
            currentTestCase = "ThenPresenceShouldReturnReceivedMessageSSL";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", true);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "ThenPresenceShouldReturnReceivedMessage";
            pubnub.PubnubUnitTest = unitTest;

            presenceManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);
            unsubscribeManualEvent = new ManualResetEvent(false);

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
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "Presence message not received");
        }

        [Test]
        public void ThenPresenceShouldReturnCustomUUID()
        {
            receivedCustomUUID = false;
            currentTestCase = "ThenPresenceShouldReturnCustomUUID";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "ThenPresenceShouldReturnCustomUUID";
            pubnub.PubnubUnitTest = unitTest;

            presenceUUIDManualEvent = new ManualResetEvent(false);
            subscribeUUIDManualEvent = new ManualResetEvent(false);
            unsubscribeUUIDManualEvent = new ManualResetEvent(false);

            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            pubnub.Presence<string>(channel, ThenPresenceWithCustomUUIDShouldReturnMessage, PresenceUUIDDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(2000);
            
            //since presence expects from stimulus from sub/unsub...
            pubnub.SessionUUID = customUUID;
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribeUUID, SubscribeUUIDDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeUUIDManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribeUUID, UnsubscribeUUIDDummyMethodForConnectCallback, UnsubscribeUUIDDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeUUIDManualEvent.WaitOne(manualResetEventsWaitTimeout);

            presenceUUIDManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
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

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipher()
        {
            receivedHereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecret()
        {
            receivedHereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "enigma", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher and secret");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL()
        {
            receivedHereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "enigma", true);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher, secret, ssl");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSSL()
        {
            receivedHereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", true);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher, ssl");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSecret()
        {
            receivedHereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with secret key");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSecretSSL()
        {
            receivedHereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", true);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received ,with secret key, ssl");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSSL()
        {
            receivedHereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", true);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with ssl");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoWithUserState()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfHereNowIsCalledThenItShouldReturnInfoWithUserState";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = customUUID;
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfoWithUserState";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            userStateManualEvent = new ManualResetEvent(false);
            jsonUserState = "{\"testkey\":\"testval\"}";
            pubnub.SetUserState<string>(channel, jsonUserState, SetUserStateDummyMethodCallback, DummyErrorCallback);
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow<string>(channel, true, true, ThenHereNowShouldReturnMessage, DummyErrorCallback);
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with user state");
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
            
			string channel = "hello_my_channel";
			manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

			subscribeManualEvent = new ManualResetEvent(false);
			pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
			subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            globalHereNowManualEvent = new ManualResetEvent(false);
            pubnub.GlobalHereNow<string>(true, true, ThenGlobalHereNowShouldReturnMessage, DummyErrorCallback);
            globalHereNowManualEvent.WaitOne();

			unsubscribeManualEvent = new ManualResetEvent(false);
			pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
			Thread.Sleep(1000);
			unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGlobalHereNowMessage, "global_here_now message not received");
        }

        [Test]
        public void IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState()
        {
            receivedGlobalHereNowMessage = false;
            currentTestCase = "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = customUUID;
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            userStateManualEvent = new ManualResetEvent(false);
            jsonUserState = "{\"testkey\":\"testval\"}";
            pubnub.SetUserState<string>(channel, jsonUserState, SetUserStateDummyMethodCallback, DummyErrorCallback);
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            globalHereNowManualEvent = new ManualResetEvent(false);
            pubnub.GlobalHereNow<string>(true, true, ThenGlobalHereNowShouldReturnMessage, DummyErrorCallback);
			globalHereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGlobalHereNowMessage, "global_here_now message not received for user state");
        }

        [Test]
        public void IfWhereNowIsCalledThenItShouldReturnInfo()
        {
            receivedWhereNowMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = customUUID;
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfWhereNowIsCalledThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;

            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            whereNowChannel = "hello_my_channel";

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>(whereNowChannel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
			Thread.Sleep(2000);

            whereNowManualEvent = new ManualResetEvent(false);
            pubnub.WhereNow<string>(customUUID, ThenWhereNowShouldReturnMessage, DummyErrorCallback);
            whereNowManualEvent.WaitOne();

            if (!pubnub.PubnubUnitTest.EnableStubTest)
            {
                unsubscribeManualEvent = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(whereNowChannel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                Thread.Sleep(1000);
                unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedWhereNowMessage, "where_now message not received");
        }

        [Test]
        public void IfSetAndGetUserStateThenItShouldReturnInfo()
        {
            receivedUserStateMessage = false;
            currentTestCase = "IfSetAndGetUserStateThenItShouldReturnInfo";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = customUUID;

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfSetAndGetUserStateThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;

            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            string channel = "hello_my_channel";

            jsonUserState = "{\"testkey\":\"testval\"}";
            userStateManualEvent = new ManualResetEvent(false);
            pubnub.SetUserState<string>(channel, jsonUserState, SetUserStateDummyMethodCallback, DummyErrorCallback);
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            if (receivedUserStateMessage)
            {
                Thread.Sleep(2000);
                receivedUserStateMessage = false;
                userStateManualEvent = new ManualResetEvent(false);
                pubnub.GetUserState<string>(channel, "", GetUserStateRegularCallback, DummyErrorCallback);
                userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedUserStateMessage, "IfSetAndGetUserStateThenItShouldReturnInfo failed");
        }

        [Test]
        public void IfSetAndDeleteUserStateThenItShouldReturnInfo()
        {
            receivedUserStateMessage = false;
            currentTestCase = "IfSetAndDeleteUserStateThenItShouldReturnInfo";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = customUUID;

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "IfSetAndDeleteUserStateThenItShouldReturnInfo";
            pubnub.PubnubUnitTest = unitTest;

            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            string channel = "hello_my_channel";

            jsonUserState = "{\"k\":\"v\"}";
            KeyValuePair<string, object> kvp = new KeyValuePair<string, object>("k", "v");
            userStateManualEvent = new ManualResetEvent(false);
            pubnub.SetUserState<string>(channel, kvp, SetUserStateDummyMethodCallback, DummyErrorCallback);
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);
            receivedUserStateMessage = false;
            KeyValuePair<string, object> kvp2 = new KeyValuePair<string, object>("k2", "v2");
            userStateManualEvent = new ManualResetEvent(false);
            pubnub.SetUserState<string>(channel, kvp2, SetUserStateDummyMethodCallback, DummyErrorCallback);
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);
            receivedUserStateMessage = false;
            userStateManualEvent = new ManualResetEvent(false);
            pubnub.GetUserState<string>(channel, "", GetUserStateRegularCallback, DummyErrorCallback);
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);
            receivedUserStateMessage = false;
            KeyValuePair<string, object> kvp22 = new KeyValuePair<string, object>("k2", null);
            userStateManualEvent = new ManualResetEvent(false);
            pubnub.SetUserState<string>(channel, kvp22, SetUserStateDummyMethodCallback, DummyErrorCallback);
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);
            receivedUserStateMessage = false;
            userStateManualEvent = new ManualResetEvent(false);
            pubnub.GetUserState<string>(channel, "", GetUserStateRegularCallback, DummyErrorCallback);
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);
            
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedUserStateMessage, "IfSetAndDeleteUserStateThenItShouldReturnInfo message not received");
        }

        [Test]
        public void ThenPresenceHeartbeatShouldReturnMessage()
        {
            receivedPresenceMessage = false;
            currentTestCase = "ThenPresenceHeartbeatShouldReturnMessage";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.PresenceHeartbeat = 5;
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "ThenPresenceShouldReturnReceivedMessage";
            pubnub.PubnubUnitTest = unitTest;

            presenceManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);
            unsubscribeManualEvent = new ManualResetEvent(false);

            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            pubnub.Presence<string>(channel, ThenPresenceShouldReturnMessage, PresenceDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);

            //since presence expects from stimulus from sub/unsub...
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(pubnub.PresenceHeartbeat+3 * 1000);

            pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            presenceManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "ThenPresenceHeartbeatShouldReturnMessage not received");
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
                Console.WriteLine("ThenPresenceShouldReturnMessage -> result = " + receivedMessage);
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null && dictionary.Count > 0 && dictionary.ContainsKey("uuid"))
                        {
                            string action = dictionary.ContainsKey("action") ? dictionary["action"].ToString() : "";
                            if (currentTestCase == "ThenPresenceHeartbeatShouldReturnMessage")
                            {
                                if (action == "timeout")
                                {
                                    receivedPresenceMessage = false;
                                }
                                else
                                {
                                    receivedPresenceMessage = true;
                                }
                            }
                            else
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
                Console.WriteLine("ThenHereNowShouldReturnMessage -> result = " + receivedMessage);
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null && dictionary.Count > 0)
                        {
							var uuids = (dictionary.ContainsKey("uuids")) ? dictionary["uuids"] : null;
                            if (uuids != null)
                            {
                                object[] uuidList = null;
                                if (uuids.GetType().Equals(typeof(string[])))
                                {
                                    uuidList = uuids as string[];
                                }
                                else if (uuids.GetType().Equals(typeof(object[])))
                                {
                                    uuidList = uuids as object[];
                                }
                                else
                                {
                                    uuidList = pubnub.JsonPluggableLibrary.ConvertToObjectArray(uuids);
                                }
                                if (uuidList != null)
                                {
                                    if (currentTestCase == "IfHereNowIsCalledThenItShouldReturnInfoWithUserState")
                                    {
                                        foreach (object obj in uuidList)
                                        {
                                            Dictionary<string, object> uuidDic = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(obj);
                                            if (uuidDic != null)
                                            {
                                                if (uuidDic.ContainsKey("uuid") && uuidDic.ContainsKey("state"))
                                                {
                                                    string receivedState = pubnub.JsonPluggableLibrary.SerializeToJsonString(uuidDic["state"]);
                                                    string receivedUUID = uuidDic["uuid"].ToString();
                                                    if (receivedUUID == pubnub.SessionUUID && receivedState == jsonUserState)
                                                    {
                                                        receivedHereNowMessage = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (object obj in uuidList)
                                        {
                                            if (pubnub.PubnubUnitTest != null && pubnub.PubnubUnitTest.EnableStubTest)
                                            {
                                                receivedHereNowMessage = true;
                                                break;
                                            }
                                            if (obj.Equals(pubnub.SessionUUID))
                                            {
                                                receivedHereNowMessage = true;
                                                break;
                                            }
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
                hereNowManualEvent.Set();
            }
        }

        void ThenGlobalHereNowShouldReturnMessage(string receivedMessage)
        {
            try
            {
                Console.WriteLine(string.Format("ThenGlobalHereNowShouldReturnMessage result = {0}", receivedMessage));
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
                                    foreach (KeyValuePair<string,object> channelUUID in channels)
                                    {
                                        var channelName = channelUUID.Key;
                                        Dictionary<string, object> channelUuidListDictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(channelUUID.Value);
                                        if (channelUuidListDictionary != null)
                                        {
                                            foreach (KeyValuePair<string, object> keyPair in channelUuidListDictionary)
                                            {
                                                if (pubnub.PubnubUnitTest != null && pubnub.PubnubUnitTest.EnableStubTest)
                                                {
                                                    receivedGlobalHereNowMessage = true;
                                                    break;
                                                }
                                                if (keyPair.Key == "uuids")
                                                {
                                                    object[] uuidList = pubnub.JsonPluggableLibrary.ConvertToObjectArray(keyPair.Value);
                                                    if (currentTestCase == "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState")
                                                    {
                                                        foreach (object uuid in uuidList)
                                                        {
                                                            Dictionary<string, object> uuidDic = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(uuid);
                                                            if (uuidDic.ContainsKey("uuid") && uuidDic.ContainsKey("state"))
                                                            {
                                                                string receivedState = pubnub.JsonPluggableLibrary.SerializeToJsonString(uuidDic["state"]);
                                                                string receivedUUID = uuidDic["uuid"].ToString();
                                                                if (receivedUUID == pubnub.SessionUUID && receivedState == jsonUserState)
                                                                {
                                                                    receivedGlobalHereNowMessage = true;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        receivedGlobalHereNowMessage = true;
                                                        break;
                                                    }

                                                }
                                            }
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
                                    foreach (object channel in channels)
                                    {
                                        if (channel.Equals(whereNowChannel))
                                        {
                                            receivedWhereNowMessage = true;
                                            break;
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

        void SetUserStateDummyMethodCallback(string receivedMessage)
        {
            Console.WriteLine(string.Format("SetUserStateDummyMethodCallback result = {0}", receivedMessage));
            receivedUserStateMessage = true;
            userStateManualEvent.Set();
        }

        void GetUserStateRegularCallback(string receivedMessage)
        {
            try
            {
                Console.WriteLine(string.Format("GetUserStateRegularCallback result = {0}", receivedMessage));
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null && dictionary.Count > 0)
                        {
                            string uuid = (dictionary.ContainsKey("uuid")) ? dictionary["uuid"].ToString() : "";
                            string channel = (dictionary.ContainsKey("channel")) ? dictionary["channel"].ToString() : "";
                            string receivedUserState = "";
                            if (dictionary.ContainsKey("payload"))
                            {
                                receivedUserState = pubnub.JsonPluggableLibrary.SerializeToJsonString(dictionary["payload"]);
                            }
                            if (uuid == pubnub.SessionUUID && jsonUserState == receivedUserState)
                            {
                                receivedUserStateMessage = true;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                userStateManualEvent.Set();
            }
        }

        void DummyErrorCallback(PubnubClientError result)
        {
            if (currentTestCase == "IfSetAndGetUserStateThenItShouldReturnInfo")
            {
                userStateManualEvent.Set();
            }
        }

    }
}
