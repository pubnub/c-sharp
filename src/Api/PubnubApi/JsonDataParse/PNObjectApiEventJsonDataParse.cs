using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNObjectApiEventJsonDataParse
    {
        internal static PNObjectApiEventResult GetObject(List<object> listObject)
        {
            PNObjectApiEventResult result = null;

            Dictionary<string, object> objectEventDicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[0]);
            if (objectEventDicObj != null)
            {
                result = new PNObjectApiEventResult();

                result.Event = objectEventDicObj["event"].ToString();

                if (listObject.Count > 2)
                {
                    long objectApiEventTimeStamp;
                    if (Int64.TryParse(listObject[2].ToString(), out objectApiEventTimeStamp))
                    {
                        result.Timestamp = objectApiEventTimeStamp;
                    }
                }

                if (objectEventDicObj.ContainsKey("type"))
                {
                    result.Type = objectEventDicObj["type"].ToString();
                }

                if (objectEventDicObj.ContainsKey("data"))
                {
                    Dictionary<string, object> dataDic = objectEventDicObj["data"] as Dictionary<string, object>;
                    if (dataDic != null)
                    {
                        if (result.Type.ToLowerInvariant() == "user" && dataDic.ContainsKey("id"))
                        {
                            result.UserId = dataDic["id"].ToString();
                            result.User = new PNUserResult
                            {
                                Id = dataDic["id"].ToString(),
                                Name = (dataDic.ContainsKey("name") && dataDic["name"] != null) ? dataDic["name"].ToString() : null,
                                ExternalId = (dataDic.ContainsKey("externalId") && dataDic["externalId"] != null) ? dataDic["externalId"].ToString() : null,
                                ProfileUrl = (dataDic.ContainsKey("profileUrl") && dataDic["profileUrl"] != null) ? dataDic["profileUrl"].ToString() : null,
                                Email = (dataDic.ContainsKey("email") && dataDic["email"] != null) ? dataDic["email"].ToString() : null,
                                Custom = (dataDic.ContainsKey("custom") && dataDic["custom"] != null) ? JsonDataParseInternalUtil.ConvertToDictionaryObject(dataDic["custom"]) : null,
                                Created = (dataDic.ContainsKey("created") && dataDic["created"] != null) ? dataDic["created"].ToString() : "",
                                Updated = (dataDic.ContainsKey("updated") && dataDic["updated"] != null) ? dataDic["updated"].ToString() : ""
                            };
                        }
                        else if (result.Type.ToLowerInvariant() == "space" && dataDic.ContainsKey("id"))
                        {
                            result.SpaceId = dataDic["id"].ToString();
                            result.Space = new PNSpaceResult
                            {
                                Id = dataDic["id"].ToString(),
                                Name = (dataDic.ContainsKey("name") && dataDic["name"] != null) ? dataDic["name"].ToString() : null,
                                Description = (dataDic.ContainsKey("description") && dataDic["description"] != null) ? dataDic["description"].ToString() : null,
                                Custom = (dataDic.ContainsKey("custom") && dataDic["custom"] != null) ? JsonDataParseInternalUtil.ConvertToDictionaryObject(dataDic["custom"]) : null,
                                Created = (dataDic.ContainsKey("created") && dataDic["created"] != null) ? dataDic["created"].ToString() : "",
                                Updated = (dataDic.ContainsKey("updated") && dataDic["updated"] != null) ? dataDic["updated"].ToString() : ""
                            };
                        }
                        else if (result.Type.ToLowerInvariant() == "membership" && dataDic.ContainsKey("userId") && dataDic.ContainsKey("spaceId"))
                        {
                            result.UserId = dataDic["userId"].ToString();
                            result.SpaceId = dataDic["spaceId"].ToString();
                        }
                    }
                }

                result.Channel = (listObject.Count == 6) ? listObject[5].ToString() : listObject[4].ToString();
            }

            return result;
        }
    }

}
