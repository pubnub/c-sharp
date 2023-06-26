using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class ReceiveStoppedState : Core.IState {

		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;


		public IEnumerable<IEffectInvocation> OnEntry { get; }
		public IEnumerable<IEffectInvocation> OnExit { get; }
		public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e) {
			switch (e) {
				case Events.SubscriptionChangedEvent subscriptionChanged:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() {
							Channels = subscriptionChanged.Channels,
							ChannelGroups = subscriptionChanged.ChannelGroups,
							Cursor = subscriptionChanged.Cursor,
						},
						new[] {
							new ReceiveMessagesInvocation() {
								Channels = subscriptionChanged.Channels,
								ChannelGroups = subscriptionChanged.ChannelGroups,
								Cursor = subscriptionChanged.Cursor,
							},
						}
					);
				case Events.ReconnectEvent reconnect:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() {
							Channels = reconnect.Channels,
							ChannelGroups = reconnect.ChannelGroups,
							Cursor = reconnect.Cursor
						},
						new[] {
							new ReceiveMessagesInvocation() {
								Channels = reconnect.Channels,
								ChannelGroups = reconnect.ChannelGroups,
								Cursor = reconnect.Cursor
							},
						}
					);
				case Events.SubscriptionRestoredEvent subscriptionRestored:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() { 
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

				default: return null;
			}
		}
	}
}
