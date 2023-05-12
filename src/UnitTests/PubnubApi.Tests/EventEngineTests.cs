using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using PubnubApi;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;
using System.Diagnostics;
using PubnubApi.PubnubEventEngine;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class EventEngineTests
    {
        EventEngine pnEventEngine { get; set; }
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
            IPubnubUnitTest pubnubUnitTest = new PubnubUnitTest();
			var effectDispatcher = new EffectDispatcher();
			var eventEmitter = new EventEmitter();
            pnEventEngine = new EventEngine(effectDispatcher, eventEmitter);
            pnEventEngine.PubnubUnitTest = pubnubUnitTest;
            pnEventEngine.Setup<object>(new PNConfiguration(new UserId("testuserid")));
        }

        [Test]
        public void TestWhenStateTypeUnsubscribed()
        {
            //Unsubscribed => SubscriptionChanged  => Handshaking
            pnEventEngine.CurrentState = new State(StateType.Unsubscribed) { EventType = EventType.SubscriptionChanged };
            State currentHandshakingState = pnEventEngine.NextState();
            if (currentHandshakingState.StateType == StateType.Handshaking) 
            {
                //Expected result.
            }
            else
            {
                Assert.Fail("Unsubscribed + SubscriptionChanged => Handshaking FAILED");
                return;
            }

            //Unsubscribed => SubscriptionRestored  => Receiving
            pnEventEngine.CurrentState = new State(StateType.Unsubscribed) { EventType = EventType.SubscriptionRestored };
            State currentReceiveState = pnEventEngine.NextState();
            if (currentReceiveState.StateType == StateType.Receiving) 
            {
                //Expected result.
            }
            else
            {
                Assert.Fail("Unsubscribed + SubscriptionRestored => Receiving FAILED");
                return;
            }

        }

        [Test]
        public void TestWhenStateTypeHandshaking()
        {
            //Handshaking => SubscriptionChanged  => Handshaking
            pnEventEngine.CurrentState = new State(StateType.Handshaking) { EventType = EventType.SubscriptionChanged };
            State currentHandshakingState = pnEventEngine.NextState();
            if (currentHandshakingState.StateType == StateType.Handshaking) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("Handshaking + SubscriptionChanged => Handshaking");
                return;
            }

            //Handshaking => HandshakeFailure  => HandshakeReconnecting
            pnEventEngine.CurrentState = new State(StateType.Handshaking) { EventType = EventType.HandshakeFailure };
            State currentHandshakeReconnectingState = pnEventEngine.NextState();
            if (currentHandshakeReconnectingState.StateType == StateType.HandshakeReconnecting) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking + HandshakeFailure => HandshakeReconnecting FAILED");
                return;
            }

            //Handshaking => Disconnect  => HandshakeStopped
            pnEventEngine.CurrentState = new State(StateType.Handshaking) { EventType = EventType.Disconnect };
            State currentHandshakeStoppedState = pnEventEngine.NextState();
            if (currentHandshakeStoppedState.StateType == StateType.HandshakeStopped) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking + Disconnect => HandshakeStopped FAILED");
                return;
            }

            //Handshaking => HandshakeSuccess  => Receiving
            pnEventEngine.CurrentState = new State(StateType.Handshaking) { EventType = EventType.HandshakeSuccess };
            State currentReceivingState = pnEventEngine.NextState();
            if (currentReceivingState.StateType == StateType.Receiving) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking + HandshakeSuccess => Receiving FAILED");
                return;
            }

            //Handshaking => SubscriptionRestored  => Receiving
            pnEventEngine.CurrentState = new State(StateType.Handshaking) { EventType = EventType.SubscriptionRestored };
            currentReceivingState = pnEventEngine.NextState();
            if (currentReceivingState.StateType == StateType.Receiving) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking + SubscriptionRestored => Receiving FAILED");
                return;
            }
        }

        [Test]
        public void TestWhenStateTypeHandshakeReconnecting()
        {
            //HandshakeReconnecting => SubscriptionChanged  => Handshaking
            pnEventEngine.CurrentState = new State(StateType.HandshakeReconnecting) { EventType = EventType.SubscriptionChanged };
            State currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Handshaking) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + SubscriptionChanged => Handshaking");
                return;
            }

            //HandshakeReconnecting => HandshakeReconnectFailure  => HandshakeReconnecting
            pnEventEngine.CurrentState = new State(StateType.HandshakeReconnecting) { EventType = EventType.HandshakeReconnectFailure };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.HandshakeReconnecting) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + HandshakeReconnectFailure => HandshakeReconnecting");
                return;
            }

            //HandshakeReconnecting => Disconnect  => HandshakeStopped
            pnEventEngine.CurrentState = new State(StateType.HandshakeReconnecting) { EventType = EventType.Disconnect };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.HandshakeStopped) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + Disconnect => HandshakeStopped");
                return;
            }

            //HandshakeReconnecting => HandshakeReconnectGiveUp  => HandshakeFailed
            pnEventEngine.CurrentState = new State(StateType.HandshakeReconnecting) { EventType = EventType.HandshakeReconnectGiveUp };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.HandshakeFailed) 
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
            pnEventEngine.CurrentState = new State(StateType.HandshakeReconnecting) { EventType = EventType.HandshakeReconnectSuccess };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + HandshakeReconnectSuccess => Receiving");
                return;
            }

            //HandshakeReconnecting => SubscriptionRestored  => Receiving
            pnEventEngine.CurrentState = new State(StateType.HandshakeReconnecting) { EventType = EventType.SubscriptionRestored };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + SubscriptionRestored => Receiving");
                return;
            }

        }

        [Test]
        public void TestWhenStateTypeHandshakeFailed()
        {
            //HandshakeFailed => SubscriptionRestored  => Receiving
            pnEventEngine.CurrentState = new State(StateType.HandshakeFailed) { EventType = EventType.SubscriptionRestored };
            State currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Receiving) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeFailed + SubscriptionRestored => ReceiveReconnecting");
                return;
            }

            //HandshakeFailed => SubscriptionChanged  => Handshaking
            pnEventEngine.CurrentState = new State(StateType.HandshakeFailed) { EventType = EventType.SubscriptionChanged };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Handshaking) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeFailed + SubscriptionChanged => Handshaking");
                return;
            }


            //HandshakeFailed => Reconnect  => Handshaking
            pnEventEngine.CurrentState = new State(StateType.HandshakeFailed) { EventType = EventType.Reconnect };
            currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Handshaking) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeFailed + Reconnect => Handshaking");
                return;
            }
        }

        [Test]
        public void TestWhenStateTypeHandshakeStopped()
        {
            //HandshakeStopped => Reconnect  => Handshaking
            pnEventEngine.CurrentState = new State(StateType.HandshakeStopped) { EventType = EventType.Reconnect };
            State currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.Handshaking) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeStopped + Reconnect => Handshaking");
                return;
            }
        }


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
    }
}
