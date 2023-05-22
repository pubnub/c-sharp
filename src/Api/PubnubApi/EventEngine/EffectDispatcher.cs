using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine
{
	public class EffectDispatcher
	{
		public enum DispatcherType
		{
			Entry,
			Exit,
			Managed
		}
		public IPubnubUnitTest PubnubUnitTest { get; set; }

		public Dictionary<EventType, IEffectInvocationHandler> effectActionMap;
		public EffectDispatcher()
		{
			effectActionMap = new Dictionary<EventType, IEffectInvocationHandler>();
		}

		public async void dispatch(EventType effect, ExtendedState stateContext)
		{
			IEffectInvocationHandler? handler;
			if (effectActionMap.TryGetValue(effect, out handler)) {
				if (handler != null)
				{
					await Task.Factory.StartNew(()=> handler.Start(stateContext, effect)).ConfigureAwait(false);
				}
			}
		}

		public async void dispatch(DispatcherType dispatchType, EventType eventType,List<EffectInvocation> effectInvocations, ExtendedState stateContext)
		{
			foreach (var effect in effectInvocations) {
				PubnubUnitTest?.EventTypeList?.Add(new KeyValuePair<string, string>("invocation", effect.Name));
				System.Diagnostics.Debug.WriteLine("Found effect " + effect.Effectype);
				if (dispatchType == DispatcherType.Exit)
                {
					await Task.Factory.StartNew(()=> effect.Handler?.Cancel()).ConfigureAwait(false);
				}
				else if (dispatchType == DispatcherType.Entry)
				{
					await Task.Factory.StartNew(()=> effect.Handler?.Start(stateContext, eventType)).ConfigureAwait(false);
				}
				else if (dispatchType == DispatcherType.Managed)
				{
					if (effect is EmitStatus)
					{
						((EmitStatus)effect).Announce();
					}
					else if (effect is EmitMessages<object>)
					{
						((EmitMessages<object>)effect).Announce<string>();
					}
					else if (effect is EmitMessages<string>)
					{
						((EmitMessages<string>)effect).Announce<string>();
					}
				}
			}

		}

		public void Register(EventType type, IEffectInvocationHandler handler)
		{
			effectActionMap.Add(type, handler);
		}
	}
}
