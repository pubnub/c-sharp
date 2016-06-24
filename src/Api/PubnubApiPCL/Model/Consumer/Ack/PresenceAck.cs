
namespace PubnubApi
{
    public class PresenceAck
    {
        public PresenceAck()
        {
            this.Action = "";
            this.UUID = "";
            this.ChannelName = "";
            this.ChannelGroupName = "";
        }
        public string Action { get; set; }
        public long Timestamp { get; set; }
        public string UUID { get; set; }
        public int Occupancy { get; set; }

        public string ChannelName { get; set; }
        public string ChannelGroupName { get; set; }

        public long Timetoken { get; set; }
    }
}
