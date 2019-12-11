using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;

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

        [TestFixtureSetUp]
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
            Assert.IsTrue(receivedGrantMessage, "WhenAMessageIsPublished Grant access failed.");
        }

        [TestFixtureTearDown]
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
    }
}
