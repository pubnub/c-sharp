using System;
using System.Threading.Tasks;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
	public class EmitStatusEffectHandler: IEffectHandler<EmitStatusInvocation>
	{
		private readonly Action<Pubnub, PNStatus> statusEmitterFunction;
		private readonly Pubnub pubnubInstance;

		public EmitStatusEffectHandler(Pubnub pn, Action<Pubnub, PNStatus> statusEmitter)
		{
			this.statusEmitterFunction = statusEmitter;
			this.pubnubInstance = pn;
		}

		public Task Cancel() => Utils.EmptyTask;

		public bool IsBackground(EmitStatusInvocation invocation) => false;

		public async Task Run(EmitStatusInvocation invocation)
		{
			this.statusEmitterFunction(this.pubnubInstance, invocation.Status);
		}
	}
}