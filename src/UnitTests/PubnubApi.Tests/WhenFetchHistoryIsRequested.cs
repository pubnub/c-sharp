using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenFetchHistoryIsRequested : TestHarness
    {
        const string messageForNoStorePublish = "Pubnub Messaging With No Storage";
        const string messageForPublish = "Pubnub Messaging API 1";

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

            string grantChannel = "hello_my_channel";

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"user\",\"subscribe_key\":\"demo-36\",\"ttl\":20,\"channel\":\"hello_my_channel\",\"auths\":{\"myAuth\":{\"r\":1,\"w\":1,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v2/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel", grantChannel)
                    .WithParameter("m", "1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r", "1")
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl", "20")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithParameter("w", "1")
                    .WithParameter("signature", "JMQKzXgfqNo-HaHuabC0gq0X6IkVMHa9AWBCg6BGN1Q=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent grantManualEvent = new ManualResetEvent(false);
            pubnub.Grant().Channels(new[] { grantChannel }).AuthKeys(new[] { authKey }).Read(true).Write(true).Manage(true).TTL(20)
                .Execute(new PNAccessManagerGrantResultExt((r, s) => {
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
                                if (read && write)
                                {
                                    receivedGrantMessage = true;
                                }
                            }
                        }
                    }
                    catch { /* ignore */ }
                    finally { grantManualEvent.Set(); }
                }));
            Thread.Sleep(1000);
            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenDetailedHistoryIsRequested Grant access failed.");
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void FetchHistoryNoStoreShouldNotGetMessage()
        {
            server.ClearRequests();

            bool receivedMessage = true;
            long publishTimetoken = 0;

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

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            string message = messageForNoStorePublish;

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22Pubnub%20Messaging%20With%20No%20Storage%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("store", "0")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Publish().Channel(channel).Message(message).ShouldStore(false)
                .Execute(new PNPublishResultExt((r, s) => {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        publishTimetoken = r.Timetoken;
                        receivedMessage = true;
                    }
                    publishManualEvent.Set();
                }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "No Store Publish Failed");
            }
            else
            {
                receivedMessage = false;

                Thread.Sleep(1000);

                expected = "[[\"Pubnub Messaging API 1\"],14715432709547189,14715432709547189]";
                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("start", "14715322883933786")
                        .WithParameter("uuid", config.UserId.ToString())
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);
                pubnub.FetchHistory().Channels(new string[] { channel })
                    .Start(publishTimetoken)
                    .Reverse(false)
                    .IncludeMeta(false)
                    .Execute(new PNFetchHistoryResultExt((r, s) => {
                        if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count > 0)
                        {
                            foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in r.Messages)
                            {
                                List<PNHistoryItemResult> itemList = channelItem.Value;
                                foreach(PNHistoryItemResult item in itemList)
                                {
                                    if (item.Entry != null && item.Entry.ToString() == messageForNoStorePublish && item.Timetoken == publishTimetoken)
                                    {
                                        receivedMessage = true;
                                        break;
                                    }
                                }
                            }
                        }
                        historyManualEvent.Set();
                    }));

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(!receivedMessage, "Message stored for Publish when no store is expected");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void FetchHistoryShouldReturnDecryptMessage()
        {
            server.ClearRequests();

            bool receivedMessage = false;
            long publishTimetoken = 0;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
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
            if (PubnubCommon.PAMServerSideRun)
            {
                config.AuthKey = "myAuth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            string message = messageForPublish;

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22f42pIQcWZ9zbTbH8cyLwByD%2FGsviOE0vcREIEVPARR0%3D%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channel).Message(message).ShouldStore(true)
                .Execute(new PNPublishResultExt((r, s) => {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        publishTimetoken = r.Timetoken;
                        receivedMessage = true;
                    }
                    publishManualEvent.Set();
                }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "Encrypted message Publish Failed");
            }
            else
            {
                receivedMessage = false;

                Thread.Sleep(1000);

                expected = "[[{\"message\":\"f42pIQcWZ9zbTbH8cyLwByD/GsviOE0vcREIEVPARR0=\",\"timetoken\":14715322883933786}],14834460344901569,14834460344901569]";
                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "1")
                        .WithParameter("end", "14715322883933786")
                        .WithParameter("include_token", "true")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("start", "14715322883933785")
                        .WithParameter("uuid", config.UserId.ToString())
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);
                pubnub.FetchHistory()
                    .Channels(new string[] {channel })
                    .Start(publishTimetoken - 1)
                    .End(publishTimetoken)
                    .MaximumPerChannel(1)
                    .Reverse(false)
                    .IncludeMeta(false)
                    .Execute(new PNFetchHistoryResultExt((r, s) => {
                        if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count > 0)
                        {
                            foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in r.Messages)
                            {
                                List<PNHistoryItemResult> itemList = channelItem.Value;
                                foreach (PNHistoryItemResult item in itemList)
                                {
                                    if (item.Entry != null && item.Entry.ToString() == messageForPublish && item.Timetoken == publishTimetoken)
                                    {
                                        receivedMessage = true;
                                        break;
                                    }
                                }
                            }
                        }
                        historyManualEvent.Set();
                    }));
                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                Assert.IsTrue(receivedMessage, "Encrypted message not showed up in history");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncShouldReturnDecryptMessage()
#else
        public static async Task FetchHistoryWithAsyncShouldReturnDecryptMessage()
#endif
        {
            server.ClearRequests();

            bool receivedMessage = false;
            long publishTimetoken = 0;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
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
            if (PubnubCommon.PAMServerSideRun)
            {
                config.AuthKey = "myAuth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            string message = messageForPublish;

            string expected = "[1,\"Sent\",\"14715322883933786\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, "%22f42pIQcWZ9zbTbH8cyLwByD%2FGsviOE0vcREIEVPARR0%3D%22"))
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNPublishResult> publishResult = Task.Factory.StartNew(async () => await pubnub.Publish().Channel(channel).Message(message).ShouldStore(true)
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNPublishResult> publishResult = await pubnub.Publish().Channel(channel).Message(message).ShouldStore(true)
                .ExecuteAsync();
#endif
            if (publishResult.Result != null && publishResult.Status.StatusCode == 200 && !publishResult.Status.Error)
            {
                publishTimetoken = publishResult.Result.Timetoken;
                receivedMessage = true;
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "Encrypted message Publish Failed");
            }
            else
            {
                receivedMessage = false;

                Thread.Sleep(1000);

                expected = "[[{\"message\":\"f42pIQcWZ9zbTbH8cyLwByD/GsviOE0vcREIEVPARR0=\",\"timetoken\":14715322883933786}],14834460344901569,14834460344901569]";
                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "1")
                        .WithParameter("end", "14715322883933786")
                        .WithParameter("include_token", "true")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("start", "14715322883933785")
                        .WithParameter("uuid", config.UserId.ToString())
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
                PNResult<PNFetchHistoryResult> fetchHistResult = Task.Factory.StartNew(async () => await pubnub.FetchHistory()
                    .Channels(new string[] { channel })
                    .Start(publishTimetoken - 1)
                    .End(publishTimetoken)
                    .MaximumPerChannel(1)
                    .Reverse(false)
                    .IncludeMeta(false)
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNFetchHistoryResult> fetchHistResult = await pubnub.FetchHistory()
                    .Channels(new string[] { channel })
                    .Start(publishTimetoken - 1)
                    .End(publishTimetoken)
                    .MaximumPerChannel(1)
                    .Reverse(false)
                    .IncludeMeta(false)
                    .ExecuteAsync();
#endif
                if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error 
                    && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel) 
                    && fetchHistResult.Result.Messages[channel].Count > 0)
                {
                    foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in fetchHistResult.Result.Messages)
                    {
                        List<PNHistoryItemResult> itemList = channelItem.Value;
                        foreach (PNHistoryItemResult item in itemList)
                        {
                            if (item.Entry != null && item.Entry.ToString() == messageForPublish && item.Timetoken == publishTimetoken)
                            {
                                receivedMessage = true;
                                break;
                            }
                        }
                    }
                }

                Assert.IsTrue(receivedMessage, "Encrypted message not showed up in history");
            }
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }


        [Test]
        public static void FetchHistoryCount10ReturnsRecords()
        {
            server.ClearRequests();

            bool receivedMessage = false;

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

            server.RunOnHttps(false);
            if (PubnubCommon.PAMServerSideRun)
            {
                config.AuthKey = "myAuth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("count", "10")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent historyManualEvent = new ManualResetEvent(false);
            pubnub.FetchHistory().Channels(new string[] { channel })
                .MaximumPerChannel(10)
                .IncludeMeta(false)
                .Execute(new PNFetchHistoryResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count >= 10)
                    {
                        System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    historyManualEvent.Set();
                }));
            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History Failed");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncCount10ReturnsRecords()
#else
        public static async Task FetchHistoryWithAsyncCount10ReturnsRecords()
#endif
        {
            server.ClearRequests();

            bool receivedMessage = false;

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

            server.RunOnHttps(false);
            if (PubnubCommon.PAMServerSideRun)
            {
                config.AuthKey = "myAuth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("count", "10")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNFetchHistoryResult> fetchHistResult = Task.Factory.StartNew(async () => await pubnub.FetchHistory().Channels(new string[] { channel })
                .MaximumPerChannel(10)
                .IncludeMeta(false)
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNFetchHistoryResult> fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                .MaximumPerChannel(10)
                .IncludeMeta(false)
                .ExecuteAsync();
#endif
            if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error 
                && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel) 
                && fetchHistResult.Result.Messages[channel].Count >= 10)
            {
                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fetchHistResult.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History Failed");
        }

        [Test]
        public static void FetchHistoryWithMessageActionsReturnsRecords()
        {
            server.ClearRequests();

            bool receivedMessage = false;

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

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history-with-actions/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("count", "10")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent historyManualEvent = new ManualResetEvent(false);
            pubnub.FetchHistory().Channels(new string[] { channel })
                .IncludeMeta(false)
                .IncludeMessageActions(true)
                .Execute(new PNFetchHistoryResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count >= 10)
                    {
                        System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    historyManualEvent.Set();
                }));
            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History Failed");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncWithMessageActionsReturnsRecords()
#else
        public static async Task FetchHistoryWithAsyncWithMessageActionsReturnsRecords()
#endif
        {
            server.ClearRequests();

            bool receivedMessage = false;

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

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history-with-actions/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("count", "10")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNFetchHistoryResult> fetchHistResult = Task.Factory.StartNew(async () => await pubnub.FetchHistory().Channels(new string[] { channel })
                .IncludeMeta(false)
                .IncludeMessageActions(true)
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNFetchHistoryResult> fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                .IncludeMeta(false)
                .IncludeMessageActions(true)
                .ExecuteAsync();
#endif
            if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error 
                && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel) 
                && fetchHistResult.Result.Messages[channel].Count >= 10)
            {
                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fetchHistResult.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History Failed");
        }

        [Test]
        public static void FetchHistoryCount10ReverseTrueReturnsRecords()
        {
            server.ClearRequests();

            bool receivedMessage = false;

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

            server.RunOnHttps(false);
            if (PubnubCommon.PAMServerSideRun)
            {
                config.AuthKey = "myAuth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";

            string expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("count", "10")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("reverse", "true")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent historyManualEvent = new ManualResetEvent(false);
            pubnub.FetchHistory().Channels(new string[] {channel })
                .MaximumPerChannel(10)
                .Reverse(true)
                .IncludeMeta(false)
                .Execute(new PNFetchHistoryResultExt((r, s) => {
                    if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count >= 10)
                    {
                        receivedMessage = true;
                    }
                    historyManualEvent.Set();
                }));
            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History Failed");
        }

        [Test]
        public static void FetchHistoryStartWithReverseTrue()
        {
            server.ClearRequests();

            bool receivedMessage = false;
            long publishTimetoken = 0;

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

            server.RunOnHttps(false);
            if (PubnubCommon.PAMServerSideRun)
            {
                config.AuthKey = "myAuth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

            long currentTimetoken = 0;

            string expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent timeManualEvent = new ManualResetEvent(false);
            pubnub.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                    currentTimetoken = (r != null && s.StatusCode == 200 && s.Error == false) ? r.Timetoken : 0;
                }
                catch { /* ignore */ }
                finally { timeManualEvent.Set(); }
            }));
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            Thread.Sleep(2000);

            for (int index = 0; index < 10; index++)
            {
                receivedMessage = false;
                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, String.Format("%22DetailedHistoryStartTimeWithReverseTrue%20{0}%22", index)))
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId.ToString())
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(channel)
                    .Message(string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index))
                    .Execute(new PNPublishResultExt((r, s) => {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            publishTimetoken = r.Timetoken;
                            receivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                if (!receivedMessage)
                {
                    break;
                }
            }

            if (receivedMessage)
            {
                Thread.Sleep(2000);

                expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("start", "1356998400")
                        .WithParameter("uuid", config.UserId.ToString())
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent historyManualEvent = new ManualResetEvent(false);
                pubnub.FetchHistory().Channels(new string[] {channel })
                    .Start(currentTimetoken)
                    .Reverse(false)
                    .IncludeMeta(false)
                    .Execute(new PNFetchHistoryResultExt((r, s) => {
                        if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.Count >= 10)
                        {
                            receivedMessage = true;
                        }
                        historyManualEvent.Set();
                    }));

                historyManualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History with Start and Reverse True Failed");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncStartWithReverseTrue()
#else
        public static async Task FetchHistoryWithAsyncStartWithReverseTrue()
#endif
        {
            server.ClearRequests();

            bool receivedMessage = false;

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

            server.RunOnHttps(false);
            if (PubnubCommon.PAMServerSideRun)
            {
                config.AuthKey = "myAuth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

            long currentTimetoken = 0;

            string expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNTimeResult> timeResult = Task.Factory.StartNew(async () => await pubnub.Time().ExecuteAsync()).Result.Result;
#else
            PNResult<PNTimeResult> timeResult = await pubnub.Time().ExecuteAsync();
#endif
            try
            {
                Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(timeResult.Result));
                currentTimetoken = (timeResult.Result != null && timeResult.Status.StatusCode == 200 && !timeResult.Status.Error) ? timeResult.Result.Timetoken : 0;
            }
            catch { /* ignore */ }

            Thread.Sleep(2000);

            for (int index = 0; index < 10; index++)
            {
                receivedMessage = false;
                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, String.Format("%22DetailedHistoryStartTimeWithReverseTrue%20{0}%22", index)))
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("uuid", config.UserId.ToString())
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
                PNResult<PNPublishResult> publishResult = Task.Factory.StartNew(async () => await pubnub.Publish().Channel(channel)
                    .Message(string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index))
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNPublishResult> publishResult = await pubnub.Publish().Channel(channel)
                    .Message(string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index))
                    .ExecuteAsync();
#endif
                if (publishResult.Result != null && publishResult.Status.StatusCode == 200 && !publishResult.Status.Error && publishResult.Result.Timetoken > 0)
                {
                    receivedMessage = true;
                }

                if (!receivedMessage)
                {
                    break;
                }
            }

            if (receivedMessage)
            {
                Thread.Sleep(2000);

                expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithParameter("count", "100")
                        .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                        .WithParameter("requestid", "myRequestId")
                        .WithParameter("start", "1356998400")
                        .WithParameter("uuid", config.UserId.ToString())
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
                PNResult<PNFetchHistoryResult> fetchHistResult = Task.Factory.StartNew(async () => await pubnub.FetchHistory().Channels(new string[] { channel })
                    .Start(currentTimetoken)
                    .Reverse(false)
                    .IncludeMeta(false)
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNFetchHistoryResult> fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                    .Start(currentTimetoken)
                    .Reverse(false)
                    .IncludeMeta(false)
                    .ExecuteAsync();
#endif
                if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.Count >= 10)
                {
                    receivedMessage = true;
                }
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Async/Await Fetch History with Start and Reverse True Failed");
        }


        [Test]
        public static void FetchHistoryWithNullKeysReturnsError()
        {
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = null,
                SubscribeKey = null,
                SecretKey = null,
                CipherKey = null,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            Assert.Throws<MissingMemberException>(() =>
            {
                ManualResetEvent historyManualEvent = new ManualResetEvent(false);
                pubnub.FetchHistory().Channels(new string[] { channel })
                    .MaximumPerChannel(10)
                    .Reverse(true)
                    .IncludeMeta(false)
                    .Execute(new PNFetchHistoryResultExt((r, s) => {
                        receivedMessage = r == null || s.StatusCode != 200 || s.Error;
                        historyManualEvent.Set();
                    }));
                historyManualEvent.WaitOne(manualResetEventWaitTimeout);
            });

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void FetchHistoryShouldReturnUnencrypedSecretMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;
            CommonFetchHistoryShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnUnencrypedSecretMessage - Fetch History Result not expected");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncShouldReturnUnencrypedSecretMessage()
#else
        public static async Task FetchHistoryWithAsyncShouldReturnUnencrypedSecretMessage()
#endif
        {
            server.ClearRequests();
#if NET40
            bool receivedMessage = Task.Factory.StartNew(async () => await CommonFetchHistoryWithAsyncShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", false)).Result.Result;
#else
            bool receivedMessage = await CommonFetchHistoryWithAsyncShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
#endif
            Assert.IsTrue(receivedMessage, "FetchHistoryWithAsyncShouldReturnUnencrypedSecretMessage - Fetch History Result not expected");
        }

        [Test]
        public static void FetchHistoryShouldReturnUnencrypedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;
            CommonFetchHistoryShouldReturnUnencryptedMessageBasedOnParams("", "", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnUnencrypedMessage - Fetch History Result not expected");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncShouldReturnUnencrypedMessage()
#else
        public static async Task FetchHistoryWithAsyncShouldReturnUnencrypedMessage()
#endif
        {
            server.ClearRequests();
            if (PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("Ignored for Server side run");
                return;
            }

#if NET40
            bool receivedMessage = Task.Factory.StartNew(async () => await CommonFetchHistoryWithAsyncShouldReturnUnencryptedMessageBasedOnParams("", "", false)).Result.Result;
#else
            bool receivedMessage = await CommonFetchHistoryWithAsyncShouldReturnUnencryptedMessageBasedOnParams("", "", false);
#endif
            Assert.IsTrue(receivedMessage, "FetchHistoryWithAsyncShouldReturnUnencrypedMessage - Fetch History Result not expected");
        }


        [Test]
        public static void FetchHistoryShouldReturnUnencrypedSecretSSLMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;
            CommonFetchHistoryShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnUnencrypedSecretSSLMessage - Fetch History Result not expected");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncShouldReturnUnencrypedSecretSSLMessage()
#else
        public static async Task FetchHistoryWithAsyncShouldReturnUnencrypedSecretSSLMessage()
#endif
        {
            server.ClearRequests();
#if NET40
            bool receivedMessage = Task.Factory.StartNew(async () => await CommonFetchHistoryWithAsyncShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", true)).Result.Result;
#else
            bool receivedMessage = await CommonFetchHistoryWithAsyncShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
#endif
            Assert.IsTrue(receivedMessage, "FetchHistoryWithAsyncShouldReturnUnencrypedSecretSSLMessage - Fetch History Result not expected");
        }

        [Test]
        public static void FetchHistoryShouldReturnUnencrypedSSLMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;
            CommonFetchHistoryShouldReturnUnencryptedMessageBasedOnParams("", "", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnUnencrypedSSLMessage - Fetch History Result not expected");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncShouldReturnUnencrypedSSLMessage()
#else
        public static async Task FetchHistoryWithAsyncShouldReturnUnencrypedSSLMessage()
#endif
        {
            server.ClearRequests();
            if (PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("Ignored for Server side run");
                return;
            }

#if NET40
            bool receivedMessage = Task.Factory.StartNew(async () => await CommonFetchHistoryWithAsyncShouldReturnUnencryptedMessageBasedOnParams("", "", true)).Result.Result;
#else
            bool receivedMessage = await CommonFetchHistoryWithAsyncShouldReturnUnencryptedMessageBasedOnParams("", "", true);
#endif
            Assert.IsTrue(receivedMessage, "FetchHistoryWithAsyncShouldReturnUnencrypedSSLMessage - Fetch History Result not expected");
        }

        [Test]
        public static void FetchHistoryShouldReturnEncrypedMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;
            CommonFetchHistoryShouldReturnEncryptedMessageBasedOnParams("", "enigma", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnEncrypedMessage - Fetch History Result not expected");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncShouldReturnEncrypedMessage()
#else
        public static async Task FetchHistoryWithAsyncShouldReturnEncrypedMessage()
#endif
        {
            server.ClearRequests();
            if (PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("Ignored for Server side run");
                return;
            }

#if NET40
            bool receivedMessage = Task.Factory.StartNew(async () => await CommonFetchHistoryWithAsyncShouldReturnEncryptedMessageBasedOnParams("", "enigma", false)).Result.Result;
#else
            bool receivedMessage = await CommonFetchHistoryWithAsyncShouldReturnEncryptedMessageBasedOnParams("", "enigma", false);
#endif
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnEncrypedMessage - Fetch History Result not expected");
        }


        [Test]
        public static void FetchHistoryShouldReturnEncrypedSecretMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;
            CommonFetchHistoryShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnEncrypedSecretMessage - Fetch History Result not expected");
        }
        
        [Test]
#if NET40
        public static void FetchHistoryWithAsyncShouldReturnEncrypedSecretMessage()
#else
        public static async Task FetchHistoryWithAsyncShouldReturnEncrypedSecretMessage()
#endif
        {
            server.ClearRequests();
#if NET40
            bool receivedMessage = Task.Factory.StartNew(async () => await CommonFetchHistoryWithAsyncShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false)).Result.Result;
#else
            bool receivedMessage = await CommonFetchHistoryWithAsyncShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
#endif
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnEncrypedSecretMessage - Fetch History Result not expected");
        }

        [Test]
        public static void FetchHistoryShouldReturnEncrypedSecretSSLMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;
            CommonFetchHistoryShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnEncrypedSecretSSLMessage - Fetch History Result not expected");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncShouldReturnEncrypedSecretSSLMessage()
#else
        public static async Task FetchHistoryWithAsyncShouldReturnEncrypedSecretSSLMessage()
#endif
        {
            server.ClearRequests();
#if NET40
            bool receivedMessage = Task.Factory.StartNew(async () => await CommonFetchHistoryWithAsyncShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true)).Result.Result;
#else
            bool receivedMessage = await CommonFetchHistoryWithAsyncShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
#endif
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnEncrypedSecretSSLMessage - Fetch History Result not expected");
        }


        [Test]
        public static void FetchHistoryShouldReturnEncrypedSSLMessage()
        {
            server.ClearRequests();
            bool receivedMessage = false;
            CommonFetchHistoryShouldReturnEncryptedMessageBasedOnParams("", "enigma", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnEncrypedSSLMessage - Fetch History Result not expected");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncShouldReturnEncrypedSSLMessage()
#else
        public static async Task FetchHistoryWithAsyncShouldReturnEncrypedSSLMessage()
#endif
        {
            server.ClearRequests();
            if (PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("Ignored for Server side run");
                return;
            }

#if NET40
            bool receivedMessage = Task.Factory.StartNew(async () => await CommonFetchHistoryWithAsyncShouldReturnEncryptedMessageBasedOnParams("", "enigma", true)).Result.Result;
#else
            bool receivedMessage = await CommonFetchHistoryWithAsyncShouldReturnEncryptedMessageBasedOnParams("", "enigma", true);
#endif
            Assert.IsTrue(receivedMessage, "FetchHistoryShouldReturnEncrypedSSLMessage - Fetch History Result not expected");
        }

        private static void CommonFetchHistoryShouldReturnEncryptedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl, out bool outReceivedMessage)
        {
            server.ClearRequests();
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }

            bool receivedMessage = false;
            int totalMessages = 10;
            long starttime = 0;
            long midtime = 0;
            long endtime = 0;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = cipherKey,
                Secure = ssl
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = secretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(ssl);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";

            long currentTimetoken = 0;

            string expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent timeManualEvent = new ManualResetEvent(false);
            pubnub.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                    currentTimetoken = (r != null && s.StatusCode == 200 && s.Error == false) ? r.Timetoken : 0;
                }
                catch { /* ignore */ }
                finally { timeManualEvent.Set(); }
            }));
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            starttime = currentTimetoken;

            System.Diagnostics.Debug.WriteLine(string.Format("Start Time = {0}", starttime));
            List<string> firstPublishSet = new List<string>();

            for (int index = 0; index < totalMessages / 2; index++)
            {
                object message = string.Format("Set1-{0}",index);
                firstPublishSet.Add(string.Format("Set1-{0}", index));

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, index))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(channel).Message(message).ShouldStore(true)
                    .Execute(new PNPublishResultExt((r, s) => {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            receivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                System.Diagnostics.Debug.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            timeManualEvent = new ManualResetEvent(false);
            pubnub.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                    currentTimetoken = (r != null && s.StatusCode == 200 && s.Error == false) ? r.Timetoken : 0;
                }
                catch { /* ignore */ }
                finally { timeManualEvent.Set(); }
            }));
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            midtime = currentTimetoken;

            System.Diagnostics.Debug.WriteLine(string.Format("Mid Time = {0}", midtime));
            List<string> secondPublishSet = new List<string>();
            int arrayIndex = 0;

            for (int index = totalMessages / 2; index < totalMessages; index++)
            {
                receivedMessage = false;

                object message = string.Format("Set2-{0}", (double)index + 0.1D);
                secondPublishSet.Add(string.Format("Set2-{0}", (double)index + 0.1D));
                arrayIndex++;

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, message))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(channel).Message(message).ShouldStore(true)
                    .Execute(new PNPublishResultExt((r, s) => {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            receivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                System.Diagnostics.Debug.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            timeManualEvent = new ManualResetEvent(false);
            pubnub.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                    currentTimetoken = (r != null && s.StatusCode == 200 && s.Error == false) ? r.Timetoken : 0;
                }
                catch { /* ignore */ }
                finally { timeManualEvent.Set(); }
            }));
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            endtime = currentTimetoken;

            System.Diagnostics.Debug.WriteLine(string.Format("End Time = {0}", endtime));

            Thread.Sleep(1000);

            System.Diagnostics.Debug.WriteLine("Detailed History with Start & End");

            expected = "[[\"kvIeHmojsLyV1KMBo82DYQ==\",\"Ld0rZfbe4yN0Qj4V7o2BuQ==\",\"zNlnhYco9o6a646+Ox6ksg==\",\"mR8EEMx154BBHU3OOa+YjQ==\",\"v+viLoq0Gj2docUMAYyoYg==\"],14835539837820376,14835539843298232]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            List<object> historyMessageList = new List<object>();
            ManualResetEvent historyManualEvent = new ManualResetEvent(false);
            pubnub.FetchHistory().Channels(new string[] {channel })
                .Start(starttime)
                .End(midtime)
                .MaximumPerChannel(totalMessages / 2)
                .Reverse(true)
                .IncludeMeta(false)
                .Execute(new PNFetchHistoryResultExt((r, s) => {
                    historyMessageList = new List<object>();
                    if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in r.Messages)
                        {
                            List<PNHistoryItemResult> itemList = channelItem.Value;
                            foreach (PNHistoryItemResult item in itemList)
                            {
                                if (item.Entry != null)
                                {
                                    historyMessageList.Add(item.Entry);
                                }
                            }
                        }
                    }
                    historyManualEvent.Set();
                }));
            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            foreach (object item in historyMessageList)
            {
                if (!firstPublishSet.Contains(item.ToString()))
                {
                    receivedMessage = false;
                    break;
                }
                receivedMessage = true;
            }

            if (!receivedMessage)
            {
                System.Diagnostics.Debug.WriteLine("firstPublishSet did not match");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("firstPublishSet SUCCESS. All published messages received.");
                System.Diagnostics.Debug.WriteLine("FetchHistory with start & reverse = true");

                expected = "[[\"F2ZPfJnzuU34VKe24ds81A==\",\"2K/TO5WADvJRhvX7Zk0IpQ==\",\"oWOYyGxkWFJ1gpJxhcyzjA==\",\"LwEzvPCHdM8Yagg6oKknvg==\",\"/jjH/PT4NrK5HHjDT2KAlQ==\"],14835549524365492,14835549537755368]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                historyManualEvent = new ManualResetEvent(false);
                pubnub.FetchHistory().Channels(new string[] {channel })
                    .Start(midtime)
                    .End(endtime)
                    .MaximumPerChannel(totalMessages / 2)
                    .Reverse(true)
                    .IncludeMeta(false)
                    .Execute(new PNFetchHistoryResultExt((r, s) => {
                        historyMessageList = new List<object>();
                        if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                            foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in r.Messages)
                            {
                                List<PNHistoryItemResult> itemList = channelItem.Value;
                                foreach (PNHistoryItemResult item in itemList)
                                {
                                    if (item.Entry != null)
                                    {
                                        historyMessageList.Add(item.Entry);
                                    }
                                }
                            }
                        }
                        historyManualEvent.Set();
                    }));
                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                foreach (object item in historyMessageList)
                {
                    if (!secondPublishSet.Contains(item.ToString()))
                    {
                        receivedMessage = false;
                        break;
                    }
                    receivedMessage = true;
                }

                if (!receivedMessage)
                {
                    System.Diagnostics.Debug.WriteLine("secondPublishSet did not match");
                }
                else
                {
                    Debug.WriteLine("FetchHistory with start & reverse = false");
                    expected = "[[\"kvIeHmojsLyV1KMBo82DYQ==\",\"Ld0rZfbe4yN0Qj4V7o2BuQ==\",\"zNlnhYco9o6a646+Ox6ksg==\",\"mR8EEMx154BBHU3OOa+YjQ==\",\"v+viLoq0Gj2docUMAYyoYg==\"],14835550731714499,14835550737165103]";

                    server.AddRequest(new Request()
                            .WithMethod("GET")
                            .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                            .WithResponse(expected)
                            .WithStatusCode(System.Net.HttpStatusCode.OK));

                    historyManualEvent = new ManualResetEvent(false);
                    pubnub.FetchHistory().Channels(new string[] {channel })
                        .Start(midtime - 1)
                        .MaximumPerChannel(totalMessages / 2)
                        .Reverse(false)
                        .IncludeMeta(false)
                        .Execute(new PNFetchHistoryResultExt((r, s) => {
                            historyMessageList = new List<object>();
                            if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count > 0)
                            {
                                foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in r.Messages)
                                {
                                    List<PNHistoryItemResult> itemList = channelItem.Value;
                                    foreach (PNHistoryItemResult item in itemList)
                                    {
                                        if (item.Entry != null)
                                        {
                                            historyMessageList.Add(item.Entry);
                                        }
                                    }
                                }
                            }
                            historyManualEvent.Set();
                        }));
                    historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                    foreach (object item in historyMessageList)
                    {
                        if (!firstPublishSet.Contains(item.ToString()))
                        {
                            receivedMessage = false;
                            break;
                        }
                        receivedMessage = true;
                    }
                }
            }

            outReceivedMessage = receivedMessage;
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        private static async Task<bool> CommonFetchHistoryWithAsyncShouldReturnEncryptedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            server.ClearRequests();
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }

            bool receivedMessage = false;
            int totalMessages = 10;
            long starttime = 0;
            long midtime = 0;
            long endtime = 0;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = cipherKey,
                Secure = ssl
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = secretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(ssl);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";

            long currentTimetoken = 0;

            string expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            PNResult<PNTimeResult> timeResult = await pubnub.Time().ExecuteAsync();
            try
            {
                Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(timeResult.Result));
                currentTimetoken = (timeResult.Result != null && timeResult.Status.StatusCode == 200 && !timeResult.Status.Error) ? timeResult.Result.Timetoken : 0;
            }
            catch { /* ignore */ }

            starttime = currentTimetoken;

            System.Diagnostics.Debug.WriteLine(string.Format("Start Time = {0}", starttime));
            List<string> firstPublishSet = new List<string>();

            for (int index = 0; index < totalMessages / 2; index++)
            {
                object message = string.Format("Set1-{0}", index);
                firstPublishSet.Add(string.Format("Set1-{0}", index));

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, index))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                PNResult<PNPublishResult> publishResult = await pubnub.Publish().Channel(channel).Message(message).ShouldStore(true)
                    .ExecuteAsync();
                if (publishResult.Result != null && publishResult.Status.StatusCode == 200 && !publishResult.Status.Error)
                {
                    receivedMessage = true;
                }

                System.Diagnostics.Debug.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }
            Thread.Sleep(2000);

            currentTimetoken = 0;

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            timeResult = await pubnub.Time().ExecuteAsync();
            try
            {
                Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(timeResult.Result));
                currentTimetoken = (timeResult.Result != null && timeResult.Status.StatusCode == 200 && !timeResult.Status.Error) ? timeResult.Result.Timetoken : 0;
            }
            catch { /* ignore */ }

            midtime = currentTimetoken;

            System.Diagnostics.Debug.WriteLine(string.Format("Mid Time = {0}", midtime));
            List<string> secondPublishSet = new List<string>();
            int arrayIndex = 0;

            for (int index = totalMessages / 2; index < totalMessages; index++)
            {
                receivedMessage = false;

                object message = string.Format("Set2-{0}", (double)index + 0.1D);
                secondPublishSet.Add(string.Format("Set2-{0}", (double)index + 0.1D));
                arrayIndex++;

                //manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, message))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                PNResult<PNPublishResult> publishResult = await pubnub.Publish().Channel(channel).Message(message).ShouldStore(true)
                    .ExecuteAsync();
                if (publishResult.Result != null && publishResult.Status.StatusCode == 200 && !publishResult.Status.Error)
                {
                    receivedMessage = true;
                }

                System.Diagnostics.Debug.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }
            Thread.Sleep(3000);
            currentTimetoken = 0;

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            timeResult = await pubnub.Time().ExecuteAsync();
            try
            {
                Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(timeResult.Result));
                currentTimetoken = (timeResult.Result != null && timeResult.Status.StatusCode == 200 && !timeResult.Status.Error) ? timeResult.Result.Timetoken : 0;
            }
            catch { /* ignore */ }

            endtime = currentTimetoken;

            System.Diagnostics.Debug.WriteLine(string.Format("End Time = {0}", endtime));

            Thread.Sleep(1000);

            System.Diagnostics.Debug.WriteLine("Detailed History with Start & End");

            expected = "[[\"kvIeHmojsLyV1KMBo82DYQ==\",\"Ld0rZfbe4yN0Qj4V7o2BuQ==\",\"zNlnhYco9o6a646+Ox6ksg==\",\"mR8EEMx154BBHU3OOa+YjQ==\",\"v+viLoq0Gj2docUMAYyoYg==\"],14835539837820376,14835539843298232]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            List<object> historyMessageList = new List<object>();
            PNResult<PNFetchHistoryResult> fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                .Start(starttime)
                .End(midtime)
                .MaximumPerChannel(totalMessages / 2)
                .Reverse(true)
                .IncludeMeta(false)
                .ExecuteAsync();
            if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error
                && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel)
                && fetchHistResult.Result.Messages[channel].Count > 0)
            {
                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fetchHistResult.Result));
                foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in fetchHistResult.Result.Messages)
                {
                    List<PNHistoryItemResult> itemList = channelItem.Value;
                    foreach (PNHistoryItemResult item in itemList)
                    {
                        if (item.Entry != null)
                        {
                            historyMessageList.Add(item.Entry);
                        }
                    }
                }
            }

            foreach (object item in historyMessageList)
            {
                if (!firstPublishSet.Contains(item.ToString()))
                {
                    receivedMessage = false;
                    break;
                }
                receivedMessage = true;
            }

            if (!receivedMessage)
            {
                System.Diagnostics.Debug.WriteLine("firstPublishSet did not match");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("firstPublishSet SUCCESS. All published messages received.");
                System.Diagnostics.Debug.WriteLine("FetchHistory with start & reverse = true");

                expected = "[[\"F2ZPfJnzuU34VKe24ds81A==\",\"2K/TO5WADvJRhvX7Zk0IpQ==\",\"oWOYyGxkWFJ1gpJxhcyzjA==\",\"LwEzvPCHdM8Yagg6oKknvg==\",\"/jjH/PT4NrK5HHjDT2KAlQ==\"],14835549524365492,14835549537755368]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                historyMessageList = new List<object>();
                fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                    .Start(midtime)
                    .End(endtime)
                    .MaximumPerChannel(totalMessages / 2)
                    .Reverse(true)
                    .IncludeMeta(false)
                    .ExecuteAsync();
                if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error
                    && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel)
                    && fetchHistResult.Result.Messages[channel].Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fetchHistResult.Result));
                    foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in fetchHistResult.Result.Messages)
                    {
                        List<PNHistoryItemResult> itemList = channelItem.Value;
                        foreach (PNHistoryItemResult item in itemList)
                        {
                            if (item.Entry != null)
                            {
                                historyMessageList.Add(item.Entry);
                            }
                        }
                    }
                }

                foreach (object item in historyMessageList)
                {
                    if (!secondPublishSet.Contains(item.ToString()))
                    {
                        receivedMessage = false;
                        break;
                    }
                    receivedMessage = true;
                }

                if (!receivedMessage)
                {
                    System.Diagnostics.Debug.WriteLine("secondPublishSet did not match");
                }
                else
                {
                    Debug.WriteLine("FetchHistory with start & reverse = false");
                    expected = "[[\"kvIeHmojsLyV1KMBo82DYQ==\",\"Ld0rZfbe4yN0Qj4V7o2BuQ==\",\"zNlnhYco9o6a646+Ox6ksg==\",\"mR8EEMx154BBHU3OOa+YjQ==\",\"v+viLoq0Gj2docUMAYyoYg==\"],14835550731714499,14835550737165103]";

                    server.AddRequest(new Request()
                            .WithMethod("GET")
                            .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                            .WithResponse(expected)
                            .WithStatusCode(System.Net.HttpStatusCode.OK));

                    historyMessageList = new List<object>();
                    fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                        .Start(midtime - 1)
                        .MaximumPerChannel(totalMessages / 2)
                        .Reverse(false)
                        .IncludeMeta(false)
                        .ExecuteAsync();
                    if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error
                        && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel)
                        && fetchHistResult.Result.Messages[channel].Count > 0)
                    {
                        foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in fetchHistResult.Result.Messages)
                        {
                            List<PNHistoryItemResult> itemList = channelItem.Value;
                            foreach (PNHistoryItemResult item in itemList)
                            {
                                if (item.Entry != null)
                                {
                                    historyMessageList.Add(item.Entry);
                                }
                            }
                        }
                    }

                    foreach (object item in historyMessageList)
                    {
                        if (!firstPublishSet.Contains(item.ToString()))
                        {
                            receivedMessage = false;
                            break;
                        }
                        receivedMessage = true;
                    }
                }
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            return receivedMessage;
        }


        private static void CommonFetchHistoryShouldReturnUnencryptedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl, out bool outReceivedMessage)
        {
            bool receivedMessage = false;
            int totalMessages = 10;
            long starttime = 0;
            long midtime = 0;
            long endtime = 0;
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = cipherKey,
                Secure = ssl
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = secretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(ssl);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";

            long currentTimetoken = 0;

            string expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent timeManualEvent = new ManualResetEvent(false);
            pubnub.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                    currentTimetoken = (r != null && s.StatusCode == 200 && s.Error == false) ? r.Timetoken : 0;
                }
                catch { /* ignore */ }
                finally { timeManualEvent.Set(); }
            }));
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            starttime = currentTimetoken;

            Debug.WriteLine(string.Format("Start Time = {0}", starttime));
            List<int> firstPublishSet = new List<int>();

            for (int index = 0; index < totalMessages / 2; index++)
            {
                receivedMessage = false;

                object message = index;
                firstPublishSet.Add(index);

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, index))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(channel).Message(message).ShouldStore(true)
                    .Execute(new PNPublishResultExt((r, s) => {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            receivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                Debug.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            timeManualEvent = new ManualResetEvent(false);
            pubnub.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                    currentTimetoken = (r != null && s.StatusCode == 200 && s.Error == false) ? r.Timetoken : 0;
                }
                catch { /* ignore */ }
                finally { timeManualEvent.Set(); }
            }));
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            midtime = currentTimetoken;

            Debug.WriteLine(string.Format("Mid Time = {0}", midtime));
            List<double> secondPublishSet = new List<double>();
            int arrayIndex = 0;

            for (int index = totalMessages / 2; index < totalMessages; index++)
            {
                receivedMessage = false;

                object message = (double)index + 0.1D;
                secondPublishSet.Add((double)index + 0.1D);
                arrayIndex++;

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, message))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(channel).Message(message).ShouldStore(true)
                    .Execute(new PNPublishResultExt((r, s) => {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            receivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                Debug.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            timeManualEvent = new ManualResetEvent(false);

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            timeManualEvent = new ManualResetEvent(false);
            pubnub.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                    currentTimetoken = (r != null && s.StatusCode == 200 && s.Error == false) ? r.Timetoken : 0;
                }
                catch { /* ignore */ }
                finally { timeManualEvent.Set(); }
            }));
            timeManualEvent.WaitOne(manualResetEventWaitTimeout);

            endtime = currentTimetoken;

            Debug.WriteLine(string.Format("End Time = {0}", endtime));

            Thread.Sleep(1000);

            Debug.WriteLine("Detailed History with Start & End");

            expected = "[[0,1,2,3,4],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            List<object> historyMessageList = new List<object>();
            ManualResetEvent historyManualEvent = new ManualResetEvent(false);
            pubnub.FetchHistory().Channels(new string[] {channel })
                .Start(starttime)
                .End(midtime)
                .MaximumPerChannel(totalMessages / 2)
                .Reverse(true)
                .IncludeMeta(false)
                .Execute(new PNFetchHistoryResultExt((r, s) => {
                    historyMessageList = new List<object>();
                    if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count > 0)
                    {
                        foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in r.Messages)
                        {
                            List<PNHistoryItemResult> itemList = channelItem.Value;
                            foreach (PNHistoryItemResult item in itemList)
                            {
                                if (item.Entry != null)
                                {
                                    historyMessageList.Add(item.Entry);
                                }
                            }
                        }
                    }
                    historyManualEvent.Set();
                }));
            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            foreach (object item in historyMessageList)
            {
                int num;
                if (int.TryParse(item.ToString(), out num))
                {
                    if (!firstPublishSet.Contains(num))
                    {
                        receivedMessage = false;
                        break;
                    }
                    receivedMessage = true;
                }
            }

            if (!receivedMessage)
            {
                Debug.WriteLine("firstPublishSet did not match");
            }
            else
            {
                Debug.WriteLine("FetchHistory with start & reverse = true");


                expected = "[[5.1,6.1,7.1,8.1,9.1],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                historyManualEvent = new ManualResetEvent(false);
                pubnub.FetchHistory().Channels(new string[] {channel })
                    .Start(midtime - 1)
                    .End(endtime)
                    .MaximumPerChannel(totalMessages / 2)
                    .Reverse(true)
                    .IncludeMeta(false)
                    .Execute(new PNFetchHistoryResultExt((r, s) => {
                        historyMessageList = new List<object>();
                        if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count > 0)
                        {
                            foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in r.Messages)
                            {
                                List<PNHistoryItemResult> itemList = channelItem.Value;
                                foreach (PNHistoryItemResult item in itemList)
                                {
                                    if (item.Entry != null)
                                    {
                                        historyMessageList.Add(item.Entry);
                                    }
                                }
                            }
                        }
                        historyManualEvent.Set();
                    }));
                historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                foreach (object item in historyMessageList)
                {
                    int num;
                    if (int.TryParse(item.ToString(), out num))
                    {
                        if (!secondPublishSet.Contains(num))
                        {
                            receivedMessage = false;
                            break;
                        }
                        receivedMessage = true;
                    }
                }

                if (!receivedMessage)
                {
                    Debug.WriteLine("secondPublishSet did not match");
                }
                else
                {
                    Debug.WriteLine("DetailedHistory with start & reverse = false");
                    expected = "[[5.1,6.1,7.1,8.1,9.1],14715432709547189,14715432709547189]";

                    server.AddRequest(new Request()
                            .WithMethod("GET")
                            .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                            .WithResponse(expected)
                            .WithStatusCode(System.Net.HttpStatusCode.OK));

                    historyManualEvent = new ManualResetEvent(false);
                    pubnub.FetchHistory().Channels(new string[] {channel })
                        .Start(midtime - 1)
                        .MaximumPerChannel(totalMessages / 2)
                        .Reverse(false)
                        .IncludeMeta(false)
                        .Execute(new PNFetchHistoryResultExt((r, s) => {
                            historyMessageList = new List<object>();
                            if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count > 0)
                            {
                                foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in r.Messages)
                                {
                                    List<PNHistoryItemResult> itemList = channelItem.Value;
                                    foreach (PNHistoryItemResult item in itemList)
                                    {
                                        if (item.Entry != null)
                                        {
                                            historyMessageList.Add(item.Entry);
                                        }
                                    }
                                }
                            }
                            historyManualEvent.Set();
                        }));
                    historyManualEvent.WaitOne(manualResetEventWaitTimeout);

                    foreach (object item in historyMessageList)
                    {
                        int num;
                        if (int.TryParse(item.ToString(), out num))
                        {
                            if (!firstPublishSet.Contains(num))
                            {
                                receivedMessage = false;
                                break;
                            }
                            receivedMessage = true;
                        }
                    }
                }

            }

            outReceivedMessage = receivedMessage;
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        private static async Task<bool> CommonFetchHistoryWithAsyncShouldReturnUnencryptedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            bool receivedMessage = false;
            int totalMessages = 10;
            long starttime = 0;
            long midtime = 0;
            long endtime = 0;
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = cipherKey,
                Secure = ssl
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = secretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(ssl);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";

            long currentTimetoken = 0;

            string expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            PNResult<PNTimeResult> timeResult = await pubnub.Time().ExecuteAsync();
            try
            {
                Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(timeResult.Result));
                currentTimetoken = (timeResult.Result != null && timeResult.Status.StatusCode == 200 && !timeResult.Status.Error) ? timeResult.Result.Timetoken : 0;
            }
            catch { /* ignore */ }

            starttime = currentTimetoken;

            Debug.WriteLine(string.Format("Start Time = {0}", starttime));
            List<int> firstPublishSet = new List<int>();

            for (int index = 0; index < totalMessages / 2; index++)
            {
                receivedMessage = false;

                object message = index;
                firstPublishSet.Add(index);

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, index))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                PNResult<PNPublishResult> publishResult = await pubnub.Publish().Channel(channel).Message(message).ShouldStore(true).ExecuteAsync();
                if (publishResult.Result != null && publishResult.Status.StatusCode == 200 && !publishResult.Status.Error)
                {
                    receivedMessage = true;
                }

                Debug.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            timeResult = await pubnub.Time().ExecuteAsync();
            try
            {
                Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(timeResult.Result));
                currentTimetoken = (timeResult.Result != null && timeResult.Status.StatusCode == 200 && !timeResult.Status.Error) ? timeResult.Result.Timetoken : 0;
            }
            catch { /* ignore */ }

            midtime = currentTimetoken;

            Debug.WriteLine(string.Format("Mid Time = {0}", midtime));
            List<double> secondPublishSet = new List<double>();
            int arrayIndex = 0;

            for (int index = totalMessages / 2; index < totalMessages; index++)
            {
                receivedMessage = false;

                object message = (double)index + 0.1D;
                secondPublishSet.Add((double)index + 0.1D);
                arrayIndex++;

                manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 2000 : 310 * 1000;

                expected = "[1,\"Sent\",\"14715322883933786\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/{3}", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel, message))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                PNResult<PNPublishResult> publishResult = await pubnub.Publish().Channel(channel).Message(message).ShouldStore(true).ExecuteAsync();
                if (publishResult.Result != null && publishResult.Status.StatusCode == 200 && !publishResult.Status.Error)
                {
                    receivedMessage = true;
                }

                Debug.WriteLine(string.Format("Message #{0} publish {1}", index, (receivedMessage) ? "SUCCESS" : "FAILED"));
            }

            currentTimetoken = 0;

            expected = "[1356998400]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            timeResult = await pubnub.Time().ExecuteAsync();
            try
            {
                Debug.WriteLine("result={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(timeResult.Result));
                currentTimetoken = (timeResult.Result != null && timeResult.Status.StatusCode == 200 && !timeResult.Status.Error) ? timeResult.Result.Timetoken : 0;
            }
            catch { /* ignore */ }

            endtime = currentTimetoken;

            Debug.WriteLine(string.Format("End Time = {0}", endtime));

            Thread.Sleep(1000);

            Debug.WriteLine("Detailed History with Start & End");

            expected = "[[0,1,2,3,4],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            List<object> historyMessageList;
            PNResult<PNFetchHistoryResult> fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                .Start(starttime)
                .End(midtime)
                .MaximumPerChannel(totalMessages / 2)
                .Reverse(true)
                .IncludeMeta(false)
                .ExecuteAsync();
            historyMessageList = new List<object>();
            if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error 
                && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel) 
                && fetchHistResult.Result.Messages[channel].Count > 0)
            {
                foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in fetchHistResult.Result.Messages)
                {
                    List<PNHistoryItemResult> itemList = channelItem.Value;
                    foreach (PNHistoryItemResult item in itemList)
                    {
                        if (item.Entry != null)
                        {
                            historyMessageList.Add(item.Entry);
                        }
                    }
                }
            }

            foreach (object item in historyMessageList)
            {
                int num;
                if (int.TryParse(item.ToString(), out num))
                {
                    if (!firstPublishSet.Contains(num))
                    {
                        receivedMessage = false;
                        break;
                    }
                    receivedMessage = true;
                }
            }

            if (!receivedMessage)
            {
                Debug.WriteLine("firstPublishSet did not match");
            }
            else
            {
                Debug.WriteLine("FetchHistory with start & reverse = true");


                expected = "[[5.1,6.1,7.1,8.1,9.1],14715432709547189,14715432709547189]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                    .Start(midtime - 1)
                    .End(endtime)
                    .MaximumPerChannel(totalMessages / 2)
                    .Reverse(true)
                    .IncludeMeta(false)
                    .ExecuteAsync();
                historyMessageList = new List<object>();
                if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error 
                    && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel) 
                    && fetchHistResult.Result.Messages[channel].Count > 0)
                {
                    foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in fetchHistResult.Result.Messages)
                    {
                        List<PNHistoryItemResult> itemList = channelItem.Value;
                        foreach (PNHistoryItemResult item in itemList)
                        {
                            if (item.Entry != null)
                            {
                                historyMessageList.Add(item.Entry);
                            }
                        }
                    }
                }

                foreach (object item in historyMessageList)
                {
                    int num;
                    if (int.TryParse(item.ToString(), out num))
                    {
                        if (!secondPublishSet.Contains(num))
                        {
                            receivedMessage = false;
                            break;
                        }
                        receivedMessage = true;
                    }
                }

                if (!receivedMessage)
                {
                    Debug.WriteLine("secondPublishSet did not match");
                }
                else
                {
                    Debug.WriteLine("DetailedHistory with start & reverse = false");
                    expected = "[[5.1,6.1,7.1,8.1,9.1],14715432709547189,14715432709547189]";

                    server.AddRequest(new Request()
                            .WithMethod("GET")
                            .WithPath(String.Format("/v2/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                            .WithResponse(expected)
                            .WithStatusCode(System.Net.HttpStatusCode.OK));

                    fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                        .Start(midtime - 1)
                        .MaximumPerChannel(totalMessages / 2)
                        .Reverse(false)
                        .IncludeMeta(false)
                        .ExecuteAsync();
                    historyMessageList = new List<object>();
                    if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error 
                        && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel) 
                        && fetchHistResult.Result.Messages[channel].Count > 0)
                    {
                        foreach (KeyValuePair<string, List<PNHistoryItemResult>> channelItem in fetchHistResult.Result.Messages)
                        {
                            List<PNHistoryItemResult> itemList = channelItem.Value;
                            foreach (PNHistoryItemResult item in itemList)
                            {
                                if (item.Entry != null)
                                {
                                    historyMessageList.Add(item.Entry);
                                }
                            }
                        }
                    }

                    foreach (object item in historyMessageList)
                    {
                        int num;
                        if (int.TryParse(item.ToString(), out num))
                        {
                            if (!firstPublishSet.Contains(num))
                            {
                                receivedMessage = false;
                                break;
                            }
                            receivedMessage = true;
                        }
                    }
                }

            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            return receivedMessage;
        }

        [Test]
        public static void FetchHistoryDefaultMax100()
        {
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun) {
                config.SecretKey = PubnubCommon.SecretKey;
            } else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey) {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "{ \"status\":200, \"error\":false, \"error_message\":\"\", \"channels\":{ \"my_channel\":[ { \"message\":\"Hello world\", \"timetoken\":15610547826970040, \"uuid\":\"my-uuid\", \"message_type\":1, \"meta\":\"\" } ] } }";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v3/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("max", "100")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent historyManualEvent = new ManualResetEvent(false);
            pubnub.FetchHistory().Channels(new string[] { channel })
                .IncludeMeta(false)
                .Execute(new PNFetchHistoryResultExt((r, s) => {
                    if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count >= 10) {
                        System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    historyManualEvent.Set();
                }));
            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History Failed");
        }

        [Test]
#if NET40
        public static void FetchHistoryAsyncDefaultMax100()
#else
        public static async Task FetchHistoryAsyncDefaultMax100()
#endif
        {
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun) {
                config.SecretKey = PubnubCommon.SecretKey;
            } else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey) {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v3/history/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("max", "100")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNFetchHistoryResult> fetchHistResult = Task.Factory.StartNew(async () => await pubnub.FetchHistory().Channels(new string[] { channel })
                .IncludeMeta(false)
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNFetchHistoryResult> fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                .IncludeMeta(false)
                .ExecuteAsync();
#endif
            if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error
                && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel)
                && fetchHistResult.Result.Messages[channel].Count >= 100) {
                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fetchHistResult.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History Failed");
        }

        [Test]
        public static void FetchHistoryWithMessageActionsDefaultMax25()
        {
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun) {
                config.SecretKey = PubnubCommon.SecretKey;
            } else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey) {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v3/history-with-actions/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("max", "25")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent historyManualEvent = new ManualResetEvent(false);
            pubnub.FetchHistory().Channels(new string[] { channel })
                .IncludeMeta(false)
                .IncludeMessageActions(true)
                .Execute(new PNFetchHistoryResultExt((r, s) => {
                    if (r != null && s.StatusCode == 200 && !s.Error && r.Messages != null && r.Messages.ContainsKey(channel) && r.Messages[channel].Count >= 25) {
                        System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    historyManualEvent.Set();
                }));
            historyManualEvent.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History with message actions Failed");
        }

        [Test]
#if NET40
        public static void FetchHistoryWithAsyncWithMessageActionsDefaultMax25()
#else
        public static async Task FetchHistoryWithAsyncWithMessageActionsDefaultMax25()
#endif
        {
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun) {
                config.SecretKey = PubnubCommon.SecretKey;
            } else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey) {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "[[\"Pubnub Messaging API 1\",\"Pubnub Messaging API 2\",\"Pubnub Messaging API 3\",\"Pubnub Messaging API 4\",\"Pubnub Messaging API 5\",\"Pubnub Messaging API 6\",\"Pubnub Messaging API 7\",\"Pubnub Messaging API 8\",\"Pubnub Messaging API 9\",\"Pubnub Messaging API 10\"],14715432709547189,14715432709547189]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v3/history-with-actions/sub-key/{0}/channel/{1}", PubnubCommon.SubscribeKey, channel))
                    .WithParameter("max", "25")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            PNResult<PNFetchHistoryResult> fetchHistResult = Task.Factory.StartNew(async () => await pubnub.FetchHistory().Channels(new string[] { channel })
                .IncludeMeta(false)
                .IncludeMessageActions(true)
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNFetchHistoryResult> fetchHistResult = await pubnub.FetchHistory().Channels(new string[] { channel })
                .IncludeMeta(false)
                .IncludeMessageActions(true)
                .ExecuteAsync();
#endif
            if (fetchHistResult.Result != null && fetchHistResult.Status.StatusCode == 200 && !fetchHistResult.Status.Error
                && fetchHistResult.Result.Messages != null && fetchHistResult.Result.Messages.ContainsKey(channel)
                && fetchHistResult.Result.Messages[channel].Count >= 25) {
                System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fetchHistResult.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "Fetch History with Message Actions Async Failed");
        }
    }
}
