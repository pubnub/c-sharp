using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.States;
using System.Linq;

namespace PubnubApi.Tests.EventEngine
{
    internal class ReceiveReconnectingStateTransition
    {
        private static object[] receiveReconnectingEventCases = {
            new object[] {
                new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } },
                new SubscriptionChangedEvent()
                {
                    Channels = new string[] { "ch1", "ch2", "ch3" },
                    ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
                },
                new ReceivingState(){ Channels = new string[] { "ch1", "ch2", "ch3" }, ChannelGroups = new string[] { "cg1", "cg2", "cg3" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } }
            },
            new object[]
            {
                new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } },
                new SubscriptionRestoredEvent()
                {
                    Channels = new string[] { "ch1", "ch2" },
                    ChannelGroups = new string[] { "cg1", "cg2" },
                    Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
                },
                new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } }
            },
            new object[]
            {
                new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } },
                new ReceiveReconnectSuccessEvent()
                {
                    Channels = new string[] { "ch1", "ch2" },
                    ChannelGroups = new string[] { "cg1", "cg2" },
                    Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                    Status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNReconnectedCategory)
                },
                new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } }
            }

        };
        
        [TestCaseSource(nameof(receiveReconnectingEventCases))]
        public void ReceiveReconnectingState_OnEvent_TransitionToReceivingState(
            ReceiveReconnectingState receiveReconnectingState, IEvent @event, ReceivingState expectedState) 
        {
            //Act
            var result = receiveReconnectingState.Transition(@event);

            //Assert
            Assert.IsInstanceOf<ReceivingState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((ReceivingState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((ReceivingState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.Cursor.Region, ((ReceivingState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((ReceivingState)result.State).Cursor.Timetoken);
            if (@event is ReceiveReconnectSuccessEvent)
            {
                Assert.IsInstanceOf<EmitMessagesInvocation>(result.Invocations.ElementAt(0));
            }
        }

        private ReceiveReconnectingState CreateReceiveReconnectingState()
        {
            return new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 } };
        }

        [Test]
        public void ReceiveReconnectingState_OnReceiveReconnectFailureEvent_TransitionToReceiveReconnectingState()
        {
            //Arrange
            var currentState = CreateReceiveReconnectingState();
            var eventToTriggerTransition = new ReceiveReconnectFailureEvent() { };
            var expectedState = new ReceiveReconnectingState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
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
        public void ReceiveReconnectingState_OnDisconnectEvent_TransitionToReceiveStoppedState()
        {
            //Arrange
            var currentState = CreateReceiveReconnectingState();
            var eventToTriggerTransition = new DisconnectEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            };
            var expectedState = new ReceiveStoppedState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
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
        public void ReceiveReconnectingState_OnReceiveReconnectGiveupEvent_TransitionToReceiveFailedState()
        {
            //Arrange
            var currentState = CreateReceiveReconnectingState();
            var eventToTriggerTransition = new ReceiveReconnectGiveUpEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            };
            var expectedState = new ReceiveFailedState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            };

            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<ReceiveFailedState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((ReceiveFailedState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((ReceiveFailedState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.Cursor.Region, ((ReceiveFailedState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((ReceiveFailedState)result.State).Cursor.Timetoken);
            Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(0));
            Assert.AreEqual(PNStatusCategory.PNUnexpectedDisconnectCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void ReceiveReconnectingState_OnReceiveReconnectGiveupEvent_EmitsUnexpectedDisconnectStatus()
        {
            // The give-up transition always emits a PNUnexpectedDisconnectCategory status,
            // regardless of the category carried on the give-up event's own Status.
            //Arrange
            var currentState = CreateReceiveReconnectingState();
            var eventToTriggerTransition = new ReceiveReconnectGiveUpEvent()
            {
                Status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNUnknownCategory)
            };

            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<ReceiveFailedState>(result.State);
            Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(0));
            Assert.AreEqual(PNStatusCategory.PNUnexpectedDisconnectCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void ReceiveReconnectingState_OnReceiveReconnectGiveupEventWithEmptyEventData_TransitionToReceiveFailedStatePreservingStateData()
        {
            // Regression: the give-up event enqueued by ReceivingReconnectEffectHandler only sets
            // Status; its Channels/ChannelGroups/Cursor are null. The failed state must therefore
            // carry the reconnecting state's own channels/groups/cursor so that a subsequent
            // Reconnect() can restore the subscription and resume from the last timetoken.
            //Arrange
            var currentState = CreateReceiveReconnectingState();
            var eventToTriggerTransition = new ReceiveReconnectGiveUpEvent()
            {
                // Channels, ChannelGroups and Cursor intentionally left null, mirroring the real event.
                Status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory)
            };
            var expectedState = new ReceiveFailedState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            };

            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<ReceiveFailedState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((ReceiveFailedState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((ReceiveFailedState)result.State).ChannelGroups);
            Assert.IsNotNull(((ReceiveFailedState)result.State).Cursor);
            Assert.AreEqual(expectedState.Cursor.Region, ((ReceiveFailedState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((ReceiveFailedState)result.State).Cursor.Timetoken);
        }

        [Test]
        public void ReceiveReconnectingState_OnUnsubscribeAllEvent_TransitionToUnsubscribedState()
        {
            //Arrange
            var currentState = CreateReceiveReconnectingState();
            var eventToTriggerTransition = new UnsubscribeAllEvent();
            
            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<UnsubscribedState>(result.State);
        }

    }
}
