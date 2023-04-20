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
            //eventEmitter.RegisterJsonListener(JsonCallback);

            var handshakeEffect = new HandshakeEffectHandler(eventEmitter);
            //handshakeEffect.InvocationType = EffectInvocationType.Entry;
            handshakeEffect.LogCallback = delegate (string log)
            {
            };
            handshakeEffect.HandshakeRequested += delegate (object sender, HandshakeRequestEventArgs e)
            {
            };

            //var receivingEffect = new ReceivingEffectHandler<string>(eventEmitter);
            //receivingEffect.LogCallback = delegate(string log)
            //{ 
            //};
            //receivingEffect.ReceiveRequested += delegate(object sender, ReceiveRequestEventArgs e)
            //{ 
            //};

            //var reconnectionEffect = new ReconnectingEffectHandler<string>(eventEmitter);

            //effectDispatcher.Register(EffectInvocationType.HandshakeSuccess, handshakeEffect);
            //effectDispatcher.Register(EffectInvocationType.ReceiveSuccess, receivingEffect);
            //effectDispatcher.Register(EffectInvocationType.ReconnectionAttempt, reconnectionEffect);

            pnEventEngine = new EventEngine(effectDispatcher, eventEmitter);
            pnEventEngine.PubnubUnitTest = pubnubUnitTest;

			unsubscribeState = pnEventEngine.CreateState(StateType.Unsubscribed)
				.On(EventType.SubscriptionChanged, StateType.Handshaking)
                .On(EventType.SubscriptionRestored, StateType.Receiving);

            handshakingState = pnEventEngine.CreateState(StateType.Handshaking)
                .On(EventType.SubscriptionChanged, StateType.Handshaking)
                .On(EventType.HandshakeSuccess, StateType.Receiving)
                .On(EventType.HandshakeFailure, StateType.HandshakeReconnecting)
                .On(EventType.Disconnect, StateType.HandshakeStopped)
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .OnEntry(entryInvocationList: new List<IEffectInvocationHandler>(){ handshakeEffect })
                .OnExit(exitInvocationList: new List<IEffectInvocationHandler>() { handshakeEffect })
                .EffectInvocation(EffectInvocationType.HandshakeSuccess, effectInvocationHandler: null);
            //handshakingState.EffectInvocationsEffectInvocationsList = new List<EffectInvocationType>() { EffectInvocationType.HandshakeSuccess };
            //handshakingState.OnEntry(Func<bool>);

            handshakeReconnectingState = pnEventEngine.CreateState(StateType.HandshakeReconnecting)
                //.OnEntry(() => { System.Diagnostics.Debug.WriteLine("handshakeReconnectingState: OnEntry()"); return true; })
                //.OnExit(() =>  { System.Diagnostics.Debug.WriteLine("handshakeReconnectingState: OnExit()"); return true; })
                .On(EventType.SubscriptionChanged, StateType.HandshakeReconnecting)
                .On(EventType.HandshakeReconnectFailure, StateType.HandshakeReconnecting)
                .On(EventType.Disconnect, StateType.HandshakeStopped)
                .On(EventType.HandshakeReconnectGiveUp, StateType.HandshakeFailed)
                .On(EventType.HandshakeReconnectSuccess, StateType.Receiving)
                .On(EventType.SubscriptionRestored, StateType.Receiving);

            handshakeStoppedState = pnEventEngine.CreateState(StateType.HandshakeStopped)
                //.OnEntry(() => { System.Diagnostics.Debug.WriteLine("handshakeStoppedState: OnEntry()"); return true; })
                //.OnExit(() =>  { System.Diagnostics.Debug.WriteLine("handshakeStoppedState: OnExit()"); return true; })
                .On(EventType.Reconnect, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionChanged, StateType.Handshaking)
                .On(EventType.HandshakeSuccess, StateType.Receiving)
                .On(EventType.HandshakeFailure, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .EffectInvocation(EffectInvocationType.HandshakeSuccess, effectInvocationHandler: null);

            handshakeFailedState = pnEventEngine.CreateState(StateType.HandshakeFailed)
                //.OnEntry(() => { System.Diagnostics.Debug.WriteLine("handshakeFailedState: OnEntry()"); return true; })
                //.OnExit(() =>  { System.Diagnostics.Debug.WriteLine("handshakeFailedState: OnExit()"); return true; })
                .On(EventType.HandshakeReconnectRetry, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionChanged, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionRestored, StateType.ReceiveReconnecting)
                .On(EventType.Reconnect, StateType.HandshakeReconnecting);

            receivingState = pnEventEngine.CreateState(StateType.Receiving)
                //.OnEntry(() => { System.Diagnostics.Debug.WriteLine("receivingState: OnEntry()"); return true; })
                //.OnExit(() => { System.Diagnostics.Debug.WriteLine("receivingState: OnExit()"); return true; })
                .On(EventType.SubscriptionChanged, StateType.Receiving)
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .On(EventType.ReceiveSuccess, StateType.Receiving)
                .On(EventType.Disconnect, StateType.ReceiveStopped)
                .On(EventType.ReceiveFailure, StateType.ReceiveReconnecting)
                .EffectInvocation(EffectInvocationType.ReceiveSuccess, effectInvocationHandler: null)
                .EffectInvocation(EffectInvocationType.Disconnect, effectInvocationHandler: null);

            receiveReconnectingState = pnEventEngine.CreateState(StateType.ReceiveReconnecting)
                //.OnEntry(() => { System.Diagnostics.Debug.WriteLine("receiveReconnectingState: OnEntry()"); return true; })
                //.OnExit(() => { System.Diagnostics.Debug.WriteLine("receiveReconnectingState: OnExit()"); return true; })
                .On(EventType.SubscriptionChanged, StateType.ReceiveReconnecting)
                .On(EventType.ReceiveReconnectFailure, StateType.ReceiveReconnecting)
                .On(EventType.SubscriptionRestored, StateType.ReceiveReconnecting)
                .On(EventType.ReceiveReconnectSuccess, StateType.Receiving)
                .On(EventType.Disconnect, StateType.ReceiveStopped)
                .On(EventType.ReceiveReconnectGiveUp, StateType.ReceiveFailed);
        }

        [Test]
        public void TestWhenStateTypeUnsubscribed()
        {
            //Unsubscribed => Subscription_Changed  => Handshaking
            pnEventEngine.InitialState(unsubscribeState);
            pnEventEngine.Transition(new Event() 
                                        { 
                                            EventType = EventType.SubscriptionChanged, 
                                            EventPayload = new EventPayload(){ }
                                        });
            State currentHandshakingState = pnEventEngine.CurrentState;
            if (currentHandshakingState.EventType == EventType.SubscriptionChanged &&
                currentHandshakingState.StateType == StateType.Handshaking) 
            {
                //Expected result.
            }
            else
            {
                Assert.Fail("unsubscribeState SubscriptionChanged => Handshaking FAILED");
                return;
            }

            //Unsubscribed => Subscription_Restored  => Receiving
            pnEventEngine.InitialState(unsubscribeState);
            pnEventEngine.Transition(new Event() 
                                        { 
                                            EventType = EventType.SubscriptionRestored, 
                                            EventPayload = new EventPayload(){ }
                                        });
            State currentReceiveState = pnEventEngine.CurrentState;
            if (currentReceiveState.EventType == EventType.SubscriptionRestored &&
                currentReceiveState.StateType == StateType.Receiving) 
            {
                //Expected result.
            }
            else
            {
                Assert.Fail("unsubscribeState SubscriptionRestored => Receiving FAILED");
                return;
            }

        }

        [Test]
        public void TestWhenStateTypeHandshaking()
        {
            //Handshaking => Subscription_Changed  => Handshaking
            pnEventEngine.InitialState(handshakingState);
            pnEventEngine.Transition(new Event() 
                                        { 
                                            EventType = EventType.SubscriptionChanged, 
                                            EventPayload = new EventPayload(){ }
                                        });
            State currentHandshakingState = pnEventEngine.CurrentState;
            if (currentHandshakingState.EventType == EventType.SubscriptionChanged &&
                currentHandshakingState.StateType == StateType.Handshaking) 
            {
                //Continue for further tests on transition
            }
            else
            {
                Assert.Fail("handshakingState SubscriptionChanged => Handshaking");
                return;
            }

            //Handshaking => Handshake_Failure  => HandshakeReconnecting
            pnEventEngine.InitialState(handshakingState);
            pnEventEngine.Transition(new Event() 
                                        { 
                                            EventType = EventType.HandshakeFailure, 
                                            EventPayload = new EventPayload(){ }
                                        });
            State currentHandshakeReconnectingState = pnEventEngine.CurrentState;
            if (currentHandshakeReconnectingState.EventType == EventType.HandshakeFailure &&
                currentHandshakeReconnectingState.StateType == StateType.HandshakeReconnecting) 
            {
                //empty
            }
            else
            {
                Assert.Fail("handshakingState HandshakeFailure => HandshakeReconnecting FAILED");
                return;
            }

            //Handshaking => Disconnect  => HandshakeStopped
            pnEventEngine.InitialState(handshakingState);
            pnEventEngine.Transition(new Event() 
                                        { 
                                            EventType = EventType.Disconnect, 
                                            EventPayload = new EventPayload(){ }
                                        });
            State currentHandshakeStoppedState = pnEventEngine.CurrentState;
            if (currentHandshakeStoppedState.EventType == EventType.Disconnect &&
                currentHandshakeStoppedState.StateType == StateType.HandshakeStopped) 
            {
                //empty
            }
            else
            {
                Assert.Fail("handshakingState Disconnect => HandshakeStopped FAILED");
                return;
            }

            //Handshaking => Handshake_Success  => Receiving
            pnEventEngine.InitialState(handshakingState);
            pnEventEngine.Transition(new Event() 
                                        { 
                                            EventType = EventType.HandshakeSuccess, 
                                            EventPayload = new EventPayload(){ }
                                        });
            State currentReceivingState = pnEventEngine.CurrentState;
            if (currentReceivingState.EventType == EventType.HandshakeSuccess &&
                currentReceivingState.StateType == StateType.Receiving) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking HandshakeSuccess => Receiving FAILED");
                return;
            }

            //Handshaking => Subscription_Restored  => Receiving
            pnEventEngine.InitialState(handshakingState);
            pnEventEngine.Transition(new Event() 
                                        { 
                                            EventType = EventType.SubscriptionRestored, 
                                            EventPayload = new EventPayload(){ }
                                        });
            currentReceivingState = pnEventEngine.CurrentState;
            if (currentReceivingState.EventType == EventType.SubscriptionRestored &&
                currentReceivingState.StateType == StateType.Receiving) 
            {
                //empty
            }
            else
            {
                Assert.Fail("Handshaking SubscriptionRestored => Receiving FAILED");
                return;
            }
        }
    }
}
