using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubNubMessaging.Tests
{
    /// <summary>
    /// Event Engine regression tests for UnsubscribeAll() followed by Subscribe(), for both the
    /// Subscribe&lt;T&gt;().Channels() builder and the entity API.
    /// Tests use NonPAMPublishKey and NONPAMSubscribeKey against production PubNub servers.
    /// </summary>
    [TestFixture]
    public class WhenUnsubscribeAllIsCalled : TestHarness
    {
        private const int TimeoutMs = 10000;
        private const int NoDeliveryWaitMs = 5000;

        private Pubnub pubnub;
        private Recorder recorder;

        [SetUp]
        public void Setup()
        {
            if (string.IsNullOrEmpty(PubnubCommon.NonPAMPublishKey) || string.IsNullOrEmpty(PubnubCommon.NONPAMSubscribeKey))
            {
                Assert.Ignore("PN_PUB_KEY / PN_SUB_KEY are not set.");
            }
            recorder = new Recorder();
            var config = new PNConfiguration(new UserId($"unsub_all_{Guid.NewGuid().ToString().Substring(0, 8)}"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Secure = true,
                EnableEventEngine = true,
                LogLevel = PubnubLogLevel.Debug
            };
            pubnub = createPubNubInstance(config);
            pubnub.SetLogger(recorder);
            pubnub.AddListener(new SubscribeCallbackExt(
                (_, message) => recorder.Messages.Enqueue(message.Message?.ToString()),
                (_, presence) => { },
                (_, status) => recorder.Statuses.Enqueue(status.Category)));
        }

        [TearDown]
        public void TearDown()
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

        [Test]
        public async Task ThenSubscribeBuilderShouldReconnectAndReceiveMessages()
        {
            string channel = RandomChannel("unsub_all_builder");

            await SubscribeAndWaitForConnected(() => pubnub.Subscribe<string>().Channels(new[] { channel }).Execute());
            await UnsubscribeAllAndWaitForDisconnected();

            await SubscribeAndWaitForConnected(() => pubnub.Subscribe<string>().Channels(new[] { channel }).Execute());
            Assert.IsTrue(await PublishAndWaitForMessage(channel, "builder-after"), "Message was not delivered after UnsubscribeAll + Subscribe");
        }

        [Test]
        public async Task ThenEntitySubscriptionShouldReconnectAndReceiveMessagesAcrossCycles()
        {
            string channel = RandomChannel("unsub_all_entity");
            var subscription = pubnub.Channel(channel).Subscription();

            await SubscribeAndWaitForConnected(() => subscription.Subscribe<string>());
            for (int cycle = 1; cycle <= 2; cycle++)
            {
                await UnsubscribeAllAndWaitForDisconnected();
                await SubscribeAndWaitForConnected(() => subscription.Subscribe<string>());
                Assert.IsTrue(await PublishAndWaitForMessage(channel, $"entity-after-{cycle}"), $"Message was not delivered after cycle {cycle}");
            }
        }

        [Test]
        public async Task ThenSubscribedChannelsShouldBeEmpty()
        {
            string channel = RandomChannel("unsub_all_channels");

            await SubscribeAndWaitForConnected(() => pubnub.Subscribe<string>().Channels(new[] { channel }).Execute());
            await UnsubscribeAllAndWaitForDisconnected();

            Assert.IsEmpty(pubnub.GetSubscribedChannels(), "GetSubscribedChannels() should be empty after UnsubscribeAll");
            Assert.IsEmpty(pubnub.GetSubscribedChannelGroups(), "GetSubscribedChannelGroups() should be empty after UnsubscribeAll");
        }

        [Test]
        public async Task ThenUnsubscribeAfterCursorResubscribeShouldStopDelivery()
        {
            string channel = RandomChannel("unsub_all_cursor");
            var subscription = pubnub.Channel(channel).Subscription();

            await SubscribeAndWaitForConnected(() => subscription.Subscribe<string>());
            long timetoken = await Publish(channel, "cursor-before");
            Assert.IsTrue(await WaitUntil(() => recorder.Messages.Any(m => m.Contains("cursor-before"))), "Initial message was not delivered");

            await UnsubscribeAllAndWaitForDisconnected();
            await SubscribeAndWaitForConnected(() => subscription.Subscribe<string>(new SubscriptionCursor(timetoken)));

            int statusIndex = recorder.Statuses.Count;
            subscription.Unsubscribe<string>();
            Assert.IsTrue(await WaitUntil(() => recorder.Statuses.Skip(statusIndex).Contains(PNStatusCategory.PNDisconnectedCategory)),
                "Unsubscribe after a cursor resubscribe did not disconnect");

            await Publish(channel, "cursor-after-unsubscribe");
            Assert.IsFalse(await WaitUntil(() => recorder.Messages.Any(m => m.Contains("cursor-after-unsubscribe")), NoDeliveryWaitMs),
                "Message was delivered after Unsubscribe");
        }

        [Test]
        public async Task ThenLeaveShouldBeSentForSubscribedChannels()
        {
            string channel = RandomChannel("unsub_all_leave");

            await SubscribeAndWaitForConnected(() => pubnub.Subscribe<string>().Channels(new[] { channel }).Execute());
            await UnsubscribeAllAndWaitForDisconnected();

            Assert.IsTrue(await WaitUntil(() => recorder.LeaveUrls.Any(url => url.Contains($"/channel/{channel}/leave"))),
                "UnsubscribeAll did not send a leave request for the subscribed channel");
            Assert.IsFalse(recorder.LeaveUrls.Any(url => url.Contains("/channel/,/leave")),
                "UnsubscribeAll sent a leave request without channels");
        }

        private static string RandomChannel(string prefix) => $"{prefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        private async Task SubscribeAndWaitForConnected(Action subscribe)
        {
            int statusIndex = recorder.Statuses.Count;
            subscribe();
            Assert.IsTrue(await WaitUntil(() => recorder.Statuses.Skip(statusIndex).Contains(PNStatusCategory.PNConnectedCategory)),
                "Subscribe did not connect");
        }

        private async Task UnsubscribeAllAndWaitForDisconnected()
        {
            int statusIndex = recorder.Statuses.Count;
            pubnub.UnsubscribeAll<string>();
            Assert.IsTrue(await WaitUntil(() => recorder.Statuses.Skip(statusIndex).Contains(PNStatusCategory.PNDisconnectedCategory)),
                "UnsubscribeAll did not disconnect");
        }

        private async Task<long> Publish(string channel, string text)
        {
            var result = await pubnub.Publish().Channel(channel).Message(text).ExecuteAsync();
            Assert.IsFalse(result.Status.Error, $"Publish of '{text}' failed");
            return result.Result.Timetoken;
        }

        private async Task<bool> PublishAndWaitForMessage(string channel, string text)
        {
            await Publish(channel, text);
            return await WaitUntil(() => recorder.Messages.Any(m => m.Contains(text)));
        }

        private static async Task<bool> WaitUntil(Func<bool> condition, int timeoutMs = TimeoutMs)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                if (condition()) return true;
                await Task.Delay(100);
            }
            return condition();
        }

        private sealed class Recorder : IPubnubLogger
        {
            public ConcurrentQueue<string> Messages { get; } = new ConcurrentQueue<string>();
            public ConcurrentQueue<PNStatusCategory> Statuses { get; } = new ConcurrentQueue<PNStatusCategory>();
            public ConcurrentQueue<string> LeaveUrls { get; } = new ConcurrentQueue<string>();

            public void Trace(string logMessage) => Capture(logMessage);
            public void Debug(string logMessage) => Capture(logMessage);
            public void Info(string logMessage) => Capture(logMessage);
            public void Warn(string logMessage) => Capture(logMessage);
            public void Error(string logMessage) => Capture(logMessage);

            private void Capture(string logMessage)
            {
                if (logMessage != null && logMessage.Contains("Sending HTTP request") && logMessage.Contains("/leave"))
                {
                    LeaveUrls.Enqueue(logMessage);
                }
            }
        }
    }
}
