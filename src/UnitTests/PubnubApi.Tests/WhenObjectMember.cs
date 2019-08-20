using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using PubnubApi.Tests;
using System.Diagnostics;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenObjectMember : TestHarness
    {
        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;
        private static Server server;

        [TestFixtureSetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();

            if (!PubnubCommon.PAMEnabled) { return; }

            if (PubnubCommon.PAMEnabled && string.IsNullOrEmpty(PubnubCommon.SecretKey))
            {
                return;
            }

            bool receivedGrantMessage = false;
            string channel = "hello_my_channel";
            string authKey = "myAuth";

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
            pubnub.Grant().Channels(new[] { channel }).AuthKeys(new[] { authKey }).Read(true).Write(true).Manage(true).TTL(20)
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
                                                var read = r.Channels[channel][authKey].ReadEnabled;
                                                var write = r.Channels[channel][authKey].WriteEnabled;
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
            Assert.IsTrue(receivedGrantMessage, "WhenAMessageIsPublished Grant access failed.");
        }

        [TestFixtureTearDown]
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
            if (PubnubCommon.PAMEnabled)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
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
                pubnub.Members().SpaceId(spaceId)
                    .Add(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1 },
                            new PNMember() { UserId = userId2 }
                    })
                    .Execute(new PNMembersResultExt((r, s) =>
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
                #region "Members Update"
                System.Diagnostics.Debug.WriteLine("pubnub.Members() UPDATE STARTED");
                pubnub.Members().SpaceId(spaceId)
                    .Update(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMember() { UserId = userId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Execute(new PNMembersResultExt((r, s) =>
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
                pubnub.Members().SpaceId(spaceId)
                    .Remove(new List<string>() { userId2 })
                    .Execute(new PNMembersResultExt((r, s) =>
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
                Secure = false
            };
            if (PubnubCommon.PAMEnabled)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
            pubnub.AddListener(eventListener);

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { "pnuser-" + userId1, "pnuser-" + userId2, spaceId }).Execute();
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
                pubnub.Members().SpaceId(spaceId)
                    .Add(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1 },
                            new PNMember() { UserId = userId2 }
                    })
                    .Execute(new PNMembersResultExt((r, s) =>
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
                System.Diagnostics.Debug.WriteLine("pubnub.Members() UPDATE/REMOVE STARTED");
                pubnub.Members().SpaceId(spaceId)
                    .Update(new List<PNMember>()
                            {
                            new PNMember() { UserId = userId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                    })
                    .Remove(new List<string>() { userId2 })
                    .Execute(new PNMembersResultExt((r, s) =>
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

            pubnub.Unsubscribe<string>().Channels(new string[] { "pnuser-" + userId1, "pnuser-" + userId2, spaceId }).Execute();
            pubnub.RemoveListener(eventListener);

            if (!receivedDeleteEvent || !receivedUpdateEvent || !receivedCreateEvent)
            {
                Assert.IsTrue(receivedMessage, "Member events Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
    }
}
