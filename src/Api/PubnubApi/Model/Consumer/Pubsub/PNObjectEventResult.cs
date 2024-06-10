using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNObjectEventResult
    {
        public PNObjectEventResult()
        {
            this.Event = ""; //values = set/delete for uuid/channel/UuidAssociation/ChannelAssociation
            this.Type = "";  //values = uuid/channel/membership
            this.Channel = "";
        }
        public string Event { get; internal set; }
        public string Type { get; internal set; }
        public PNUuidMetadataResult UuidMetadata { get; internal set; } //Populate when Type = uuid
        public PNChannelMetadataResult ChannelMetadata { get; internal set; } //Populate when Type = channel
        public long Timestamp { get; internal set; }
        public string Channel { get; internal set; } //Subscribed channel
        public string Subscription { get; internal set; }
    }
}
