using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EventEngine.Presence.Invocations
{
	//TODO: Bring EmitStatusInvocation into common folder for both presence and subscribe
	public class EmitStatusInvocation : Core.IEffectInvocation {
		// TODO merge status variables into one?
		public PNStatusCategory StatusCategory;
		public PNStatus Status;
		public string Name { get; set; } = "EMIT_STATUS";

		public EmitStatusInvocation(PNStatus status)
		{
			this.Status = status;
			if (status != null)
			{
				this.StatusCategory = status.Category;
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
	}
    public class HeartbeatInvocation : Core.IEffectInvocation {
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		//public SubscriptionCursor  Cursor;
		// TODO if we need these, figure out how to pass them.
		public Dictionary<string, string> InitialSubscribeQueryParams = new Dictionary<string, string>();
		public Dictionary<string, object> ExternalQueryParams = new Dictionary<string, object>();
		public virtual string Name { get; set; } = "HEARTBEAT";
	}
    public class CancelHeartbeatInvocation : HeartbeatInvocation, Core.IEffectCancelInvocation 
	{
		public override string Name { get; set; } = "CANCEL_HANDSHAKE";
	}

}
