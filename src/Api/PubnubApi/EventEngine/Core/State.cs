using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine.Core {
	internal abstract class State {
		public abstract IEnumerable<IEffectInvocation> onEntry { get; }
		public abstract IEnumerable<IEffectInvocation> onExit { get; }

		/// <summary>
		/// The EE transition pure function.
		/// </summary>
		/// <param name="e">Input event</param>
		/// <returns>Target state and invocation list, or null for no-transition</returns>
		public abstract System.Tuple<State, IEnumerable<IEffectInvocation>> Transition(IEvent e);
	}
}