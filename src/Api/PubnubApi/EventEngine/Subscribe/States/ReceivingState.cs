using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class ReceivingState : Core.IState {

		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;

		public IEnumerable<IEffectInvocation> OnEntry { get; }
		public IEnumerable<IEffectInvocation> OnExit { get; }
		public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e) {
			switch (e) {
				case Events.ReceiveSuccessEvent receiveSuccess:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() {
							Channels = receiveSuccess.Channels,
							ChannelGroups = receiveSuccess.ChannelGroups
						},
						new[] {
							new ReceiveMessagesInvocation() {
								Channels = receiveSuccess.Channels,
								ChannelGroups = receiveSuccess.ChannelGroups,
							},
						}
					);
				case Events.SubscriptionChangedEvent subscriptionChanged:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() {
							Channels = subscriptionChanged.Channels,
							ChannelGroups = subscriptionChanged.ChannelGroups,
						},
						new[] {
							new ReceiveMessagesInvocation() {
								Channels = subscriptionChanged.Channels,
								ChannelGroups = subscriptionChanged.ChannelGroups,
							},
						}
					);
				case Events.SubscriptionRestoredEvent subscriptionRestored:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new HandshakeFailedState() { 
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
				case Events.DisconnectEvent disconnect:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceiveStoppedState() {
							Channels = disconnect.Channels,
							ChannelGroups = disconnect.ChannelGroups
						},
						new[] {
							new EmitStatusInvocation() {
								Channels = disconnect.Channels,
								ChannelGroups = disconnect.ChannelGroups,
							},
						}
					);
				case Events.ReceiveFailureEvent receiveFailure:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceiveReconnectingState() {
							Channels = receiveFailure.Channels,
							ChannelGroups = receiveFailure.ChannelGroups
						},
						null
					);

				default: return null;
			}
		}
	}
}

