using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Subscribe.Invocations {
	internal class EmitMessagesInvocation : Core.IEffectInvocation {
		public List<object> messages;
	}
}