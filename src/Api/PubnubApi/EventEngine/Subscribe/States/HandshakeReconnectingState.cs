using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class HandshakeReconnectingState : Core.IState {

		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;

		public IEnumerable<IEffectInvocation> OnEntry { get; }
		public IEnumerable<IEffectInvocation> OnExit { get; }
		public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e) {
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
				case Events.DisconnectEvent disconnectEvent:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new HandshakeStoppedState() {
							Channels = disconnectEvent.Channels,
							ChannelGroups = disconnectEvent.ChannelGroups
						},
						null
					);
				case Events.HandshakeReconnectGiveUpEvent handshakeReconnectGiveUpEvent:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new HandshakeFailedState() { },
						null
					);

				default: return null;
			}
		}
	}
}