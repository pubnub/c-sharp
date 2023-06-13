using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Subscribe.Effects;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;
using PubnubApi.PubnubEventEngine.Subscribe.States;

namespace PubnubApi.PubnubEventEngine.Subscribe {
	internal class SubscribeEventEngine : PubnubEventEngine.Core.Engine {
		private SubscribeManager2 subscribeManager;
		
		public SubscribeEventEngine(SubscribeManager2 subscribeManager) {
			this.subscribeManager = subscribeManager;

			dispatcher.Register<EmitMessagesInvocation, EmitMessagesEffect>(new EmitMessagesEffect());

			currentState = new UnsubscribedState();
		}
	}
}