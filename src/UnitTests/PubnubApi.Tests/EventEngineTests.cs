using NUnit.Framework;
using PubnubApi;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.States;
using System.Linq;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class EventEngineTests
    {
        [SetUp]
        public void Init()
        {
            // Method intentionally left empty.
        }

        //Create unit tests for each state transition and event 
        //to make sure that the transition is correct
        [Test]
        public void TestUnsubscribedStateTransitionWithSubscriptionChangedEvent()
        {
            //Arrange
            State unsubscribeState = new UnsubscribedState() { Channels = new string[] { "ch1", "ch2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };   
            State handshakingState = new HandshakingState();
            //Act
            TransitionResult result = unsubscribeState.Transition(new SubscriptionChangedEvent()
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
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((HandshakingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((HandshakingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
        }
        
        [Test]
        public void TestUnsubscribedStateTransitionWithSubscriptionRestoreEvent()
        {
            //Arrange
            State unsubscribeState = new UnsubscribedState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State receivingState = new ReceivingState();
            //Act
            TransitionResult result = unsubscribeState.Transition(new SubscriptionRestoredEvent()
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
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((ReceivingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((ReceivingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
        }
        
        [Test]
        public void TestHandshakingStateTransitionWithSubscriptionRestoredEvent()
        {
            //Arrange
            State handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State handshakingState2 = new HandshakingState();
            //Act
            TransitionResult result = handshakingState.Transition(new SubscriptionRestoredEvent()
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
            State handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State handshakingState2 = new HandshakingState();
            //Act
            TransitionResult result = handshakingState.Transition(new SubscriptionChangedEvent()
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
            State handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State handshakeReconnectingState = new HandshakeReconnectingState();
            //Act
            TransitionResult result = handshakingState.Transition(new HandshakeFailureEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakeReconnectingState.GetType()));
            Assert.AreEqual("ch1", ((HandshakeReconnectingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakeReconnectingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakeReconnectingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakeReconnectingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((HandshakeReconnectingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((HandshakeReconnectingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
        }

        [Test]
        public void TestHandshakingStateTransitionWithDisconnectEvent() 
        {
            //Arrange
            State handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State handshakeStoppedState = new HandshakeStoppedState();
            //Act
            TransitionResult result = handshakingState.Transition(new DisconnectEvent() 
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
        }

        [Test]
        public void TestHandshakingStateTransitionWithHandshakeSuccessEvent()
        {
            //Arrange
            State handshakingState = new HandshakingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State receivingState = new ReceivingState();
            //Act
            TransitionResult result = handshakingState.Transition(new HandshakeSuccessEvent() 
            { 
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 }
            });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(receivingState.GetType()));
            Assert.AreEqual("ch1", ((ReceivingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((ReceivingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((ReceivingState)(result.State)).ChannelGroups.ElementAt(1));
            Assert.AreEqual(PNReconnectionPolicy.LINEAR, ((ReceivingState)(result.State)).ReconnectionConfiguration.ReconnectionPolicy);
            Assert.AreEqual(50, ((ReceivingState)(result.State)).ReconnectionConfiguration.MaximumReconnectionRetries);
        }

        [Test]
        public void TestHandshakeReconnectingStateTransitionWithSubscriptionChangedEvent() 
        {
            //Arrange
            State handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            State handshakingState = new HandshakingState();
            //Act
            TransitionResult result = handshakeReconnectingState.Transition(new SubscriptionChangedEvent() 
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
            State handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            State handshakingState = new HandshakingState();
            //Act
            TransitionResult result = handshakeReconnectingState.Transition(new SubscriptionRestoredEvent() 
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
            State handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            State handshakeReconnectingState2 = new HandshakeReconnectingState();
            //Act
            TransitionResult result = handshakeReconnectingState.Transition(new HandshakeReconnectFailureEvent() { });
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakeReconnectingState2.GetType()));
            Assert.AreEqual("ch1", ((HandshakeReconnectingState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakeReconnectingState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakeReconnectingState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakeReconnectingState)(result.State)).ChannelGroups.ElementAt(1));
        }

        [Test]
        public void TestHandshakeReconnectingStateTransitionWithDisconnectEvent()
        {
            //Arrange
            State handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            State handshakeStoppedState = new HandshakeStoppedState();
            //Act
            TransitionResult result = handshakeReconnectingState.Transition(new DisconnectEvent() 
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
            State handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" } };
            State handshakeFailedState = new HandshakeFailedState();
            //Act
            TransitionResult result = handshakeReconnectingState.Transition(new HandshakeReconnectGiveUpEvent() { } );
            //Assert
            Assert.IsTrue(result.State.GetType().Equals(handshakeFailedState.GetType()));
            Assert.AreEqual("ch1", ((HandshakeFailedState)(result.State)).Channels.ElementAt(0));
            Assert.AreEqual("ch2", ((HandshakeFailedState)(result.State)).Channels.ElementAt(1));
            Assert.AreEqual("cg1", ((HandshakeFailedState)(result.State)).ChannelGroups.ElementAt(0));
            Assert.AreEqual("cg2", ((HandshakeFailedState)(result.State)).ChannelGroups.ElementAt(1));
        }

        [Test]
        public void TestHandshakeReconnectingStateTransitionWithHandshakeReconnectSuccessEvent()
        {
            //Arrange
            State handshakeReconnectingState = new HandshakeReconnectingState() { Channels = new string[] { "ch1", "ch2" }, ChannelGroups = new string[] { "cg1", "cg2" }, ReconnectionConfiguration = new ReconnectionConfiguration(PNReconnectionPolicy.LINEAR, 50) };
            State receivingState = new ReceivingState();
            //Act
            TransitionResult result = handshakeReconnectingState.Transition(new HandshakeReconnectSuccessEvent() 
            {
                Cursor = new SubscriptionCursor() { Region = 1, Timetoken = 1234567890 },
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
        }

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
        
    }
}
