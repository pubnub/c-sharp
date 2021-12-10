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

        public string Level { get; internal set; }
        public string SubscribeKey { get; internal set; }

        public string Channel { get; internal set; }

        public string ChannelGroup { get; internal set; }

        public Dictionary<string, PNAccessManagerKeyData> AuthKeys { get; internal set; }
    }
}
