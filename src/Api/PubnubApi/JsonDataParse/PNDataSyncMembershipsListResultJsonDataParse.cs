using System.Collections.Generic;
using System.Linq;
using PubnubApi.EndPoint;

namespace PubnubApi;

internal static class PNDataSyncMembershipsListResultJsonDataParse
{
    internal static PNDataSyncMembershipsListResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
    {
        var result = new PNDataSyncMembershipsListResult();
        foreach (var rawObject in listObject)
        {
            var objectDictionary = jsonPlug.ConvertToDictionaryObject(rawObject);
            if (objectDictionary == null || !objectDictionary.Any())
            {
                continue;
            }

            if (objectDictionary.TryGetValue("data", out var dataObject) && dataObject != null)
            {
                var dataArray = jsonPlug.ConvertToObjectArray(dataObject);
                if (dataArray == null || !dataArray.Any())
                {
                    continue;
                }

                for (int i = 0; i < dataArray.Length; i++)
                {
                    if (dataArray[i] is not Dictionary<string, object> dataEntryDictionary)
                    {
                        continue;
                    }
                    var data = new PNDataSyncMembershipResult
                    {
                        Id = dataEntryDictionary.ContainsKey("id") && dataEntryDictionary["id"] != null
                            ? dataEntryDictionary["id"].ToString()
                            : null,
                        ChannelId = dataEntryDictionary.ContainsKey("channelId") && dataEntryDictionary["channelId"] != null
                            ? dataEntryDictionary["channelId"].ToString()
                            : null,
                        UserId = dataEntryDictionary.ContainsKey("userId") && dataEntryDictionary["userId"] != null
                            ? dataEntryDictionary["userId"].ToString()
                            : null,
                        RelationshipClass = dataEntryDictionary.ContainsKey("relationshipClass") && dataEntryDictionary["relationshipClass"] != null
                            ? dataEntryDictionary["relationshipClass"].ToString()
                            : null,
                        RelationshipClassVersion = dataEntryDictionary.ContainsKey("relationshipClassVersion") && dataEntryDictionary["relationshipClassVersion"] != null
                            ? (int)(long)dataEntryDictionary["relationshipClassVersion"]
                            : 1,
                        Status = dataEntryDictionary.ContainsKey("status") && dataEntryDictionary["status"] != null
                            ? dataEntryDictionary["status"].ToString()
                            : null,
                        CreatedAt = dataEntryDictionary.ContainsKey("createdAt") && dataEntryDictionary["createdAt"] != null
                            ? dataEntryDictionary["createdAt"].ToString()
                            : null,
                        UpdatedAt = dataEntryDictionary.ContainsKey("updatedAt") && dataEntryDictionary["updatedAt"] != null
                            ? dataEntryDictionary["updatedAt"].ToString()
                            : null,
                        ETag = dataEntryDictionary.ContainsKey("eTag") && dataEntryDictionary["eTag"] != null
                            ? dataEntryDictionary["eTag"].ToString()
                            : null,
                        ExpiresAt = dataEntryDictionary.ContainsKey("expiresAt") && dataEntryDictionary["expiresAt"] != null
                            ? dataEntryDictionary["expiresAt"].ToString()
                            : null
                    };
                    if (dataEntryDictionary.TryGetValue("payload", out var payloadObject) && payloadObject != null)
                    {
                        data.Payload = jsonPlug.ConvertToDictionaryObject(payloadObject);
                    }
                    result.Data.Add(data);
                }
            }
            else if (objectDictionary.TryGetValue("meta", out var metaObject) && metaObject != null)
            {
                var metaDictionary = jsonPlug.ConvertToDictionaryObject(metaObject);
                if (metaDictionary == null || !metaDictionary.Any())
                {
                    continue;
                }

                var meta = new PaginationMeta();
                if (metaDictionary.TryGetValue("next_cursor", out var nextCursor) && nextCursor != null)
                {
                    meta.NextCursor = nextCursor.ToString();
                }

                if (metaDictionary.TryGetValue("has_next", out var hasNext) && hasNext != null)
                {
                    meta.HasNext = (bool)hasNext;
                }

                if (metaDictionary.TryGetValue("limit", out var limit) && limit != null)
                {
                    meta.Limit = (int)(long)limit;
                }

                result.Meta = meta;
            }
        }

        return result;
    }
}
