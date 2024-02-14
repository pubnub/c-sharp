using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Core;

namespace PubnubApi.EventEngine.Subscribe.Invocations
{
    public class HeartbeatInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
    }
}
