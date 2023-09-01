using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Invocations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EventEngine.Presence.States
{
    public class HeartbeatCooldownState : PresenceState
    {
        public override IEnumerable<IEffectInvocation> OnEntry => new WaitInvocation()
            { Channels = this.Channels, ChannelGroups = this.ChannelGroups }.AsArray();
        public override IEnumerable<IEffectInvocation> OnExit { get; } = new CancelWaitInvocation().AsArray();
        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.LeftAllEvent leftAll => new HeartbeatInactiveState() 
                {
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                }, 

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
                }, 

                Events.StateSetEvent stateSet => new States.HeartbeatingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(stateSet.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(stateSet.ChannelGroups),
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.TimesUpEvent timesUp => new States.HeartbeatingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(timesUp.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(timesUp.ChannelGroups),
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },

                Events.DisconnectEvent disconnect => new States.HeartbeatStoppedState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(disconnect.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(disconnect.ChannelGroups),
                    ReconnectionConfiguration = this.ReconnectionConfiguration,
                },

                _ => null
            };
        }
    }
}
