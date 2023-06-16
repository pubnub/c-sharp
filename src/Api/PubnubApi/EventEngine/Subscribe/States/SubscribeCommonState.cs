using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class SubscribeCommonState : Core.State {

		public IEnumerable<string> channels;
		public IEnumerable<string> channelGroups;
		public SubscriptionCursor cursor;

		public override IEnumerable<IEffectInvocation> onEntry { get; }
		public override IEnumerable<IEffectInvocation> onExit { get; }
		public override Tuple<Core.State, IEnumerable<IEffectInvocation>> Transition(Core.IEvent e) {
			throw new NotImplementedException();
		}
	}
}