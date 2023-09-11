using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.States;
using System.Linq;

namespace PubnubApi.Tests.EventEngine
{
    internal class HandshakeReconnectingStateTransition
    {
        private static object[] handshakeReconnectingEventCases = {
            new object[] {
                new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } },
                new SubscriptionChangedEvent()
                {
                    Channels = new string[] { "ch1", "ch2", "ch3" },
                    ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
                },
                new HandshakingState(){ Channels = new string[] { "ch1", "ch2", "ch3" }, ChannelGroups = new string[] { "cg1", "cg2", "cg3" } }
            },
            new object[]
            {
                new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } },
                new SubscriptionRestoredEvent()
                {
                    Channels = new string[] { "ch1", "ch2" },
                    ChannelGroups = new string[] { "cg1", "cg2" }
                },
                new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } }
            }
        };

        [TestCaseSource(nameof(handshakeReconnectingEventCases))]
        public void HandshakeReconnectingState_OnEvent_TransitionToHandshakingState(
            HandshakeReconnectingState handshakeReconnectingState, IEvent @event, HandshakingState expectedState) 
        {
            //Act
            var result = handshakeReconnectingState.Transition(@event);

            //Assert
            Assert.IsInstanceOf<HandshakingState>(result.State);
            Assert.AreEqual(expectedState.Channels, ((HandshakingState)result.State).Channels);
            Assert.AreEqual(expectedState.ChannelGroups, ((HandshakingState)result.State).ChannelGroups);
        }

        private HandshakeReconnectingState CreateHandshakeReconnectingState()
        {
            return new HandshakeReconnectingState() 
            { 
                Channels = new string[] { "ch1", "ch2" }, 
                ChannelGroups = new string[] { "cg1", "cg2" } ,
                ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50)
            };
        }

        [Test]
        public void HandshakeReconnectingState_OnHandshakeReconnectFailureEvent_TransitionToHandshakeReconnectingState()
        {
            //Arrange
            var currentState = CreateHandshakeReconnectingState();
            var eventToTriggerTransition = new HandshakeReconnectFailureEvent();
            
            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<HandshakeReconnectingState>(result.State);
            CollectionAssert.AreEqual(currentState.Channels, ((HandshakeReconnectingState)result.State).Channels);
            CollectionAssert.AreEqual(currentState.ChannelGroups, ((HandshakeReconnectingState)result.State).ChannelGroups);

            Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(0));
            Assert.AreEqual(PNStatusCategory.PNUnknownCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void HandshakeReconnectingState_OnDisconnectEvent_TransitionToHandshakeStoppedState()
        {
            //Arrange
            var currentState = CreateHandshakeReconnectingState();
            var eventToTriggerTransition = new DisconnectEvent() 
            { 
                Channels = new string[] { "ch1", "ch2" }, 
                ChannelGroups = new string[] { "cg1", "cg2" } 
            };
            var expectedState = new HandshakeStoppedState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" }
            };

            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<HandshakeStoppedState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((HandshakeStoppedState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((HandshakeStoppedState)result.State).ChannelGroups);
        }

        [Test]
        public void HandshakeReconnectingState_OnHandshakeReconnectGiveupEvent_TransitionToHandshakeFailedState() 
        {
            //Arrange
            var currentState = CreateHandshakeReconnectingState();
            var eventToTriggerTransition = new HandshakeReconnectGiveUpEvent() { };
            var expectedState = new HandshakeFailedState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" }
            };

            //Act
            var result = currentState.Transition(@eventToTriggerTransition);
            
            //Assert
            Assert.IsInstanceOf<HandshakeFailedState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((HandshakeFailedState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((HandshakeFailedState)result.State).ChannelGroups);

            Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(0));
            Assert.AreEqual(PNStatusCategory.PNUnknownCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void HandshakeReconnectingState_OnHandshakeReconnectSuccessEvent_TransitionToReceivingState()
        {
            //Arrange
            var currentState = CreateHandshakeReconnectingState();
            var eventToTriggerTransition = new HandshakeReconnectSuccessEvent()
            {
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                Status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNConnectedCategory, currentState.Channels, currentState.ChannelGroups)
            };
            var expectedState = new ReceivingState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50)
            };

            //Act
            var result = currentState.Transition(@eventToTriggerTransition);
            
            //Assert
            Assert.IsInstanceOf<ReceivingState>(result.State);

            CollectionAssert.AreEqual(expectedState.Channels, ((ReceivingState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((ReceivingState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.Cursor.Region, ((ReceivingState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((ReceivingState)result.State).Cursor.Timetoken);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.ReconnectionPolicy, ((ReceivingState)result.State).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.MaximumReconnectionRetries, ((ReceivingState)result.State).ReconnectionConfiguration.MaximumReconnectionRetries);
            Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(0));
            Assert.AreEqual(PNStatusCategory.PNConnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void HandshakeReconnectingState_OnUnsubscribeAllEvent_TransitionToUnsubscribedState()
        {
            // Arrange
            var currentState = CreateHandshakeReconnectingState();
            var eventToTriggerTransition = new UnsubscribeAllEvent();

            // Act
            var result = currentState.Transition(eventToTriggerTransition);

            // Assert
            Assert.IsInstanceOf<UnsubscribedState>(result.State);        
        }

    }
}
