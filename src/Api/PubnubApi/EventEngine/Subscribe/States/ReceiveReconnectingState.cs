using System;
using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.States
{
    internal class ReceiveReconnectingState : Core.State
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
		public PNReconnectionPolicy ReconnectionPolicy;
		public int MaximumReconnectionRetries;
		public int AttemptedRetries;
		public SubscriptionCursor Cursor;

        public override IEnumerable<IEffectInvocation> OnEntry => new ReceiveReconnectInvocation()
            {
                Channels = this.Channels,
                ChannelGroups = this.ChannelGroups,
                Cursor = this.Cursor,
                ReconnectionPolicy = this.ReconnectionPolicy,
                MaximumReconnectionRetries = this.MaximumReconnectionRetries,
                AttemptedRetries = this.AttemptedRetries
            }.AsArray();

        public override IEnumerable<IEffectInvocation> OnExit { get; } =
            new CancelReceiveReconnectInvocation().AsArray();

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.SubscriptionChangedEvent subscriptionChanged => new ReceivingState()
                {
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
                    Cursor = this.Cursor
                },

                Events.DisconnectEvent disconnect => new ReceiveStoppedState()
                {
                    Channels = disconnect.Channels,
                    ChannelGroups = disconnect.ChannelGroups,
                    Cursor = disconnect.Cursor
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)),

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor
                },

                Events.ReceiveReconnectSuccessEvent receiveReconnectSuccess => new ReceivingState()
                {
                    Channels = receiveReconnectSuccess.Channels,
                    ChannelGroups = receiveReconnectSuccess.ChannelGroups,
                    Cursor = receiveReconnectSuccess.Cursor
                }.With(new EmitStatusInvocation(receiveReconnectSuccess.Status)),

                Events.ReceiveReconnectFailureEvent receiveReconnectFailure => new ReceiveReconnectingState()
                {
                    Channels = this.Channels,
                    ChannelGroups = this.ChannelGroups,
                    Cursor = this.Cursor,
                    AttemptedRetries = this.AttemptedRetries,
                    MaximumReconnectionRetries = this.MaximumReconnectionRetries,
                    ReconnectionPolicy = this.ReconnectionPolicy
                }.With(new EmitStatusInvocation(receiveReconnectFailure.Status)),

                Events.ReceiveReconnectGiveUpEvent receiveReconnectGiveUp => new ReceiveFailedState()
                {
                    Channels = receiveReconnectGiveUp.Channels,
                    ChannelGroups = receiveReconnectGiveUp.ChannelGroups,
                    Cursor = receiveReconnectGiveUp.Cursor
                }.With(new EmitStatusInvocation(receiveReconnectGiveUp.Status)),

                _ => null
            };
        }
    }
}