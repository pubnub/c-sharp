using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.States;
using System.Linq;

namespace PubnubApi.Tests.EventEngine
{
    internal class ReceiveFailedStateTransition
    {
        private static object[] receiveFailedEventCases = {
            new object[] {
                new ReceiveFailedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } },
                new SubscriptionChangedEvent()
                {
                    Channels = new string[] { "ch1", "ch2", "ch3" },
                    ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
                },
                new HandshakingState(){ Channels = new string[] { "ch1", "ch2", "ch3" }, ChannelGroups = new string[] { "cg1", "cg2", "cg3" } }
            },
            new object[]
            {
                new ReceiveFailedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } },
                new SubscriptionRestoredEvent()
                {
                    Channels = new string[] { "ch1", "ch2" },
                    ChannelGroups = new string[] { "cg1", "cg2" }
                },
                new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } }
            },
            new object[]
            {
                new ReceiveFailedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } },
                new ReconnectEvent()
                {
                    Channels = new string[] { "ch1", "ch2" },
                    ChannelGroups = new string[] { "cg1", "cg2" },
                    Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
                },
                new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } }
            }
        };

        [TestCaseSource(nameof(receiveFailedEventCases))]
        public void ReceiveFailedState_OnEvent_TransitionToHandshakingState(
            ReceiveFailedState receiveFailedState, IEvent @event, HandshakingState expectedState)
        {
            //Act
            var result = receiveFailedState.Transition(@event);

            //Assert
            Assert.IsInstanceOf<HandshakingState>(result.State);
            Assert.AreEqual(expectedState.Channels, ((HandshakingState)result.State).Channels);
            Assert.AreEqual(expectedState.ChannelGroups, ((HandshakingState)result.State).ChannelGroups);
            if (@event is ReconnectEvent reconnectEvent)
            {
                Assert.AreEqual(reconnectEvent.Cursor, ((HandshakingState)result.State).Cursor);
            }
        }

        [Test]
        public void ReceiveFailedState_OnUnsubscribeAllEvent_TransitionToUnsubscribedState()
        {
            //Arrange
            var receiveFailedState = new ReceiveFailedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } };
            var @event = new UnsubscribeAllEvent() { };

            //Act
            var result = receiveFailedState.Transition(@event);

            //Assert
            Assert.IsInstanceOf<UnsubscribedState>(result.State);
        }

    }
}
