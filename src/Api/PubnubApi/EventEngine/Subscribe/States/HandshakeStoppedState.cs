using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class HandshakeStoppedState : Core.IState {

		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;

		public IEnumerable<IEffectInvocation> OnEntry { get; }
		public IEnumerable<IEffectInvocation> OnExit { get; }
		public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e) {
			switch (e) {
				case Events.SubscriptionChangedEvent subscriptionChanged:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new HandshakingState() {
							Channels = subscriptionChanged.Channels,
							ChannelGroups = subscriptionChanged.ChannelGroups,
						},
						new[] {
							new HandshakeInvocation() {
								Channels = subscriptionChanged.Channels,
								ChannelGroups = subscriptionChanged.ChannelGroups,
							},
						}
					);
				case Events.ReconnectEvent reconnect:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new HandshakingState() {
							Channels = reconnect.Channels,
							ChannelGroups = reconnect.ChannelGroups,
						},
						new[] {
							new HandshakeInvocation() {
								Channels = reconnect.Channels,
								ChannelGroups = reconnect.ChannelGroups,
							},
						}
					);
				case Events.SubscriptionRestoredEvent subscriptionRestored:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() { 
							Channels = subscriptionRestored.Channels,
							ChannelGroups = subscriptionRestored.ChannelGroups
						},
						new[] {
							new ReceiveMessagesInvocation() {
								Channels = subscriptionRestored.Channels,
								ChannelGroups = subscriptionRestored.ChannelGroups,
							},
						}
					);

				default: return null;
			}
		}
	}
}