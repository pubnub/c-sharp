using System.Collections.Generic;

namespace PubnubApi
{
    public class PNAccessManagerGrantResult
    {
        public PNAccessManagerGrantResult()
        {
            this.Level = "";
            this.SubscribeKey = "";
        }

        public string Level { get; set; }
        public int Ttl { get; set; }
        public string SubscribeKey { get; set; }

        public Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> Channels { get; set; }

        public Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> ChannelGroups { get; set; }

    }
}
