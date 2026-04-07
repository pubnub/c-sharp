using System.Collections.Generic;

namespace PubnubApi.EndPoint;

/// <summary>
/// Result returned by a single-entity operation (Create, Get, Update, Patch).
/// The server response envelope is deserialized into this type.
/// </summary>
public class PNDataSyncEntityResult
{
    public string Id { get; internal set; }
    public string EntityClass { get; internal set; }
    public int EntityClassVersion { get; internal set; }
    public string Status { get; internal set; }
    public Dictionary<string, object> Payload { get; internal set; }
    public string CreatedAt { get; internal set; }
    public string UpdatedAt { get; internal set; }
    public string ETag { get; internal set; }
    public string ExpiresAt { get; internal set; }
}