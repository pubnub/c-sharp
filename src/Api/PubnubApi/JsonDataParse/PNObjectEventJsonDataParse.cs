﻿using System.Collections.Generic;

namespace PubnubApi;

internal static class PNObjectEventJsonDataParse
{
    internal static PNObjectEventResult GetObject(IJsonPluggableLibrary jsonPlug,
        IDictionary<string, object> jsonFields)
    {
        PNObjectEventResult result = null;

        var objectEventDicObj = jsonFields is { Count: > 0 }
            ? jsonPlug.ConvertToDictionaryObject(jsonFields["payload"])
            : null;
        if (objectEventDicObj != null)
        {
            result = new PNObjectEventResult();

            if (objectEventDicObj.ContainsKey("event") && objectEventDicObj["event"] != null)
            {
                result.Event = objectEventDicObj["event"].ToString();
            }

            if (long.TryParse(jsonFields["publishTimetoken"]?.ToString(), out var objectEventTimeStamp))
            {
                result.Timestamp = objectEventTimeStamp;
            }


            if (objectEventDicObj.ContainsKey("type") && objectEventDicObj["type"] != null)
            {
                result.Type = objectEventDicObj["type"].ToString();
            }

            if (objectEventDicObj.ContainsKey("data") && objectEventDicObj["data"] != null)
            {
                var dataFields = objectEventDicObj["data"] as Dictionary<string, object>;
                if (dataFields != null)
                {
                    //Common fields
                    var custom = dataFields.ContainsKey("custom") && dataFields["custom"] != null
                        ? jsonPlug.ConvertToDictionaryObject(dataFields["custom"])
                        : null;
                    var status = GetStringValue(dataFields, "status");
                    var type = GetStringValue(dataFields, "type");
                    var updated = GetStringValue(dataFields, "updated");
                    
                    if (result.Type?.ToLowerInvariant() == "uuid" && dataFields.ContainsKey("id"))
                    {
                        result.UuidMetadata = new PNUuidMetadataResult
                        {
                            Uuid = dataFields["id"]?.ToString(),
                            Name = GetStringValue(dataFields, "name"),
                            ExternalId = GetStringValue(dataFields, "externalId"),
                            ProfileUrl = GetStringValue(dataFields, "profileUrl"),
                            Email = GetStringValue(dataFields, "email"),
                            Status = status,
                            Type = type,
                            Custom = custom,
                            Updated = updated
                        };
                    }
                    else if (result.Type.ToLowerInvariant() == "channel" && dataFields.ContainsKey("id"))
                    {
                        result.ChannelMetadata = new PNChannelMetadataResult
                        {
                            Channel = dataFields["id"]?.ToString(),
                            Name = GetStringValue(dataFields, "name"),
                            Description = GetStringValue(dataFields, "description"),
                            Status = status,
                            Type = type,
                            Custom = custom,
                            Updated = updated
                        };
                    }
                    else if (result.Type.ToLowerInvariant() == "membership" && dataFields.ContainsKey("uuid") &&
                             dataFields.ContainsKey("channel"))
                    {
                        result.MembershipMetadata = new PNMembershipMetadataResult()
                        {
                            Custom = custom,
                            Status = status,
                            Type = type,
                            Updated = updated
                        };
                        var userMetadataFields = dataFields["uuid"] as Dictionary<string, object>;
                        if (userMetadataFields != null && userMetadataFields.ContainsKey("id"))
                        {
                            result.MembershipMetadata.Uuid = userMetadataFields["id"]?.ToString();
                        }
                        var channelMetadataFields = dataFields["channel"] as Dictionary<string, object>;
                        if (channelMetadataFields != null && channelMetadataFields.ContainsKey("id"))
                        {
                            result.MembershipMetadata.Channel = channelMetadataFields["id"]?.ToString();
                        }
                    }
                }
            }

            result.Subscription = jsonFields["channelGroup"]?.ToString();
            result.Channel = jsonFields["channel"].ToString();
        }

        return result;
    }

    private static string GetStringValue(Dictionary<string, object> dictionary, string key)
    {
        return dictionary.ContainsKey(key) && dictionary[key] != null
            ? dictionary[key].ToString()
            : null;
    }
}