using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine
{
	public class EffectDispatcher
	{
		public IPubnubUnitTest PubnubUnitTest { get; set; }

		private Dictionary<EventType, IEffectInvocationHandler> effectInvocationActionMap;
		public EffectDispatcher()
		{
			effectInvocationActionMap = new Dictionary<EventType, IEffectInvocationHandler>();
		}

		public async void dispatch(EventType eventType,List<EffectInvocation> effectInvocations, ExtendedState stateContext)
		{
			if (effectInvocations == null || effectInvocations.Count == 0) { return; }
			foreach (var invocation in effectInvocations) {
				PubnubUnitTest?.EventTypeList?.Add(new KeyValuePair<string, string>("invocation", invocation.Name));
				System.Diagnostics.Debug.WriteLine("Found effect " + invocation.Effectype);
				IEffectInvocationHandler currentEffectInvocationhandler;
				if (effectInvocationActionMap.TryGetValue(invocation.Effectype, out currentEffectInvocationhandler))
				{
					if (invocation.IsManaged())
					{
						await Task.Factory.StartNew(()=> currentEffectInvocationhandler?.Start(stateContext, eventType)).ConfigureAwait(false);
					}
					else if (invocation.IsCancelling())
					{
						await Task.Factory.StartNew(()=> currentEffectInvocationhandler?.Cancel()).ConfigureAwait(false);
					}
					else
					{
						currentEffectInvocationhandler.Run(stateContext);
					}
				}
			}

		}

		public void Register(EventType type, IEffectInvocationHandler handler)
		{
			if (effectInvocationActionMap.ContainsKey(type))
			{
				throw new ArgumentException("EventType already exist");
			}
			effectInvocationActionMap.Add(type, handler);
		}
	}
}
