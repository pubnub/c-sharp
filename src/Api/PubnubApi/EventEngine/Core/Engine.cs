using System.Threading.Tasks;
using System.Collections.Generic;

namespace PubnubApi.EventEngine.Core {
	internal abstract class Engine {
		public EventQueue eventQueue = new EventQueue();
		
		protected EffectDispatcher dispatcher = new EffectDispatcher();
		protected State currentState;

		private Task currentTransitionLoop = Utils.EmptyTask;

		private readonly IEffectInvocation[] emptyInvocationList = new IEffectInvocation[0];

		public Engine() {
			eventQueue.OnEventQueued += OnEvent;
		}

		~Engine() {
			eventQueue.OnEventQueued -= OnEvent;
		}

		private async void OnEvent(EventQueue q)
		{
			await currentTransitionLoop;
			currentTransitionLoop = eventQueue.Loop(async e => currentState = await Transition(e));
		}
		
		private async Task<State> Transition(IEvent e) {
			var stateInvocationPair = currentState.Transition(e);

			if (stateInvocationPair is null) {
				return currentState;
			}

			await ExecuteStateChange(currentState, stateInvocationPair.State, stateInvocationPair.Invocations);

			return stateInvocationPair.State;
		}

		/// <summary>
		/// Launch the invocations associated with transitioning between states
		/// </summary>
		private async Task ExecuteStateChange(State sourceState, State targetState, IEnumerable<IEffectInvocation> invocations) {
			foreach (var effectInvocation in sourceState.OnExit ?? emptyInvocationList) {
				await dispatcher.Dispatch(effectInvocation);
			}
			foreach (var effectInvocation in invocations ?? emptyInvocationList) {
				await dispatcher.Dispatch(effectInvocation);
			}
			foreach (var effectInvocation in targetState.OnEntry ?? emptyInvocationList) {
				await dispatcher.Dispatch(effectInvocation);
			}
		}
	}
}