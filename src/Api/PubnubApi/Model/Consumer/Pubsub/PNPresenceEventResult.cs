
using System.Collections.Generic;

namespace PubnubApi
{
    public class PNPresenceEventResult
    {
        public PNPresenceEventResult()
        {
            this.Event = "";
            this.Uuid = "";
            this.Channel = "";
            this.Subscription = "";
        }
        public string Event { get; internal set; }

        public string Uuid { get; internal set; }
        public long Timestamp { get; internal set; }
        public int Occupancy { get; internal set; }
        public Dictionary<string, object> State { get; internal set; }

        public string Channel { get; internal set; }
        public string Subscription { get; internal set; }

        public long Timetoken { get; internal set; }
        public Dictionary<string, object> UserMetadata { get; internal set; }
        public string[] Join { get; internal set; }
        public string[] Timeout { get; internal set; }
        public string[] Leave { get; internal set; }
        public bool HereNowRefresh { get; internal set; }
    }
}
