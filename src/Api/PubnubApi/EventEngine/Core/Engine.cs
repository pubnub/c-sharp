using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine.Core {
	internal abstract class Engine {
		public EventQueue eventQueue = new EventQueue();
		
		protected EffectDispatcher dispatcher = new EffectDispatcher();
		protected State currentState;

		private Task<State> currentTransition;

		public Engine() {
			eventQueue.onEventQueued += OnEvent;
		}

		~Engine() {
			eventQueue.onEventQueued -= OnEvent;
		}

		private async void OnEvent(EventQueue q) {
			if (!(currentTransition is null)) {
				await currentTransition;
			}
			currentTransition = Transition(q.Dequeue()).ContinueWith((res) => currentState = res.Result);
		}
		
		private async Task<State> Transition(Event e) {
			var ret = currentState.Transition(e);

			await ExecuteStateChange(currentState, ret.Item1, ret.Item2);

			return ret.Item1;
		}

		/// <summary>
		/// Launch the invocations associated with transitioning between states
		/// </summary>
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