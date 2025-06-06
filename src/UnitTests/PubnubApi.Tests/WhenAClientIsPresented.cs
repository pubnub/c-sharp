﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubnubApi;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAClientIsPresented : TestHarness
    {
        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;
        private static Server server;
        private static string authToken;
        private static string presenceTestChannel = $"presenceTest{new Random().Next(100,1000)}";

        public class TestLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine(logText);
            }
        }

        [SetUp]
        public static async Task Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            if (PubnubCommon.EnableStubTest)
            {
                server.Start();   
            }

            if (!PubnubCommon.PAMServerSideGrant) { return; }
            
            string channel = "hello_my_channel";
            string channel1 = "hello_my_channel_1";
            string channel2 = "hello_my_channel_2";
            string channel3 = "hello_my_channel_3";
            string channel4 = "hello_my_channel_4";

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false,
                EnableEventEngine = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (string.IsNullOrEmpty(PubnubCommon.GrantToken))
            {
                await GenerateTestGrantToken(pubnub, presenceTestChannel);
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

#if (USE_JSONFX)
        [Test]
#else
        [Ignore("Ignore this for non-JsonFX")]
#endif
        public void UsingJsonFx()
        {
            Debug.Write("UsingJsonFx");
            Assert.True(true, "UsingJsonFx");
        }

#if (USE_JSONFX)
        [Ignore]
#else
        [Test]
#endif
        public void UsingNewtonSoft()
        {
            Debug.Write("UsingNewtonSoft");
            Assert.True(true, "UsingNewtonSoft");
        }

        [Test]
        public static void ThenPresenceShouldReturnReceivedMessage()
        {
            server.ClearRequests();

            bool receivedPresenceMessage = false;

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
            
            server.RunOnHttps(false);

            ManualResetEvent presenceManualEvent = new ManualResetEvent(false);

            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(p));
                    if (p.Event == "join") { receivedPresenceMessage = true; }
                    presenceManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        presenceManualEvent.Set();
                    }
                });

            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel_3";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14833694874957031\",\"r\":7},\"m\":[{\"a\":\"4\",\"f\":512,\"p\":{\"t\":\"14833694873794045\",\"r\":2},\"k\":\"demo-36\",\"c\":\"hello_my_channel-pnpres\",\"d\":{\"action\": \"join\", \"timestamp\": 1483369487, \"uuid\": \"mylocalmachine.mydomain.com\", \"occupancy\": 1},\"b\":\"hello_my_channel-pnpres\"}]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1},{2}/0", PubnubCommon.SubscribeKey, channel, channel + "-pnpres"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "_JVs4gooSMhdgxRO6FNkk6HwlkyxqcRATHU5j3vkJ9s=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).WithPresence().Execute();
            presenceManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
            else Thread.Sleep(100);

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) { Thread.Sleep(1000); }
            else { Thread.Sleep(100); }

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "Presence message not received");
        }

        [Test]
        public static void ThenPresenceShouldReturnReceivedMessageSSL()
        {
            server.ClearRequests();

            bool receivedPresenceMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = true,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog()
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            server.RunOnHttps(true);

            ManualResetEvent presenceManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(p));
                    if (p.Event == "join") { receivedPresenceMessage = true; }
                    presenceManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        presenceManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }


            string channel = "hello_my_channel_4";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14833694874957031\",\"r\":7},\"m\":[{\"a\":\"4\",\"f\":512,\"p\":{\"t\":\"14833694873794045\",\"r\":2},\"k\":\"demo-36\",\"c\":\"hello_my_channel-pnpres\",\"d\":{\"action\": \"join\", \"timestamp\": 1483369487, \"uuid\": \"mylocalmachine.mydomain.com\", \"occupancy\": 1},\"b\":\"hello_my_channel-pnpres\"}]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1},{2}/0", PubnubCommon.SubscribeKey, channel, channel + "-pnpres"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "_JVs4gooSMhdgxRO6FNkk6HwlkyxqcRATHU5j3vkJ9s=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).WithPresence().Execute();
            presenceManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) { Thread.Sleep(4000); }
            else { Thread.Sleep(100); }

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            Thread.Sleep(4000);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "Presence message not received");
        }

        [Test]
        public static void ThenPresenceShouldReturnCustomUserId()
        {
            server.ClearRequests();
            
            bool receivedCustomUUID = false;

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

            server.RunOnHttps(false);

            ManualResetEvent presenceManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(p));
                    receivedCustomUUID = true;
                    presenceManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        presenceManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel_2";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14833694874957031\",\"r\":7},\"m\":[{\"a\":\"4\",\"f\":512,\"p\":{\"t\":\"14833694873794045\",\"r\":2},\"k\":\"demo-36\",\"c\":\"hello_my_channel-pnpres\",\"d\":{\"action\": \"join\", \"timestamp\": 1483369487, \"uuid\": \"mylocalmachine.mydomain.com\", \"occupancy\": 1},\"b\":\"hello_my_channel-pnpres\"}]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1},{2}/0", PubnubCommon.SubscribeKey, channel, channel + "-pnpres"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    //.WithParameter("uuid", customUserId.ToString())
                    .WithParameter("signature", "D7lw9Np5UU_xUTUAe0Sc0L0eSP9aTQljeith_M_rXzI=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).WithPresence().Execute();
            presenceManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) { Thread.Sleep(1000); }
            else { Thread.Sleep(100); }

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) { Thread.Sleep(1000); }
            else { Thread.Sleep(100); }

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedCustomUUID, "Custom UUID not received");
        }

        [Test]
        public static void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;
            bool receivedErrorMessage = false;

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
            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedErrorMessage)
            {
                if (!PubnubCommon.EnableStubTest) { Thread.Sleep(2000); }
                else Thread.Sleep(200);

                expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
                pubnub.HereNow().Channels(new[] { channel }).Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
                hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

                expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
                else Thread.Sleep(100);
            }


            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received");
        }

        [Test]
#if NET40
        public static void IfWithAsyncHereNowIsCalledThenItShouldReturnInfo()
#else
        public static async Task IfWithAsyncHereNowIsCalledThenItShouldReturnInfo()
#endif
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;
            bool receivedErrorMessage = false;

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
            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedErrorMessage)
            {
                if (!PubnubCommon.EnableStubTest) { Thread.Sleep(2000); }
                else Thread.Sleep(200);

                expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [\"mytestuuid\"], \"occupancy\": 1}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
                PNResult<PNHereNowResult> r = Task.Factory.StartNew(async () => await pubnub.HereNow().Channels(new[] { channel }).ExecuteAsync()).Result.Result;
#else
                PNResult<PNHereNowResult> r = await pubnub.HereNow().Channels(new[] { channel }).ExecuteAsync();
#endif
                if (r.Result != null)
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                    receivedHereNowMessage = true;
                }

                expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
                else Thread.Sleep(100);
            }


            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received");
        }


        [Test]
        public static void IfHereNowIsCalledThenItShouldReturnInfoCipher()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;
            bool receivedErrorMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            server.RunOnHttps(false);


            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedErrorMessage)
            {
                if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
                else Thread.Sleep(200);

                expected = "{\"TotalChannels\":1,\"TotalOccupancy\":1,\"Channels\":{\"hello_my_channel\":{\"ChannelName\":\"hello_my_channel\",\"Occupancy\":1,\"Occupants\":[{\"Uuid\":\"mytestuuid\",\"State\":null}]}}}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
                pubnub.HereNow().Channels(new[] { channel }).Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
                hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

                expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
                else Thread.Sleep(100);
            }

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher");
        }

        [Test]
        public static void IfHereNowIsCalledThenItShouldReturnInfoCipherSecret()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            server.RunOnHttps(true);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "o3VANfuhvrxfff1jsBMOc6EQ4LCe8LXHGaDh58QBZFA=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            
            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
            else Thread.Sleep(200);

            expected = "{\"TotalChannels\":1,\"TotalOccupancy\":1,\"Channels\":{\"hello_my_channel\":{\"ChannelName\":\"hello_my_channel\",\"Occupancy\":1,\"Occupants\":[{\"Uuid\":\"mytestuuid\",\"State\":null}]}}}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new [] { channel }).Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
            else Thread.Sleep(100);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher and secret");
        }

        [Test]
        public static void IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = true
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            server.RunOnHttps(true);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "o3VANfuhvrxfff1jsBMOc6EQ4LCe8LXHGaDh58QBZFA=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            
            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
            else Thread.Sleep(200);

            expected = "{\"TotalChannels\":1,\"TotalOccupancy\":1,\"Channels\":{\"hello_my_channel\":{\"ChannelName\":\"hello_my_channel\",\"Occupancy\":1,\"Occupants\":[{\"Uuid\":\"mytestuuid\",\"State\":null}]}}}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new [] { channel }).Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
            else Thread.Sleep(100);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher, secret, ssl");
        }

        [Test]
        public static void IfHereNowIsCalledThenItShouldReturnInfoCipherSSL()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;
            bool receivedErrorMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = true
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            server.RunOnHttps(true);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                            (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                            (o, p) => {
                                subscribeManualEvent.Set();
                            },
                            (o, s) => {
                                Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                                if (s.StatusCode != 200 || s.Error)
                                {
                                    receivedErrorMessage = true;
                                    if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                                }
                                subscribeManualEvent.Set();
                            });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedErrorMessage)
            {
                if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
                else Thread.Sleep(200);

                expected = "{\"TotalChannels\":1,\"TotalOccupancy\":1,\"Channels\":{\"hello_my_channel\":{\"ChannelName\":\"hello_my_channel\",\"Occupancy\":1,\"Occupants\":[{\"Uuid\":\"mytestuuid\",\"State\":null}]}}}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
                pubnub.HereNow().Channels(new[] { channel }).Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
                hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

                expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
                else Thread.Sleep(100);
            }

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with cipher, ssl");
        }

        [Test]
        public static void IfHereNowIsCalledThenItShouldReturnInfoSecret()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;

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

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "o3VANfuhvrxfff1jsBMOc6EQ4LCe8LXHGaDh58QBZFA=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            
            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
            else Thread.Sleep(200);

            expected = "{\"TotalChannels\":1,\"TotalOccupancy\":1,\"Channels\":{\"hello_my_channel\":{\"ChannelName\":\"hello_my_channel\",\"Occupancy\":1,\"Occupants\":[{\"Uuid\":\"mytestuuid\",\"State\":null}]}}}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new [] { channel }).Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
            else Thread.Sleep(100);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with secret key");
        }

        [Test]
        public static void IfHereNowIsCalledThenItShouldReturnInfoSecretSSL()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = true
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            server.RunOnHttps(true);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "o3VANfuhvrxfff1jsBMOc6EQ4LCe8LXHGaDh58QBZFA=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            
            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
            else Thread.Sleep(200);

            expected = "{\"TotalChannels\":1,\"TotalOccupancy\":1,\"Channels\":{\"hello_my_channel\":{\"ChannelName\":\"hello_my_channel\",\"Occupancy\":1,\"Occupants\":[{\"Uuid\":\"mytestuuid\",\"State\":null}]}}}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Channels(new [] { channel }).Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
            else Thread.Sleep(100);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received ,with secret key, ssl");
        }

        [Test]
        public static void IfHereNowIsCalledThenItShouldReturnInfoSSL()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;
            bool receivedErrorMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = true
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            server.RunOnHttps(true);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            
            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedErrorMessage)
            {
                if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
                else Thread.Sleep(200);

                expected = "{\"TotalChannels\":1,\"TotalOccupancy\":1,\"Channels\":{\"hello_my_channel\":{\"ChannelName\":\"hello_my_channel\",\"Occupancy\":1,\"Occupants\":[{\"Uuid\":\"mytestuuid\",\"State\":null}]}}}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
                pubnub.HereNow().Channels(new[] { channel }).Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
                hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

                expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
                else Thread.Sleep(100);
            }

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with ssl");
        }

        [Test]
        public static void IfHereNowIsCalledThenItShouldReturnInfoWithUserState()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;
            bool receivedErrorMessage = false;

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

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828455563482572\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            
            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedErrorMessage)
            {
                if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
                else Thread.Sleep(200);

                
                Dictionary<string, object> dicState = new Dictionary<string, object>();
                dicState.Add("testkey", "testval");

                expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"testkey\": \"testval\"}, \"service\": \"Presence\"}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/uuid/{2}/data", PubnubCommon.SubscribeKey, channel, config.UserId))
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("state", "%7B%22testkey%22%3A%22testval%22%7D")
                        .WithParameter("timestamp", "1356998400")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent userStateManualEvent = new ManualResetEvent(false);
                pubnub.SetPresenceState()
                                .Channels(new[] { channel })
                                .State(dicState)
                                .Execute(new PNSetStateResultExt(
                                (r, s) => {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    userStateManualEvent.Set();
                                }));
                userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

                expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"Presence\", \"uuids\": [{\"state\": {\"testkey\": \"testval\"}, \"uuid\": \"mytestuuid\"}], \"occupancy\": 1}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("disable_uuids", "0")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("state", "1")
                        .WithParameter("timestamp", "1356998400")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
                pubnub.HereNow().Channels(new[] { channel })
                        .IncludeState(true)
                        .IncludeUUIDs(true)
                        .Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
                hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

                expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
                else Thread.Sleep(100);
            }

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "here_now message not received with user state");
        }

        //TODO: CLEN-2044
        //[Test]
        public static void IfGlobalHereNowIsCalledThenItShouldReturnInfo()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;

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
            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14827658395446362\",\"r\":7},\"m\":[]}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "o3VANfuhvrxfff1jsBMOc6EQ4LCe8LXHGaDh58QBZFA=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            
            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
            else Thread.Sleep(200);

            expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"channels\": {}, \"total_channels\": 0, \"total_occupancy\": 0}, \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("disable_uuids", "0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("state", "0")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow().Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));

            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "{\"status\": 200, \"action\": \"leave\", \"message\": \"OK\", \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
            else Thread.Sleep(100);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "global_here_now message not received");
        }

        //TODO: CLEN-2044
        //[Test]
        public static void IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState()
        {
            server.ClearRequests();

            bool receivedHereNowMessage = false;

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

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";

            string expected = "{\"t\":{\"t\":\"14827658395446362\",\"r\":7},\"m\":[]}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest ? 2000 : 310 * 1000);

            
            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            
            Dictionary<string, object> dicState = new Dictionary<string, object>();
            dicState.Add("testkey", "testval");

            expected = "[[],\"14740704540745015\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/uuid/{2}/data", PubnubCommon.SubscribeKey, channel, config.UserId))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("state", "%7B%22testkey%22%3A%22testval%22%7D")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent userStateManualEvent = new ManualResetEvent(false);
            pubnub.SetPresenceState()
                            .Channels(new [] { channel })
                            .State(dicState)
                            .Execute(new PNSetStateResultExt(
                                (r, s) => {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    userStateManualEvent.Set();
                                }));
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
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
            pubnub.HereNow()
                    .IncludeState(true)
                    .IncludeUUIDs(true)
                    .Execute(new PNHereNowResultEx(
                                (r, s) => {
                                    if (r == null) { return; }
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedHereNowMessage = true;
                                    hereNowManualEvent.Set();
                                }));
            hereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            expected = "[[],\"14740704540745015\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
            else Thread.Sleep(100);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedHereNowMessage, "global_here_now message not received for user state");
        }

        [Test]
        public static void IfWhereNowIsCalledThenItShouldReturnInfo()
        {
            server.ClearRequests();

            bool receivedWhereNowMessage = false;

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

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14827658395446362\",\"r\":7},\"m\":[]}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            
            pubnub.Subscribe<string>().Channels(new [] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
            else Thread.Sleep(200);

            expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"channels\": {}, \"total_channels\": 0, \"total_occupancy\": 0}, \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/uuid/{1}", PubnubCommon.SubscribeKey, config.UserId))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent whereNowManualEvent = new ManualResetEvent(false);
            pubnub.WhereNow().Uuid(config.UserId).Execute(new PNWhereNowResultExt(
                                (r, s) => {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedWhereNowMessage = true;
                                    whereNowManualEvent.Set();
                                }));
            whereNowManualEvent.WaitOne();

            expected = "[[],\"14740704540745015\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
            else Thread.Sleep(100);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedWhereNowMessage, "where_now message not received");
        }

        [Test]
#if NET40
        public static void IfWithAsyncWhereNowIsCalledThenItShouldReturnInfo()
#else
        public static async Task IfWithAsyncWhereNowIsCalledThenItShouldReturnInfo()
#endif
        {
            server.ClearRequests();

            bool receivedWhereNowMessage = false;

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

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                    }
                    subscribeManualEvent.Set();
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14827658395446362\",\"r\":7},\"m\":[]}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));


            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
            else Thread.Sleep(200);

            expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"channels\": {}, \"total_channels\": 0, \"total_occupancy\": 0}, \"service\": \"Presence\"}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/uuid/{1}", PubnubCommon.SubscribeKey, config.UserId))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNWhereNowResult> r = Task.Factory.StartNew(async () => await pubnub.WhereNow().Uuid(config.UserId).ExecuteAsync()).Result.Result;
#else
            PNResult<PNWhereNowResult> r = await pubnub.WhereNow().Uuid(config.UserId).ExecuteAsync();
#endif
            if (r.Result != null)
            {
                Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                receivedWhereNowMessage = true;
            }

            expected = "[[],\"14740704540745015\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/leave", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(1000);
            else Thread.Sleep(100);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedWhereNowMessage, "where_now message not received");
        }


        [Test]
        public static void IfSetAndGetUserStateThenItShouldReturnInfo()
        {
            server.ClearRequests();
            
            bool receivedUserStateMessage = false;

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

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = 310 * 1000;
            string channel = "hello_my_channel";

            Dictionary<string, object> dicState = new Dictionary<string, object>();
            dicState.Add("testkey", "testval");

            string expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"testkey\": \"testval\"}, \"service\": \"Presence\"}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    //.WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/uuid/{2}/data", PubnubCommon.SubscribeKey, channel, customUserId.ToString()))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent userStateManualEvent = new ManualResetEvent(false);
            pubnub.SetPresenceState()
                            .Channels(new [] { channel })
                            .State(dicState)
                            .Execute(new PNSetStateResultExt(
                                (r, s) => {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedUserStateMessage = true;
                                    userStateManualEvent.Set();
                                }));

            userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedUserStateMessage)
            {
                if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
                else Thread.Sleep(200);

                receivedUserStateMessage = false;
                userStateManualEvent = new ManualResetEvent(false);

                expected = "{\"status\": 200, \"uuid\": \"mylocalmachine.mydomain.com\", \"service\": \"Presence\", \"message\": \"OK\", \"payload\": {\"testkey\": \"testval\"}, \"channel\": \"hello_my_channel\"}";
                server.AddRequest(new Request()
                        .WithMethod("GET")
                        //.WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/uuid/{2}", PubnubCommon.SubscribeKey, channel, customUserId.ToString()))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.GetPresenceState()
                                .Channels(new [] { channel })
                                .Execute(new PNGetStateResultExt(
                                (r, s) => {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedUserStateMessage = true;
                                    userStateManualEvent.Set();
                                }));
                userStateManualEvent.WaitOne(manualResetEventWaitTimeout);
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedUserStateMessage, "IfSetAndGetUserStateThenItShouldReturnInfo failed");
        }

        [Test]
        public static void IfSetAndDeleteUserStateThenItShouldReturnInfo()
        {
            server.ClearRequests();

            Request getRequest = new Request();
            bool receivedUserStateMessage = false;

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

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config, authToken);

            manualResetEventWaitTimeout = 310 * 1000;
            string channel = "hello_my_channel";

            
            Dictionary<string, object> dicState = new Dictionary<string, object>();
            dicState.Add("k", "v");

            string expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"k\": \"v\"}, \"service\": \"Presence\"}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/uuid/{2}/data", PubnubCommon.SubscribeKey, channel, config.UserId))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("state", "%7B%22k%22%3A%22v%22%7D")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent userStateManualEvent = new ManualResetEvent(false);
            pubnub.SetPresenceState()
                            .Channels(new [] { channel })
                            .State(dicState)
                            .Execute(new PNSetStateResultExt(
                                (r, s) => {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedUserStateMessage = true;
                                    userStateManualEvent.Set();
                                }));

            userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
            else Thread.Sleep(200);

            if (receivedUserStateMessage)
            {
                expected = "{\"status\": 200, \"uuid\": \"mytestuuid\", \"service\": \"Presence\", \"message\": \"OK\", \"payload\": {\"k\": \"v\"}, \"channel\": \"hello_my_channel\"}";
                getRequest = new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/uuid/{2}", PubnubCommon.SubscribeKey, channel, config.UserId))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK);
                server.AddRequest(getRequest);

                receivedUserStateMessage = false;
                userStateManualEvent = new ManualResetEvent(false);
                pubnub.GetPresenceState()
                                .Channels(new [] { channel })
                                .Execute(new PNGetStateResultExt(
                                (r, s) => {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedUserStateMessage = true;
                                    userStateManualEvent.Set();
                                }));

                userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
                else Thread.Sleep(200);
            }

            if (receivedUserStateMessage)
            {
                receivedUserStateMessage = false;

                userStateManualEvent = new ManualResetEvent(false);
                dicState = new Dictionary<string, object>();
                dicState.Add("k", null);

                expected = "{\"status\": 200, \"message\": \"OK\", \"payload\": {\"k\": null}, \"service\": \"Presence\"}";
                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/presence/sub_key/{0}/channel/{1}/uuid/{2}/data", PubnubCommon.SubscribeKey, channel, config.UserId))
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("state", "%7B%22k%22%3Anull%7D")
                        .WithParameter("timestamp", "1356998400")
                        .WithParameter("uuid", config.UserId)
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.SetPresenceState()
                                .Channels(new [] { channel })
                                .State(dicState)
                                .Execute(new PNSetStateResultExt(
                                (r, s) => {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedUserStateMessage = true;
                                    userStateManualEvent.Set();
                                }));

                userStateManualEvent.WaitOne(manualResetEventWaitTimeout);

                if (!PubnubCommon.EnableStubTest) Thread.Sleep(2000);
                else Thread.Sleep(200);
            }

            if (receivedUserStateMessage)
            {
                receivedUserStateMessage = false;

                expected = "{\"status\": 200, \"uuid\": \"mytestuuid\", \"service\": \"Presence\", \"message\": \"OK\", \"payload\": {\"k\": null}, \"channel\": \"hello_my_channel\"}";
                getRequest.WithResponse(expected);
                server.AddRequest(getRequest);

                userStateManualEvent = new ManualResetEvent(false);
                pubnub.GetPresenceState()
                                .Channels(new [] { channel })
                                .Execute(new PNGetStateResultExt(
                                (r, s) => {
                                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                    receivedUserStateMessage = true;
                                    userStateManualEvent.Set();
                                }));

                userStateManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedUserStateMessage, "IfSetAndDeleteUserStateThenItShouldReturnInfo message not received");
        }

        [Test]
        public static void ThenPresenceHeartbeatShouldReturnMessage()
        {
            server.ClearRequests();

            bool receivedPresenceMessage = false;

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
            server.RunOnHttps(false);


            ManualResetEvent presenceManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(p));
                    if (p.Event == "join") { receivedPresenceMessage = true; }
                    presenceManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        presenceManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: AddListener failed");
            }


            string channel = "hello_my_channel_1";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14828440156769626\",\"r\":7},\"m\":[{\"a\":\"4\",\"f\":512,\"p\":{\"t\":\"14828440155770431\",\"r\":2},\"k\":\"demo-36\",\"c\":\"hello_my_channel-pnpres\",\"d\":{\"action\": \"join\", \"timestamp\": 1482844015, \"uuid\": \"mytestuuid\", \"occupancy\": 1},\"b\":\"hello_my_channel-pnpres\"}]}";
            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel + "," + channel + "-pnpres"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "_JVs4gooSMhdgxRO6FNkk6HwlkyxqcRATHU5j3vkJ9s=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { channel }).WithPresence().Execute();
            presenceManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!PubnubCommon.EnableStubTest) { Thread.Sleep(pubnub.PNConfig.PresenceTimeout + (3 * 1000)); }

            pubnub.Unsubscribe<string>().Channels(new [] { channel }).Execute();

            if (!PubnubCommon.EnableStubTest) { Thread.Sleep(1000); }
            else { Thread.Sleep(100); }

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                System.Diagnostics.Debug.WriteLine("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "ThenPresenceHeartbeatShouldReturnMessage not received");
        }
        
        [Test]
        public static async Task ThenReceiveCustomObjectPresenceCallback()
        {
            bool receivedPresenceMessage = false;

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
            ManualResetEvent presenceManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) => { Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m)); },
                (o, p) => {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(p));
                    if (p.Event == "join") { receivedPresenceMessage = true; }
                    presenceManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        presenceManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            if (!pubnub.AddListener(listenerSubCallack))
            {
                Assert.Fail("ATTENTION: AddListener failed");
            }

            manualResetEventWaitTimeout = 15000;
            
            pubnub.Subscribe<WhenAMessageIsPublished.MockObject>().Channels(new [] { presenceTestChannel }).WithPresence().Execute();
            presenceManualEvent.WaitOne(manualResetEventWaitTimeout);

            await Task.Delay(10000);

            pubnub.Unsubscribe<string>().Channels(new [] { presenceTestChannel }).Execute();

            await Task.Delay(2000);

            if (!pubnub.RemoveListener(listenerSubCallack))
            {
                Assert.Fail("ATTENTION: RemoveListener failed");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedPresenceMessage, "ThenReceiveCustomObjectPresenceCallback not received");
        }
        
        [Test]
        public static async Task ThenWhereNowShouldReturnSubscribedChannel()
        {
            string subscribedChannel = "hello_my_channel";

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

            pubnub = createPubNubInstance(config, authToken);

            pubnub.Subscribe<string>().Channels(new[] { subscribedChannel }).WithPresence().Execute();
            await Task.Delay(3000);

            ManualResetEvent whereNowManualEvent = new ManualResetEvent(false);
            PNWhereNowResult whereNowResult = null;

            pubnub.WhereNow().Uuid(config.UserId).Execute(new PNWhereNowResultExt(
                (r, s) => {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                    whereNowResult = r;
                    whereNowManualEvent.Set();
                }));
            whereNowManualEvent.WaitOne(manualResetEventWaitTimeout);

            Assert.IsNotNull(whereNowResult, "WhereNow result should not be null");
            Assert.IsNotNull(whereNowResult.Channels, "Channels list should not be null");
            Assert.Contains(subscribedChannel, whereNowResult.Channels, "Subscribed channel should be in the channels list");
            
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenSetPresenceStateShouldWorkCorrectly()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSetPresenceStateShouldWorkCorrectly");
                return;
            }

            string channel = "hello_my_channel";
            string channelGroup = "hello_my_group";
            string customUuid = "mytestuuid";

            PNConfiguration config = new PNConfiguration(new UserId(customUuid))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }

            // Test 1: Set state for channel
            Dictionary<string, object> channelState = new Dictionary<string, object>
            {
                { "status", "online" },
                { "lastSeen", DateTime.UtcNow.ToString() }
            };

            PNResult<PNSetStateResult> setChannelStateResult = await pubnub.SetPresenceState()
                .Channels(new[] { channel })
                .State(channelState)
                .ExecuteAsync();

            Assert.IsNotNull(setChannelStateResult.Result, "Set channel state result should not be null");
            Assert.AreEqual(200, setChannelStateResult.Status.StatusCode, "Set channel state status code should be 200");
            Assert.IsFalse(setChannelStateResult.Status.Error, "Set channel state should not have errors");
            Assert.IsNotNull(setChannelStateResult.Result.State, "State should not be null");
            Assert.IsTrue(setChannelStateResult.Result.State.ContainsKey("status"), "State should contain status");
            Assert.AreEqual("online", setChannelStateResult.Result.State["status"].ToString(), "Status should be online");

            // Test 2: Set state for channel group
            Dictionary<string, object> channelGroupState = new Dictionary<string, object>
            {
                { "type", "group" },
                { "members", 5 }
            };

            PNResult<PNSetStateResult> setChannelGroupStateResult = await pubnub.SetPresenceState()
                .ChannelGroups(new[] { channelGroup })
                .State(channelGroupState)
                .ExecuteAsync();

            Assert.IsNotNull(setChannelGroupStateResult.Result, "Set channel group state result should not be null");
            Assert.AreEqual(200, setChannelGroupStateResult.Status.StatusCode, "Set channel group state status code should be 200");
            Assert.IsFalse(setChannelGroupStateResult.Status.Error, "Set channel group state should not have errors");
            Assert.IsNotNull(setChannelGroupStateResult.Result.State, "State should not be null");
            Assert.IsTrue(setChannelGroupStateResult.Result.State.ContainsKey("type"), "State should contain type");
            Assert.AreEqual("group", setChannelGroupStateResult.Result.State["type"].ToString(), "Type should be group");

            // Test 3: Set state for both channel and channel group
            Dictionary<string, object> combinedState = new Dictionary<string, object>
            {
                { "combined", true },
                { "timestamp", DateTime.UtcNow.ToString() }
            };

            PNResult<PNSetStateResult> setCombinedStateResult = await pubnub.SetPresenceState()
                .Channels(new[] { channel })
                .ChannelGroups(new[] { channelGroup })
                .State(combinedState)
                .ExecuteAsync();

            Assert.IsNotNull(setCombinedStateResult.Result, "Set combined state result should not be null");
            Assert.AreEqual(200, setCombinedStateResult.Status.StatusCode, "Set combined state status code should be 200");
            Assert.IsFalse(setCombinedStateResult.Status.Error, "Set combined state should not have errors");
            Assert.IsNotNull(setCombinedStateResult.Result.State, "State should not be null");
            Assert.IsTrue(setCombinedStateResult.Result.State.ContainsKey("combined"), "State should contain combined flag");
            Assert.IsTrue((bool)setCombinedStateResult.Result.State["combined"], "Combined flag should be true");

            // Test 4: Set state with custom UUID
            Dictionary<string, object> customState = new Dictionary<string, object>
            {
                { "customUuid", true },
                { "timestamp", DateTime.UtcNow.ToString() }
            };

            PNResult<PNSetStateResult> setCustomUuidStateResult = await pubnub.SetPresenceState()
                .Channels(new[] { channel })
                .Uuid(customUuid)
                .State(customState)
                .ExecuteAsync();

            Assert.IsNotNull(setCustomUuidStateResult.Result, "Set custom UUID state result should not be null");
            Assert.AreEqual(200, setCustomUuidStateResult.Status.StatusCode, "Set custom UUID state status code should be 200");
            Assert.IsFalse(setCustomUuidStateResult.Status.Error, "Set custom UUID state should not have errors");
            Assert.IsNotNull(setCustomUuidStateResult.Result.State, "State should not be null");
            Assert.IsTrue(setCustomUuidStateResult.Result.State.ContainsKey("customUuid"), "State should contain customUuid flag");
            Assert.IsTrue((bool)setCustomUuidStateResult.Result.State["customUuid"], "Custom UUID flag should be true");

            // Test 5: Set state with query parameters
            Dictionary<string, object> queryParams = new Dictionary<string, object>
            {
                { "custom_param", "value" }
            };

            Dictionary<string, object> queryState = new Dictionary<string, object>
            {
                { "hasQueryParams", true }
            };

            PNResult<PNSetStateResult> setQueryStateResult = await pubnub.SetPresenceState()
                .Channels(new[] { channel })
                .State(queryState)
                .QueryParam(queryParams)
                .ExecuteAsync();

            Assert.IsNotNull(setQueryStateResult.Result, "Set query state result should not be null");
            Assert.AreEqual(200, setQueryStateResult.Status.StatusCode, "Set query state status code should be 200");
            Assert.IsFalse(setQueryStateResult.Status.Error, "Set query state should not have errors");
            Assert.IsNotNull(setQueryStateResult.Result.State, "State should not be null");
            Assert.IsTrue(setQueryStateResult.Result.State.ContainsKey("hasQueryParams"), "State should contain hasQueryParams flag");
            Assert.IsTrue((bool)setQueryStateResult.Result.State["hasQueryParams"], "Has query params flag should be true");

            // Test 6: Verify state persistence with GetPresenceState
            PNResult<PNGetStateResult> getStateResult = await pubnub.GetPresenceState()
                .Channels(new[] { channel })
                .ExecuteAsync();

            Assert.IsNotNull(getStateResult.Result, "Get state result should not be null");
            Assert.AreEqual(200, getStateResult.Status.StatusCode, "Get state status code should be 200");
            Assert.IsFalse(getStateResult.Status.Error, "Get state should not have errors");
            Assert.IsNotNull(getStateResult.Result.StateByUUID, "State by UUID should not be null");
            Assert.IsTrue(getStateResult.Result.StateByUUID.ContainsKey("hasQueryParams"), "State should contain key \"hasQueryParams\"");

            // Cleanup
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenHereNowShouldReturnCorrectUserData()
        {
            string channel = $"foo.tc_{Guid.NewGuid()}";
            string customUuid = $"fuu.tu_{Guid.NewGuid()}";
            Dictionary<string, object> userState = new Dictionary<string, object>
            {
                { "status", "online" },
                { "lastSeen", DateTime.UtcNow.ToString() },
                { "customData", new Dictionary<string, object> { { "key", "value" } } }
            };

            PNConfiguration config = new PNConfiguration(new UserId(customUuid))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false
            };

            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }

            // Subscribe to channel
            pubnub.Subscribe<string>().Channels(new[] { channel }).WithPresence().Execute();
            await Task.Delay(3000); // Wait for subscription to complete

            // Set user state
            PNResult<PNSetStateResult> setStateResult = await pubnub.SetPresenceState()
                .Uuid(customUuid)
                .Channels(new[] { channel })
                .State(userState)
                .ExecuteAsync();

            Assert.IsNotNull(setStateResult.Result, "Set state result should not be null");
            Assert.IsFalse(setStateResult.Status.Error, "Set state should not have errors");
            Assert.AreEqual(200, setStateResult.Status.StatusCode, "Set state should return 200 status code");

            // Get HereNow data
            PNResult<PNHereNowResult> hereNowResult = await pubnub.HereNow()
                .Channels(new[] { channel })
                .IncludeState(true)
                .IncludeUUIDs(true)
                .ExecuteAsync();

            Assert.IsNotNull(hereNowResult.Result, "HereNow result should not be null");
            Assert.IsFalse(hereNowResult.Status.Error, "HereNow should not have errors");
            Assert.AreEqual(200, hereNowResult.Status.StatusCode, "HereNow should return 200 status code");

            // Verify channel data
            Assert.IsNotNull(hereNowResult.Result.Channels, "Channels should not be null");
            Assert.IsTrue(hereNowResult.Result.Channels.ContainsKey(channel), "Channel should be present in results");
            
            var channelData = hereNowResult.Result.Channels[channel];
            Assert.IsNotNull(channelData, "Channel data should not be null");
            Assert.AreEqual(1, channelData.Occupancy, "Channel should have one occupant");
            Assert.IsNotNull(channelData.Occupants, "Occupants should not be null");
            Assert.AreEqual(1, channelData.Occupants.Count, "Should have one occupant");

            // Verify user data
            var occupant = channelData.Occupants[0];
            Assert.AreEqual(customUuid, occupant.Uuid, "UUID should match");
            Assert.IsNotNull(occupant.State, "State should not be null");
            
            // Cast state to dictionary and verify its contents
            var stateDict = occupant.State as Dictionary<string, object>;
            Assert.IsNotNull(stateDict, "State should be castable to Dictionary<string,object>");
            Assert.IsTrue(stateDict.ContainsKey("status"), "State should contain status key");
            Assert.AreEqual("online", stateDict["status"].ToString(), "Status should match");
            Assert.IsTrue(stateDict.ContainsKey("lastSeen"), "State should contain lastSeen key");
            Assert.IsNotNull(stateDict["lastSeen"], "LastSeen should be present");
            Assert.IsTrue(stateDict.ContainsKey("customData"), "State should contain customData key");
            
            // Verify nested customData dictionary
            var customData = stateDict["customData"] as Dictionary<string, object>;
            Assert.IsNotNull(customData, "CustomData should be castable to Dictionary<string,object>");
            Assert.IsTrue(customData.ContainsKey("key"), "CustomData should contain key");
            Assert.AreEqual("value", customData["key"].ToString(), "CustomData value should match");

            // Cleanup
            pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();
            await Task.Delay(1000);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

    }
}
