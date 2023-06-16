using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class UnsubscribedState : SubscribeCommonState {
		public override IEnumerable<IEffectInvocation> onEntry { get; }
		public override IEnumerable<IEffectInvocation> onExit { get; }

		public override Tuple<Core.State, IEnumerable<IEffectInvocation>> Transition(Core.IEvent e) {
			switch (e) {
				case Events.SubscriptionChangedEvent subscriptionChanged:
					return new Tuple<Core.State, IEnumerable<IEffectInvocation>>(
						new HandshakingState() {
							channels = subscriptionChanged.channels,
							channelGroups = subscriptionChanged.channelGroups,
							cursor = cursor
						},
						new[] {
							new HandshakeInvocation() {
								channels = subscriptionChanged.channels,
								channelGroups = subscriptionChanged.channelGroups,
								cursor = cursor
							},
						}
					);

				default: return null;
			}
		}
	}
}