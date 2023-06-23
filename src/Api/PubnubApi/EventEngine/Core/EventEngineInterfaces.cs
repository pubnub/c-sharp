using System.Threading.Tasks;
using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Core {
	internal interface IEffectHandler {
		Task Cancel();
	}
	
	internal interface IEffectHandler<in T> : IEffectHandler where T : IEffectInvocation {
		Task Run(T invocation);
	}
	
	internal interface IEffectInvocation { }

	internal interface IEffectCancelInvocation : IEffectInvocation { }

	internal interface IEvent { };
	
	internal interface IState {
		public abstract IEnumerable<IEffectInvocation> OnEntry { get; }
		public abstract IEnumerable<IEffectInvocation> OnExit { get; }

		/// <summary>
		/// The EE transition pure function.
		/// </summary>
		/// <param name="e">Input event</param>
		/// <returns>Target state and invocation list, or null for no-transition</returns>
		public abstract System.Tuple<IState, IEnumerable<IEffectInvocation>> Transition(IEvent e);
	}
}