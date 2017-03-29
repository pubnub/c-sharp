using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToAChannel : TestHarness
    {
        private static ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent publishManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static object publishedMessage = null;
        private static long publishTimetoken = 0;
        private static bool receivedGrantMessage = false;

        private static int numberOfReceivedMessages = 0;

        int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string[] channelsGrant = { "hello_my_channel", "hello_my_channel1", "hello_my_channel2" };
        private static string authKey = "myAuth";
        private static string currentTestCase = "";

        private static Pubnub pubnub = null;

        private Server server;
        private UnitTestLog unitLog;

        [TestFixtureSetUp]
        public void Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
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
                Secure = true
            };
            server.RunOnHttps(true);

            pubnub = this.createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", "hello_my_channel%2Chello_my_channel1%2Chello_my_channel2")
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "hc7IKhEB7tyL6ENR3ndOOlHqPIG3RmzxwJMSGpofE6Q=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Grant().Channels(channelsGrant).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedGrantMessage, "WhenSubscribedToAChannel Grant access failed.");
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

        [Test]
        public void ThenComplexMessageSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenComplexMessageSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSubscribeShouldReturnReceivedMessage Failed");
        }

        private void CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = secretKey,
                CipherKey = cipherKey,
                Uuid = "mytestuuid",
                AuthKey = authKey,
                Secure = ssl
            };
            server.RunOnHttps(ssl);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("auth", authKey)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("signature", "zJpO1HpSZxGkOr3EALbOk-vQgjIZTZ6AU5svzNU9l_A=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("auth", authKey)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            publishManualEvent = new ManualResetEvent(false);
            publishedMessage = new CustomClass();

            expected = "[1,\"Sent\",\"14836234233392078\"]";

            server.AddRequest(new Request()
                .WithMethod("GET")
                .WithPath(string.Format("/publish/demo-36/{0}/0/{1}/0/{2}", PubnubCommon.SubscribeKey, channel, "%7B%22foo%22%3A%22hi%21%22%2C%22bar%22%3A%5B1%2C2%2C3%2C4%2C5%5D%7D"))
                .WithResponse(expected)
                .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                .WithMethod("GET")
                .WithPath(string.Format("/publish/demo-36/{0}/0/{1}/0/{2}", PubnubCommon.SubscribeKey, channel, "%22Zbr7pEF%2FGFGKj1rOstp0tWzA4nwJXEfj%2BezLtAr8qqE%3D%22"))
                .WithResponse(expected)
                .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            pubnub.Publish().Channel(channel).Message(publishedMessage).Async(new UTPublishResult());

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }

        [Test]
        public void ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            currentTestCase = "ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenSubscribeShouldReturnConnectStatus";

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
            manualResetEventWaitTimeout = 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status


            Thread.Sleep(1000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [Test]
        public void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenMultiSubscribeShouldReturnConnectStatus";

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

            manualResetEventWaitTimeout = 310 * 1000;

            string channel1 = "hello_my_channel1";
            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new string[] { channel1 }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            string channel2 = "hello_my_channel2";
            if (receivedMessage)
            {
                receivedMessage = false;

                subscribeManualEvent = new ManualResetEvent(false);

                expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1 + "," + channel2))
                        .WithParameter("heartbeat", "300")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("tt", "0")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                expected = "{}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1 + "," + channel2))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);
                pubnub.Subscribe<string>().Channels(new string[] { channel2 }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status
            }

            Thread.Sleep(1000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel1, channel2 }).Execute();
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [Test]
        public void ThenMultiSubscribeShouldReturnConnectStatusSSL()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenMultiSubscribeShouldReturnConnectStatusSSL";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = true
            };
            server.RunOnHttps(true);

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = 310 * 1000;

            string channel1 = "hello_my_channel1";
            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new string[] { channel1 }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            string channel2 = "hello_my_channel2";
            if (receivedMessage)
            {
                receivedMessage = false;
                subscribeManualEvent = new ManualResetEvent(false);

                expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1 + "," + channel2))
                        .WithParameter("heartbeat", "300")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("tt", "0")
                        .WithParameter("uuid", config.Uuid)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                expected = "{}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel1 + "," + channel2))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);

                pubnub.Subscribe<string>().Channels(new string[] { channel2 }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status
            }

            Thread.Sleep(1000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel1, channel2 }).Execute();
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenMultiSubscribeShouldReturnConnectStatusSSL Failed");
        }

        [Test]
        public void ThenSubscriberShouldBeAbleToReceiveManyMessages()
        {
            server.ClearRequests();

            receivedMessage = false;
            currentTestCase = "ThenSubscriberShouldBeAbleToReceiveManyMessages";

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

            manualResetEventWaitTimeout = 310 * 1000;

            string channel = "hello_my_channel";
            numberOfReceivedMessages = 0;

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status


            if (receivedMessage && !PubnubCommon.EnableStubTest)
            {
                subscribeManualEvent = new ManualResetEvent(false); //for messages

                for (int index = 0; index < 10; index++)
                {
                    Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Publishing " + index.ToString());
                    publishManualEvent = new ManualResetEvent(false);
                    pubnub.Publish().Channel(channel).Message(index.ToString()).Async(new UTPublishResult());
                    publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                }

            }

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscriberShouldBeAbleToReceiveManyMessages Failed");
        }

        //void ThenSubscribeInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
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
        //        mreGrant.Set();
        //    }
        //}

        //private void SubscriberDummyMethodForManyMessagesUserCallback(PNMessageResult<string> result)
        //{
        //    //Console.WriteLine("WhenSubscribedToChannel -> \n ThenSubscriberShouldBeAbleToReceiveManyMessages -> \n SubscriberDummyMethodForManyMessagesUserCallback -> result = " + result);
        //    numberOfReceivedMessages = numberOfReceivedMessages + 1;
        //    if (numberOfReceivedMessages >= 10)
        //    {
        //        receivedManyMessages = true;
        //        mreSubscriberManyMessages.Set();
        //    }

        //}

        //private void SubscribeDummyMethodForManyMessagesConnectCallback(ConnectOrDisconnectAck result)
        //{
        //    //Console.WriteLine("ThenSubscriberShouldBeAbleToReceiveManyMessages..Subscribe Connected");
        //    mreSubscribe.Set();
        //}

        //private void SubscribeDummyMethodForManyMessagesDisconnectCallback(ConnectOrDisconnectAck result)
        //{
        //}

        //private void ReceivedChannelUserCallback(PNMessageResult<string> result)
        //{
        //}

        //private void ReceivedChannel1ConnectCallback(ConnectOrDisconnectAck result)
        //{
        //    if (result != null)
        //    {
        //        long statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "connected")
        //        {
        //            receivedChannel1ConnectMessage = true;
        //        }
        //    }
        //    mreChannel1SubscribeConnect.Set();
        //}

        //private void ReceivedChannel2ConnectCallback(ConnectOrDisconnectAck result)
        //{
        //    if (result != null)
        //    {
        //        long statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "connected")
        //        {
        //            receivedChannel2ConnectMessage = true;
        //        }
        //    }
        //    mreChannel2SubscribeConnect.Set();
        //}

        //private void DummyMethodDuplicateChannelUserCallback1(PNMessageResult<string> result)
        //{
        //}

        //private void DummyMethodDuplicateChannelUserCallback2(PNMessageResult<string> result)
        //{
        //}

        //private void DummyMethodDuplicateChannelConnectCallback(ConnectOrDisconnectAck result)
        //{
        //}

        //private void DummyMethodDuplicateChannelDisconnectCallback(ConnectOrDisconnectAck result)
        //{
        //}

        //private void ReceivedMessageCallbackWhenSubscribed(PNMessageResult<string> result)
        //{
        //    if (result != null && result.Data != null)
        //    {
        //        string serializedPublishMesage = pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage);
        //        if (result.Data == serializedPublishMesage)
        //        {
        //            receivedMessage = true;
        //        }
        //    }
        //    mreSubscribe.Set();
        //}

        //private void ReceivedMessageCallbackYesConnect(PNMessageResult<string> result)
        //{
        //    //dummy method provided as part of subscribe connect status check.
        //}

        //private void ConnectStatusCallback(ConnectOrDisconnectAck result)
        //{
        //    if (result != null)
        //    {
        //        long statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "connected")
        //        {
        //            receivedConnectMessage = true;
        //        }
        //    }
        //    mreSubscribeConnect.Set();
        //}

        //private void dummyPublishCallback(PNPublishResult result)
        //{
        //    //Console.WriteLine("dummyPublishCallback -> result = " + result);
        //    if (result != null)
        //    {
        //        long statusCode = result.StatusCode;
        //        string statusMessage = result.StatusMessage;
        //        if (statusCode == 1 && statusMessage.ToLower() == "sent")
        //        {
        //            isPublished = true;
        //        }
        //    }

        //    mrePublish.Set();
        //}

        //private void DummyErrorCallback(PubnubClientError result)
        //{
        //    if (result != null)
        //    {
        //        Console.WriteLine("DummyErrorCallback result = " + result.Message);
        //    }
        //}

        //private void DuplicateChannelErrorCallback(PubnubClientError result)
        //{
        //    if (result != null && result.Message.ToLower().Contains("already subscribed"))
        //    {
        //        receivedAlreadySubscribedMessage = true;
        //    }
        //    mreAlreadySubscribed.Set();
        //}

        //private void dummyUnsubscribeCallback(string result)
        //{

        //}

        //void SubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //    mreSubscribeConnect.Set();
        //}

        //void UnsubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        //{
        //    mreUnsubscribe.Set();
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
                            foreach(KeyValuePair<string, Dictionary<string, PNAccessManagerKeyData>> channelKP in result.Channels)
                            {
                                string channel = channelKP.Key;
                                if (Array.IndexOf(channelsGrant,channel) > -1)
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (read && write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    else
                                    {
                                        receivedGrantMessage = false;
                                    }
                                }
                                else
                                {
                                    receivedGrantMessage = false;
                                    break;
                                }
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
                    switch(currentTestCase)
                    {
                        case "ThenComplexMessageSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage":
                            if (pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage) == message.Message.ToString())
                            {
                                receivedMessage = true;
                            }
                            subscribeManualEvent.Set();
                            break;
                        case "ThenSubscriberShouldBeAbleToReceiveManyMessages":
                            numberOfReceivedMessages++;
                            if (numberOfReceivedMessages >= 10)
                            {
                                receivedMessage = true;
                                subscribeManualEvent.Set();
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
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
                            //presenceManualEvent.Set();
                            break;
                        case "ThenComplexMessageSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenSubscribeShouldReturnConnectStatus":
                        case "ThenMultiSubscribeShouldReturnConnectStatus":
                        case "ThenMultiSubscribeShouldReturnConnectStatusSSL":
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
                    switch (currentTestCase)
                    {
                        case "ThenComplexMessageSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage":
                            subscribeManualEvent.Set();
                            break;
                        case "ThenSubscribeShouldReturnConnectStatus":
                        case "ThenMultiSubscribeShouldReturnConnectStatus":
                        case "ThenMultiSubscribeShouldReturnConnectStatusSSL":
                        case "ThenSubscriberShouldBeAbleToReceiveManyMessages":
                            receivedMessage = true;
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }


            }
        }

        public class UTPublishResult : PNCallback<PNPublishResult>
        {
            public override void OnResponse(PNPublishResult result, PNStatus status)
            {
                Console.WriteLine("Publish Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("Publish PNStatus => Status = : " + status.StatusCode.ToString());
                if (result != null && status.StatusCode == 200 && !status.Error)
                {
                    publishTimetoken = result.Timetoken;
                    switch (currentTestCase)
                    {
                        case "ThenComplexMessageSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage":
                        case "ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage":
                        case "ThenSubscriberShouldBeAbleToReceiveManyMessages":
                            receivedMessage = true;
                            publishManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
