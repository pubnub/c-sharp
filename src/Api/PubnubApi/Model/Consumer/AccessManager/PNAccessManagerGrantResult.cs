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

        public string Level { get; internal set; }
        public int Ttl { get; internal set; }
        public string SubscribeKey { get; internal set; }

        public Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> Channels { get; internal set; }

        public Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> ChannelGroups { get; internal set; }

    }
}
