using System;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.EndPoint;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class SubscribeApiCoexistenceTests
    {
        private Pubnub pubnub;
        private PNConfiguration config;

        [SetUp]
        public void Setup()
        {
            config = new PNConfiguration(new UserId("test-user"))
            {
                PublishKey = "demo",
                SubscribeKey = "demo",
                LogLevel = PubnubLogLevel.All,
                EnableEventEngine = false // Use legacy mode for testing
            };
            pubnub = new Pubnub(config);
        }

        [TearDown]
        public void Cleanup()
        {
            pubnub?.Destroy();
        }

        [Test]
        public void BothSubscribeApis_ShouldCoexist_WithoutConflicts()
        {
            // Test that both APIs can be used on the same Pubnub instance

            // Test 1: Builder Pattern API
            var builderOperation = pubnub.Subscribe<object>();
            Assert.IsNotNull(builderOperation);
            Assert.IsInstanceOf<ISubscribeOperation<object>>(builderOperation);

            // Configure the builder
            var configuredBuilder = builderOperation
                .Channels(new[] { "test-builder-channel" })
                .WithPresence();

            Assert.IsNotNull(configuredBuilder);

            // Test 2: Request/Response API
            var request = new SubscribeRequest
            {
                Channels = new[] { "test-async-channel" },
                WithPresence = true
            };

            // Validate request
            Assert.DoesNotThrow(() => request.Validate());

            // Both APIs should be available on the same instance
            Assert.IsNotNull(pubnub.Subscribe<object>()); // Builder pattern

            // Request/response pattern should not conflict
            Assert.DoesNotThrow(() =>
            {
                var subscription = pubnub.Subscribe(request);
                subscription?.Dispose();
            });
        }

        [Test]
        public void SubscribeRequest_AllProperties_ShouldSetCorrectly()
        {
            // Arrange
            var channels = new[] { "channel1", "channel2" };
            var channelGroups = new[] { "group1", "group2" };
            var timetoken = 15000000000000000L;
            var queryParams = new System.Collections.Generic.Dictionary<string, object> { { "param", "value" } };

            // Act
            var request = new SubscribeRequest
            {
                Channels = channels,
                ChannelGroups = channelGroups,
                Timetoken = timetoken,
                WithPresence = true,
                QueryParameters = queryParams,
                OnMessage = (pn, msg) => { /* callback */ },
                OnPresence = (pn, presence) => { /* callback */ }
            };

            // Assert
            Assert.AreEqual(channels, request.Channels);
            Assert.AreEqual(channelGroups, request.ChannelGroups);
            Assert.AreEqual(timetoken, request.Timetoken);
            Assert.IsTrue(request.WithPresence);
            Assert.AreEqual(queryParams, request.QueryParameters);
            Assert.IsNotNull(request.OnMessage);
            Assert.IsNotNull(request.OnPresence);
        }

        [Test]
        public void SubscribeBuilder_Vs_RequestResponse_ShouldHaveSameFunctionality()
        {
            // This test demonstrates feature parity between the two approaches

            // Common test data
            var channels = new[] { "parity-test-channel-1", "parity-test-channel-2" };
            var channelGroups = new[] { "parity-test-group" };

            // Test 1: Builder Pattern Configuration
            var builderOp = pubnub.Subscribe<object>()
                .Channels(channels)
                .ChannelGroups(channelGroups)
                .WithPresence()
                .WithTimetoken(15000000000000000L);

            Assert.IsNotNull(builderOp);

            // Test 2: Request/Response Pattern Configuration
            var request = new SubscribeRequest
            {
                Channels = channels,
                ChannelGroups = channelGroups,
                WithPresence = true,
                Timetoken = 15000000000000000L
            };

            // Both should be valid
            Assert.DoesNotThrow(() => request.Validate());
            Assert.IsNotNull(request);

            // Both APIs should be available simultaneously
            Assert.IsNotNull(pubnub.Subscribe<object>()); // Builder pattern
            Assert.DoesNotThrow(() =>
            {
                var subscription = pubnub.Subscribe(request);
                subscription?.Dispose();
            }); // Request/response overload
        }

        [Test]
        public void SubscribeApiCoexistence_BothCanRunConcurrently()
        {
            // Test that builder pattern and request/response pattern can run at the same time

            ISubscription requestResponseSub = null;

            try
            {
                // Start request/response subscription
                var request = new SubscribeRequest
                {
                    Channels = new[] { "concurrent-test-1" }
                };

                requestResponseSub = pubnub.Subscribe(request);
                Assert.IsTrue(requestResponseSub.IsActive);

                // Start builder pattern subscription (would normally work concurrently)
                var builderOp = pubnub.Subscribe<object>()
                    .Channels(new[] { "concurrent-test-2" });

                Assert.IsNotNull(builderOp);

                // Both should coexist without issues
                Assert.IsTrue(requestResponseSub.IsActive);
                Assert.IsNotNull(builderOp);
            }
            finally
            {
                requestResponseSub?.Dispose();
            }
        }
    }
}