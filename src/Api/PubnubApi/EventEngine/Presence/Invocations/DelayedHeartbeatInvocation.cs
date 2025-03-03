using PubnubApi.EventEngine.Presence.Common;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class DelayedHeartbeatInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
		public int RetryCount { get; set; }
        public PNStatus Reason { get; set; }
        public virtual string Name { get; set; } = "DELAYED_HEARTBEAT";
        
        public override string ToString()
        {
            return $"Invocation : {Name}";
        }
    }
}
