using PubnubApi.EventEngine.Presence.Common;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class HeartbeatInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
        public virtual string Name { get; set; } = "HEARTBEAT";
        public override string ToString()
        {
            return $"Invocation : {Name}";
        }
    }
}
