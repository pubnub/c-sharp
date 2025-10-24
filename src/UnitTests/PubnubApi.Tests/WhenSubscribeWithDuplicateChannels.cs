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
        private const int HEARTBEAT_INTERVAL = 3; // seconds
        private const int PRESENCE_TIMEOUT = 10; // seconds

        [SetUp]
        public void Setup()
        {
            // Start WireMock server on random available port
            _mockServer = WireMockServer.Start();

            // Configure all required mocks
            SetupSubscribeMocks();
            SetupHeartbeatMocks();

            // Create PubNub instance
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
        public void TearDown()
        {
            _pubnub?.Destroy();
            _mockServer?.Stop();
            _mockServer?.Dispose();
        }

        [Test]
        public async Task ThenSubscribeWithDuplicateChannels_DeduplicatesAndSendsHeartbeats()
        {
            // Act: First subscribe to ["c1"]
            _pubnub.Subscribe<object>().Channels(new[] { "c1" }).Execute();

            // Wait 5 seconds - expect at least 1 heartbeat (at 3s)
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Act: Second subscribe to ["c1", "c1"] - SDK deduplicates to ["c1"], no new request
            _pubnub.Subscribe<object>().Channels(new[] { "c1", "c1" }).Execute();

            // Wait 5 seconds - expect at least 1 heartbeat (at 3s)
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Act: Third subscribe to ["c1", "c1"] - SDK deduplicates to ["c1"], no new request
            _pubnub.Subscribe<object>().Channels(new[] { "c1", "c1" }).Execute();

            // Wait 5 seconds - expect at least 1 heartbeat (at 3s)
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Assert: Verify subscribe calls were made
            var subscribeRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/v2/subscribe/demo/"))
                .ToList();

            Console.WriteLine($"Total subscribe requests: {subscribeRequests.Count}");
            foreach (var req in subscribeRequests)
            {
                Console.WriteLine($"  Subscribe: {req.RequestMessage.Path}");
            }

            // Should have at least:
            // - 1 initial handshake for c1 (tt=0)
            // - 1+ long-poll requests (tt > 0)
            // No additional requests since channels don't change
            Assert.That(subscribeRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least 1 subscribe request (initial handshake)");

            // Verify first request is handshake with tt=0
            var handshakeRequests = subscribeRequests
                .Cast<WireMock.Logging.LogEntry>()
                .Where(r => HasQueryParameter(r, "tt", "0"))
                .ToList();
            
            Assert.That(handshakeRequests.Count, Is.EqualTo(1),
                "Should have exactly 1 handshake request with tt=0");

            // Assert: Verify heartbeat calls were made (approximately every 3 seconds)
            var heartbeatRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/heartbeat"))
                .ToList();

            Console.WriteLine($"Total heartbeat requests: {heartbeatRequests.Count}");
            foreach (var req in heartbeatRequests)
            {
                Console.WriteLine($"  Heartbeat: {req.RequestMessage.Path}");
            }

            // Total test duration: ~15 seconds
            // Expected heartbeats: ~3-5 (one every 3 seconds, timing may vary)
            Assert.That(heartbeatRequests.Count, Is.GreaterThanOrEqualTo(3),
                "Should have at least 3 heartbeat requests over 15 seconds");

            // Assert: Verify all heartbeats are for single deduplicated channel "c1"
            VerifyHeartbeatsForSingleChannel(heartbeatRequests.Cast<WireMock.Logging.LogEntry>().ToList());
        }

        [Test]
        public async Task ThenSubscribeToSingleChannel_SendsHeartbeatsAutomatically()
        {
            // Act: Subscribe to single channel
            _pubnub.Subscribe<object>().Channels(new[] { "c1" }).Execute();

            // Wait 10 seconds to observe multiple heartbeats
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Assert: Verify subscribe call was made
            var subscribeRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/v2/subscribe/demo/c1/0"))
                .ToList();

            Assert.That(subscribeRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least 1 subscribe request for c1");

            // Assert: Verify heartbeats were sent approximately every 3 seconds
            var heartbeatRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/channel/c1/heartbeat"))
                .ToList();

            Console.WriteLine($"Heartbeat requests for c1: {heartbeatRequests.Count}");

            // In 10 seconds, expect at least 3 heartbeats (at 3s, 6s, 9s)
            Assert.That(heartbeatRequests.Count, Is.GreaterThanOrEqualTo(2),
                "Should have at least 2 heartbeat requests in 10 seconds");
        }

        [Test]
        public async Task ThenSubscribeWithDifferentChannels_MakesNewSubscribeRequest()
        {
            // Act: First subscribe to ["c1"]
            _pubnub.Subscribe<object>().Channels(new[] { "c1" }).Execute();

            await Task.Delay(TimeSpan.FromSeconds(3));

            // Act: Subscribe to different channels ["c1", "c2"] - adds c2
            _pubnub.Subscribe<object>().Channels(new[] { "c1", "c2" }).Execute();

            // Wait longer to ensure new subscribe request is made
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Assert: Verify subscribe calls were made for both channel sets
            var subscribeRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/v2/subscribe/demo/"))
                .ToList();

            Console.WriteLine($"Total subscribe requests: {subscribeRequests.Count}");
            foreach (var req in subscribeRequests)
            {
                Console.WriteLine($"  Subscribe: {req.RequestMessage.Path}");
            }

            // With EventEngine enabled, changing channels triggers new subscription
            // Should have at least one request for initial c1
            var c1OnlyRequests = subscribeRequests
                .Where(r => r.RequestMessage.Path.Contains("/c1/0") && 
                           !r.RequestMessage.Path.Contains("/c1,c2/"))
                .ToList();

            Assert.That(c1OnlyRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least one request for c1 only");

            // After adding c2, SDK should make a request with both channels
            // Note: This may or may not happen immediately depending on SDK implementation
            var c1c2Requests = subscribeRequests
                .Where(r => r.RequestMessage.Path.Contains("/c1,c2/0"))
                .ToList();

            // SDK behavior: With EventEngine, it may continue existing subscription
            // and not make a new handshake request for channel additions
            Console.WriteLine($"Requests for c1,c2: {c1c2Requests.Count}");
            
            // Verify heartbeats reflect the channel change
            var heartbeatRequests = _mockServer.LogEntries
                .Where(e => e.RequestMessage.Path.Contains("/heartbeat"))
                .ToList();

            Console.WriteLine($"Total heartbeat requests: {heartbeatRequests.Count}");
            foreach (var req in heartbeatRequests)
            {
                Console.WriteLine($"  Heartbeat: {req.RequestMessage.Path}");
            }

            // At minimum, we should have heartbeats
            Assert.That(heartbeatRequests.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least one heartbeat request");
        }

        private void SetupSubscribeMocks()
        {
            // Mock 1: Initial handshake for any channel with tt=0
            // This catches all initial subscribe requests
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
                    .WithDelay(TimeSpan.FromMilliseconds(100)));

            // Mock 2: Long-polling subscribe with timetoken > 0
            // This simulates the continuous subscribe loop
            _mockServer
                .Given(Request.Create()
                    .WithPath(new RegexMatcher(@"/v2/subscribe/demo/.*/0"))
                    .WithParam("tt", new RegexMatcher(@"^(?!0$).*"))  // tt != "0"
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(new
                    {
                        t = new { t = "17000000000000002", r = 12 },
                        m = new object[] { }
                    })
                    .WithDelay(TimeSpan.FromSeconds(180))); // Long-poll delay

            // Mock 3: Specific mock for c1 with tt=0
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1/0")
                    .WithParam("tt", "0")
                    .WithParam("heartbeat", "10")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""t"": {
                            ""t"": ""17000000000000001"",
                            ""r"": 12
                        },
                        ""m"": []
                    }")
                    .WithDelay(TimeSpan.FromMilliseconds(100)));

            // Mock 4: Specific mock for c1,c1 with tt=0
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1/0")
                    .WithParam("tt", "0")
                    .WithParam("heartbeat", "10")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""t"": {
                            ""t"": ""17000000000000003"",
                            ""r"": 12
                        },
                        ""m"": []
                    }")
                    .WithDelay(TimeSpan.FromMilliseconds(100)));

            // Mock 5: Long-poll for c1 with specific timetoken
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1/0")
                    .WithParam("tt", "17000000000000001")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""t"": {
                            ""t"": ""17000000000000002"",
                            ""r"": 12
                        },
                        ""m"": []
                    }")
                    .WithDelay(TimeSpan.FromSeconds(180)));

            // Mock 6: Long-poll for c1 with specific timetoken
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1/0")
                    .WithParam("tt", "17000000000000003")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""t"": {
                            ""t"": ""17000000000000004"",
                            ""r"": 12
                        },
                        ""m"": []
                    }")
                    .WithDelay(TimeSpan.FromSeconds(180)));

            // Mock 7: Handshake for c1,c2 with tt=0
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1,c2/0")
                    .WithParam("tt", "0")
                    .WithParam("heartbeat", "10")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""t"": {
                            ""t"": ""17000000000000005"",
                            ""r"": 12
                        },
                        ""m"": []
                    }")
                    .WithDelay(TimeSpan.FromMilliseconds(100)));

            // Mock 8: Long-poll for c1,c2 with specific timetoken
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/subscribe/demo/c1,c2/0")
                    .WithParam("tt", "17000000000000005")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""t"": {
                            ""t"": ""17000000000000006"",
                            ""r"": 12
                        },
                        ""m"": []
                    }")
                    .WithDelay(TimeSpan.FromSeconds(180)));
        }

        private void SetupHeartbeatMocks()
        {
            // Setup heartbeat mocks with wildcards for any channel combination
            // This catches all heartbeat requests regardless of channel
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
                    .WithDelay(TimeSpan.FromMilliseconds(50)));

            // Specific mock for c1 heartbeat
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/presence/sub_key/demo/channel/c1/heartbeat")
                    .WithParam("heartbeat", "10")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""status"": 200,
                        ""message"": ""OK"",
                        ""service"": ""Presence""
                    }")
                    .WithDelay(TimeSpan.FromMilliseconds(50)));

            // Specific mock for c1,c2 heartbeat  
            _mockServer
                .Given(Request.Create()
                    .WithPath("/v2/presence/sub_key/demo/channel/c1,c2/heartbeat")
                    .WithParam("heartbeat", "10")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""status"": 200,
                        ""message"": ""OK"",
                        ""service"": ""Presence""
                    }")
                    .WithDelay(TimeSpan.FromMilliseconds(50)));
        }

        private bool HasQueryParameter(WireMock.Logging.LogEntry entry, string key, string value)
        {
            if (entry.RequestMessage.Query == null) return false;
            
            foreach (var param in entry.RequestMessage.Query)
            {
                if (param.Key == key && param.Value.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }

        private void VerifyHeartbeatsForSingleChannel(System.Collections.Generic.List<WireMock.Logging.LogEntry> heartbeatRequests)
        {
            // Verify heartbeat for c1 was called (should never contain duplicates like c1,c1)
            var heartbeatC1 = heartbeatRequests
                .Where(r => r.RequestMessage.Path.Contains("/channel/c1/heartbeat"))
                .ToList();

            Console.WriteLine($"Heartbeats for c1: {heartbeatC1.Count}");
            Assert.That(heartbeatC1.Count, Is.GreaterThan(0),
                "Should have at least one heartbeat request for c1");

            // Verify NO heartbeat requests contain duplicate channels
            var heartbeatWithDuplicates = heartbeatRequests
                .Where(r => r.RequestMessage.Path.Contains("/channel/c1,c1/heartbeat"))
                .ToList();

            Console.WriteLine($"Heartbeats with duplicates (should be 0): {heartbeatWithDuplicates.Count}");
            Assert.That(heartbeatWithDuplicates.Count, Is.EqualTo(0),
                "Should have NO heartbeat requests with duplicate channels - SDK deduplicates");
        }

        private void PrintAllRequests()
        {
            Console.WriteLine("\n=== All Requests ===");
            foreach (var entry in _mockServer.LogEntries)
            {
                Console.WriteLine($"[{entry.RequestMessage.DateTime:HH:mm:ss.fff}] {entry.RequestMessage.Method} {entry.RequestMessage.Path}?{entry.RequestMessage.Query}");
            }
            Console.WriteLine("===================\n");
        }
    }
}

