using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.States;
using PubnubApi.EventEngine.Subscribe.Effects;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe {
	public class SubscribeEventEngine : Engine {
		private SubscribeManager2 subscribeManager;
		
		internal SubscribeEventEngine(SubscribeManager2 subscribeManager) {
			this.subscribeManager = subscribeManager;

			// initialize the handler, pass dependencies
			var handshakeHandler = new Effects.HandshakeEffectHandler(subscribeManager, eventQueue);
			dispatcher.Register<Invocations.HandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.HandshakeReconnectInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.CancelHandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);

			var receiveHandler = new Effects.ReceivingEffectHandler(subscribeManager, eventQueue);
			dispatcher.Register<Invocations.ReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.ReceiveReconnectInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.CancelReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);

			// TODO: ReconnectionConfiguration
			currentState = new UnsubscribedState();
		}
	}
}