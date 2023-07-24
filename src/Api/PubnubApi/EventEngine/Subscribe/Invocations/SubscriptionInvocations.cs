﻿using System.Collections.Generic;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.Invocations {
	internal class EmitMessagesInvocation : Core.IEffectInvocation {
		public ReceivingResponse<string> Messages;

		public EmitMessagesInvocation(ReceivingResponse<string> messages)
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
		public Dictionary<string, string> InitialSubscribeQueryParams = new Dictionary<string, string>();
		public Dictionary<string, object> ExternalQueryParams = new Dictionary<string, object>();
	}
	
	internal class CancelReceiveMessagesInvocation : ReceiveMessagesInvocation, Core.IEffectCancelInvocation { }

	internal class CancelHandshakeInvocation : HandshakeInvocation, Core.IEffectCancelInvocation { }

	internal class HandshakeReconnectInvocation: HandshakeInvocation
	{
		public int AttemptedRetries;
		public int MaxConnectionRetry;
		public PNReconnectionPolicy Policy;
	}

	internal class CancelHandshakeReconnectInvocation: HandshakeReconnectInvocation, Core.IEffectCancelInvocation { }
	
	internal class ReceiveReconnectInvocation: ReceiveMessagesInvocation 
	{ 
		public int AttemptedRetries;
		public int MaxConnectionRetry;
		public PNReconnectionPolicy Policy;
	}

	internal class CancelReceiveReconnectInvocation: ReceiveReconnectInvocation, Core.IEffectCancelInvocation { }
}