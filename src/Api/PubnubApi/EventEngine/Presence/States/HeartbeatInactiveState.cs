using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EventEngine.Presence.States
{
    public class HeartbeatInactiveState : PresenceState
    {
        public override TransitionResult Transition(IEvent e)
        {
            return e switch
            {
                Events.JoinedEvent joined => new States.HeartbeatingState()
                {
                    Channels = (Channels ?? Enumerable.Empty<string>()).Union(joined.Channels),
                    ChannelGroups = (ChannelGroups ?? Enumerable.Empty<string>()).Union(joined.ChannelGroups),
                    ReconnectionConfiguration = this.ReconnectionConfiguration
                },
                _ => null
            };
        }
    }
}
