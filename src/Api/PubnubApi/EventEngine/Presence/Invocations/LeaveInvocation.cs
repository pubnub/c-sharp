using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Core;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class LeaveInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
    }
}
