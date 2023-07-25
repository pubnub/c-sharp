using System;
using System.Threading.Tasks;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
	public class EmitStatusEffectHandler: Core.IEffectHandler<EmitStatusInvocation>
	{
		public delegate void EmitStatusFunction(Pubnub PnReference, PNStatus status);

		public EmitStatusFunction StatusEmitter;

		public Pubnub PnReference;

		public EmitStatusEffectHandler(Pubnub pn, Action<Pubnub, PNStatus> statusAction)
		{
			this.StatusEmitter = new EmitStatusFunction(statusAction);
			this.PnReference = pn;
		}

		public Task Cancel()
		{
			return Task.FromResult(0);
		}

		bool IEffectHandler<EmitStatusInvocation>.IsBackground(EmitStatusInvocation invocation)
		{
			return false;
		}

		Task IEffectHandler<EmitStatusInvocation>.Run(EmitStatusInvocation invocation)
		{
			this.StatusEmitter(this.PnReference, invocation.Status);
			return Task.FromResult(0);
		}
	}
}

