

using System.Threading.Tasks;
using PubnubApi.PubnubEventEngine.Subscribe.Invocations;

namespace PubnubApi.PubnubEventEngine.Subscribe.Effects {
	public class EmitMessagesEffect : Core.IEffectHandler<EmitMessagesInvocation> {
		public Task Run(EmitMessagesInvocation invocation) {
			throw new System.NotImplementedException();
		}

		public Task Cancel() {
			throw new System.NotImplementedException();
		}
	}
}