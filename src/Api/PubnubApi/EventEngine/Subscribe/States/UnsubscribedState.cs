using System;
using PubnubApi.PubnubEventEngine.Core;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	public class UnsubscribedState : Core.State {
		public override IEffectInvocation[] onEntry { get; }
		public override IEffectInvocation[] onExit { get; }
		public override Tuple<Core.State, IEffectInvocation[]> Transition(Event e) {
			throw new NotImplementedException();
		}
	}
}