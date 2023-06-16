using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;

namespace PubnubApi.PubnubEventEngine.Subscribe.Invocations {
	internal class EmitMessagesInvocation : Core.IEffectInvocation {
		public List<object> messages;
	}

	internal class EmitStatusInvocation : Core.IEffectInvocation {
		
	}

	internal class HandshakeInvocation : Core.IEffectInvocation {
		public IEnumerable<string> channels;
		public IEnumerable<string> channelGroups;
		public SubscriptionCursor cursor;
	}

	internal class HandshakeCancelInvocation : Core.IEffectCancelInvocation { }
}