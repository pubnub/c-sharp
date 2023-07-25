using System;
using System.Threading.Tasks;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
	public class EmitStatusEffectHandler: Core.IEffectHandler<EmitStatusInvocation>
	{
		private readonly Action<Pubnub, PNStatus> statusEmitterFunction;
		private readonly Pubnub pubnubInstance;

		public Pubnub PnReference;

		public EmitStatusEffectHandler(Pubnub pn, Action<Pubnub, PNStatus> statusEmitter)
		{
			this.statusEmitterFunction = statusEmitter;
			this.PnReference = pn;
		}

		public Task Cancel() => Utils.EmptyTask;

		bool IEffectHandler<EmitStatusInvocation>.IsBackground(EmitStatusInvocation invocation) => false;

		Task IEffectHandler<EmitStatusInvocation>.Run(EmitStatusInvocation invocation)
		{
			this.statusEmitterFunction(this.PnReference, invocation.Status);
			return Utils.EmptyTask;
		}
	}
}

