using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.States;
using System.Linq;

namespace PubnubApi.Tests.EventEngine
{
    internal class HandshakeFailedStateTransition
    {
        [Test]
        public void TestHandshakeFailedStateTransitionWithSubscriptionChangedEvent()
        {
            //Arrange
            var handshakeFailedState = new HandshakeFailedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            var handshakingState = new HandshakingState();
            //Act
            var result = handshakeFailedState.Transition(new SubscriptionChangedEvent()
            {
                Channels = new string[] { "ch1", "ch2", "ch3" },
                ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
            } );
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
        public void TestHandshakeFailedStateTransitionWithSubscriptionRestoredEvent()
        {
            //Arrange
            var handshakeFailedState = new HandshakeFailedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            var handshakingState = new HandshakingState();
            //Act
            var result = handshakeFailedState.Transition(new SubscriptionRestoredEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" }
            } );
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakingState.GetType()));
            Assert.AreEqual("ch1", ((HandshakingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(1));
        }

        [Test]
        public void TestHandshakeFailedStateTransitionWithReconnectEvent()
        {
            //Arrange
            var handshakeFailedState = new HandshakeFailedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            var handshakingState = new HandshakingState();
            //Act
            var result = handshakeFailedState.Transition(new ReconnectEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            } );
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakingState.GetType()));
            Assert.AreEqual("ch1", ((HandshakingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((HandshakingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((HandshakingState)(result.State)).Cursor.Timetoken);
        }

        [Test]
        public void TestHandshakeFailedStateTransitionWithUnsubscribeEvent()
        {
            //Arrange
            var handshakeFailedState = new HandshakeFailedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            var unsubscribedState = new UnsubscribedState();
            //Act
            var result = handshakeFailedState.Transition(new UnsubscribeAllEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(unsubscribedState.GetType()));
        }

    }
}
