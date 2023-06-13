using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine.Core {
	public interface IEffectHandler {
		Task Cancel();
	}
	
	public interface IEffectHandler<T> : IEffectHandler where T : IEffectInvocation {
		Task Run(T invocation);
	}
	
	public interface IEffectInvocation { }

	public interface IEffectCancelInvocation : IEffectInvocation { }

	public interface IEvent { };
}