﻿using System.Threading.Tasks;
using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Core {
	internal abstract class Engine {
		public EventQueue eventQueue = new EventQueue();
		
		protected EffectDispatcher dispatcher = new EffectDispatcher();
		protected IState currentState;

		private Task<IState> currentTransition;

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
		
		private async Task<IState> Transition(IEvent e) {
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
		private async Task ExecuteStateChange(IState sourceState, IState targetState, IEnumerable<IEffectInvocation> invocations) {
			foreach (var effectInvocation in sourceState.OnExit) {
				await dispatcher.Dispatch(effectInvocation);
			}
			foreach (var effectInvocation in invocations) {
				await dispatcher.Dispatch(effectInvocation);
			}
			foreach (var effectInvocation in targetState.OnEntry) {
				await dispatcher.Dispatch(effectInvocation);
			}
		}
	}
}