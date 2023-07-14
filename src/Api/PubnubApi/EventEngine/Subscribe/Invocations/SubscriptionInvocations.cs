using System.Collections.Generic;
using PubnubApi.PubnubEventEngine.Core;

namespace PubnubApi.PubnubEventEngine.Subscribe.Invocations {
	internal class EmitMessagesInvocation : Core.IEffectInvocation {
		public IEnumerable<PNMessageResult<object>> Messages;

		public EmitMessagesInvocation(IEnumerable<PNMessageResult<object>> messages)
		{
			this.Messages = messages;
		}
	}

	internal class EmitStatusInvocation : Core.IEffectInvocation {
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

	internal class HandshakeInvocation : Core.IEffectInvocation {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		// TODO if we need these, figure out how to pass them.
		public Dictionary<string, string> InitialSubscribeQueryParams = new Dictionary<string, string>();
		public Dictionary<string, object> ExternalQueryParams = new Dictionary<string, object>();
	}
	
	internal class ReceiveMessagesInvocation : Core.IEffectInvocation 
	{ 
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor  Cursor;
	}
	
	internal class CancelReceiveMessagesInvocation : ReceiveMessagesInvocation, Core.IEffectCancelInvocation { }

	internal class CancelHandshakeInvocation : HandshakeInvocation, Core.IEffectCancelInvocation { }

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
		public SubscriptionCursor Cursor;
	}

	internal class CancelReceiveReconnectInvocation: ReceiveReconnectInvocation, Core.IEffectCancelInvocation { }

	internal class LeaveInvocation: Core.IEffectInvocation 
	{ 
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
	}
	internal class CancelLeaveInvocation : LeaveInvocation, Core.IEffectCancelInvocation { }
}