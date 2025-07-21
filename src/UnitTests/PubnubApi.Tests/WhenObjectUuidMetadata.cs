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
    public class WhenObjectUuidMetadata : TestHarness
    {
        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;
        private static Server server;
        private static string authToken = "myauth";

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
            string uuid_metadata_id = "pandu-ut-uid";

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            ManualResetEvent grantManualEvent = new ManualResetEvent(false);
            pubnub.GrantToken()
                .TTL(20)
                .Resources(new PNTokenResources()
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>() {
                                        { uuid_metadata_id, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } },
                                        },
                    Uuids = new Dictionary<string, PNTokenAuthValues>() {
                                        { uuid_metadata_id, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } } },
                }
                )
                .Execute(new PNAccessManagerTokenResultExt(
                                (r, s) =>
                                {
                                    try
                                    {
                                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                                        if (r != null)
                                        {
                                            Debug.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                            if (!string.IsNullOrEmpty(r.Token))
                                            {
                                                authToken = r.Token;
                                                receivedGrantMessage = true;
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
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }
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

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                IncludeInstanceIdentifier = false,
                IncludeRequestIdentifier = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }
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
                pubnub.GetAllUuidMetadata().IncludeCount(true).Filter($"id LIKE '{uuidMetadataId}*'")
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

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                IncludeInstanceIdentifier = false,
                IncludeRequestIdentifier = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }

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
                PNResult<PNGetAllUuidMetadataResult> getUsersResult = await pubnub.GetAllUuidMetadata().IncludeCount(true).Filter($"id LIKE '{uuidMetadataId}*'").ExecuteAsync();
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

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }
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

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }
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

        [Test]
        public static async Task ThenUuidMetadataShouldSupportAllFields()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenUuidMetadataShouldSupportAllFields");
                return;
            }

            bool receivedMessage = false;
            string uuidMetadataId = "pandu-ut-uid";

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
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

            ManualResetEvent manualEvent = new ManualResetEvent(false);

            // First create with all fields
            pubnub.SetUuidMetadata()
                .Uuid(uuidMetadataId)
                .Name("pandu-ut-un-all-fields")
                .ProfileUrl("pandu-sample-profile-url")
                .ExternalId("pandu-sample-ext-id")
                .Email("test@test.com")
                .Custom(new Dictionary<string, object>() { { "color", "red" } })
                .IncludeCustom(true)
                .QueryParam(new Dictionary<string, object>() { { "test_param", "test_value" } })
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
            
            manualEvent.WaitOne(manualResetEventWaitTimeout);

            await Task.Delay(4000);

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);

                // Then get to verify all fields
                pubnub.GetUuidMetadata()
                    .Uuid(uuidMetadataId)
                    .IncludeCustom(true)
                    .Execute(new PNGetUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            Assert.That(r.Uuid, Is.EqualTo(uuidMetadataId), "UUID should match");
                            Assert.That(r.Name, Is.EqualTo("pandu-ut-un-all-fields"), "Name should match");
                            Assert.That(r.ProfileUrl, Is.EqualTo("pandu-sample-profile-url"), "ProfileUrl should match");
                            Assert.That(r.ExternalId, Is.EqualTo("pandu-sample-ext-id"), "ExternalId should match");
                            Assert.That(r.Email, Is.EqualTo("test@test.com"), "Email should match");
                            Assert.That(r.Custom, Is.Not.Null, "Custom should not be null");
                            Assert.That(r.Custom.ContainsKey("color"), Is.True, "Custom should contain 'color' key");
                            Assert.That(r.Custom["color"].ToString(), Is.EqualTo("red"), "Custom color value should match");
                            receivedMessage = true;
                        }
                        manualEvent.Set();
                    }));
            }

            manualEvent.WaitOne(manualResetEventWaitTimeout);

            Assert.IsTrue(receivedMessage, "UuidMetadata with all fields test failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenGetAllUuidMetadataShouldSupportAllFields()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenGetAllUuidMetadataShouldSupportAllFields");
                return;
            }

            bool receivedMessage = false;
            string uuidMetadataId = "pandu-ut-uid";

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
            
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }

            ManualResetEvent manualEvent = new ManualResetEvent(false);

            // First create with all fields
            pubnub.SetUuidMetadata()
                .Uuid(uuidMetadataId)
                .Name("pandu-ut-un-all-fields")
                .ProfileUrl("pandu-sample-profile-url")
                .ExternalId("pandu-sample-ext-id")
                .Email("test@test.com")
                .Custom(new Dictionary<string, object>() { { "color", "red" } })
                .IncludeCustom(true)
                .QueryParam(new Dictionary<string, object>() { { "test_param", "test_value" } })
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

            manualEvent.WaitOne(manualResetEventWaitTimeout);
            await Task.Delay(2000);

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);

                // Then get all to verify fields
                pubnub.GetAllUuidMetadata()
                    .IncludeCount(true)
                    .IncludeCustom(true)
                    .Filter($"id == '{uuidMetadataId}'")
                    .Execute(new PNGetAllUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            Assert.That(r.Uuids, Is.Not.Null, "Uuids list should not be null");
                            Assert.That(r.Uuids.Count, Is.EqualTo(1), "Should find exactly one UUID");
                            
                            var uuid = r.Uuids[0];
                            Assert.That(uuid.Uuid, Is.EqualTo(uuidMetadataId), "UUID should match");
                            Assert.That(uuid.Name, Is.EqualTo("pandu-ut-un-all-fields"), "Name should match");
                            Assert.That(uuid.ProfileUrl, Is.EqualTo("pandu-sample-profile-url"), "ProfileUrl should match");
                            Assert.That(uuid.ExternalId, Is.EqualTo("pandu-sample-ext-id"), "ExternalId should match");
                            Assert.That(uuid.Email, Is.EqualTo("test@test.com"), "Email should match");
                            Assert.That(uuid.Custom, Is.Not.Null, "Custom should not be null");
                            Assert.That(uuid.Custom.ContainsKey("color"), Is.True, "Custom should contain 'color' key");
                            Assert.That(uuid.Custom["color"].ToString(), Is.EqualTo("red"), "Custom color value should match");
                            
                            Assert.That(r.TotalCount, Is.EqualTo(1), "Total count should be 1");
                            receivedMessage = true;
                        }
                        manualEvent.Set();
                    }));
            }

            manualEvent.WaitOne(manualResetEventWaitTimeout);

            Assert.IsTrue(receivedMessage, "GetAllUuidMetadata with all fields test failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenRemoveUuidMetadataShouldRemoveAllFields()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenRemoveUuidMetadataShouldRemoveAllFields");
                return;
            }

            bool receivedMessage = false;
            string uuidMetadataId = "pandu-ut-uid";

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
            
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            
            // First create with all fields
            pubnub.SetUuidMetadata()
                .Uuid(uuidMetadataId)
                .Name("pandu-ut-un-remove")
                .ProfileUrl("pandu-sample-profile-url")
                .ExternalId("pandu-sample-ext-id")
                .Email("test@test.com")
                .Custom(new Dictionary<string, object>() { { "color", "red" } })
                .IncludeCustom(true)
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

            manualEvent.WaitOne(manualResetEventWaitTimeout);
            await Task.Delay(2000);
            
            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);

                // Then remove the metadata
                pubnub.RemoveUuidMetadata()
                    .Uuid(uuidMetadataId)
                    .Execute(new PNRemoveUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            receivedMessage = true;
                        }
                        manualEvent.Set();
                    }));
            }

            manualEvent.WaitOne(manualResetEventWaitTimeout);
            await Task.Delay(2000);

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);

                // Finally verify it's gone by trying to get it
                pubnub.GetUuidMetadata()
                    .Uuid(uuidMetadataId)
                    .Execute(new PNGetUuidMetadataResultExt((r, s) =>
                    {
                        Assert.That(r, Is.Null, "Result should be null for removed UUID");
                        Assert.That(s.StatusCode, Is.EqualTo(404), "Status code should be 404");
                        Assert.That(s.Error, Is.True, "Should not indicate error");
                        receivedMessage = true;
                        manualEvent.Set();
                    }));
            }

            manualEvent.WaitOne(manualResetEventWaitTimeout);

            Assert.IsTrue(receivedMessage, "UuidMetadata removal test failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenUuidMetadataShouldSupportStatusAndTypeFields()
        {
            var r = new Random();
            string uuidMetadataId = $"uuid{r.Next(100, 1000)}";
            string status = $"status{r.Next(100, 1000)}";
            string type = $"type{r.Next(100, 1000)}";
            string name =  $"name{r.Next(100, 1000)}";
            PNConfiguration configuration = new PNConfiguration(new UserId($"user{r.Next(100,1000)}"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey
            };
            pubnub = createPubNubInstance(configuration);
            
            bool receivedSetEvent = false;
            ManualResetEvent setEventManualEvent = new ManualResetEvent(false);
            
            var channelSubscription = pubnub.Channel(uuidMetadataId).Subscription();
            channelSubscription.onObject += (_, appContextEvent) =>
            {
                var eventType = appContextEvent.Type;
                // check event type to be 'set'
                Assert.That(eventType.ToLowerInvariant(), Is.EqualTo("uuid"), "Event type should be 'uuid'");
                
                if (appContextEvent.Event.ToLowerInvariant() == "set")
                {
                    receivedSetEvent = true;
                    // check value of status and type to match set value
                    Assert.That(appContextEvent.UuidMetadata, Is.Not.Null, "UuidMetadata should not be null for set event");
                    Assert.That(appContextEvent.UuidMetadata.Status, Is.EqualTo(status), "Status should match the set value");
                    Assert.That(appContextEvent.UuidMetadata.Type, Is.EqualTo(type), "Type should match the set value");
                    Assert.That(appContextEvent.UuidMetadata.Name, Is.EqualTo(name), "Name should match the set value");
                    Assert.That(appContextEvent.UuidMetadata.Custom, Is.Not.Null, "Custom should not be null");
                    Assert.That(appContextEvent.UuidMetadata.Custom.ContainsKey("key"), Is.True, "Custom should contain 'key'");
                    Assert.That(appContextEvent.UuidMetadata.Custom["key"].ToString(), Is.EqualTo("value"), "Custom key value should match");
                    setEventManualEvent.Set();
                }
            };
            channelSubscription.Subscribe<object>();
            
            await Task.Delay(2000);
            // now set the UUID metadata.
            
            var setUuidMetadata =  await pubnub.SetUuidMetadata().
                Uuid(uuidMetadataId).
                Status(status).
                Name(name).
                Type(type).
                Custom(new Dictionary<string, object>{ {"key", "value"}}).
                ExecuteAsync();
            
            // check uuid metadata return status and type., Custom also?
            Assert.That(setUuidMetadata.Result, Is.Not.Null, "SetUuidMetadata result should not be null");
            Assert.That(setUuidMetadata.Status.StatusCode, Is.EqualTo(200), "SetUuidMetadata status code should be 200");
            Assert.That(setUuidMetadata.Status.Error, Is.False, "SetUuidMetadata should not indicate error");
            Assert.That(setUuidMetadata.Result.Uuid, Is.EqualTo(uuidMetadataId), "UUID should match");
            Assert.That(setUuidMetadata.Result.Name, Is.EqualTo(name), "Name should match");
            Assert.That(setUuidMetadata.Result.Status, Is.EqualTo(status), "Status should match");
            Assert.That(setUuidMetadata.Result.Type, Is.EqualTo(type), "Type should match");
            Assert.That(setUuidMetadata.Result.Custom, Is.Not.Null, "Custom should not be null");
            Assert.That(setUuidMetadata.Result.Custom.ContainsKey("key"), Is.True, "Custom should contain 'key'");
            Assert.That(setUuidMetadata.Result.Custom["key"].ToString(), Is.EqualTo("value"), "Custom key value should match");
            
            await Task.Delay(2000);

            var getUuidMetadata =
                await pubnub.GetUuidMetadata().Uuid(uuidMetadataId).IncludeStatus(true).IncludeType(true).IncludeCustom(true).ExecuteAsync();
            
            // check uuid data should contain status, type and custom fields using Assert.
            Assert.That(getUuidMetadata.Result, Is.Not.Null, "GetUuidMetadata result should not be null");
            Assert.That(getUuidMetadata.Status.StatusCode, Is.EqualTo(200), "GetUuidMetadata status code should be 200");
            Assert.That(getUuidMetadata.Status.Error, Is.False, "GetUuidMetadata should not indicate error");
            Assert.That(getUuidMetadata.Result.Uuid, Is.EqualTo(uuidMetadataId), "UUID should match");
            Assert.That(getUuidMetadata.Result.Name, Is.EqualTo(name), "Name should match");
            Assert.That(getUuidMetadata.Result.Status, Is.EqualTo(status), "Status should match");
            Assert.That(getUuidMetadata.Result.Type, Is.EqualTo(type), "Type should match");
            Assert.That(getUuidMetadata.Result.Custom, Is.Not.Null, "Custom should not be null");
            Assert.That(getUuidMetadata.Result.Custom.ContainsKey("key"), Is.True, "Custom should contain 'key'");
            Assert.That(getUuidMetadata.Result.Custom["key"].ToString(), Is.EqualTo("value"), "Custom key value should match");
            
            // Wait for subscription events
            setEventManualEvent.WaitOne(3000);
            Assert.That(receivedSetEvent, Is.True, "Should have received set event via subscription");
            
            // As a part of cleanup delete the uuidMetadata
            var removeUuidMetadata = await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
            
            // Wait for delete event
            await Task.Delay(1000);
            
            // Cleanup
            channelSubscription.Unsubscribe<object>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
    }
}
