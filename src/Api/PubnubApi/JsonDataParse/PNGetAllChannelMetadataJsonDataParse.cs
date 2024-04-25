using System;
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNGetAllChannelMetadataJsonDataParse
    {
        internal static PNGetAllChannelMetadataResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNGetAllChannelMetadataResult result = null;
            for (int listIndex = 0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = jsonPlug.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    if (result == null)
                    {
                        result = new PNGetAllChannelMetadataResult();
                    }
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        result.Channels = new List<PNChannelMetadataResult>();

                        Dictionary<string, object> getChMetadataDataDic = jsonPlug.ConvertToDictionaryObject(dicObj["data"]);
                        if (getChMetadataDataDic != null && getChMetadataDataDic.Count > 0)
                        {
                            var chMetadata = new PNChannelMetadataResult
                            {
                                Channel = (getChMetadataDataDic.ContainsKey("id") && getChMetadataDataDic["id"] != null) ? getChMetadataDataDic["id"].ToString() : null,
                                Name = (getChMetadataDataDic.ContainsKey("name") && getChMetadataDataDic["name"] != null) ? getChMetadataDataDic["name"].ToString() : null,
                                Description = (getChMetadataDataDic.ContainsKey("description") && getChMetadataDataDic["description"] != null) ? getChMetadataDataDic["description"].ToString() : null,
                                Updated = (getChMetadataDataDic.ContainsKey("updated") && getChMetadataDataDic["updated"] != null) ? getChMetadataDataDic["updated"].ToString() : null
                            };
                            if (getChMetadataDataDic.ContainsKey("custom"))
                            {
                                chMetadata.Custom = jsonPlug.ConvertToDictionaryObject(getChMetadataDataDic["custom"]);
                            }
                            result.Channels.Add(chMetadata);
                        }
                        else
                        {
                            object[] chMetadataDataArray = jsonPlug.ConvertToObjectArray(dicObj["data"]);
                            if (chMetadataDataArray != null && chMetadataDataArray.Length > 0)
                            {
                                for (int index = 0; index < chMetadataDataArray.Length; index++)
                                {
                                    Dictionary<string, object> chMetadataDataDic = jsonPlug.ConvertToDictionaryObject(chMetadataDataArray[index]);
                                    if (chMetadataDataDic != null && chMetadataDataDic.Count > 0)
                                    {
                                        var chMetadataData = new PNChannelMetadataResult
                                        {
                                            Channel = (chMetadataDataDic.ContainsKey("id") && chMetadataDataDic["id"] != null) ? chMetadataDataDic["id"].ToString() : null,
                                            Name = (chMetadataDataDic.ContainsKey("name") && chMetadataDataDic["name"] != null) ? chMetadataDataDic["name"].ToString() : null,
                                            Description = (chMetadataDataDic.ContainsKey("description") && chMetadataDataDic["description"] != null) ? chMetadataDataDic["description"].ToString() : null,
                                            Updated = (chMetadataDataDic.ContainsKey("updated") && chMetadataDataDic["updated"] != null) ? chMetadataDataDic["updated"].ToString() : null
                                        };

                                        if (chMetadataDataDic.ContainsKey("custom"))
                                        {
                                            chMetadataData.Custom = jsonPlug.ConvertToDictionaryObject(chMetadataDataDic["custom"]);
                                        }
                                        result.Channels.Add(chMetadataData);
                                    }
                                }
                            }
                        }

                    }
                    else if (dicObj.ContainsKey("totalCount") && dicObj["totalCount"] != null)
                    {
                        int chMetadataCount;
                        var _ = Int32.TryParse(dicObj["totalCount"].ToString(), out chMetadataCount);
                        result.TotalCount = chMetadataCount;
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
