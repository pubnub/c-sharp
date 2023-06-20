using System.Threading.Tasks;
using System.Collections.Generic;

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
		
		private async Task<State> Transition(IEvent e) {
			var stateInvocationPair = currentState.Transition(e);

			if (stateInvocationPair is null) {
				return currentState;
			}

			await ExecuteStateChange(currentState, stateInvocationPair.Item1, stateInvocationPair.Item2);

			return stateInvocationPair.Item1;
		}

		/// <summary>
		/// Launch the invocations associated with transitioning between states
		/// </summary>
		private async Task ExecuteStateChange(State sourceState, State targetState, IEnumerable<IEffectInvocation> invocations) {
			foreach (var effectInvocation in sourceState.onExit) {
				await dispatcher.Dispatch(effectInvocation);
			}
			foreach (var effectInvocation in invocations) {
				await dispatcher.Dispatch(effectInvocation);
			}
			foreach (var effectInvocation in targetState.onEntry) {
				await dispatcher.Dispatch(effectInvocation);
			}
		}
	}
}