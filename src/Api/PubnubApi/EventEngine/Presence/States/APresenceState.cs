using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Invocations;

namespace PubnubApi.EventEngine.Presence.States
{
    public abstract class APresenceState : Core.State
    {
        public PresenceInput Input { get; set; }

        public bool IsEmpty()
        {
            return Input.IsEmpty();
        }

        protected TransitionResult HandleLeftEvent(Events.LeftEvent e) 
        {
            var newInput = this.Input - e.Input;

            State state = newInput.IsEmpty()
                ? new InactiveState()
                : new HeartbeatingState()
                {
                    Input = newInput,
                };

            return state.With(new LeaveInvocation(){ Input = e.Input });
        }
    }
}
