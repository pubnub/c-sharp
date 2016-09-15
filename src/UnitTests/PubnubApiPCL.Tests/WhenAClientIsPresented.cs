using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubnubApi;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAClientIsPresented : TestHarness
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
        Dictionary<string, object> dicState = null;
        string currentTestCase = "";
        string whereNowChannel = "";
        int manualResetEventsWaitTimeout = 310 * 1000;

        Pubnub pubnub = null;

        [TestFixtureSetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel,hello_my_channel-pnpres";

            pubnub.Grant().Channels(new string[] { channel }).Read(true).Write(true).Manage(false).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenPresenceInitializeShouldReturnGrantMessage, Error = DummyErrorCallback });
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
        [Ignore]
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            subscribeManualEvent = new ManualResetEvent(false);
            unsubscribeManualEvent = new ManualResetEvent(false);

            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            presenceManualEvent = new ManualResetEvent(false);
            //pubnub.Presence(channel, ThenPresenceShouldReturnMessage, PresenceDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).WithPresence().Execute(new SubscribeCallback<string>() { Connect = PresenceDummyMethodForConnectCallback, Error = DummyErrorCallback, Presence = ThenPresenceShouldReturnMessage, Disconnect = UnsubscribeDummyMethodForDisconnectCallback });
            presenceManualEvent.WaitOne(manualResetEventsWaitTimeout);

            //Thread.Sleep(1000);

            presenceManualEvent = new ManualResetEvent(false);

            //since presence expects from stimulus from sub/unsub...
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = UnsubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            presenceManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            subscribeManualEvent = new ManualResetEvent(false);
            unsubscribeManualEvent = new ManualResetEvent(false);

            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            presenceManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).WithPresence().Execute(new SubscribeCallback<string>() { Presence = ThenPresenceShouldReturnMessage, Connect = PresenceDummyMethodForConnectCallback, Disconnect = UnsubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            presenceManualEvent.WaitOne(manualResetEventsWaitTimeout);

            presenceManualEvent = new ManualResetEvent(false);

            //since presence expects from stimulus from sub/unsub...
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = UnsubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            presenceManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            subscribeUUIDManualEvent = new ManualResetEvent(false);
            unsubscribeUUIDManualEvent = new ManualResetEvent(false);

            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            presenceUUIDManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).WithPresence().Execute(new SubscribeCallback<string>() { Presence = ThenPresenceWithCustomUUIDShouldReturnMessage, Connect = PresenceUUIDDummyMethodForConnectCallback, Disconnect = UnsubscribeUUIDDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            presenceUUIDManualEvent.WaitOne(manualResetEventsWaitTimeout);
            
            //Thread.Sleep(1000);
            presenceUUIDManualEvent = new ManualResetEvent(false);
            
            //since presence expects from stimulus from sub/unsub...
            pubnub.SessionUUID = customUUID;
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribeUUID, Connect = SubscribeUUIDDummyMethodForConnectCallback, Disconnect = UnsubscribeUUIDDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(2000);
            subscribeUUIDManualEvent.WaitOne(manualResetEventsWaitTimeout);

            presenceUUIDManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            string channel = "hello_my_channel";

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = UnsubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new PNCallback<PNHereNowResult>() { Result = ThenHereNowShouldReturnMessage, Error = DummyErrorCallback });
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CiperKey = "enigma",
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new PNCallback<PNHereNowResult>() { Result = ThenHereNowShouldReturnMessage, Error = DummyErrorCallback });
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                CiperKey = "enigma",
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new PNCallback<PNHereNowResult>() { Result = ThenHereNowShouldReturnMessage, Error = DummyErrorCallback });
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                CiperKey = "enigma",
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new PNCallback<PNHereNowResult>() { Result = ThenHereNowShouldReturnMessage, Error = DummyErrorCallback });
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CiperKey = "enigma",
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new PNCallback<PNHereNowResult>() { Result = ThenHereNowShouldReturnMessage, Error = DummyErrorCallback });
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(2000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new PNCallback<PNHereNowResult>() { Result = ThenHereNowShouldReturnMessage, Error = DummyErrorCallback });
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(2000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new PNCallback<PNHereNowResult>() { Result = ThenHereNowShouldReturnMessage, Error = DummyErrorCallback });
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(2000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new PNCallback<PNHereNowResult>() { Result = ThenHereNowShouldReturnMessage, Error = DummyErrorCallback });
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            userStateManualEvent = new ManualResetEvent(false);
            //jsonUserState = "{\"testkey\":\"testval\"}";
            dicState = new Dictionary<string, object>();
            dicState.Add("testkey", "testval");

            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new PNCallback<PNSetStateResult>() { Result = SetUserStateDummyMethodCallback, Error = DummyErrorCallback });
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).IncludeState(true).IncludeUUIDs(true).Async(new PNCallback<PNHereNowResult>() { Result = ThenHereNowShouldReturnMessage, Error = DummyErrorCallback });
            hereNowManualEvent.WaitOne(manualResetEventsWaitTimeout);

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            globalHereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().IncludeUUIDs(true).IncludeState(true).Async(new PNCallback<PNHereNowResult>() { Result = ThenGlobalHereNowShouldReturnMessage, Error = DummyErrorCallback });
            globalHereNowManualEvent.WaitOne();

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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            userStateManualEvent = new ManualResetEvent(false);

            dicState = new Dictionary<string, object>();
            dicState.Add("testkey", "testval");
            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new PNCallback<PNSetStateResult>() { Result = SetUserStateDummyMethodCallback, Error = DummyErrorCallback });

            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(1000);

            globalHereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().IncludeState(true).IncludeUUIDs(true).Async(new PNCallback<PNHereNowResult>() { Result = ThenGlobalHereNowShouldReturnMessage, Error = DummyErrorCallback });
            globalHereNowManualEvent.WaitOne();

            unsubscribeManualEvent = new ManualResetEvent(false);
            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            whereNowChannel = "hello_my_channel";

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { whereNowChannel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = SubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            whereNowManualEvent = new ManualResetEvent(false);
            pubnub.WhereNow().Uuid(customUUID).Async(new PNCallback<PNWhereNowResult>() { Result = ThenWhereNowShouldReturnMessage, Error = DummyErrorCallback });
            whereNowManualEvent.WaitOne();

            if (!PubnubCommon.EnableStubTest)
            {
                unsubscribeManualEvent = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>().Channels(new string[] { whereNowChannel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            pubnub.SessionUUID = customUUID;

            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            string channel = "hello_my_channel";

            userStateManualEvent = new ManualResetEvent(false);

            dicState = new Dictionary<string, object>();
            dicState.Add("testkey", "testval");
            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new PNCallback<PNSetStateResult>() { Result = SetUserStateDummyMethodCallback, Error = DummyErrorCallback });

            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            if (receivedUserStateMessage)
            {
                Thread.Sleep(2000);
                receivedUserStateMessage = false;
                userStateManualEvent = new ManualResetEvent(false);
                pubnub.GetPresenceState()
                                .Channels(new string[] { channel })
                                .Async(new PNCallback<PNGetStateResult>() { Result = GetUserStateRegularCallback, Error = DummyErrorCallback });
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);
            pubnub.SessionUUID = customUUID;

            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            string channel = "hello_my_channel";

            //jsonUserState = "{\"k\":\"v\"}";
            //KeyValuePair<string, object> kvp = new KeyValuePair<string, object>("k", "v");
            userStateManualEvent = new ManualResetEvent(false);
            //pubnub.SetUserState(new string[] { channel }, kvp, SetUserStateDummyMethodCallback, DummyErrorCallback);
            dicState = new Dictionary<string, object>();
            dicState.Add("k", "v");
            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new PNCallback<PNSetStateResult>() { Result = SetUserStateDummyMethodCallback, Error = DummyErrorCallback });

            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);
            receivedUserStateMessage = false;
            //KeyValuePair<string, object> kvp2 = new KeyValuePair<string, object>("k2", "v2");
            userStateManualEvent = new ManualResetEvent(false);
            //pubnub.SetUserState(new string[] { channel }, kvp2, SetUserStateDummyMethodCallback, DummyErrorCallback);
            dicState = new Dictionary<string, object>();
            dicState.Add("k2", "v2");
            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new PNCallback<PNSetStateResult>() { Result = SetUserStateDummyMethodCallback, Error = DummyErrorCallback });

            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);
            receivedUserStateMessage = false;
            userStateManualEvent = new ManualResetEvent(false);
            pubnub.GetPresenceState()
                            .Channels(new string[] { channel })
                            .Async(new PNCallback<PNGetStateResult>() { Result = GetUserStateRegularCallback, Error = DummyErrorCallback });

            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);
            receivedUserStateMessage = false;
            //KeyValuePair<string, object> kvp22 = new KeyValuePair<string, object>("k2", null);
            userStateManualEvent = new ManualResetEvent(false);
            //pubnub.SetUserState(new string[] { channel }, kvp22, SetUserStateDummyMethodCallback, DummyErrorCallback);
            dicState = new Dictionary<string, object>();
            dicState.Add("k2", null);
            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new PNCallback<PNSetStateResult>() { Result = SetUserStateDummyMethodCallback, Error = DummyErrorCallback });
            userStateManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(2000);
            receivedUserStateMessage = false;
            userStateManualEvent = new ManualResetEvent(false);
            pubnub.GetPresenceState()
                            .Channels(new string[] { channel })
                            .Async(new PNCallback<PNGetStateResult>() { Result = GetUserStateRegularCallback, Error = DummyErrorCallback });

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
            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            config.SetPresenceHeartbeatTimeout(5);

            pubnub = this.createPubNubInstance(config);

            presenceManualEvent = new ManualResetEvent(false);
            subscribeManualEvent = new ManualResetEvent(false);
            unsubscribeManualEvent = new ManualResetEvent(false);

            string channel = "hello_my_channel";
            manualResetEventsWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            pubnub.Subscribe<string>().Channels(new string[] { channel }).WithPresence().Execute(new SubscribeCallback<string>() { Presence = ThenPresenceShouldReturnMessage, Connect = PresenceDummyMethodForConnectCallback, Disconnect = UnsubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(1000);

            //since presence expects from stimulus from sub/unsub...
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute(new SubscribeCallback<string>() { Message = DummyMethodForSubscribe, Connect = SubscribeDummyMethodForConnectCallback, Disconnect = UnsubscribeDummyMethodForDisconnectCallback, Error = DummyErrorCallback });
            Thread.Sleep(1000);
            subscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            Thread.Sleep(pubnub.PNConfig.PresenceHeartbeatTimeout+3 * 1000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
            Thread.Sleep(1000);
            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

            presenceManualEvent.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "ThenPresenceHeartbeatShouldReturnMessage not received");
        }

        void ThenPresenceInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
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
        
        void ThenPresenceShouldReturnMessage(PNPresenceEventResult receivedMessage)
        {
            try
            {
                Console.WriteLine("ThenPresenceShouldReturnMessage -> result = " + receivedMessage.Action);
                string action = receivedMessage.Action.ToLower() ;
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
            catch { }
            finally
            {
                presenceManualEvent.Set();
            }
        }

        void ThenPresenceWithCustomUUIDShouldReturnMessage(PNPresenceEventResult receivedMessage)
        {
            try
            {
                Console.WriteLine("ThenPresenceWithCustomUUIDShouldReturnMessage -> result = " + receivedMessage.Action);

                if (receivedMessage != null && !string.IsNullOrWhiteSpace(receivedMessage.UUID) && receivedMessage.UUID.Contains(customUUID))
                {
                    receivedCustomUUID = true;
                }
            }
            catch { }
            finally
            {
                presenceUUIDManualEvent.Set();
            }
        }

        void ThenHereNowShouldReturnMessage(PNHereNowResult receivedMessage)
        {
            try
            {
                if (receivedMessage != null && receivedMessage.Payload != null)
                {
                    string channelName = receivedMessage.ChannelName;

                    Console.WriteLine("ThenHereNowShouldReturnMessage -> result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage));
                    Dictionary<string,PNHereNowResult.Data.ChannelData> channelDataDic = receivedMessage.Payload.Channels;
                    if (channelDataDic != null && channelDataDic.Count > 0)
                    {
                        PNHereNowResult.Data.ChannelData.UuidData[] uuidDataArray = channelDataDic["hello_my_channel"].Uuids;
                        if (uuidDataArray != null && uuidDataArray.Length > 0)
                        {
                            if (currentTestCase == "IfHereNowIsCalledThenItShouldReturnInfoWithUserState")
                            {
                                foreach (PNHereNowResult.Data.ChannelData.UuidData uuidData in uuidDataArray)
                                {
                                    if (uuidData.Uuid != null && uuidData.State != null)
                                    {
                                        string receivedState = pubnub.JsonPluggableLibrary.SerializeToJsonString(uuidData.State);
                                        string receivedUUID = uuidData.Uuid;

                                        if (receivedUUID == pubnub.SessionUUID && receivedState == jsonUserState)
                                        {
                                            receivedHereNowMessage = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (PNHereNowResult.Data.ChannelData.UuidData uuidData in uuidDataArray)
                                {
                                    if (PubnubCommon.EnableStubTest)
                                    {
                                        receivedHereNowMessage = true;
                                        break;
                                    }
                                    if (uuidData.Uuid != null && uuidData.Uuid == pubnub.SessionUUID)
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
            catch { }
            finally
            {
                hereNowManualEvent.Set();
            }
        }

        void ThenGlobalHereNowShouldReturnMessage(PNHereNowResult receivedMessage)
        {
            try
            {
                Console.WriteLine(string.Format("ThenGlobalHereNowShouldReturnMessage result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage)));
                if (receivedMessage != null)
                {
                    if (receivedMessage.Payload != null)
                    {
                        Dictionary<string, PNHereNowResult.Data.ChannelData> channels = receivedMessage.Payload.Channels;
                        if (channels != null && channels.Count >= 0)
                        {
                            if (channels.Count == 0)
                            {
                                receivedGlobalHereNowMessage = true;
                            }
                            else
                            {
                                foreach (KeyValuePair<string, PNHereNowResult.Data.ChannelData> channelUUID in channels)
                                {
                                    var channelName = channelUUID.Key;
                                    PNHereNowResult.Data.ChannelData channelUuidListDictionary = channelUUID.Value;
                                    if (channelUuidListDictionary != null && channelUuidListDictionary.Uuids != null)
                                    {
                                        if (PubnubCommon.EnableStubTest)
                                        {
                                            receivedGlobalHereNowMessage = true;
                                            break;
                                        }

                                        PNHereNowResult.Data.ChannelData.UuidData[] uuidDataList = channelUuidListDictionary.Uuids;
                                        if (currentTestCase == "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState")
                                        {
                                            foreach (PNHereNowResult.Data.ChannelData.UuidData uuidData in uuidDataList)
                                            {
                                                if (uuidData.Uuid != null && uuidData.State != null)
                                                {
                                                    string receivedState = pubnub.JsonPluggableLibrary.SerializeToJsonString(uuidData.State);
                                                    string receivedUUID = uuidData.Uuid;
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
            catch { }
            finally
            {
                globalHereNowManualEvent.Set();
            }
        }

        void ThenWhereNowShouldReturnMessage(PNWhereNowResult receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    Console.WriteLine(string.Format("ThenWhereNowShouldReturnMessage result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage)));

                    if (receivedMessage.Payload != null)
                    {
                        string[] channels = receivedMessage.Payload.Channels;
                        if (channels != null && channels.Length >= 0)
                        {
                            foreach (string channel in channels)
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
            catch { }
            finally
            {
                whereNowManualEvent.Set();
            }
        }

        void DummyMethodForSubscribe(PNMessageResult<string> receivedMessage)
        {
            try
            {
                if (receivedMessage != null && !string.IsNullOrEmpty(receivedMessage.Data) && !string.IsNullOrEmpty(receivedMessage.Data.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage.Data);
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

        void DummyMethodForSubscribeUUID(PNMessageResult<string> receivedMessage)
        {
            try
            {
                if (receivedMessage != null && !string.IsNullOrEmpty(receivedMessage.Data) && !string.IsNullOrEmpty(receivedMessage.Data.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage.Data);
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

        void PresenceDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            presenceManualEvent.Set();
        }

        void PresenceUUIDDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            presenceUUIDManualEvent.Set();
        }

        void SubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            subscribeManualEvent.Set();
        }

        void SubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            unsubscribeManualEvent.Set();
        }

        void SubscribeUUIDDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            subscribeUUIDManualEvent.Set();
        }


        void UnsubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
        }

        void UnsubscribeUUIDDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
        }

        void UnsubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            unsubscribeManualEvent.Set();
        }

        void UnsubscribeUUIDDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            unsubscribeUUIDManualEvent.Set();
        }

        void SetUserStateDummyMethodCallback(PNSetStateResult receivedMessage)
        {
            Console.WriteLine(string.Format("SetUserStateDummyMethodCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage)));
            receivedUserStateMessage = true;
            userStateManualEvent.Set();
        }

        void GetUserStateRegularCallback(PNGetStateResult receivedMessage)
        {
            try
            {
                Console.WriteLine(string.Format("GetUserStateRegularCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage)));
                if (receivedMessage != null)
                {
                    string uuid = receivedMessage.UUID;
                    string channel = receivedMessage.ChannelName[0];
                    Dictionary<string, object> receivedUserStatePayload = receivedMessage.Payload;
                    Dictionary<string, object> expectedUserState = pubnub.JsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonUserState);
                    string receivedPayload = pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedUserStatePayload);
                    string expectedPayload = pubnub.JsonPluggableLibrary.SerializeToJsonString(expectedUserState);

                    if (uuid == pubnub.SessionUUID && receivedPayload == expectedPayload)
                    {
                        receivedUserStateMessage = true;
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
