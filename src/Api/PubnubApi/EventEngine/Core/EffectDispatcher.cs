using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine.Core {
	internal class EffectDispatcher {
		// assumes 1 instance of handler - capable of managing itself
		private readonly Dictionary<System.Type, IEffectHandler> effectInvocationHandlerMap =
			new Dictionary<System.Type, IEffectHandler>();

		/// <summary>
		/// Dispatch an invocation i.e. call a registered effect handler.
		/// </summary>
		public async Task Dispatch<T>(T invocation) where T : IEffectInvocation {
			if (!effectInvocationHandlerMap.ContainsKey(invocation.GetType())) {
				throw new ArgumentException($"No handler for {invocation.GetType().Name} found.");
			}

			if (invocation is IEffectCancelInvocation) {
				await effectInvocationHandlerMap[invocation.GetType()].Cancel();
			} else {
				await ((IEffectHandler<T>)effectInvocationHandlerMap[invocation.GetType()]).Run(invocation);
			}
		}
		
		/// <summary>
		/// Assign a handler implementation to an invocation.
		/// </summary>
		public EffectDispatcher Register<TEffectInvocation, TEffectHandler>(TEffectHandler handler)
			where TEffectInvocation : IEffectInvocation
			where TEffectHandler : IEffectHandler<TEffectInvocation> {
			// TODO log
			// if (effectInvocationHandlerMap.ContainsKey(typeof(TEffectInvocation)))
			
			effectInvocationHandlerMap[typeof(TEffectInvocation)] = handler;
			return this;
		}
	}
}