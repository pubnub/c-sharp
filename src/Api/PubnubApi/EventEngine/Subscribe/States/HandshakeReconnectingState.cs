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
				case Events.DisconnectEvent disconnect:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new HandshakeStoppedState() {
							Channels = disconnect.Channels,
							ChannelGroups = disconnect.ChannelGroups
						},
						null
					);
				case Events.HandshakeReconnectGiveUpEvent handshakeReconnectGiveUp:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new HandshakeFailedState() { 
							Channels = handshakeReconnectGiveUp.Channels,
							ChannelGroups = handshakeReconnectGiveUp.ChannelGroups
						},
						null
					);
				case Events.HandshakeReconnectSuccessEvent handshakeReconnectSuccess:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() { 
							Channels = handshakeReconnectSuccess.Channels,
							ChannelGroups = handshakeReconnectSuccess.ChannelGroups
						},
						new[] {
							new HandshakeReconnectInvocation() {
								Channels = handshakeReconnectSuccess.Channels,
								ChannelGroups = handshakeReconnectSuccess.ChannelGroups,
							},
						}
					);
				case Events.SubscriptionRestoredEvent subscriptionRestored:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new HandshakeFailedState() { 
							Channels = subscriptionRestored.Channels,
							ChannelGroups = subscriptionRestored.ChannelGroups
						},
						null
					);

				default: return null;
			}
		}
	}
}