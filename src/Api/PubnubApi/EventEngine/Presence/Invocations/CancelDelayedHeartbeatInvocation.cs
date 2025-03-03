namespace PubnubApi.EventEngine.Presence.Invocations
{
	public class CancelDelayedHeartbeatInvocation : DelayedHeartbeatInvocation, Core.IEffectCancelInvocation { 
		public override string Name { get; set; } = "CANCEL_DELAYED_HEARTBEAT";
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}
}
