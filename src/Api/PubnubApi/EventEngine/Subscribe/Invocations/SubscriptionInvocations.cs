using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Context;

namespace PubnubApi.EventEngine.Subscribe.Invocations {
	public class EmitMessagesInvocation : Core.IEffectInvocation {
		public ReceivingResponse<string> Messages;

		public EmitMessagesInvocation(ReceivingResponse<string> messages)
		{
			this.Messages = messages;
		}
	}

	public class EmitStatusInvocation : Core.IEffectInvocation {
		// TODO merge status variables into one?
		public PNStatusCategory StatusCategory;
		public PNStatus Status;

		public EmitStatusInvocation(PNStatus status)
		{
			this.Status = status;
		}

		public EmitStatusInvocation(PNStatusCategory category)
		{
			this.StatusCategory = category;
			this.Status = new PNStatus()
			{
				Category = category,
			};
		}
	}

	public class HandshakeInvocation : Core.IEffectInvocation {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		// TODO if we need these, figure out how to pass them.
		public Dictionary<string, string> InitialSubscribeQueryParams = new Dictionary<string, string>();
		public Dictionary<string, object> ExternalQueryParams = new Dictionary<string, object>();
	}
	
	public class ReceiveMessagesInvocation : Core.IEffectInvocation 
	{ 
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor  Cursor;
		public Dictionary<string, string> InitialSubscribeQueryParams = new Dictionary<string, string>();
		public Dictionary<string, object> ExternalQueryParams = new Dictionary<string, object>();
	}
	
	public class CancelReceiveMessagesInvocation : ReceiveMessagesInvocation, Core.IEffectCancelInvocation { }

	public class CancelHandshakeInvocation : HandshakeInvocation, Core.IEffectCancelInvocation { }

	public class HandshakeReconnectInvocation: HandshakeInvocation
	{
		public ReconnectionConfiguration ReconnectionConfiguration;
		public int AttemptedRetries;
	}

	public class CancelHandshakeReconnectInvocation: HandshakeReconnectInvocation, Core.IEffectCancelInvocation { }
	
	public class ReceiveReconnectInvocation: ReceiveMessagesInvocation 
	{ 
		public ReconnectionConfiguration ReconnectionConfiguration;
		public int AttemptedRetries;
	}

	public class CancelReceiveReconnectInvocation: ReceiveReconnectInvocation, Core.IEffectCancelInvocation { }
}