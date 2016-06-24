
namespace PubnubApi
{
    internal class GrantAckPayload
    {
        public GrantAckPayload()
        {
            this.Level = "";
            this.SubscribeKey = "";
        }

        public string Level { get; set; }
        public string SubscribeKey { get; set; }
        public int TTL { get; set; }
        public string[] ChannelName { get; set; }
        public string ChannelGroupName { get; set; }
    }
}
