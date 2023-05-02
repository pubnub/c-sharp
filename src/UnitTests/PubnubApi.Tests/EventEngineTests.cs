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


            var handshakeEffectHandler = new HandshakeEffectHandler(eventEmitter);
            handshakeEffectHandler.LogCallback = delegate (string log)
            {
            };
            handshakeEffectHandler.HandshakeRequested += delegate (object sender, HandshakeRequestEventArgs e)
            {
            };
            EffectInvocation handshakeInvocation = new Handshake();
            handshakeInvocation.Effectype = EventType.Handshake;
            handshakeInvocation.Handler = handshakeEffectHandler;

            EffectInvocation cancelHandshakeInvocation = new CancelHandshake();
            cancelHandshakeInvocation.Effectype = EventType.CancelHandshake;
            cancelHandshakeInvocation.Handler = handshakeEffectHandler;

            var receivingEffectHandler = new ReceivingEffectHandler<object>(eventEmitter);
            receivingEffectHandler.LogCallback = delegate (string log)
            {
            };
            receivingEffectHandler.ReceiveRequested += delegate (object sender, ReceiveRequestEventArgs e)
            {
            };
            EffectInvocation receiveMessagesInvocation = new ReceiveMessages();
            receiveMessagesInvocation.Effectype = EventType.ReceiveMessages;
            receiveMessagesInvocation.Handler = receivingEffectHandler;

            EffectInvocation cancelReceiveMessages = new CancelReceiveMessages();
            cancelReceiveMessages.Effectype = EventType.CancelReceiveMessages;
            cancelReceiveMessages.Handler = receivingEffectHandler;

            EmitStatus emitStatus = new EmitStatus(PNStatusCategory.PNUnknownCategory);
            //emitStatus.Effectype = EventType.HandshakeSuccess;

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
                .On(EventType.HandshakeSuccess, StateType.Receiving, new List<EffectInvocation>()
                            { 
                                emitStatus
                            }
                )
                .On(EventType.HandshakeFailure, StateType.HandshakeReconnecting)
                .On(EventType.Disconnect, StateType.HandshakeStopped)
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .OnEntry(entryInvocationList: new List<EffectInvocation>()
                            { 
                                handshakeInvocation 
                            }
                )
                .OnExit(exitInvocationList: new List<EffectInvocation>()
                            { 
                                cancelHandshakeInvocation 
                            }
                );

            handshakeReconnectingState = pnEventEngine.CreateState(StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionChanged, StateType.HandshakeReconnecting)
                .On(EventType.HandshakeReconnectFailure, StateType.HandshakeReconnecting)
                .On(EventType.Disconnect, StateType.HandshakeStopped)
                .On(EventType.HandshakeReconnectGiveUp, StateType.HandshakeFailed)
                .On(EventType.HandshakeReconnectSuccess, StateType.Receiving, new List<EffectInvocation>()
                            { 
                                new EmitStatus(PNStatusCategory.PNConnectedCategory)
                            }
                )
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .OnEntry(entryInvocationList: new List<EffectInvocation>()
                            { 
                                new HandshakeReconnect() 
                            }
                )
                .OnExit(exitInvocationList: new List<EffectInvocation>()
                            { 
                                new CancelHandshakeReconnect() 
                            }
                );

            handshakeFailedState = pnEventEngine.CreateState(StateType.HandshakeFailed)
                .On(EventType.HandshakeReconnectRetry, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionChanged, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionRestored, StateType.ReceiveReconnecting)
                .On(EventType.Reconnect, StateType.HandshakeReconnecting);

            handshakeStoppedState = pnEventEngine.CreateState(StateType.HandshakeStopped)
                .On(EventType.Reconnect, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionChanged, StateType.Handshaking)
                .On(EventType.HandshakeSuccess, StateType.Receiving)
                .On(EventType.HandshakeFailure, StateType.HandshakeReconnecting)
                .On(EventType.SubscriptionRestored, StateType.Receiving);

            receivingState = pnEventEngine.CreateState(StateType.Receiving)
                .On(EventType.SubscriptionChanged, StateType.Receiving)
                .On(EventType.SubscriptionRestored, StateType.Receiving)
                .On(EventType.ReceiveSuccess, StateType.Receiving, new List<EffectInvocation>()
                            { 
                                //new EmitMessages(null),
                                new EmitStatus(PNStatusCategory.PNConnectedCategory)
                            }
                )
                .On(EventType.Disconnect, StateType.ReceiveStopped, new List<EffectInvocation>()
                            { 
                                new EmitStatus(PNStatusCategory.PNDisconnectedCategory)
                            }
                )
                .On(EventType.ReceiveFailure, StateType.ReceiveReconnecting)
                .OnEntry(entryInvocationList: new List<EffectInvocation>()
                            { 
                                receiveMessagesInvocation 
                            }
                )
                .OnExit(exitInvocationList: new List<EffectInvocation>()
                            { 
                                cancelReceiveMessages 
                            }
                );

            receiveReconnectingState = pnEventEngine.CreateState(StateType.ReceiveReconnecting)
                .On(EventType.SubscriptionChanged, StateType.ReceiveReconnecting)
                .On(EventType.ReceiveReconnectFailure, StateType.ReceiveReconnecting)
                .On(EventType.SubscriptionRestored, StateType.ReceiveReconnecting)
                .On(EventType.ReceiveReconnectSuccess, StateType.Receiving, new List<EffectInvocation>()
                            { 
                                //new EmitMessages(null),
                                new EmitStatus(PNStatusCategory.PNConnectedCategory)
                            }
                )
                .On(EventType.Disconnect, StateType.ReceiveStopped, new List<EffectInvocation>()
                            { 
                                new EmitStatus(PNStatusCategory.PNDisconnectedCategory)
                            }
                )
                .On(EventType.ReceiveReconnectGiveUp, StateType.ReceiveFailed, new List<EffectInvocation>()
                            { 
                                new EmitStatus(PNStatusCategory.PNDisconnectedCategory)
                            }
                )
                .OnEntry(entryInvocationList: new List<EffectInvocation>()
                            { 
                                new ReceiveReconnect()
                            }
                )
                .OnExit(exitInvocationList: new List<EffectInvocation>()
                            { 
                                new CancelReceiveReconnect()
                            }
                );
        }

        [Test]
        public void TestWhenStateTypeUnsubscribed()
        {
            //Unsubscribed => Subscription_Changed  => Handshaking
            pnEventEngine.InitialState(unsubscribeState);
            pnEventEngine.Transition(new SubscriptionChanged() 
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
            pnEventEngine.Transition(new SubscriptionChanged() 
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
            pnEventEngine.Transition(new SubscriptionChanged() 
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
            pnEventEngine.Transition(new HandshakeFailure() 
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
            pnEventEngine.Transition(new Disconnect() 
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
            pnEventEngine.Transition(new HandshakeSuccess() 
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
            pnEventEngine.Transition(new SubscriptionRestored() 
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
