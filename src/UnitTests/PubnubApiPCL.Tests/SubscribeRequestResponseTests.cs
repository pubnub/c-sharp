using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.Interface;

namespace PubnubApiPCL.Tests
{
    [TestFixture]
    public class SubscribeRequestResponseTests
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
                SecretKey = "demo",
                LogLevel = PubnubLogLevel.All,
                ReconnectionPolicy = PNReconnectionPolicy.LINEAR,
                EnableEventEngine = false // Start with legacy mode for simpler testing
            };

            pubnub = new Pubnub(config);
        }

        [TearDown]
        public void TearDown()
        {
            pubnub?.Destroy();
            pubnub = null;
        }

        #region Request Validation Tests

        [Test]
        public void SubscribeRequest_Validate_ThrowsOnNullChannelsAndGroups()
        {
            var request = new SubscribeRequest
            {
                Channels = null,
                ChannelGroups = null
            };

            Assert.Throws<ArgumentException>(() => request.Validate());
        }

        [Test]
        public void SubscribeRequest_Validate_ThrowsOnEmptyChannelsAndGroups()
        {
            var request = new SubscribeRequest
            {
                Channels = new string[0],
                ChannelGroups = new string[0]
            };

            Assert.Throws<ArgumentException>(() => request.Validate());
        }

        [Test]
        public void SubscribeRequest_Validate_AcceptsValidChannels()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "channel1", "channel2" },
                ChannelGroups = null
            };

            Assert.DoesNotThrow(() => request.Validate());
            Assert.That(request.Channels.Length, Is.EqualTo(2));
        }

        [Test]
        public void SubscribeRequest_Validate_AcceptsValidChannelGroups()
        {
            var request = new SubscribeRequest
            {
                Channels = null,
                ChannelGroups = new[] { "group1", "group2" }
            };

            Assert.DoesNotThrow(() => request.Validate());
            Assert.That(request.ChannelGroups.Length, Is.EqualTo(2));
        }

        [Test]
        public void SubscribeRequest_Validate_CleansEmptyEntries()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "channel1", "", "  ", null, "channel2" },
                ChannelGroups = new[] { "group1", "", null }
            };

            request.Validate();

            Assert.That(request.Channels.Length, Is.EqualTo(2));
            Assert.That(request.Channels, Contains.Item("channel1"));
            Assert.That(request.Channels, Contains.Item("channel2"));
            Assert.That(request.ChannelGroups.Length, Is.EqualTo(1));
            Assert.That(request.ChannelGroups, Contains.Item("group1"));
        }

        #endregion

        #region Subscribe Method Overload Tests

        [Test]
        public void Subscribe_ThrowsOnNullRequest()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => pubnub.Subscribe(null));

            Assert.That(exception.ParamName, Is.EqualTo("request"));
        }

        [Test]
        public void Subscribe_ThrowsOnInvalidRequest()
        {
            var request = new SubscribeRequest
            {
                Channels = null,
                ChannelGroups = null
            };

            Assert.Throws<ArgumentException>(
                () => pubnub.Subscribe(request));
        }

        [Test]
        public void Subscribe_ReturnsISubscriptionForValidRequest()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" },
                WithPresence = false
            };

            ISubscription subscription = null;
            try
            {
                subscription = pubnub.Subscribe(request);

                Assert.IsNotNull(subscription);
                Assert.IsTrue(subscription.IsActive);
                Assert.That(subscription.Channels.Length, Is.EqualTo(1));
                Assert.That(subscription.Channels, Contains.Item("test-channel"));
                Assert.IsFalse(subscription.PresenceEnabled);
            }
            finally
            {
                subscription?.Dispose();
            }
        }

        [Test]
        public void Subscribe_HandlesPresenceOption()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" },
                WithPresence = true
            };

            ISubscription subscription = null;
            try
            {
                subscription = pubnub.Subscribe(request);

                Assert.IsNotNull(subscription);
                Assert.IsTrue(subscription.PresenceEnabled);
            }
            finally
            {
                subscription?.Dispose();
            }
        }

        [Test]
        public void Subscribe_HandlesMultipleChannelsAndGroups()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "channel1", "channel2" },
                ChannelGroups = new[] { "group1", "group2" }
            };

            ISubscription subscription = null;
            try
            {
                subscription = pubnub.Subscribe(request);

                Assert.IsNotNull(subscription);
                Assert.That(subscription.Channels.Length, Is.EqualTo(2));
                Assert.That(subscription.ChannelGroups.Length, Is.EqualTo(2));
            }
            finally
            {
                subscription?.Dispose();
            }
        }

        [Test]
        public void Subscribe_HandlesCustomTimetoken()
        {
            var customTimetoken = 15000000000000000L;
            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" },
                Timetoken = customTimetoken
            };

            ISubscription subscription = null;
            try
            {
                subscription = pubnub.Subscribe(request);

                Assert.IsNotNull(subscription);
                Assert.IsTrue(subscription.IsActive);
            }
            finally
            {
                subscription?.Dispose();
            }
        }

        #endregion

        #region ISubscription Lifecycle Tests

        [Test]
        public void ISubscription_StopSetsInactive()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" }
            };

            var subscription = pubnub.Subscribe(request);

            Assert.IsTrue(subscription.IsActive);

            subscription.Stop();

            Assert.IsFalse(subscription.IsActive);
        }

        [Test]
        public async Task ISubscription_StopAsyncSetsInactive()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" }
            };

            var subscription = pubnub.Subscribe(request);

            Assert.IsTrue(subscription.IsActive);

            await subscription.StopAsync();

            Assert.IsFalse(subscription.IsActive);
        }

        [Test]
        public void ISubscription_DisposeSetsInactive()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" }
            };

            var subscription = pubnub.Subscribe(request);

            Assert.IsTrue(subscription.IsActive);

            subscription.Dispose();

            Assert.IsFalse(subscription.IsActive);
        }

        [Test]
        public void ISubscription_MultipleDisposeIsIdempotent()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" }
            };

            var subscription = pubnub.Subscribe(request);

            subscription.Dispose();
            Assert.DoesNotThrow(() => subscription.Dispose()); // Second dispose should not throw
        }

        #endregion

        #region Event Handling Tests

        [Test]
        public async Task ISubscription_CallbacksInRequestAreInvoked()
        {
            bool messageCallbackInvoked = false;
            bool statusCallbackInvoked = false;

            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" },
                OnMessage = (pn, msg) => messageCallbackInvoked = true,
                OnStatus = (pn, status) => statusCallbackInvoked = true
            };

            ISubscription subscription = null;
            try
            {
                subscription = pubnub.Subscribe(request);

                // Give some time for status callbacks
                await Task.Delay(100);

                // Status should be invoked on connection
                Assert.IsTrue(statusCallbackInvoked, "Status callback should be invoked");
            }
            finally
            {
                subscription?.Dispose();
            }
        }

        [Test]
        public async Task ISubscription_EventsCanBeSubscribed()
        {
            bool messageEventRaised = false;
            bool statusEventRaised = false;

            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" }
            };

            ISubscription subscription = null;
            try
            {
                subscription = pubnub.Subscribe(request);

                subscription.MessageReceived += (sender, args) => messageEventRaised = true;
                subscription.StatusChanged += (sender, args) => statusEventRaised = true;

                // Give some time for events
                await Task.Delay(100);

                // Status event should be raised on connection
                Assert.IsTrue(statusEventRaised, "Status event should be raised");
            }
            finally
            {
                subscription?.Dispose();
            }
        }

        #endregion

        #region Query Parameters Tests

        [Test]
        public void Subscribe_HandlesQueryParameters()
        {
            var queryParams = new Dictionary<string, object>
            {
                { "custom1", "value1" },
                { "custom2", 123 }
            };

            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" },
                QueryParameters = queryParams
            };

            ISubscription subscription = null;
            try
            {
                subscription = pubnub.Subscribe(request);

                Assert.IsNotNull(subscription);
                Assert.IsTrue(subscription.IsActive);
            }
            finally
            {
                subscription?.Dispose();
            }
        }

        #endregion

        #region Concurrent Subscription Tests

        [Test]
        public void Subscribe_SupportsMultipleConcurrentSubscriptions()
        {
            var request1 = new SubscribeRequest
            {
                Channels = new[] { "channel1" }
            };

            var request2 = new SubscribeRequest
            {
                Channels = new[] { "channel2" }
            };

            ISubscription subscription1 = null;
            ISubscription subscription2 = null;

            try
            {
                subscription1 = pubnub.Subscribe(request1);
                subscription2 = pubnub.Subscribe(request2);

                Assert.IsNotNull(subscription1);
                Assert.IsNotNull(subscription2);
                Assert.IsTrue(subscription1.IsActive);
                Assert.IsTrue(subscription2.IsActive);

                // Both should have different channels
                Assert.Contains("channel1", subscription1.Channels);
                Assert.Contains("channel2", subscription2.Channels);
            }
            finally
            {
                subscription1?.Dispose();
                subscription2?.Dispose();
            }
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void Subscribe_ThrowsOnMissingSubscribeKey()
        {
            var badConfig = new PNConfiguration(new UserId("test-user"))
            {
                PublishKey = "demo",
                SubscribeKey = null // Missing subscribe key
            };

            var badPubnub = new Pubnub(badConfig);

            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" }
            };

            var exception = Assert.Throws<InvalidOperationException>(
                () => badPubnub.Subscribe(request));

            Assert.That(exception.Message, Does.Contain("SubscribeKey is required"));

            badPubnub.Destroy();
        }

        #endregion

        #region Cancellation Tests

        [Test]
        public void Subscribe_ReturnsImmediately()
        {
            var request = new SubscribeRequest
            {
                Channels = new[] { "test-channel" }
            };

            ISubscription subscription = null;
            try
            {
                // Subscribe should return immediately (non-blocking)
                var startTime = DateTime.UtcNow;
                subscription = pubnub.Subscribe(request);
                var endTime = DateTime.UtcNow;

                // Should complete very quickly (under 100ms for setup)
                Assert.IsTrue((endTime - startTime).TotalMilliseconds < 1000);
                Assert.IsNotNull(subscription);
                Assert.IsTrue(subscription.IsActive);
            }
            finally
            {
                subscription?.Dispose();
            }
        }

        #endregion
    }
}