using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.States;
using System.Linq;

namespace PubnubApi.Tests.EventEngine
{
    internal class ReceiveStoppedStateTransition
    {
        private static object[] receiveStoppedEventCases = {
            new object[] {
                new ReceiveStoppedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } },
                new SubscriptionChangedEvent()
                {
                    Channels = new string[] { "ch1", "ch2", "ch3" },
                    ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
                },
                new ReceiveStoppedState(){ Channels = new string[] { "ch1", "ch2", "ch3" }, ChannelGroups = new string[] { "cg1", "cg2", "cg3" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } }
            },
            new object[]
            {
                new ReceiveStoppedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } },
                new SubscriptionRestoredEvent()
                {
                    Channels = new string[] { "ch1", "ch2" },
                    ChannelGroups = new string[] { "cg1", "cg2" },
                    Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
                },
                new ReceiveStoppedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } }
            },
        };

        [TestCase]
        public void ReceiveStoppedState_OnReconnectEvent_TransitionToHandshakingState()
        {
            //Arrange

            var currentState = new ReceiveStoppedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } };
            var eventToTriggerTransition = new ReconnectEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            };
            var expectedState = new HandshakingState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            };

            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<HandshakingState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((HandshakingState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((HandshakingState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.Cursor.Region, ((HandshakingState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((HandshakingState)result.State).Cursor.Timetoken);
        }

        [TestCaseSource(nameof(receiveStoppedEventCases))]
        public void ReceiveStoppedState_OnEvent_TransitionToReceiveStoppedState(
            ReceiveStoppedState receiveStoppedState, IEvent @event, ReceiveStoppedState expectedState)
        {
            //Act
            var result = receiveStoppedState.Transition(@event);
            //Assert
            Assert.IsInstanceOf<ReceiveStoppedState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((ReceiveStoppedState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((ReceiveStoppedState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.Cursor.Region, ((ReceiveStoppedState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((ReceiveStoppedState)result.State).Cursor.Timetoken);
        }

        [Test]
        public void ReceiveStoppedState_OnUnsubscribeAllEvent_TransitionToUnsubscribedState()
        {
            //Arrange
            var currentState = new ReceiveStoppedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } };
            var eventToTriggerTransition = new UnsubscribeAllEvent();
            
            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<UnsubscribedState>(result.State);  
        }

    }
}
