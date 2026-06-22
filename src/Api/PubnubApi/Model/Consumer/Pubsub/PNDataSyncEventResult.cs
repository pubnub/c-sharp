namespace PubnubApi
{
    public class PNDataSyncEventResult
    {
        public string Version { get; internal set; } = ""; //e.g. "3.0"
        public string Event { get; internal set; } = ""; //values = create/update/delete
        public string Source { get; internal set; } = ""; //values = "data-sync"
        public string Type { get; internal set; } = ""; //values = entity/relationship
        public string ClassName { get; internal set; } = "";
        public int ClassVersion { get; internal set; }
        public PubnubApi.EndPoint.PNDataSyncEntityResult EntityData { get; internal set; } //Populated when Type = entity (create/update)
        public PubnubApi.EndPoint.PNDataSyncRelationshipResult RelationshipData { get; internal set; } //Populated when Type = relationship (create/update)
        public string Id { get; internal set; } //Populated for delete events
        public string DeletedAt { get; internal set; } //Populated for delete events
        public long Timestamp { get; internal set; }
        public string Channel { get; internal set; } = ""; //Subscribed channel
        public string Subscription { get; internal set; }
    }
}
