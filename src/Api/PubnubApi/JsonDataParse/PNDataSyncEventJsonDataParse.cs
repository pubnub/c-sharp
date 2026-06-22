using System.Collections.Generic;
using System.Linq;
using PubnubApi.EndPoint;

namespace PubnubApi;

internal static class PNDataSyncEventJsonDataParse
{
    internal static PNDataSyncEventResult GetObject(IJsonPluggableLibrary jsonPlug,
        IDictionary<string, object> jsonFields)
    {
        var payload = jsonFields is { Count: > 0 }
            ? jsonPlug.ConvertToDictionaryObject(jsonFields["payload"])
            : null;
        if (payload == null)
        {
            return null;
        }

        var result = new PNDataSyncEventResult
        {
            Version = GetStringValue(payload, "version")
        };

        var metadata = payload.ContainsKey("metadata") && payload["metadata"] != null
            ? jsonPlug.ConvertToDictionaryObject(payload["metadata"])
            : null;
        if (metadata != null)
        {
            result.Event = GetStringValue(metadata, "event");
            result.Source = GetStringValue(metadata, "source");
            result.Type = GetStringValue(metadata, "type");
            //Splitting here to extract the class name and skip classes it's inheriting from
            result.ClassName = GetStringValue(metadata, "className").Split(':').Last();
            if (int.TryParse(GetStringValue(metadata, "classVersion"), out var classVersion))
            {
                result.ClassVersion = classVersion;
            }
        }

        var data = payload.ContainsKey("data") && payload["data"] != null
            ? jsonPlug.ConvertToDictionaryObject(payload["data"])
            : null;
        if (data != null)
        {
            var eventName = result.Event?.ToLowerInvariant();
            var type = result.Type?.ToLowerInvariant();

            if (eventName == "delete")
            {
                result.Id = GetStringValue(data, "id");
                result.DeletedAt = GetStringValue(data, "deletedAt");
            }
            else if (type == "entity")
            {
                result.EntityData = new PNDataSyncEntityResult
                {
                    Id = GetStringValue(data, "id"),
                    EntityClass = result.ClassName,
                    EntityClassVersion = result.ClassVersion,
                    Status = GetStringValue(data, "status"),
                    Payload = data.ContainsKey("payload") && data["payload"] != null
                        ? jsonPlug.ConvertToDictionaryObject(data["payload"])
                        : null,
                    CreatedAt = GetStringValue(data, "createdAt"),
                    UpdatedAt = GetStringValue(data, "updatedAt"),
                    ETag = GetStringValue(data, "eTag"),
                    ExpiresAt = GetStringValue(data, "expiresAt"),
                };
            }
            else if (type == "relationship")
            {
                result.RelationshipData = new PNDataSyncRelationshipResult
                {
                    Id = GetStringValue(data, "id"),
                    EntityAId = GetStringValue(data, "entityAId"),
                    EntityBId = GetStringValue(data, "entityBId"),
                    RelationshipClass = result.ClassName,
                    RelationshipClassVersion = result.ClassVersion,
                    Status = GetStringValue(data, "status"),
                    Payload = data.ContainsKey("payload") && data["payload"] != null
                        ? jsonPlug.ConvertToDictionaryObject(data["payload"])
                        : null,
                    CreatedAt = GetStringValue(data, "createdAt"),
                    UpdatedAt = GetStringValue(data, "updatedAt"),
                    ETag = GetStringValue(data, "eTag"),
                    ExpiresAt = GetStringValue(data, "expiresAt"),
                };
            }
        }

        if (long.TryParse(jsonFields["publishTimetoken"]?.ToString(), out var timestamp))
        {
            result.Timestamp = timestamp;
        }

        result.Subscription = jsonFields["channelGroup"]?.ToString();
        result.Channel = jsonFields["channel"]?.ToString();

        return result;
    }

    private static string GetStringValue(IDictionary<string, object> dictionary, string key)
    {
        return dictionary.ContainsKey(key) && dictionary[key] != null
            ? dictionary[key].ToString()
            : null;
    }
}
