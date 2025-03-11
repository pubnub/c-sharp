using PubnubApi.EventEngine.Presence.Common;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class WaitInvocation : Core.IEffectInvocation
    {
        public PresenceInput Input { get; set; }
        public virtual string Name { get; set; } = "WAIT";
        public override string ToString()
        {
            return $"Invocation : {Name}";
        }
    }
}
