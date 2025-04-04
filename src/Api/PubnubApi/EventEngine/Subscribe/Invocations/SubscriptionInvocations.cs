﻿using System.Collections.Generic;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.Invocations {
	public class EmitMessagesInvocation : Core.IEffectInvocation {
		public ReceivingResponse<object> Messages;
		public SubscriptionCursor  Cursor;
		public string Name { get; set; } = "EMIT_MESSAGES";
		public EmitMessagesInvocation(SubscriptionCursor  cursor, ReceivingResponse<object> messages)
		{
			this.Cursor = cursor;
			this.Messages = messages;
		}
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}

	public class EmitStatusInvocation : Core.IEffectInvocation {
		// TODO merge status variables into one?
		public PNStatusCategory StatusCategory;
		public PNStatus Status;
		public int StatusCode;
		public string Name { get; set; } = "EMIT_STATUS";

		public EmitStatusInvocation(PNStatus status)
		{
			this.Status = status;
			if (status != null)
			{
				this.StatusCategory = status.Category;
				this.StatusCode = status.StatusCode;
			}
		}

		public EmitStatusInvocation(PNStatusCategory category)
		{
			this.StatusCategory = category;
			this.Status = new PNStatus()
			{
				Category = category,
			};
		}
		public override string ToString()
		{
			return $"Invocation : {Name}, StatusCode={Status.StatusCode}, Category={StatusCategory}";
		}
	}

	public class HandshakeInvocation : Core.IEffectInvocation {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor  Cursor;
		// TODO if we need these, figure out how to pass them.
		public Dictionary<string, string> InitialSubscribeQueryParams = new Dictionary<string, string>();
		public Dictionary<string, object> ExternalQueryParams = new Dictionary<string, object>();
		public virtual string Name { get; set; } = "HANDSHAKE";
		
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}
	
	public class ReceiveMessagesInvocation : Core.IEffectInvocation 
	{ 
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor  Cursor;
		public Dictionary<string, string> InitialSubscribeQueryParams = new Dictionary<string, string>();
		public Dictionary<string, object> ExternalQueryParams = new Dictionary<string, object>();
		public virtual string Name { get; set; } = "RECEIVE_MESSAGES";
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}
	
	public class CancelReceiveMessagesInvocation : ReceiveMessagesInvocation, Core.IEffectCancelInvocation 
	{
		public override string Name { get; set; } = "CANCEL_RECEIVE_MESSAGES";
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}

	public class CancelHandshakeInvocation : HandshakeInvocation, Core.IEffectCancelInvocation 
	{
		public override string Name { get; set; } = "CANCEL_HANDSHAKE";
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}

	public class HandshakeReconnectInvocation: HandshakeInvocation
	{
		public int AttemptedRetries;
		public PNStatus Reason { get; set; }
		public override string Name { get; set; } = "HANDSHAKE_RECONNECT";
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}

	public class CancelHandshakeReconnectInvocation: HandshakeReconnectInvocation, Core.IEffectCancelInvocation 
	{ 
		public override string Name { get; set; } = "CANCEL_HANDSHAKE_RECONNECT";
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}
	
	public class ReceiveReconnectInvocation: ReceiveMessagesInvocation 
	{ 
		public int AttemptedRetries;
		public PNStatus Reason { get; set; }
		public override string Name { get; set; } = "RECEIVE_RECONNECT";
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}

	public class CancelReceiveReconnectInvocation: ReceiveReconnectInvocation, Core.IEffectCancelInvocation 
	{
		public override string Name { get; set; } = "CANCEL_RECEIVE_RECONNECT";
		public override string ToString()
		{
			return $"Invocation : {Name}";
		}
	}
}