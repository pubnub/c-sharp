using PubnubApi.EventEngine.Presence.Invocations;
using PubnubApi.EventEngine.Core;

namespace PubnubApi.EventEngine.Presence.States
{
    public class InactiveState : APresenceState
    {
        public InactiveState()
        {
            Channels = null;
            ChannelGroups = null;
        }

        // TODO: Dummy Invocation until we have real ones
        public override IEnumerable<IEffectInvocation> OnEntry => DummyInvocations();
        public override IEnumerable<IEffectInvocation> OnExit => DummyInvocations();

        // TODO: transitions
        public override TransitionResult Transition(Transition transition)
        {
            return e switch 
            {
                _ => null,
            };
        }
    }
    
}
