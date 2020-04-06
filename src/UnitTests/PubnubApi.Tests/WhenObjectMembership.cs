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
    public class WhenObjectMembership : TestHarness
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
            string spaceId1 = "pandu-ut-sid1";
            string spaceId2 = "pandu-ut-sid2";

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
                .Spaces(new Dictionary<string, PNResourcePermission>() { { spaceId1, perm }, { spaceId2, perm } })
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
        public static void ThenAddUpdateRemoveSpaceShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenAddUpdateRemoveSpaceShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string userId = "pandu-ut-uid";
            string spaceId1 = "pandu-ut-sid1";
            string spaceId2 = "pandu-ut-sid2";

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
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() STARTED");
            pubnub.DeleteUser().Id(userId).Execute(new PNDeleteUserResultExt(
                delegate (PNDeleteUserResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() 1 STARTED");
            pubnub.DeleteSpace().Id(spaceId1).Execute(new PNDeleteSpaceResultExt(
                delegate (PNDeleteSpaceResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() 2 STARTED");
            pubnub.DeleteSpace().Id(spaceId2).Execute(new PNDeleteSpaceResultExt(
                delegate (PNDeleteSpaceResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
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
                #region "CreateSpace 1"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() 1 STARTED");
                pubnub.CreateSpace().Id(spaceId1).Name("pandu-ut-spname")
                        .Execute(new PNCreateSpaceResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (spaceId1 == r.Id)
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
                #region "CreateSpace 2"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() 2 STARTED");
                pubnub.CreateSpace().Id(spaceId2).Name("pandu-ut-spname")
                        .Execute(new PNCreateSpaceResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (spaceId2 == r.Id)
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
                #region "Memberships Add"
                System.Diagnostics.Debug.WriteLine("pubnub.Memberships() ADD STARTED");
                pubnub.ManageMemberships().UserId(userId)
                    .Add(new List<PNMembership>()
                            {
                            new PNMembership() { SpaceId = spaceId1 },
                            new PNMembership() { SpaceId = spaceId2 }
                    })
                    .Execute(new PNManageMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null 
                            && r.Memberships.Find(x=> x.SpaceId == spaceId1) != null
                            && r.Memberships.Find(x => x.SpaceId == spaceId2) != null)
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
                #region "Memberships Update"
                System.Diagnostics.Debug.WriteLine("pubnub.Memberships() UPDATE STARTED");
                pubnub.ManageMemberships().UserId(userId)
                    .Update(new List<PNMembership>()
                            {
                            new PNMembership() { SpaceId = spaceId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMembership() { SpaceId = spaceId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Execute(new PNManageMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.SpaceId == spaceId1) != null
                            && r.Memberships.Find(x => x.SpaceId == spaceId2) != null)
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
                #region "Memberships Remove"
                System.Diagnostics.Debug.WriteLine("pubnub.Memberships() REMOVE STARTED");
                pubnub.ManageMemberships().UserId(userId)
                    .Remove(new List<string>() { spaceId2 })
                    .Execute(new PNManageMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null)
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
                #region "GetMemberships"
                System.Diagnostics.Debug.WriteLine("pubnub.GetMemberships() STARTED");
                pubnub.GetMemberships().UserId(userId)
                    .Execute(new PNGetMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.SpaceId == spaceId1) != null)
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
                Assert.IsTrue(receivedMessage, "CreateUser/CreateSpace/Membership AddUpdateRemove Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenWithAsyncAddUpdateRemoveSpaceShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncAddUpdateRemoveSpaceShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string userId = "pandu-ut-uid";
            string spaceId1 = "pandu-ut-sid1";
            string spaceId2 = "pandu-ut-sid2";

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

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() STARTED");
            await pubnub.DeleteUser().Id(userId).ExecuteAsync();

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() 1 STARTED");
            await pubnub.DeleteSpace().Id(spaceId1).ExecuteAsync();

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() 2 STARTED");
            await pubnub.DeleteSpace().Id(spaceId2).ExecuteAsync();

            receivedMessage = false;
            #region "CreateUser"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() STARTED");
            PNResult<PNCreateUserResult> createUserResult = await pubnub.CreateUser().Id(userId).Name("pandu-ut-un").ExecuteAsync();
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
                #region "CreateSpace 1"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() 1 STARTED");
                PNResult<PNCreateSpaceResult> createSpace1Result = await pubnub.CreateSpace().Id(spaceId1).Name("pandu-ut-spname").ExecuteAsync();
                if (createSpace1Result.Result != null && createSpace1Result.Status.StatusCode == 200 && !createSpace1Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace1Result.Result);
                    if (spaceId1 == createSpace1Result.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "CreateSpace 2"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() 2 STARTED");
                PNResult<PNCreateSpaceResult> createSpace2Result = await pubnub.CreateSpace().Id(spaceId2).Name("pandu-ut-spname").ExecuteAsync();
                if (createSpace2Result.Result != null && createSpace2Result.Status.StatusCode == 200 && !createSpace2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace2Result.Result);
                    if (spaceId2 == createSpace2Result.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "Memberships Add"
                System.Diagnostics.Debug.WriteLine("pubnub.Memberships() ADD STARTED");
                PNResult<PNManageMembershipsResult> manageMbrshipAddResult = await pubnub.ManageMemberships().UserId(userId)
                    .Add(new List<PNMembership>()
                            {
                            new PNMembership() { SpaceId = spaceId1 },
                            new PNMembership() { SpaceId = spaceId2 }
                    })
                    .ExecuteAsync();
                if (manageMbrshipAddResult.Result != null && manageMbrshipAddResult.Status.StatusCode == 200 && !manageMbrshipAddResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipAddResult.Result);
                    if (manageMbrshipAddResult.Result.Memberships != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.SpaceId == spaceId1) != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.SpaceId == spaceId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage && !string.IsNullOrEmpty(config.SecretKey))
            {
                receivedMessage = false;
                #region "Memberships Update"
                System.Diagnostics.Debug.WriteLine("pubnub.Memberships() UPDATE STARTED");
                PNResult<PNManageMembershipsResult> manageMbrshipUpdResult = await pubnub.ManageMemberships().UserId(userId)
                    .Update(new List<PNMembership>()
                            {
                            new PNMembership() { SpaceId = spaceId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMembership() { SpaceId = spaceId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .ExecuteAsync();
                if (manageMbrshipUpdResult.Result != null && manageMbrshipUpdResult.Status.StatusCode == 200 && !manageMbrshipUpdResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipUpdResult.Result);
                    if (manageMbrshipUpdResult.Result.Memberships != null
                    && manageMbrshipUpdResult.Result.Memberships.Find(x => x.SpaceId == spaceId1) != null
                    && manageMbrshipUpdResult.Result.Memberships.Find(x => x.SpaceId == spaceId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "Memberships Remove"
                System.Diagnostics.Debug.WriteLine("pubnub.Memberships() REMOVE STARTED");
                PNResult<PNManageMembershipsResult> manageMbrshipDelResult = await pubnub.ManageMemberships().UserId(userId)
                    .Remove(new List<string>() { spaceId2 })
                    .ExecuteAsync();
                if (manageMbrshipDelResult.Result != null && manageMbrshipDelResult.Status.StatusCode == 200 && !manageMbrshipDelResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipDelResult.Result);
                    if (manageMbrshipDelResult.Result.Memberships != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "GetMemberships"
                System.Diagnostics.Debug.WriteLine("pubnub.GetMemberships() STARTED");
                PNResult<PNGetMembershipsResult> getMbrshipResult = await pubnub.GetMemberships().UserId(userId).ExecuteAsync();
                if (getMbrshipResult.Result != null && getMbrshipResult.Status.StatusCode == 200 && !getMbrshipResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getMbrshipResult.Result);
                    if (getMbrshipResult.Result.Memberships != null
                    && getMbrshipResult.Result.Memberships.Find(x => x.SpaceId == spaceId1) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "With Async CreateUser/CreateSpace/Membership AddUpdateRemove Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenMembershipAddUpdateRemoveShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenMembershipAddUpdateRemoveShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedCreateEvent = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string userId = "pandu-ut-uid";
            string spaceId1 = "pandu-ut-sid1";
            string spaceId2 = "pandu-ut-sid2";

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
            pubnub.Subscribe<string>().Channels(new string[] { userId, spaceId1, spaceId2 }).Execute();
            manualEvent.WaitOne(2000);


            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.DeleteUser().Id(userId).Execute(new PNDeleteUserResultExt(
                delegate (PNDeleteUserResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() 1 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.DeleteSpace().Id(spaceId1).Execute(new PNDeleteSpaceResultExt(
                delegate (PNDeleteSpaceResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() 2 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.DeleteSpace().Id(spaceId2).Execute(new PNDeleteSpaceResultExt(
                delegate (PNDeleteSpaceResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
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
                #region "CreateSpace 1"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() 1 STARTED");
                pubnub.CreateSpace().Id(spaceId1).Name("pandu-ut-spname")
                        .Execute(new PNCreateSpaceResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (spaceId1 == r.Id)
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
                #region "CreateSpace 2"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() 2 STARTED");
                pubnub.CreateSpace().Id(spaceId2).Name("pandu-ut-spname")
                        .Execute(new PNCreateSpaceResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (spaceId2 == r.Id)
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
                #region "Memberships Add"
                System.Diagnostics.Debug.WriteLine("pubnub.Memberships() ADD STARTED");
                pubnub.ManageMemberships().UserId(userId)
                    .Add(new List<PNMembership>()
                            {
                            new PNMembership() { SpaceId = spaceId1 },
                            new PNMembership() { SpaceId = spaceId2 }
                    })
                    .Execute(new PNManageMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.SpaceId == spaceId1) != null
                            && r.Memberships.Find(x => x.SpaceId == spaceId2) != null)
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
                #region "Memberships Update/Remove"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Memberships() UPDATE/REMOVE STARTED");
                    pubnub.ManageMemberships().UserId(userId)
                        .Update(new List<PNMembership>()
                                {
                            new PNMembership() { SpaceId = spaceId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { spaceId2 })
                        .Execute(new PNManageMembershipsResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (r.Memberships != null
                                && r.Memberships.Find(x => x.SpaceId == spaceId1) != null)
                                {
                                    receivedMessage = true;
                                }
                            }
                            manualEvent.Set();
                        }));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Memberships() REMOVE STARTED");
                    pubnub.ManageMemberships().UserId(userId)
                        .Remove(new List<string>() { spaceId2 })
                        .Execute(new PNManageMembershipsResultExt((r, s) =>
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

            pubnub.Unsubscribe<string>().Channels(new string[] { userId, spaceId1, spaceId2 }).Execute();
            pubnub.RemoveListener(eventListener);

            if (!string.IsNullOrEmpty(config.SecretKey))
            {
                Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent && receivedCreateEvent, "Membership events Failed");
            }
            else
            {
                Assert.IsTrue(receivedDeleteEvent && receivedCreateEvent, "Membership events Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenWithAsyncMembershipAddUpdateRemoveShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncMembershipAddUpdateRemoveShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedCreateEvent = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string userId = "pandu-ut-uid";
            string spaceId1 = "pandu-ut-sid1";
            string spaceId2 = "pandu-ut-sid2";

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
            pubnub.Subscribe<string>().Channels(new string[] { userId, spaceId1, spaceId2 }).Execute();
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUser() STARTED");
            await pubnub.DeleteUser().Id(userId).ExecuteAsync();

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() 1 STARTED");
            await pubnub.DeleteSpace().Id(spaceId1).ExecuteAsync();

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() 2 STARTED");
            await pubnub.DeleteSpace().Id(spaceId2).ExecuteAsync();

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
            #region "CreateUser"
            System.Diagnostics.Debug.WriteLine("pubnub.CreateUser() STARTED");
            PNResult<PNCreateUserResult> createUserResult = await pubnub.CreateUser().Id(userId).Name("pandu-ut-un").ExecuteAsync();
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
                #region "CreateSpace 1"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() 1 STARTED");
                PNResult<PNCreateSpaceResult> createSpace1Result = await pubnub.CreateSpace().Id(spaceId1).Name("pandu-ut-spname").ExecuteAsync();
                if (createSpace1Result.Result != null && createSpace1Result.Status.StatusCode == 200 && !createSpace1Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace1Result.Result);
                    if (spaceId1 == createSpace1Result.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "CreateSpace 2"
                System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() 2 STARTED");
                PNResult<PNCreateSpaceResult> createSpace2Result = await pubnub.CreateSpace().Id(spaceId2).Name("pandu-ut-spname").ExecuteAsync();
                if (createSpace2Result.Result != null && createSpace2Result.Status.StatusCode == 200 && !createSpace2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace2Result.Result);
                    if (spaceId2 == createSpace2Result.Result.Id)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "Memberships Add"
                System.Diagnostics.Debug.WriteLine("pubnub.Memberships() ADD STARTED");
                PNResult<PNManageMembershipsResult> manageMbrshipAddResult = await pubnub.ManageMemberships().UserId(userId)
                    .Add(new List<PNMembership>()
                            {
                            new PNMembership() { SpaceId = spaceId1 },
                            new PNMembership() { SpaceId = spaceId2 }
                    })
                    .ExecuteAsync();
                if (manageMbrshipAddResult.Result != null && manageMbrshipAddResult.Status.StatusCode == 200 && !manageMbrshipAddResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipAddResult.Result);
                    if (manageMbrshipAddResult.Result.Memberships != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.SpaceId == spaceId1) != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.SpaceId == spaceId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
                manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "Memberships Update/Remove"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Memberships() UPDATE/REMOVE STARTED");
                    PNResult<PNManageMembershipsResult> manageMbrshipUpdResult = await pubnub.ManageMemberships().UserId(userId)
                        .Update(new List<PNMembership>()
                                {
                            new PNMembership() { SpaceId = spaceId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { spaceId2 })
                        .ExecuteAsync();
                    if (manageMbrshipUpdResult.Result != null && manageMbrshipUpdResult.Status.StatusCode == 200 && !manageMbrshipUpdResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipUpdResult.Result);
                        if (manageMbrshipUpdResult.Result.Memberships != null
                        && manageMbrshipUpdResult.Result.Memberships.Find(x => x.SpaceId == spaceId1) != null)
                        {
                            receivedMessage = true;
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Memberships() REMOVE STARTED");
                    PNResult<PNManageMembershipsResult> manageMbrshipDelResult = await pubnub.ManageMemberships().UserId(userId)
                        .Remove(new List<string>() { spaceId2 })
                        .ExecuteAsync();
                    if (manageMbrshipDelResult.Result != null && manageMbrshipDelResult.Status.StatusCode == 200 && !manageMbrshipDelResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipDelResult.Result);
                        receivedMessage = true;
                    }
                }
                #endregion
                manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { userId, spaceId1, spaceId2 }).Execute();
            pubnub.RemoveListener(eventListener);

            if (!string.IsNullOrEmpty(config.SecretKey))
            {
                Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent && receivedCreateEvent, "Membership events Failed");
            }
            else
            {
                Assert.IsTrue(receivedDeleteEvent && receivedCreateEvent, "With Async Membership events Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
    }
}
