using System.Collections.Generic;
using PubnubApi.EndPoint;

namespace PubnubApi;

internal static class PNDataSyncMembershipResultJsonDataParse
{
    internal static PNDataSyncMembershipResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
    {
        Dictionary<string, object> dictionaryObject = (listObject != null && listObject.Count == 1)
            ? jsonPlug.ConvertToDictionaryObject(listObject[0])
            : null;
        PNDataSyncMembershipResult result = null;
        if (dictionaryObject != null && dictionaryObject.ContainsKey("data"))
        {
            result = new PNDataSyncMembershipResult();

            Dictionary<string, object> dataDictionary =
                jsonPlug.ConvertToDictionaryObject(dictionaryObject["data"]);
            if (dataDictionary != null && dataDictionary.Count > 0)
            {
                result.Id = dataDictionary.ContainsKey("id") && dataDictionary["id"] != null
                    ? dataDictionary["id"].ToString()
                    : null;
                result.ChannelId = dataDictionary.ContainsKey("entityAId") && dataDictionary["entityAId"] != null
                    ? dataDictionary["entityAId"].ToString()
                    : null;
                result.UserId = dataDictionary.ContainsKey("entityBId") && dataDictionary["entityBId"] != null
                    ? dataDictionary["entityBId"].ToString()
                    : null;
                result.RelationshipClassVersion = dataDictionary.ContainsKey("relationshipClassVersion") && dataDictionary["relationshipClassVersion"] != null
                    ? (int)(long)dataDictionary["relationshipClassVersion"]
                    : 1;
                result.Status = dataDictionary.ContainsKey("status") && dataDictionary["status"] != null
                    ? dataDictionary["status"].ToString()
                    : null;
                result.CreatedAt = dataDictionary.ContainsKey("createdAt") && dataDictionary["createdAt"] != null
                    ? dataDictionary["createdAt"].ToString()
                    : null;
                result.UpdatedAt = dataDictionary.ContainsKey("updatedAt") && dataDictionary["updatedAt"] != null
                    ? dataDictionary["updatedAt"].ToString()
                    : null;
                result.ETag = dataDictionary.ContainsKey("eTag") && dataDictionary["eTag"] != null
                    ? dataDictionary["eTag"].ToString()
                    : null;
                result.ExpiresAt = dataDictionary.ContainsKey("expiresAt") && dataDictionary["expiresAt"] != null
                    ? dataDictionary["expiresAt"].ToString()
                    : null;
                if (dataDictionary.TryGetValue("payload", out var payloadObject) && payloadObject != null)
                {
                    result.Payload = jsonPlug.ConvertToDictionaryObject(payloadObject);
                }
            }
        }

        return result;
    }
}
