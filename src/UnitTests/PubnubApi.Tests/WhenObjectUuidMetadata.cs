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
    public class WhenObjectUuidMetadata : TestHarness
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
            string uuid_metadata_id = "pandu-ut-uid";

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            ManualResetEvent grantManualEvent = new ManualResetEvent(false);
            pubnub.Grant().Channels(new[] { uuid_metadata_id }).AuthKeys(new[] { authKey }).Read(true).Write(true).Manage(true).TTL(20)
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
                                                var read = r.Channels[uuid_metadata_id][authKey].ReadEnabled;
                                                var write = r.Channels[uuid_metadata_id][authKey].WriteEnabled;
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
            Assert.IsTrue(receivedGrantMessage, "WhenObjectUuidMetaId Grant access failed.");
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenUuidMetadataCRUDShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenUuidMetadataCRUDShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string uuidMetadataId = "pandu-ut-uid";

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false,
                IncludeInstanceIdentifier = false,
                IncludeRequestIdentifier = false
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
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUuidMetadata() STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            #region "SetUuidMetadata"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() STARTED");
            pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un")
                    .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (uuidMetadataId == r.Uuid)
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
                #region "SetUuidMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() STARTED");
                pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (uuidMetadataId == r.Uuid)
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
                #region "GetUuidMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.GetUuidMetadata() STARTED");
                pubnub.GetUuidMetadata().Uuid(uuidMetadataId).IncludeCustom(true)
                    .Execute(new PNGetUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (uuidMetadataId == r.Uuid)
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
                #region "GetAllUuidMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.GetAllUuidMetadata() STARTED");
                pubnub.GetAllUuidMetadata().IncludeCount(true)
                    .Execute(new PNGetAllUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            List<PNUuidMetadataResult> userList = r.Uuids;
                            if (userList != null && userList.Count > 0 && userList.Find(x => x.Uuid == uuidMetadataId) != null)
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
                Assert.IsTrue(receivedMessage, "SetUuidMetadata/DeleteUser Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncUuidMetadataCRUDShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncUuidMetadataCRUDShouldReturnSuccessCodeAndInfo()
#endif        
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncUuidCRUDShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string uuidMetadataId = "pandu-ut-uid";

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false,
                IncludeInstanceIdentifier = false,
                IncludeRequestIdentifier = false
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

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUuidMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync());
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
#endif

            #region "SetUuidMetadata"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() STARTED");
#if NET40
            PNResult<PNSetUuidMetadataResult> createUserResult = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetUuidMetadataResult> createUserResult = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un").ExecuteAsync();
#endif
            if (createUserResult.Result != null && createUserResult.Status.StatusCode == 200 && !createUserResult.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUserResult.Result);
                if (uuidMetadataId == createUserResult.Result.Uuid)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "UpdateUser"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() STARTED");
#if NET40
                PNResult<PNSetUuidMetadataResult> updateUserResult = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetUuidMetadataResult> updateUserResult = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync();
#endif
                if (updateUserResult.Result != null && updateUserResult.Status.StatusCode == 200 && !updateUserResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(updateUserResult.Result);
                    if (uuidMetadataId == updateUserResult.Result.Uuid)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "GetUuidMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.GetUuidMetadata() STARTED");
#if NET40
                PNResult<PNGetUuidMetadataResult> getUserResult = Task.Factory.StartNew(async () => await pubnub.GetUuidMetadata().Uuid(uuidMetadataId).IncludeCustom(true).ExecuteAsync()).Result.Result;
#else
                PNResult<PNGetUuidMetadataResult> getUserResult = await pubnub.GetUuidMetadata().Uuid(uuidMetadataId).IncludeCustom(true).ExecuteAsync();
#endif
                if (getUserResult.Result != null && getUserResult.Status.StatusCode == 200 && !getUserResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getUserResult.Result);
                    if (uuidMetadataId == getUserResult.Result.Uuid)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "GetAllUuidMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.GetAllUuidMetadata() STARTED");
#if NET40
                PNResult<PNGetAllUuidMetadataResult> getUsersResult = Task.Factory.StartNew(async () => await pubnub.GetAllUuidMetadata().IncludeCount(true).ExecuteAsync()).Result.Result;
#else
                PNResult<PNGetAllUuidMetadataResult> getUsersResult = await pubnub.GetAllUuidMetadata().IncludeCount(true).ExecuteAsync();
#endif
                if (getUsersResult.Result != null && getUsersResult.Status.StatusCode == 200 && !getUsersResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getUsersResult.Result);
                    List<PNUuidMetadataResult> userList = getUsersResult.Result.Uuids;
                    if (userList != null && userList.Count > 0 && userList.Find(x => x.Uuid == uuidMetadataId) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "SetUuidMetadata/UpdateUser/DeleteUser Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenUuidMetadataSetDeleteShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenUuidMetadataSetDeleteShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string uuidMetadataId = "pandu-ut-uid";
            manualResetEventWaitTimeout = 310 * 1000;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate(Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "uuid")
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
                delegate(Pubnub pnObj, PNStatus status)
                {

                }
                );

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
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
            pubnub.Subscribe<string>().Channels(new string[] { uuidMetadataId }).Execute();
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            #region "SetUuidMetadata"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() STARTED");
            pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un")
                    .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (uuidMetadataId == r.Uuid)
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
                #region "SetUuidMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.UpdateUser() STARTED");
                pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (uuidMetadataId == r.Uuid)
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
                System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 2 STARTED");
                pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).Execute(new PNRemoveUuidMetadataResultExt(
                    delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
                manualEvent.WaitOne(2000);
            }

            Thread.Sleep(4000);

            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent, "User events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }

        [Test]
#if NET40
        public static void ThenWithAsyncUuidMetadataUpdateDeleteShouldReturnEventInfo()
#else
        public static async Task ThenWithAsyncUuidMetadataUpdateDeleteShouldReturnEventInfo()
#endif        
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncUuidUpdateDeleteShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string uuidMetadataId = "pandu-ut-uid";
            manualResetEventWaitTimeout = 310 * 1000;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "uuid")
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

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
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
            pubnub.Subscribe<string>().Channels(new string[] { uuidMetadataId }).Execute();
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUuidMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync());
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
#endif

            #region "SetUuidMetadata"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() STARTED");
#if NET40
            PNResult<PNSetUuidMetadataResult> createUserResult = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetUuidMetadataResult> createUserResult = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un").ExecuteAsync();
#endif
            if (createUserResult.Result != null && createUserResult.Status.StatusCode == 200 && !createUserResult.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUserResult.Result);
                if (uuidMetadataId == createUserResult.Result.Uuid)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetUuidMetadata"
                System.Diagnostics.Debug.WriteLine("pubnub.UpdateUser() STARTED");
#if NET40
                PNResult<PNSetUuidMetadataResult> updateUserResult = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetUuidMetadataResult> updateUserResult = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync();
#endif
                if (updateUserResult.Result != null && updateUserResult.Status.StatusCode == 200 && !updateUserResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(updateUserResult.Result);
                    if (uuidMetadataId == updateUserResult.Result.Uuid)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedDeleteEvent)
            {
                System.Diagnostics.Debug.WriteLine("pubnub.DeleteUuidMetadata() 2 STARTED");
#if NET40
                Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync());
#else
                await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
#endif
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent, "User events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }
    }
}
