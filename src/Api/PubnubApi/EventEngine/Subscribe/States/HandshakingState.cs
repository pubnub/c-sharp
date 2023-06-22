using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class HandshakingState : Core.IState {

		public IEnumerable<string> channels;
		public IEnumerable<string> channelGroups;

		public IEnumerable<IEffectInvocation> onEntry { get; }
		public IEnumerable<IEffectInvocation> onExit { get; }
		public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e) {
			throw new NotImplementedException();
		}
	}
}