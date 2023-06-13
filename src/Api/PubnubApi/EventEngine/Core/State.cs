using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine.Core {
	internal abstract class State {
		public abstract IEnumerable<IEffectInvocation> onEntry { get; }
		public abstract IEnumerable<IEffectInvocation> onExit { get; }

		/// <summary>
		/// Implement event handling here.
		/// </summary>
		public abstract System.Tuple<State, IEnumerable<IEffectInvocation>> Transition(Event e);
	}
}