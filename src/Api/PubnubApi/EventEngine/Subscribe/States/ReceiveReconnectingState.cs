using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States {
	internal class ReceiveReconnectingState : Core.IState {

		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;

		public IEnumerable<IEffectInvocation> OnEntry { get; }
		public IEnumerable<IEffectInvocation> OnExit { get; }
		public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e) {
			switch (e) {
				case Events.SubscriptionChangedEvent subscriptionChanged:
					return new Tuple<Core.IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() {
							Channels = subscriptionChanged.Channels,
							ChannelGroups = subscriptionChanged.ChannelGroups
						},
						new[] {
							new ReceiveMessagesInvocation() {
								Channels = subscriptionChanged.Channels,
								ChannelGroups = subscriptionChanged.ChannelGroups,
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
				case Events.ReceiveReconnectSuccessEvent receiveReconnectSuccess:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new ReceivingState() { 
							Channels = receiveReconnectSuccess.Channels,
							ChannelGroups = receiveReconnectSuccess.ChannelGroups
						},
						new IEffectInvocation[] {
							new EmitStatusInvocation() {
								Channels = receiveReconnectSuccess.Channels,
								ChannelGroups = receiveReconnectSuccess.ChannelGroups,
							},
							new ReceiveMessagesInvocation() {
								Channels = receiveReconnectSuccess.Channels,
								ChannelGroups = receiveReconnectSuccess.ChannelGroups,
							}
						}
					);
				case Events.ReceiveReconnectFailureEvent receiveReconnectFailure:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new ReceiveReconnectingState() { 
							Channels = receiveReconnectFailure.Channels,
							ChannelGroups = receiveReconnectFailure.ChannelGroups
						},
						new[]
						{
							new ReceiveReconnectInvocation()
							{
								Channels = receiveReconnectFailure.Channels,
								ChannelGroups = receiveReconnectFailure.ChannelGroups,
							}
						}
						);
				case Events.ReceiveReconnectGiveUpEvent receiveReconnectGiveUp:
					return new Tuple<IState, IEnumerable<IEffectInvocation>>(
						new ReceiveFailedState() { 
							Channels = receiveReconnectGiveUp.Channels,
							ChannelGroups = receiveReconnectGiveUp.ChannelGroups
						},
						null
					);

				default: return null;
			}
		}
	}
}


