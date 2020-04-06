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
    public class WhenObjectMember : TestHarness
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
            string spaceId = "pandu-ut-sid";
            string userId1 = "pandu-ut-uid1";
            string userId2 = "pandu-ut-uid2";

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
                .Users(new Dictionary<string, PNResourcePermission>() { { userId1, perm }, { userId2, perm } })
                .Spaces(new Dictionary<string, PNResourcePermission>() { { spaceId, perm } })
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
                                    catch {  }
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
        public static void ThenAddUpdateRemoveUserShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenAddUpdateRemoveUserShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string spaceId = "pandu-ut-sid";
            string userId1 = "pandu-ut-uid1";
            string userId2 = "pandu-ut-uid2";

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
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
            if (!PubnubCommon.PAMServerSideRun && !string.IsNullOrEmpty(authToken))
            {
                pubnub.ClearTokens();
                pubnub.SetToken(authToken);
            }
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 1 STARTED");
            pubnub.DeleteUser().Id(userId1).Execute(new PNDeleteUserResultExt(
                delegate (PNDeleteUserResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 2 STARTED");
            pubnub.DeleteUser().Id(userId2).Execute(new PNDeleteUserResultExt(
                delegate (PNDeleteUserResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
            pubnub.DeleteSpace().Id(spaceId).Execute(new PNDeleteSpaceResultExt(
                delegate (PNDeleteSpaceResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
            #region "CreateUser 1"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() 1 STARTED");
            pubnub.CreateUser().Id(userId1).Name("pandu-ut-un1")
                    .Execute(new PNCreateUserResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (userId1 == r.Id)
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
                #region "CreateUser 2"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() 2 STARTED");
                pubnub.CreateUser().Id(userId2).Name("pandu-ut-un2")
                        .Execute(new PNCreateUserResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (userId2 == r.Id)
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
                #region "CreateSpace"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() STARTED");
                pubnub.CreateSpace().Id(spaceId).Name("pandu-ut-spname")
                        .Execute(new PNCreateSpaceResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (spaceId == r.Id)
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
                #region "Members Add"
                System.Diagnostics.Debug.WriteLine("pubnub.Members() ADD STARTED");
                pubnub.ManageMembers().SpaceId(spaceId)
                    .Add(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1 },
                            new PNMember() { UserId = userId2 }
                    })
                    .Include(new PNMemberField[] { PNMemberField.CUSTOM, PNMemberField.USER, PNMemberField.USER_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPage() { Next = "", Prev = "" })
                    .Execute(new PNManageMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Members != null
                            && r.Members.Find(x => x.UserId == userId1) != null
                            && r.Members.Find(x => x.UserId == userId2) != null)
                            {
                                receivedMessage = true;
                            }
                        }
                        manualEvent.Set();
                    }));
                #endregion
                manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedMessage && !string.IsNullOrEmpty(config.SecretKey))
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "Members Update"
                System.Diagnostics.Debug.WriteLine("pubnub.Members() UPDATE STARTED");
                pubnub.ManageMembers().SpaceId(spaceId)
                    .Update(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMember() { UserId = userId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Include(new PNMemberField[] { PNMemberField.CUSTOM, PNMemberField.USER, PNMemberField.USER_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPage() { Next = "", Prev = "" })
                    .Execute(new PNManageMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Members != null
                            && r.Members.Find(x => x.UserId == userId1) != null
                            && r.Members.Find(x => x.UserId == userId2) != null)
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
                #region "Members Remove"
                System.Diagnostics.Debug.WriteLine("pubnub.Members() REMOVE STARTED");
                pubnub.ManageMembers().SpaceId(spaceId)
                    .Remove(new List<string>() { userId2 })
                    .Include(new PNMemberField[] { PNMemberField.CUSTOM, PNMemberField.USER, PNMemberField.USER_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPage() { Next = "", Prev = "" })
                    .Execute(new PNManageMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Members != null)
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
                #region "GetMembers"
                System.Diagnostics.Debug.WriteLine("pubnub.GetMembers() STARTED");
                pubnub.GetMembers().SpaceId(spaceId)
                    .Execute(new PNGetMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Members != null
                            && r.Members.Find(x => x.UserId == userId1) != null)
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
                Assert.IsTrue(receivedMessage, "CreateUser/CreateSpace/Member AddUpdateRemove Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenWithAsyncAddUpdateRemoveUserShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenAddUpdateRemoveUserShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string spaceId = "pandu-ut-sid";
            string userId1 = "pandu-ut-uid1";
            string userId2 = "pandu-ut-uid2";

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
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
            if (!PubnubCommon.PAMServerSideRun && !string.IsNullOrEmpty(authToken))
            {
                pubnub.ClearTokens();
                pubnub.SetToken(authToken);
            }

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 1 STARTED");
            await pubnub.DeleteUser().Id(userId1).ExecuteAsync();

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 2 STARTED");
            await pubnub.DeleteUser().Id(userId2).ExecuteAsync();

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
            await pubnub.DeleteSpace().Id(spaceId).ExecuteAsync();

            receivedMessage = false;
            #region "CreateUser 1"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() 1 STARTED");
            PNResult<PNCreateUserResult> createUser1Result = await pubnub.CreateUser().Id(userId1).Name("pandu-ut-un1").ExecuteAsync();

            if (createUser1Result.Result != null && createUser1Result.Status.StatusCode == 200 && !createUser1Result.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser1Result.Result);
                if (userId1 == createUser1Result.Result.Id)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "CreateUser 2"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() 2 STARTED");
                PNResult<PNCreateUserResult> createUser2Result = await pubnub.CreateUser().Id(userId2).Name("pandu-ut-un2").ExecuteAsync();
                if (createUser2Result.Result != null && createUser2Result.Status.StatusCode == 200 && !createUser2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser2Result.Result);
                    if (userId2 == createUser2Result.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "CreateSpace"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() STARTED");
                PNResult<PNCreateSpaceResult> createSpaceResult = await pubnub.CreateSpace().Id(spaceId).Name("pandu-ut-spname").ExecuteAsync();
                if (createSpaceResult.Result != null && createSpaceResult.Status.StatusCode == 200 && !createSpaceResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpaceResult.Result);
                    if (spaceId == createSpaceResult.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "Members Add"
                System.Diagnostics.Debug.WriteLine("pubnub.Members() ADD STARTED");
                PNResult<PNManageMembersResult> manageMemberResult = await pubnub.ManageMembers().SpaceId(spaceId)
                    .Add(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1 },
                            new PNMember() { UserId = userId2 }
                    })
                    .Include(new PNMemberField[] { PNMemberField.CUSTOM, PNMemberField.USER, PNMemberField.USER_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPage() { Next = "", Prev = "" })
                    .ExecuteAsync();
                if (manageMemberResult.Result != null && manageMemberResult.Status.StatusCode == 200 && !manageMemberResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMemberResult.Result);
                    if (manageMemberResult.Result.Members != null
                    && manageMemberResult.Result.Members.Find(x => x.UserId == userId1) != null
                    && manageMemberResult.Result.Members.Find(x => x.UserId == userId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage && !string.IsNullOrEmpty(config.SecretKey))
            {
                receivedMessage = false;
                #region "Members Update"
                System.Diagnostics.Debug.WriteLine("pubnub.Members() UPDATE STARTED");
                PNResult<PNManageMembersResult> manageMmbrUpdResult = await pubnub.ManageMembers().SpaceId(spaceId)
                    .Update(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMember() { UserId = userId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Include(new PNMemberField[] { PNMemberField.CUSTOM, PNMemberField.USER, PNMemberField.USER_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPage() { Next = "", Prev = "" })
                    .ExecuteAsync();
                if (manageMmbrUpdResult.Result != null && manageMmbrUpdResult.Status.StatusCode == 200 && !manageMmbrUpdResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrUpdResult.Result);
                    if (manageMmbrUpdResult.Result.Members != null
                    && manageMmbrUpdResult.Result.Members.Find(x => x.UserId == userId1) != null
                    && manageMmbrUpdResult.Result.Members.Find(x => x.UserId == userId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "Members Remove"
                System.Diagnostics.Debug.WriteLine("pubnub.Members() REMOVE STARTED");
                PNResult<PNManageMembersResult> manageMmbrDelResult = await pubnub.ManageMembers().SpaceId(spaceId)
                    .Remove(new List<string>() { userId2 })
                    .Include(new PNMemberField[] { PNMemberField.CUSTOM, PNMemberField.USER, PNMemberField.USER_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPage() { Next = "", Prev = "" })
                    .ExecuteAsync();
                if (manageMmbrDelResult.Result != null && manageMmbrDelResult.Status.StatusCode == 200 && !manageMmbrDelResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrDelResult.Result);
                    if (manageMmbrDelResult.Result.Members != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "GetMembers"
                System.Diagnostics.Debug.WriteLine("pubnub.GetMembers() STARTED");
                PNResult<PNGetMembersResult> getMbrsResult = await pubnub.GetMembers().SpaceId(spaceId)
                    .ExecuteAsync();
                if (getMbrsResult.Result != null && getMbrsResult.Status.StatusCode == 200 && !getMbrsResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getMbrsResult.Result);
                    if (getMbrsResult.Result.Members != null
                    && getMbrsResult.Result.Members.Find(x => x.UserId == userId1) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "CreateUser/CreateSpace/Member AddUpdateRemove Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }


        [Test]
        public static void ThenMemberAddUpdateRemoveShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenMemberAddUpdateRemoveShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedCreateEvent = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string spaceId = "pandu-ut-sid";
            string userId1 = "pandu-ut-uid1";
            string userId2 = "pandu-ut-uid2";

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectApiEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "membership")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "create")
                        {
                            receivedCreateEvent = true;
                        }
                        else if (eventResult.Event.ToLowerInvariant() == "update")
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
            pubnub.Subscribe<string>().Channels(new string[] { userId1, userId2, spaceId }).Execute();
            manualEvent.WaitOne(2000);


            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 1 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.DeleteUser().Id(userId1).Execute(new PNDeleteUserResultExt(
                delegate (PNDeleteUserResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 2 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.DeleteUser().Id(userId2).Execute(new PNDeleteUserResultExt(
                delegate (PNDeleteUserResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.DeleteSpace().Id(spaceId).Execute(new PNDeleteSpaceResultExt(
                delegate (PNDeleteSpaceResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
            #region "CreateUser 1"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() 1 STARTED");
            pubnub.CreateUser().Id(userId1).Name("pandu-ut-un1")
                    .Execute(new PNCreateUserResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (userId1 == r.Id)
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
                #region "CreateUser 2"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() 2 STARTED");
                pubnub.CreateUser().Id(userId2).Name("pandu-ut-un2")
                        .Execute(new PNCreateUserResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (userId2 == r.Id)
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
                #region "CreateSpace"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() STARTED");
                pubnub.CreateSpace().Id(spaceId).Name("pandu-ut-spname")
                        .Execute(new PNCreateSpaceResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (spaceId == r.Id)
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
                #region "Members Add"
                System.Diagnostics.Debug.WriteLine("pubnub.Members() ADD STARTED");
                pubnub.ManageMembers().SpaceId(spaceId)
                    .Add(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1 },
                            new PNMember() { UserId = userId2 }
                    })
                    .Execute(new PNManageMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Members != null
                            && r.Members.Find(x => x.UserId == userId1) != null
                            && r.Members.Find(x => x.UserId == userId2) != null)
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
                #region "Members Update/Remove"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Members() UPDATE/REMOVE STARTED");
                    pubnub.ManageMembers().SpaceId(spaceId)
                        .Update(new List<PNMember>()
                                {
                            new PNMember() { UserId = userId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { userId2 })
                        .Execute(new PNManageMembersResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (r.Members != null
                                && r.Members.Find(x => x.UserId == userId1) != null)
                                {
                                    receivedMessage = true;
                                }
                            }
                            manualEvent.Set();
                        }));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Members() REMOVE STARTED");
                    pubnub.ManageMembers().SpaceId(spaceId)
                        .Remove(new List<string>() { userId2 })
                        .Execute(new PNManageMembersResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                receivedMessage = true;
                            }
                            manualEvent.Set();
                        }));
                }
                #endregion
                manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { userId1, userId2, spaceId }).Execute();
            pubnub.RemoveListener(eventListener);

            if (!string.IsNullOrEmpty(config.SecretKey))
            {
                Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent && receivedCreateEvent, "Member events Failed");
            }
            else
            {
                Assert.IsTrue(receivedDeleteEvent && receivedCreateEvent, "Member events Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenWithAsyncMemberAddUpdateRemoveShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncMemberAddUpdateRemoveShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedCreateEvent = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string spaceId = "pandu-ut-sid";
            string userId1 = "pandu-ut-uid1";
            string userId2 = "pandu-ut-uid2";

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectApiEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "membership")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "create")
                        {
                            receivedCreateEvent = true;
                        }
                        else if (eventResult.Event.ToLowerInvariant() == "update")
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
            pubnub.Subscribe<string>().Channels(new string[] { userId1, userId2, spaceId }).Execute();
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 1 STARTED");
            await pubnub.DeleteUser().Id(userId1).ExecuteAsync();

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() 2 STARTED");
            await pubnub.DeleteUser().Id(userId2).ExecuteAsync();

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
            await pubnub.DeleteSpace().Id(spaceId).ExecuteAsync();

            receivedMessage = false;
            #region "CreateUser 1"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() 1 STARTED");
            PNResult<PNCreateUserResult> createUser1Result = await pubnub.CreateUser().Id(userId1).Name("pandu-ut-un1").ExecuteAsync();
            if (createUser1Result.Result != null && createUser1Result.Status.StatusCode == 200 && !createUser1Result.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser1Result.Result);
                if (userId1 == createUser1Result.Result.Id)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            /* */
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "CreateUser 2"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() 2 STARTED");
                PNResult<PNCreateUserResult> createUser2Result = await pubnub.CreateUser().Id(userId2).Name("pandu-ut-un2").ExecuteAsync();
                if (createUser2Result.Result != null && createUser2Result.Status.StatusCode == 200 && !createUser2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser2Result.Result);
                    if (userId2 == createUser2Result.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "CreateSpace"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() STARTED");
                PNResult<PNCreateSpaceResult> createSpaceResult = await pubnub.CreateSpace().Id(spaceId).Name("pandu-ut-spname").ExecuteAsync();
                if (createSpaceResult.Result != null && createSpaceResult.Status.StatusCode == 200 && !createSpaceResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpaceResult.Result);
                    if (spaceId == createSpaceResult.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "Members Add"
                System.Diagnostics.Debug.WriteLine("pubnub.Members() ADD STARTED");
                PNResult<PNManageMembersResult> manageMemberResult = await pubnub.ManageMembers().SpaceId(spaceId)
                    .Add(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1 },
                            new PNMember() { UserId = userId2 }
                    })
                    .ExecuteAsync();
                if (manageMemberResult.Result != null && manageMemberResult.Status.StatusCode == 200 && !manageMemberResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMemberResult.Result);
                    if (manageMemberResult.Result.Members != null
                    && manageMemberResult.Result.Members.Find(x => x.UserId == userId1) != null
                    && manageMemberResult.Result.Members.Find(x => x.UserId == userId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "Members Update/Remove"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Members() UPDATE/REMOVE STARTED");
                    PNResult<PNManageMembersResult> manageMmbrUpdResult = await pubnub.ManageMembers().SpaceId(spaceId)
                        .Update(new List<PNMember>()
                                {
                            new PNMember() { UserId = userId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { userId2 })
                        .ExecuteAsync();
                    if (manageMmbrUpdResult.Result != null && manageMmbrUpdResult.Status.StatusCode == 200 && !manageMmbrUpdResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrUpdResult.Result);
                        if (manageMmbrUpdResult.Result.Members != null
                        && manageMmbrUpdResult.Result.Members.Find(x => x.UserId == userId1) != null)
                        {
                            receivedMessage = true;
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Members() REMOVE STARTED");
                    PNResult<PNManageMembersResult> manageMmbrDelResult = await pubnub.ManageMembers().SpaceId(spaceId)
                        .Remove(new List<string>() { userId2 })
                        .ExecuteAsync();
                    if (manageMmbrDelResult.Result != null && manageMmbrDelResult.Status.StatusCode == 200 && !manageMmbrDelResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrDelResult.Result);
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            Thread.Sleep(2000);
            /* */
            pubnub.Unsubscribe<string>().Channels(new string[] { userId1, userId2, spaceId }).Execute();
            pubnub.RemoveListener(eventListener);

            if (!string.IsNullOrEmpty(config.SecretKey))
            {
                Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent && receivedCreateEvent, "Member events Failed");
            }
            else
            {
                Assert.IsTrue(receivedDeleteEvent && receivedCreateEvent, "Member events Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
    }
}
