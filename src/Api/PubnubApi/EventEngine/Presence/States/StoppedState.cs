using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;
using System.Collections.Generic;
using System;

namespace PubnubApi.EventEngine.Presence.States
{
    public class StoppedState : APresenceState
    {
        public override TransitionResult Transition(IEvent ev)
        {
            return ev switch 
            {
                Events.JoinedEvent e => new StoppedState()
                {
                    Input = e.Input != this.Input ? this.Input + e.Input : this.Input,
                },
                Events.LeftEvent e => HandleLeftEvent(e),
                Events.LeftAllEvent e => new InactiveState(),
                Events.ReconnectEvent e => new HeartbeatingState()
                {
                    Input = this.Input,
                },
                 _ => null,
            };
        }

        protected TransitionResult HandleLeftEvent(Events.LeftEvent e)
        {
            var newInput = this.Input - e.Input;

            return newInput.IsEmpty()
                ? (TransitionResult)new InactiveState()
                : (TransitionResult)new StoppedState()
                {
                    Input = newInput,
                };
        }
    }
    
}
