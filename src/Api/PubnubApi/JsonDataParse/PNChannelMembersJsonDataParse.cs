using System;
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNChannelMembersJsonDataParse
    {
        internal static PNChannelMembersResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNChannelMembersResult result = null;
            for (int listIndex = 0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = jsonPlug.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    if (result == null)
                    {
                        result = new PNChannelMembersResult();
                    }
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        result.ChannelMembers = new List<PNChannelMembersItemResult>();

                        object[] userArray = jsonPlug.ConvertToObjectArray(dicObj["data"]);
                        if (userArray != null && userArray.Length > 0)
                        {
                            for (int index = 0; index < userArray.Length; index++)
                            {
                                Dictionary<string, object> getMbrItemDataDic = jsonPlug.ConvertToDictionaryObject(userArray[index]);
                                if (getMbrItemDataDic != null && getMbrItemDataDic.Count > 0)
                                {
                                    var mbrItem = new PNChannelMembersItemResult
                                    {
                                        Updated = (getMbrItemDataDic.ContainsKey("updated") && getMbrItemDataDic["updated"] != null) ? getMbrItemDataDic["updated"].ToString() : ""
                                    };
                                    if (getMbrItemDataDic.ContainsKey("custom"))
                                    {
                                        mbrItem.Custom = jsonPlug.ConvertToDictionaryObject(getMbrItemDataDic["custom"]);
                                    }
                                    if (getMbrItemDataDic.ContainsKey("uuid"))
                                    {
                                        Dictionary<string, object> uuidMetadataDic = jsonPlug.ConvertToDictionaryObject(getMbrItemDataDic["uuid"]);
                                        if (uuidMetadataDic != null && uuidMetadataDic.Count > 0)
                                        {
                                            var uuidMetadataResult = new PNUuidMetadataResult
                                            {
                                                Uuid = (uuidMetadataDic.ContainsKey("id") && uuidMetadataDic["id"] != null) ? uuidMetadataDic["id"].ToString() : "",
                                                Name = (uuidMetadataDic.ContainsKey("name") && uuidMetadataDic["name"] != null) ? uuidMetadataDic["name"].ToString() : "",
                                                ExternalId = (uuidMetadataDic.ContainsKey("externalId") && uuidMetadataDic["externalId"] != null) ? uuidMetadataDic["externalId"].ToString() : "",
                                                ProfileUrl = (uuidMetadataDic.ContainsKey("profileUrl") && uuidMetadataDic["profileUrl"] != null) ? uuidMetadataDic["profileUrl"].ToString() : "",
                                                Email = (uuidMetadataDic.ContainsKey("email") && uuidMetadataDic["email"] != null) ? uuidMetadataDic["email"].ToString() : "",
                                                Updated = (uuidMetadataDic.ContainsKey("updated") && uuidMetadataDic["updated"] != null) ? uuidMetadataDic["updated"].ToString() : "",
                                                Custom = (uuidMetadataDic.ContainsKey("custom") && uuidMetadataDic["custom"] != null) ? jsonPlug.ConvertToDictionaryObject(uuidMetadataDic["custom"]) : null,
                                                Status = (uuidMetadataDic.ContainsKey("status") && uuidMetadataDic["status"] != null) ? uuidMetadataDic["status"].ToString() : null,
                                                Type = (uuidMetadataDic.ContainsKey("type") && uuidMetadataDic["type"] != null) ? uuidMetadataDic["id"].ToString() : null,
                                            };
                                            mbrItem.UuidMetadata = uuidMetadataResult;
                                        }
                                    }
                                    mbrItem.Status =
                                        (getMbrItemDataDic.ContainsKey("status") && getMbrItemDataDic["status"] != null) ? getMbrItemDataDic["status"]?.ToString() : null;
                                    mbrItem.Type =
                                        (getMbrItemDataDic.ContainsKey("type") && getMbrItemDataDic["type"] != null) ? getMbrItemDataDic["type"]?.ToString() : null;
                                    result.ChannelMembers.Add(mbrItem);
                                }
                            }
                        }


                    }
                    else if (dicObj.ContainsKey("totalCount") && dicObj["totalCount"] != null)
                    {
                        int usersCount;
                        var _ = Int32.TryParse(dicObj["totalCount"].ToString(), out usersCount);
                        result.TotalCount = usersCount;
                    }
                    else if (dicObj.ContainsKey("next") && dicObj["next"] != null)
                    {
                        if (result.Page == null)
                        {
                            result.Page = new PNPageObject();
                        }
                        result.Page.Next = dicObj["next"].ToString();
                    }
                    else if (dicObj.ContainsKey("prev") && dicObj["prev"] != null)
                    {
                        if (result.Page == null)
                        {
                            result.Page = new PNPageObject();
                        }
                        result.Page.Prev = dicObj["prev"].ToString();
                    }
                }
            }
            return result;
        }
    }
}
