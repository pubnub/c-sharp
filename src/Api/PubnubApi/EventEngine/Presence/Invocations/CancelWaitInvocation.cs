using PubnubApi.EventEngine.Core;

namespace PubnubApi.EventEngine.Presence.Invocations
{
    public class CancelWaitInvocation : WaitInvocation, Core.IEffectCancelInvocation {
        public override string Name { get; set; } = "CANCEL_WAIT";
    }
}
