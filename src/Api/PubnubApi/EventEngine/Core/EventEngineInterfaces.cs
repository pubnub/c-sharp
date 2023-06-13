using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine.Core {
	internal interface IEffectHandler {
		Task Cancel();
	}
	
	internal interface IEffectHandler<T> : IEffectHandler where T : IEffectInvocation {
		Task Run(T invocation);
	}
	
	internal interface IEffectInvocation { }

	internal interface IEffectCancelInvocation : IEffectInvocation { }

	internal interface IEvent { };
}