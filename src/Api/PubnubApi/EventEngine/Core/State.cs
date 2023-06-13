using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine.Core {
	public abstract class State {
		public abstract IEffectInvocation[] onEntry { get; }
		public abstract IEffectInvocation[] onExit { get; }

		/// <summary>
		/// Implement event handling here.
		/// </summary>
		public abstract System.Tuple<State, IEffectInvocation[]> Transition(Event e);
	}
}