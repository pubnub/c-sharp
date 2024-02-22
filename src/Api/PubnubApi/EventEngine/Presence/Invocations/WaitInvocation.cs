using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Core;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class WaitInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
        public long HeartbeatInterval { get; set; }
    }
}
