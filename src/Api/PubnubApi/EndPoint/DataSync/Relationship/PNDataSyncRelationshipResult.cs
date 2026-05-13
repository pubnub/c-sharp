using System.Collections.Generic;

namespace PubnubApi.EndPoint;

/// <summary>
/// Result returned by a single-relationship operation (Create, Get, Update, Patch).
/// The server response envelope is deserialized into this type.
/// </summary>
public class PNDataSyncRelationshipResult
{
    public string Id { get; internal set; }
    public string EntityAId { get; internal set; }
    public string EntityBId { get; internal set; }
    public string RelationshipClass { get; internal set; }
    public int RelationshipClassVersion { get; internal set; }
    public string Status { get; internal set; }
    public Dictionary<string, object> Payload { get; internal set; }
    public string CreatedAt { get; internal set; }
    public string UpdatedAt { get; internal set; }
    public string ETag { get; internal set; }
    public string ExpiresAt { get; internal set; }
}
