using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNMembershipsJsonDataParse
    {
        internal static PNMembershipsResult GetObject(List<object> listObject)
        {
            PNMembershipsResult result = null;
            for (int listIndex = 0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    if (result == null)
                    {
                        result = new PNMembershipsResult();
                    }
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        result.Memberships = new List<PNMembershipsItemResult>();

                        object[] channelMetadataArray = JsonDataParseInternalUtil.ConvertToObjectArray(dicObj["data"]);
                        if (channelMetadataArray != null && channelMetadataArray.Length > 0)
                        {
                            for (int index = 0; index < channelMetadataArray.Length; index++)
                            {
                                Dictionary<string, object> getMbrshipItemDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(channelMetadataArray[index]);
                                if (getMbrshipItemDataDic != null && getMbrshipItemDataDic.Count > 0)
                                {
                                    var mbrshipItem = new PNMembershipsItemResult
                                    {
                                        Updated = (getMbrshipItemDataDic.ContainsKey("updated") && getMbrshipItemDataDic["updated"] != null) ? getMbrshipItemDataDic["updated"].ToString() : null
                                    };
                                    if (getMbrshipItemDataDic.ContainsKey("custom"))
                                    {
                                        mbrshipItem.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(getMbrshipItemDataDic["custom"]);
                                    }
                                    if (getMbrshipItemDataDic.ContainsKey("channel"))
                                    {
                                        Dictionary<string, object> channelMetadataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(getMbrshipItemDataDic["channel"]);
                                        if (channelMetadataDic != null && channelMetadataDic.Count > 0)
                                        {
                                            var channelMetadataResult = new PNChannelMetadataResult
                                            {
                                                Channel = (channelMetadataDic.ContainsKey("id") && channelMetadataDic["id"] != null) ? channelMetadataDic["id"].ToString() : null,
                                                Name = (channelMetadataDic.ContainsKey("name") && channelMetadataDic["name"] != null) ? channelMetadataDic["name"].ToString() : null,
                                                Description = (channelMetadataDic.ContainsKey("description") && channelMetadataDic["description"] != null) ? channelMetadataDic["description"].ToString() : null,
                                                Updated = (channelMetadataDic.ContainsKey("updated") && channelMetadataDic["updated"] != null) ? channelMetadataDic["updated"].ToString() : null,
                                                Custom = (channelMetadataDic.ContainsKey("custom") && channelMetadataDic["custom"] != null) ? JsonDataParseInternalUtil.ConvertToDictionaryObject(channelMetadataDic["custom"]) : null
                                            };
                                            mbrshipItem.ChannelMetadata = channelMetadataResult;
                                        }
                                    }
                                    result.Memberships.Add(mbrshipItem);
                                }
                            }
                        }


                    }
                    else if (dicObj.ContainsKey("totalCount") && dicObj["totalCount"] != null)
                    {
                        int usersCount;
                        Int32.TryParse(dicObj["totalCount"].ToString(), out usersCount);
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
