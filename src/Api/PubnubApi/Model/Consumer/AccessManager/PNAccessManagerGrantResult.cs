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

        public Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> Uuids { get; internal set; }

        /// <summary>
        /// Category-level permissions keyed by category ("channels", "uuids") and then by auth key.
        /// </summary>
        public Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> Categories { get; internal set; }

    }
}
