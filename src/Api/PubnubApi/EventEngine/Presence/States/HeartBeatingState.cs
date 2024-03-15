using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;
using System.Collections.Generic;
using System;

namespace PubnubApi.EventEngine.Presence.States
{
    public class HeartbeatingState : APresenceState
    {
        public override IEnumerable<IEffectInvocation> OnEntry => new HeartbeatInvocation() {
            Input = this.Input
        }.AsArray();

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
                Events.HeartbeatSuccessEvent e => new CooldownState()
                {
                    Input = this.Input,
                },
                Events.HeartbeatFailureEvent e => HandleHeartbeatFailureEvent(e), 
                Events.DisconnectEvent e => new StoppedState()
                {
                    Input = this.Input,
                }.With(new LeaveInvocation(){ Input = this.Input }), 
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
                    RetryCount = 0,
                    Reason = e.Status,
                };                
        }
    }
    
}
