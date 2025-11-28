using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Matchers;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribeWithDuplicateChannels
    {
        private WireMockServer _mockServer;
        private Pubnub _pubnub;
        private const int HEARTBEAT_INTERVAL = 3;
        private const int PRESENCE_TIMEOUT = 10;
        private const int EVENT_TIMEOUT_MS = 2000;

        [SetUp]
        public void Setup()
        {
            _mockServer = WireMockServer.Start();

            SetupSubscribeMocks();
            SetupHeartbeatMocks();
            SetupLeaveMocks();

            var config = new PNConfiguration(new UserId("csharp"))
            {
                SubscribeKey = "demo",
                PublishKey = "demo",
                Origin = $"localhost:{_mockServer.Port}",
                Secure = false,
                EnableEventEngine = true
            };
            config.SetPresenceTimeoutWithCustomInterval(PRESENCE_TIMEOUT, HEARTBEAT_INTERVAL);

            _pubnub = new Pubnub(config);
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_pubnub != null)
            {
                try
                {
                    _pubnub.UnsubscribeAll<object>();
                    await Task.Delay(200);
                }
                catch { }
                
                try
                {
                    _pubnub.Destroy();
                }
                catch { }
            }
            
            _mockServer?.Stop();
            _mockServer?.Dispose();
        }

        [Test]
        public async Task ThenSubscribeWithDuplicateChannels_DeduplicatesAndSendsHeartbeats()
        {
            var connectedReceived = new TaskCompletionSource<bool>();

            var listener = new SubscribeCallbackExt(
                (pn, msg) => { },
                (pn, presence) => { },
                (pn, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                        connectedReceived.TrySetResult(true);
                }
            );
            _pubnub.AddListener(listener);

            _pubnub.Subscribe<object>().Channels(new[] { "c1" }).Execute();
            await Task.WhenAny(connectedReceived.Task, Task.Delay(EVENT_TIMEOUT_MS));

            // Subscribe with duplicates - should be deduplicated
            _pubnub.Subscribe<object>().Channels(new[] { "c1", "c1" }).Execute();
            await Task.Delay(200);

            _pubnub.Subscribe<object>().Channels(new[] { "c1", "c1" }).Execute();
            
            // Wait for at least 1 heartbeat
            await Task.Delay(4000);

            var subscribeRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/v2/subscribe/demo/"))
                .ToList();

            Assert.That(subscribeRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least 1 subscribe request (initial handshake)");

            var handshakeRequests = subscribeRequests
                .Cast<WireMock.Logging.LogEntry>()
                .Where(r => HasQueryParameter(r, "tt", "0"))
                .ToList();
            
            Assert.That(handshakeRequests.Count, Is.EqualTo(1),
                "Should have exactly 1 handshake request with tt=0");

            var heartbeatRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/heartbeat"))
                .ToList();

            Assert.That(heartbeatRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least 1 heartbeat request");

            VerifyHeartbeatsForSingleChannel(heartbeatRequests.Cast<WireMock.Logging.LogEntry>().ToList());
        }

        [Test]
        public async Task ThenSubscribeToSingleChannel_SendsHeartbeatsAutomatically()
        {
            var connectedReceived = new TaskCompletionSource<bool>();

            var listener = new SubscribeCallbackExt(
                (pn, msg) => { },
                (pn, presence) => { },
                (pn, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                        connectedReceived.TrySetResult(true);
                }
            );
            _pubnub.AddListener(listener);

            _pubnub.Subscribe<object>().Channels(new[] { "c1" }).Execute();
            await Task.WhenAny(connectedReceived.Task, Task.Delay(EVENT_TIMEOUT_MS));

            // Wait for at least 1 heartbeat (interval is 3s, wait 4s to be safe)
            await Task.Delay(4000);

            var subscribeRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/v2/subscribe/demo/c1/0"))
                .ToList();

            Assert.That(subscribeRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least 1 subscribe request for c1");

            var heartbeatRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/channel/c1/heartbeat"))
                .ToList();

            Assert.That(heartbeatRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least 1 heartbeat request");
        }

        [Test]
        public async Task ThenSubscribeWithDifferentChannels_MakesNewSubscribeRequest()
        {
            var connectedReceived = new TaskCompletionSource<bool>();
            var subscriptionChangedReceived = new TaskCompletionSource<bool>();

            var listener = new SubscribeCallbackExt(
                (pn, msg) => { },
                (pn, presence) => { },
                (pn, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory && !connectedReceived.Task.IsCompleted)
                        connectedReceived.TrySetResult(true);
                    else if (status.Category == PNStatusCategory.PNSubscriptionChangedCategory)
                        subscriptionChangedReceived.TrySetResult(true);
                }
            );
            _pubnub.AddListener(listener);

            _pubnub.Subscribe<object>().Channels(new[] { "c1" }).Execute();
            await Task.WhenAny(connectedReceived.Task, Task.Delay(EVENT_TIMEOUT_MS));

            _pubnub.Subscribe<object>().Channels(new[] { "c1", "c2" }).Execute();
            await Task.WhenAny(subscriptionChangedReceived.Task, Task.Delay(EVENT_TIMEOUT_MS));

            var subscribeRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/v2/subscribe/demo/"))
                .ToList();

            var c1OnlyRequests = subscribeRequests
                .Where(r => r.RequestMessage.Path.Contains("/c1/0") && 
                           !r.RequestMessage.Path.Contains("/c1,c2/"))
                .ToList();

            Assert.That(c1OnlyRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least one request for c1 only");

            var heartbeatRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/heartbeat"))
                .ToList();

            Assert.That(heartbeatRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least one heartbeat request");
        }

        /// <summary>
        /// Tests that subscribing to the same channel twice does NOT trigger a new handshake.
        /// It emits a status with PNSubscriptionChangedCategory without making additional network requests.
        /// </summary>
        [Test]
        public async Task ThenSubscribeToSameChannelTwice_EmitsStatusWithoutNewHandshake()
        {
            var statusEvents = new System.Collections.Generic.List<PNStatus>();
            var connectedReceived = new TaskCompletionSource<bool>();
            var secondStatusReceived = new TaskCompletionSource<bool>();
            int statusCount = 0;

            var listener = new SubscribeCallbackExt(
                (pn, msg) => { },
                (pn, presence) => { },
                (pn, status) =>
                {
                    statusEvents.Add(status);
                    statusCount++;
                    
                    if (statusCount == 1 && status.Category == PNStatusCategory.PNConnectedCategory)
                        connectedReceived.TrySetResult(true);
                    else if (statusCount == 2 && status.Category == PNStatusCategory.PNSubscriptionChangedCategory)
                        secondStatusReceived.TrySetResult(true);
                }
            );
            _pubnub.AddListener(listener);

            _pubnub.Subscribe<object>().Channels(new[] { "c1" }).Execute();

            var firstConnectedTask = await Task.WhenAny(connectedReceived.Task, Task.Delay(EVENT_TIMEOUT_MS));
            Assert.That(firstConnectedTask == connectedReceived.Task, Is.True, 
                "Should receive PNConnectedCategory status after first subscribe");

            var handshakeCountAfterFirst = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/v2/subscribe/demo/c1/0") && 
                           HasQueryParameter((WireMock.Logging.LogEntry)e, "tt", "0"))
                .Count();
            
            Assert.That(handshakeCountAfterFirst, Is.EqualTo(1), 
                "Should have exactly 1 handshake request after first subscribe");

            _pubnub.Subscribe<object>().Channels(new[] { "c1" }).Execute();

            var secondStatusTask = await Task.WhenAny(secondStatusReceived.Task, Task.Delay(EVENT_TIMEOUT_MS));
            Assert.That(secondStatusTask == secondStatusReceived.Task, Is.True,
                "Should receive PNSubscriptionChangedCategory status callback after second subscribe to same channel");

            var handshakeCountAfterSecond = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/v2/subscribe/demo/c1/0") && 
                           HasQueryParameter((WireMock.Logging.LogEntry)e, "tt", "0"))
                .Count();

            Assert.That(handshakeCountAfterSecond, Is.EqualTo(1),
                "Should have exactly 1 handshake request - second subscribe to same channel should NOT trigger new handshake");

            Assert.That(statusEvents.Count, Is.GreaterThanOrEqualTo(2),
                "Should have at least 2 status events (one for each subscribe call)");
            
            Assert.That(statusEvents[0].Category, Is.EqualTo(PNStatusCategory.PNConnectedCategory),
                "First status should be PNConnectedCategory (from handshake success)");
            Assert.That(statusEvents[1].Category, Is.EqualTo(PNStatusCategory.PNSubscriptionChangedCategory),
                "Second status should be PNSubscriptionChangedCategory (emitted directly without state transition)");
        }

        /// <summary>
        /// Tests that subscribing to a mix of existing and new channels DOES trigger a state transition.
        /// </summary>
        [Test]
        public async Task ThenSubscribeWithMixOfExistingAndNewChannels_TriggersSubscriptionChange()
        {
            var statusEvents = new System.Collections.Generic.List<PNStatus>();
            var connectedReceived = new TaskCompletionSource<bool>();
            var subscriptionChangedReceived = new TaskCompletionSource<bool>();

            var listener = new SubscribeCallbackExt(
                (pn, msg) => { },
                (pn, presence) => { },
                (pn, status) =>
                {
                    statusEvents.Add(status);
                    
                    if (status.Category == PNStatusCategory.PNConnectedCategory && !connectedReceived.Task.IsCompleted)
                        connectedReceived.TrySetResult(true);
                    else if (status.Category == PNStatusCategory.PNSubscriptionChangedCategory)
                        subscriptionChangedReceived.TrySetResult(true);
                }
            );
            _pubnub.AddListener(listener);

            _pubnub.Subscribe<object>().Channels(new[] { "c1" }).Execute();

            var connectedTask = await Task.WhenAny(connectedReceived.Task, Task.Delay(EVENT_TIMEOUT_MS));
            Assert.That(connectedTask == connectedReceived.Task, Is.True, 
                "Should receive PNConnectedCategory after first subscribe");

            _pubnub.Subscribe<object>().Channels(new[] { "c1", "c2" }).Execute();

            var changedTask = await Task.WhenAny(subscriptionChangedReceived.Task, Task.Delay(EVENT_TIMEOUT_MS));
            Assert.That(changedTask == subscriptionChangedReceived.Task, Is.True,
                "Should receive PNSubscriptionChangedCategory when adding new channel");

            Assert.That(statusEvents[0].Category, Is.EqualTo(PNStatusCategory.PNConnectedCategory),
                "First status should be PNConnectedCategory");

            Assert.That(statusEvents.Any(s => s.Category == PNStatusCategory.PNSubscriptionChangedCategory), Is.True,
                "Should have received PNSubscriptionChangedCategory when adding new channel");
        }

        private void SetupSubscribeMocks()
        {
            // Generic handshake mock (tt=0)
            _mockServer
                .Given(Request.Create()
                    .WithPath(new RegexMatcher(@"/v2/subscribe/demo/.*/0"))
                    .WithParam("tt", "0")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new
                    {
                        t = new { t = "17000000000000001", r = 12 },
                        m = new object[] { }
                    })
                    .WithDelay(TimeSpan.FromMilliseconds(50)));

            // Long-poll mock (tt > 0) - keep connection alive
            _mockServer
                .Given(Request.Create()
                    .WithPath(new RegexMatcher(@"/v2/subscribe/demo/.*/0"))
                    .WithParam("tt", new RegexMatcher(@"^(?!0$).*"))
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new
                    {
                        t = new { t = "17000000000000002", r = 12 },
                        m = new object[] { }
                    })
                    .WithDelay(TimeSpan.FromSeconds(5)));

            // Specific c1 handshake
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1/0")
                    .WithParam("tt", "0")
                    .WithParam("heartbeat", "10")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{""t"":{""t"":""17000000000000001"",""r"":12},""m"":[]}")
                    .WithDelay(TimeSpan.FromMilliseconds(50)));

            // c1 long-poll
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1/0")
                    .WithParam("tt", "17000000000000001")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{""t"":{""t"":""17000000000000002"",""r"":12},""m"":[]}")
                    .WithDelay(TimeSpan.FromSeconds(5)));

            // c1,c2 handshake
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1,c2/0")
                    .WithParam("tt", "0")
                    .WithParam("heartbeat", "10")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{""t"":{""t"":""17000000000000005"",""r"":12},""m"":[]}")
                    .WithDelay(TimeSpan.FromMilliseconds(50)));

            // c1,c2 long-poll
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1,c2/0")
                    .WithParam("tt", "17000000000000005")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{""t"":{""t"":""17000000000000006"",""r"":12},""m"":[]}")
                    .WithDelay(TimeSpan.FromSeconds(5)));
        }

        private void SetupHeartbeatMocks()
        {
            _mockServer
                .Given(Request.Create()
                    .WithPath(new RegexMatcher(@"/v2/presence/sub_key/demo/channel/.*/heartbeat"))
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new
                    {
                        status = 200,
                        message = "OK",
                        service = "Presence"
                    })
                    .WithDelay(TimeSpan.FromMilliseconds(20)));
        }

        private void SetupLeaveMocks()
        {
            _mockServer
                .Given(Request.Create()
                    .WithPath(new RegexMatcher(@"/v2/presence/sub_key/demo/channel/.*/leave"))
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new
                    {
                        status = 200,
                        message = "OK",
                        action = "leave",
                        service = "Presence"
                    })
                    .WithDelay(TimeSpan.FromMilliseconds(20)));
        }

        private bool HasQueryParameter(WireMock.Logging.LogEntry entry, string key, string value)
        {
            if (entry.RequestMessage.Query == null) return false;
            
            foreach (var param in entry.RequestMessage.Query)
            {
                if (param.Key == key && param.Value.Contains(value))
                    return true;
            }
            return false;
        }

        private void VerifyHeartbeatsForSingleChannel(System.Collections.Generic.List<WireMock.Logging.LogEntry> heartbeatRequests)
        {
            var heartbeatC1 = heartbeatRequests
                .Where(r => r.RequestMessage.Path.Contains("/channel/c1/heartbeat"))
                .ToList();
            Assert.That(heartbeatC1.Count, Is.GreaterThan(0),
                "Should have at least one heartbeat request for c1");

            var heartbeatWithDuplicates = heartbeatRequests
                .Where(r => r.RequestMessage.Path.Contains("/channel/c1,c1/heartbeat"))
                .ToList();
            Assert.That(heartbeatWithDuplicates.Count, Is.EqualTo(0),
                "Should have NO heartbeat requests with duplicate channels - SDK deduplicates");
        }
    }
}
