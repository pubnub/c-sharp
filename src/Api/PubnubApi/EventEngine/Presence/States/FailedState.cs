using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;
using System.Collections.Generic;

namespace PubnubApi.EventEngine.Presence.States
{
    public class FailedState : APresenceState
    {
        public PNStatus Reason { get; set; }

        // TODO: Dummy Invocation until we have real ones
        public override IEnumerable<IEffectInvocation> OnEntry => DummyInvocations();
        public override IEnumerable<IEffectInvocation> OnExit => DummyInvocations();

        // TODO: transitions
        public override TransitionResult Transition(IEvent e)
        {
            return e switch 
            {
                Events.JoinedEvent e => new HeartbeatingState()
                {
                    Input = e.Input != this.Input ? this.Input + e.Input : this.Input,
                },
                Events.LeftEvent e => () => {
                    var newInput = this.Input - e.Input;

                    return newInput.IsEmpty()
                        ? new InactiveState()
                        : new HeartbeatingState()
                        {
                            Input = newInput,
                        };
                },
                Events.LeftAllEvent => new InactiveState(),
                Events.ReconnectEvent => new HeartbeatingState()
                {
                    Input = this.Input,
                },
                Events.DisconnectEvent => new StoppedState()
                {
                    Input = this.Input,
                }, 
                _ => null,
            };
        }
    }
    
}
