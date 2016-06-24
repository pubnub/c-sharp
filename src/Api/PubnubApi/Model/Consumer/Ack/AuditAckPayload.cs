
namespace PubnubApi
{
    internal class AuditAckPayload
    {
        public AuditAckPayload()
        {
            this.Level = "";
            this.SubscribeKey = "";
        }

        public string Level { get; set; }
        public string SubscribeKey { get; set; }
        public int TTL { get; set; }
        public string ChannelGroups { get; set; }
    }
}
