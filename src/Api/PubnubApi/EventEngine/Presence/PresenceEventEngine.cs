using System;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
namespace PubnubApi.EventEngine.Presence
{
	public class PresenceEventEngine : Engine
	{
		public PresenceEventEngine(PNConfiguration pnConfiguration, HeartbeatOperation heartbeatOperation, LeaveOperation leaveOperation)
		{
			var heartbeatEffectHandler = new Effects.HeartbeatEffectHandler(heartbeatOperation, EventQueue);
			dispatcher.Register<Invocations.HeartbeatInvocation, Effects.HeartbeatEffectHandler>(heartbeatEffectHandler);

			var delayedHeartbeatEffectHandler = new Effects.DelayedHeartbeatEffectHandler(pnConfiguration, heartbeatOperation, EventQueue);
			dispatcher.Register<Invocations.DelayedHeartbeatInvocation, Effects.DelayedHeartbeatEffectHandler>(delayedHeartbeatEffectHandler);
			dispatcher.Register<Invocations.CancelDelayedHeartbeatInvocation, Effects.DelayedHeartbeatEffectHandler>(delayedHeartbeatEffectHandler);

			var waitEffectHandler = new Effects.WaitEffectHandler(pnConfiguration, EventQueue);
			dispatcher.Register<Invocations.WaitInvocation, Effects.WaitEffectHandler>(waitEffectHandler);
			dispatcher.Register<Invocations.CancelWaitInvocation, Effects.WaitEffectHandler>(waitEffectHandler);

			var leaveEffectHandler = new Effects.LeaveEffectHandler(leaveOperation);
			dispatcher.Register<Invocations.LeaveInvocation, Effects.LeaveEffectHandler>(leaveEffectHandler);

			currentState = new States.InactiveState();
		}
	}
}

