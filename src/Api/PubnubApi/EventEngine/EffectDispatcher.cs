using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.PubnubEventEngine
{
	public class EffectDispatcher
	{
		public Dictionary<EffectInvocationType, IEffectInvocationHandler> effectActionMap;
		public EffectDispatcher()
		{
			effectActionMap = new Dictionary<EffectInvocationType, IEffectInvocationHandler>();
		}

		public async void dispatch(EffectInvocationType effect, ExtendedState stateContext)
		{
			IEffectInvocationHandler? handler;
			if (effectActionMap.TryGetValue(effect, out handler)) {
				if (handler != null)
				{
					await Task.Factory.StartNew(()=> handler.Start(stateContext)).ConfigureAwait(false);;
				}
			}
		}

		public void Register(EffectInvocationType type, IEffectInvocationHandler handler)
		{
			effectActionMap.Add(type, handler);
		}
	}
}
