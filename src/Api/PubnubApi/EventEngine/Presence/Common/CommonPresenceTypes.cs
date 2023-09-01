using PubnubApi.EventEngine.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EventEngine.Presence.Common
{
    public abstract class PresenceState : Core.State
	{
		public IEnumerable<string> Channels { get; set;}
		public IEnumerable<string> ChannelGroups { get; set;}
		//public SubscriptionCursor Cursor;
		public ReconnectionConfiguration ReconnectionConfiguration;
	}
}
