using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.EventEngine.Core {
	public class EffectDispatcher {
		// assumes 1 instance of handler - capable of managing itself
		private readonly Dictionary<System.Type, IEffectHandler> effectInvocationHandlerMap =
			new Dictionary<System.Type, IEffectHandler>();

		private PubnubLogModule logger;
		
		public event System.Action<IEffectInvocation> OnEffectDispatch;

		public EffectDispatcher(PubnubLogModule logModule)
		{
			logger = logModule;
		}

		/// <summary>
		/// Dispatch an invocation i.e. call a registered effect handler.
		/// </summary>
		public async Task Dispatch(IEffectInvocation invocation) {
			if (!effectInvocationHandlerMap.ContainsKey(invocation.GetType())) {
				throw new ArgumentException($"No handler for {invocation.GetType().Name} found.");
			}
			
			OnEffectDispatch?.Invoke(invocation);

			if (invocation is IEffectCancelInvocation) {
				await effectInvocationHandlerMap[invocation.GetType()].Cancel().ConfigureAwait(false);
			} else
			{
				var handler = effectInvocationHandlerMap[invocation.GetType()];
				if (handler.IsBackground(invocation))
					FireAndForget(handler, invocation);
				else
					await handler.Run(invocation).ConfigureAwait(false);
			}
		}

		void FireAndForget(IEffectHandler handler, IEffectInvocation invocation)
		{
			handler.Run(invocation).ContinueWith(t =>
			{
				if (t.Exception != null)
				{
					logger.Error($"Error occured when trying to run effect handler: {t.Exception.Message}");
				}
			}, TaskContinuationOptions.OnlyOnFaulted);
		}

		/// <summary>
		/// Assign a handler implementation to an invocation.
		/// </summary>
		public EffectDispatcher Register<TEffectInvocation, TEffectHandler>(TEffectHandler handler)
			where TEffectInvocation : IEffectInvocation
			where TEffectHandler : IEffectHandler<TEffectInvocation> {
			// if (effectInvocationHandlerMap.ContainsKey(typeof(TEffectInvocation)))

			effectInvocationHandlerMap[typeof(TEffectInvocation)] = handler;
			return this;
		}
	}
}