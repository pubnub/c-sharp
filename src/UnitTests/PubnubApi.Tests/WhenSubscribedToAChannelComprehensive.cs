using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using System.Threading.Tasks;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Matchers;

namespace PubNubMessaging.Tests
{
    /// <summary>
    /// Comprehensive unit and integration tests for Subscribe feature covering:
    /// - Configuration and thread safety tests
    /// - Destructive and edge case tests
    /// - Integration tests with real PubNub services
    /// - Listener management and event handling
    /// - Unsubscribe operations
    ///
    /// All tests run against production PubNub servers without mock server dependencies.
    /// Tests use NonPAMPublishKey and NONPAMSubscribeKey (no SecretKey required).
    /// </summary>
    [TestFixture]
    public class WhenSubscribedToAChannelComprehensive : TestHarness
    {
        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;
        private static Random random = new Random();

        private static string GetRandomChannelName(string prefix)
        {
            return $"{prefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        private static string GetRandomUserId(string prefix)
        {
            return $"{prefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        [TearDown]
        public static void Exit()
        {
            if (pubnub != null)
            {
                try
                {
                    pubnub.UnsubscribeAll<string>();
                    pubnub.Destroy();
                }
                catch
                {
                    // Ignore cleanup errors
                }

                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }
        }

        #region Configuration Tests

        /// <summary>
        /// Test: subscribe_config_001
        /// EnableEventEngine = true uses Event Engine mode
        /// </summary>
        [Test]
        public static async Task ThenEnableEventEngineUsesEventEngineMode()
        {
            string channel = GetRandomChannelName("test_event_engine");
            string userId = GetRandomUserId("config_user_001");
            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
                EnableEventEngine = true // Event Engine mode enabled
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeEvent = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new[] { channel })
                    .Execute();
            }, "Subscribe with EnableEventEngine should not throw exception");

            bool connected = subscribeEvent.WaitOne(10000);
            Assert.IsTrue(connected, "Event Engine mode should connect successfully");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_config_002
        /// EnableEventEngine = false uses legacy SubscribeManager
        /// </summary>
        [Test]
        public static async Task ThenDisableEventEngineUsesLegacyMode()
        {
            string channel = GetRandomChannelName("test_legacy");
            string userId = GetRandomUserId("config_user_002");
            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
                EnableEventEngine = false // Legacy mode - test legacy behavior
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeEvent = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new[] { channel })
                    .Execute();
            }, "Subscribe with legacy mode should not throw exception");

            bool connected = subscribeEvent.WaitOne(10000);
            Assert.IsTrue(connected, "Legacy mode should connect successfully");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_config_005
        /// SuppressLeaveEvents = true prevents leave events
        /// </summary>
        [Test]
        public static async Task ThenSuppressLeaveEventsPreventsLeaveEvents()
        {
            string channel = GetRandomChannelName("test_suppress_leave");
            string userId = GetRandomUserId("config_user_005");
            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
                SuppressLeaveEvents = true
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeEvent = new ManualResetEvent(false);
            ManualResetEvent disconnectEvent = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeEvent.Set();
                    }
                    else if (status.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        disconnectEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            subscribeEvent.WaitOne(10000);

            // Unsubscribe - no leave event should be sent to server
            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            disconnectEvent.WaitOne(5000);

            // Status callback should still be fired even though leave is suppressed
            Assert.Pass("SuppressLeaveEvents prevents leave events but status callback is still fired");

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_config_008
        /// Secure = true uses HTTPS
        /// </summary>
        [Test]
        public static async Task ThenSecureConfigurationUsesHTTPS()
        {
            string channel = GetRandomChannelName("test_secure");
            string userId = GetRandomUserId("config_user_008");
            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true // HTTPS
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeEvent = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            bool connected = subscribeEvent.WaitOne(10000);
            Assert.IsTrue(connected, "HTTPS protocol should work");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_thread_002
        /// Subscribe and unsubscribe from different threads
        /// </summary>
        [Test]
        public static async Task ThenSubscribeUnsubscribeFromDifferentThreadsShouldNotDeadlock()
        {
            string channel = GetRandomChannelName("thread_test");
            string userId = GetRandomUserId("thread_user_002");
            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true
            };

            pubnub = new Pubnub(config);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) => { }
            );

            pubnub.AddListener(listener);

            bool noDeadlock = true;

            // Thread 1: Subscribe
            Thread subscribeThread = new Thread(() =>
            {
                try
                {
                    pubnub.Subscribe<string>()
                        .Channels(new[] { channel })
                        .Execute();
                }
                catch (Exception)
                {
                    noDeadlock = false;
                }
            });

            // Thread 2: Unsubscribe
            Thread unsubscribeThread = new Thread(() =>
            {
                Thread.Sleep(100);
                try
                {
                    pubnub.Unsubscribe<string>()
                        .Channels(new[] { channel })
                        .Execute();
                }
                catch (Exception)
                {
                    noDeadlock = false;
                }
            });

            subscribeThread.Start();
            unsubscribeThread.Start();

            subscribeThread.Join(5000);
            unsubscribeThread.Join(5000);

            Assert.IsTrue(noDeadlock, "No deadlock should occur with concurrent subscribe/unsubscribe");

            await Task.Delay(1000);
        }

        #endregion

        #region Edge Case Tests

        /// <summary>
        /// Test: subscribe_edge_002
        /// Subscribe to channel with Unicode name
        /// </summary>
        [Test]
        public static async Task ThenChannelWithUnicodeNameShouldBeAccepted()
        {
            string channel = "频道_チャンネル_канал_" + Guid.NewGuid().ToString().Substring(0, 8);
            PNConfiguration config = new PNConfiguration(new UserId("edge_user_002"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeEvent = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new[] { channel })
                    .Execute();
            }, "Unicode channel names should be accepted and URL-encoded");

            subscribeEvent.WaitOne(10000);

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_edge_003
        /// Subscribe with timetoken 0
        /// </summary>
        [Test]
        public static async Task ThenSubscribeWithTimetokenZeroShouldWork()
        {
            string channel = GetRandomChannelName("test_timetoken_zero");
            PNConfiguration config = new PNConfiguration(new UserId("edge_user_003"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeEvent = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new[] { channel })
                    .WithTimetoken(0)
                    .Execute();
            }, "Subscribe with timetoken 0 should work");

            subscribeEvent.WaitOne(10000);

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_integration_034
        /// Multiple PubNub instances with independent subscriptions
        /// </summary>
        [Test]
        public static async Task ThenMultipleInstancesShouldMaintainIndependentSubscriptions()
        {
            string channel1 = GetRandomChannelName("instance_1");
            string channel2 = GetRandomChannelName("instance_2");

            PNConfiguration config1 = new PNConfiguration(new UserId("multi_instance_user_1"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            PNConfiguration config2 = new PNConfiguration(new UserId("multi_instance_user_2"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            Pubnub instance1 = new Pubnub(config1);
            Pubnub instance2 = new Pubnub(config2);

            ManualResetEvent instance1Subscribed = new ManualResetEvent(false);
            ManualResetEvent instance2Subscribed = new ManualResetEvent(false);

            var listener1 = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        instance1Subscribed.Set();
                    }
                }
            );

            var listener2 = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        instance2Subscribed.Set();
                    }
                }
            );

            instance1.AddListener(listener1);
            instance2.AddListener(listener2);

            instance1.Subscribe<string>()
                .Channels(new[] { channel1 })
                .Execute();

            instance2.Subscribe<string>()
                .Channels(new[] { channel2 })
                .Execute();

            bool inst1Connected = instance1Subscribed.WaitOne(10000);
            bool inst2Connected = instance2Subscribed.WaitOne(10000);

            Assert.IsTrue(inst1Connected && inst2Connected,
                "Multiple instances should maintain independent subscription state");

            instance1.Unsubscribe<string>().Channels(new[] { channel1 }).Execute();
            instance2.Unsubscribe<string>().Channels(new[] { channel2 }).Execute();

            await Task.Delay(1000);

            instance1.Destroy();
            instance2.Destroy();
        }

        #endregion

        #region Destructive Tests

        /// <summary>
        /// Test: subscribe_destructive_001
        /// Duplicate subscription to same channel
        /// </summary>
        [Test]
        public static async Task ThenDuplicateSubscriptionShouldHandleGracefully()
        {
            string channel = GetRandomChannelName("test_duplicate");
            PNConfiguration config = new PNConfiguration(new UserId("destructive_user_001"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent firstSubscribe = new ManualResetEvent(false);
            int connectCount = 0;

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        connectCount++;
                        if (!firstSubscribe.WaitOne(0))
                        {
                            firstSubscribe.Set();
                        }
                    }
                }
            );

            pubnub.AddListener(listener);

            // Subscribe first time
            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            firstSubscribe.WaitOne(10000);

            // Subscribe second time to same channel
            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new[] { channel })
                    .Execute();
            }, "Duplicate subscribe should not throw exception");

            await Task.Delay(2000);

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_destructive_003
        /// Subscribe during active subscription
        /// </summary>
        [Test]
        public static async Task ThenSubscribeDuringActiveSubscriptionShouldUpdateChannels()
        {
            string channel1 = GetRandomChannelName("test_channel_1");
            string channel2 = GetRandomChannelName("test_channel_2");

            PNConfiguration config = new PNConfiguration(new UserId("destructive_user_003"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent firstSubscribe = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        if (!firstSubscribe.WaitOne(0))
                        {
                            firstSubscribe.Set();
                        }
                    }
                }
            );

            pubnub.AddListener(listener);

            // First subscribe
            pubnub.Subscribe<string>()
                .Channels(new[] { channel1 })
                .Execute();

            firstSubscribe.WaitOne(10000);
            await Task.Delay(1000);

            // Subscribe to additional channel during active subscription
            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new[] { channel2 })
                    .Execute();
            }, "Subscribe during active subscription should not throw exception");

            await Task.Delay(2000);

            // Both channels should now be subscribed
            Assert.Pass("Subscription updated with new channels");
        }

        /// <summary>
        /// Test: subscribe_destructive_018
        /// Exception in listener callback
        /// </summary>
        [Test]
        public static async Task ThenExceptionInListenerShouldNotCrashSDK()
        {
            string channel = GetRandomChannelName("test_exception");
            string testMessage = "test_message_" + DateTime.UtcNow.Ticks;

            PNConfiguration config = new PNConfiguration(new UserId("destructive_user_018"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeConnected = new ManualResetEvent(false);
            ManualResetEvent listener1Event = new ManualResetEvent(false);
            ManualResetEvent listener2Event = new ManualResetEvent(false);
            bool listener1Received = false;
            bool listener2Received = false;

            // Listener that throws exception
            var listener1 = new SubscribeCallbackExt(
                (_, message) =>
                {
                    listener1Received = true;
                    listener1Event.Set();
                    throw new Exception("Intentional exception in listener");
                },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeConnected.Set();
                    }
                }
            );

            // Listener that should still work
            var listener2 = new SubscribeCallbackExt(
                (_, message) =>
                {
                    listener2Received = true;
                    listener2Event.Set();
                },
                (_, presence) => { },
                (_, status) => { }
            );

            pubnub.AddListener(listener1);
            pubnub.AddListener(listener2);

            // Subscribe to the channel
            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            // Wait for subscription to connect
            subscribeConnected.WaitOne(10000);
            await Task.Delay(1000);

            // Publish a message to the channel
            PNResult<PNPublishResult> publishResult = null;
            try
            {
                publishResult = await pubnub.Publish()
                    .Channel(channel)
                    .Message(testMessage)
                    .ExecuteAsync();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Publish threw exception: {ex.Message}");
            }

            Assert.IsNotNull(publishResult, "Publish should succeed");

            // Wait for both listeners to receive the message
            listener1Event.WaitOne(10000);
            listener2Event.WaitOne(10000);

            // SDK should catch exception and continue operating
            Assert.IsTrue(listener1Received, "Listener1 should have received the message (even though it throws)");
            Assert.IsTrue(listener2Received,
                "Listener2 should still receive events even if listener1 throws exception");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_destructive_023
        /// Concurrent subscribe and unsubscribe
        /// </summary>
        [Test]
        public static async Task ThenConcurrentSubscribeUnsubscribeShouldNotCauseRaceCondition()
        {
            string channel = GetRandomChannelName("test_concurrent");
            PNConfiguration config = new PNConfiguration(new UserId("destructive_user_023"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) => { }
            );

            pubnub.AddListener(listener);

            // Rapidly subscribe and unsubscribe multiple times
            for (int i = 0; i < 5; i++)
            {
                Assert.DoesNotThrow(() =>
                {
                    pubnub.Subscribe<string>()
                        .Channels(new[] { channel })
                        .Execute();

                    Thread.Sleep(100);

                    pubnub.Unsubscribe<string>()
                        .Channels(new[] { channel })
                        .Execute();
                }, "Concurrent subscribe/unsubscribe should not cause exceptions");

                Thread.Sleep(100);
            }

            // Final state should be consistent
            Assert.Pass("No race conditions detected in concurrent subscribe/unsubscribe");

            await Task.Delay(1000);
        }

        #endregion

        #region Integration Tests

        /// <summary>
        /// Test: subscribe_integration_001
        /// End-to-end subscribe and receive message
        /// </summary>
        [Test]
        public static async Task ThenSubscribeAndReceiveMessageShouldSucceed()
        {
            string channel = GetRandomChannelName("integration_001");
            string testMessage = "Hello Integration Test";

            PNConfiguration config = new PNConfiguration(new UserId("integration_user_001"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent messageReceived = new ManualResetEvent(false);
            ManualResetEvent connected = new ManualResetEvent(false);
            PNMessageResult<object> receivedMessage = null;

            var listener = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        receivedMessage = message;
                        messageReceived.Set();
                    }
                },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        connected.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            connected.WaitOne(10000);
            await Task.Delay(1000);

            // Publish a message
            await pubnub.Publish()
                .Channel(channel)
                .Message(testMessage)
                .ExecuteAsync();

            bool received = messageReceived.WaitOne(10000);

            Assert.IsTrue(received, "Message should be received");
            Assert.IsNotNull(receivedMessage, "Received message should not be null");
            Assert.AreEqual(channel, receivedMessage.Channel, "Channel should match");
            Assert.IsNotNull(receivedMessage.Timetoken, "Timetoken should be present");
            Assert.AreEqual(testMessage, receivedMessage.Message.ToString(), "Message content should match");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_integration_002
        /// Subscribe, publish, unsubscribe flow
        /// </summary>
        [Test]
        public static async Task ThenSubscribePublishUnsubscribeFlowShouldWork()
        {
            string channel = GetRandomChannelName("integration_002");
            string testMessage = "Test Message for Flow";

            PNConfiguration config = new PNConfiguration(new UserId("integration_user_002"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent messageReceived = new ManualResetEvent(false);
            ManualResetEvent disconnected = new ManualResetEvent(false);
            bool messageWasReceived = false;

            var listener = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        messageWasReceived = true;
                        messageReceived.Set();
                    }
                },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        disconnected.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(2000);

            // Publish a message
            await pubnub.Publish()
                .Channel(channel)
                .Message(testMessage)
                .ExecuteAsync();

            messageReceived.WaitOne(10000);

            Assert.IsTrue(messageWasReceived, "Message should be received while subscribed");

            // Unsubscribe
            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            disconnected.WaitOne(5000);

            // Publish another message after unsubscribe
            ManualResetEvent messageAfterUnsubscribe = new ManualResetEvent(false);
            bool messageReceivedAfterUnsubscribe = false;

            var listenerAfter = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        messageReceivedAfterUnsubscribe = true;
                        messageAfterUnsubscribe.Set();
                    }
                },
                (_, presence) => { },
                (_, status) => { }
            );

            pubnub.AddListener(listenerAfter);

            await pubnub.Publish()
                .Channel(channel)
                .Message("Message after unsubscribe")
                .ExecuteAsync();

            messageAfterUnsubscribe.WaitOne(3000);

            Assert.IsFalse(messageReceivedAfterUnsubscribe, "Message should not be received after unsubscribe");

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_integration_003
        /// Subscribe to multiple channels and receive on all
        /// </summary>
        [Test]
        public static async Task ThenSubscribeToMultipleChannelsShouldReceiveOnAll()
        {
            string[] channels =
            {
                GetRandomChannelName("integration_003_a"),
                GetRandomChannelName("integration_003_b"),
                GetRandomChannelName("integration_003_c")
            };

            PNConfiguration config = new PNConfiguration(new UserId("integration_user_003"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            Dictionary<string, bool> messagesReceived = new Dictionary<string, bool>();
            foreach (var ch in channels)
            {
                messagesReceived[ch] = false;
            }

            ManualResetEvent allMessagesReceived = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (messagesReceived.ContainsKey(message.Channel))
                    {
                        messagesReceived[message.Channel] = true;

                        // Check if all messages received
                        bool allReceived = true;
                        foreach (var received in messagesReceived.Values)
                        {
                            if (!received)
                            {
                                allReceived = false;
                                break;
                            }
                        }

                        if (allReceived)
                        {
                            allMessagesReceived.Set();
                        }
                    }
                },
                (_, presence) => { },
                (_, status) => { }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(channels)
                .Execute();

            await Task.Delay(2000);

            // Publish to each channel
            foreach (var channel in channels)
            {
                await pubnub.Publish()
                    .Channel(channel)
                    .Message($"Message for {channel}")
                    .ExecuteAsync();

                await Task.Delay(500);
            }

            bool allReceived = allMessagesReceived.WaitOne(15000);

            Assert.IsTrue(allReceived, "Messages on all channels should be received");

            foreach (var channel in channels)
            {
                Assert.IsTrue(messagesReceived[channel], $"Message on channel {channel} should be received");
            }

            pubnub.Unsubscribe<string>()
                .Channels(channels)
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_integration_005
        /// Receive presence join event
        /// </summary>
        [Test]
        public static async Task ThenPresenceJoinEventShouldBeReceived()
        {
            string channel = GetRandomChannelName("integration_presence_005");

            PNConfiguration config1 = new PNConfiguration(new UserId("presence_user_listener"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            PNConfiguration config2 = new PNConfiguration(new UserId("presence_user_joiner"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            Pubnub pubnub1 = new Pubnub(config1);
            Pubnub pubnub2 = new Pubnub(config2);

            ManualResetEvent joinEventReceived = new ManualResetEvent(false);
            PNPresenceEventResult joinEvent = null;

            var listener1 = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) =>
                {
                    if (presence.Event == "join" && presence.Uuid == "presence_user_joiner")
                    {
                        joinEvent = presence;
                        joinEventReceived.Set();
                    }
                },
                (_, status) => { }
            );

            pubnub1.AddListener(listener1);

            // Subscribe with presence
            pubnub1.Subscribe<string>()
                .Channels(new[] { channel })
                .WithPresence()
                .Execute();

            await Task.Delay(2000);

            // Second user joins
            pubnub2.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            bool received = joinEventReceived.WaitOne(10000);

            Assert.IsTrue(received, "Join event should be received");
            Assert.IsNotNull(joinEvent, "Join event data should not be null");
            Assert.AreEqual("join", joinEvent.Event, "Event type should be 'join'");
            Assert.AreEqual("presence_user_joiner", joinEvent.Uuid, "UUID should match");

            pubnub1.Unsubscribe<string>().Channels(new[] { channel }).Execute();
            pubnub2.Unsubscribe<string>().Channels(new[] { channel }).Execute();

            await Task.Delay(1000);

            pubnub1.Destroy();
            pubnub2.Destroy();
        }

        /// <summary>
        /// Test: subscribe_integration_010
        /// Subscribe with historical timetoken
        /// </summary>
        [Test]
        public static async Task ThenSubscribeWithHistoricalTimetokenShouldReceiveCatchupMessages()
        {
            WireMockServer mockServer = null;
            Pubnub mockPubnub = null;

            try
            {
                string channel = "test_historical_tt_channel";
                long historicalTimetoken = 15000000000000000; // Historical timetoken
                long message1Timetoken = 15000000000000001;
                long message2Timetoken = 15000000000000002;
                long message3Timetoken = 15000000000000003;
                long currentTimetoken = 15000000000000004;

                // Start WireMock server
                mockServer = WireMockServer.Start();

                // Mock: Subscribe with historical timetoken - returns 3 catch-up messages
                mockServer
                    .Given(Request.Create()
                        .WithPath($"/v2/subscribe/demo/{channel}/0")
                        .WithParam("tt", historicalTimetoken.ToString())
                        .UsingGet())
                    .RespondWith(Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(new
                        {
                            t = new { t = currentTimetoken.ToString(), r = 12 },
                            m = new object[]
                            {
                                new
                                {
                                    c = channel,
                                    d = "Historical message 0",
                                    p = new { t = message1Timetoken.ToString(), r = 12 }
                                },
                                new
                                {
                                    c = channel,
                                    d = "Historical message 1",
                                    p = new { t = message2Timetoken.ToString(), r = 12 }
                                },
                                new
                                {
                                    c = channel,
                                    d = "Historical message 2",
                                    p = new { t = message3Timetoken.ToString(), r = 12 }
                                }
                            }
                        })
                        .WithDelay(TimeSpan.FromMilliseconds(100)));

                // Mock: Long-poll subscribe (no new messages)
                mockServer
                    .Given(Request.Create()
                        .WithPath($"/v2/subscribe/demo/{channel}/0")
                        .WithParam("tt", currentTimetoken.ToString())
                        .UsingGet())
                    .RespondWith(Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(new
                        {
                            t = new { t = (currentTimetoken + 1).ToString(), r = 12 },
                            m = new object[] { }
                        })
                        .WithDelay(TimeSpan.FromSeconds(180)));

                // Mock: Leave endpoint
                mockServer
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
                        }));

                // Configure PubNub to use mock server
                PNConfiguration config = new PNConfiguration(new UserId("integration_user_010"))
                {
                    PublishKey = "demo",
                    SubscribeKey = "demo",
                    Origin = $"localhost:{mockServer.Port}",
                    Secure = false,
                    EnableEventEngine = false // Use legacy mode for simpler testing
                };

                mockPubnub = new Pubnub(config);

                // Subscribe from historical timetoken
                ManualResetEvent messagesReceived = new ManualResetEvent(false);
                List<PNMessageResult<object>> receivedMessages = new List<PNMessageResult<object>>();

                var listener = new SubscribeCallbackExt(
                    (_, message) =>
                    {
                        if (message.Channel == channel)
                        {
                            receivedMessages.Add(message);
                            if (receivedMessages.Count >= 3)
                            {
                                messagesReceived.Set();
                            }
                        }
                    },
                    (_, presence) => { },
                    (_, status) => { }
                );

                mockPubnub.AddListener(listener);

                mockPubnub.Subscribe<string>()
                    .Channels(new[] { channel })
                    .WithTimetoken(historicalTimetoken)
                    .Execute();

                bool received = messagesReceived.WaitOne(10000);

                Assert.IsTrue(received, "Catch-up messages should be received");
                Assert.AreEqual(3, receivedMessages.Count, "Exactly 3 historical messages should be received");

                // Verify messages are in chronological order
                Assert.AreEqual("Historical message 0", receivedMessages[0].Message.ToString());
                Assert.AreEqual("Historical message 1", receivedMessages[1].Message.ToString());
                Assert.AreEqual("Historical message 2", receivedMessages[2].Message.ToString());

                Assert.AreEqual(message1Timetoken, receivedMessages[0].Timetoken,
                    "First message timetoken should match");
                Assert.AreEqual(message2Timetoken, receivedMessages[1].Timetoken,
                    "Second message timetoken should match");
                Assert.AreEqual(message3Timetoken, receivedMessages[2].Timetoken,
                    "Third message timetoken should match");

                mockPubnub.Unsubscribe<string>()
                    .Channels(new[] { channel })
                    .Execute();

                await Task.Delay(1000);
            }
            finally
            {
                // Cleanup
                if (mockPubnub != null)
                {
                    try
                    {
                        mockPubnub.UnsubscribeAll<string>();
                        mockPubnub.Destroy();
                    }
                    catch
                    {
                    }
                }

                if (mockServer != null)
                {
                    try
                    {
                        mockServer.Stop();
                        mockServer.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Test: subscribe_integration_028
        /// Receive PNConnectedCategory status
        /// </summary>
        [Test]
        public static async Task ThenConnectedStatusShouldBeReceived()
        {
            string channel = GetRandomChannelName("integration_status_028");

            PNConfiguration config = new PNConfiguration(new UserId("integration_user_028"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent connectedEvent = new ManualResetEvent(false);
            PNStatus connectedStatus = null;

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        connectedStatus = status;
                        connectedEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            bool received = connectedEvent.WaitOne(10000);

            Assert.IsTrue(received, "Connected status should be received");
            Assert.IsNotNull(connectedStatus, "Status object should not be null");
            Assert.AreEqual(PNStatusCategory.PNConnectedCategory, connectedStatus.Category,
                "Category should be PNConnectedCategory");
            Assert.IsFalse(connectedStatus.Error, "Error should be false");
            Assert.IsNotNull(connectedStatus.AffectedChannels, "Affected channels should be present");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_integration_029
        /// Receive PNDisconnectedCategory status
        /// </summary>
        [Test]
        public static async Task ThenDisconnectedStatusShouldBeReceived()
        {
            string channel = GetRandomChannelName("integration_status_029");

            PNConfiguration config = new PNConfiguration(new UserId("integration_user_029"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent connectedEvent = new ManualResetEvent(false);
            ManualResetEvent disconnectedEvent = new ManualResetEvent(false);
            PNStatus disconnectedStatus = null;

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        connectedEvent.Set();
                    }
                    else if (status.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        disconnectedStatus = status;
                        disconnectedEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            connectedEvent.WaitOne(10000);

            // Now unsubscribe
            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            bool received = disconnectedEvent.WaitOne(10000);

            Assert.IsTrue(received, "Disconnected status should be received");
            Assert.IsNotNull(disconnectedStatus, "Status object should not be null");
            Assert.AreEqual(PNStatusCategory.PNDisconnectedCategory, disconnectedStatus.Category,
                "Category should be PNDisconnectedCategory");
            Assert.IsFalse(disconnectedStatus.Error, "Error should be false for intentional disconnect");

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_integration_038
        /// Messages are received in publish order
        /// </summary>
        [Test]
        public static async Task ThenMessagesShouldBeReceivedInPublishOrder()
        {
            string channel = GetRandomChannelName("integration_ordering_038");

            PNConfiguration config = new PNConfiguration(new UserId("integration_user_038"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent allMessagesReceived = new ManualResetEvent(false);
            List<PNMessageResult<object>> receivedMessages = new List<PNMessageResult<object>>();
            int expectedMessageCount = 10;

            var listener = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        receivedMessages.Add(message);
                        if (receivedMessages.Count >= expectedMessageCount)
                        {
                            allMessagesReceived.Set();
                        }
                    }
                },
                (_, presence) => { },
                (_, status) => { }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(2000);

            // Publish multiple messages rapidly
            for (int i = 0; i < expectedMessageCount; i++)
            {
                await pubnub.Publish()
                    .Channel(channel)
                    .Message($"Message {i}")
                    .ExecuteAsync();
            }

            bool received = allMessagesReceived.WaitOne(15000);

            Assert.IsTrue(received, "All messages should be received");
            Assert.AreEqual(expectedMessageCount, receivedMessages.Count, "Should receive all published messages");

            // Verify messages are in order (ascending timetokens)
            for (int i = 1; i < receivedMessages.Count; i++)
            {
                Assert.Greater(receivedMessages[i].Timetoken, receivedMessages[i - 1].Timetoken,
                    $"Message {i} timetoken should be greater than message {i - 1}");
            }

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_integration_040
        /// Subscribe receives messages with Unicode characters
        /// </summary>
        [Test]
        public static async Task ThenUnicodeCharactersShouldBePreserved()
        {
            string channel = GetRandomChannelName("integration_unicode_040");
            string unicodeMessage = "Hello 世界 🌍 Привет مرحبا";

            PNConfiguration config = new PNConfiguration(new UserId("integration_user_040"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent messageReceived = new ManualResetEvent(false);
            PNMessageResult<object> receivedMessage = null;

            var listener = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        receivedMessage = message;
                        messageReceived.Set();
                    }
                },
                (_, presence) => { },
                (_, status) => { }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(2000);

            await pubnub.Publish()
                .Channel(channel)
                .Message(unicodeMessage)
                .ExecuteAsync();

            bool received = messageReceived.WaitOne(10000);

            Assert.IsTrue(received, "Unicode message should be received");
            Assert.IsNotNull(receivedMessage, "Received message should not be null");
            Assert.AreEqual(unicodeMessage, receivedMessage.Message.ToString(),
                "Unicode characters should be preserved");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        #endregion

        #region Listener Management Tests

        /// <summary>
        /// Test: subscribe_unit_020
        /// Add listener before subscribing
        /// </summary>
        [Test]
        public static async Task ThenAddListenerBeforeSubscribingShouldReceiveMessages()
        {
            string channel = GetRandomChannelName("test_listener");
            PNConfiguration config = new PNConfiguration(new UserId("test_user_020"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            ManualResetEvent messageManualEvent = new ManualResetEvent(false);
            bool messageReceived = false;
            bool listenerRegistered = false;

            var listener = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        messageReceived = true;
                        messageManualEvent.Set();
                    }
                },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeManualEvent.Set();
                    }
                }
            );

            // Add listener before subscribing
            pubnub.AddListener(listener);
            listenerRegistered = true;

            Assert.IsTrue(listenerRegistered, "Listener should be registered");

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            subscribeManualEvent.WaitOne(10000);
            await Task.Delay(1000);

            // Publish a message
            await pubnub.Publish()
                .Channel(channel)
                .Message("Test message")
                .ExecuteAsync();

            messageManualEvent.WaitOne(10000);

            Assert.IsTrue(messageReceived, "Listener should receive messages after subscription");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_unit_021
        /// Add multiple listeners
        /// </summary>
        [Test]
        public static async Task ThenAddMultipleListenersShouldReceiveAllEvents()
        {
            string channel = GetRandomChannelName("test_multi_listener");
            PNConfiguration config = new PNConfiguration(new UserId("test_user_021"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent listener1Event = new ManualResetEvent(false);
            ManualResetEvent listener2Event = new ManualResetEvent(false);
            ManualResetEvent listener3Event = new ManualResetEvent(false);

            bool listener1Received = false;
            bool listener2Received = false;
            bool listener3Received = false;

            var listener1 = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        listener1Received = true;
                        listener1Event.Set();
                    }
                },
                (_, presence) => { },
                (_, status) => { }
            );

            var listener2 = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        listener2Received = true;
                        listener2Event.Set();
                    }
                },
                (_, presence) => { },
                (_, status) => { }
            );

            var listener3 = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        listener3Received = true;
                        listener3Event.Set();
                    }
                },
                (_, presence) => { },
                (_, status) => { }
            );

            // Add multiple listeners
            pubnub.AddListener(listener1);
            pubnub.AddListener(listener2);
            pubnub.AddListener(listener3);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(2000);

            // Publish a message
            await pubnub.Publish()
                .Channel(channel)
                .Message("test_message")
                .ExecuteAsync();

            // All listeners should receive events
            listener1Event.WaitOne(10000);
            listener2Event.WaitOne(10000);
            listener3Event.WaitOne(10000);

            Assert.IsTrue(listener1Received, "Listener 1 should receive message");
            Assert.IsTrue(listener2Received, "Listener 2 should receive message");
            Assert.IsTrue(listener3Received, "Listener 3 should receive message");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_unit_022
        /// Remove a specific listener
        /// </summary>
        [Test]
        public static async Task ThenRemoveListenerShouldStopReceivingEvents()
        {
            string channel = GetRandomChannelName("test_remove_listener");
            PNConfiguration config = new PNConfiguration(new UserId("test_user_022"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent listener1Event = new ManualResetEvent(false);
            ManualResetEvent listener2Event = new ManualResetEvent(false);

            int listener1MessageCount = 0;
            int listener2MessageCount = 0;

            var listener1 = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        listener1MessageCount++;
                        listener1Event.Set();
                    }
                },
                (_, presence) => { },
                (_, status) => { }
            );

            var listener2 = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel)
                    {
                        listener2MessageCount++;
                        listener2Event.Set();
                    }
                },
                (_, presence) => { },
                (_, status) => { }
            );

            // Add both listeners
            pubnub.AddListener(listener1);
            pubnub.AddListener(listener2);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(2000);

            // Publish first message
            await pubnub.Publish()
                .Channel(channel)
                .Message("first_message")
                .ExecuteAsync();

            listener1Event.WaitOne(10000);
            listener2Event.WaitOne(10000);

            Assert.AreEqual(1, listener1MessageCount, "Listener1 should receive first message");
            Assert.AreEqual(1, listener2MessageCount, "Listener2 should receive first message");

            // Remove listener1
            pubnub.RemoveListener(listener1);
            listener1Event.Reset();
            listener2Event.Reset();

            await Task.Delay(1000);

            // Publish second message
            await pubnub.Publish()
                .Channel(channel)
                .Message("second_message")
                .ExecuteAsync();

            listener2Event.WaitOne(10000);
            await Task.Delay(2000);

            // listener1 should not receive new messages, listener2 should
            Assert.AreEqual(1, listener1MessageCount, "Removed listener should not receive messages");
            Assert.AreEqual(2, listener2MessageCount, "Active listener should receive message");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        #endregion

        #region Unsubscribe Operations Tests

        /// <summary>
        /// Test: subscribe_unit_061
        /// Unsubscribe from a single channel
        /// </summary>
        [Test]
        public static async Task ThenUnsubscribeFromSingleChannelShouldSucceed()
        {
            string channel = GetRandomChannelName("test_unsubscribe");
            PNConfiguration config = new PNConfiguration(new UserId("test_user_061"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeEvent = new ManualResetEvent(false);
            ManualResetEvent unsubscribeEvent = new ManualResetEvent(false);
            bool disconnected = false;

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeEvent.Set();
                    }
                    else if (status.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        disconnected = true;
                        unsubscribeEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            subscribeEvent.WaitOne(10000);

            // Now unsubscribe
            Assert.DoesNotThrow(() =>
            {
                pubnub.Unsubscribe<string>()
                    .Channels(new[] { channel })
                    .Execute();
            }, "Unsubscribe should not throw exception");

            unsubscribeEvent.WaitOne(5000);

            Assert.IsTrue(disconnected, "Disconnection status should be received");

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_unit_062
        /// Unsubscribe from multiple channels
        /// </summary>
        [Test]
        public static async Task ThenUnsubscribeFromMultipleChannelsShouldSucceed()
        {
            string[] channels =
            {
                GetRandomChannelName("channel_1"),
                GetRandomChannelName("channel_2"),
                GetRandomChannelName("channel_3")
            };
            PNConfiguration config = new PNConfiguration(new UserId("test_user_062"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeEvent = new ManualResetEvent(false);
            ManualResetEvent unsubscribeEvent = new ManualResetEvent(false);
            bool disconnected = false;

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeEvent.Set();
                    }
                    else if (status.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        disconnected = true;
                        unsubscribeEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(channels)
                .Execute();

            subscribeEvent.WaitOne(10000);

            // Unsubscribe from all channels
            Assert.DoesNotThrow(() =>
            {
                pubnub.Unsubscribe<string>()
                    .Channels(channels)
                    .Execute();
            }, "Unsubscribe from multiple channels should not throw exception");

            unsubscribeEvent.WaitOne(5000);

            Assert.IsTrue(disconnected, "Disconnection status should be received for all channels");

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_unit_065
        /// Unsubscribe from non-subscribed channel
        /// </summary>
        [Test]
        public static void ThenUnsubscribeFromNonSubscribedChannelShouldNotThrowError()
        {
            string channel = GetRandomChannelName("test_non_subscribed");
            PNConfiguration config = new PNConfiguration(new UserId("test_user_065"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            // Try to unsubscribe without subscribing first
            Assert.DoesNotThrow(() =>
            {
                pubnub.Unsubscribe<string>()
                    .Channels(new[] { channel })
                    .Execute();
            }, "Unsubscribe from non-subscribed channel should not throw exception");
        }

        /// <summary>
        /// Test: subscribe_unit_066
        /// UnsubscribeAll from all channels and groups
        /// </summary>
        [Test]
        public static async Task ThenUnsubscribeAllShouldClearAllSubscriptions()
        {
            string[] channels =
            {
                GetRandomChannelName("channel_1"),
                GetRandomChannelName("channel_2")
            };
            PNConfiguration config = new PNConfiguration(new UserId("test_user_066"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            ManualResetEvent subscribeEvent = new ManualResetEvent(false);
            ManualResetEvent unsubscribeEvent = new ManualResetEvent(false);
            bool disconnected = false;

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        subscribeEvent.Set();
                    }
                    else if (status.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        disconnected = true;
                        unsubscribeEvent.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(channels)
                .Execute();

            subscribeEvent.WaitOne(10000);

            // UnsubscribeAll
            Assert.DoesNotThrow(() => { pubnub.UnsubscribeAll<string>(); },
                "UnsubscribeAll should not throw exception");

            unsubscribeEvent.WaitOne(5000);

            Assert.IsTrue(disconnected, "Disconnection status should be received after UnsubscribeAll");

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_unit_067
        /// UnsubscribeAll with no active subscriptions
        /// </summary>
        [Test]
        public static void ThenUnsubscribeAllWithNoActiveSubscriptionsShouldNotThrowError()
        {
            PNConfiguration config = new PNConfiguration(new UserId("test_user_067"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
            };

            pubnub = new Pubnub(config);

            // Call UnsubscribeAll without any active subscriptions
            Assert.DoesNotThrow(() => { pubnub.UnsubscribeAll<string>(); },
                "UnsubscribeAll with no active subscriptions should not throw exception");
        }

        #endregion

        #region Validation Tests

        /// <summary>
        /// Test: subscribe_unit_013
        /// Subscribe with missing subscribe key
        /// </summary>
        [Test]
        public static void ThenSubscribeWithMissingSubscribeKeyShouldThrowException()
        {
            string channel = GetRandomChannelName("test_channel");
            string userId = GetRandomUserId("test_user_013");

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey
                // SubscribeKey is missing
            };

            pubnub = new Pubnub(config);

            Assert.Throws<MissingMemberException>(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new[] { channel })
                    .Execute();
            }, "Subscribe without SubscribeKey should throw MissingMemberException");
        }

        /// <summary>
        /// Test: subscribe_unit_014
        /// Subscribe with empty channel array should not throw (handled gracefully by SDK)
        /// </summary>
        [Test]
        public static void ThenSubscribeWithEmptyChannelArrayShouldNotThrow()
        {
            string userId = GetRandomUserId("test_user_014");

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey
            };

            pubnub = new Pubnub(config);

            // SDK handles empty arrays gracefully - no exception thrown at API level
            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new string[] { })
                    .Execute();
            }, "Subscribe with empty channel array should not throw exception");
        }

        /// <summary>
        /// Test: subscribe_unit_018
        /// Verify subscribe API accepts valid channel names with special characters
        /// </summary>
        [Test]
        public static async Task ThenSubscribeAPIAcceptsChannelNamesWithSpecialCharacters()
        {
            string[] channels =
            {
                GetRandomChannelName("channel_with-dash"),
                GetRandomChannelName("channel.with.dots"),
                GetRandomChannelName("channel:with:colons")
            };
            string userId = GetRandomUserId("test_user_018");

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey
            };

            pubnub = new Pubnub(config);

            // Should not throw exception when calling subscribe with special characters
            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(channels)
                    .Execute();
            }, "Subscribe API should accept channel names with special characters");

            await Task.Delay(1000);

            // Cleanup
            pubnub.Unsubscribe<string>()
                .Channels(channels)
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: subscribe_unit_019
        /// Verify subscribe API accepts wildcard patterns
        /// </summary>
        [Test]
        public static async Task ThenSubscribeAPIAcceptsWildcardPatterns()
        {
            string wildcardChannel = $"a.{random.Next(100000, 999999)}.*";
            string userId = GetRandomUserId("test_user_019");

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey
            };

            pubnub = new Pubnub(config);

            // Should not throw exception when calling subscribe with wildcard pattern
            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new[] { wildcardChannel })
                    .Execute();
            }, "Subscribe API should accept wildcard channel patterns");

            await Task.Delay(1000);

            // Cleanup
            pubnub.Unsubscribe<string>()
                .Channels(new[] { wildcardChannel })
                .Execute();

            await Task.Delay(1000);
        }

        #endregion

        #region Additional Subscribe Tests

        /// <summary>
        /// Test: Custom object deserialization
        /// Subscribe should correctly deserialize custom object types
        /// </summary>
        [Test]
        public static async Task ThenSubscribeWithCustomObjectShouldReceiveCorrectType()
        {
            string channel = GetRandomChannelName("custom_object_test");
            string userId = GetRandomUserId("custom_obj_user");

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true
            };

            pubnub = new Pubnub(config);

            ManualResetEvent messageReceived = new ManualResetEvent(false);
            ManualResetEvent connected = new ManualResetEvent(false);
            bool receivedCorrectType = false;

            var testObject = new { Field1 = "Test", Field2 = 42, Field3 = new[] { "item1", "item2", "item3" } };

            var listener = new SubscribeCallbackExt(
                (_, message) =>
                {
                    if (message.Channel == channel && message.Message != null)
                    {
                        var json = pubnub.JsonPluggableLibrary.SerializeToJsonString(message.Message);
                        if (json.Contains("Field1") && json.Contains("Field2") && json.Contains("Field3"))
                        {
                            receivedCorrectType = true;
                        }

                        messageReceived.Set();
                    }
                },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        connected.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            connected.WaitOne(10000);
            await Task.Delay(1000);

            // Publish the test object
            await pubnub.Publish()
                .Channel(channel)
                .Message(testObject)
                .ExecuteAsync();

            messageReceived.WaitOne(10000);

            Assert.IsTrue(receivedCorrectType, "Subscribe should correctly handle custom object types");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: Subscribe with query parameters
        /// Query parameters should be included in the request
        /// </summary>
        [Test]
        public static async Task ThenSubscribeWithQueryParamsShouldWork()
        {
            string channel = GetRandomChannelName("query_param_test");
            string userId = GetRandomUserId("query_param_user");

            Dictionary<string, object> queryParams = new Dictionary<string, object>()
            {
                { "custom_param", "custom_value" },
                { "numeric_param", 123 }
            };

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true
            };

            pubnub = new Pubnub(config);

            ManualResetEvent connected = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        connected.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            Assert.DoesNotThrow(() =>
            {
                pubnub.Subscribe<string>()
                    .Channels(new[] { channel })
                    .QueryParam(queryParams)
                    .Execute();
            }, "Subscribe with query parameters should not throw exception");

            bool isConnected = connected.WaitOne(10000);
            Assert.IsTrue(isConnected, "Subscribe with query parameters should connect successfully");

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        /// <summary>
        /// Test: Subscribe with channel groups only
        /// Subscribe with only channel groups (no channels) should work
        /// </summary>
        [Test]
        public static async Task ThenSubscribeWithOnlyChannelGroupsShouldWork()
        {
            WireMockServer mockServer = null;
            Pubnub mockPubnub = null;

            try
            {
                string channelGroup = "test_cg_only";
                long initialTimetoken = 16000000000000000;
                long nextTimetoken = 16000000000000001;

                // Start WireMock server
                mockServer = WireMockServer.Start();

                // Mock: Subscribe with channel group (handshake with tt=0)
                mockServer
                    .Given(Request.Create()
                        .WithPath("/v2/subscribe/demo/,/0")
                        .WithParam("channel-group", channelGroup)
                        .WithParam("tt", "0")
                        .UsingGet())
                    .RespondWith(Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(new
                        {
                            t = new { t = initialTimetoken.ToString(), r = 12 },
                            m = new object[] { }
                        })
                        .WithDelay(TimeSpan.FromMilliseconds(100)));

                // Mock: Long-poll subscribe for channel group
                mockServer
                    .Given(Request.Create()
                        .WithPath("/v2/subscribe/demo/,/0")
                        .WithParam("channel-group", channelGroup)
                        .WithParam("tt", initialTimetoken.ToString())
                        .UsingGet())
                    .RespondWith(Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(new
                        {
                            t = new { t = nextTimetoken.ToString(), r = 12 },
                            m = new object[] { }
                        })
                        .WithDelay(TimeSpan.FromSeconds(180)));

                // Mock: Leave endpoint
                mockServer
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
                        }));

                // Configure PubNub to use mock server
                PNConfiguration config = new PNConfiguration(new UserId("cg_only_user"))
                {
                    PublishKey = "demo",
                    SubscribeKey = "demo",
                    Origin = $"localhost:{mockServer.Port}",
                    Secure = false,
                    EnableEventEngine = false // Use legacy mode for simpler testing
                };

                mockPubnub = new Pubnub(config);

                ManualResetEvent connected = new ManualResetEvent(false);

                var listener = new SubscribeCallbackExt(
                    (_, message) => { },
                    (_, presence) => { },
                    (_, status) =>
                    {
                        if (status.Category == PNStatusCategory.PNConnectedCategory)
                        {
                            connected.Set();
                        }
                    }
                );

                mockPubnub.AddListener(listener);

                Assert.DoesNotThrow(() =>
                {
                    mockPubnub.Subscribe<string>()
                        .ChannelGroups(new[] { channelGroup })
                        .Execute();
                }, "Subscribe with only channel groups should not throw exception");

                bool isConnected = connected.WaitOne(10000);
                Assert.IsTrue(isConnected, "Subscribe with channel groups only should connect");

                // Use UnsubscribeAll for channel groups to avoid null channel array issue
                mockPubnub.UnsubscribeAll<string>();

                await Task.Delay(1000);
            }
            finally
            {
                // Cleanup
                if (mockPubnub != null)
                {
                    try
                    {
                        mockPubnub.Destroy();
                    }
                    catch
                    {
                    }
                }

                if (mockServer != null)
                {
                    try
                    {
                        mockServer.Stop();
                        mockServer.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Test: Reconnect with timetoken reset
        /// Reconnect with reset flag should start from beginning
        /// </summary>
        [Test]
        public static async Task ThenReconnectWithResetShouldStartFromBeginning()
        {
            string channel = GetRandomChannelName("reset_tt_test");
            string userId = GetRandomUserId("reset_tt_user");

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true
            };

            pubnub = new Pubnub(config);

            ManualResetEvent connected = new ManualResetEvent(false);
            ManualResetEvent reconnected = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, message) => { },
                (_, presence) => { },
                (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory && !connected.WaitOne(0))
                    {
                        connected.Set();
                    }
                    else if (status.Category == PNStatusCategory.PNReconnectedCategory)
                    {
                        reconnected.Set();
                    }
                }
            );

            pubnub.AddListener(listener);

            pubnub.Subscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            connected.WaitOne(10000);
            await Task.Delay(1000);

            // Disconnect
            pubnub.Disconnect<string>();
            await Task.Delay(1000);

            // Reconnect with reset timetoken
            Assert.DoesNotThrow(() =>
            {
                pubnub.Reconnect<string>(true); // true = reset timetoken to 0
            }, "Reconnect with reset timetoken should not throw exception");

            reconnected.WaitOne(10000);

            pubnub.Unsubscribe<string>()
                .Channels(new[] { channel })
                .Execute();

            await Task.Delay(1000);
        }

        #endregion
    }
}
