
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
        public string Event { get; set; }

        public string Uuid { get; set; }
        public long Timestamp { get; set; }
        public int Occupancy { get; set; }
        public Dictionary<string, object> State { get; set; }

        public string Channel { get; set; }
        public string Subscription { get; set; }

        public long Timetoken { get; set; }
        public object UserMetadata { get; set; }
        public string[] Join { get; set; } //New
        public string[] Timeout { get; set; } //New
        public string[] Leave { get; set; } //New
        public bool HereNowRefresh { get; set; }
    }
}
