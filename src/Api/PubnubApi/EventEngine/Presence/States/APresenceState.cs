using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;

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

            return newInput.IsEmpty()
                ? (TransitionResult)new InactiveState()
                : (TransitionResult)new HeartbeatingState()
                {
                    Input = newInput,
                };
        }
    }
}
