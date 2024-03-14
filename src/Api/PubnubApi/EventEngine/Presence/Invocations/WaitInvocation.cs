using PubnubApi.EventEngine.Presence.Common;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class WaitInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
    }
}
