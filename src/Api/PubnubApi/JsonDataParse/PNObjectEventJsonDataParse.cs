using System;
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNObjectEventJsonDataParse
    {
        internal static PNObjectEventResult GetObject(IJsonPluggableLibrary jsonPlug, IDictionary<string, object> jsonFields)
        {
            PNObjectEventResult result = null;

            Dictionary<string, object> objectEventDicObj = jsonFields is { Count: > 0 } ? jsonPlug.ConvertToDictionaryObject(jsonFields["payload"]) : null;
            if (objectEventDicObj != null)
            {
                result = new PNObjectEventResult();

                if (objectEventDicObj.ContainsKey("event") && objectEventDicObj["event"] != null)
                {
                    result.Event = objectEventDicObj["event"].ToString();
                }

                if (Int64.TryParse(jsonFields["publishTimetoken"]?.ToString(), out var objectEventTimeStamp))
                {
                    result.Timestamp = objectEventTimeStamp;
                }
                

                if (objectEventDicObj.ContainsKey("type") && objectEventDicObj["type"] != null)
                {
                    result.Type = objectEventDicObj["type"].ToString();
                }

                if (objectEventDicObj.ContainsKey("data") && objectEventDicObj["data"] != null)
                {
                    Dictionary<string, object> dataFields = objectEventDicObj["data"] as Dictionary<string, object>;
                    if (dataFields != null)
                    {
                        if (result.Type?.ToLowerInvariant() == "uuid" && dataFields.ContainsKey("id"))
                        {
                            result.UuidMetadata = new PNUuidMetadataResult
                            {
                                Uuid = dataFields["id"]?.ToString(),
                                Name = (dataFields.ContainsKey("name") && dataFields["name"] != null) ? dataFields["name"].ToString() : null,
                                ExternalId = (dataFields.ContainsKey("externalId") && dataFields["externalId"] != null) ? dataFields["externalId"].ToString() : null,
                                ProfileUrl = (dataFields.ContainsKey("profileUrl") && dataFields["profileUrl"] != null) ? dataFields["profileUrl"].ToString() : null,
                                Email = (dataFields.ContainsKey("email") && dataFields["email"] != null) ? dataFields["email"].ToString() : null,
                                Status = (dataFields.ContainsKey("status") && dataFields["status"] != null) ? dataFields["status"].ToString() : null,
                                Type = (dataFields.ContainsKey("type") && dataFields["type"] != null) ? dataFields["type"].ToString() : null,
                                Custom = (dataFields.ContainsKey("custom") && dataFields["custom"] != null) ? jsonPlug.ConvertToDictionaryObject(dataFields["custom"]) : null,
                                Updated = (dataFields.ContainsKey("updated") && dataFields["updated"] != null) ? dataFields["updated"].ToString() : null
                            };
                        }
                        else if (result.Type.ToLowerInvariant() == "channel" && dataFields.ContainsKey("id"))
                        {
                            result.ChannelMetadata = new PNChannelMetadataResult
                            {
                                Channel = dataFields["id"]?.ToString(),
                                Name = (dataFields.ContainsKey("name") && dataFields["name"] != null) ? dataFields["name"].ToString() : null,
                                Description = (dataFields.ContainsKey("description") && dataFields["description"] != null) ? dataFields["description"].ToString() : null,
                                Status = (dataFields.ContainsKey("status") && dataFields["status"] != null) ? dataFields["status"].ToString() : null,
                                Type = (dataFields.ContainsKey("type") && dataFields["type"] != null) ? dataFields["type"].ToString() : null,
                                Custom = (dataFields.ContainsKey("custom") && dataFields["custom"] != null) ? jsonPlug.ConvertToDictionaryObject(dataFields["custom"]) : null,
                                Updated = (dataFields.ContainsKey("updated") && dataFields["updated"] != null) ? dataFields["updated"].ToString() : null
                            };
                        }
                        else if (result.Type.ToLowerInvariant() == "membership" && dataFields.ContainsKey("uuid") && dataFields.ContainsKey("channel"))
                        {
                            Dictionary<string, object> userMetadataFields = dataFields["uuid"] as Dictionary<string, object>;
                            if (userMetadataFields != null && userMetadataFields.ContainsKey("id"))
                            {
                                result.UuidMetadata = new PNUuidMetadataResult
                                {
                                    Uuid = userMetadataFields["id"]?.ToString()
                                };
                            }

                            Dictionary<string, object> channelMetadataFields = dataFields["channel"] as Dictionary<string, object>;
                            if (channelMetadataFields != null && channelMetadataFields.ContainsKey("id"))
                            {
                                result.ChannelMetadata = new PNChannelMetadataResult
                                {
                                    Channel = channelMetadataFields["id"]?.ToString()
                                };
                            }

                        }
                    }
                }
                result.Subscription = jsonFields["channelGroup"]?.ToString();
                result.Channel = jsonFields["channel"].ToString();
            }

            return result;
        }
    }

}
