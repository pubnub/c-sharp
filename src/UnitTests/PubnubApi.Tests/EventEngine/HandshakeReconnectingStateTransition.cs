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
        [Test]
        public void TestHandshakeReconnectingStateTransitionWithSubscriptionChangedEvent() 
        {
            //Arrange
            var handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            var handshakingState = new HandshakingState();
            //Act
            var result = handshakeReconnectingState.Transition(new SubscriptionChangedEvent() 
            { 
                Channels = new string[] { "ch1", "ch2", "ch3" },
                ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakingState.GetType()));
            Assert.AreEqual("ch1", ((HandshakingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("ch3", ((HandshakingState)(result.State)).Channels.ElementAt(2));
            Assert.AreEqual("cg1", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual("cg3", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(2));
        }

        [Test]
        public void TestHandshakeReconnectingStateTransitionWithSubscriptionRestoredEvent() 
        {
            //Arrange
            var handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            var handshakingState = new HandshakingState();
            //Act
            var result = handshakeReconnectingState.Transition(new SubscriptionRestoredEvent() 
            { 
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakingState.GetType()));
            Assert.AreEqual("ch1", ((HandshakingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(1));
        }

        [Test]
        public void TestHandshakeReconnectingStateTransitionWithHandshakeReconnectFailureEvent()
        {
            //Arrange
            var handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            var handshakeReconnectingState2 = new HandshakeReconnectingState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = handshakeReconnectingState.Transition(new HandshakeReconnectFailureEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakeReconnectingState2.GetType()));
            Assert.AreEqual("ch1", ((HandshakeReconnectingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakeReconnectingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakeReconnectingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakeReconnectingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNUnknownCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestHandshakeReconnectingStateTransitionWithDisconnectEvent()
        {
            //Arrange
            var handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            var handshakeStoppedState = new HandshakeStoppedState();
            //Act
            var result = handshakeReconnectingState.Transition(new DisconnectEvent() 
            { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } } );
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakeStoppedState.GetType()));
            Assert.AreEqual("ch1", ((HandshakeStoppedState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakeStoppedState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakeStoppedState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakeStoppedState)(result.State)).ChannelGroups.ElementAt(1));
        }

        [Test]
        public void TestHandshakeReconnectingStateTransitionWithHandshakeReconnectGiveupEvent() 
        {
            //Arrange
            var handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            var handshakeFailedState = new HandshakeFailedState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = handshakeReconnectingState.Transition(new HandshakeReconnectGiveUpEvent() { } );
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakeFailedState.GetType()));
            Assert.AreEqual("ch1", ((HandshakeFailedState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakeFailedState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakeFailedState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakeFailedState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNUnknownCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestHandshakeReconnectingStateTransitionWithHandshakeReconnectSuccessEvent()
        {
            //Arrange
            var handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var receivingState = new ReceivingState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = handshakeReconnectingState.Transition(new HandshakeReconnectSuccessEvent() 
            {
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                Status = new PNStatus(null,PNOperationType.PNSubscribeOperation, PNStatusCategory.PNConnectedCategory, handshakeReconnectingState.Channels, handshakeReconnectingState.ChannelGroups)
            } );
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receivingState.GetType()));
            Assert.AreEqual("ch1", ((ReceivingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceivingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceivingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceivingState)(result.State)).Cursor.Timetoken);
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((ReceivingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((ReceivingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNConnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestHandshakeReconnectingStateTransitionWithUnsubscribeEvent()
        {
            //Arrange
            var handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var unsubscribedState = new UnsubscribedState();
            //Act
            var result = handshakeReconnectingState.Transition(new UnsubscribeAllEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(unsubscribedState.GetType()));
        }

    }
}
