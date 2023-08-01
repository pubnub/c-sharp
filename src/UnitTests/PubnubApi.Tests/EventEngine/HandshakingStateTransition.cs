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
        [Test]
        public void TestHandshakingStateTransitionWithSubscriptionRestoredEvent()
        {
            //Arrange
            var handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var handshakingState2 = new HandshakingState();
            //Act
            var result = handshakingState.Transition(new SubscriptionRestoredEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakingState2.GetType()));
            Assert.AreEqual("ch1", ((HandshakingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((HandshakingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((HandshakingState)(result.State)).Cursor.Timetoken);
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((HandshakingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((HandshakingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
        }

        [Test]
        public void TestHandshakingStateTransitionWithWithSubscriptionChangedEvent()
        {
            //Arrange
            var handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var handshakingState2 = new HandshakingState();
            //Act
            var result = handshakingState.Transition(new SubscriptionChangedEvent()
            {
                Channels = new string[] { "ch1", "ch2", "ch3" },
                ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakingState2.GetType()));
            Assert.AreEqual("ch1", ((HandshakingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("ch3", ((HandshakingState)(result.State)).Channels.ElementAt(2));
            Assert.AreEqual("cg1", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual("cg3", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(2));
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((HandshakingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((HandshakingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
        }

        [Test]
        public void TestHandshakingStateTransitionWithHandshakeFailureEvent()
        {
            //Arrange
            var handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var handshakeReconnectingState = new HandshakeReconnectingState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = handshakingState.Transition(new HandshakeFailureEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakeReconnectingState.GetType()));
            Assert.AreEqual("ch1", ((HandshakeReconnectingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakeReconnectingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakeReconnectingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakeReconnectingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((HandshakeReconnectingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((HandshakeReconnectingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNUnknownCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestHandshakingStateTransitionWithDisconnectEvent() 
        {
            //Arrange
            var handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var handshakeStoppedState = new HandshakeStoppedState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = handshakingState.Transition(new DisconnectEvent() 
            { 
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakeStoppedState.GetType()));
            Assert.AreEqual("ch1", ((HandshakeStoppedState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakeStoppedState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakeStoppedState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakeStoppedState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((HandshakeStoppedState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((HandshakeStoppedState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNDisconnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestHandshakingStateTransitionWithHandshakeSuccessEvent()
        {
            //Arrange
            var handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var receivingState = new ReceivingState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = handshakingState.Transition(new HandshakeSuccessEvent() 
            { 
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                Status = new PNStatus(null,PNOperationType.PNSubscribeOperation, PNStatusCategory.PNConnectedCategory, handshakingState.Channels, handshakingState.ChannelGroups)
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receivingState.GetType()));
            Assert.AreEqual("ch1", ((ReceivingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceivingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((ReceivingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((ReceivingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNConnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestHandshakingStateTransitionWithUnsubscribeEvent()
        {
            //Arrange
            var handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var unsubscribedState = new UnsubscribedState();
            //Act
            var result = handshakingState.Transition(new UnsubscribeAllEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(unsubscribedState.GetType()));
        }

    }
}
