using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine.Core {
	public abstract class Engine {
		public EventQueue eventQueue = new EventQueue();
		
		private EffectDispatcher dispatcher = new EffectDispatcher();
		private State currentState;

		public Engine() {
			eventQueue.onEventQueued += OnEvent;
		}

		~Engine() {
			eventQueue.onEventQueued -= OnEvent;
		}

		private async void OnEvent() {
			currentState = await Transition(eventQueue.Dequeue());
		}

		/// <summary>
		/// Implement event handling here.
		/// </summary>
		public abstract Task<State> Transition(Event e);

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