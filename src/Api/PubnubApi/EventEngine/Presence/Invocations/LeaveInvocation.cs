using PubnubApi.EventEngine.Presence.Common;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class LeaveInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
    }
}
