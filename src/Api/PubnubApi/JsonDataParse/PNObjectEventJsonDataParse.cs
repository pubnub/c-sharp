using System;
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNObjectEventJsonDataParse
    {
        internal static PNObjectEventResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNObjectEventResult result = null;

            Dictionary<string, object> objectEventDicObj = (listObject != null && listObject.Count > 0) ? jsonPlug.ConvertToDictionaryObject(listObject[0]) : null;
            if (objectEventDicObj != null)
            {
                if (result == null)
                {
                    result = new PNObjectEventResult();
                }

                if (objectEventDicObj.ContainsKey("event") && objectEventDicObj["event"] != null)
                {
                    result.Event = objectEventDicObj["event"].ToString();
                }

                if (listObject.Count > 2)
                {
                    long objectEventTimeStamp;
                    if (Int64.TryParse(listObject[2].ToString(), out objectEventTimeStamp))
                    {
                        result.Timestamp = objectEventTimeStamp;
                    }
                }

                if (objectEventDicObj.ContainsKey("type") && objectEventDicObj["type"] != null)
                {
                    result.Type = objectEventDicObj["type"].ToString();
                }

                if (objectEventDicObj.ContainsKey("data") && objectEventDicObj["data"] != null)
                {
                    Dictionary<string, object> dataDic = objectEventDicObj["data"] as Dictionary<string, object>;
                    if (dataDic != null)
                    {
                        if (result.Type.ToLowerInvariant() == "uuid" && dataDic.ContainsKey("id"))
                        {
                            result.UuidMetadata = new PNUuidMetadataResult
                            {
                                Uuid = dataDic["id"] != null ? dataDic["id"].ToString() : null,
                                Name = (dataDic.ContainsKey("name") && dataDic["name"] != null) ? dataDic["name"].ToString() : null,
                                ExternalId = (dataDic.ContainsKey("externalId") && dataDic["externalId"] != null) ? dataDic["externalId"].ToString() : null,
                                ProfileUrl = (dataDic.ContainsKey("profileUrl") && dataDic["profileUrl"] != null) ? dataDic["profileUrl"].ToString() : null,
                                Email = (dataDic.ContainsKey("email") && dataDic["email"] != null) ? dataDic["email"].ToString() : null,
                                Custom = (dataDic.ContainsKey("custom") && dataDic["custom"] != null) ? jsonPlug.ConvertToDictionaryObject(dataDic["custom"]) : null,
                                Updated = (dataDic.ContainsKey("updated") && dataDic["updated"] != null) ? dataDic["updated"].ToString() : null
                            };
                        }
                        else if (result.Type.ToLowerInvariant() == "channel" && dataDic.ContainsKey("id"))
                        {
                            result.ChannelMetadata = new PNChannelMetadataResult
                            {
                                Channel = dataDic["id"] != null ? dataDic["id"].ToString() : null,
                                Name = (dataDic.ContainsKey("name") && dataDic["name"] != null) ? dataDic["name"].ToString() : null,
                                Description = (dataDic.ContainsKey("description") && dataDic["description"] != null) ? dataDic["description"].ToString() : null,
                                Custom = (dataDic.ContainsKey("custom") && dataDic["custom"] != null) ? jsonPlug.ConvertToDictionaryObject(dataDic["custom"]) : null,
                                Updated = (dataDic.ContainsKey("updated") && dataDic["updated"] != null) ? dataDic["updated"].ToString() : null
                            };
                        }
                        else if (result.Type.ToLowerInvariant() == "membership" && dataDic.ContainsKey("uuid") && dataDic.ContainsKey("channel"))
                        {
                            Dictionary<string, object> uuidMetadataIdDic = dataDic["uuid"] as Dictionary<string, object>;
                            if (uuidMetadataIdDic != null && uuidMetadataIdDic.ContainsKey("id"))
                            {
                                result.UuidMetadata = new PNUuidMetadataResult
                                {
                                    Uuid = uuidMetadataIdDic["id"] != null ? uuidMetadataIdDic["id"].ToString() : null
                                };
                            }

                            Dictionary<string, object> channelMetadataIdDic = dataDic["channel"] as Dictionary<string, object>;
                            if (channelMetadataIdDic != null && channelMetadataIdDic.ContainsKey("id"))
                            {
                                result.ChannelMetadata = new PNChannelMetadataResult
                                {
                                    Channel = channelMetadataIdDic["id"] != null ? channelMetadataIdDic["id"].ToString() : null
                                };
                            }

                        }
                    }
                }
                if (listObject.Count == 6) {
                    result.Subscription = listObject[4].ToString();
                    result.Channel = listObject[5].ToString();
                } else if (listObject.Count == 5) {
                    result.Channel = listObject[4].ToString();
                }
            }

            return result;
        }
    }

}
