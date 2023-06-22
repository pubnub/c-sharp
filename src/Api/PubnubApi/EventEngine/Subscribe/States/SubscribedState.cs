using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class SubscribedState : Core.IState {
		public IEnumerable<IEffectInvocation> onEntry { get; }
		public IEnumerable<IEffectInvocation> onExit { get; }
		public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e) {
			throw new NotImplementedException();
		}
	}
}