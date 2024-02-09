using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.EventEngine.Core {
	public class EffectDispatcher {
        private readonly List<Task> runningHandlers = new List<Task>();
		// assumes 1 instance of handler - capable of managing itself
		private readonly Dictionary<System.Type, IEffectHandler> effectInvocationHandlerMap =
			new Dictionary<System.Type, IEffectHandler>();

		public event System.Action<IEffectInvocation> OnEffectDispatch;

		/// <summary>
		/// Dispatch an invocation i.e. call a registered effect handler.
		/// </summary>
		public async Task Dispatch(IEffectInvocation invocation) {
            runningHandlers.RemoveAll(t => t.IsCompleted || t.IsCanceled || t.IsFaulted);

			if (!effectInvocationHandlerMap.ContainsKey(invocation.GetType())) {
				throw new ArgumentException($"No handler for {invocation.GetType().Name} found.");
			}

			OnEffectDispatch?.Invoke(invocation);

			if (invocation is IEffectCancelInvocation) {
				await effectInvocationHandlerMap[invocation.GetType()].Cancel();
			} else
			{
				var handler = effectInvocationHandlerMap[invocation.GetType()];
				if (handler.IsBackground(invocation))
#if NET35 || NET40
                    runningHandlers.Add(handler.Run(invocation));
#else
					runningHandlers.Add(Task.Run(() => handler.Run(invocation)));
#endif
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

        ~EffectDispatcher()
        {
            foreach (var handler in effectInvocationHandlerMap.Values)
            {
                handler.Cancel().Wait();
            }
            foreach (var task in runningHandlers)
            {
                task.Dispose();
            }
        }
	}
}
