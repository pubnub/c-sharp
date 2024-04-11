using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Core;
using System.Collections.Generic;
using System;

namespace PubnubApi.EventEngine.Presence.States
{
    public class InactiveState : APresenceState
    {
        public InactiveState()
        {
            Input = new PresenceInput();
        }

        public override TransitionResult Transition(IEvent ev)
        {
            return ev switch 
            {
                Events.JoinedEvent e => new HeartbeatingState()
                {
                    Input = e.Input,
                },
                _ => null,
            };
        }
    }
    
}
