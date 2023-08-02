using System.Threading.Tasks;
using System.Collections.Generic;

namespace PubnubApi.EventEngine.Core {
	public abstract class Engine {
		public EventQueue eventQueue = new EventQueue();
		
		protected EffectDispatcher dispatcher = new EffectDispatcher();
		protected State currentState;

		private Task currentTransitionLoop = Utils.EmptyTask;

		private readonly IEffectInvocation[] emptyInvocationList = new IEffectInvocation[0];
		
		/// <summary>
		/// Subscribe to receive notification on effect dispatch
		/// </summary>
		public event System.Action<IEffectInvocation> OnEffectDispatch
		{
			add => dispatcher.OnEffectDispatch += value;
			remove => dispatcher.OnEffectDispatch -= value;
		}

		/// <summary>
		/// Subscribe to receive notification on state transition
		/// </summary>
		public event System.Action<TransitionResult> OnStateTransition;

		/// <summary>
		/// Subscribe to receive notification on event being queued
		/// </summary>
		public event System.Action<IEvent> OnEventQueued;

		public Engine() {
			eventQueue.OnEventQueued += OnEvent;
		}

		~Engine() {
			eventQueue.OnEventQueued -= OnEvent;
		}

		private async void OnEvent(EventQueue q)
		{
			OnEventQueued?.Invoke(q.Peek());
			await currentTransitionLoop;
			currentTransitionLoop = eventQueue.Loop(async e => currentState = await Transition(e));
		}
		
		private async Task<State> Transition(IEvent e) {
			var stateInvocationPair = currentState.Transition(e);
			OnStateTransition?.Invoke(stateInvocationPair);

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