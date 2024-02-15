using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;
using System.Collections.Generic;
using System;

namespace PubnubApi.EventEngine.Presence.States
{
    public class FailedState : APresenceState
    {
        public PNStatus Reason { get; set; }

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
                    Input = e.Input != this.Input ? this.Input + e.Input : this.Input,
                },
                Events.LeftEvent e => HandleLeftEvent(e),
                Events.LeftAllEvent e => new InactiveState()
                    .With(new LeaveInvocation(){ Input = this.Input }),
                Events.ReconnectEvent e => new HeartbeatingState()
                {
                    Input = this.Input,
                }.With(new LeaveInvocation(){ Input = this.Input }),
                Events.DisconnectEvent e => new StoppedState()
                {
                    Input = this.Input,
                }, 
                _ => null,
            };
        }
    }
    
}
