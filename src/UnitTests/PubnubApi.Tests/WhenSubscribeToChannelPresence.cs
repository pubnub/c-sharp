using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribeToChannelPresence : TestHarness
    {
        static int manualResetEventWaitTimeout = 310 * 1000;
        private static Pubnub pubnub;

        [SetUp]
        public static void Init()
        {
            pubnub = null;
        }

        [TearDown]
        public static void Exit()
        {
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub = null;
            }
        }
        
        [Test]
        public static async Task ThenJoinEventReceived()
        {
            int randomNumber = new Random().Next(1000, 10000);
            string userId = $"testUser{randomNumber}";
            bool receivedJoinEvent = false;
            string randomChannel = $"testChannel{randomNumber}";

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            ManualResetEvent joinEvent = new ManualResetEvent(false);
            var listener = new SubscribeCallbackExt(
                (pn, messageEvent) => { }
                , (pn, presenceEvent) =>
                {
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    }
                }
                , (_, status) =>
                {
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    }
                }
            );
            pubnub = new Pubnub(config);
            pubnub.AddListener(listener);
            manualResetEventWaitTimeout = 310 * 1000;
            pubnub.Subscribe<string>().Channels(new[] { randomChannel }).WithPresence().Execute();
            joinEvent.WaitOne(manualResetEventWaitTimeout);
            await Task.Delay(2000).ConfigureAwait(false);
            pubnub.RemoveListener(listener);
            pubnub.UnsubscribeAll<string>();
            pubnub.Destroy();
            pubnub = null;
            Assert.IsTrue(receivedJoinEvent, "WhenSubscribedToAChannel, Then JoinEvent should be received");

        }

        [Test]
        public static async Task ThenJoinEventReceivedForSubsequentSubscribe()
        {
            int randomNumber = new Random().Next(1000, 99999);
            string userId = $"testUser{randomNumber}";
            bool receivedJoinEvent = false;
            bool receivedSecondJoinEvent = false;
            string channel = $"testChannel{randomNumber}";
            string channel2 = $"testChannel{randomNumber}_2";

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };

            ManualResetEvent joinEvent = new ManualResetEvent(false);
            ManualResetEvent secondJoinEvent = new ManualResetEvent(false);
            var listener = new SubscribeCallbackExt(
                (pn, messageEvent) => { }
                , (pn, presenceEvent) =>
                {
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId &&
                        presenceEvent.Channel == channel)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    }

                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId &&
                        presenceEvent.Channel == channel2)
                    {
                        secondJoinEvent.Set();
                        receivedSecondJoinEvent = true;
                    }

                }
                , (_, status) => { }
            );
            pubnub = createPubNubInstance(config);
            pubnub.AddListener(listener);
            manualResetEventWaitTimeout = 310 * 1000;
            pubnub.Subscribe<string>().Channels(new[] { channel }).WithPresence().Execute();
            joinEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.Subscribe<string>().Channels(new[] { channel2 }).WithPresence().Execute();
            secondJoinEvent.WaitOne(manualResetEventWaitTimeout);
            await Task.Delay(2000).ConfigureAwait(false);
            pubnub.RemoveListener(listener);
            pubnub.UnsubscribeAll<string>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedJoinEvent && receivedSecondJoinEvent,
                "WhenSubscribedToAChannel, Then JoinEvent should be received for subsequent Subscribe");
        }

        [Test]
        public static async Task ThenJoinEventReceivedForSubsequentSubscribeWithPresenceEventEngine()
        {
            int randomNumber = new Random().Next(1000, 10000);
            string userId = $"testUser{randomNumber}";
            bool receivedJoinEvent = false;
            bool receivedSecondJoinEvent = false;
            string channel = $"testChannel{randomNumber}";
            string channel2 = $"testChannel{randomNumber}_3";

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                PresenceTimeout = 310
            };

            ManualResetEvent joinEvent = new ManualResetEvent(false);
            ManualResetEvent secondJoinEvent = new ManualResetEvent(false);
            var listener = new SubscribeCallbackExt(
                (pn, messageEvent) => { }
                , (pn, presenceEvent) =>
                {
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId &&
                        presenceEvent.Channel == channel)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    }

                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId &&
                        presenceEvent.Channel == channel2)
                    {
                        secondJoinEvent.Set();
                        receivedSecondJoinEvent = true;
                    }
                }
                , (_, status) => { }
            );
            pubnub = new Pubnub(config);
            pubnub.AddListener(listener);
            manualResetEventWaitTimeout = 310 * 1000;
            pubnub.Subscribe<string>().Channels(new[] { channel }).WithPresence().Execute();
            joinEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.Subscribe<string>().Channels(new[] { channel2 }).WithPresence().Execute();
            secondJoinEvent.WaitOne(manualResetEventWaitTimeout);
            await Task.Delay(2000).ConfigureAwait(false);
            pubnub.RemoveListener(listener);
            pubnub.UnsubscribeAll<string>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedJoinEvent && receivedSecondJoinEvent,
                "WhenSubscribedToAChannel, Then JoinEvent should be received when presence timeout is set");
        }

        [Test]
        public static async Task ThenJoinEventReceivedForSubsequentSubscribeWithZeroInterval()
        {
            int randomNumber = new Random().Next(1000, 10000);
            string userId = $"testUser{randomNumber}";
            bool receivedJoinEvent = false;
            bool receivedSecondJoinEvent = false;
            string channel = $"testChannel{randomNumber}";
            string channel2 = $"testChannel{randomNumber}_4";

            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };
            config.SetPresenceTimeoutWithCustomInterval(300, 0);
            ManualResetEvent joinEvent = new ManualResetEvent(false);
            ManualResetEvent secondJoinEvent = new ManualResetEvent(false);
            var listener = new SubscribeCallbackExt(
                (pn, messageEvent) => { }
                , (pn, presenceEvent) =>
                {
                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId &&
                        presenceEvent.Channel == channel)
                    {
                        joinEvent.Set();
                        receivedJoinEvent = true;
                    }

                    if (presenceEvent.Event == "join" && presenceEvent.Uuid == userId &&
                        presenceEvent.Channel == channel2)
                    {
                        secondJoinEvent.Set();
                        receivedSecondJoinEvent = true;
                    }

                }
                , (_, status) => { }
            );
            pubnub = new Pubnub(config);
            pubnub.AddListener(listener);
            manualResetEventWaitTimeout = 310 * 1000;
            pubnub.Subscribe<string>().Channels(new[] { channel }).WithPresence().Execute();
            joinEvent.WaitOne(manualResetEventWaitTimeout);
            pubnub.Subscribe<string>().Channels(new[] { channel2 }).WithPresence().Execute();
            secondJoinEvent.WaitOne(manualResetEventWaitTimeout);
            await Task.Delay(2000).ConfigureAwait(false);
            pubnub.RemoveListener(listener);
            pubnub.UnsubscribeAll<string>();
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedJoinEvent && receivedSecondJoinEvent,
                "WhenSubscribedToAChannel, Then JoinEvent should be received when presenceTimeout is set to 0");
        }
    }
}