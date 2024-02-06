using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.EventEngine.Core {
	public class EffectDispatcher {
        private int _dispatchCount = 0;
        private string _log = "";
		// assumes 1 instance of handler - capable of managing itself
		private readonly Dictionary<System.Type, IEffectHandler> effectInvocationHandlerMap =
			new Dictionary<System.Type, IEffectHandler>();

		public event System.Action<IEffectInvocation> OnEffectDispatch;

		/// <summary>
		/// Dispatch an invocation i.e. call a registered effect handler.
		/// </summary>
		public async Task Dispatch(IEffectInvocation invocation) {
			if (!effectInvocationHandlerMap.ContainsKey(invocation.GetType())) {
				throw new ArgumentException($"No handler for {invocation.GetType().Name} found.");
			}

            this._dispatchCount++;
            this._log += $"Dispatched {invocation.GetType().Name}\n";

            if (this._dispatchCount >= 1000) {
                throw new Exception(this._log);
            }
			
			OnEffectDispatch?.Invoke(invocation);

			if (invocation is IEffectCancelInvocation) {
				await effectInvocationHandlerMap[invocation.GetType()].Cancel();
			} else
			{
				var handler = effectInvocationHandlerMap[invocation.GetType()];
				if (handler.IsBackground(invocation))
					handler.Run(invocation).Start();
				else
					await handler.Run(invocation);
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
