using System;
using System.Threading.Tasks;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
	public class EmitStatusEffectHandler: EffectHandler<EmitStatusInvocation>
	{
		private readonly Action<Pubnub, PNStatus> statusEmitterFunction;
		private readonly Pubnub pubnubInstance;

		public EmitStatusEffectHandler(Pubnub pn, Action<Pubnub, PNStatus> statusEmitter)
		{
			this.statusEmitterFunction = statusEmitter;
			this.pubnubInstance = pn;
		}

		public override Task Cancel() => Utils.EmptyTask;

		public override bool IsBackground(EmitStatusInvocation invocation) => false;

		public override async Task Run(EmitStatusInvocation invocation)
		{
			this.statusEmitterFunction(this.pubnubInstance, invocation.Status);
		}
	}
}
