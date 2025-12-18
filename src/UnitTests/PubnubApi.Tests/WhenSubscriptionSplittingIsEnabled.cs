using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;

namespace PubNubMessaging.Tests
{

    [TestFixture]
    public class WhenSubscriptionSplittingIsEnabled : TestHarness
    {
        private static Pubnub pubnub;
        private static string authToken;

        [SetUp]
        public static async Task Init()
        {
            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey
            };

            pubnub = createPubNubInstance(config);

            if (string.IsNullOrEmpty(PubnubCommon.GrantToken))
            {
                await GenerateTestGrantToken(pubnub);
            }

            authToken = PubnubCommon.GrantToken;

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [TearDown]
        public static void Exit()
        {
            if (pubnub != null)
            {
                pubnub.UnsubscribeAll<string>();
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }
        }

        private static void InitPubnubForTest()
        {
            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SplitSubscribeCalls = true
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            pubnub = createPubNubInstance(config, authToken);
        }
        
        [Test]
        public static async Task ThenWithSplitSubscriptionSubscribeToMultipleShouldWork()
        {
            InitPubnubForTest();

            var channels = new string[]
            {
                $"foo.1",
                $"foo.2",
                $"foo.3",
                $"foo.4",
            };
            var channelsWithConnection = new List<string>();
            var connectedToAllReset = new ManualResetEvent(false);

            var eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                },
                delegate (Pubnub pnObj, PNStatus status)
                {
                    if (status.Category == PNStatusCategory.PNSubscriptionChangedCategory ||
                        status.AffectedChannels.Count > 1)
                    {
                        Assert.Fail("Received more than 1 channel in status callback with subscription splitting enabled!");
                    }
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        channelsWithConnection.AddRange(status.AffectedChannels);
                        if (channels.All(x => channelsWithConnection.Contains(x)))
                        {
                            connectedToAllReset.Set();
                        }
                    }
                }
            );
            pubnub.AddListener(eventListener);
            pubnub.Subscribe<string>().Channels(channels).Execute();
            var connectedToAll = connectedToAllReset.WaitOne(25000);
            pubnub.RemoveListener(eventListener);
            Assert.True(connectedToAll, "Split subscription didn't receive connected status for all channels!");
            var subscribedChannels = pubnub.GetSubscribedChannels();
            Assert.True(channels.All(x => subscribedChannels.Contains(x)), "Not all channels were present in subscription list!");
        }
        
        [Test]
        public static async Task ThenWithSplitSubscriptionSubscribeToTenPlusChannelsShouldWork()
        {
            InitPubnubForTest();

            var channels = new string[]
            {
                $"foo.1",
                $"foo.2",
                $"foo.3",
                $"foo.5",
                $"foo.6",
                $"foo.7",
                $"foo.8",
                $"foo.9",
                $"foo.10",
                $"foo.11",
                $"foo.12",
                $"foo.13",
                $"foo.14",
            };
            var channelsWithConnection = new ConcurrentBag<string>();
            var connectedToAllReset = new ManualResetEvent(false);

            var eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                },
                delegate (Pubnub pnObj, PNStatus status)
                {
                    if (status.Category == PNStatusCategory.PNSubscriptionChangedCategory ||
                        status.AffectedChannels.Count > 1)
                    {
                        Assert.Fail("Received more than 1 channel in status callback with subscription splitting enabled!");
                    }
                    if (status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        if (string.IsNullOrEmpty(status.AffectedChannels[0]))
                        {
                            ;
                        }
                        channelsWithConnection.Add(status.AffectedChannels[0]);
                        if (channelsWithConnection.Any(string.IsNullOrEmpty))
                        {
                            ;
                        }
                        if (channels.All(x => channelsWithConnection.Contains(x)))
                        {
                            connectedToAllReset.Set();
                        }
                    }
                }
            );
            pubnub.AddListener(eventListener);
            pubnub.Subscribe<string>().Channels(channels).Execute();
            var connectedToAll = connectedToAllReset.WaitOne(50000);
            pubnub.RemoveListener(eventListener);
            Assert.True(connectedToAll, "Split subscription didn't receive connected status for all channels!");
            var subscribedChannels = pubnub.GetSubscribedChannels();
            Assert.True(channels.All(x => subscribedChannels.Contains(x)), "Not all channels were present in subscription list!");
        }
        
        [Test]
        public static async Task ThenWithSplitSubscriptionUnSubscribeShouldWork()
        {
            InitPubnubForTest();

            var channels = new string[]
            {
                $"foo.1",
                $"foo.2",
                $"foo.3",
                $"foo.4",
            };
            var disconnectReset = new ManualResetEvent(false);

            var eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                }
                ,delegate (Pubnub pnObj, PNStatus status)
                {
                    if (status.AffectedChannels.Count > 1)
                    {
                        Assert.Fail("Received more than 1 channel in status callback with subscription splitting enabled!");
                    }
                    if (status.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        disconnectReset.Set();
                    }
                }
            );
            pubnub.AddListener(eventListener);
            pubnub.Subscribe<string>().Channels(channels).Execute();

            await Task.Delay(5000);
            
            pubnub.Unsubscribe<string>().Channels(new []{"foo.1"}).Execute();
            var receivedDisconnect = disconnectReset.WaitOne(25000);
            pubnub.RemoveListener(eventListener);
            Assert.True(receivedDisconnect, "Split subscription didn't receive disconnection status for channel!");
            Assert.False(pubnub.GetSubscribedChannels().Contains("foo.1"), "Subscribed channels contains disconnected channel!");
        }
        
        [Test]
        public static async Task ThenWithSplitSubscriptionUnSubscribeAllShouldWork()
        {
            InitPubnubForTest();

            var channels = new string[]
            {
                $"foo.1",
                $"foo.2",
                $"foo.3",
                $"foo.4",
            };
            var disconnectReset = new ManualResetEvent(false);

            var eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNObjectEventResult eventResult)
                {
                }
                ,delegate (Pubnub pnObj, PNStatus status)
                {
                    if (status.AffectedChannels.Count > 1)
                    {
                        Assert.Fail("Received more than 1 channel in status callback with subscription splitting enabled!");
                    }
                    if (status.Category == PNStatusCategory.PNDisconnectedCategory)
                    {
                        disconnectReset.Set();
                    }
                }
            );
            pubnub.AddListener(eventListener);
            pubnub.Subscribe<string>().Channels(channels).Execute();

            await Task.Delay(5000);

            pubnub.UnsubscribeAll<string>();
            var receivedDisconnect = disconnectReset.WaitOne(25000);

            pubnub.RemoveListener(eventListener);
            Assert.True(receivedDisconnect, "Split subscription didn't receive disconnection status for channel!");
        }
    }
}