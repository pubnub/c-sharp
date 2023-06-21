using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class UnsubscribedState : Core.IState {
		public IEnumerable<IEffectInvocation> onEntry { get; }
		public IEnumerable<IEffectInvocation> onExit { get; }

		public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(Core.IEvent e) {
			switch (e) {
				case Events.SubscriptionChangedEvent subscriptionChanged:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new HandshakingState() {
							channels = subscriptionChanged.channels,
							channelGroups = subscriptionChanged.channelGroups,
						},
						new[] {
							new HandshakeInvocation() {
								channels = subscriptionChanged.channels,
								channelGroups = subscriptionChanged.channelGroups,
							},
						}
					);

				default: return null;
			}
		}
	}
}