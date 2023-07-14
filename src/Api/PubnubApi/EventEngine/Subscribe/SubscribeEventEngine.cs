using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Subscribe.Effects;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;
using PubnubApi.PubnubEventEngine.Subscribe.States;

namespace PubnubApi.PubnubEventEngine.Subscribe {
	internal class SubscribeEventEngine : PubnubEventEngine.Core.Engine {
		private SubscribeManager2 subscribeManager;
		
		public SubscribeEventEngine(SubscribeManager2 subscribeManager) {
			this.subscribeManager = subscribeManager;

			// initialize the handler, pass dependencies
			var handshakeHandler = new Effects.HandshakeEffectHandler(subscribeManager, eventQueue);
			dispatcher.Register<Invocations.HandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.CancelHandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);

			var leaveHandler = new Effects.LeaveEffectHandler(subscribeManager, eventQueue);
			dispatcher.Register<Invocations.LeaveInvocation, Effects.LeaveEffectHandler>(leaveHandler);
			dispatcher.Register<Invocations.CancelLeaveInvocation, Effects.LeaveEffectHandler>(leaveHandler);

			currentState = new UnsubscribedState();
		}
	}
}