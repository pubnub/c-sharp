using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;
using System.Collections.Generic;
using System;

namespace PubnubApi.EventEngine.Presence.States
{
    public class StoppedState : APresenceState
    {
        // TODO: Dummy Invocation until we have real ones
        public override IEnumerable<IEffectInvocation> OnEntry => new DummyInvocation().AsArray();
        public override IEnumerable<IEffectInvocation> OnExit => new DummyInvocation().AsArray();

        // TODO: transitions
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
