using System.Collections.Generic;

namespace PubnubApi.EndPoint;

public class PNDataSyncMembershipResult
{
    public string Id { get; internal set; }
    public string ChannelId { get; internal set; }
    public string UserId { get; internal set; }
    public int RelationshipClassVersion { get; internal set; }
    public string Status { get; internal set; }
    public Dictionary<string, object> Payload { get; internal set; }
    public string CreatedAt { get; internal set; }
    public string UpdatedAt { get; internal set; }
    public string ETag { get; internal set; }
    public string ExpiresAt { get; internal set; }
}
