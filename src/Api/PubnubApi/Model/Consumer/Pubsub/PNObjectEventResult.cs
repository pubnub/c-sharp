namespace PubnubApi
{
    public class PNObjectEventResult
    {
        public string Event { get; internal set; } = ""; //values = set/delete for uuid/channel/UuidAssociation/ChannelAssociation
        public string Type { get; internal set; } = ""; //values = uuid/channel/membership
        public PNUuidMetadataResult UuidMetadata { get; internal set; } //Populate when Type = uuid
        public PNChannelMetadataResult ChannelMetadata { get; internal set; } //Populate when Type = channel
        public PNMembershipMetadataResult MembershipMetadata { get; internal set; } //Populate when Type = membership
        public long Timestamp { get; internal set; }
        public string Channel { get; internal set; } = ""; //Subscribed channel
        public string Subscription { get; internal set; }
    }
}
