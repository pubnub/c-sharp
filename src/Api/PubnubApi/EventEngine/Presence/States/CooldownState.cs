using PubnubApi;
using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;
using System.Collections.Generic;
using System;

namespace PubnubApi.EventEngine.Presence.States
{
    public class CooldownState : APresenceState
    {
        public override IEnumerable<IEffectInvocation> OnEntry => new WaitInvocation() {
            Input = this.Input
        }.AsArray();
        public override IEnumerable<IEffectInvocation> OnExit => new CancelWaitInvocation().AsArray();

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
                Events.DisconnectEvent e => new StoppedState()
                {
                    Input = this.Input,
                }.With(new LeaveInvocation(){ Input = this.Input }),
                Events.TimesUpEvent e => new HeartbeatingState()
                {
                    Input = this.Input,
                },
                _ => null,
            };
        }
    }
}
