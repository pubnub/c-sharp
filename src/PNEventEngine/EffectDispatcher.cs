using System.Collections.Generic;
using System.Threading.Tasks;

namespace PNEventEngine
{
	public class EffectDispatcher
	{
		public Dictionary<EffectType, IEffectHandler> effectActionMap;
		public EffectDispatcher()
		{
			effectActionMap = new Dictionary<EffectType, IEffectHandler>();
		}

		public async void dispatch(EffectType effect, ExtendedState stateContext)
		{
			IEffectHandler? handler;
			if (effectActionMap.TryGetValue(effect, out handler)) {
				if (handler != null)
				{
					await Task.Factory.StartNew(()=> handler.Start(stateContext)).ConfigureAwait(false);;
				}
			}
		}

		public void Register(EffectType type, IEffectHandler handler)
		{
			effectActionMap.Add(type, handler);
		}
	}
}
