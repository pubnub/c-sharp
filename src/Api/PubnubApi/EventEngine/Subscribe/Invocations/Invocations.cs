using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Subscribe.Invocations {
	public class EmitMessagesInvocation : Core.IEffectInvocation {
		public List<object> messages;
	}
}