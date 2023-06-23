using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;

namespace PubnubApi.PubnubEventEngine.Subscribe.Invocations {
	internal class EmitMessagesInvocation : Core.IEffectInvocation {
		public List<object> messages;
	}

	internal class EmitStatusInvocation : Core.IEffectInvocation {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
	}

	internal class HandshakeInvocation : Core.IEffectInvocation {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public Dictionary<string, string> InitialSubscribeQueryParams = new Dictionary<string, string>();
		public Dictionary<string, object> ExternalQueryParams = new Dictionary<string, object>();
	}
	
	internal class ReceiveMessagesInvocation : Core.IEffectInvocation 
	{ 
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
	}
	
	internal class CancelReceiveMessagesInvocation : ReceiveMessagesInvocation, Core.IEffectCancelInvocation { }

	internal class HandshakeCancelInvocation : HandshakeInvocation, Core.IEffectCancelInvocation { }

	//internal class ReconnectInvocation : Core.IEffectInvocation { }
	internal class HandshakeReconnectInvocation: Core.IEffectInvocation 
	{ 
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
	}

	internal class CancelHandshakeReconnectInvocation: HandshakeReconnectInvocation, Core.IEffectCancelInvocation { }
	
	internal class ReceiveReconnectInvocation: Core.IEffectInvocation 
	{ 
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
	}

	internal class CancelReceiveReconnectInvocation: ReceiveReconnectInvocation, Core.IEffectCancelInvocation { }
	//internal class CancelReconnectInvocation : ReconnectInvocation, Core.IEffectCancelInvocation { }
}