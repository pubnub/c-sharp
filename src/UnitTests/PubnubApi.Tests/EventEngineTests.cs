using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.States;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class EventEngineTests
    {
        State unsubscribeState;
        State handshakingState;
        State handshakeReconnectingState;
        State handshakeStoppedState;
        State handshakeFailedState;
        State receivingState;
        State receiveReconnectingState;

        [SetUp]
        public void Init()
        {
        }

        [Test]
        public void TestWhenStateTypeUnsubscribed()
        {
            //Unsubscribed => SubscriptionChanged  => Handshaking
            unsubscribeState = new UnsubscribedState();
            IEvent subscriptionChanged = new SubscriptionChangedEvent();
            TransitionResult transitionResult = unsubscribeState.Transition(subscriptionChanged);
            if (transitionResult.State is HandshakingState) 
            {
                //Expected result.
            }
            else
            {
                Assert.Fail("Unsubscribed + SubscriptionChanged => Handshaking FAILED");
                return;
            }

            //Unsubscribed => SubscriptionRestored  => Receiving
            IEvent subscriptionRestored = new SubscriptionRestoredEvent();
            transitionResult = unsubscribeState.Transition(subscriptionRestored);
            if (transitionResult.State is ReceivingState) 
            {
                //Expected result.
            }
            else
            {
                Assert.Fail("Unsubscribed + SubscriptionRestored => Receiving FAILED");
            }
        }

        [Test]
        public void TestWhenStateTypeHandshaking()
        {
            //Handshaking => SubscriptionChanged  => Handshaking
            handshakingState = new HandshakingState();
            IEvent subscriptionChanged = new SubscriptionChangedEvent();
            TransitionResult transitionResult = handshakingState.Transition(subscriptionChanged);
            if (transitionResult.State is HandshakingState) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("Handshaking + SubscriptionChanged => Handshaking");
                return;
            }

            //Handshaking => HandshakeFailure  => HandshakeReconnecting
            IEvent handshakeFailure = new HandshakeFailureEvent();
            transitionResult = handshakingState.Transition(handshakeFailure);
            if (transitionResult.State is HandshakeReconnectingState) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking + HandshakeFailure => HandshakeReconnecting FAILED");
                return;
            }

            //Handshaking => Disconnect  => HandshakeStopped
            IEvent disconnect = new DisconnectEvent();
            transitionResult = handshakingState.Transition(disconnect);
            if (transitionResult.State is HandshakeStoppedState) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking + Disconnect => HandshakeStopped FAILED");
                return;
            }

            //Handshaking => HandshakeSuccess  => Receiving
            IEvent handshakeSuccess = new HandshakeSuccessEvent();
            transitionResult = handshakingState.Transition(handshakeSuccess);
            if (transitionResult.State is ReceivingState) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking + HandshakeSuccess => Receiving FAILED");
                return;
            }

            //Handshaking => SubscriptionRestored  => Receiving
            IEvent subscriptionRestored = new SubscriptionRestoredEvent();
            transitionResult = handshakingState.Transition(subscriptionRestored);
            if (transitionResult.State is ReceivingState) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking + SubscriptionRestored => Receiving FAILED");
            }
        }

        [Test]
        public void TestWhenStateTypeHandshakeReconnecting()
        {
            //HandshakeReconnecting => SubscriptionChanged  => Handshaking
            handshakeReconnectingState = new HandshakeReconnectingState();
            IEvent subscriptionChanged = new SubscriptionChangedEvent();
            TransitionResult transitionResult = handshakeReconnectingState.Transition(subscriptionChanged);
            if (!(transitionResult.State is HandshakingState))
            {
                Assert.Fail("HandshakeReconnecting + SubscriptionChanged => Handshaking");
                return;
            }

            //HandshakeReconnecting => HandshakeReconnectFailure  => HandshakeReconnecting
            IEvent handshakeReconnectFailure = new HandshakeReconnectFailureEvent();
            transitionResult = handshakeReconnectingState.Transition(handshakeReconnectFailure);
            if (transitionResult.State is  HandshakeReconnectingState) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + HandshakeReconnectFailure => HandshakeReconnecting");
                return;
            }

            //HandshakeReconnecting => Disconnect  => HandshakeStopped
            IEvent disconnect = new DisconnectEvent();
            transitionResult = handshakeReconnectingState.Transition(disconnect);
            if (transitionResult.State is  HandshakeStoppedState) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + Disconnect => HandshakeStopped");
                return;
            }

            //HandshakeReconnecting => HandshakeReconnectGiveUp  => HandshakeFailed
            IEvent handshakeReconnectGiveup = new HandshakeReconnectGiveUpEvent();
            transitionResult = handshakeReconnectingState.Transition(handshakeReconnectGiveup);
            if (transitionResult.State is  HandshakeFailedState) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + HandshakeReconnectGiveUp => HandshakeFailed");
                return;
            }

            //
            //HandshakeReconnecting => HandshakeReconnectSuccess  => Receiving
            IEvent handshakeReconnectSuccess = new HandshakeReconnectSuccessEvent();
            transitionResult = handshakeReconnectingState.Transition(handshakeReconnectSuccess);
            if (transitionResult.State is  ReceivingState) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + HandshakeReconnectSuccess => Receiving");
                return;
            }

            //HandshakeReconnecting => SubscriptionRestored  => Receiving
            IEvent subscriptionRestored = new SubscriptionRestoredEvent();
            transitionResult = handshakeReconnectingState.Transition(subscriptionRestored);
            if (!(transitionResult.State is ReceivingState))
            {
                Assert.Fail("HandshakeReconnecting + SubscriptionRestored => Receiving");
            }
        }

        [Test]
        public void TestWhenStateTypeHandshakeFailed()
        {
            //HandshakeFailed => SubscriptionRestored  => Receiving
            handshakeFailedState = new HandshakeFailedState();
            IEvent subscriptionRestored = new SubscriptionRestoredEvent();
            TransitionResult transitionResult = handshakeFailedState.Transition(subscriptionRestored);
            if (!(transitionResult.State is ReceivingState))
            { 
                Assert.Fail("HandshakeFailed + SubscriptionRestored => Receiving");
                return;
            }

            //HandshakeFailed => SubscriptionChanged  => Handshaking
            IEvent subscriptionChanged = new SubscriptionChangedEvent();
            transitionResult = handshakeFailedState.Transition(subscriptionChanged);
            if (!(transitionResult.State is HandshakingState))
            {
                Assert.Fail("HandshakeFailed + SubscriptionChanged => Handshaking");
                return;
            }

            //HandshakeFailed => Reconnect  => Handshaking
            IEvent reconnect = new ReconnectEvent();
            transitionResult = handshakeFailedState.Transition(reconnect);
            if (!(transitionResult.State is HandshakingState))
            {
                Assert.Fail("HandshakeFailed + Reconnect => Handshaking");
            }
        }

        [Test]
        public void TestWhenStateTypeHandshakeStopped()
        {
            //HandshakeStopped => Reconnect  => Handshaking
            handshakeStoppedState = new HandshakeStoppedState();
            IEvent reconnect = new ReconnectEvent();
            TransitionResult transitionResult = handshakeStoppedState.Transition(reconnect);
            if (!(transitionResult.State is HandshakingState))
            {
                Assert.Fail("HandshakeStopped + Reconnect => Handshaking");
            }
        }


        /*
        [Test]
        public void TestWhenStateTypeReceiving()
        {
            //Receiving => SubscriptionChanged  => Receiving
            pnEventEngine.CurrentState = new State(StateType.Receiving) { EventType = EventType.SubscriptionChanged };
            State currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("Receiving + SubscriptionChanged => Receiving");
                return;
            }

            //Receiving => SubscriptionRestored  => Receiving
            pnEventEngine.CurrentState = new State(StateType.Receiving) { EventType = EventType.SubscriptionRestored };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("Receiving + SubscriptionRestored => Receiving");
                return;
            }

            //Receiving => ReceiveSuccess  => Receiving
            pnEventEngine.CurrentState = new State(StateType.Receiving) { EventType = EventType.ReceiveSuccess };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("Receiving + ReceiveSuccess => Receiving");
                return;
            }

            //Receiving => ReceiveFailure  => ReceiveReconnecting
            pnEventEngine.CurrentState = new State(StateType.Receiving) { EventType = EventType.ReceiveFailure };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.ReceiveReconnecting) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("Receiving + ReceiveFailure => ReceiveReconnecting");
                return;
            }

            //Receiving => Disconnect  => ReceiveStopped
            pnEventEngine.CurrentState = new State(StateType.Receiving) { EventType = EventType.Disconnect };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.ReceiveStopped) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("Receiving + Disconnect => ReceiveStopped");
                return;
            }
        }

        [Test]
        public void TestWhenStateTypeReceiveReconnecting()
        {
            //ReceiveReconnecting => SubscriptionChanged  => Receiving
            pnEventEngine.CurrentState = new State(StateType.ReceiveReconnecting) { EventType = EventType.SubscriptionChanged };
            State currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("ReceiveReconnecting + SubscriptionChanged => Receiving");
                return;
            }

            //ReceiveReconnecting => SubscriptionRestored  => Receiving
            pnEventEngine.CurrentState = new State(StateType.ReceiveReconnecting) { EventType = EventType.SubscriptionRestored };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("ReceiveReconnecting + SubscriptionRestored => Receiving");
                return;
            }

            //ReceiveReconnecting => ReceiveReconnectFailure  => ReceiveReconnecting
            pnEventEngine.CurrentState = new State(StateType.ReceiveReconnecting) { EventType = EventType.ReceiveReconnectFailure };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.ReceiveReconnecting) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("ReceiveReconnecting + ReceiveReconnectFailure => ReceiveReconnecting");
                return;
            }

            //ReceiveReconnecting => ReceiveReconnectGiveUp  => ReceiveFailed
            pnEventEngine.CurrentState = new State(StateType.ReceiveReconnecting) { EventType = EventType.ReceiveReconnectGiveUp };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.ReceiveFailed) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("ReceiveReconnecting + ReceiveReconnectGiveUp => ReceiveFailed");
                return;
            }

            //ReceiveReconnecting => Disconnect  => ReceiveStopped
            pnEventEngine.CurrentState = new State(StateType.ReceiveReconnecting) { EventType = EventType.Disconnect };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.ReceiveStopped) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("ReceiveReconnecting + Disconnect => ReceiveStopped");
                return;
            }

        }

        [Test]
        public void TestWhenStateTypeReceiveFailed()
        {
            //ReceiveFailed => SubscriptionChanged  => Receiving
            pnEventEngine.CurrentState = new State(StateType.ReceiveFailed) { EventType = EventType.SubscriptionChanged };
            State currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("ReceiveFailed + SubscriptionChanged => Receiving");
                return;
            }

            //ReceiveFailed => SubscriptionRestored  => Receiving
            pnEventEngine.CurrentState = new State(StateType.ReceiveFailed) { EventType = EventType.SubscriptionRestored };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("ReceiveFailed + SubscriptionRestored => Receiving");
                return;
            }

            //ReceiveFailed => Reconnect  => Receiving
            pnEventEngine.CurrentState = new State(StateType.ReceiveFailed) { EventType = EventType.Reconnect };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("ReceiveFailed + Reconnect => Receiving");
                return;
            }
        }

        [Test]
        public void TestWhenStateTypeReceiveStopped()
        {
            //ReceiveStopped => Reconnect  => Receiving
            pnEventEngine.CurrentState = new State(StateType.ReceiveStopped) { EventType = EventType.Reconnect };
            State currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("ReceiveStopped + Reconnect => Receiving");
                return;
            }

        }
        */
    }
}
