using System.Collections.Generic;

namespace PubnubApi
{
    public class PNAccessManagerAuditResult
    {
        public PNAccessManagerAuditResult()
        {
            this.Level = "";
            this.SubscribeKey = "";
            this.Channel = "";
            this.ChannelGroup = "";
        }

        public string Level { get; set; }
        public string SubscribeKey { get; set; }

        public string Channel { get; set; }

        public string ChannelGroup { get; set; }

        public Dictionary<string, PNAccessManagerKeyData> AuthKeys { get; set; }
    }
}
