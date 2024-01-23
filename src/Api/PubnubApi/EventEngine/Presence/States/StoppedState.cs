using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;

namespace PubnubApi.EventEngine.Presence.States
{
    public class StoppedState : APresenceState
    {
       // TODO: Dummy Invocation until we have real ones
        public override IEnumerable<IEffectInvocation> OnEntry => DummyInvocations();
        public override IEnumerable<IEffectInvocation> OnExit => DummyInvocations();

        // TODO: transitions
        public override TransitionResult Transition(IEvent e)
        {
            return e switch 
            {
                Events.JoinedEvent e => new StoppedState()
                {
                    Input = e.Input != this.Input ? this.Input + e.Input : this.Input,
                },
                Events.LeftEvent e => () => {
                    var newInput = this.Input - e.Input;

                    return newInput.IsEmpty()
                        ? new InactiveState()
                        : new StoppedState()
                        {
                            Input = newInput,
                        };
                },
                Events.LeftAllEvent => new InactiveState(),
                Events.ReconnectEvent => new HeartbeatingState()
                {
                    Input = this.Input,
                },
                 _ => null,
            };
        }
    }
    
}
