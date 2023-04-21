using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine
{
	public class EffectDispatcher
	{
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
					await Task.Factory.StartNew(()=> handler.Start(stateContext)).ConfigureAwait(false);;
				}
			}
		}

		public void Register(EventType type, IEffectInvocationHandler handler)
		{
			effectActionMap.Add(type, handler);
		}
	}
}
