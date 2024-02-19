using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;
using System.Collections.Generic;
using System;

namespace PubnubApi.EventEngine.Presence.States
{
    public class HeartbeatingState : APresenceState
    {
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
                Events.LeftAllEvent e => new InactiveState(),
                Events.HeartbeatSuccessEvent e => new CooldownState()
                {
                    Input = this.Input,
                },
                Events.HeartbeatFailureEvent e => HandleHeartbeatFailureEvent(e), 
                Events.DisconnectEvent e => new StoppedState()
                {
                    Input = this.Input,
                }, 
                _ => null,
            };
        }

        private TransitionResult HandleHeartbeatFailureEvent(Events.HeartbeatFailureEvent e)
        {
            return e.Status.Category == PNStatusCategory.PNCancelledCategory
                ? (TransitionResult)null 
                : (TransitionResult)new ReconnectingState()
                {
                    Input = this.Input,
                    RetryCount = 1,
                    Reason = e.Status,
                };                
        }
    }
    
}
