using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;
using System.Collections.Generic;
using System;

namespace PubnubApi.EventEngine.Presence.States
{
    public class ReconnectingState : APresenceState
    {
        public int RetryCount { get; set; }
        public PNStatus Reason { get; set; }

        public override IEnumerable<IEffectInvocation> OnEntry => new DelayedHeartbeatInvocation()
        {
            Input = this.Input,
            RetryCount = this.RetryCount,
            Reason = Reason
        }.AsArray();
        public override IEnumerable<IEffectInvocation> OnExit => new CancelDelayedHeartbeatInvocation().AsArray();

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
                Events.HeartbeatFailureEvent e => new ReconnectingState() {
                    Input = this.Input,
                    RetryCount = RetryCount + 1,
                    Reason = e.Status
                },
                Events.HeartbeatGiveUpEvent e => new FailedState()
                {
                    Input = this.Input,
                    Reason = this.Reason,
                },
                Events.DisconnectEvent e => new StoppedState()
                {
                    Input = this.Input,
                }.With(new LeaveInvocation(){ Input = this.Input }), 
                _ => null,
            };
        }
    }
    
}
