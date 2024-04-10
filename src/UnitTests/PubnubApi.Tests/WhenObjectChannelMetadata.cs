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
    public class WhenObjectChannelMetadata : TestHarness
    {
        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;
        private static Server server;
        private static string authKey = "myauth";
        private static string authToken = "";

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
            string channelMetadataId = "pandu-ut-sid";

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

            ManualResetEvent grantManualEvent = new ManualResetEvent(false);
            pubnub.Grant().Channels(new[] { channelMetadataId }).AuthKeys(new[] { authKey }).Read(true).Write(true).Manage(true).Update(true).Delete(true).Get(true).TTL(20)
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
                                                var read = r.Channels[channelMetadataId][authKey].ReadEnabled;
                                                var write = r.Channels[channelMetadataId][authKey].WriteEnabled;
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
            Assert.IsTrue(receivedGrantMessage, "WhenObjectChannelMetadata Grant access failed.");
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenChannelMetadataCRUDShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenChannelMetadataCRUDShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string channelMetadataId = "pandu-ut-sid";

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
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            #region "CreateSpace"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() STARTED");
            pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname")
                    .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (channelMetadataId == r.Channel)
                            {
                                receivedMessage = true;
                            }
                        }
                        manualEvent.Set();
                    }));
            #endregion
            manualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "SetChannelMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId == r.Channel)
                                {
                                    receivedMessage = true;
                                }
                            }
                            manualEvent.Set();
                        }));
                #endregion
                manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "GetChannelMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.GetChannelMetadata() STARTED");
                pubnub.GetChannelMetadata().Channel(channelMetadataId).IncludeCustom(true)
                    .Execute(new PNGetChannelMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (channelMetadataId == r.Channel)
                            {
                                receivedMessage = true;
                            }
                        }
                        manualEvent.Set();
                    }));
                #endregion
                manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "GetAllChannelMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.GetAllChannelMetadata() STARTED");
                pubnub.GetAllChannelMetadata().IncludeCount(true)
                    .Execute(new PNGetAllChannelMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            List<PNChannelMetadataResult> spaceList = r.Channels;
                            if (spaceList != null && spaceList.Count > 0 && spaceList.Find(x => x.Channel == channelMetadataId) != null)
                            {
                                receivedMessage = true;
                            }
                        }
                        manualEvent.Set();
                    }));
                #endregion
                manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "SetChannelMetadata/DeleteChannelMetadataId Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncChannelMetadataCRUDShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncChannelMetadataCRUDShouldReturnSuccessCodeAndInfo()
#endif        
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncChannelMetadataCRUDShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string channelMetadataId = "pandu-ut-sid";

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

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync());
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif

            #region "SetChannelMetadata"
            System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
#if NET40
            PNResult<PNSetChannelMetadataResult> createSpaceResult = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetChannelMetadataResult> createSpaceResult = await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname").ExecuteAsync();
#endif
            if (createSpaceResult.Result != null && createSpaceResult.Status.StatusCode == 200 && !createSpaceResult.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpaceResult.Result);
                if (channelMetadataId == createSpaceResult.Result.Channel)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetChannelMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> updateSpaceResult = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> updateSpaceResult = await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                    .ExecuteAsync();
#endif
                if (updateSpaceResult.Result != null && updateSpaceResult.Status.StatusCode == 200 && !updateSpaceResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(updateSpaceResult.Result);
                    if (channelMetadataId == updateSpaceResult.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "GetSpace"
                System.Diagnostics.Debug.WriteLine("pubnub.GetChannelMetadata() STARTED");
#if NET40
                PNResult<PNGetChannelMetadataResult> getSpaceResult = Task.Factory.StartNew(async () => await pubnub.GetChannelMetadata().Channel(channelMetadataId).IncludeCustom(true)
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNGetChannelMetadataResult> getSpaceResult = await pubnub.GetChannelMetadata().Channel(channelMetadataId).IncludeCustom(true)
                    .ExecuteAsync();
#endif
                if (getSpaceResult.Result != null && getSpaceResult.Status.StatusCode == 200 && !getSpaceResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getSpaceResult.Result);
                    if (channelMetadataId == getSpaceResult.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "GetSpaces"
                System.Diagnostics.Debug.WriteLine("pubnub.GetAllChannelMetadata() STARTED");
#if NET40
                PNResult<PNGetAllChannelMetadataResult> getSpacesResult = Task.Factory.StartNew(async () => await pubnub.GetAllChannelMetadata().IncludeCount(true).ExecuteAsync()).Result.Result;
#else
                PNResult<PNGetAllChannelMetadataResult> getSpacesResult = await pubnub.GetAllChannelMetadata().IncludeCount(true).ExecuteAsync();
#endif
                if (getSpacesResult.Result != null && getSpacesResult.Status.StatusCode == 200 && !getSpacesResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getSpacesResult.Result);
                    List<PNChannelMetadataResult> spaceList = getSpacesResult.Result.Channels;
                    if (spaceList != null && spaceList.Count > 0 && spaceList.Find(x => x.Channel == channelMetadataId) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "SeChannelMetadata/DeleteChannelMetadata Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenChannelMetadataSetDeleteShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenChannelMetadataSetDeleteShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string channelMetadataId = "pandu-ut-sid";
            manualResetEventWaitTimeout = 310 * 1000;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "channel")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "set")
                        {
                            receivedUpdateEvent = true;
                        }
                        else if (eventResult.Event.ToLowerInvariant() == "delete")
                        {
                            receivedDeleteEvent = true;
                        }
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
                Secure = false,
                AuthKey = "myauth"
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

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channelMetadataId }).Execute();
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            #region "SetChannelMetadata"
            System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
            pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname")
                    .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (channelMetadataId == r.Channel)
                            {
                                receivedMessage = true;
                            }
                        }
                        manualEvent.Set();
                    }));
            #endregion
            manualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "SetChannelMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId == r.Channel)
                                {
                                    receivedMessage = true;
                                }
                            }
                            manualEvent.Set();
                        }));
                #endregion
                manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (!receivedDeleteEvent)
            {
                manualEvent = new ManualResetEvent(false);
                System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
                pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                    delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
                manualEvent.WaitOne(2000);
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channelMetadataId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent, "Channel Metadata events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }

        [Test]
#if NET40
        public static void ThenWithAsyncChannelMetadataUpdateDeleteShouldReturnEventInfo()
#else
        public static async Task ThenWithAsyncChannelMetadataUpdateDeleteShouldReturnEventInfo()
#endif        
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncChannelMetadataUpdateDeleteShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string channelMetadataId = "pandu-ut-sid";
            manualResetEventWaitTimeout = 310 * 1000;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "channel")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "set")
                        {
                            receivedUpdateEvent = true;
                        }
                        else if (eventResult.Event.ToLowerInvariant() == "delete")
                        {
                            receivedDeleteEvent = true;
                        }
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
                Secure = false,
                AuthKey = "myauth"
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

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channelMetadataId }).Execute();
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync());
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif

            #region "SetChannelMetadata"
            System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
#if NET40
            PNResult<PNSetChannelMetadataResult> createSpaceResult = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetChannelMetadataResult> createSpaceResult = await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname").ExecuteAsync();
#endif
            if (createSpaceResult.Result != null && createSpaceResult.Status.StatusCode == 200 && !createSpaceResult.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpaceResult.Result);
                if (channelMetadataId == createSpaceResult.Result.Channel)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetChannelMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> updateSpaceResult = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> updateSpaceResult = await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                    .ExecuteAsync();
#endif
                if (updateSpaceResult.Result != null && updateSpaceResult.Status.StatusCode == 200 && !updateSpaceResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(updateSpaceResult.Result);
                    if (channelMetadataId == updateSpaceResult.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedDeleteEvent)
            {
                System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
#if NET40
                Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync());
#else
                await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channelMetadataId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent, "Space events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }
    }
}
