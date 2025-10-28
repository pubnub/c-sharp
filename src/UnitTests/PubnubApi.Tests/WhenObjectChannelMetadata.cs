using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenObjectChannelMetadata : TestHarness
    {
        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;
        private static Server server;
        private static string authToken;
        private static string channelMetadataId = "foo.pandu-ut-sid";

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

            if (string.IsNullOrEmpty(PubnubCommon.GrantToken))
            {
                await GenerateTestGrantToken(pubnub);
            }
            authToken = PubnubCommon.GrantToken;
            
            if (!PubnubCommon.EnableStubTest)
            {
                await Task.Delay(3000);
            }
            
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
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
        public static async Task ThenChannelMetadataCRUDShouldReturnSuccessCodeAndInfo()
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
            await Task.Delay(2000);

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
            
            await Task.Delay(2000);

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
                pubnub.GetAllChannelMetadata().IncludeCount(true).Sort(new List<string>(){"updated:desc"}).IncludeCustom(true)
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
                    await pubnub.GetAllChannelMetadata().IncludeCount(true).Sort(new List<string>(){"updated:desc"}).ExecuteAsync();
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

        [Test]
        public static async Task ThenSetChannelMembersShouldReturnSuccess()
        {
            string testChannel = $"foo.tc_{Guid.NewGuid()}";
            string testUuid = "fuu.test-uuid-1";
            Dictionary<string, object> customData = new Dictionary<string, object> { { "role", "admin" } };

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
            pubnub.SetAuthToken(authToken);

            // Create channel member
            var member = new PNChannelMember
            {
                Uuid = testUuid,
                Custom = customData
            };

            // Set channel member
            PNResult<PNChannelMembersResult> setResult = await pubnub.SetChannelMembers()
                .Channel(testChannel)
                .Uuids(new List<PNChannelMember> { member })
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(setResult, "Set result should not be null");
            Assert.IsNotNull(setResult.Result, "Set result data should not be null");
            Assert.IsFalse(setResult.Status.Error, "Set operation should not have errors");
            Assert.AreEqual(200, setResult.Status.StatusCode, "Set operation should return 200 status code");
            Assert.IsNotNull(setResult.Result.ChannelMembers, "Members list should not be null");
            Assert.AreEqual(1, setResult.Result.ChannelMembers.Count, "Should have one member");
            Assert.AreEqual(testUuid, setResult.Result.ChannelMembers[0].UuidMetadata.Uuid, "Member UUID should match");
            Assert.IsNotNull(setResult.Result.ChannelMembers[0].Custom, "Member custom data should not be null");
            Assert.AreEqual("admin", setResult.Result.ChannelMembers[0].Custom["role"], "Member role should be admin");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenSetAndGetChannelMembersShouldReturnSuccess()
        {
            string testChannel = $"foo.tc_{Guid.NewGuid()}";
            string testUuid = "fuu.test-uuid-2";
            Dictionary<string, object> customData = new Dictionary<string, object> { { "role", "user" } };

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
            pubnub.SetAuthToken(authToken);

            // Create and set channel member
            var member = new PNChannelMember
            {
                Uuid = testUuid,
                Custom = customData
            };

            PNResult<PNChannelMembersResult> setResult = await pubnub.SetChannelMembers()
                .Channel(testChannel)
                .Uuids(new List<PNChannelMember> { member })
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(setResult, "Set result should not be null");
            Assert.IsFalse(setResult.Status.Error, "Set operation should not have errors");

            await Task.Delay(4000);

            // Get channel members
            PNResult<PNChannelMembersResult> getResult = await pubnub.GetChannelMembers()
                .Channel(testChannel)
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(getResult, "Get result should not be null");
            Assert.IsNotNull(getResult.Result, "Get result data should not be null");
            Assert.IsFalse(getResult.Status.Error, "Get operation should not have errors");
            Assert.AreEqual(200, getResult.Status.StatusCode, "Get operation should return 200 status code");
            Assert.IsNotNull(getResult.Result.ChannelMembers, "Members list should not be null");
            Assert.AreEqual(1, getResult.Result.ChannelMembers.Count, "Should have one member");
            Assert.AreEqual(testUuid, getResult.Result.ChannelMembers[0].UuidMetadata.Uuid, "Member UUID should match");
            Assert.IsNotNull(getResult.Result.ChannelMembers[0].Custom, "Member custom data should not be null");
            Assert.AreEqual("user", getResult.Result.ChannelMembers[0].Custom["role"], "Member role should be user");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenSetGetAndRemoveChannelMembersShouldReturnSuccess()
        {
            string testChannel = $"foo.tc_{Guid.NewGuid()}";
            string testUuid = "fuu.test-uuid-3";
            Dictionary<string, object> customData = new Dictionary<string, object> { { "role", "moderator" } };

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
            pubnub.SetAuthToken(authToken);

            // Create and set channel member
            var member = new PNChannelMember
            {
                Uuid = testUuid,
                Custom = customData
            };

            PNResult<PNChannelMembersResult> setResult = await pubnub.SetChannelMembers()
                .Channel(testChannel)
                .Uuids(new List<PNChannelMember> { member })
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(setResult, "Set result should not be null");
            Assert.IsFalse(setResult.Status.Error, "Set operation should not have errors");

            await Task.Delay(6000);

            // Get channel members before removal
            PNResult<PNChannelMembersResult> getResult = await pubnub.GetChannelMembers()
                .Channel(testChannel)
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(getResult, "Get result should not be null");
            Assert.IsFalse(getResult.Status.Error, "Get operation should not have errors");
            Assert.AreEqual(1, getResult.Result.ChannelMembers.Count, "Should have one member before removal");

            // Remove channel member
            PNResult<PNChannelMembersResult> removeResult = await pubnub.RemoveChannelMembers()
                .Channel(testChannel)
                .Uuids(new List<string> { testUuid })
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(removeResult, "Remove result should not be null");
            Assert.IsNotNull(removeResult.Result, "Remove result data should not be null");
            Assert.IsFalse(removeResult.Status.Error, "Remove operation should not have errors");
            Assert.AreEqual(200, removeResult.Status.StatusCode, "Remove operation should return 200 status code");
            await Task.Delay(500); 

            // Get channel members after removal
            PNResult<PNChannelMembersResult> getAfterRemoveResult = await pubnub.GetChannelMembers()
                .Channel(testChannel)
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(getAfterRemoveResult, "Get after remove result should not be null");
            Assert.IsFalse(getAfterRemoveResult.Status.Error, "Get after remove operation should not have errors");
            Assert.AreEqual(0, getAfterRemoveResult.Result.ChannelMembers.Count, "Should have no members after removal");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static async Task ThenSetAndManageChannelMembersShouldReturnSuccess()
        {
            string testChannel = $"foo.tc_{Guid.NewGuid()}";
            string testUuid1 = $"fuu.tu_{Guid.NewGuid()}";
            string testUuid2 = $"fuu.tu_{Guid.NewGuid()}";
            Dictionary<string, object> customData1 = new Dictionary<string, object> { { "role", "admin" } };
            Dictionary<string, object> customData2 = new Dictionary<string, object> { { "role", "user" } };

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
            pubnub.SetAuthToken(authToken);

            // Create and set initial channel member
            var member1 = new PNChannelMember
            {
                Uuid = testUuid1,
                Custom = customData1
            };

            PNResult<PNChannelMembersResult> setResult = await pubnub.SetChannelMembers()
                .Channel(testChannel)
                .Uuids(new List<PNChannelMember> { member1 })
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(setResult, "Set result should not be null");
            Assert.IsFalse(setResult.Status.Error, "Set operation should not have errors");

            await Task.Delay(6000);

            // Manage channel members (add new, update existing)
            var member2 = new PNChannelMember
            {
                Uuid = testUuid2,
                Custom = customData2
            };

            PNResult<PNChannelMembersResult> manageResult = await pubnub.ManageChannelMembers()
                .Channel(testChannel)
                .Set(new List<PNChannelMember> { member1, member2 })
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(manageResult, "Manage result should not be null");
            Assert.IsNotNull(manageResult.Result, "Manage result data should not be null");
            Assert.IsFalse(manageResult.Status.Error, "Manage operation should not have errors");
            Assert.AreEqual(200, manageResult.Status.StatusCode, "Manage operation should return 200 status code");
            Assert.IsNotNull(manageResult.Result.ChannelMembers, "Members list should not be null");
            Assert.AreEqual(2, manageResult.Result.ChannelMembers.Count, "Should have two members");

            await Task.Delay(5000);

            // Get channel members to verify
            PNResult<PNChannelMembersResult> getResult = await pubnub.GetChannelMembers()
                .Channel(testChannel)
                .Include(new[] { PNChannelMemberField.CUSTOM, PNChannelMemberField.UUID })
                .ExecuteAsync();

            Assert.IsNotNull(getResult, "Get result should not be null");
            Assert.IsFalse(getResult.Status.Error, "Get operation should not have errors");
            Assert.AreEqual(2, getResult.Result.ChannelMembers.Count, "Should have two members after manage operation");
            
            var members = getResult.Result.ChannelMembers;
            Assert.IsTrue(members.Any(m => m.UuidMetadata.Uuid == testUuid1 && m.Custom["role"].ToString() == "admin"), "First member should exist with admin role");
            Assert.IsTrue(members.Any(m => m.UuidMetadata.Uuid == testUuid2 && m.Custom["role"].ToString() == "user"), "Second member should exist with user role");

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
        
        [Test]
        public static async Task ThenChannelMetadataShouldSupportStatusAndTypeFields()
        {
            var r = new Random();
            string channelMetadataId = $"channel{r.Next(100, 1000)}";
            string status = $"status{r.Next(100, 1000)}";
            string type = $"type{r.Next(100, 1000)}";
            string name = $"name{r.Next(100, 1000)}";
            string description = $"description{r.Next(100, 1000)}";
            
            PNConfiguration configuration = new PNConfiguration(new UserId($"user{r.Next(100,1000)}"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey
            };
            pubnub = createPubNubInstance(configuration);
            
            bool receivedSetEvent = false;
            ManualResetEvent setEventManualEvent = new ManualResetEvent(false);
            
            var channelSubscription = pubnub.Channel(channelMetadataId).Subscription();
            channelSubscription.onObject += (_, appContextEvent) =>
            {
                var eventType = appContextEvent.Type;
                // check event type to be 'channel'
                Assert.That(eventType.ToLowerInvariant(), Is.EqualTo("channel"), "Event type should be 'channel'");
                
                if (appContextEvent.Event.ToLowerInvariant() == "set")
                {
                    receivedSetEvent = true;
                    // check value of status and type to match set value
                    Assert.That(appContextEvent.ChannelMetadata, Is.Not.Null, "ChannelMetadata should not be null for set event");
                    Assert.That(appContextEvent.ChannelMetadata.Status, Is.EqualTo(status), "Status should match the set value");
                    Assert.That(appContextEvent.ChannelMetadata.Type, Is.EqualTo(type), "Type should match the set value");
                    Assert.That(appContextEvent.ChannelMetadata.Name, Is.EqualTo(name), "Name should match the set value");
                    Assert.That(appContextEvent.ChannelMetadata.Description, Is.EqualTo(description), "Description should match the set value");
                    Assert.That(appContextEvent.ChannelMetadata.Custom, Is.Not.Null, "Custom should not be null");
                    Assert.That(appContextEvent.ChannelMetadata.Custom.ContainsKey("key"), Is.True, "Custom should contain 'key'");
                    Assert.That(appContextEvent.ChannelMetadata.Custom["key"].ToString(), Is.EqualTo("value"), "Custom key value should match");
                    setEventManualEvent.Set();
                }
            };
            channelSubscription.Subscribe<object>();
            
            await Task.Delay(2000);
            // now set the Channel metadata.
            
            var setChannelMetadata = await pubnub.SetChannelMetadata()
                .Channel(channelMetadataId)
                .Status(status)
                .Name(name)
                .Type(type)
                .Description(description)
                .Custom(new Dictionary<string, object> { { "key", "value" } })
                .ExecuteAsync();
            
            // check channel metadata return status and type., Custom also?
            Assert.That(setChannelMetadata.Result, Is.Not.Null, "SetChannelMetadata result should not be null");
            Assert.That(setChannelMetadata.Status.StatusCode, Is.EqualTo(200), "SetChannelMetadata status code should be 200");
            Assert.That(setChannelMetadata.Status.Error, Is.False, "SetChannelMetadata should not indicate error");
            Assert.That(setChannelMetadata.Result.Channel, Is.EqualTo(channelMetadataId), "Channel should match");
            Assert.That(setChannelMetadata.Result.Name, Is.EqualTo(name), "Name should match");
            Assert.That(setChannelMetadata.Result.Status, Is.EqualTo(status), "Status should match");
            Assert.That(setChannelMetadata.Result.Type, Is.EqualTo(type), "Type should match");
            Assert.That(setChannelMetadata.Result.Description, Is.EqualTo(description), "Description should match");
            Assert.That(setChannelMetadata.Result.Custom, Is.Not.Null, "Custom should not be null");
            Assert.That(setChannelMetadata.Result.Custom.ContainsKey("key"), Is.True, "Custom should contain 'key'");
            Assert.That(setChannelMetadata.Result.Custom["key"].ToString(), Is.EqualTo("value"), "Custom key value should match");
            
            await Task.Delay(2000);

            var getChannelMetadata = await pubnub.GetChannelMetadata()
                .Channel(channelMetadataId)
                .IncludeStatus(true)
                .IncludeType(true)
                .IncludeCustom(true)
                .ExecuteAsync();
            
            // check channel data should contain status, type and custom fields using Assert.
            Assert.That(getChannelMetadata.Result, Is.Not.Null, "GetChannelMetadata result should not be null");
            Assert.That(getChannelMetadata.Status.StatusCode, Is.EqualTo(200), "GetChannelMetadata status code should be 200");
            Assert.That(getChannelMetadata.Status.Error, Is.False, "GetChannelMetadata should not indicate error");
            Assert.That(getChannelMetadata.Result.Channel, Is.EqualTo(channelMetadataId), "Channel should match");
            Assert.That(getChannelMetadata.Result.Name, Is.EqualTo(name), "Name should match");
            Assert.That(getChannelMetadata.Result.Status, Is.EqualTo(status), "Status should match");
            Assert.That(getChannelMetadata.Result.Type, Is.EqualTo(type), "Type should match");
            Assert.That(getChannelMetadata.Result.Description, Is.EqualTo(description), "Description should match");
            Assert.That(getChannelMetadata.Result.Custom, Is.Not.Null, "Custom should not be null");
            Assert.That(getChannelMetadata.Result.Custom.ContainsKey("key"), Is.True, "Custom should contain 'key'");
            Assert.That(getChannelMetadata.Result.Custom["key"].ToString(), Is.EqualTo("value"), "Custom key value should match");
            
            // Wait for subscription events
            setEventManualEvent.WaitOne(3000);
            Assert.That(receivedSetEvent, Is.True, "Should have received set event via subscription");
            
            // As a part of cleanup delete the channelMetadata
            var removeChannelMetadata = await pubnub.RemoveChannelMetadata().Channel(channelMetadataId).ExecuteAsync();
            Assert.That(removeChannelMetadata.Result, Is.Not.Null, "RemoveChannelMetadata result should not be null");
            Assert.That(removeChannelMetadata.Status.StatusCode, Is.EqualTo(200), "RemoveChannelMetadata status code should be 200");
            Assert.That(removeChannelMetadata.Status.Error, Is.False, "RemoveChannelMetadata should not indicate error");
            
            // Wait for delete event
            await Task.Delay(2000);
            
            // Cleanup
            channelSubscription.Unsubscribe<object>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }
        [Test]
        public static async Task ThenChannelMetadataSetShouldWorkWithSecretKey()
        {
            var r = new Random();
            string channelMetadataId = $"channel{r.Next(100, 1000)}";
            string status = $"status{r.Next(100, 1000)}";
            string type = $"type{r.Next(100, 1000)}";
            string name =  $"name{r.Next(100, 1000)}";
            PNConfiguration configuration = new PNConfiguration(new UserId($"user{r.Next(100,1000)}"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };
            pubnub = createPubNubInstance(configuration);
            var channelMetadata =  await pubnub.SetChannelMetadata().
                Channel(channelMetadataId).
                Status(status).
                Name(name).
                Type(type).
                Custom(new Dictionary<string, object>{ {"key", "value"}}).
                ExecuteAsync();
            Assert.That(channelMetadata.Result, Is.Not.Null);
            Assert.AreEqual(channelMetadata.Status.StatusCode, 200);
            pubnub.Destroy();
            pubnub =  null;
        }
        
        [Test]
        public static async Task ThenChannelMetadataSetShouldWorkWithToken()
        {
            var r = new Random();
            string channelMetadataId = $"channel{r.Next(100, 1000)}";
            string status = $"status{r.Next(100, 1000)}";
            string type = $"type{r.Next(100, 1000)}";
            string name =  $"name{r.Next(100, 1000)}";
            var id = $"test_{new Random().Next(1000, 10000)}";
            PNConfiguration configuration = new PNConfiguration(new UserId($"user_{id}"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };
            pubnub = createPubNubInstance(configuration);
            PNResult<PNAccessManagerTokenResult> grantTokenResponse = await pubnub.GrantToken()
                .TTL(15)
                .AuthorizedUuid($"user_{id}")
                .Resources(new PNTokenResources
                {
                    Uuids = new Dictionary<string, PNTokenAuthValues>
                    {
                        { channelMetadataId, new PNTokenAuthValues { Read = true, Write = true, Join = true, Manage = true} }
                    }
                })
                .ExecuteAsync();
            var token =  grantTokenResponse.Result.Token;
            Assert.NotNull(token, "token should not be null for uuid setter");
            // wait after grant token to be effective
            await Task.Delay(1000);
            PNConfiguration channelMetadataSetterConfiguration = new PNConfiguration(new UserId($"user_{id}"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };
            var channelMetadataSetter = createPubNubInstance(channelMetadataSetterConfiguration, token);
            var setChannelMetadata =  await pubnub.SetChannelMetadata().
                Channel(channelMetadataId).
                Status(status).
                Name(name).
                Type(type).
                Custom(new Dictionary<string, object>{ {"key", "value"}}).
                ExecuteAsync();
            Assert.That(setChannelMetadata.Result, Is.Not.Null);
            Assert.AreEqual(setChannelMetadata.Status.StatusCode, 200);
            pubnub.Destroy();
            channelMetadataSetter.Destroy();
        }
    }
}