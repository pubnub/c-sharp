using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class ReceivingState : Core.IState {

		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor  Cursor;

		public IEnumerable<IEffectInvocation> OnEntry { get; }
		public IEnumerable<IEffectInvocation> OnExit { get; }
		public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e) {
			switch (e) {
				case Events.ReceiveSuccessEvent receiveSuccess:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() {
							Channels = receiveSuccess.Channels,
							ChannelGroups = receiveSuccess.ChannelGroups,
							Cursor = receiveSuccess.Cursor
						},
						new[] {
							new ReceiveMessagesInvocation() {
								Channels = receiveSuccess.Channels,
								ChannelGroups = receiveSuccess.ChannelGroups,
								Cursor = receiveSuccess.Cursor
							},
						}
					);
				case Events.SubscriptionChangedEvent subscriptionChanged:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() {
							Channels = subscriptionChanged.Channels,
							ChannelGroups = subscriptionChanged.ChannelGroups,
							Cursor = subscriptionChanged.Cursor
						},
						new[] {
							new ReceiveMessagesInvocation() {
								Channels = subscriptionChanged.Channels,
								ChannelGroups = subscriptionChanged.ChannelGroups,
								Cursor = subscriptionChanged.Cursor
							},
						}
					);
				case Events.SubscriptionRestoredEvent subscriptionRestored:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new HandshakeFailedState() { 
							Channels = subscriptionRestored.Channels,
							ChannelGroups = subscriptionRestored.ChannelGroups,
							Cursor = subscriptionRestored.Cursor
						},
						new[] {
							new ReceiveMessagesInvocation() {
								Channels = subscriptionRestored.Channels,
								ChannelGroups = subscriptionRestored.ChannelGroups,
								Cursor = subscriptionRestored.Cursor
							},
						}
					);
				case Events.DisconnectEvent disconnect:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceiveStoppedState() {
							Channels = disconnect.Channels,
							ChannelGroups = disconnect.ChannelGroups,
							Cursor = disconnect.Cursor
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
							ChannelGroups = receiveFailure.ChannelGroups,
							Cursor = receiveFailure.Cursor
						},
						null
					);

				default: return null;
			}
		}
	}
}

