using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;

namespace PubnubApi.EventEngine.Presence.States
{
    public class HeartbeatingState : APresenceState
    {
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
                Events.HeartbeatFailureEvent e => () => {
                    // Request cancellation shouldn't cause any transition because there
                    // will be another event after this. 
                    return e.Status.Category == PNStatusCategory.PNCancelledCategory
                        ? null 
                        : new ReconnectingState()
                        {
                            Input = this.Input,
                            RetryCount = 1,
                            Reason = e.Status,
                        };                
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
