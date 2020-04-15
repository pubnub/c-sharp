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
    public class WhenObjectUser : TestHarness
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
            string userId = "pandu-ut-uid";

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
            PNResourcePermission perm = new PNResourcePermission();
            perm.Read = true;
            perm.Write = true;
            perm.Manage = true;
            perm.Delete = true;
            perm.Create = true;

            ManualResetEvent grantManualEvent = new ManualResetEvent(false);
            pubnub.GrantToken()
                .Users(new Dictionary<string, PNResourcePermission>() { { userId, perm } })
                .AuthKey(authKey)
                .TTL(20)
                .Execute(new PNAccessManagerTokenResultExt(
                                (r, s) =>
                                {
                                    try
                                    {
                                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                                        if (r != null && !string.IsNullOrEmpty(r.Token))
                                        {
                                            Debug.WriteLine("PNAccessManagerTokenResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                            authToken = r.Token;
                                            receivedGrantMessage = true;
                                        }
                                    }
                                    catch { }
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
            Assert.IsTrue(receivedGrantMessage, "WhenObjectUser Grant access failed.");
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenUserCRUDShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenUserCRUDShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string userId = "pandu-ut-uid";

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
            if (!PubnubCommon.PAMServerSideRun && !string.IsNullOrEmpty(authToken))
            {
                pubnub.ClearTokens();
                pubnub.SetToken(authToken);
            }
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() STARTED");
            pubnub.DeleteUser().Id(userId).Execute(new PNDeleteUserResultExt(
                delegate (PNDeleteUserResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            #region "CreateUser"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() STARTED");
            pubnub.CreateUser().Id(userId).Name("pandu-ut-un")
                    .Execute(new PNCreateUserResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (userId == r.Id)
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
                #region "UpdateUser"
                System.Diagnostics.Debug.WriteLine("pubnub.UpdateUser() STARTED");
                pubnub.UpdateUser().Id(userId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .CustomObject(new Dictionary<string, object>() { { "color", "red" } })
                        .Execute(new PNUpdateUserResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (userId == r.Id)
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
                #region "GetUser"
                System.Diagnostics.Debug.WriteLine("pubnub.GetUser() STARTED");
                pubnub.GetUser().UserId(userId).IncludeCustom(true)
                    .Execute(new PNGetUserResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (userId == r.Id)
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
                #region "GetUsers"
                System.Diagnostics.Debug.WriteLine("pubnub.GetUsers() STARTED");
                pubnub.GetUsers().IncludeCount(true)
                    .Execute(new PNGetUsersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            List<PNUserResult> userList = r.Users;
                            if (userList != null && userList.Count > 0 && userList.Find(x => x.Id == userId) != null)
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
                Assert.IsTrue(receivedMessage, "CreateUser/UpdateUser/DeleteUser Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncUserCRUDShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncUserCRUDShouldReturnSuccessCodeAndInfo()
#endif        
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncUserCRUDShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string userId = "pandu-ut-uid";

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
            if (!PubnubCommon.PAMServerSideRun && !string.IsNullOrEmpty(authToken))
            {
                pubnub.ClearTokens();
                pubnub.SetToken(authToken);
            }

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.DeleteUser().Id(userId).ExecuteAsync());
#else
            await pubnub.DeleteUser().Id(userId).ExecuteAsync();
#endif

            #region "CreateUser"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() STARTED");
#if NET40
            PNResult<PNCreateUserResult> createUserResult = Task.Factory.StartNew(async () => await pubnub.CreateUser().Id(userId).Name("pandu-ut-un").ExecuteAsync()).Result.Result;
#else
            PNResult<PNCreateUserResult> createUserResult = await pubnub.CreateUser().Id(userId).Name("pandu-ut-un").ExecuteAsync();
#endif
            if (createUserResult.Result != null && createUserResult.Status.StatusCode == 200 && !createUserResult.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUserResult.Result);
                if (userId == createUserResult.Result.Id)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "UpdateUser"
                System.Diagnostics.Debug.WriteLine("pubnub.UpdateUser() STARTED");
#if NET40
                PNResult<PNUpdateUserResult> updateUserResult = Task.Factory.StartNew(async () => await pubnub.UpdateUser().Id(userId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .CustomObject(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync()).Result.Result;
#else
                PNResult<PNUpdateUserResult> updateUserResult = await pubnub.UpdateUser().Id(userId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .CustomObject(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync();
#endif
                if (updateUserResult.Result != null && updateUserResult.Status.StatusCode == 200 && !updateUserResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(updateUserResult.Result);
                    if (userId == updateUserResult.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "GetUser"
                System.Diagnostics.Debug.WriteLine("pubnub.GetUser() STARTED");
#if NET40
                PNResult<PNGetUserResult> getUserResult = Task.Factory.StartNew(async () => await pubnub.GetUser().UserId(userId).IncludeCustom(true).ExecuteAsync()).Result.Result;
#else
                PNResult<PNGetUserResult> getUserResult = await pubnub.GetUser().UserId(userId).IncludeCustom(true).ExecuteAsync();
#endif
                if (getUserResult.Result != null && getUserResult.Status.StatusCode == 200 && !getUserResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getUserResult.Result);
                    if (userId == getUserResult.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "GetUsers"
                System.Diagnostics.Debug.WriteLine("pubnub.GetUsers() STARTED");
#if NET40
                PNResult<PNGetUsersResult> getUsersResult = Task.Factory.StartNew(async () => await pubnub.GetUsers().IncludeCount(true).ExecuteAsync()).Result.Result;
#else
                PNResult<PNGetUsersResult> getUsersResult = await pubnub.GetUsers().IncludeCount(true).ExecuteAsync();
#endif
                if (getUsersResult.Result != null && getUsersResult.Status.StatusCode == 200 && !getUsersResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getUsersResult.Result);
                    List<PNUserResult> userList = getUsersResult.Result.Users;
                    if (userList != null && userList.Count > 0 && userList.Find(x => x.Id == userId) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "CreateUser/UpdateUser/DeleteUser Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenUserUpdateDeleteShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenUserUpdateDeleteShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string userId = "pandu-ut-uid";
            manualResetEventWaitTimeout = 310 * 1000;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate(Pubnub pnObj, PNObjectApiEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "user")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "update")
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
            if (!PubnubCommon.PAMServerSideRun && !string.IsNullOrEmpty(authToken))
            {
                pubnub.ClearTokens();
                pubnub.SetToken(authToken);
            }
            pubnub.AddListener(eventListener);

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { userId }).Execute();
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() STARTED");
            pubnub.DeleteUser().Id(userId).Execute(new PNDeleteUserResultExt(
                delegate (PNDeleteUserResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            #region "CreateUser"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() STARTED");
            pubnub.CreateUser().Id(userId).Name("pandu-ut-un")
                    .Execute(new PNCreateUserResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (userId == r.Id)
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
                #region "UpdateUser"
                System.Diagnostics.Debug.WriteLine("pubnub.UpdateUser() STARTED");
                pubnub.UpdateUser().Id(userId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .CustomObject(new Dictionary<string, object>() { { "color", "red" } })
                        .Execute(new PNUpdateUserResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (userId == r.Id)
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
                System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 2 STARTED");
                pubnub.DeleteUser().Id(userId).Execute(new PNDeleteUserResultExt(
                    delegate (PNDeleteUserResult result, PNStatus status) { }));
                manualEvent.WaitOne(2000);
            }

            Thread.Sleep(4000);

            pubnub.Unsubscribe<string>().Channels(new string[] { userId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent, "User events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }

        [Test]
#if NET40
        public static void ThenWithAsyncUserUpdateDeleteShouldReturnEventInfo()
#else
        public static async Task ThenWithAsyncUserUpdateDeleteShouldReturnEventInfo()
#endif        
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncUserUpdateDeleteShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string userId = "pandu-ut-uid";
            manualResetEventWaitTimeout = 310 * 1000;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectApiEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "user")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "update")
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
            if (!PubnubCommon.PAMServerSideRun && !string.IsNullOrEmpty(authToken))
            {
                pubnub.ClearTokens();
                pubnub.SetToken(authToken);
            }
            pubnub.AddListener(eventListener);

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { userId }).Execute();
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.DeleteUser().Id(userId).ExecuteAsync());
#else
            await pubnub.DeleteUser().Id(userId).ExecuteAsync();
#endif

            #region "CreateUser"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() STARTED");
#if NET40
            PNResult<PNCreateUserResult> createUserResult = Task.Factory.StartNew(async () => await pubnub.CreateUser().Id(userId).Name("pandu-ut-un").ExecuteAsync()).Result.Result;
#else
            PNResult<PNCreateUserResult> createUserResult = await pubnub.CreateUser().Id(userId).Name("pandu-ut-un").ExecuteAsync();
#endif
            if (createUserResult.Result != null && createUserResult.Status.StatusCode == 200 && !createUserResult.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUserResult.Result);
                if (userId == createUserResult.Result.Id)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "UpdateUser"
                System.Diagnostics.Debug.WriteLine("pubnub.UpdateUser() STARTED");
#if NET40
                PNResult<PNUpdateUserResult> updateUserResult = Task.Factory.StartNew(async () => await pubnub.UpdateUser().Id(userId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .CustomObject(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync()).Result.Result;
#else
                PNResult<PNUpdateUserResult> updateUserResult = await pubnub.UpdateUser().Id(userId).Name("pandu-ut-un-upd")
                    .ProfileUrl("pandu-sample-profile-url").ExternalId("pandu-sample-ext-id")
                    .Email("test@test.com")
                    .CustomObject(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync();
#endif
                if (updateUserResult.Result != null && updateUserResult.Status.StatusCode == 200 && !updateUserResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(updateUserResult.Result);
                    if (userId == updateUserResult.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedDeleteEvent)
            {
                System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 2 STARTED");
#if NET40
                Task.Factory.StartNew(async () => await pubnub.DeleteUser().Id(userId).ExecuteAsync());
#else
                await pubnub.DeleteUser().Id(userId).ExecuteAsync();
#endif
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { userId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent, "User events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }
    }
}
