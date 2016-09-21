
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
        public JsonNode State { get; set; }

        public string Channel { get; set; }
        public string Subscription { get; set; }

        public long Timetoken { get; set; }
        public object UserMetadata { get; set; }
    }
}
