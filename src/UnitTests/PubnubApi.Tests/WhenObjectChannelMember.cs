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
    public class WhenObjectChannelMember : TestHarness
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
            string channelMetadataId = "pandu-ut-sid";
            string uuidMetadataId1 = "pandu-ut-uid1";
            string uuidMetadataId2 = "pandu-ut-uid2";

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
                    .Resources(new PNTokenResources()
                    {
                        Channels = new Dictionary<string, PNTokenAuthValues>() {
                                            { channelMetadataId, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } },
                                            { uuidMetadataId1, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } },
                                            { uuidMetadataId2, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } }},
                        Uuids = new Dictionary<string, PNTokenAuthValues>() {
                                            { uuidMetadataId1, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } },
                                            { uuidMetadataId2, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } }},
                    }
                    )
                .TTL(20)
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
            Assert.IsTrue(receivedGrantMessage, "WhenObjectChannelMember Grant access failed.");
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
        public static void ThenSetRemoveUuidMetadataWithManageMemberShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSetRemoveUuidMetadataWithManageMemberShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string channelMetadataId = "pandu-ut-sid";
            string uuidMetadataId1 = "pandu-ut-uid1";
            string uuidMetadataId2 = "pandu-ut-uid2";

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

            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 1 STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 2 STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
            #region "SetUuidMetadata 1"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 1 STARTED");
            pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1")
                    .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (uuidMetadataId1 == r.Uuid)
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
                #region "SetUuidMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 2 STARTED");
                pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2")
                        .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (uuidMetadataId2 == r.Uuid)
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
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "ManageMembers Set"
                System.Diagnostics.Debug.WriteLine("pubnub.ManageMembers() SET STARTED");
                pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Set(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.ChannelMembers != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
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
                #region "ManageChannelMembers for Update"
                System.Diagnostics.Debug.WriteLine("pubnub.ManageChannelMembers() UPDATE STARTED");
                pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Set(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNChannelMember() { Uuid = uuidMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.ChannelMembers != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
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
                #region "ManageChannelMembers"
                System.Diagnostics.Debug.WriteLine("pubnub.ManageChannelMembers() STARTED");
                pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Remove(new List<string>() { uuidMetadataId2 })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.ChannelMembers != null)
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
                #region "GetChannelMembers"
                System.Diagnostics.Debug.WriteLine("pubnub.GetMembers() STARTED");
                pubnub.GetChannelMembers().Channel(channelMetadataId)
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.ChannelMembers != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null)
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
                Assert.IsTrue(receivedMessage, "Set/Remove UuidMetadata With Manage Member Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncSetRemoveUuidMetadataWithManageMemberShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncSetRemoveUuidMetadataWithManageMemberShouldReturnSuccessCodeAndInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncSetRemoveUuidMetadataWithManageMemberShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string channelMetadataId = "pandu-ut-sid";
            string uuidMetadataId1 = "pandu-ut-uid1";
            string uuidMetadataId2 = "pandu-ut-uid2";

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

            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 1 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).ExecuteAsync());
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 2 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).ExecuteAsync());
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync());
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif

            receivedMessage = false;
            #region "SetUuidMetadata 1"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 1 STARTED");
#if NET40
            PNResult<PNSetUuidMetadataResult> createUser1Result = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetUuidMetadataResult> createUser1Result = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1").ExecuteAsync();
#endif
            if (createUser1Result.Result != null && createUser1Result.Status.StatusCode == 200 && !createUser1Result.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser1Result.Result);
                if (uuidMetadataId1 == createUser1Result.Result.Uuid)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetUuidMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 2 STARTED");
#if NET40
                PNResult<PNSetUuidMetadataResult> createUser2Result = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetUuidMetadataResult> createUser2Result = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2").ExecuteAsync();
#endif
                if (createUser2Result.Result != null && createUser2Result.Status.StatusCode == 200 && !createUser2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser2Result.Result);
                    if (uuidMetadataId2 == createUser2Result.Result.Uuid)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
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
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "ManageChannelMembers Add"
                System.Diagnostics.Debug.WriteLine("pubnub.ManageChannelMembers() SET STARTED");
#if NET40
                PNResult<PNChannelMembersResult> manageMemberResult = Task.Factory.StartNew(async () => await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Set(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> manageMemberResult = await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Set(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync();
#endif
                if (manageMemberResult.Result != null && manageMemberResult.Status.StatusCode == 200 && !manageMemberResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMemberResult.Result);
                    if (manageMemberResult.Result.ChannelMembers != null
                    && manageMemberResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                    && manageMemberResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage && !string.IsNullOrEmpty(config.SecretKey))
            {
                receivedMessage = false;
                #region "ManageChannelMembers Update"
                System.Diagnostics.Debug.WriteLine("pubnub.ManageMembers() SET for UPDATE STARTED");
#if NET40
                PNResult<PNChannelMembersResult> manageMmbrUpdResult = Task.Factory.StartNew(async () => await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Set(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNChannelMember() { Uuid = uuidMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> manageMmbrUpdResult = await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Set(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNChannelMember() { Uuid = uuidMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync();
#endif
                if (manageMmbrUpdResult.Result != null && manageMmbrUpdResult.Status.StatusCode == 200 && !manageMmbrUpdResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrUpdResult.Result);
                    if (manageMmbrUpdResult.Result.ChannelMembers != null
                    && manageMmbrUpdResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                    && manageMmbrUpdResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "ManageChannelMembers Remove"
                System.Diagnostics.Debug.WriteLine("pubnub.ManageChannelMembers() REMOVE STARTED");
#if NET40
                PNResult<PNChannelMembersResult> manageMmbrDelResult = Task.Factory.StartNew(async () => await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Remove(new List<string>() { uuidMetadataId2 })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> manageMmbrDelResult = await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Remove(new List<string>() { uuidMetadataId2 })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync();
#endif
                if (manageMmbrDelResult.Result != null && manageMmbrDelResult.Status.StatusCode == 200 && !manageMmbrDelResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrDelResult.Result);
                    if (manageMmbrDelResult.Result.ChannelMembers != null)
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
#if NET40
                PNResult<PNChannelMembersResult> getMbrsResult = Task.Factory.StartNew(async () => await pubnub.GetChannelMembers().Channel(channelMetadataId)
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> getMbrsResult = await pubnub.GetChannelMembers().Channel(channelMetadataId)
                    .ExecuteAsync();
#endif
                if (getMbrsResult.Result != null && getMbrsResult.Status.StatusCode == 200 && !getMbrsResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getMbrsResult.Result);
                    if (getMbrsResult.Result.ChannelMembers != null
                    && getMbrsResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "SetUuidMetadata/SetChannelMetadata/ Manage ChannelMember Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }


        [Test]
        public static void ThenSetRemoveUuidMetadataWithSetRemoveMemberShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSetRemoveUuidMetadataWithSetRemoveMemberShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string channelMetadataId = "pandu-ut-sid";
            string uuidMetadataId1 = "pandu-ut-uid1";
            string uuidMetadataId2 = "pandu-ut-uid2";

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

            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 1 STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 2 STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
            #region "SetUuidMetadata 1"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 1 STARTED");
            pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1")
                    .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (uuidMetadataId1 == r.Uuid)
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
                #region "SetUuidMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 2 STARTED");
                pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2")
                        .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (uuidMetadataId2 == r.Uuid)
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
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "SetChannelMembers Set"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMembers() SET STARTED");
                pubnub.SetChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.ChannelMembers != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
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
                #region "SetChannelMembers for Update"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMembers() UPDATE STARTED");
                pubnub.SetChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNChannelMember() { Uuid = uuidMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.ChannelMembers != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
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
                #region "RemoveChannelMembers"
                System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMembers() STARTED");
                pubnub.RemoveChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<string>() { uuidMetadataId2 })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.ChannelMembers != null)
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
                #region "GetChannelMembers"
                System.Diagnostics.Debug.WriteLine("pubnub.GetMembers() STARTED");
                pubnub.GetChannelMembers().Channel(channelMetadataId)
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.ChannelMembers != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null)
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
                Assert.IsTrue(receivedMessage, "Set/Remove UuidMetadata With Set/Remove Member Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }


        [Test]
#if NET40
        public static void ThenWithAsyncSetRemoveUuidMetadataWithSetRemoveMemberShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncSetRemoveUuidMetadataWithSetRemoveMemberShouldReturnSuccessCodeAndInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncSetRemoveUuidMetadataWithSetRemoveMemberShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string channelMetadataId = "pandu-ut-sid";
            string uuidMetadataId1 = "pandu-ut-uid1";
            string uuidMetadataId2 = "pandu-ut-uid2";

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

            server.RunOnHttps(false);
            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 1 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).ExecuteAsync());
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 2 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).ExecuteAsync());
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync());
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif

            receivedMessage = false;
            #region "SetUuidMetadata 1"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 1 STARTED");
#if NET40
            PNResult<PNSetUuidMetadataResult> createUser1Result = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetUuidMetadataResult> createUser1Result = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1").ExecuteAsync();
#endif
            if (createUser1Result.Result != null && createUser1Result.Status.StatusCode == 200 && !createUser1Result.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser1Result.Result);
                if (uuidMetadataId1 == createUser1Result.Result.Uuid)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetUuidMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 2 STARTED");
#if NET40
                PNResult<PNSetUuidMetadataResult> createUser2Result = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetUuidMetadataResult> createUser2Result = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2").ExecuteAsync();
#endif
                if (createUser2Result.Result != null && createUser2Result.Status.StatusCode == 200 && !createUser2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser2Result.Result);
                    if (uuidMetadataId2 == createUser2Result.Result.Uuid)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
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
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetChannelMembers Add"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMembers() SET STARTED");
#if NET40
                PNResult<PNChannelMembersResult> manageMemberResult = Task.Factory.StartNew(async () => await pubnub.SetChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> manageMemberResult = await pubnub.SetChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync();
#endif
                if (manageMemberResult.Result != null && manageMemberResult.Status.StatusCode == 200 && !manageMemberResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMemberResult.Result);
                    if (manageMemberResult.Result.ChannelMembers != null
                    && manageMemberResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                    && manageMemberResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage && !string.IsNullOrEmpty(config.SecretKey))
            {
                receivedMessage = false;
                #region "SetChannelMembers Update"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMembers() SET for UPDATE STARTED");
#if NET40
                PNResult<PNChannelMembersResult> manageMmbrUpdResult = Task.Factory.StartNew(async () => await pubnub.SetChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNChannelMember() { Uuid = uuidMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> manageMmbrUpdResult = await pubnub.SetChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNChannelMember() { Uuid = uuidMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync();
#endif
                if (manageMmbrUpdResult.Result != null && manageMmbrUpdResult.Status.StatusCode == 200 && !manageMmbrUpdResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrUpdResult.Result);
                    if (manageMmbrUpdResult.Result.ChannelMembers != null
                    && manageMmbrUpdResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                    && manageMmbrUpdResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "RemoveChannelMembers Remove"
                System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMembers() REMOVE STARTED");
#if NET40
                PNResult<PNChannelMembersResult> manageMmbrDelResult = Task.Factory.StartNew(async () => await pubnub.RemoveChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<string>() { uuidMetadataId2 })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> manageMmbrDelResult = await pubnub.RemoveChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<string>() { uuidMetadataId2 })
                    .Include(new PNChannelMemberField[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID, PNChannelMemberField.UUID_CUSTOM })
                    .IncludeCount(true)
                    .Page(new PNPageObject() { Next = "", Prev = "" })
                    .ExecuteAsync();
#endif
                if (manageMmbrDelResult.Result != null && manageMmbrDelResult.Status.StatusCode == 200 && !manageMmbrDelResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrDelResult.Result);
                    if (manageMmbrDelResult.Result.ChannelMembers != null)
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
#if NET40
                PNResult<PNChannelMembersResult> getMbrsResult = Task.Factory.StartNew(async () => await pubnub.GetChannelMembers().Channel(channelMetadataId)
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> getMbrsResult = await pubnub.GetChannelMembers().Channel(channelMetadataId)
                    .ExecuteAsync();
#endif
                if (getMbrsResult.Result != null && getMbrsResult.Status.StatusCode == 200 && !getMbrsResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getMbrsResult.Result);
                    if (getMbrsResult.Result.ChannelMembers != null
                    && getMbrsResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "SetUuidMetadata/SetChannelMetadata/ Set/Remove ChannelMember Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }


        [Test]
        public async Task ThenManageChannelMembersShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenManageChannelMembersShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedSetEvent = false;
            bool receivedDeleteEvent = false;

            string channelMetadataId = "pandu-ut-sid";
            string uuidMetadataId1 = "pandu-ut-uid1";
            string uuidMetadataId2 = "pandu-ut-uid2";

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "membership")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "set")
                        {
                            receivedSetEvent = true;
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

            
            pubnub.Subscribe<string>().Channels(new string[] { uuidMetadataId1, uuidMetadataId2, channelMetadataId }).Execute();
            await Task.Delay(3000);

            
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 1 STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            await Task.Delay(3000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 2 STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            await Task.Delay(3000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            await Task.Delay(3000);
            
            manualResetEventWaitTimeout = 310 * 1000;
            var manualEvent = new ManualResetEvent(false);
            #region "SetUuidMetadata 1"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 1 STARTED");
            pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1")
                    .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (uuidMetadataId1 == r.Uuid)
                            {
                                manualEvent.Set();
                            }
                        }
                    }));
            #endregion
            receivedMessage = manualEvent.WaitOne(manualResetEventWaitTimeout);

            if (receivedMessage)
            {
                manualEvent = new ManualResetEvent(false);
                #region "SetUuidMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 2 STARTED");
                pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2")
                        .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (uuidMetadataId2 == r.Uuid)
                                {
                                    manualEvent.Set();
                                }
                            }
                        }));
                #endregion
                receivedMessage = manualEvent.WaitOne(manualResetEventWaitTimeout);
            }
            if (receivedMessage)
            {
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
                                    manualEvent.Set();
                                }
                            }
                        }));
                #endregion
                receivedMessage = manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedMessage)
            {
                manualEvent = new ManualResetEvent(false);
                #region "ManageChannelMembers Add"
                System.Diagnostics.Debug.WriteLine("pubnub.ManageChannelMembers() ADD STARTED");
                pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Set(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.ChannelMembers != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
                            {
                                manualEvent.Set();
                            }
                        }
                    }));
                #endregion
                receivedMessage = manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (receivedMessage)
            {
                manualEvent = new ManualResetEvent(false);
                #region "ManageChannelMembers Update"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.ManageChannelMembers() UPDATE STARTED");
                    pubnub.ManageChannelMembers().Channel(channelMetadataId)
                        .Set(new List<PNChannelMember>()
                                {
                            new PNChannelMember() { Uuid = uuidMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { uuidMetadataId2 })
                        .Execute(new PNChannelMembersResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (r.ChannelMembers != null
                                && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null)
                                {
                                    manualEvent.Set();
                                }
                            }
                        }));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.ManageChannelMembers() REMOVE STARTED");
                    pubnub.ManageChannelMembers().Channel(channelMetadataId)
                        .Remove(new List<string>() { uuidMetadataId2 })
                        .Execute(new PNChannelMembersResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                manualEvent.Set();
                            }
                        }));
                }
                #endregion
                receivedMessage = manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            await Task.Delay(3000);

            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId1, uuidMetadataId2, channelMetadataId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedSetEvent, "Manage Channel Member events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncManageChannelMembersShouldReturnEventInfo()
#else
        public static async Task ThenWithAsyncManageChannelMembersShouldReturnEventInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncMemberSetRemoveShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedSetEvent = false;
            bool receivedDeleteEvent = false;

            string channelMetadataId = "pandu-ut-sid";
            string uuidMetadataId1 = "pandu-ut-uid1";
            string uuidMetadataId2 = "pandu-ut-uid2";

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "membership")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "set")
                        {
                            receivedSetEvent = true;
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
            pubnub.Subscribe<string>().Channels(new string[] { uuidMetadataId1, uuidMetadataId2, channelMetadataId }).Execute();
            manualEvent.WaitOne(4000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 1 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 2 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif

            receivedMessage = false;
            #region "SetUuidMetadata 1"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 1 STARTED");
#if NET40
            PNResult<PNSetUuidMetadataResult> createUser1Result = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetUuidMetadataResult> createUser1Result = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1").ExecuteAsync();
#endif
            if (createUser1Result.Result != null && createUser1Result.Status.StatusCode == 200 && !createUser1Result.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser1Result.Result);
                if (uuidMetadataId1 == createUser1Result.Result.Uuid)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            /* */
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetUuidMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 2 STARTED");
#if NET40
                PNResult<PNSetUuidMetadataResult> createUser2Result = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetUuidMetadataResult> createUser2Result = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2").ExecuteAsync();
#endif
                if (createUser2Result.Result != null && createUser2Result.Status.StatusCode == 200 && !createUser2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser2Result.Result);
                    if (uuidMetadataId2 == createUser2Result.Result.Uuid)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
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
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "ManageChannelMembers Add"
                System.Diagnostics.Debug.WriteLine("pubnub.ManageChannelMembers() ADD STARTED");
#if NET40
                PNResult<PNChannelMembersResult> manageMemberResult = Task.Factory.StartNew(async () => await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Set(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> manageMemberResult = await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                    .Set(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .ExecuteAsync();
#endif
                if (manageMemberResult.Result != null && manageMemberResult.Status.StatusCode == 200 && !manageMemberResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMemberResult.Result);
                    if (manageMemberResult.Result.ChannelMembers != null
                    && manageMemberResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                    && manageMemberResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            await Task.Delay(4000);

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "ManageChannelMembers Update/Remove"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Members() UPDATE/REMOVE STARTED");
#if NET40
                    PNResult<PNChannelMembersResult> manageMmbrUpdResult = Task.Factory.StartNew(async () => await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                        .Set(new List<PNChannelMember>()
                                {
                            new PNChannelMember() { Uuid = uuidMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { uuidMetadataId2 })
                        .ExecuteAsync()).Result.Result;
#else
                    PNResult<PNChannelMembersResult> manageMmbrUpdResult = await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                        .Set(new List<PNChannelMember>()
                                {
                            new PNChannelMember() { Uuid = uuidMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { uuidMetadataId2 })
                        .ExecuteAsync();
#endif
                    if (manageMmbrUpdResult.Result != null && manageMmbrUpdResult.Status.StatusCode == 200 && !manageMmbrUpdResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrUpdResult.Result);
                        if (manageMmbrUpdResult.Result.ChannelMembers != null
                        && manageMmbrUpdResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null)
                        {
                            receivedMessage = true;
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.ManageChannelMembers() REMOVE STARTED");
#if NET40
                    PNResult<PNChannelMembersResult> manageMmbrDelResult = Task.Factory.StartNew(async () => await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                        .Remove(new List<string>() { uuidMetadataId2 })
                        .ExecuteAsync()).Result.Result;
#else
                    PNResult<PNChannelMembersResult> manageMmbrDelResult = await pubnub.ManageChannelMembers().Channel(channelMetadataId)
                        .Remove(new List<string>() { uuidMetadataId2 })
                        .ExecuteAsync();
#endif
                    if (manageMmbrDelResult.Result != null && manageMmbrDelResult.Status.StatusCode == 200 && !manageMmbrDelResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrDelResult.Result);
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            await Task.Delay(4000);
            /* */
            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId1, uuidMetadataId2, channelMetadataId }).Execute();
            await Task.Delay(4000);
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedSetEvent, "Async ManageMembers events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSetRemoveChannelMembersShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSetRemoveChannelMembersShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedSetEvent = false;
            bool receivedDeleteEvent = false;

            string channelMetadataId = "pandu-ut-sid";
            string uuidMetadataId1 = "pandu-ut-uid1";
            string uuidMetadataId2 = "pandu-ut-uid2";

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "channel" || eventResult.Type.ToLowerInvariant() == "uuid")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "set")
                        {
                            receivedSetEvent = true;
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
            pubnub.Subscribe<string>().Channels(new string[] { uuidMetadataId1, uuidMetadataId2, channelMetadataId }).Execute();
            manualEvent.WaitOne(2000);


            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 1 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 2 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
            #region "SetUuidMetadata 1"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 1 STARTED");
            pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1")
                    .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (uuidMetadataId1 == r.Uuid)
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
                #region "SetUuidMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 2 STARTED");
                pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2")
                        .Execute(new PNSetUuidMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (uuidMetadataId2 == r.Uuid)
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
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);
                #region "SetChannelMembers Add"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMembers() ADD STARTED");
                pubnub.SetChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .Execute(new PNChannelMembersResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            System.Diagnostics.Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                            if (r.ChannelMembers != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                            && r.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
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
                #region "RemoveChannelMembers"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    manualEvent = new ManualResetEvent(false);
                    System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMembers() REMOVE STARTED");
                    pubnub.RemoveChannelMembers().Channel(channelMetadataId)
                        .Uuids(new List<string>() { uuidMetadataId2 })
                        .Execute(new PNChannelMembersResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                receivedMessage = true;
                            }
                            manualEvent.Set();
                        }));
                    manualEvent.WaitOne(manualResetEventWaitTimeout);
                }
                #endregion
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId1, uuidMetadataId2, channelMetadataId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedSetEvent, "Manage Channel Member events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }


        [Test]
#if NET40
        public static void ThenWithAsyncSetRemoveChannelMemberShouldReturnEventInfo()
#else
        public static async Task ThenWithAsyncSetRemoveChannelMemberShouldReturnEventInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncSetRemoveChannelMemberShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedSetEvent = false;
            bool receivedDeleteEvent = false;

            string channelMetadataId = "pandu-ut-sid";
            string uuidMetadataId1 = "pandu-ut-uid1";
            string uuidMetadataId2 = "pandu-ut-uid2";

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "channel" || eventResult.Type.ToLowerInvariant() == "uuid")
                    {
                        if (eventResult.Event.ToLowerInvariant() == "set")
                        {
                            receivedSetEvent = true;
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
            pubnub.Subscribe<string>().Channels(new string[] { uuidMetadataId1, uuidMetadataId2, channelMetadataId }).Execute();
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 1 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId1).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() 2 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId2).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif

            receivedMessage = false;
            #region "SetUuidMetadata 1"
            System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 1 STARTED");
#if NET40
            PNResult<PNSetUuidMetadataResult> createUser1Result = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetUuidMetadataResult> createUser1Result = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId1).Name("pandu-ut-un1").ExecuteAsync();
#endif
            if (createUser1Result.Result != null && createUser1Result.Status.StatusCode == 200 && !createUser1Result.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser1Result.Result);
                if (uuidMetadataId1 == createUser1Result.Result.Uuid)
                {
                    receivedMessage = true;
                }
            }
            #endregion

            /* */
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetUuidMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetUuidMetadata() 2 STARTED");
#if NET40
                PNResult<PNSetUuidMetadataResult> createUser2Result = Task.Factory.StartNew(async () => await pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetUuidMetadataResult> createUser2Result = await pubnub.SetUuidMetadata().Uuid(uuidMetadataId2).Name("pandu-ut-un2").ExecuteAsync();
#endif
                if (createUser2Result.Result != null && createUser2Result.Status.StatusCode == 200 && !createUser2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createUser2Result.Result);
                    if (uuidMetadataId2 == createUser2Result.Result.Uuid)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
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
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetChannelMembers Add"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMembers() ADD STARTED");
#if NET40
                PNResult<PNChannelMembersResult> manageMemberResult = Task.Factory.StartNew(async () => await pubnub.SetChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNChannelMembersResult> manageMemberResult = await pubnub.SetChannelMembers().Channel(channelMetadataId)
                    .Uuids(new List<PNChannelMember>()
                            {
                            new PNChannelMember() { Uuid = uuidMetadataId1 },
                            new PNChannelMember() { Uuid = uuidMetadataId2 }
                    })
                    .ExecuteAsync();
#endif
                if (manageMemberResult.Result != null && manageMemberResult.Status.StatusCode == 200 && !manageMemberResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMemberResult.Result);
                    if (manageMemberResult.Result.ChannelMembers != null
                    && manageMemberResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId1) != null
                    && manageMemberResult.Result.ChannelMembers.Find(x => x.UuidMetadata.Uuid == uuidMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            Thread.Sleep(2000);

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "RemoveChannelMembers"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMembers() STARTED");
#if NET40
                    PNResult<PNChannelMembersResult> manageMmbrDelResult = Task.Factory.StartNew(async () => await pubnub.RemoveChannelMembers().Channel(channelMetadataId)
                        .Uuids(new List<string>() { uuidMetadataId2 })
                        .ExecuteAsync()).Result.Result;
#else
                    PNResult<PNChannelMembersResult> manageMmbrDelResult = await pubnub.RemoveChannelMembers().Channel(channelMetadataId)
                        .Uuids(new List<string>() { uuidMetadataId2 })
                        .ExecuteAsync();
#endif
                    if (manageMmbrDelResult.Result != null && manageMmbrDelResult.Status.StatusCode == 200 && !manageMmbrDelResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMmbrDelResult.Result);
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            Thread.Sleep(4000);
            /* */
            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId1, uuidMetadataId2, channelMetadataId }).Execute();
            Thread.Sleep(1000);
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedSetEvent, "Async Set/Remove ChannelMembers events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenGetChannelMembersShouldReturnMembershipWithStatusTypeAndCustomFields()
        {
            var r = new Random();
            string channelMetadataId = $"channel{r.Next(100, 1000)}";
            string uuidMetadataId = $"uuid{r.Next(100, 1000)}";
            string membershipStatus = $"membershipstatus{r.Next(100, 1000)}";
            string membershipType = $"membershiptype{r.Next(100, 1000)}";
            string uuidName = $"uuidname{r.Next(100, 1000)}";
            string channelName = $"channelname{r.Next(100, 1000)}";
            
            PNConfiguration configuration = new PNConfiguration(new UserId($"user{r.Next(100,1000)}"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };
            pubnub = createPubNubInstance(configuration);
            
            bool receivedMembershipSetEvent = false;
            ManualResetEvent membershipSetEventManualEvent = new ManualResetEvent(false);
            
            var channelSubscription = pubnub.Channel(channelMetadataId).Subscription();
            channelSubscription.onObject += (_, appContextEvent) =>
            {
                var eventType = appContextEvent.Type;
                
                // Only validate membership events, ignore channel and uuid events
                if (eventType.ToLowerInvariant() == "membership" && appContextEvent.Event.ToLowerInvariant() == "set")
                {
                    receivedMembershipSetEvent = true;
                    membershipSetEventManualEvent.Set();
                }
            };
            channelSubscription.Subscribe<object>();
            
            await Task.Delay(2000);
            
            // 1. Set UUIDMetadata for random user
            var setUuidMetadata = await pubnub.SetUuidMetadata()
                .Uuid(uuidMetadataId)
                .Name(uuidName)
                .ExecuteAsync();
            
            // 2. Set ChannelMetadata for random channel
            var setChannelMetadata = await pubnub.SetChannelMetadata()
                .Channel(channelMetadataId)
                .Name(channelName)
                .ExecuteAsync();
            
            await Task.Delay(2000);
            
            // 3. Set Membership for those given UUID and channel metadata with status, custom and type
            var setMembership = await pubnub.SetChannelMembers()
                .Channel(channelMetadataId)
                .Uuids(new List<PNChannelMember>()
                {
                    new PNChannelMember() 
                    { 
                        Uuid = uuidMetadataId,
                        Status = membershipStatus,
                        Type = membershipType,
                        Custom = new Dictionary<string, object> { { "membership_key", "membership_value" } }
                    }
                })
                .Include(new PNChannelMemberField[] { 
                    PNChannelMemberField.CUSTOM, 
                    PNChannelMemberField.UUID, 
                    PNChannelMemberField.UUID_CUSTOM,
                    PNChannelMemberField.STATUS,
                    PNChannelMemberField.TYPE
                })
                .IncludeCount(true)
                .ExecuteAsync();
            
            Assert.That(setMembership.Result, Is.Not.Null, "SetMembership result should not be null");
            Assert.That(setMembership.Status.StatusCode, Is.EqualTo(200), "SetMembership status code should be 200");
            Assert.That(setMembership.Status.Error, Is.False, "SetMembership should not indicate error");
            
            await Task.Delay(2000);
            
            // 4. Use API GetChannelMembers() and check the result
            var getChannelMembers = await pubnub.GetChannelMembers()
                .Channel(channelMetadataId)
                .Include(new PNChannelMemberField[] { 
                    PNChannelMemberField.CUSTOM, 
                    PNChannelMemberField.UUID, 
                    PNChannelMemberField.UUID_CUSTOM,
                    PNChannelMemberField.STATUS,
                    PNChannelMemberField.TYPE
                })
                .IncludeCount(true)
                .ExecuteAsync();
            
            // Assertions relevant to GetMembers
            Assert.That(getChannelMembers.Result, Is.Not.Null, "GetChannelMembers result should not be null");
            Assert.That(getChannelMembers.Status.StatusCode, Is.EqualTo(200), "GetChannelMembers status code should be 200");
            Assert.That(getChannelMembers.Status.Error, Is.False, "GetChannelMembers should not indicate error");
            Assert.That(getChannelMembers.Result.ChannelMembers, Is.Not.Null, "ChannelMembers list should not be null");
            Assert.That(getChannelMembers.Result.ChannelMembers.Count, Is.EqualTo(1), "Should find exactly one member");
            Assert.That(getChannelMembers.Result.TotalCount, Is.EqualTo(1), "Total count should be 1");
            
            var member = getChannelMembers.Result.ChannelMembers[0];
            Assert.That(member.UuidMetadata, Is.Not.Null, "UuidMetadata should not be null");
            Assert.That(member.UuidMetadata.Uuid, Is.EqualTo(uuidMetadataId), "Member UUID should match");
            Assert.That(member.UuidMetadata.Name, Is.EqualTo(uuidName), "Member name should match");
            Assert.That(member.Status, Is.EqualTo(membershipStatus), "Membership status should match");
            Assert.That(member.Type, Is.EqualTo(membershipType), "Membership type should match");
            Assert.That(member.Custom, Is.Not.Null, "Membership custom should not be null");
            Assert.That(member.Custom.ContainsKey("membership_key"), Is.True, "Membership custom should contain 'membership_key'");
            Assert.That(member.Custom["membership_key"].ToString(), Is.EqualTo("membership_value"), "Membership custom value should match");
            
            // Wait for subscription events
            membershipSetEventManualEvent.WaitOne(3000);
            Assert.That(receivedMembershipSetEvent, Is.True, "Should have received membership set event via subscription");
            
            // Cleanup: Remove membership, then UUID and channel metadata
            await pubnub.RemoveChannelMembers()
                .Channel(channelMetadataId)
                .Uuids(new List<string>() { uuidMetadataId })
                .ExecuteAsync();
            
            await Task.Delay(2000);
            
            await pubnub.RemoveUuidMetadata()
                .Uuid(uuidMetadataId)
                .ExecuteAsync();
            
            await pubnub.RemoveChannelMetadata()
                .Channel(channelMetadataId)
                .ExecuteAsync();
            
            // Cleanup
            // channelSubscription.Unsubscribe<object>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

    }
}
