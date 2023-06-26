using System;
using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.States
{
    internal class ReceiveFailedState : Core.IState
    {
        public IEnumerable<string> Channels;
        public IEnumerable<string> ChannelGroups;
        public SubscriptionCursor Cursor;

        public IEnumerable<IEffectInvocation> OnEntry { get; }
        public IEnumerable<IEffectInvocation> OnExit { get; }

        public Tuple<Core.IState, IEnumerable<IEffectInvocation>> Transition(IEvent e)
        {
            switch (e)
            {
                case Events.SubscriptionChangedEvent subscriptionChanged:
                    return new ReceivingState()
                    {
                        Channels = subscriptionChanged.Channels,
                        ChannelGroups = subscriptionChanged.ChannelGroups,
                        Cursor = this.Cursor
                    }.With(null);
                case Events.ReconnectEvent reconnect:
                    return new ReceivingState()
                    {
                        Channels = reconnect.Channels,
                        ChannelGroups = reconnect.ChannelGroups,
                        Cursor = reconnect.Cursor
                    }.With(null);
                case Events.SubscriptionRestoredEvent subscriptionRestored:
                    return new ReceivingState()
                    {
                        Channels = subscriptionRestored.Channels,
                        ChannelGroups = subscriptionRestored.ChannelGroups,
                        Cursor = subscriptionRestored.Cursor
                    }.With(null);
                default: return null;
            }
        }
    }
}