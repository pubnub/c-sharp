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
        //ManualResetEvent mreUnsubscribe = new ManualResetEvent(false);
        //ManualResetEvent mreAlreadySubscribed = new ManualResetEvent(false);
        //ManualResetEvent mreChannel1SubscribeConnect = new ManualResetEvent(false);
        //ManualResetEvent mreChannel2SubscribeConnect = new ManualResetEvent(false);
        //ManualResetEvent mreSubscriberManyMessages = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        //ManualResetEvent mreSubscribe = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static object publishedMessage = null;
        private static long publishTimetoken = 0;
        //bool receivedConnectMessage = false;
        //bool receivedAlreadySubscribedMessage = false;
        //bool receivedChannel1ConnectMessage = false;
        //bool receivedChannel2ConnectMessage = false;
        //bool receivedManyMessages = false;
        private static bool receivedGrantMessage = false;
        //private static bool receivedPublishMessage = false;

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

            pubnub = this.createPubNubInstance(config);

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
                CiperKey = cipherKey,
                Uuid = "mytestuuid",
                Secure = ssl
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            publishManualEvent = new ManualResetEvent(false);
            publishedMessage = new CustomClass();
            pubnub.Publish().Channel(channel).Message(publishedMessage).Async(new UTPublishResult());

            subscribeManualEvent = new ManualResetEvent(false);
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for message

            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedMessage)
            {

                Thread.Sleep(1000);

                pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
                Thread.Sleep(2000);
            }

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }

        [Test]
        public void ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage()
        {
            currentTestCase = "ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage()
        {
            currentTestCase = "ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage()
        {
            currentTestCase = "ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageSecretSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage()
        {
            currentTestCase = "ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage()
        {
            currentTestCase = "ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage()
        {
            currentTestCase = "ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage()
        {
            currentTestCase = "ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage";
            CommonComplexMessageSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenComplexMessageCipherSecretSSLSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnConnectStatus()
        {
            receivedMessage = false;
            currentTestCase = "ThenSubscribeShouldReturnConnectStatus";

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
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            Thread.Sleep(2000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [Test]
        public void ThenMultiSubscribeShouldReturnConnectStatus()
        {
            receivedMessage = false;
            currentTestCase = "ThenMultiSubscribeShouldReturnConnectStatus";

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

            string channel1 = "hello_my_channel1";
            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel1 }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            if (receivedMessage)
            {
                receivedMessage = false;

                string channel2 = "hello_my_channel2";
                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Subscribe<string>().Channels(new string[] { channel2 }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status
            }

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenSubscribeShouldReturnConnectStatus Failed");
        }

        [Test]
        public void ThenMultiSubscribeShouldReturnConnectStatusSSL()
        {
            receivedMessage = false;
            currentTestCase = "ThenMultiSubscribeShouldReturnConnectStatusSSL";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = true
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string channel1 = "hello_my_channel1";
            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel1 }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            if (receivedMessage)
            {
                receivedMessage = false;

                string channel2 = "hello_my_channel2";
                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Subscribe<string>().Channels(new string[] { channel2 }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status
            }

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToAChannel --> ThenMultiSubscribeShouldReturnConnectStatusSSL Failed");
        }

        [Test]
        public void ThenSubscriberShouldBeAbleToReceiveManyMessages()
        {
            receivedMessage = false;
            currentTestCase = "ThenSubscriberShouldBeAbleToReceiveManyMessages";

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
            numberOfReceivedMessages = 0;

            subscribeManualEvent = new ManualResetEvent(false);
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

                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

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
