using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.States;
using PubnubApi.EventEngine.Subscribe.Effects;
using PubnubApi.EventEngine.Subscribe.Invocations;
using System;

namespace PubnubApi.EventEngine.Subscribe {
	public class SubscribeEventEngine : Engine {
		private SubscribeManager2 subscribeManager;
		
		internal SubscribeEventEngine(
			PNConfiguration pubnubConfiguration, 
			Pubnub pubnubInstance, 
			SubscribeManager2 subscribeManager, 
			Action<Pubnub, PNStatus> statusListener = null,
			Action<Pubnub, PNMessageResult<object>> messageListener = null) 
		{
			this.subscribeManager = subscribeManager;

			// initialize the handler, pass dependencies
			var handshakeHandler = new Effects.HandshakeEffectHandler(subscribeManager, EventQueue);
			dispatcher.Register<Invocations.HandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.CancelHandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.HandshakeReconnectInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
            dispatcher.Register<Invocations.CancelHandshakeReconnectInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);

			var receiveHandler = new Effects.ReceivingEffectHandler(subscribeManager, EventQueue);
			dispatcher.Register<Invocations.ReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.CancelReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.ReceiveReconnectInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.CancelReceiveReconnectInvocation, Effects.ReceivingEffectHandler>(receiveHandler);

			var emitMessageHandler = new Effects.EmitMessagesHandler(pubnubInstance, messageListener);
			dispatcher.Register<Invocations.EmitMessagesInvocation, Effects.EmitMessagesHandler>(emitMessageHandler);

			var emitStatusHandler = new Effects.EmitStatusEffectHandler(pubnubInstance, statusListener);
			dispatcher.Register<Invocations.EmitStatusInvocation, Effects.EmitStatusEffectHandler>(emitStatusHandler);

			currentState = new UnsubscribedState() { ReconnectionConfiguration = new Context.ReconnectionConfiguration(pubnubConfiguration.ReconnectionPolicy, pubnubConfiguration.ConnectionMaxRetries) };
		}
	}
}