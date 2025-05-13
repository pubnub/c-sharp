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
    public class WhenObjectChannelMetadata : TestHarness
    {
        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;
        private static Server server;
        private static string authToken;
        private static string channelMetadataId = "pandu-ut-sid";

        [SetUp]
        public static async Task Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            if (PubnubCommon.EnableStubTest)
            {
                server.Start();
            }

            if (!PubnubCommon.PAMServerSideGrant)
            {
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            var grantResult = await pubnub.GrantToken().TTL(20).AuthorizedUuid(config.UserId).Resources(
                new PNTokenResources()
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>()
                    {
                        {
                            channelMetadataId, new PNTokenAuthValues()
                            {
                                Read = true,
                                Write = true,
                                Create = true,
                                Get = true,
                                Delete = true,
                                Join = true,
                                Update = true,
                                Manage = true
                            }
                        }
                    }
                }).ExecuteAsync();
            if (!PubnubCommon.EnableStubTest)
            {
                await Task.Delay(3000);
            }

            authToken = grantResult.Result?.Token;

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(grantResult.Result != null && grantResult.Status.Error == false,
                "WhenObjectChannelMetadata Grant access failed.");
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
        public static void ThenChannelMetadataCRUDShouldReturnSuccessCodeAndInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenChannelMetadataCRUDShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

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
            pubnub.SetAuthToken(authToken);
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            manualResetEventWaitTimeout = 310 * 1000;
            System.Diagnostics.Debug.WriteLine("pubnub.DeleteSpace() STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                delegate(PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);

            #region "CreateSpace"

            System.Diagnostics.Debug.WriteLine("pubnub.CreateSpace() STARTED");
            string initialName = "pandu-ut-spname";
            string initialDescription = "Initial description";
            Dictionary<string, object> initialCustomData = new Dictionary<string, object>() { { "type", "test" } };
            
            pubnub.SetChannelMetadata()
                .Channel(channelMetadataId)
                .Name(initialName)
                .Description(initialDescription)
                .Custom(initialCustomData)
                .IncludeCustom(true)
                .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                {
                    if (r != null && s.StatusCode == 200 && !s.Error)
                    {
                        string jsonString = pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                        Debug.WriteLine($"CreateSpace Response: {jsonString}");
                        
                        // Validate all fields in the response
                        Assert.AreEqual(channelMetadataId, r.Channel, "Channel ID mismatch");
                        Assert.AreEqual(initialName, r.Name, "Channel name mismatch");
                        Assert.AreEqual(initialDescription, r.Description, "Description mismatch");
                        Assert.IsNotNull(r.Custom, "Custom data should not be null");
                        Assert.AreEqual("test", r.Custom["type"], "Custom data type value mismatch");
                        Assert.IsNotNull(r.Updated, "Updated timestamp should not be null");
                        
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

                #region "SetChannelMetadata"

                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
                Dictionary<string, object> customData = new Dictionary<string, object>() { { "color", "red" } };
                string expectedName = "pandu-ut-spname-upd";
                string expectedDescription = "pandu-ut-spdesc";
                
                pubnub.SetChannelMetadata()
                    .Channel(channelMetadataId)
                    .Name(expectedName)
                    .Description(expectedDescription)
                    .Custom(customData)
                    .IncludeCustom(true)
                    .Execute(new PNSetChannelMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            string jsonString = pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            Debug.WriteLine($"SetChannelMetadata Response: {jsonString}");
                            
                            // Validate all fields in the response
                            Assert.AreEqual(channelMetadataId, r.Channel, "Channel ID mismatch");
                            Assert.AreEqual(expectedName, r.Name, "Channel name mismatch");
                            Assert.AreEqual(expectedDescription, r.Description, "Description mismatch");
                            Assert.IsNotNull(r.Custom, "Custom data should not be null");
                            Assert.AreEqual("red", r.Custom["color"], "Custom data color value mismatch");
                            Assert.IsNotNull(r.Updated, "Updated timestamp should not be null");
                            
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

                #region "GetChannelMetadata"

                System.Diagnostics.Debug.WriteLine("pubnub.GetChannelMetadata() STARTED");
                pubnub.GetChannelMetadata().Channel(channelMetadataId).IncludeCustom(true)
                    .Execute(new PNGetChannelMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            string jsonString = pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            Debug.WriteLine($"GetChannelMetadata Response: {jsonString}");
                            
                            // Validate all fields in the response
                            Assert.AreEqual(channelMetadataId, r.Channel, "Channel ID mismatch");
                            Assert.AreEqual("pandu-ut-spname-upd", r.Name, "Channel name mismatch");
                            Assert.AreEqual("pandu-ut-spdesc", r.Description, "Description mismatch");
                            Assert.IsNotNull(r.Custom, "Custom data should not be null");
                            Assert.AreEqual("red", r.Custom["color"], "Custom data color value mismatch");
                            Assert.IsNotNull(r.Updated, "Updated timestamp should not be null");
                            
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

                #region "GetAllChannelMetadata"

                System.Diagnostics.Debug.WriteLine("pubnub.GetAllChannelMetadata() STARTED");
                pubnub.GetAllChannelMetadata().IncludeCount(true).IncludeCustom(true)
                    .Execute(new PNGetAllChannelMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            string jsonString = pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            Debug.WriteLine($"GetAllChannelMetadata Response: {jsonString}");
                            
                            // Validate all fields in the response
                            Assert.IsNotNull(r.Channels, "Channels list should not be null");
                            Assert.Greater(r.Channels.Count, 0, "Channels list should not be empty");
                            
                            PNChannelMetadataResult channelMetadata = r.Channels.Find(x => x.Channel == channelMetadataId);
                            Assert.IsNotNull(channelMetadata, "Channel metadata not found in list");
                            
                            // Validate the found channel metadata
                            Assert.AreEqual(channelMetadataId, channelMetadata.Channel, "Channel ID mismatch");
                            Assert.AreEqual("pandu-ut-spname-upd", channelMetadata.Name, "Channel name mismatch");
                            Assert.AreEqual("pandu-ut-spdesc", channelMetadata.Description, "Description mismatch");
                            Assert.IsNotNull(channelMetadata.Custom, "Custom data should not be null");
                            Assert.AreEqual("red", channelMetadata.Custom["color"], "Custom data color value mismatch");
                            Assert.IsNotNull(channelMetadata.Updated, "Updated timestamp should not be null");
                            
                            // Validate pagination data
                            Assert.Greater(r.TotalCount, 0, "Total count should be greater than 0");
                            
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

                #region "RemoveChannelMetadata"

                System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
                pubnub.RemoveChannelMetadata().Channel(channelMetadataId)
                    .Execute(new PNRemoveChannelMetadataResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            string jsonString = pubnub.JsonPluggableLibrary.SerializeToJsonString(r);
                            Debug.WriteLine($"RemoveChannelMetadata Response: {jsonString}");
                            
                            // No properties to validate as the response is empty
                            receivedMessage = true;
                        }

                        manualEvent.Set();
                    }));

                #endregion

                manualEvent.WaitOne(manualResetEventWaitTimeout);
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, $"ChannelMetadata CRUD operations failed.");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncChannelMetadataCRUDShouldReturnSuccessCodeAndInfo()
#else
        public static async Task ThenWithAsyncChannelMetadataCRUDShouldReturnSuccessCodeAndInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncChannelMetadataCRUDShouldReturnSuccessCodeAndInfo");
                return;
            }

            bool receivedMessage = false;

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
            pubnub.SetAuthToken(authToken);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync());
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif

            #region "SetChannelMetadata"

            System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
#if NET40
            PNResult<PNSetChannelMetadataResult> createSpaceResult =
 Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetChannelMetadataResult> createSpaceResult = await pubnub.SetChannelMetadata()
                .Channel(channelMetadataId).Name("pandu-ut-spname").ExecuteAsync();
#endif
            if (createSpaceResult.Result != null && createSpaceResult.Status.StatusCode == 200 &&
                !createSpaceResult.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpaceResult.Result);
                if (channelMetadataId == createSpaceResult.Result.Channel)
                {
                    receivedMessage = true;
                }
            }

            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;

                #region "SetChannelMetadata"

                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> updateSpaceResult =
 Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> updateSpaceResult = await pubnub.SetChannelMetadata()
                    .Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                    .ExecuteAsync();
#endif
                if (updateSpaceResult.Result != null && updateSpaceResult.Status.StatusCode == 200 &&
                    !updateSpaceResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(updateSpaceResult.Result);
                    if (channelMetadataId == updateSpaceResult.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }

                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;

                #region "GetSpace"

                System.Diagnostics.Debug.WriteLine("pubnub.GetChannelMetadata() STARTED");
#if NET40
                PNResult<PNGetChannelMetadataResult> getSpaceResult =
 Task.Factory.StartNew(async () => await pubnub.GetChannelMetadata().Channel(channelMetadataId).IncludeCustom(true)
                    .ExecuteAsync()).Result.Result;
#else
                PNResult<PNGetChannelMetadataResult> getSpaceResult = await pubnub.GetChannelMetadata()
                    .Channel(channelMetadataId).IncludeCustom(true)
                    .ExecuteAsync();
#endif
                if (getSpaceResult.Result != null && getSpaceResult.Status.StatusCode == 200 &&
                    !getSpaceResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getSpaceResult.Result);
                    if (channelMetadataId == getSpaceResult.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }

                #endregion
            }

            if (receivedMessage)
            {
                receivedMessage = false;

                #region "GetSpaces"

                System.Diagnostics.Debug.WriteLine("pubnub.GetAllChannelMetadata() STARTED");
#if NET40
                PNResult<PNGetAllChannelMetadataResult> getSpacesResult =
 Task.Factory.StartNew(async () => await pubnub.GetAllChannelMetadata().IncludeCount(true).ExecuteAsync()).Result.Result;
#else
                PNResult<PNGetAllChannelMetadataResult> getSpacesResult =
                    await pubnub.GetAllChannelMetadata().IncludeCount(true).ExecuteAsync();
#endif
                if (getSpacesResult.Result != null && getSpacesResult.Status.StatusCode == 200 &&
                    !getSpacesResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(getSpacesResult.Result);
                    List<PNChannelMetadataResult> spaceList = getSpacesResult.Result.Channels;
                    if (spaceList != null && spaceList.Count > 0 &&
                        spaceList.Find(x => x.Channel == channelMetadataId) != null)
                    {
                        receivedMessage = true;
                    }
                }

                #endregion
            }

            if (!receivedMessage)
            {
                Assert.IsTrue(receivedMessage, "SeChannelMetadata/DeleteChannelMetadata Failed");
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenChannelMetadataSetDeleteShouldReturnEventInfo()
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenChannelMetadataSetDeleteShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;
            
            manualResetEventWaitTimeout = 310 * 1000;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate(Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" +
                                                       pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "channel")
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
                delegate(Pubnub pnObj, PNStatus status) { }
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
            pubnub.SetAuthToken(authToken);
            pubnub.AddListener(eventListener);

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channelMetadataId }).Execute();
            manualEvent.WaitOne(2000);

            manualEvent = new ManualResetEvent(false);
            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
            pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                delegate(PNRemoveChannelMetadataResult result, PNStatus status) { }));
            manualEvent.WaitOne(2000);

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

            if (receivedMessage)
            {
                receivedMessage = false;
                manualEvent = new ManualResetEvent(false);

                #region "SetChannelMetadata"

                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
                pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
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

            if (!receivedDeleteEvent)
            {
                manualEvent = new ManualResetEvent(false);
                System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
                pubnub.RemoveChannelMetadata().Channel(channelMetadataId).Execute(new PNRemoveChannelMetadataResultExt(
                    delegate(PNRemoveChannelMetadataResult result, PNStatus status) { }));
                manualEvent.WaitOne(2000);
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channelMetadataId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent, "Channel Metadata events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
#if NET40
        public static void ThenWithAsyncChannelMetadataUpdateDeleteShouldReturnEventInfo()
#else
        public static async Task ThenWithAsyncChannelMetadataUpdateDeleteShouldReturnEventInfo()
#endif
        {
            server.ClearRequests();

            if (PubnubCommon.EnableStubTest)
            {
                Assert.Ignore("Ignored ThenWithAsyncChannelMetadataUpdateDeleteShouldReturnEventInfo");
                return;
            }

            bool receivedMessage = false;
            bool receivedDeleteEvent = false;
            bool receivedUpdateEvent = false;
            
            manualResetEventWaitTimeout = 310 * 1000;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate(Pubnub pnObj, PNObjectEventResult eventResult)
                {
                    System.Diagnostics.Debug.WriteLine("EVENT:" +
                                                       pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                    if (eventResult.Type.ToLowerInvariant() == "channel")
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
                delegate(Pubnub pnObj, PNStatus status) { }
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
            pubnub.SetAuthToken(authToken);
            pubnub.AddListener(eventListener);

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channelMetadataId }).Execute();
            manualEvent.WaitOne(2000);

            System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() STARTED");
#if NET40
            Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync());
#else
            await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif

            #region "SetChannelMetadata"

            System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
#if NET40
            PNResult<PNSetChannelMetadataResult> createSpaceResult =
 Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname").ExecuteAsync()).Result.Result;
#else
            PNResult<PNSetChannelMetadataResult> createSpaceResult = await pubnub.SetChannelMetadata()
                .Channel(channelMetadataId).Name("pandu-ut-spname").ExecuteAsync();
#endif
            if (createSpaceResult.Result != null && createSpaceResult.Status.StatusCode == 200 &&
                !createSpaceResult.Status.Error)
            {
                pubnub.JsonPluggableLibrary.SerializeToJsonString(createSpaceResult.Result);
                if (channelMetadataId == createSpaceResult.Result.Channel)
                {
                    receivedMessage = true;
                }
            }

            #endregion

            if (receivedMessage)
            {
                receivedMessage = false;

                #region "SetChannelMetadata"

                System.Diagnostics.Debug.WriteLine("pubnub.SetChannelMetadata() STARTED");
#if NET40
                PNResult<PNSetChannelMetadataResult> updateSpaceResult =
 Task.Factory.StartNew(async () => await pubnub.SetChannelMetadata().Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                        .ExecuteAsync()).Result.Result;
#else
                PNResult<PNSetChannelMetadataResult> updateSpaceResult = await pubnub.SetChannelMetadata()
                    .Channel(channelMetadataId).Name("pandu-ut-spname-upd")
                    .Description("pandu-ut-spdesc")
                    .Custom(new Dictionary<string, object>() { { "color", "red" } })
                    .ExecuteAsync();
#endif
                if (updateSpaceResult.Result != null && updateSpaceResult.Status.StatusCode == 200 &&
                    !updateSpaceResult.Status.Error)
                {
                    pubnub.JsonPluggableLibrary.SerializeToJsonString(updateSpaceResult.Result);
                    if (channelMetadataId == updateSpaceResult.Result.Channel)
                    {
                        receivedMessage = true;
                    }
                }

                #endregion
            }

            if (!receivedDeleteEvent)
            {
                System.Diagnostics.Debug.WriteLine("pubnub.RemoveChannelMetadata() 2 STARTED");
#if NET40
                Task.Factory.StartNew(async () => await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync());
#else
                await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
#endif
            }

            Thread.Sleep(2000);

            pubnub.Unsubscribe<string>().Channels(new string[] { channelMetadataId }).Execute();
            pubnub.RemoveListener(eventListener);

            Assert.IsTrue(receivedDeleteEvent && receivedUpdateEvent, "Space events Failed");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
    }
}