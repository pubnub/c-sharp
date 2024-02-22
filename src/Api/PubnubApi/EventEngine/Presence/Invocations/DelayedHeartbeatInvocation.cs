using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Context;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class DelayedHeartbeatInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
		public ReconnectionConfiguration ReconnectionConfiguration;
		public int AttemptedRetries;
        public PNStatus Reason { get; set; }
    }
}
