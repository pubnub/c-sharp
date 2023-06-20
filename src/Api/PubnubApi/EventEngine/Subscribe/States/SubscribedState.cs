﻿using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class SubscribedState : Core.State {
		public override IEnumerable<IEffectInvocation> onEntry { get; }
		public override IEnumerable<IEffectInvocation> onExit { get; }
		public override Tuple<Core.State, IEnumerable<IEffectInvocation>> Transition(IEvent e) {
			throw new NotImplementedException();
		}
	}
}