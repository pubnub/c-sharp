using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.States;
using System.Linq;

namespace PubnubApi.Tests.EventEngine
{
    internal class ReceivingStateTransition
    {
        private static object[] receivingEventCases = {
            new object[] {
                new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } },
                new SubscriptionChangedEvent()
                {
                    Channels = new string[] { "ch1", "ch2", "ch3" },
                    ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
                },
                new ReceivingState(){ Channels = new string[] { "ch1", "ch2", "ch3" }, ChannelGroups = new string[] { "cg1", "cg2", "cg3" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } }
            },
            new object[]
            {
                new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } },
                new SubscriptionRestoredEvent()
                {
                    Channels = new string[] { "ch1", "ch2" },
                    ChannelGroups = new string[] { "cg1", "cg2" },
                    Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
                },
                new ReceivingState(){ Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } }
            },
            new object[]
            {
                new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } },
                new ReceiveSuccessEvent()
                {
                    Channels = new string[] { "ch1", "ch2" },
                    ChannelGroups = new string[] { "cg1", "cg2" },
                    Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                    Status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNConnectedCategory),
                    Messages = new ReceivingResponse<string>() {  Messages = new Message<string>[]{ }, Timetoken = new Timetoken(){ Region = 1, Timestamp = 1234567890 } }
                },
                new ReceivingState(){ Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } }
            }

        };

        [TestCaseSource(nameof(receivingEventCases))]
        public void ReceivingState_OnEvent_TransitionToReceivingState(
            ReceivingState receivingState, IEvent @event, ReceivingState expectedState) 
        {
            //Act
            var result = receivingState.Transition(@event);

            //Assert
            Assert.IsInstanceOf<ReceivingState>(result.State);
            Assert.AreEqual(expectedState.Channels, ((ReceivingState)result.State).Channels);
            Assert.AreEqual(expectedState.ChannelGroups, ((ReceivingState)result.State).ChannelGroups);
            if (@event is SubscriptionRestoredEvent || @event is ReceiveSuccessEvent)
            {
            Assert.AreEqual(expectedState.Cursor.Region, ((ReceivingState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((ReceivingState)result.State).Cursor.Timetoken);
            }
            if (@event is ReceiveSuccessEvent)
            {
                Assert.IsInstanceOf<EmitMessagesInvocation>(result.Invocations.ElementAt(0));
                Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(1));
                Assert.AreEqual(PNStatusCategory.PNConnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(1)).StatusCategory);
            }
        }

        private ReceivingState CreateReceivingState()
        {
            return new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } };
        }

        [Test]
        public void ReceivingState_OnDisconnectEvent_TransitionToReceiveStoppedState()
        {
            //Arrange
            var currentState = CreateReceivingState();
            var eventToTriggerTransition = new DisconnectEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" }
            };
            var expectedState = new ReceiveStoppedState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
            };

            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<ReceiveStoppedState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((ReceiveStoppedState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((ReceiveStoppedState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.Cursor.Region, ((ReceiveStoppedState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((ReceiveStoppedState)result.State).Cursor.Timetoken);
            Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(0));
            Assert.AreEqual(PNStatusCategory.PNDisconnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void ReceivingState_OnReceiveFailureEvent_TransitionToReceiveReconnectingState()
        {
            //Arrange
            var currentState = CreateReceivingState();
            var eventToTriggerTransition = new ReceiveFailureEvent() { };
            var expectedState = new ReceiveReconnectingState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
            };

            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<ReceiveReconnectingState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((ReceiveReconnectingState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((ReceiveReconnectingState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.Cursor.Region, ((ReceiveReconnectingState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((ReceiveReconnectingState)result.State).Cursor.Timetoken);
        }

        [Test]
        public void ReceivingState_OnUnsubscribeAllEvent_TransitionToUnsubscribedState()
        {
            //Arrange
            var currentState = CreateReceivingState();
            var eventToTriggerTransition = new UnsubscribeAllEvent();

            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<UnsubscribedState>(result.State);
        }

    }
}
