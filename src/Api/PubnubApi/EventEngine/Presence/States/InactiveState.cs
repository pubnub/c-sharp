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

        // TODO: Dummy Invocation until we have real ones
        public override IEnumerable<IEffectInvocation> OnEntry => new DummyInvocation().AsArray();
        public override IEnumerable<IEffectInvocation> OnExit => new DummyInvocation().AsArray();

        // TODO: transitions
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
