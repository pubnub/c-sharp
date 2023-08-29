using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Context;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.States;
using System.Linq;

namespace PubnubApi.Tests.EventEngine
{
    internal class UnsubscribedStateTransition
    {
        [Test]
        public void UnsubscribedState_OnSubscriptionChangedEvent_TransitionToHandshakingState()
        {
            //Arrange
            var currentState = new UnsubscribedState() { Channels = new string[] { "ch1", "ch2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };   
            var eventToTriggerTransition = new SubscriptionChangedEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" }
            };
            var expectedState = new HandshakingState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50)
            };

            //Act
            var result = currentState.Transition(eventToTriggerTransition);
            
            //Assert
            Assert.IsInstanceOf<HandshakingState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((HandshakingState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((HandshakingState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.ReconnectionPolicy, ((HandshakingState)result.State).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.MaximumReconnectionRetries, ((HandshakingState)result.State).ReconnectionConfiguration.MaximumReconnectionRetries);
        }
        
        [Test]
        public void UnsubscribedState_OnSubscriptionRestoreEvent_TransitionToReceivingState()
        {
            //Arrange
            var currentState = new UnsubscribedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var eventToTriggerTransition = new SubscriptionRestoredEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            };
            var expectedState = new ReceivingState()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50)
            };
            
            //Act
            var result = currentState.Transition(eventToTriggerTransition);

            //Assert
            Assert.IsInstanceOf<ReceivingState>(result.State);
            CollectionAssert.AreEqual(expectedState.Channels, ((ReceivingState)result.State).Channels);
            CollectionAssert.AreEqual(expectedState.ChannelGroups, ((ReceivingState)result.State).ChannelGroups);
            Assert.AreEqual(expectedState.Cursor.Region, ((ReceivingState)result.State).Cursor.Region);
            Assert.AreEqual(expectedState.Cursor.Timetoken, ((ReceivingState)result.State).Cursor.Timetoken);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.ReconnectionPolicy, ((ReceivingState)result.State).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(expectedState.ReconnectionConfiguration.MaximumReconnectionRetries, ((ReceivingState)result.State).ReconnectionConfiguration.MaximumReconnectionRetries);
        }
    }
}
