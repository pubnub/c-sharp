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
        public string Event { get; set; }
        public string Type { get; set; }
        public PNUuidMetadataResult UuidMetadata { get; set; } //Populate when Type = uuid
        public PNChannelMetadataResult ChannelMetadata { get; set; } //Populate when Type = channel
        public long Timestamp { get; set; }
        public string Channel { get; set; } //Subscribed channel
    }
}
