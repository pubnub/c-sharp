using System.Collections.Generic;

namespace PubnubApi;

public class PNMembershipMetadataResult
{
    public string Channel { get; internal set; }
    public string Uuid { get; internal set; }
    public Dictionary<string, object> Custom { get; internal set; }
    public string Status { get; internal set; }
    public string Type { get; internal set; }
    public string Updated { get; internal set; }
}