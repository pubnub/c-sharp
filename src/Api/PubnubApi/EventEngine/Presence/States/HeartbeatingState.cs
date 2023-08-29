using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Invocations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EventEngine.Presence.States
{
    public class HeartbeatingState : PresenceState
    {
        public override IEnumerable<IEffectInvocation> OnEntry => new HeartbeatInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups }.AsArray();

        public override IEnumerable<IEffectInvocation> OnExit { get; } = new CancelHeartbeatInvocation().AsArray();

        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.LeftAllEvent leftAll => new HeartbeatInactiveState() 
                {
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)), //TODO: Do we need PNLeaveCategory?,

                Events.JoinedEvent joined => new States.HeartbeatingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(joined.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(joined.ChannelGroups),
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.LeftEvent left => new States.HeartbeatingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(left.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(left.ChannelGroups),
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)), //TODO: Do we need PNLeaveCategory?

                Events.StateSetEvent stateSet => new States.HeartbeatingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(stateSet.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(stateSet.ChannelGroups),
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.HeartbeatSuccessEvent heartbeatSuccess => new States.HeartbeatCooldownState()
                {
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.HeartbeatFailureEvent heartbeatFailure => new States.HeartbeatReconnectingState()
                {
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.DisconnectEvent disconnect => new States.HeartbeatStoppedState()
                {
                    ReconnectionConfiguration = this.ReconnectionConfiguration,
                }.With(new EmitStatusInvocation(PNStatusCategory.PNDisconnectedCategory)), //TODO: Do we need PNLeaveCategory?,

                _ => null
            };
        }
    }
}
