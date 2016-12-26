using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubnubApi;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAClientIsPresented : TestHarness
    {
        private static ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent presenceManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent whereNowManualEvent = new ManualResetEvent(false);

        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent userStateManualEvent = new ManualResetEvent(false);

        static bool receivedPresenceMessage = false;
        static bool receivedHereNowMessage = false;
        static bool receivedWhereNowMessage = false;
        static bool receivedCustomUUID = false;
        static bool receivedGrantMessage = false;
        static bool receivedUserStateMessage = false;

        string customUUID = "mylocalmachine.mydomain.com";
        string jsonUserState = "";
        Dictionary<string, object> dicState = null;
        private static string currentTestCase = "";
        string whereNowChannel = "";
        int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string authKey = "myAuth";

        private static Pubnub pubnub = null;

        private Server server;
        private UnitTestLog unitLog;

        [TestFixtureSetUp]
        public void Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = new Server(new Uri("https://" + PubnubCommon.StubOrign));
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();

            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", channel)
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "O6PrtqdYo_x7D6R5hEse1A_bd3VkZrHIDmU8LxwwdlU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed.");

            expected = "[14827611897607991]";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
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
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            presenceManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).WithPresence().Execute();
            presenceManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(1000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
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
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                Secure = true
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);


            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            presenceManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).WithPresence().Execute();
            presenceManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(1000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
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
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            presenceManualEvent = new ManualResetEvent(false);
            pubnub.ChangeUUID(customUUID);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).WithPresence().Execute();
            presenceManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(1000);

            

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedCustomUUID, "Custom UUID not received");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfHereNowIsCalledThenItShouldReturnInfo";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = "",
                Uuid = "mytestuuid",
                Secure = false
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("state", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipher()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfHereNowIsCalledThenItShouldReturnInfoCipher";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = "",
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = false
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("state", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecret()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfHereNowIsCalledThenItShouldReturnInfoCipherSecret";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid"
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("state", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher and secret");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = true
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("state", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher, secret, ssl");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSSL()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfHereNowIsCalledThenItShouldReturnInfoCipherSSL";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = "",
                CipherKey = "enigma",
                Uuid = "mytestuuid",
                Secure = true
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("state", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher, ssl");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSecret()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfHereNowIsCalledThenItShouldReturnInfoSecret";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("state", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with secret key");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSecretSSL()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfHereNowIsCalledThenItShouldReturnInfoSecretSSL";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                Secure = true
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("state", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received ,with secret key, ssl");
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSSL()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfHereNowIsCalledThenItShouldReturnInfoSSL";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = "",
                Uuid = "mytestuuid",
                Secure = true
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("state", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel }).Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
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
                Secure = false
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            userStateManualEvent = new ManualResetEvent(false);
            dicState = new Dictionary<string, object>();
            dicState.Add("testkey", "testval");

            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new UTPNSetStateResult());
            userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new string[] { channel })
                    .IncludeState(true)
                    .IncludeUUIDs(true)
                    .Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with user state");
        }

        [Test]
        public void IfGlobalHereNowIsCalledThenItShouldReturnInfo()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfGlobalHereNowIsCalledThenItShouldReturnInfo";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14827658395446362\",\"r\":7},\"m\":[]}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"channels\": {}, \"total_channels\": 0, \"total_occupancy\": 0}, \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("state", "0")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "global_here_now message not received");
        }

        [Test]
        public void IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState()
        {
            receivedHereNowMessage = false;
            currentTestCase = "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";

            string expected = "{\"t\":{\"t\":\"14827658395446362\",\"r\":7},\"m\":[]}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            userStateManualEvent = new ManualResetEvent(false);
            dicState = new Dictionary<string, object>();
            dicState.Add("testkey", "testval");

            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new UTPNSetStateResult());
            userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

           
            expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"channels\": {\"bot_object\": {\"uuids\": [{\"uuid\": \"0ccff0c1-aa81-421b-8c2b-08a59bd5138c\"}], \"occupancy\": 1}, \"hello_my_channel\": {\"uuids\": [{\"state\": {\"testkey\": \"testval\"}, \"uuid\": \"mytestuuid\"}], \"occupancy\": 1}}, \"total_channels\": 2, \"total_occupancy\": 2}, \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("state", "1")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow()
                    .IncludeState(true)
                    .IncludeUUIDs(true)
                    .Async(new UTHereNowResult());
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "[[],\"14740704540745015\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("state", "%7B%22hello_my_channel%22%3A%7B%22testkey%22%3A%22testval%22%7D%7D")
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "global_here_now message not received for user state");
        }

        [Test]
        public void IfWhereNowIsCalledThenItShouldReturnInfo()
        {
            receivedWhereNowMessage = false;
            currentTestCase = "IfWhereNowIsCalledThenItShouldReturnInfo";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14827658395446362\",\"r\":7},\"m\":[]}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "[[],\"14742283085315695\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/subscribe/{0}/{1}/0/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"channels\": {}, \"total_channels\": 0, \"total_occupancy\": 0}, \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/uuid/{1}", PubnubCommon.SubscribeKey, config.Uuid))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            whereNowManualEvent = new ManualResetEvent(false);
            pubnub.WhereNow().Uuid(config.Uuid).Async(new UTWhereNowResult());
            whereNowManualEvent.WaitOne();

            expected = "[[],\"14740704540745015\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
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
                Secure = false 
            };
            server.RunOnHttps(false);

            pubnub = this.createPubNubInstance(config);
            pubnub.ChangeUUID(customUUID);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            string channel = "hello_my_channel";

            userStateManualEvent = new ManualResetEvent(false);
            dicState = new Dictionary<string, object>();
            dicState.Add("testkey", "testval");

            string expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"testkey\": \"testval\"}, \"service\": \"Presence\"}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/uuid/{2}/data", PubnubCommon.SubscribeKey, channel, customUUID))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new UTPNSetStateResult());

            userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedUserStateMessage)
            {
                Thread.Sleep(2000);
                receivedUserStateMessage = false;
                userStateManualEvent = new ManualResetEvent(false);

                expected = "{\"status\": 200, \"uuid\": \"mylocalmachine.mydomain.com\", \"service\": \"Presence\", \"message\": \"OK\", \"payload\": {\"testkey\": \"testval\"}, \"channel\": \"hello_my_channel\"}";
                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/uuid/{2}", PubnubCommon.SubscribeKey, channel, customUUID))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.GetPresenceState()
                                .Channels(new string[] { channel })
                                .Async(new UTPNGetStateResult());
                userStateManualEvent.WaitOne(manualResetEventWaitTimeout);
            }
            pubnub.Destroy();
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
                Secure = false
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            string channel = "hello_my_channel";

            userStateManualEvent = new ManualResetEvent(false);
            dicState = new Dictionary<string, object>();
            dicState.Add("k", "v");

            pubnub.SetPresenceState()
                            .Channels(new string[] { channel })
                            .State(dicState)
                            .Async(new UTPNSetStateResult());

            userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            if (receivedUserStateMessage)
            {
                receivedUserStateMessage = false;
                userStateManualEvent = new ManualResetEvent(false);
                pubnub.GetPresenceState()
                                .Channels(new string[] { channel })
                                .Async(new UTPNGetStateResult());

                userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

                Thread.Sleep(2000);
            }

            if (receivedUserStateMessage)
            {
                receivedUserStateMessage = false;

                userStateManualEvent = new ManualResetEvent(false);
                dicState = new Dictionary<string, object>();
                dicState.Add("k", null);

                pubnub.SetPresenceState()
                                .Channels(new string[] { channel })
                                .State(dicState)
                                .Async(new UTPNSetStateResult());

                userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

                Thread.Sleep(2000);
            }

            if (receivedUserStateMessage)
            {
                receivedUserStateMessage = false;
                userStateManualEvent = new ManualResetEvent(false);
                pubnub.GetPresenceState()
                                .Channels(new string[] { channel })
                                .Async(new UTPNGetStateResult());

                userStateManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            pubnub.Destroy();
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
                Secure = false
            };

            config.SetPresenceTimeout(5);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            presenceManualEvent = new ManualResetEvent(false);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            presenceManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).WithPresence().Execute();
            presenceManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(pubnub.PNConfig.PresenceTimeout + 3 * 1000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "ThenPresenceHeartbeatShouldReturnMessage not received");
        }

        //void ThenPresenceInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
        //{
        //    try
        //    {
        //        if (receivedMessage != null)
        //        {
        //            var status = receivedMessage.StatusCode;
        //            if (status == 200)
        //            {
        //                receivedGrantMessage = true;
        //            }
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        grantManualEvent.Set();
        //    }
        //}

        //void ThenPresenceShouldReturnMessage(PNPresenceEventResult receivedMessage)
        //{
        //    try
        //    {
        //        Console.WriteLine("ThenPresenceShouldReturnMessage -> result = " + receivedMessage.Action);
        //        string action = receivedMessage.Action.ToLower();
        //        if (currentTestCase == "ThenPresenceHeartbeatShouldReturnMessage")
        //        {
        //            if (action == "timeout")
        //            {
        //                receivedPresenceMessage = false;
        //            }
        //            else
        //            {
        //                receivedPresenceMessage = true;
        //            }
        //        }
        //        else
        //        {
        //            receivedPresenceMessage = true;
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        presenceManualEvent.Set();
        //    }
        //}

        //void ThenPresenceWithCustomUUIDShouldReturnMessage(PNPresenceEventResult receivedMessage)
        //{
        //    try
        //    {
        //        Console.WriteLine("ThenPresenceWithCustomUUIDShouldReturnMessage -> result = " + receivedMessage.Action);

        //        if (receivedMessage != null && !string.IsNullOrWhiteSpace(receivedMessage.UUID) && receivedMessage.UUID.Contains(customUUID))
        //        {
        //            receivedCustomUUID = true;
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        presenceUUIDManualEvent.Set();
        //    }
        //}

        //void ThenHereNowShouldReturnMessage(PNHereNowResult receivedMessage)
        //{
        //    try
        //    {
        //        if (receivedMessage != null && receivedMessage.Payload != null)
        //        {
        //            string channelName = receivedMessage.ChannelName;

        //            Console.WriteLine("ThenHereNowShouldReturnMessage -> result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage));
        //            Dictionary<string, PNHereNowResult.Data.ChannelData> channelDataDic = receivedMessage.Payload.Channels;
        //            if (channelDataDic != null && channelDataDic.Count > 0)
        //            {
        //                PNHereNowResult.Data.ChannelData.UuidData[] uuidDataArray = channelDataDic["hello_my_channel"].Uuids;
        //                if (uuidDataArray != null && uuidDataArray.Length > 0)
        //                {
        //                    if (currentTestCase == "IfHereNowIsCalledThenItShouldReturnInfoWithUserState")
        //                    {
        //                        foreach (PNHereNowResult.Data.ChannelData.UuidData uuidData in uuidDataArray)
        //                        {
        //                            if (uuidData.Uuid != null && uuidData.State != null)
        //                            {
        //                                string receivedState = pubnub.JsonPluggableLibrary.SerializeToJsonString(uuidData.State);
        //                                string receivedUUID = uuidData.Uuid;

        //                                if (receivedUUID == pubnub.SessionUUID && receivedState == jsonUserState)
        //                                {
        //                                    receivedHereNowMessage = true;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        foreach (PNHereNowResult.Data.ChannelData.UuidData uuidData in uuidDataArray)
        //                        {
        //                            if (PubnubCommon.EnableStubTest)
        //                            {
        //                                receivedHereNowMessage = true;
        //                                break;
        //                            }
        //                            if (uuidData.Uuid != null && uuidData.Uuid == pubnub.SessionUUID)
        //                            {
        //                                receivedHereNowMessage = true;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        hereNowManualEvent.Set();
        //    }
        //}

        //void ThenGlobalHereNowShouldReturnMessage(PNHereNowResult receivedMessage)
        //{
        //    try
        //    {
        //        Console.WriteLine(string.Format("ThenGlobalHereNowShouldReturnMessage result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage)));
        //        if (receivedMessage != null)
        //        {
        //            if (receivedMessage.Payload != null)
        //            {
        //                Dictionary<string, PNHereNowResult.Data.ChannelData> channels = receivedMessage.Payload.Channels;
        //                if (channels != null && channels.Count >= 0)
        //                {
        //                    if (channels.Count == 0)
        //                    {
        //                        receivedGlobalHereNowMessage = true;
        //                    }
        //                    else
        //                    {
        //                        foreach (KeyValuePair<string, PNHereNowResult.Data.ChannelData> channelUUID in channels)
        //                        {
        //                            var channelName = channelUUID.Key;
        //                            PNHereNowResult.Data.ChannelData channelUuidListDictionary = channelUUID.Value;
        //                            if (channelUuidListDictionary != null && channelUuidListDictionary.Uuids != null)
        //                            {
        //                                if (PubnubCommon.EnableStubTest)
        //                                {
        //                                    receivedGlobalHereNowMessage = true;
        //                                    break;
        //                                }

        //                                PNHereNowResult.Data.ChannelData.UuidData[] uuidDataList = channelUuidListDictionary.Uuids;
        //                                if (currentTestCase == "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState")
        //                                {
        //                                    foreach (PNHereNowResult.Data.ChannelData.UuidData uuidData in uuidDataList)
        //                                    {
        //                                        if (uuidData.Uuid != null && uuidData.State != null)
        //                                        {
        //                                            string receivedState = pubnub.JsonPluggableLibrary.SerializeToJsonString(uuidData.State);
        //                                            string receivedUUID = uuidData.Uuid;
        //                                            if (receivedUUID == pubnub.SessionUUID && receivedState == jsonUserState)
        //                                            {
        //                                                receivedGlobalHereNowMessage = true;
        //                                                break;
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    receivedGlobalHereNowMessage = true;
        //                                    break;
        //                                }

        //                            }
        //                        }
        //                    }


        //                }
        //            }

        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        globalHereNowManualEvent.Set();
        //    }
        //}

        //void ThenWhereNowShouldReturnMessage(PNWhereNowResult receivedMessage)
        //{
        //    try
        //    {
        //        if (receivedMessage != null)
        //        {
        //            Console.WriteLine(string.Format("ThenWhereNowShouldReturnMessage result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage)));

        //            if (receivedMessage.Payload != null)
        //            {
        //                string[] channels = receivedMessage.Payload.Channels;
        //                if (channels != null && channels.Length >= 0)
        //                {
        //                    foreach (string channel in channels)
        //                    {
        //                        if (channel.Equals(whereNowChannel))
        //                        {
        //                            receivedWhereNowMessage = true;
        //                            break;
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        whereNowManualEvent.Set();
        //    }
        //}

        //void DummyMethodForSubscribe(PNMessageResult<string> receivedMessage)
        //{
        //    try
        //    {
        //        if (receivedMessage != null && !string.IsNullOrEmpty(receivedMessage.Data) && !string.IsNullOrEmpty(receivedMessage.Data.Trim()))
        //        {
        //            List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage.Data);
        //            if (serializedMessage != null && serializedMessage.Count > 0)
        //            {
        //                Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
        //                if (dictionary != null && dictionary.Count > 0)
        //                {
        //                    var uuid = dictionary["uuid"].ToString();
        //                    if (uuid != null)
        //                    {
        //                        receivedPresenceMessage = true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        presenceManualEvent.Set();
        //    }
        //    //Dummary callback method for subscribe and unsubscribe to test presence
        //}

        //void DummyMethodForSubscribeUUID(PNMessageResult<string> receivedMessage)
        //{
        //    try
        //    {
        //        if (receivedMessage != null && !string.IsNullOrEmpty(receivedMessage.Data) && !string.IsNullOrEmpty(receivedMessage.Data.Trim()))
        //        {
        //            List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage.Data);
        //            if (serializedMessage != null && serializedMessage.Count > 0)
        //            {
        //                Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
        //                if (dictionary != null && dictionary.Count > 0)
        //                {
        //                    var uuid = dictionary["uuid"].ToString();
        //                    if (uuid != null)
        //                    {
        //                        receivedCustomUUID = true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        presenceUUIDManualEvent.Set();
        //    }
        //    //Dummary callback method for subscribe and unsubscribe to test presence
        //}

        //void DummyMethodForUnSubscribe(string receivedMessage)
        //{
        //    //Dummary callback method for unsubscribe to test presence
        //}

        //void DummyMethodForUnSubscribeUUID(string receivedMessage)
        //{
        //    //Dummary callback method for unsubscribe to test presence
        //}

        //void PresenceDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //    presenceManualEvent.Set();
        //}

        //void PresenceUUIDDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //    presenceUUIDManualEvent.Set();
        //}

        //void SubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //    subscribeManualEvent.Set();
        //}

        //void SubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //    unsubscribeManualEvent.Set();
        //}

        //void SubscribeUUIDDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //    subscribeUUIDManualEvent.Set();
        //}


        //void UnsubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //}

        //void UnsubscribeUUIDDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //}

        //void UnsubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //    unsubscribeManualEvent.Set();
        //}

        //void UnsubscribeUUIDDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //    unsubscribeUUIDManualEvent.Set();
        //}

        //void SetUserStateDummyMethodCallback(PNSetStateResult receivedMessage)
        //{
        //    Console.WriteLine(string.Format("SetUserStateDummyMethodCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage)));
        //    receivedUserStateMessage = true;
        //    userStateManualEvent.Set();
        //}

        //void GetUserStateRegularCallback(PNGetStateResult receivedMessage)
        //{
        //    try
        //    {
        //        Console.WriteLine(string.Format("GetUserStateRegularCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage)));
        //        if (receivedMessage != null)
        //        {
        //            string uuid = receivedMessage.UUID;
        //            string channel = receivedMessage.ChannelName[0];
        //            Dictionary<string, object> receivedUserStatePayload = receivedMessage.Payload;
        //            Dictionary<string, object> expectedUserState = pubnub.JsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonUserState);
        //            string receivedPayload = pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedUserStatePayload);
        //            string expectedPayload = pubnub.JsonPluggableLibrary.SerializeToJsonString(expectedUserState);

        //            if (uuid == pubnub.SessionUUID && receivedPayload == expectedPayload)
        //            {
        //                receivedUserStateMessage = true;
        //            }
        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        userStateManualEvent.Set();
        //    }
        //}

        //void DummyErrorCallback(PubnubClientError result)
        //{
        //    if (currentTestCase == "IfSetAndGetUserStateThenItShouldReturnInfo")
        //    {
        //        userStateManualEvent.Set();
        //    }
        //}

        private class UTGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (result.Channels != null && result.Channels.Count > 0)
                        {
                            var read = result.Channels[channel][authKey].ReadEnabled;
                            var write = result.Channels[channel][authKey].WriteEnabled;
                            if (read && write)
                            {
                                receivedGrantMessage = true;
                            }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    grantManualEvent.Set();
                }
            }
        }

        public class UTSubscribeCallback : SubscribeCallback
        {
            public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
            {
                if (message != null)
                {
                    Console.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(message.Message));
                }
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
                Console.WriteLine("SubscribeCallback: Presence: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(presence));
                switch(currentTestCase)
                {
                    case "ThenPresenceShouldReturnReceivedMessage":
                    case "ThenPresenceShouldReturnReceivedMessageSSL":
                    case "ThenPresenceHeartbeatShouldReturnMessage":
                        if (presence.Event == "join")
                        {
                            receivedPresenceMessage = true;
                            presenceManualEvent.Set();
                        }
                        else if (presence.Event == "leave")
                        {
                            presenceManualEvent.Set();
                        }
                        break;
                    case "ThenPresenceShouldReturnCustomUUID":
                        receivedCustomUUID = true;
                        presenceManualEvent.Set();
                        break;
                    default:
                        break;
                }
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                //Console.WriteLine("SubscribeCallback: PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                Console.WriteLine("SubscribeCallback: PNStatus: " + status.StatusCode.ToString());
                if (status.StatusCode != 200 || status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "ThenPresenceShouldReturnReceivedMessage":
                        case "ThenPresenceShouldReturnReceivedMessageSSL":
                        case "ThenPresenceShouldReturnCustomUUID":
                        case "ThenPresenceHeartbeatShouldReturnMessage":
                            presenceManualEvent.Set();
                            break;
                        case "IfHereNowIsCalledThenItShouldReturnInfo":
                        case "IfHereNowIsCalledThenItShouldReturnInfoSSL":
                        case "IfHereNowIsCalledThenItShouldReturnInfoSecret":
                        case "IfHereNowIsCalledThenItShouldReturnInfoSecretSSL":
                        case "IfHereNowIsCalledThenItShouldReturnInfoCipher":
                        case "IfHereNowIsCalledThenItShouldReturnInfoCipherSSL":
                        case "IfHereNowIsCalledThenItShouldReturnInfoCipherSecret":
                        case "IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL":
                        case "IfHereNowIsCalledThenItShouldReturnInfoWithUserState":
                        case "IfGlobalHereNowIsCalledThenItShouldReturnInfo":
                        case "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState":
                        case "IfWhereNowIsCalledThenItShouldReturnInfo":
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    if (status.ErrorData != null)
                    {
                        Console.WriteLine(status.ErrorData.Information);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNConnectedCategory)
                {
                    switch(currentTestCase)
                    {
                        case "IfHereNowIsCalledThenItShouldReturnInfo":
                        case "IfHereNowIsCalledThenItShouldReturnInfoSSL":
                        case "IfHereNowIsCalledThenItShouldReturnInfoSecret":
                        case "IfHereNowIsCalledThenItShouldReturnInfoSecretSSL":
                        case "IfHereNowIsCalledThenItShouldReturnInfoCipher":
                        case "IfHereNowIsCalledThenItShouldReturnInfoCipherSSL":
                        case "IfHereNowIsCalledThenItShouldReturnInfoCipherSecret":
                        case "IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL":
                        case "IfHereNowIsCalledThenItShouldReturnInfoWithUserState":
                        case "IfGlobalHereNowIsCalledThenItShouldReturnInfo":
                        case "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState":
                        case "IfWhereNowIsCalledThenItShouldReturnInfo":
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }

                
            }
        }

        public class UTHereNowResult : PNCallback<PNHereNowResult>
        {
            public override void OnResponse(PNHereNowResult result, PNStatus status)
            {
                if (result == null) return;

                Console.WriteLine("HereNow Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("HereNow PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                //Dictionary<string, PNHereNowChannelData> test = result.Channels;
                switch (currentTestCase)
                {
                    case "IfHereNowIsCalledThenItShouldReturnInfo":
                    case "IfHereNowIsCalledThenItShouldReturnInfoSSL":
                    case "IfHereNowIsCalledThenItShouldReturnInfoSecret":
                    case "IfHereNowIsCalledThenItShouldReturnInfoSecretSSL":
                    case "IfHereNowIsCalledThenItShouldReturnInfoCipher":
                    case "IfHereNowIsCalledThenItShouldReturnInfoCipherSSL":
                    case "IfHereNowIsCalledThenItShouldReturnInfoCipherSecret":
                    case "IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL":
                    case "IfHereNowIsCalledThenItShouldReturnInfoWithUserState":
                    case "IfGlobalHereNowIsCalledThenItShouldReturnInfo":
                    case "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState":
                        receivedHereNowMessage = true;
                        hereNowManualEvent.Set();
                        break;
                    default:
                        break;
                }
            }
        };

        public class UTPNSetStateResult : PNCallback<PNSetStateResult>
        {
            public override void OnResponse(PNSetStateResult result, PNStatus status)
            {
                Console.WriteLine("SetState Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("SetState PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                switch(currentTestCase)
                {
                    case "IfHereNowIsCalledThenItShouldReturnInfoWithUserState":
                    case "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState":
                        userStateManualEvent.Set();
                        break;
                    case "IfSetAndGetUserStateThenItShouldReturnInfo":
                    case "IfSetAndDeleteUserStateThenItShouldReturnInfo":
                        receivedUserStateMessage = true;
                        userStateManualEvent.Set();
                        break;
                    default:
                        break;
                }
            }
        };

        public class UTPNGetStateResult : PNCallback<PNGetStateResult>
        {
            public override void OnResponse(PNGetStateResult result, PNStatus status)
            {
                Console.WriteLine("GetState Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("GetState PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                switch (currentTestCase)
                {
                    case "IfSetAndGetUserStateThenItShouldReturnInfo":
                    case "IfSetAndDeleteUserStateThenItShouldReturnInfo":
                        receivedUserStateMessage = true;
                        userStateManualEvent.Set();
                        break;
                    default:
                        break;
                }
            }
        };


        public class UTWhereNowResult : PNCallback<PNWhereNowResult>
        {
            public override void OnResponse(PNWhereNowResult result, PNStatus status)
            {
                Console.WriteLine("WhereNow Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("WhereNow PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                switch (currentTestCase)
                {
                    case "IfWhereNowIsCalledThenItShouldReturnInfo":
                        receivedWhereNowMessage = true;
                        whereNowManualEvent.Set();
                        break;
                    default:
                        break;
                }
            }
        };
    }
}
