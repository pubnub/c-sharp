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
        private static string authToken = "myauth";

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
            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";

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
                                            { channelMetadataId1, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } },
                                            { channelMetadataId2, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } },
                                            { uuidMetadataId, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } }},
                        Uuids = new Dictionary<string, PNTokenAuthValues>() {
                                            { uuidMetadataId, new PNTokenAuthValues() { Read = true, Write = true, Manage= true, Create = true, Delete=true, Get = true, Update = true, Join = true } } },
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
                                            Debug.WriteLine("PNAccessManagerTokenResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
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
            Assert.IsTrue(receivedGrantMessage, "WhenObjectMembership Grant access failed.");
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        //TODO: CLEN-2039
        //[Test]
        public static void ThenSetRemoveChannelMetadataWithManageMembershipShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSetRemoveChannelMetadataWithManageMembershipShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";

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
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 1 STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
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
                #region "SetChannelMetadata 1"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 1 STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname")
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId1 == r.Channel)
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
                #region "SetChannelMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 2 STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname")
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId2 == r.Channel)
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
                pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Set(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1   },
                            new PNMembership() { Channel = channelMetadataId2   }
                    })
                    .Execute(new PNMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
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
                pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Set(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1 , Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMembership() { Channel = channelMetadataId2 , Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Execute(new PNMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
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
                pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Remove(new List<string>() { channelMetadataId2 })
                    .Execute(new PNMembershipsResultExt((r, s) =>
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
                pubnub.GetMemberships().Uuid(uuidMetadataId)
                    .Execute(new PNMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
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
                Assert.IsTrue(receivedMessage, "SetUuidMetadata/CreateSpace/Membership Manage Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncSetRemoveChannelMetadataWithManageMembershipShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncSetRemoveChannelMetadataWithManageMembershipShouldReturnSuccessCodeAndInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncSetRemoveChannelMetadataWithManageMembershipShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";

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


            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 1 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync();
#endif

            receivedMessage = false;
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
                #region "SetChannelMetadata 1"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 1 STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> createSpace1Result = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> createSpace1Result = await pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname").ExecuteAsync();
#endif
                if (createSpace1Result.Result != null && createSpace1Result.Status.StatusCode == 200 && !createSpace1Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace1Result.Result);
                    if (channelMetadataId1 == createSpace1Result.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetChannelMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 2 STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> createSpace2Result = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> createSpace2Result = await pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname").ExecuteAsync();
#endif
                if (createSpace2Result.Result != null && createSpace2Result.Status.StatusCode == 200 && !createSpace2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace2Result.Result);
                    if (channelMetadataId2 == createSpace2Result.Result.Channel)
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
#if NET40
                PNResult<PNMembershipsResult> manageMbrshipAddResult = Task.Factory.StartNew(async () => await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Set(new List<PNMembership>()
                            {
                                new PNMembership() { Channel = channelMetadataId1  },
                                new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> manageMbrshipAddResult = await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Set(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1  },
                            new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .ExecuteAsync();
#endif
                if (manageMbrshipAddResult.Result != null && manageMbrshipAddResult.Status.StatusCode == 200 && !manageMbrshipAddResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipAddResult.Result);
                    if (manageMbrshipAddResult.Result.Memberships != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
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
#if NET40
                PNResult<PNMembershipsResult> manageMbrshipUpdResult = Task.Factory.StartNew(async () => await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Set(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMembership() { Channel = channelMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> manageMbrshipUpdResult = await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Set(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMembership() { Channel = channelMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .ExecuteAsync();
#endif
                if (manageMbrshipUpdResult.Result != null && manageMbrshipUpdResult.Status.StatusCode == 200 && !manageMbrshipUpdResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipUpdResult.Result);
                    if (manageMbrshipUpdResult.Result.Memberships != null
                    && manageMbrshipUpdResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                    && manageMbrshipUpdResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
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
#if NET40
                PNResult<PNMembershipsResult> manageMbrshipDelResult = Task.Factory.StartNew(async () => await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Remove(new List<string>() { channelMetadataId2 })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> manageMbrshipDelResult = await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Remove(new List<string>() { channelMetadataId2 })
                    .ExecuteAsync();
#endif
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
#if NET40
                PNResult<PNMembershipsResult> getMbrshipResult = Task.Factory.StartNew(async () => await pubnub.GetMemberships().Uuid(uuidMetadataId).ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> getMbrshipResult = await pubnub.GetMemberships().Uuid(uuidMetadataId).ExecuteAsync();
#endif
                if (getMbrshipResult.Result != null && getMbrshipResult.Status.StatusCode == 200 && !getMbrshipResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getMbrshipResult.Result);
                    if (getMbrshipResult.Result.Memberships != null
                    && getMbrshipResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "With Async SetUuidMetadata/CreateSpace/Membership Manage Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSetRemoveChannelMetadataWithSetRemoveMembershipShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSetRemoveChannelMetadataWithSetRemoveMembershipShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";

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
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() STARTED");
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 1 STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
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
                #region "SetChannelMetadata 1"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 1 STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname")
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId1 == r.Channel)
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
                #region "SetChannelMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 2 STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname")
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId2 == r.Channel)
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
                #region "SetMemberships ADD"
                System.Diagnostics.Debug.WriteLine("pubnub.SetMemberships() ADD STARTED");
                pubnub.SetMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1   },
                            new PNMembership() { Channel = channelMetadataId2   }
                    })
                    .Execute(new PNMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
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
                #region "SetMemberships Update"
                System.Diagnostics.Debug.WriteLine("pubnub.SetMemberships() UPDATE STARTED");
                pubnub.SetMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1 , Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMembership() { Channel = channelMetadataId2 , Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .Execute(new PNMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
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
                #region "RemoveMemberships Remove"
                System.Diagnostics.Debug.WriteLine("pubnub.RemoveMemberships() REMOVE STARTED");
                pubnub.RemoveMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<string>() { channelMetadataId2 })
                    .Execute(new PNMembershipsResultExt((r, s) =>
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
                pubnub.GetMemberships().Uuid(uuidMetadataId)
                    .Execute(new PNMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
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
                Assert.IsTrue(receivedMessage, "SetUuidMetadata/CreateSpace/SetMemberships/RemoveMemberships Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncSetRemoveChannelMetadataWithSetRemoveMembershipShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncSetRemoveChannelMetadataWithSetRemoveMembershipShouldReturnSuccessCodeAndInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncSetRemoveChannelMetadataWithSetRemoveMembershipShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";

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


            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 1 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync();
#endif

            receivedMessage = false;
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
                #region "SetChannelMetadata 1"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 1 STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> createSpace1Result = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> createSpace1Result = await pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname").ExecuteAsync();
#endif
                if (createSpace1Result.Result != null && createSpace1Result.Status.StatusCode == 200 && !createSpace1Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace1Result.Result);
                    if (channelMetadataId1 == createSpace1Result.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetChannelMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 2 STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> createSpace2Result = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> createSpace2Result = await pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname").ExecuteAsync();
#endif
                if (createSpace2Result.Result != null && createSpace2Result.Status.StatusCode == 200 && !createSpace2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace2Result.Result);
                    if (channelMetadataId2 == createSpace2Result.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetMemberships Add"
                System.Diagnostics.Debug.WriteLine("pubnub.SetMemberships() ADD STARTED");
#if NET40
                PNResult<PNMembershipsResult> manageMbrshipAddResult = Task.Factory.StartNew(async () => await pubnub.SetMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<PNMembership>()
                            {
                                new PNMembership() { Channel = channelMetadataId1  },
                                new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> manageMbrshipAddResult = await pubnub.SetMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1  },
                            new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .ExecuteAsync();
#endif
                if (manageMbrshipAddResult.Result != null && manageMbrshipAddResult.Status.StatusCode == 200 && !manageMbrshipAddResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipAddResult.Result);
                    if (manageMbrshipAddResult.Result.Memberships != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage && !string.IsNullOrEmpty(config.SecretKey))
            {
                receivedMessage = false;
                #region "SetMemberships Update"
                System.Diagnostics.Debug.WriteLine("pubnub.SetMemberships() UPDATE STARTED");
#if NET40
                PNResult<PNMembershipsResult> manageMbrshipUpdResult = Task.Factory.StartNew(async () => await pubnub.SetMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMembership() { Channel = channelMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> manageMbrshipUpdResult = await pubnub.SetMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } },
                            new PNMembership() { Channel = channelMetadataId2, Custom = new Dictionary<string, object>(){ { "color", "green2" } } }
                    })
                    .ExecuteAsync();
#endif
                if (manageMbrshipUpdResult.Result != null && manageMbrshipUpdResult.Status.StatusCode == 200 && !manageMbrshipUpdResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipUpdResult.Result);
                    if (manageMbrshipUpdResult.Result.Memberships != null
                    && manageMbrshipUpdResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                    && manageMbrshipUpdResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "RemoveMemberships Remove"
                System.Diagnostics.Debug.WriteLine("pubnub.RemoveMemberships() REMOVE STARTED");
#if NET40
                PNResult<PNMembershipsResult> manageMbrshipDelResult = Task.Factory.StartNew(async () => await pubnub.RemoveMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<string>() { channelMetadataId2 })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> manageMbrshipDelResult = await pubnub.RemoveMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<string>() { channelMetadataId2 })
                    .ExecuteAsync();
#endif
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
#if NET40
                PNResult<PNMembershipsResult> getMbrshipResult = Task.Factory.StartNew(async () => await pubnub.GetMemberships().Uuid(uuidMetadataId).ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> getMbrshipResult = await pubnub.GetMemberships().Uuid(uuidMetadataId).ExecuteAsync();
#endif
                if (getMbrshipResult.Result != null && getMbrshipResult.Status.StatusCode == 200 && !getMbrshipResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getMbrshipResult.Result);
                    if (getMbrshipResult.Result.Memberships != null
                    && getMbrshipResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "With Async SetUuidMetadata/CreateSpace/Membership Manage Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }


        //TODO: CLEN-2037
        //[Test]
        public static void ThenManageMembershipShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenManageMembershipShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedSetEvent = false;
            bool receivedDeleteEvent = false;

            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";

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
            pubnub.Subscribe<string>().Channels(new string[] { uuidMetadataId, channelMetadataId1, channelMetadataId2 }).Execute();
            manualEvent.WaitOne(2000);


            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUuidMetadata() STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 1 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
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
                #region "SetChannelMetadata 1"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 1 STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname")
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId1 == r.Channel)
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
                #region "SetChannelMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 2 STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname")
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId2 == r.Channel)
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
                pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Set(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1  },
                            new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .Execute(new PNMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
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
                    pubnub.ManageMemberships().Uuid(uuidMetadataId)
                        .Set(new List<PNMembership>()
                                {
                            new PNMembership() { Channel = channelMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { channelMetadataId2 })
                        .Execute(new PNMembershipsResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (r.Memberships != null
                                && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
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
                    pubnub.ManageMemberships().Uuid(uuidMetadataId)
                        .Remove(new List<string>() { channelMetadataId2 })
                        .Execute(new PNMembershipsResultExt((r, s) =>
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

            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId, channelMetadataId1, channelMetadataId2 }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedSetEvent, "Manage Membership events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncManageMembershipShouldReturnEventInfo()
#else
        public static async Task ThenWithAsyncManageMembershipShouldReturnEventInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncManageMembershipShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedSetEvent = false;
            bool receivedDeleteEvent = false;

            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "channel")
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
            pubnub.Subscribe<string>().Channels(new string[] { uuidMetadataId, channelMetadataId1, channelMetadataId2 }).Execute();
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 1 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync();
#endif

            receivedMessage = false;
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
                #region "CreateSpace 1"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 1 STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> createSpace1Result = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> createSpace1Result = await pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname").ExecuteAsync();
#endif
                if (createSpace1Result.Result != null && createSpace1Result.Status.StatusCode == 200 && !createSpace1Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace1Result.Result);
                    if (channelMetadataId1 == createSpace1Result.Result.Channel)
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
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 2 STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> createSpace2Result = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> createSpace2Result = await pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname").ExecuteAsync();
#endif
                if (createSpace2Result.Result != null && createSpace2Result.Status.StatusCode == 200 && !createSpace2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace2Result.Result);
                    if (channelMetadataId2 == createSpace2Result.Result.Channel)
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
#if NET40
                PNResult<PNMembershipsResult> manageMbrshipAddResult = Task.Factory.StartNew(async () => await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Set(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1  },
                            new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> manageMbrshipAddResult = await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                    .Set(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1  },
                            new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .ExecuteAsync();
#endif
                if (manageMbrshipAddResult.Result != null && manageMbrshipAddResult.Status.StatusCode == 200 && !manageMbrshipAddResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipAddResult.Result);
                    if (manageMbrshipAddResult.Result.Memberships != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "Memberships Update/Remove"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Memberships() UPDATE/REMOVE STARTED");
#if NET40
                    PNResult<PNMembershipsResult> manageMbrshipUpdResult = Task.Factory.StartNew(async () => await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                        .Set(new List<PNMembership>()
                                {
                            new PNMembership() { Channel = channelMetadataId1 , Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { channelMetadataId2 })
                        .ExecuteAsync()).Result.Result;
#else
                    PNResult<PNMembershipsResult> manageMbrshipUpdResult = await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                        .Set(new List<PNMembership>()
                                {
                            new PNMembership() { Channel = channelMetadataId1 , Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Remove(new List<string>() { channelMetadataId2 })
                        .ExecuteAsync();
#endif
                    if (manageMbrshipUpdResult.Result != null && manageMbrshipUpdResult.Status.StatusCode == 200 && !manageMbrshipUpdResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipUpdResult.Result);
                        if (manageMbrshipUpdResult.Result.Memberships != null
                        && manageMbrshipUpdResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
                        {
                            receivedMessage = true;
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.Memberships() REMOVE STARTED");
#if NET40
                    PNResult<PNMembershipsResult> manageMbrshipDelResult = Task.Factory.StartNew(async () => await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                        .Remove(new List<string>() { channelMetadataId2 })
                        .ExecuteAsync()).Result.Result;
#else
                    PNResult<PNMembershipsResult> manageMbrshipDelResult = await pubnub.ManageMemberships().Uuid(uuidMetadataId)
                        .Remove(new List<string>() { channelMetadataId2 })
                        .ExecuteAsync();
#endif
                    if (manageMbrshipDelResult.Result != null && manageMbrshipDelResult.Status.StatusCode == 200 && !manageMbrshipDelResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipDelResult.Result);
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId, channelMetadataId1, channelMetadataId2 }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedSetEvent, "With Async Manage Membership events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSetRemoveMembershipsShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSetRemoveMembershipsShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedSetEvent = false;
            bool receivedDeleteEvent = false;

            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "channel")
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
            pubnub.Subscribe<string>().Channels(new string[] { channelMetadataId1, channelMetadataId2 }).Execute();
            manualEvent.WaitOne(2000);


            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteUuidMetadata() STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).Execute(new PNRemoveUuidMetadataResultExt(
                delegate (PNRemoveUuidMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 1 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
            manualEvent = new ManualResetEvent(false);
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).Execute(new PNRemoveChannelMetadataResultExt(
                delegate (PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            receivedMessage = false;
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
                #region "SetChannelMetadata 1"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 1 STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname")
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId1 == r.Channel)
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
                #region "SetChannelMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 2 STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname")
                        .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (channelMetadataId2 == r.Channel)
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
                #region "SetMemberships Add"
                System.Diagnostics.Debug.WriteLine("pubnub.SetMemberships() SET STARTED");
                pubnub.SetMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1  },
                            new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .Execute(new PNMembershipsResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
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
                #region "SetMemberships Update"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.SetMemberships() UPDATE STARTED");
                    pubnub.SetMemberships().Uuid(uuidMetadataId)
                        .Channels(new List<PNMembership>()
                                {
                            new PNMembership() { Channel = channelMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                        })
                        .Execute(new PNMembershipsResultExt((r, s) =>
                        {
                            if (r != null && s.StatusCode == 200 && !s.Error)
                            {
                                pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                                if (r.Memberships != null
                                && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
                                {
                                    receivedMessage = true;
                                }
                            }
                            manualEvent.Set();
                        }));
                    manualEvent.WaitOne(manualResetEventWaitTimeout);
                }
                else
                {
                }
                #endregion
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId, channelMetadataId1, channelMetadataId2 }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedSetEvent, "Manage Membership events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncSetRemoveMembershipsShouldReturnEventInfo()
#else
        public static async Task ThenWithAsyncSetRemoveMembershipsShouldReturnEventInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncSetRemoveMembershipsShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedSetEvent = false;
            bool receivedDeleteEvent = false;

            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "channel")
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
            pubnub.Subscribe<string>().Channels(new string[] { uuidMetadataId, channelMetadataId1, channelMetadataId2 }).Execute();
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveUuidMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 1 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync();
#endif

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync()).Wait();
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync();
#endif

            receivedMessage = false;
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
                #region "SetChannelMetadata 1"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 1 STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> createSpace1Result = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> createSpace1Result = await pubnub.SetChannelMetadata().Channel(channelMetadataId1).Name("pandu-ut-spname").ExecuteAsync();
#endif
                if (createSpace1Result.Result != null && createSpace1Result.Status.StatusCode == 200 && !createSpace1Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace1Result.Result);
                    if (channelMetadataId1 == createSpace1Result.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }
            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetChannelMetadata 2"
                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() 2 STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> createSpace2Result = Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> createSpace2Result = await pubnub.SetChannelMetadata().Channel(channelMetadataId2).Name("pandu-ut-spname").ExecuteAsync();
#endif
                if (createSpace2Result.Result != null && createSpace2Result.Status.StatusCode == 200 && !createSpace2Result.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpace2Result.Result);
                    if (channelMetadataId2 == createSpace2Result.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetMemberships Add"
                System.Diagnostics.Debug.WriteLine("pubnub.SetMemberships() ADD STARTED");
#if NET40
                PNResult<PNMembershipsResult> manageMbrshipAddResult = Task.Factory.StartNew(async () => await pubnub.SetMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1  },
                            new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNMembershipsResult> manageMbrshipAddResult = await pubnub.SetMemberships().Uuid(uuidMetadataId)
                    .Channels(new List<PNMembership>()
                            {
                            new PNMembership() { Channel = channelMetadataId1  },
                            new PNMembership() { Channel = channelMetadataId2  }
                    })
                    .ExecuteAsync();
#endif
                if (manageMbrshipAddResult.Result != null && manageMbrshipAddResult.Status.StatusCode == 200 && !manageMbrshipAddResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipAddResult.Result);
                    if (manageMbrshipAddResult.Result.Memberships != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                    && manageMbrshipAddResult.Result.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
                    {
                        receivedMessage = true;
                    }
                }
                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                #region "SetMemberships Update"
                if (!string.IsNullOrEmpty(config.SecretKey))
                {
                    System.Diagnostics.Debug.WriteLine("pubnub.RemoveMemberships() REMOVE STARTED");
#if NET40
                    PNResult<PNMembershipsResult> manageMbrshipDelResult = Task.Factory.StartNew(async () => await pubnub.RemoveMemberships().Uuid(uuidMetadataId)
                        .Channels(new List<string>() { channelMetadataId2 })
                        .ExecuteAsync()).Result.Result;
#else
                    PNResult<PNMembershipsResult> manageMbrshipDelResult = await pubnub.RemoveMemberships().Uuid(uuidMetadataId)
                        .Channels(new List<string>() { channelMetadataId2 })
                        .ExecuteAsync();
#endif
                    if (manageMbrshipDelResult.Result != null && manageMbrshipDelResult.Status.StatusCode == 200 && !manageMbrshipDelResult.Status.Error)
                    {
                        pubnub.JsonPluggableLibrary.SerializeToJsonString(manageMbrshipDelResult.Result);
                        receivedMessage = true;
                    }
                }
                else
                {
                }
                #endregion
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { uuidMetadataId, channelMetadataId1, channelMetadataId2 }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedSetEvent, "With Async Set/Remove Membership events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

    }
}
