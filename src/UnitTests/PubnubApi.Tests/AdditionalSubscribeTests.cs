using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using System.Net;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class AdditionalSubscribeTests : TestHarness
    {
        private static readonly int manualResetEventWaitTimeout = 310 * 1000;
        private static readonly string authKey = "myauth";
        private static readonly string channel = "hello_my_channel";
        private static string authToken;

        private static Pubnub pubnub;
        private static Server server;

        private class CustomTestObject
        {
            public string Field1 { get; set; }
            public int Field2 { get; set; }
            public List<string> Field3 { get; set; }
        }

        public class TestLog : IPubnubLog
        {
            public void WriteToLog(string logText)
            {
                Debug.WriteLine(logText);
            }
        }

        [SetUp]
        public static async Task Init()
        {
            UnitTestLog unitLog = new UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            if (PubnubCommon.EnableStubTest)
            {
                server.Start();
            }

            if (!PubnubCommon.PAMServerSideGrant) { return; }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Secure = true
            };
            server.RunOnHttps(true);

            pubnub = createPubNubInstance(config);

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
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("w", "1")
                    .WithParameter("signature", "hc7IKhEB7tyL6ENR3ndOOlHqPIG3RmzxwJMSGpofE6Q=")
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK));

            if (string.IsNullOrEmpty(PubnubCommon.GrantToken))
            {
                await GenerateTestGrantToken(pubnub);
            }
            authToken = PubnubCommon.GrantToken;
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [TearDown]
        public static void Exit()
        {
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }
            server.Stop();
        }

        [Test]
        public static void ThenSubscribeWithEmptyChannelsShouldReturnException()
        {
            server.ClearRequests();

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            Assert.Throws<ArgumentException>(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new string[] { })
                    .Execute();
            });
        }

        [Test]
        public static void ThenSubscribeShouldHandleServerErrors()
        {
            server.ClearRequests();
            
            bool receivedErrorMessage = false;
            ManualResetEvent errorManualEvent = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog()
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { },
                (o, p) => { },
                (o, s) => {
                    Debug.WriteLine($"{s.Operation} {s.Category} {s.StatusCode}");
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        errorManualEvent.Set();
                    }
                });
            
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string expected = "{\"error\": \"Forbidden\", \"status\": 403}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.Forbidden));

            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            
            errorManualEvent.WaitOne(manualResetEventWaitTimeout);
            
            Assert.IsTrue(receivedErrorMessage, "Subscribe should handle server errors properly");
            
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSubscribeWithCustomObjectShouldReceiveCorrectType()
        {
            server.ClearRequests();
            
            bool receivedCorrectType = false;
            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            ManualResetEvent messageManualEvent = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            server.RunOnHttps(false);

            CustomTestObject testObject = new CustomTestObject
            {
                Field1 = "Test",
                Field2 = 42,
                Field3 = new List<string> { "item1", "item2", "item3" }
            };

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => {
                    Debug.WriteLine("Message received: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message));
                    if (m.Message is CustomTestObject)
                    {
                        CustomTestObject received = m.Message as CustomTestObject;
                        if (received != null && received.Field1 == testObject.Field1 && 
                            received.Field2 == testObject.Field2 && 
                            received.Field3.Count == testObject.Field3.Count)
                        {
                            receivedCorrectType = true;
                        }
                    }
                    messageManualEvent.Set();
                },
                (o, p) => { },
                (o, s) => {
                    if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeManualEvent.Set();
                    }
                });
            
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK));

            // Add a second subscribe response that includes a message
            string messageJson = "{\"Field1\":\"Test\",\"Field2\":42,\"Field3\":[\"item1\",\"item2\",\"item3\"]}";
            expected = "{\"t\":{\"t\":\"14836303477713305\",\"r\":7},\"m\":[{\"a\":\"1\",\"b\":\"" + channel + "\",\"c\":\"" + channel + "\",\"d\":" + messageJson + ",\"e\":0,\"f\":\"0\",\"i\":\"Client-12345\",\"k\":\"demo-36\",\"s\":1,\"u\":{}}]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "14836303477713304")
                    .WithParameter("tr", "7")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK));

            pubnub.Subscribe<CustomTestObject>().Channels(new[] { channel }).Execute();
            
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
            messageManualEvent.WaitOne(manualResetEventWaitTimeout);
            
            Assert.IsTrue(receivedCorrectType, "Subscribe should correctly deserialize to custom object type");
            
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenDisconnectAndReconnectShouldMaintainSubscription()
        {
            server.ClearRequests();
            
            bool connectReceived = false;
            bool reconnectReceived = false;
            ManualResetEvent connectEvent = new ManualResetEvent(false);
            ManualResetEvent reconnectEvent = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { },
                (o, p) => { },
                (o, s) => {
                    Debug.WriteLine($"{s.Operation} {s.Category} {s.StatusCode}");
                    if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        connectReceived = true;
                        connectEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNReconnectedCategory)
                    {
                        reconnectReceived = true;
                        reconnectEvent.Set();
                    }
                });
            
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK));

            // Add second request for reconnect
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "14836303477713304")
                    .WithParameter("tr", "7")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            
            connectEvent.WaitOne(manualResetEventWaitTimeout);
            
            // Disconnect
            pubnub.Disconnect<string>();
            
            Thread.Sleep(2000);
            
            // Reconnect
            pubnub.Reconnect<string>();

            reconnectEvent.WaitOne(manualResetEventWaitTimeout);
            
            Assert.IsTrue(connectReceived && reconnectReceived, "Disconnect and Reconnect should maintain subscription");
            
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSubscribeWithQueryParamsShouldIncludeInRequest()
        {
            server.ClearRequests();
            
            bool requestContainsQueryParams = false;
            Dictionary<string, object> queryParams = new Dictionary<string, object>()
            {
                { "custom_param", "custom_value" },
                { "numeric_param", 123 }
            };

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog()
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

            // Set up a request that checks for our custom query parameters
            var request = new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("custom_param", "custom_value")
                    .WithParameter("numeric_param", "123")
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK);

            request.Callback = () => { requestContainsQueryParams = true; };
            server.AddRequest(request);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { },
                (o, p) => { },
                (o, s) => {
                    if (s.StatusCode == 200)
                    {
                        subscribeManualEvent.Set();
                    }
                });
            
            pubnub.AddListener(listenerSubCallack);
            pubnub.Subscribe<string>().Channels(new[] { channel }).QueryParam(queryParams).Execute();
            
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
            
            Assert.IsTrue(requestContainsQueryParams, "Query parameters should be included in the request");
            
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenUnsubscribeFromNonSubscribedChannelShouldNotFail()
        {
            server.ClearRequests();

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            // No exception should be thrown
            pubnub.Unsubscribe<string>().Channels(new[] { "non_existent_channel" }).Execute();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSubscribeWithEventEngineEnabledShouldUseEventEngine()
        {
            server.ClearRequests();
            
            bool eventEngineUsed = false;
            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                EnableEventEngine = true // Explicitly enable event engine
            };
            server.RunOnHttps(false);

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { },
                (o, p) => { },
                (o, s) => {
                    Debug.WriteLine($"{s.Operation} {s.Category} {s.StatusCode}");
                    if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        // Verify that the event engine is used by checking that the status event has
                        // proper data from event engine
                        eventEngineUsed = s.AffectedChannels != null && s.AffectedChannels.Contains(channel);
                        subscribeManualEvent.Set();
                    }
                });
            
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
            
            Assert.IsTrue(eventEngineUsed, "Event Engine should be used when enabled");
            
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSubscribeWithNoChannelsButChannelGroupsShouldWork()
        {
            server.ClearRequests();
            
            bool connectReceived = false;
            ManualResetEvent connectEvent = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            server.RunOnHttps(false);

            string channelGroup = "my_channel_group";

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { },
                (o, p) => { },
                (o, s) => {
                    Debug.WriteLine($"{s.Operation} {s.Category} {s.StatusCode}");
                    if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        connectReceived = true;
                        connectEvent.Set();
                    }
                });
            
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/,/0", PubnubCommon.SubscribeKey))
                    .WithParameter("channel-group", channelGroup)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK));

            pubnub.Subscribe<string>().ChannelGroups(new[] { channelGroup }).Execute();
            
            connectEvent.WaitOne(manualResetEventWaitTimeout);
            
            Assert.IsTrue(connectReceived, "Subscribe with only channel groups should work");
            
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenResetTimeTokenShouldStartSubscriptionFromBeginning()
        {
            server.ClearRequests();
            
            bool timeTokenReset = false;
            ManualResetEvent connectEvent = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            // First normal connection with timetoken
            string expected = "{\"t\":{\"t\":\"14836303477713304\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK));

            // Second request should reset timetoken back to 0 if reset flag is true
            var secondRequest = new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0") // This verifies timetoken reset
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(HttpStatusCode.OK);

            secondRequest.Callback = () => { timeTokenReset = true; };
            server.AddRequest(secondRequest);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { },
                (o, p) => { },
                (o, s) => { 
                    if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeManualEvent.Set();
                    }
                });
            
            pubnub.AddListener(listenerSubCallack);
            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
            
            // Disconnect and reconnect with reset flag
            pubnub.Disconnect<string>();
            Thread.Sleep(1000);
            pubnub.Reconnect<string>(true); // true = reset timetoken
            
            Thread.Sleep(3000); // Give time for the second request to be processed
            
            Assert.IsTrue(timeTokenReset, "Reconnect with reset timetoken should reset the timetoken to 0");
            
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
    }
} 