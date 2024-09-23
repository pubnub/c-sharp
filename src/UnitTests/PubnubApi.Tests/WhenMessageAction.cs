using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenMessageAction : TestHarness
    {
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
            if (PubnubCommon.EnableStubTest)
            {
                server.Start();   
            }

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
                    .WithParameter("uuid", config.UserId)
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
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }
            server.Stop();
        }

        [Test]
        public static void ThenAddMessageActionReturnsSuccess()
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
            long currentMessageTimetoken = new Random().Next(Int32.MaxValue);
            long currentActionTimetoken = 0;
            string currentUUID = "";

            ManualResetEvent me = new ManualResetEvent(false);
            System.Diagnostics.Debug.WriteLine("GetMessageActions STARTED");
            pubnub.GetMessageActions()
                .Channel(channel)
                .Execute(new PNGetMessageActionsResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        System.Diagnostics.Debug.WriteLine("GetMessageActions = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));

                        if (r.MessageActions != null && r.MessageActions.Exists(x => x.MessageTimetoken == currentMessageTimetoken))
                        {
                            PNMessageActionItem actionItem = r.MessageActions.Find(x => x.MessageTimetoken == currentMessageTimetoken);
                            currentActionTimetoken = actionItem.ActionTimetoken;
                            currentUUID = actionItem.Uuid;
                        }
                    }
                    me.Set();
                }));
            me.WaitOne(manualResetEventWaitTimeout);

            if (currentMessageTimetoken > 0 && currentActionTimetoken > 0)
            {
                System.Diagnostics.Debug.WriteLine("RemoveMessageAction STARTED");
                me = new ManualResetEvent(false);

                pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .Execute(new PNRemoveMessageActionResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        System.Diagnostics.Debug.WriteLine("RemoveMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    me.Set();
                }));

                me.WaitOne(manualResetEventWaitTimeout);
            }

            System.Diagnostics.Debug.WriteLine("AddMessageAction STARTED");
            me = new ManualResetEvent(false);
            pubnub.AddMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .Action(new PNMessageAction { Type = "reaction", Value = "smily_face" })
                .Execute(new PNAddMessageActionResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error && r.MessageTimetoken == currentMessageTimetoken)
                    {
                        System.Diagnostics.Debug.WriteLine("AddMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    me.Set();
                }));
            me.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "AddRemoveMessageActionReturnEventInfo Failed");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncAddMessageActionReturnsSuccess()
#else
        public static async Task ThenWithAsyncAddMessageActionReturnsSuccess()
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
            long currentMessageTimetoken = new Random().Next(Int32.MaxValue);
            long currentActionTimetoken = 0;
            string currentUUID = "";

            System.Diagnostics.Debug.WriteLine("GetMessageActions STARTED");
#if NET40
            PNResult< PNGetMessageActionsResult> getMsgActionResult = Task.Factory.StartNew(async () => await pubnub.GetMessageActions().Channel(channel).ExecuteAsync()).Result.Result;
#else
            PNResult<PNGetMessageActionsResult> getMsgActionResult = await pubnub.GetMessageActions().Channel(channel).ExecuteAsync();
#endif
            if (getMsgActionResult.Result != null && getMsgActionResult.Status.StatusCode == 200 && !getMsgActionResult.Status.Error)
            {
                System.Diagnostics.Debug.WriteLine("GetMessageActions = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(getMsgActionResult.Result));

                if (getMsgActionResult.Result.MessageActions != null && getMsgActionResult.Result.MessageActions.Exists(x => x.MessageTimetoken == currentMessageTimetoken))
                {
                    PNMessageActionItem actionItem = getMsgActionResult.Result.MessageActions.Find(x => x.MessageTimetoken == currentMessageTimetoken);
                    currentActionTimetoken = actionItem.ActionTimetoken;
                    currentUUID = actionItem.Uuid;
                }
            }

            if (currentMessageTimetoken > 0 && currentActionTimetoken > 0)
            {
                System.Diagnostics.Debug.WriteLine("RemoveMessageAction STARTED");
#if NET40
                PNResult<PNRemoveMessageActionResult> removeMsgActionResult = Task.Factory.StartNew(async () => await pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .ExecuteAsync()).Result.Result;
#else
                PNResult<PNRemoveMessageActionResult> removeMsgActionResult = await pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .ExecuteAsync();
#endif
                if (removeMsgActionResult.Result != null && removeMsgActionResult.Status.StatusCode == 200 && !removeMsgActionResult.Status.Error)
                {
                    System.Diagnostics.Debug.WriteLine("RemoveMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(removeMsgActionResult.Result));
                    receivedMessage = true;
                }
            }

            System.Diagnostics.Debug.WriteLine("AddMessageAction STARTED");
#if NET40
            PNResult<PNAddMessageActionResult> addMsgActionResult = Task.Factory.StartNew(async () => await pubnub.AddMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .Action(new PNMessageAction { Type = "reaction", Value = "smily_face" })
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNAddMessageActionResult> addMsgActionResult = await pubnub.AddMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .Action(new PNMessageAction { Type = "reaction", Value = "smily_face" })
                .ExecuteAsync();
#endif
            if (addMsgActionResult.Result != null && addMsgActionResult.Status.StatusCode == 200 && !addMsgActionResult.Status.Error && addMsgActionResult.Result.MessageTimetoken == currentMessageTimetoken)
            {
                System.Diagnostics.Debug.WriteLine("AddMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(addMsgActionResult.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenWithAsyncAddMessageActionReturnsSuccess Failed");
        }

        [Test]
        public static void ThenRemoveMessageActionReturnsSuccess()
        {
            server.ClearRequests();

            bool receivedMessage = false;
            if (!PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("RemoveMessageActionReturnsSuccess needs Secret Key");
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

            server.RunOnHttps(false);
            if (PubnubCommon.PAMServerSideRun)
            {
                config.AuthKey = "myauth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            long currentMessageTimetoken = 0;
            long currentActionTimetoken = 0;
            string currentUUID = "";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            System.Diagnostics.Debug.WriteLine("GetMessageActions 1 STARTED");
            ManualResetEvent me = new ManualResetEvent(false);
            pubnub.GetMessageActions()
                .Channel(channel)
                .Limit(1)
                .Execute(new PNGetMessageActionsResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        System.Diagnostics.Debug.WriteLine("GetMessageActions = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));

                        if (r.MessageActions != null && r.MessageActions.Count > 0)
                        {
                            PNMessageActionItem actionItem = r.MessageActions[0];
                            currentMessageTimetoken = actionItem.MessageTimetoken;
                            currentActionTimetoken = actionItem.ActionTimetoken;
                            currentUUID = actionItem.Uuid;
                        }
                    }
                    me.Set();
                }));
            me.WaitOne(manualResetEventWaitTimeout);

            System.Diagnostics.Debug.WriteLine("RemoveMessageAction STARTED");
            me = new ManualResetEvent(false);
            pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .Execute(new PNRemoveMessageActionResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        System.Diagnostics.Debug.WriteLine("RemoveMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    me.Set();
                }));
            me.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "RemoveMessageActionReturnsSuccess Failed");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncRemoveMessageActionReturnsSuccess()
#else
        public static async Task ThenWithAsyncRemoveMessageActionReturnsSuccess()
#endif
        {
            server.ClearRequests();

            bool receivedMessage = false;
            if (!PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("ThenWithAsyncRemoveMessageActionReturnsSuccess needs Secret Key");
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

            server.RunOnHttps(false);
            if (PubnubCommon.PAMServerSideRun)
            {
                config.AuthKey = "myauth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            long currentMessageTimetoken = 0;
            long currentActionTimetoken = 0;
            string currentUUID = "";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            System.Diagnostics.Debug.WriteLine("GetMessageActions 1 STARTED");
#if NET40
            PNResult<PNGetMessageActionsResult> getMsgActionResult = Task.Factory.StartNew(async () => await pubnub.GetMessageActions().Channel(channel).Limit(1).ExecuteAsync()).Result.Result;
#else
            PNResult<PNGetMessageActionsResult> getMsgActionResult = await pubnub.GetMessageActions().Channel(channel).Limit(1).ExecuteAsync();
#endif
            if (getMsgActionResult.Result != null && getMsgActionResult.Status.StatusCode == 200 && !getMsgActionResult.Status.Error)
            {
                System.Diagnostics.Debug.WriteLine("GetMessageActions = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(getMsgActionResult.Result));

                if (getMsgActionResult.Result.MessageActions != null && getMsgActionResult.Result.MessageActions.Count > 0)
                {
                    PNMessageActionItem actionItem = getMsgActionResult.Result.MessageActions[0];
                    currentMessageTimetoken = actionItem.MessageTimetoken;
                    currentActionTimetoken = actionItem.ActionTimetoken;
                    currentUUID = actionItem.Uuid;
                }
            }

            System.Diagnostics.Debug.WriteLine("RemoveMessageAction STARTED");
#if NET40
            PNResult<PNRemoveMessageActionResult> removeMsgActionResult = Task.Factory.StartNew(async () => await pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNRemoveMessageActionResult> removeMsgActionResult = await pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .ExecuteAsync();
#endif
            if (removeMsgActionResult.Result != null && removeMsgActionResult.Status.StatusCode == 200 && !removeMsgActionResult.Status.Error)
            {
                System.Diagnostics.Debug.WriteLine("RemoveMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(removeMsgActionResult.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenWithAsyncRemoveMessageActionReturnsSuccess Failed");
        }

        [Test]
        public static void ThenGetMessageActionsReturnsSuccess()
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
                config.AuthKey = "myauth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            System.Diagnostics.Debug.WriteLine("GetMessageActions STARTED");
            ManualResetEvent me = new ManualResetEvent(false);
            pubnub.GetMessageActions()
                .Channel(channel)
                .Execute(new PNGetMessageActionsResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        System.Diagnostics.Debug.WriteLine("GetMessageActions = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    me.Set();
                }));
            me.WaitOne(manualResetEventWaitTimeout);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "GetMessageActionsReturnsSuccess Failed");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncGetMessageActionsReturnsSuccess()
#else
        public static async Task ThenWithAsyncGetMessageActionsReturnsSuccess()
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
                config.AuthKey = "myauth";
            }

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            System.Diagnostics.Debug.WriteLine("GetMessageActions STARTED");
#if NET40
            PNResult<PNGetMessageActionsResult> getMsgActionResult = Task.Factory.StartNew(async () => await pubnub.GetMessageActions().Channel(channel).ExecuteAsync()).Result.Result;
#else
            PNResult<PNGetMessageActionsResult> getMsgActionResult = await pubnub.GetMessageActions().Channel(channel).ExecuteAsync();
#endif
            if (getMsgActionResult.Result != null && getMsgActionResult.Status.StatusCode == 200 && !getMsgActionResult.Status.Error)
            {
                System.Diagnostics.Debug.WriteLine("GetMessageActions = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(getMsgActionResult.Result));
                receivedMessage = true;
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "ThenWithAsyncGetMessageActionsReturnsSuccess Failed");
        }

        [Test]
        public static void ThenAddRemoveMessageActionReturnEventInfo()
        {
            server.ClearRequests();
            if (!PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("AddRemoveMessageActionReturnEventInfo needs Secret Key");
                return;
            }

            bool receivedMessage = false;
            bool receivedAddEvent = false;
            bool receivedRemoveEvent = false;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageActionEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Event == "added")
                    {
                        receivedAddEvent = true;
                    }
                    else if (eventResult.Event == "removed")
                    {
                        receivedRemoveEvent = true;
                    }
                },
                delegate (Pubnub pnObj, PNStatus status)
                {

                }
                );

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
            pubnub.AddListener(eventListener);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            long currentMessageTimetoken = new Random().Next(Int32.MaxValue);
            long currentActionTimetoken = 0;
            string currentUUID = "";

            ManualResetEvent me = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            me.WaitOne(2000);

            me = new ManualResetEvent(false);
            System.Diagnostics.Debug.WriteLine("GetMessageActions STARTED");
            pubnub.GetMessageActions()
                .Channel(channel)
                .Execute(new PNGetMessageActionsResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        System.Diagnostics.Debug.WriteLine("GetMessageActions = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));

                        if (r.MessageActions != null && r.MessageActions.Exists(x => x.MessageTimetoken == currentMessageTimetoken))
                        {
                            PNMessageActionItem actionItem = r.MessageActions.Find(x => x.MessageTimetoken == currentMessageTimetoken);
                            currentActionTimetoken = actionItem.ActionTimetoken;
                            currentUUID = actionItem.Uuid;
                        }
                    }
                    me.Set();
                }));
            me.WaitOne(manualResetEventWaitTimeout);
            Thread.Sleep(2000);

            if (currentMessageTimetoken > 0 && currentActionTimetoken > 0)
            {
                System.Diagnostics.Debug.WriteLine("RemoveMessageAction STARTED");
                me = new ManualResetEvent(false);

                pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .Execute(new PNRemoveMessageActionResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        System.Diagnostics.Debug.WriteLine("RemoveMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    me.Set();
                }));

                me.WaitOne(manualResetEventWaitTimeout);
            }

            System.Diagnostics.Debug.WriteLine("AddMessageAction STARTED");
            me = new ManualResetEvent(false);
            pubnub.AddMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .Action(new PNMessageAction { Type = "reaction", Value = "smily_face" })
                .Execute(new PNAddMessageActionResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error && r.MessageTimetoken == currentMessageTimetoken)
                    {
                        System.Diagnostics.Debug.WriteLine("AddMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                        currentActionTimetoken = r.ActionTimetoken;
                        currentUUID = r.Uuid;
                    }
                    me.Set();
                }));
            me.WaitOne(manualResetEventWaitTimeout);
            Thread.Sleep(2000);

            if (receivedMessage && currentActionTimetoken > 0 && currentMessageTimetoken > 0 && !receivedRemoveEvent)
            {
                System.Diagnostics.Debug.WriteLine("RemoveMessageAction To Confirm STARTED");
                me = new ManualResetEvent(false);

                pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .Execute(new PNRemoveMessageActionResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        System.Diagnostics.Debug.WriteLine("RemoveMessageAction To Confirm = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                        receivedMessage = true;
                    }
                    me.Set();
                }));

                me.WaitOne(manualResetEventWaitTimeout);
            }

            Thread.Sleep(4000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            pubnub.RemoveListener(eventListener);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedAddEvent && receivedRemoveEvent, "Message Action events Failed");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncAddRemoveMessageActionReturnEventInfo()
#else
        public static async Task ThenWithAsyncAddRemoveMessageActionReturnEventInfo()
#endif
        {
            server.ClearRequests();
            if (!PubnubCommon.PAMServerSideRun)
            {
                Assert.Ignore("ThenWithAsyncAddRemoveMessageActionReturnEventInfo needs Secret Key");
                return;
            }

            bool receivedMessage = false;
            bool receivedAddEvent = false;
            bool receivedRemoveEvent = false;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNMessageActionEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Event == "added")
                    {
                        receivedAddEvent = true;
                    }
                    else if (eventResult.Event == "removed")
                    {
                        receivedRemoveEvent = true;
                    }
                },
                delegate (Pubnub pnObj, PNStatus status)
                {

                }
                );

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
            pubnub.AddListener(eventListener);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;
            long currentMessageTimetoken = new Random().Next(Int32.MaxValue);
            long currentActionTimetoken = 0;
            string currentUUID = "";

            ManualResetEvent me = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            me.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("GetMessageActions STARTED");
#if NET40
            PNResult<PNGetMessageActionsResult> getMsgActionResult = Task.Factory.StartNew(async () => await pubnub.GetMessageActions().Channel(channel).ExecuteAsync()).Result.Result;
#else
            PNResult<PNGetMessageActionsResult> getMsgActionResult = await pubnub.GetMessageActions().Channel(channel).ExecuteAsync();
#endif
            if (getMsgActionResult.Result != null && getMsgActionResult.Status.StatusCode == 200 && !getMsgActionResult.Status.Error)
            {
                System.Diagnostics.Debug.WriteLine("GetMessageActions = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(getMsgActionResult.Result));

                if (getMsgActionResult.Result.MessageActions != null && getMsgActionResult.Result.MessageActions.Exists(x => x.MessageTimetoken == currentMessageTimetoken))
                {
                    PNMessageActionItem actionItem = getMsgActionResult.Result.MessageActions.Find(x => x.MessageTimetoken == currentMessageTimetoken);
                    currentActionTimetoken = actionItem.ActionTimetoken;
                    currentUUID = actionItem.Uuid;
                }
            }
            Thread.Sleep(2000);

            if (currentMessageTimetoken > 0 && currentActionTimetoken > 0)
            {
                System.Diagnostics.Debug.WriteLine("RemoveMessageAction STARTED");

#if NET40
                PNResult<PNRemoveMessageActionResult> removeMsgActionResult = Task.Factory.StartNew(async () => await pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .ExecuteAsync()).Result.Result;
#else
                PNResult<PNRemoveMessageActionResult> removeMsgActionResult = await pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .ExecuteAsync();
#endif
                if (removeMsgActionResult.Result != null && removeMsgActionResult.Status.StatusCode == 200 && !removeMsgActionResult.Status.Error)
                {
                    System.Diagnostics.Debug.WriteLine("RemoveMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(removeMsgActionResult.Result));
                    receivedMessage = true;
                }
            }

            System.Diagnostics.Debug.WriteLine("AddMessageAction STARTED");
#if NET40
            PNResult<PNAddMessageActionResult> addMsgActionResult = Task.Factory.StartNew(async () => await pubnub.AddMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .Action(new PNMessageAction { Type = "reaction", Value = "smily_face" })
                .ExecuteAsync()).Result.Result;
#else
            PNResult<PNAddMessageActionResult> addMsgActionResult = await pubnub.AddMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .Action(new PNMessageAction { Type = "reaction", Value = "smily_face" })
                .ExecuteAsync();
#endif
            if (addMsgActionResult.Result != null && addMsgActionResult.Status.StatusCode == 200 && !addMsgActionResult.Status.Error && addMsgActionResult.Result.MessageTimetoken == currentMessageTimetoken)
            {
                System.Diagnostics.Debug.WriteLine("AddMessageAction = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(addMsgActionResult.Result));
                receivedMessage = true;
                currentActionTimetoken = addMsgActionResult.Result.ActionTimetoken;
                currentUUID = addMsgActionResult.Result.Uuid;
            }
            Thread.Sleep(2000);

            if (receivedMessage && currentActionTimetoken > 0 && currentMessageTimetoken > 0 && !receivedRemoveEvent)
            {
                System.Diagnostics.Debug.WriteLine("RemoveMessageAction To Confirm STARTED");

#if NET40
                Task.Factory.StartNew(async () => await pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .ExecuteAsync());
#else
                await pubnub.RemoveMessageAction()
                .Channel(channel)
                .MessageTimetoken(currentMessageTimetoken)
                .ActionTimetoken(currentActionTimetoken)
                .Uuid(currentUUID)
                .ExecuteAsync();
#endif
            }

            Thread.Sleep(4000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
            pubnub.RemoveListener(eventListener);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedAddEvent && receivedRemoveEvent, "Message Action events Failed");
        }

    }
}
