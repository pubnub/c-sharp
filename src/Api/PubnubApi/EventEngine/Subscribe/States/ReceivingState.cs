using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class ReceivingState : Core.IState
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
        public SubscriptionCursor Cursor;

        public IEnumerable<IEffectInvocation> OnEntry => new ReceiveMessagesInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups, Cursor = this.Cursor }.AsArray();

        public IEnumerable<IEffectInvocation> OnExit { get; } = new CancelReceiveMessagesInvocation().AsArray();

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e)
        {
            return e switch
            {
                Events.ReceiveSuccessEvent receiveSuccess => new ReceivingState()
                {
                    Channels = receiveSuccess.Channels,
                    ChannelGroups = receiveSuccess.ChannelGroups,
                    Cursor = receiveSuccess.Cursor
                }.With(),

                Events.SubscriptionChangedEvent subscriptionChanged => new ReceivingState()
                {
                    Channels = subscriptionChanged.Channels,
                    ChannelGroups = subscriptionChanged.ChannelGroups,
                    Cursor = this.Cursor
                }.With(),

                Events.SubscriptionRestoredEvent subscriptionRestored => new ReceivingState()
                {
                    Channels = subscriptionRestored.Channels,
                    ChannelGroups = subscriptionRestored.ChannelGroups,
                    Cursor = subscriptionRestored.Cursor
                }.With(),

                Events.DisconnectEvent disconnect => new ReceiveStoppedState()
                {
                    Channels = disconnect.Channels,
                    ChannelGroups = disconnect.ChannelGroups,
                    Cursor = disconnect.Cursor
                }.With(new EmitStatusInvocation()
                {
                    Channels = disconnect.Channels,
                    ChannelGroups = disconnect.ChannelGroups,
                    StatusCategory = PNStatusCategory.PNDisconnectedCategory
                }),

                Events.ReceiveFailureEvent receiveFailure => new ReceiveReconnectingState()
                {
                    Channels = receiveFailure.Channels,
                    ChannelGroups = receiveFailure.ChannelGroups,
                    Cursor = receiveFailure.Cursor
                }.With(),

                _ => null
            };
        }
    }
}