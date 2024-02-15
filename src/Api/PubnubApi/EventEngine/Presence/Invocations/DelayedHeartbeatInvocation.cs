using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Core;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class DelayedHeartbeatInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
        public int Attempts { get; set; }
        public PNStatus Reason { get; set; }
    }
}
