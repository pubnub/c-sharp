using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;
using System.Linq;
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
            if (PubnubCommon.EnableStubTest)
            {
                server.Start();   
            }

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
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }
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
                        Assert.IsNotNull(r, $"Set UUID metadata result was null. Error info: {s.ErrorData?.Information}");
                        Assert.AreEqual(200, s.StatusCode, $"Set UUID metadata status was not 200, status: " +
                                                         $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.False(s.Error, "Set UUID metadata status reported an error, status: " +
                                             $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.AreEqual(uuidMetadataId, r.Uuid, "UUID metadata ID did not match");
                        if (uuidMetadataId == r.Uuid)
                        {
                            receivedMessage = true;
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
                            Assert.IsNotNull(r, $"Set channel 1 metadata result was null. Error info: {s.ErrorData?.Information}");
                            Assert.AreEqual(200, s.StatusCode, $"Set channel 1 metadata status was not 200, status: " +
                                                             $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                            Assert.False(s.Error, "Set channel 1 metadata status reported an error, status: " +
                                                 $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                            Assert.AreEqual(channelMetadataId1, r.Channel, "Channel 1 metadata ID did not match");
                            if (channelMetadataId1 == r.Channel)
                            {
                                receivedMessage = true;
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
                            Assert.IsNotNull(r, $"Set channel 2 metadata result was null. Error info: {s.ErrorData?.Information}");
                            Assert.AreEqual(200, s.StatusCode, $"Set channel 2 metadata status was not 200, status: " +
                                                             $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                            Assert.False(s.Error, "Set channel 2 metadata status reported an error, status: " +
                                                 $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                            Assert.AreEqual(channelMetadataId2, r.Channel, "Channel 2 metadata ID did not match");
                            if (channelMetadataId2 == r.Channel)
                            {
                                receivedMessage = true;
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
                        Assert.IsNotNull(r, $"Set memberships result was null. Error info: {s.ErrorData?.Information}");
                        Assert.AreEqual(200, s.StatusCode, $"Set memberships status was not 200, status: " +
                                                         $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.False(s.Error, "Set memberships status reported an error, status: " +
                                             $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.IsNotNull(r.Memberships, "Memberships list was null");
                        Assert.IsTrue(r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null, 
                            "Channel 1 membership not found");
                        Assert.IsTrue(r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null, 
                            "Channel 2 membership not found");
                        if (r.Memberships != null
                        && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                        && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
                        {
                            receivedMessage = true;
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
                        new PNMembership() { Channel = channelMetadataId1, Custom = new Dictionary<string, object>(){ { "color", "green1" } } }
                    })
                    .Execute(new PNMembershipsResultExt((r, s) =>
                    {
                        Assert.IsNotNull(r, $"Update memberships result was null. Error info: {s.ErrorData?.Information}");
                        Assert.AreEqual(200, s.StatusCode, $"Update memberships status was not 200, status: " +
                                                         $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.False(s.Error, "Update memberships status reported an error, status: " +
                                             $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.IsNotNull(r.Memberships, "Memberships list was null");
                        Assert.IsTrue(r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null, 
                            "Channel 1 membership not found after update");
                        if (r.Memberships != null
                        && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
                        {
                            receivedMessage = true;
                        }
                        manualEvent.Set();
                    }));
                manualEvent.WaitOne(manualResetEventWaitTimeout);
                
                #endregion
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
                        Assert.IsNotNull(r, $"Remove memberships result was null. Error info: {s.ErrorData?.Information}");
                        Assert.AreEqual(200, s.StatusCode, $"Remove memberships status was not 200, status: " +
                                                         $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.False(s.Error, "Remove memberships status reported an error, status: " +
                                             $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.IsNotNull(r.Memberships, "Memberships list was null after removal");
                        if (r.Memberships != null)
                        {
                            receivedMessage = true;
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
                        Assert.IsNotNull(r, $"Get memberships result was null. Error info: {s.ErrorData?.Information}");
                        Assert.AreEqual(200, s.StatusCode, $"Get memberships status was not 200, status: " +
                                                         $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.False(s.Error, "Get memberships status reported an error, status: " +
                                             $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.IsNotNull(r.Memberships, "Memberships list was null after get");
                        Assert.IsTrue(r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null, 
                            "Channel 1 membership not found in get result");
                        if (r.Memberships?.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
                        {
                            receivedMessage = true;
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
                        Assert.IsNotNull(r, "Set memberships result was null");
                        Assert.AreEqual(200, s.StatusCode, $"Set memberships status was not 200, status: " +
                                                         $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.False(s.Error, "Set memberships status reported an error, status: " +
                                             $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                        Assert.IsNotNull(r.Memberships, "Memberships list was null");
                        Assert.IsTrue(r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null, 
                            "Channel 1 membership not found");
                        Assert.IsTrue(r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null, 
                            "Channel 2 membership not found");
                        if (r.Memberships != null
                        && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null
                        && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId2) != null)
                        {
                            receivedMessage = true;
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
                            Assert.IsNotNull(r, "Update memberships result was null");
                            Assert.AreEqual(200, s.StatusCode, $"Update memberships status was not 200, status: " +
                                                             $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                            Assert.False(s.Error, "Update memberships status reported an error, status: " +
                                                 $"\n{pubnub.JsonPluggableLibrary.SerializeToJsonString(s)}");
                            Assert.IsNotNull(r.Memberships, "Memberships list was null");
                            Assert.IsTrue(r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null, 
                                "Channel 1 membership not found after update");
                            if (r.Memberships != null
                            && r.Memberships.Find(x => x.ChannelMetadata.Channel == channelMetadataId1) != null)
                            {
                                receivedMessage = true;
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

        [Test]
        public static async Task ThenSetMembershipsShouldHandleAllFeatures()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenSetMembershipsShouldHandleAllFeatures");
                return;
            }

            string uuidMetadataId = "pandu-ut-uid";
            string channelMetadataId1 = "pandu-ut-sid1";
            string channelMetadataId2 = "pandu-ut-sid2";
            string channelMetadataId3 = "pandu-ut-sid2"; // Using existing channel ID since we only have two available

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

            // First create the UUID metadata
            PNResult<PNSetUuidMetadataResult> createUserResult = await pubnub.SetUuidMetadata()
                .Uuid(uuidMetadataId)
                .Name("pandu-ut-un")
                .ExecuteAsync();

            Assert.IsNotNull(createUserResult.Result, "UUID metadata creation failed");
            Assert.AreEqual(200, createUserResult.Status.StatusCode, "UUID metadata creation status code should be 200");
            Assert.IsFalse(createUserResult.Status.Error, "UUID metadata creation should not have errors");

            // Create channel metadata for all channels
            PNResult<PNSetChannelMetadataResult> createChannel1Result = await pubnub.SetChannelMetadata()
                .Channel(channelMetadataId1)
                .Name("pandu-ut-spname")
                .ExecuteAsync();

            Assert.IsNotNull(createChannel1Result.Result, "Channel 1 metadata creation failed");
            Assert.AreEqual(200, createChannel1Result.Status.StatusCode, "Channel 1 metadata creation status code should be 200");

            PNResult<PNSetChannelMetadataResult> createChannel2Result = await pubnub.SetChannelMetadata()
                .Channel(channelMetadataId2)
                .Name("pandu-ut-spname")
                .ExecuteAsync();

            Assert.IsNotNull(createChannel2Result.Result, "Channel 2 metadata creation failed");
            Assert.AreEqual(200, createChannel2Result.Status.StatusCode, "Channel 2 metadata creation status code should be 200");

            // Test 1: Basic membership setting with custom data
            var memberships = new List<PNMembership>
            {
                new PNMembership 
                { 
                    Channel = channelMetadataId1,
                    Custom = new Dictionary<string, object> { { "role", "admin" } }
                },
                new PNMembership 
                { 
                    Channel = channelMetadataId2,
                    Custom = new Dictionary<string, object> { { "role", "member" } }
                }
            };

            PNResult<PNMembershipsResult> setMembershipsResult = await pubnub.SetMemberships()
                .Uuid(uuidMetadataId)
                .Channels(memberships)
                .Include(new[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL })
                .Limit(10)
                .IncludeCount(true)
                .ExecuteAsync();

            Assert.IsNotNull(setMembershipsResult.Result, "Set memberships result should not be null");
            Assert.AreEqual(200, setMembershipsResult.Status.StatusCode, "Set memberships status code should be 200");
            Assert.IsFalse(setMembershipsResult.Status.Error, "Set memberships should not have errors");
            Assert.IsNotNull(setMembershipsResult.Result.Memberships, "Memberships list should not be null");
            Assert.AreEqual(2, setMembershipsResult.Result.Memberships.Count, "Should have 2 memberships");
            Assert.IsTrue(setMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId1), "Should contain channel 1");
            Assert.IsTrue(setMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId2), "Should contain channel 2");
            Assert.IsTrue(setMembershipsResult.Result.Memberships.Any(m => m.Custom != null && m.Custom.ContainsKey("role")), "Should have custom data");

            // Test 2: Update existing membership and add new one
            var updatedMemberships = new List<PNMembership>
            {
                new PNMembership 
                { 
                    Channel = channelMetadataId1,
                    Custom = new Dictionary<string, object> { { "role", "superadmin" } }
                },
                new PNMembership 
                { 
                    Channel = channelMetadataId2,
                    Custom = new Dictionary<string, object> { { "role", "viewer" } }
                }
            };

            PNResult<PNMembershipsResult> updateMembershipsResult = await pubnub.SetMemberships()
                .Uuid(uuidMetadataId)
                .Channels(updatedMemberships)
                .Include(new[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL })
                .Limit(10)
                .IncludeCount(true)
                .ExecuteAsync();

            Assert.IsNotNull(updateMembershipsResult.Result, "Update memberships result should not be null");
            Assert.AreEqual(200, updateMembershipsResult.Status.StatusCode, "Update memberships status code should be 200");
            Assert.IsFalse(updateMembershipsResult.Status.Error, "Update memberships should not have errors");
            Assert.IsNotNull(updateMembershipsResult.Result.Memberships, "Memberships list should not be null");
            Assert.IsTrue(updateMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId1 && 
                m.Custom != null && m.Custom["role"].ToString() == "superadmin"), "Should have updated channel 1");
            Assert.IsTrue(updateMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId2), "Should contain channel 2");

            // Cleanup
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync();
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenMembershipOperationsShouldWorkCorrectly()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenMembershipOperationsShouldWorkCorrectly");
                return;
            }

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

            pubnub = createPubNubInstance(config);
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }

            // First create the UUID metadata
            PNResult<PNSetUuidMetadataResult> createUserResult = await pubnub.SetUuidMetadata()
                .Uuid(uuidMetadataId)
                .Name("pandu-ut-un")
                .ExecuteAsync();

            Assert.IsNotNull(createUserResult.Result, "UUID metadata creation failed");
            Assert.AreEqual(200, createUserResult.Status.StatusCode, "UUID metadata creation status code should be 200");
            Assert.IsFalse(createUserResult.Status.Error, "UUID metadata creation should not have errors");

            // Create channel metadata for all channels
            PNResult<PNSetChannelMetadataResult> createChannel1Result = await pubnub.SetChannelMetadata()
                .Channel(channelMetadataId1)
                .Name("pandu-ut-spname")
                .ExecuteAsync();

            Assert.IsNotNull(createChannel1Result.Result, "Channel 1 metadata creation failed");
            Assert.AreEqual(200, createChannel1Result.Status.StatusCode, "Channel 1 metadata creation status code should be 200");

            PNResult<PNSetChannelMetadataResult> createChannel2Result = await pubnub.SetChannelMetadata()
                .Channel(channelMetadataId2)
                .Name("pandu-ut-spname")
                .ExecuteAsync();

            Assert.IsNotNull(createChannel2Result.Result, "Channel 2 metadata creation failed");
            Assert.AreEqual(200, createChannel2Result.Status.StatusCode, "Channel 2 metadata creation status code should be 200");

            await Task.Delay(5000);
            
            // Test 1: Set Memberships
            var memberships = new List<PNMembership>
            {
                new PNMembership 
                { 
                    Channel = channelMetadataId1,
                    Custom = new Dictionary<string, object> { { "role", "admin" } }
                },
                new PNMembership 
                { 
                    Channel = channelMetadataId2,
                    Custom = new Dictionary<string, object> { { "role", "member" } }
                }
            };

            PNResult<PNMembershipsResult> setMembershipsResult = await pubnub.SetMemberships()
                .Uuid(uuidMetadataId)
                .Channels(memberships)
                .Include(new[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL })
                .Limit(10)
                .IncludeCount(true)
                .ExecuteAsync();

            Assert.IsNotNull(setMembershipsResult.Result, "Set memberships result should not be null");
            Assert.AreEqual(200, setMembershipsResult.Status.StatusCode, "Set memberships status code should be 200");
            Assert.IsFalse(setMembershipsResult.Status.Error, "Set memberships should not have errors");
            Assert.IsNotNull(setMembershipsResult.Result.Memberships, "Memberships list should not be null");
            Assert.AreEqual(2, setMembershipsResult.Result.Memberships.Count, "Should have 2 memberships");
            Assert.IsTrue(setMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId1), "Should contain channel 1");
            Assert.IsTrue(setMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId2), "Should contain channel 2");
            Assert.IsTrue(setMembershipsResult.Result.Memberships.Any(m => m.Custom != null && m.Custom.ContainsKey("role")), "Should have custom data");

            await Task.Delay(4000);

            // Test 2: Get Memberships
            PNResult<PNMembershipsResult> getMembershipsResult = await pubnub.GetMemberships()
                .Uuid(uuidMetadataId)
                .Include(new[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL })
                .Limit(10)
                .IncludeCount(true)
                .ExecuteAsync();

            Assert.IsNotNull(getMembershipsResult.Result, "Get memberships result should not be null");
            Assert.AreEqual(200, getMembershipsResult.Status.StatusCode, "Get memberships status code should be 200");
            Assert.IsFalse(getMembershipsResult.Status.Error, "Get memberships should not have errors");
            Assert.IsNotNull(getMembershipsResult.Result.Memberships, "Memberships list should not be null");
            Assert.AreEqual(2, getMembershipsResult.Result.Memberships.Count, "Should have 2 memberships");
            Assert.IsTrue(getMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId1), "Should contain channel 1");
            Assert.IsTrue(getMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId2), "Should contain channel 2");

            // Test 3: Manage Memberships (Update and Remove)
            var updatedMemberships = new List<PNMembership>
            {
                new PNMembership 
                { 
                    Channel = channelMetadataId1,
                    Custom = new Dictionary<string, object> { { "role", "superadmin" } }
                }
            };

            PNResult<PNMembershipsResult> manageMembershipsResult = await pubnub.ManageMemberships()
                .Uuid(uuidMetadataId)
                .Set(updatedMemberships)
                .Remove(new List<string> { channelMetadataId2 })
                .Include(new[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL })
                .Limit(10)
                .IncludeCount(true)
                .ExecuteAsync();

            Assert.IsNotNull(manageMembershipsResult.Result, "Manage memberships result should not be null");
            Assert.AreEqual(200, manageMembershipsResult.Status.StatusCode, "Manage memberships status code should be 200");
            Assert.IsFalse(manageMembershipsResult.Status.Error, "Manage memberships should not have errors");
            Assert.IsNotNull(manageMembershipsResult.Result.Memberships, "Memberships list should not be null");
            Assert.IsTrue(manageMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId1 && 
                m.Custom != null && m.Custom["role"].ToString() == "superadmin"), "Should have updated channel 1");
            Assert.IsFalse(manageMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId2), "Should not contain channel 2");

            // Test 4: Remove Memberships
            PNResult<PNMembershipsResult> removeMembershipsResult = await pubnub.RemoveMemberships()
                .Uuid(uuidMetadataId)
                .Channels(new List<string> { channelMetadataId1 })
                .Include(new[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL })
                .Limit(10)
                .IncludeCount(true)
                .ExecuteAsync();

            Assert.IsNotNull(removeMembershipsResult.Result, "Remove memberships result should not be null");
            Assert.AreEqual(200, removeMembershipsResult.Status.StatusCode, "Remove memberships status code should be 200");
            Assert.IsFalse(removeMembershipsResult.Status.Error, "Remove memberships should not have errors");
            Assert.IsNotNull(removeMembershipsResult.Result.Memberships, "Memberships list should not be null");
            Assert.IsFalse(removeMembershipsResult.Result.Memberships.Any(m => m.ChannelMetadata.Channel == channelMetadataId1), "Should not contain channel 1");

            await Task.Delay(4000);

            // Verify final state with Get Memberships
            PNResult<PNMembershipsResult> finalGetResult = await pubnub.GetMemberships()
                .Uuid(uuidMetadataId)
                .Include(new[] { PNMembershipField.CUSTOM, PNMembershipField.CHANNEL })
                .Limit(10)
                .IncludeCount(true)
                .ExecuteAsync();

            Assert.IsNotNull(finalGetResult.Result, "Final get memberships result should not be null");
            Assert.AreEqual(200, finalGetResult.Status.StatusCode, "Final get memberships status code should be 200");
            Assert.IsFalse(finalGetResult.Status.Error, "Final get memberships should not have errors");
            Assert.IsNotNull(finalGetResult.Result.Memberships, "Memberships list should not be null");
            Assert.AreEqual(0, finalGetResult.Result.Memberships.Count, "Should have no memberships");

            // Cleanup
            await pubnub.RemoveUuidMetadata().Uuid(uuidMetadataId).ExecuteAsync();
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId1).ExecuteAsync();
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId2).ExecuteAsync();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

    }
}
