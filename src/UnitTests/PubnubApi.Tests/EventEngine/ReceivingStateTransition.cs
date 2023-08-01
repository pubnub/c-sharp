using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.States;
using System.Linq;

namespace PubnubApi.Tests.EventEngine
{
    internal class ReceivingStateTransition
    {
        [Test]
        public void TestReceivingStateTransitionWithSubscriptionChangedEvent()
        {
            //Arrange
            State receivingState = new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State receivingState2 = new ReceivingState();
            //Act
            TransitionResult result = receivingState.Transition(new SubscriptionChangedEvent()
            {
                Channels = new string[] { "ch1", "ch2", "ch3" },
                ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receivingState2.GetType()));
            Assert.AreEqual("ch1", ((ReceivingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceivingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("ch3", ((ReceivingState)(result.State)).Channels.ElementAt(2));
            Assert.AreEqual("cg1", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual("cg3", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(2));
            Assert.AreEqual(1, ((ReceivingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceivingState)(result.State)).Cursor.Timetoken);
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((ReceivingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((ReceivingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
        }

        [Test]
        public void TestReceivingStateTransitionWithSubscriptionRestoredEvent()
        {
            //Arrange
            State receivingState = new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State receivingState2 = new ReceivingState();
            //Act
            TransitionResult result = receivingState.Transition(new SubscriptionChangedEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receivingState2.GetType()));
            Assert.AreEqual("ch1", ((ReceivingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceivingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceivingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceivingState)(result.State)).Cursor.Timetoken);
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((ReceivingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((ReceivingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
        }

        [Test]
        public void TestReceivingStateTransitionWithDisconnectEvent()
        {
            //Arrange
            State receivingState = new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State receiveStoppedState = new ReceiveStoppedState();
            //Act
            TransitionResult result = receivingState.Transition(new DisconnectEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receiveStoppedState.GetType()));
            Assert.AreEqual("ch1", ((ReceiveStoppedState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceiveStoppedState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceiveStoppedState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceiveStoppedState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceiveStoppedState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceiveStoppedState)(result.State)).Cursor.Timetoken);
        }

        [Test]
        public void TestReceivingStateTransitionWithReceiveFailureEvent()
        {
            //Arrange
            State receivingState = new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State receiveReconnectingState = new ReceiveReconnectingState();
            //Act
            TransitionResult result = receivingState.Transition(new ReceiveFailureEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receiveReconnectingState.GetType()));
            Assert.AreEqual("ch1", ((ReceiveReconnectingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceiveReconnectingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceiveReconnectingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceiveReconnectingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceiveReconnectingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceiveReconnectingState)(result.State)).Cursor.Timetoken);
        }

        [Test]
        public void TestReceivingStateTransitionWithReceiveSuccessEvent()
        {
            //Arrange
            State receivingState = new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State receivingState2 = new ReceivingState();
            //Act
            TransitionResult result = receivingState.Transition(new ReceiveSuccessEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receivingState2.GetType()));
            Assert.AreEqual("ch1", ((ReceivingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceivingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceivingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceivingState)(result.State)).Cursor.Timetoken);
        }

        [Test]
        public void TestReceivingStateTransitionWithUnsubscribeEvent()
        {
            //Arrange
            State receivingState = new ReceivingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State unsubscribedState = new UnsubscribedState();
            //Act
            TransitionResult result = receivingState.Transition(new UnsubscribeAllEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(unsubscribedState.GetType()));
        }

    }
}
