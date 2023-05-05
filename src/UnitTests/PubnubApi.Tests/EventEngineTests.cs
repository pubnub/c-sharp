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

            //Unsubscribed => Subscription_Restored  => Receiving
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
            //Handshaking => Subscription_Changed  => Handshaking
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

            //Handshaking => Handshake_Failure  => HandshakeReconnecting
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

            //Handshaking => Handshake_Success  => Receiving
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

            //Handshaking => Subscription_Restored  => Receiving
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
            //HandshakeReconnecting => Subscription_Changed  => HandshakeReconnecting
            pnEventEngine.CurrentState = new State(StateType.HandshakeReconnecting) { EventType = EventType.SubscriptionChanged };
            State currentNewState = pnEventEngine.NextState();
            if (currentNewState.StateType == StateType.HandshakeReconnecting) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("HandshakeReconnecting + SubscriptionChanged => HandshakeReconnecting");
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
    }
}
