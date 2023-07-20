using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PubnubApi.EventEngine.Core {
	
	/// <summary>
	/// Generic effect handler.
	/// </summary>
	internal interface IEffectHandler {
		Task Cancel();
	}
	
	/// <summary>
	/// Handler (implementation) for a given invocation. The invocation represents the input arguments of a handler.
	/// </summary>
	/// <typeparam name="T">Associated invocation</typeparam>
	internal interface IEffectHandler<in T> : IEffectHandler where T : IEffectInvocation {
		Task Run(T invocation);
		bool IsBackground(T invocation);
	}
	
	/// <summary>
	/// An effect invocation. It represents calling <c>Run()</c> on a registered effect handler - calling it is orchestrated by the dispatcher.
	/// </summary>
	internal interface IEffectInvocation { }

	/// <summary>
	/// A cancel effect invocation. It represents calling <c>Cancel()</c> on a registered effect handler - calling it is orchestrated by the dispatcher.
	/// </summary>
	internal interface IEffectCancelInvocation : IEffectInvocation { }

	internal interface IEvent { };
	
	internal abstract class State
	{
		public virtual IEnumerable<IEffectInvocation> OnEntry { get; } = null;
		public virtual IEnumerable<IEffectInvocation> OnExit { get; } = null;

		/// <summary>
		/// The EE transition pure function.
		/// </summary>
		/// <param name="e">Input event</param>
		/// <returns>Target state and invocation list, or null for no-transition</returns>
		public abstract TransitionResult Transition(IEvent e);

		public TransitionResult With(params IEffectInvocation[] invocations)
		{
			return new TransitionResult(this, invocations);
		}
		
		public static implicit operator TransitionResult(State s)
		{
			return new TransitionResult(s);
		}
	}
}