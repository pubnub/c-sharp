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
    public class WhenObjectSpace : TestHarness
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
            string spaceId = "pandu-ut-sid";

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
            Assert.IsTrue(receivedGrantMessage, "WhenObjectSpace Grant access failed.");
        }

        [TestFixtureTearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenSpaceCRUDShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSpaceCRUDShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string spaceId = "pandu-ut-sid";

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
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
            pubnub.DeleteSpace().Id(spaceId).Execute(new PNDeleteSpaceResultExt(
                delegate (PNDeleteSpaceResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

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

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "UpdateSpace"
                System.Diagnostics.Debug.WriteLine("pubnub.UpdateSpace() STARTED");
                pubnub.UpdateSpace().Id(spaceId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .CustomObject(new Dictionary<string, object>() { { "color", "red" } })
                        .Execute(new PNUpdateSpaceResultExt((r, s) =>
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
                #region "GetSpace"
                System.Diagnostics.Debug.WriteLine("pubnub.GetSpace() STARTED");
                pubnub.GetSpace().SpaceId(spaceId).IncludeCustom(true)
                    .Execute(new PNGetSpaceResultExt((r, s) =>
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
                #region "GetSpaces"
                System.Diagnostics.Debug.WriteLine("pubnub.GetSpaces() STARTED");
                pubnub.GetSpaces().IncludeCount(true)
                    .Execute(new PNGetSpacesResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            List<PNSpaceResult> spaceList = r.Spaces;
                            if (spaceList != null && spaceList.Count > 0 && spaceList.Find(x => x.Id == spaceId) != null)
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
        public static void ThenSpaceUpdateDeleteShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSpaceUpdateDeleteShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;

            string spaceId = "pandu-ut-sid";
            manualResetEventWaitTimeout = 310 * 1000;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectApiEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "space")
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
            pubnub.Subscribe<string>().Channels(new string[] { spaceId }).Execute();
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
            pubnub.DeleteSpace().Id(spaceId).Execute(new PNDeleteSpaceResultExt(
                delegate (PNDeleteSpaceResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

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

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "UpdateSpace"
                System.Diagnostics.Debug.WriteLine("pubnub.UpdateSpace() STARTED");
                pubnub.UpdateSpace().Id(spaceId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .CustomObject(new Dictionary<string, object>() { { "color", "red" } })
                        .Execute(new PNUpdateSpaceResultExt((r, s) =>
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

            if (!receivedDeleteEvent)
            {
                manualEvent = new ManualResetEvent(false);
                System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() 2 STARTED");
                pubnub.DeleteSpace().Id(spaceId).Execute(new PNDeleteSpaceResultExt(
                    delegate (PNDeleteSpaceResult result, PNStatus status) { }));
                manualEvent.WaitOne(2000);
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { spaceId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent, "Space events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

        }
    }
}
