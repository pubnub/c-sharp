using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.States;
using System;

namespace PubnubApi.EventEngine.Subscribe
{
	public class SubscribeEventEngine : Engine
	{
		private SubscribeManager2 subscribeManager;

		internal SubscribeEventEngine(Pubnub pubnubInstance,
			PNConfiguration pubnubConfiguration,
			SubscribeManager2 subscribeManager,
			Action<Pubnub, PNStatus> statusListener = null,
			Action<Pubnub, PNMessageResult<object>> messageListener = null)
		{
			this.subscribeManager = subscribeManager;

			// initialize the handler, pass dependencies
			var handshakeHandler = new Effects.HandshakeEffectHandler(pubnubConfiguration,subscribeManager, eventQueue);
			dispatcher.Register<Invocations.HandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.HandshakeReconnectInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.CancelHandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);

			var receiveHandler = new Effects.ReceivingEffectHandler(pubnubConfiguration,subscribeManager, eventQueue);
			dispatcher.Register<Invocations.ReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.ReceiveReconnectInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.CancelReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);

			var emitMessageHandler = new Effects.EmitMessagesHandler(pubnubInstance, messageListener);
			dispatcher.Register<Invocations.EmitMessagesInvocation, Effects.EmitMessagesHandler>(emitMessageHandler);

			var emitStatusHandler = new Effects.EmitStatusEffectHandler(pubnubInstance, statusListener);
			dispatcher.Register<Invocations.EmitStatusInvocation, Effects.EmitStatusEffectHandler>(emitStatusHandler);

			currentState = new UnsubscribedState();
		}
	}
}