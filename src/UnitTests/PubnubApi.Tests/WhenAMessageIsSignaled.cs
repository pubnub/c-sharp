using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;

// TODO: THIS TESTS ARE DEPENDING ON COMMON VARIABLE THAT MAKES FALSE POSITIVE RESULTS...

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAMessageIsSignaled : TestHarness
    {
        private const string messageForUnencryptSignal = "Pubnub Messaging API 1";

        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;
        private static Server server;
        private static string authKey = "myauth";

        [SetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();

            if (!PubnubCommon.PAMServerSideGrant) { return; }

            bool receivedGrantMessage = false;
            string channel = "hello_my_channel";

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

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
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "JMQKzXgfqNo-HaHuabC0gq0X6IkVMHa9AWBCg6BGN1Q=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent grantManualEvent = new ManualResetEvent(false);
            pubnub.Grant().Channels(new[] { channel }).AuthKeys(new[] { authKey }).Read(true).Write(true).Manage(true).TTL(20)
                .Execute(new PNAccessManagerGrantResultExt(
                                (r, s) =>
                                {
                                    try
                                    {
                                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                                        if (r != null)
                                        {
                                            Debug.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                            if (r.Channels != null && r.Channels.Count > 0)
                                            {
                                                var read = r.Channels[channel][authKey].ReadEnabled;
                                                var write = r.Channels[channel][authKey].WriteEnabled;
                                                if (read && write) { receivedGrantMessage = true; }
                                            }
                                        }
                                    }
                                    catch { /* ignore */  }
                                    finally
                                    {
                                        grantManualEvent.Set();
                                    }
                                }));

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenAMessageIsPublished Grant access failed.");
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenUnencryptSignalShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenUnencryptSignalShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedSignalMessage = false;

            long signalTimetoken = 0;

            string channel = "hello_my_channel";
            string message = messageForUnencryptSignal;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "test",
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715278266153304\"]";

            server.AddRequest(new Request()
                    .WithMethod("POST")
                    .WithPath(String.Format("/v1/signal/{0}/{1}/{2}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithContent("{\"message\":\"%22Pubnub%20Messaging%20API%201%22\"}")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

            ManualResetEvent signalManualEvent = new ManualResetEvent(false);
            pubnub.Signal().Channel(channel).Message(message)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            signalTimetoken = r.Timetoken;
                            receivedSignalMessage = true;
                        }
                        signalManualEvent.Set();
                    }));
            signalManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedSignalMessage)
            {
                Assert.IsTrue(receivedSignalMessage, "Unencrypt Signal Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncUnencryptSignalShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncUnencryptSignalShouldReturnSuccessCodeAndInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncUnencryptSignalShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedSignalMessage = false;

            string channel = "hello_my_channel";
            string message = messageForUnencryptSignal;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "test",
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);

            string expected = "[1,\"Sent\",\"14715278266153304\"]";

            server.AddRequest(new Request()
                    .WithMethod("POST")
                    .WithPath(String.Format("/v1/signal/{0}/{1}/{2}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithContent("{\"message\":\"%22Pubnub%20Messaging%20API%201%22\"}")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = 310 * 1000;

#if NET40
            PNResult<PNPublishResult> signalResult = Task.Factory.StartNew(async () => await pubnub.Signal().Channel(channel).Message(message).ExecuteAsync()).Result.Result;
#else
            PNResult<PNPublishResult> signalResult = await pubnub.Signal().Channel(channel).Message(message).ExecuteAsync();
#endif
            if (signalResult.Result != null && signalResult.Status.StatusCode == 200 && !signalResult.Status.Error
                && signalResult.Result.Timetoken > 0)
            {
                receivedSignalMessage = true;
            }

            if (!receivedSignalMessage)
            {
                Assert.IsTrue(receivedSignalMessage, "WithAsync Unencrypt Signal Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenUnencryptSignalListenerShouldGetMessagae()
        {
            server.ClearRequests();

            bool internalReceivedMessage = false;
            bool receivedErrorMessage = false;

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenUnencryptSignalListenerShouldGetMessagae");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(config.Secure);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                delegate (Pubnub o, PNSignalResult<object> m)
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine(string.Format("Signal SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message)));
                        if (pubnub.JsonPluggableLibrary.SerializeToJsonString(messageForUnencryptSignal) == m.Message.ToString())
                        {
                            internalReceivedMessage = true;
                        }
                        subscribeManualEvent.Set();
                    }
                },
                delegate (Pubnub o, PNStatus s)  
                {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        internalReceivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });

            pubnub = createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for connect
            Thread.Sleep(1000);
            if (!receivedErrorMessage)
            {
                subscribeManualEvent = new ManualResetEvent(false); //Reset to wait for message
                internalReceivedMessage = false;
                ManualResetEvent signalManualEvent = new ManualResetEvent(false);
                pubnub.Signal().Channel(channel).Message(messageForUnencryptSignal)
                        .Execute(new PNPublishResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                internalReceivedMessage = true;
                            }
                            signalManualEvent.Set();
                        }));

                signalManualEvent.WaitOne(manualResetEventWaitTimeout);

                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                Thread.Sleep(1000);

                pubnub.RemoveListener(listenerSubCallack);
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                if (receivedErrorMessage)
                {
                    internalReceivedMessage = false;
                }
                Assert.IsTrue(internalReceivedMessage, "WhenSubscribedToAChannel --> ThenUnencryptSignalListenerShouldGetMessagae Failed");
            }
        }

        [Test]
#if NET40
        public static void ThenWithAsyncUnencryptSignalListenerShouldGetMessagae()
#else
        public static async Task ThenWithAsyncUnencryptSignalListenerShouldGetMessagae()
#endif
        {
            server.ClearRequests();

            bool internalReceivedMessage = false;
            bool receivedErrorMessage = false;

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncUnencryptSignalListenerShouldGetMessagae");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(config.Secure);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                delegate (Pubnub o, PNSignalResult<object> m)
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine(string.Format("Signal SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message)));
                        if (pubnub.JsonPluggableLibrary.SerializeToJsonString(messageForUnencryptSignal) == m.Message.ToString())
                        {
                            internalReceivedMessage = true;
                        }
                        subscribeManualEvent.Set();
                    }
                },
                delegate (Pubnub o, PNStatus s)  
                {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        internalReceivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });

            pubnub = createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for connect
            Thread.Sleep(1000);
            if (!receivedErrorMessage)
            {
                subscribeManualEvent = new ManualResetEvent(false); //Reset to wait for message
                internalReceivedMessage = false;
#if NET40
                PNResult<PNPublishResult> signalResult = Task.Factory.StartNew(async () => await pubnub.Signal().Channel(channel).Message(messageForUnencryptSignal).ExecuteAsync()).Result.Result;
#else
                PNResult<PNPublishResult> signalResult = await pubnub.Signal().Channel(channel).Message(messageForUnencryptSignal).ExecuteAsync();
#endif
                if (signalResult.Result != null && signalResult.Status.StatusCode == 200 && !signalResult.Status.Error)
                {
                    internalReceivedMessage = true;
                }

                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                Thread.Sleep(1000);

                pubnub.RemoveListener(listenerSubCallack);
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                if (receivedErrorMessage)
                {
                    internalReceivedMessage = false;
                }
                Assert.IsTrue(internalReceivedMessage, "WhenSubscribedToAChannel --> ThenWithAsyncUnencryptSignalListenerShouldGetMessagae Failed");
            }
        }

        [Test]
        public static void ThenIgnoreCipherKeyUnencryptSignalListenerShouldGetMessagae()
        {
            server.ClearRequests();

            bool internalReceivedMessage = false;
            bool receivedErrorMessage = false;

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenIgnoreCipherKeyUnencryptSignalListenerShouldGetMessagae");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "testcipherkey",
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(config.Secure);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                delegate(Pubnub o, PNSignalResult<object> m)
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine(string.Format("Signal SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message)));
                        if (pubnub.JsonPluggableLibrary.SerializeToJsonString(messageForUnencryptSignal) == m.Message.ToString())
                        {
                            internalReceivedMessage = true;
                        }
                        subscribeManualEvent.Set();
                    }
                },
                delegate(Pubnub o, PNStatus s) {
                    Debug.WriteLine(string.Format("~~~~~~~~~~~> {0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        internalReceivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });

            pubnub = createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for connect
            Thread.Sleep(1000);
            if (!receivedErrorMessage)
            {
                subscribeManualEvent = new ManualResetEvent(false); //Reset to wait for message
                internalReceivedMessage = false;
                ManualResetEvent signalManualEvent = new ManualResetEvent(false);
                pubnub.Signal().Channel(channel).Message(messageForUnencryptSignal)
                        .Execute(new PNPublishResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                internalReceivedMessage = true;
                            }
                            signalManualEvent.Set();
                        }));

                signalManualEvent.WaitOne(manualResetEventWaitTimeout);

                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                Thread.Sleep(1000);

                pubnub.RemoveListener(listenerSubCallack);
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                if (receivedErrorMessage)
                {
                    internalReceivedMessage = false;
                }
                Assert.IsTrue(internalReceivedMessage, "WhenSubscribedToAChannel --> ThenUnencryptSignalListenerShouldGetMessagae Failed");
            }
        }

        [Test]
#if NET40
        public static void ThenWithAsyncIgnoreCipherKeyUnencryptSignalListenerShouldGetMessagae()
#else
        public static async Task ThenWithAsyncIgnoreCipherKeyUnencryptSignalListenerShouldGetMessagae()
#endif
        {
            server.ClearRequests();

            bool internalReceivedMessage = false;
            bool receivedErrorMessage = false;

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncIgnoreCipherKeyUnencryptSignalListenerShouldGetMessagae");
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "testcipherkey",
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(config.Secure);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                delegate (Pubnub o, PNSignalResult<object> m)
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine(string.Format("Signal SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message)));
                        if (pubnub.JsonPluggableLibrary.SerializeToJsonString(messageForUnencryptSignal) == m.Message.ToString())
                        {
                            internalReceivedMessage = true;
                        }
                        subscribeManualEvent.Set();
                    }
                },
                delegate (Pubnub o, PNStatus s) {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        internalReceivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });

            pubnub = createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for connect
            Thread.Sleep(1000);
            if (!receivedErrorMessage)
            {
                subscribeManualEvent = new ManualResetEvent(false); //Reset to wait for message
                internalReceivedMessage = false;
#if NET40
                PNResult<PNPublishResult> signalResult = Task.Factory.StartNew(async () => await pubnub.Signal().Channel(channel).Message(messageForUnencryptSignal).ExecuteAsync()).Result.Result;
#else
                PNResult<PNPublishResult> signalResult = await pubnub.Signal().Channel(channel).Message(messageForUnencryptSignal).ExecuteAsync();
#endif
                if (signalResult.Result != null && signalResult.Status.StatusCode == 200 && !signalResult.Status.Error)
                {
                    internalReceivedMessage = true;
                }

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                Thread.Sleep(1000);

                pubnub.RemoveListener(listenerSubCallack);
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                if (receivedErrorMessage)
                {
                    internalReceivedMessage = false;
                }
                Assert.IsTrue(internalReceivedMessage, "WhenSubscribedToAChannel --> ThenWithAsyncIgnoreCipherKeyUnencryptSignalListenerShouldGetMessagae Failed");
            }
        }
    }
}
