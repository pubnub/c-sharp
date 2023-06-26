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
            switch (e)
            {
                case Events.ReceiveSuccessEvent receiveSuccess:
                    return new ReceivingState()
                    {
                        Channels = receiveSuccess.Channels,
                        ChannelGroups = receiveSuccess.ChannelGroups,
                        Cursor = receiveSuccess.Cursor
                    }.With(null);
                case Events.SubscriptionChangedEvent subscriptionChanged:
                    return new ReceivingState()
                    {
                        Channels = subscriptionChanged.Channels,
                        ChannelGroups = subscriptionChanged.ChannelGroups,
                        Cursor = this.Cursor
                    }.With();
                case Events.SubscriptionRestoredEvent subscriptionRestored:
                    return new ReceivingState()
                    {
                        Channels = subscriptionRestored.Channels,
                        ChannelGroups = subscriptionRestored.ChannelGroups,
                        Cursor = subscriptionRestored.Cursor
                    }.With();
                case Events.DisconnectEvent disconnect:
                    return new ReceiveStoppedState()
                    {
                        Channels = disconnect.Channels,
                        ChannelGroups = disconnect.ChannelGroups,
                        Cursor = disconnect.Cursor
                    }.With(
                        new EmitStatusInvocation()
                        {
                            Channels = disconnect.Channels,
                            ChannelGroups = disconnect.ChannelGroups,
                            StatusCategory = PNStatusCategory.PNDisconnectedCategory
                        }
                    );
                case Events.ReceiveFailureEvent receiveFailure:
                    return new ReceiveReconnectingState()
                    {
                        Channels = receiveFailure.Channels,
                        ChannelGroups = receiveFailure.ChannelGroups,
                        Cursor = receiveFailure.Cursor
                    }.With();
                default: return null;
            }
        }
    }
}