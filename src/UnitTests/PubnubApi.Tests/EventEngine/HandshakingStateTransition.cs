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
    internal class HandshakingStateTransition
    {
        private static object[] handshakingEventCases = {
            new object[] {
                new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) },
                new SubscriptionChangedEvent()
                {
                    Channels = new string[] { "ch1", "ch2", "ch3" },
                    ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
                },
                new HandshakingState(){ Channels = new string[] { "ch1", "ch2", "ch3" }, ChannelGroups = new string[] { "cg1", "cg2", "cg3" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) }
            },
            new object[]
            {
                new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) },
                new SubscriptionRestoredEvent()
                {
                    Channels = new string[] { "ch1", "ch2" },
                    ChannelGroups = new string[] { "cg1", "cg2" },
                    Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
                },
                new HandshakingState(){ Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) }
            }
        };

        [TestCaseSource(nameof(handshakingEventCases))]
        public void HandshakingState_OnEvent_TransitionToHandshakingState(
            HandshakingState handshakingState, IEvent @event, HandshakingState expectedState) 
        {
            //Act
            var result = handshakingState.Transition(@event);

            //Assert
            Assert.IsInstanceOf<HandshakingState>(result.State);
            Assert.AreEqual(expectedState.Channels, ((HandshakingState)result.State).Channels);
            Assert.AreEqual(expectedState.ChannelGroups, ((HandshakingState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.ReconnectionPolicy, ((HandshakingState)result.State).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.MaximumReconnectionRetries, ((HandshakingState)result.State).ReconnectionConfiguration.MaximumReconnectionRetries);
            if (@event is SubscriptionRestoredEvent)
            {
            Assert.AreEqual(expectedState.Cursor.Region, ((HandshakingState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((HandshakingState)result.State).Cursor.Timetoken);
            }
        }

        private HandshakingState CreateHandshakingState()
        {
            return new HandshakingState() 
            { 
                Channels = new string[] { "ch1", "ch2" }, 
                ChannelGroups = new string[] { "cg1", "cg2" }, 
                ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) 
            };
        }

        [Test]
        public void HandshakingState_OnHandshakeFailureEvent_TransitionToHandshakeReconnectingState()
        {
            //Arrange
            var currentState = CreateHandshakingState();
            var eventToTriggerTransition = new HandshakeFailureEvent() { };
            var expectedState = new HandshakeReconnectingState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50)
            };
            
            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<HandshakeReconnectingState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((HandshakeReconnectingState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((HandshakeReconnectingState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.ReconnectionPolicy, ((HandshakeReconnectingState)result.State).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.MaximumReconnectionRetries, ((HandshakeReconnectingState)result.State).ReconnectionConfiguration.MaximumReconnectionRetries);
            Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(0));
            Assert.AreEqual(PNStatusCategory.PNUnknownCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void HandshakingState_OnDisconnectEvent_TransitionToHandshakeStoppedState() 
        {
            //Arrange
            var handshakingState = CreateHandshakingState();
            var eventToTriggerTransition = new DisconnectEvent() 
            { 
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            };
            var expectedState = new HandshakeStoppedState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50)
            };

            //Act
            var result = handshakingState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<HandshakeStoppedState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((HandshakeStoppedState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((HandshakeStoppedState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.ReconnectionPolicy, ((HandshakeStoppedState)result.State).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.MaximumReconnectionRetries, ((HandshakeStoppedState)result.State).ReconnectionConfiguration.MaximumReconnectionRetries);
            Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(0));
            Assert.AreEqual(PNStatusCategory.PNDisconnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void HandshakingState_OnHandshakeSuccessEvent_TransitionToReceivingState()
        {
            //Arrange
            var handshakingState = CreateHandshakingState();
            var eventToTriggerTransition = new HandshakeSuccessEvent() 
            { 
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                Status = new PNStatus(null,PNOperationType.PNSubscribeOperation, PNStatusCategory.PNConnectedCategory, handshakingState.Channels, handshakingState.ChannelGroups)
            };
            var expectedState = new ReceivingState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50)
            };

            //Act
            var result = handshakingState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<ReceivingState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((ReceivingState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((ReceivingState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.ReconnectionPolicy, ((ReceivingState)result.State).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.MaximumReconnectionRetries, ((ReceivingState)result.State).ReconnectionConfiguration.MaximumReconnectionRetries);
            Assert.IsInstanceOf<EmitStatusInvocation>(result.Invocations.ElementAt(0));
            Assert.AreEqual(PNStatusCategory.PNConnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void HandshakingState_OnUnsubscribeEvent_TransitionToUnsubscribedState()
        {
            //Arrange
            var currentState = CreateHandshakingState();
            var eventToTriggerTransition = new UnsubscribeAllEvent();

            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<UnsubscribedState>(result.State);
        }

    }
}
