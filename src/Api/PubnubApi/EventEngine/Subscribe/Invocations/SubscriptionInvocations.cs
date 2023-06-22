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
		public Dictionary<string, string> initialSubscribeQueryParams = new Dictionary<string, string>();
		public Dictionary<string, object> externalQueryParams = new Dictionary<string, object>();
	}
	
	internal class ReceiveMessagesInvocation : Core.IEffectInvocation { }
	
	internal class CancelReceiveMessagesInvocation : ReceiveMessagesInvocation, Core.IEffectCancelInvocation { }

	internal class HandshakeCancelInvocation : HandshakeInvocation, Core.IEffectCancelInvocation { }

	internal class ReconnectInvocation : Core.IEffectInvocation { }
	
	internal class CancelReconnectInvocation : ReconnectInvocation, Core.IEffectCancelInvocation { }
}