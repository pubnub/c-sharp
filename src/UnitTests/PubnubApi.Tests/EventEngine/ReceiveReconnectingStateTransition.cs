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
    internal class ReceiveReconnectingStateTransition
    {
        [Test]
        public void TestReceiveReconnectingStateTransitionWithSubscriptionChangedEvent()
        {
            //Arrange
            var receiveReconnectingState = new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var receivingState = new ReceivingState();
            //Act
            var result = receiveReconnectingState.Transition(new SubscriptionChangedEvent()
            {
                Channels = new string[] { "ch1", "ch2", "ch3" },
                ChannelGroups = new string[] { "cg1", "cg2", "cg3" }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receivingState.GetType()));
            Assert.AreEqual("ch1", ((ReceivingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceivingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("ch3", ((ReceivingState)(result.State)).Channels.ElementAt(2));
            Assert.AreEqual("cg1", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual("cg3", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(2));
            Assert.AreEqual(1, ((ReceivingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceivingState)(result.State)).Cursor.Timetoken);
        }

        [Test]
        public void TestReceiveReconnectingStateTransitionWithSubscriptionRestoredEvent()
        {
            //Arrange
            var receiveReconnectingState = new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var receivingState = new ReceivingState();
            //Act
            var result = receiveReconnectingState.Transition(new SubscriptionRestoredEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receivingState.GetType()));
            Assert.AreEqual("ch1", ((ReceivingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceivingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceivingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceivingState)(result.State)).Cursor.Timetoken);
        }

        [Test]
        public void TestReceiveReconnectingStateTransitionWithReceiveReconnectFailureEvent()
        {
            //Arrange
            var receiveReconnectingState = new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var receiveReconnectingState2 = new ReceiveReconnectingState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = receiveReconnectingState.Transition(new ReceiveReconnectFailureEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receiveReconnectingState2.GetType()));
            Assert.AreEqual("ch1", ((ReceiveReconnectingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceiveReconnectingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceiveReconnectingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceiveReconnectingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceiveReconnectingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceiveReconnectingState)(result.State)).Cursor.Timetoken);
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNUnknownCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestReceiveReconnectingStateTransitionWithReceiveReconnectSuccessEvent()
        {
            //Arrange
            var receiveReconnectingState = new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var receivingState = new ReceivingState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = receiveReconnectingState.Transition(new ReceiveReconnectSuccessEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
                Status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNReconnectedCategory, receiveReconnectingState.Channels, receiveReconnectingState.ChannelGroups)
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receivingState.GetType()));
            Assert.AreEqual("ch1", ((ReceivingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceivingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceivingState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceivingState)(result.State)).Cursor.Timetoken);
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNReconnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestReceiveReconnectingStateTransitionWithDisconnectEvent()
        {
            //Arrange
            var receiveReconnectingState = new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var receiveStoppedState = new ReceiveStoppedState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = receiveReconnectingState.Transition(new DisconnectEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receiveStoppedState.GetType()));
            Assert.AreEqual("ch1", ((ReceiveStoppedState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceiveStoppedState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceiveStoppedState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceiveStoppedState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceiveStoppedState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceiveStoppedState)(result.State)).Cursor.Timetoken);
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNDisconnectedCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestReceiveReconnectingStateTransitionWithReceiveReconnectGiveup()
        {
            //Arrange
            var receiveReconnectingState = new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var receiveFailedState = new ReceiveFailedState();
            var emitStatusInvocation = new EmitStatusInvocation(new PNStatus());
            //Act
            var result = receiveReconnectingState.Transition(new ReceiveReconnectGiveUpEvent()
            {
                Channels = new string[] { "ch1", "ch2" },
                ChannelGroups = new string[] { "cg1", "cg2" },
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receiveFailedState.GetType()));
            Assert.AreEqual("ch1", ((ReceiveFailedState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceiveFailedState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceiveFailedState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceiveFailedState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(1, ((ReceiveFailedState)(result.State)).Cursor.Region);
            Assert.AreEqual(1234567890, ((ReceiveFailedState)(result.State)).Cursor.Timetoken);
            Assert.IsTrue(result.Invocations.ElementAt(0).GetType().Equals(emitStatusInvocation.GetType()));
            Assert.AreEqual(PNStatusCategory.PNUnknownCategory, ((EmitStatusInvocation)result.Invocations.ElementAt(0)).StatusCategory);
        }

        [Test]
        public void TestReceiveReconnectingStateTransitionWithUnsubscribeEvent()
        {
            //Arrange
            var receiveReconnectingState = new ReceiveReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            var unsubscribedState = new UnsubscribedState();
            //Act
            var result = receiveReconnectingState.Transition(new UnsubscribeAllEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(unsubscribedState.GetType()));
        }

    }
}
