using PubnubApi.EndPoint;
using PubnubApi.PubnubEventEngine.Subscribe.Effects;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;
using PubnubApi.PubnubEventEngine.Subscribe.States;

namespace PubnubApi.PubnubEventEngine.Subscribe {
	public class SubscribeEventEngine : PubnubEventEngine.Core.Engine {
		private SubscribeOperation2<object> subscribeOperation2;
		
		public SubscribeEventEngine(SubscribeOperation2<object> subscribeOperation2) {
			this.subscribeOperation2 = subscribeOperation2;

			dispatcher.Register<EmitMessagesInvocation, EmitMessagesEffect>(new EmitMessagesEffect());

			currentState = new UnsubscribedState();
		}
	}
}