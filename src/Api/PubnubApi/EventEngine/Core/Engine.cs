﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PubnubApi.EventEngine.Core {
	public abstract class Engine {
		public readonly EventQueue EventQueue = new EventQueue();
		protected EffectDispatcher dispatcher = new EffectDispatcher();
		protected State currentState;
		public State CurrentState => currentState;
		private bool transitioning = false;

		private Task currentTransitionLoop = Utils.EmptyTask;

		private readonly IEffectInvocation[] emptyInvocationList = new IEffectInvocation[0];
		
		protected PubnubLogModule logger;
		
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
			EventQueue.OnEventQueued += OnEvent;
		}

		~Engine() {
			EventQueue.OnEventQueued -= OnEvent;
		}

		private async void OnEvent(EventQueue q)
		{
			OnEventQueued?.Invoke(q.Peek());
			if (transitioning) return;
			transitioning = true;
			while (q.Count > 0)
			{
				await Transition(q.Dequeue()).ConfigureAwait(false);
			}

			transitioning = false;
		}
		
		private async Task Transition(IEvent e)
		{
			var stateInvocationPair = currentState.Transition(e);
			OnStateTransition?.Invoke(stateInvocationPair);

			if (stateInvocationPair is null)
			{
				return;
			}

			await ExecuteStateChange(currentState, stateInvocationPair.State, stateInvocationPair.Invocations).ConfigureAwait(false);

			this.currentState = stateInvocationPair.State;
		}

		/// <summary>
		/// Launch the invocations associated with transitioning between states
		/// </summary>
		private async Task ExecuteStateChange(State sourceState, State targetState, IEnumerable<IEffectInvocation> invocations) {
			logger?.Debug($"Exiting state {sourceState}");
			foreach (var effectInvocation in sourceState.OnExit ?? emptyInvocationList) {
				logger?.Debug($"Dispatching effect: {effectInvocation}");
				await dispatcher.Dispatch(effectInvocation).ConfigureAwait(false);
			}
			foreach (var effectInvocation in invocations ?? emptyInvocationList) {
				logger?.Debug($"Dispatching effect: {effectInvocation}");
				await dispatcher.Dispatch(effectInvocation).ConfigureAwait(false);
			}
			logger?.Debug($"Entering state {targetState}");
			foreach (var effectInvocation in targetState.OnEntry ?? emptyInvocationList) {
				logger?.Debug($"Dispatching effect: {effectInvocation}");
				await dispatcher.Dispatch(effectInvocation).ConfigureAwait(false);
			}
		}
	}
}