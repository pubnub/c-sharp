using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine.Core {
	public abstract class Engine {
		private EffectDispatcher dispatcher;
		private State currentState;
		public abstract Task<State> Transition(Event e);

		private async Task ExecuteStateChange(State s1, State s2, params IEffectInvocation[] invocations) {
			foreach (var effectInvocation in s1.onExit) {
				await dispatcher.Dispatch(effectInvocation);
			}
			foreach (var effectInvocation in invocations) {
				await dispatcher.Dispatch(effectInvocation);
			}
			foreach (var effectInvocation in s2.onEntry) {
				await dispatcher.Dispatch(effectInvocation);
			}
		}
	}
}