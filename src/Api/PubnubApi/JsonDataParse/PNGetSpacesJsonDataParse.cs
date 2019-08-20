using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNGetSpacesJsonDataParse
    {
        internal static PNGetSpacesResult GetObject(List<object> listObject)
        {
            PNGetSpacesResult result = null;
            for (int listIndex = 0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    result = new PNGetSpacesResult();
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        result.Spaces = new List<PNSpaceResult>();

                        Dictionary<string, object> getSpaceDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(dicObj["data"]);
                        if (getSpaceDataDic != null && getSpaceDataDic.Count > 0)
                        {
                            var user = new PNSpaceResult
                            {
                                Id = (getSpaceDataDic.ContainsKey("id") && getSpaceDataDic["id"] != null) ? getSpaceDataDic["id"].ToString() : "",
                                Name = (getSpaceDataDic.ContainsKey("name") && getSpaceDataDic["name"] != null) ? getSpaceDataDic["name"].ToString() : "",
                                Description = (getSpaceDataDic.ContainsKey("description") && getSpaceDataDic["description"] != null) ? getSpaceDataDic["description"].ToString() : "",
                                Created = (getSpaceDataDic.ContainsKey("created") && getSpaceDataDic["created"] != null) ? getSpaceDataDic["created"].ToString() : "",
                                Updated = (getSpaceDataDic.ContainsKey("updated") && getSpaceDataDic["updated"] != null) ? getSpaceDataDic["updated"].ToString() : ""
                            };
                            if (getSpaceDataDic.ContainsKey("custom"))
                            {
                                user.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(getSpaceDataDic["custom"]);
                            }
                            result.Spaces.Add(user);
                        }
                        else
                        {
                            object[] spaceDataArray = JsonDataParseInternalUtil.ConvertToObjectArray(dicObj["data"]);
                            if (spaceDataArray != null && spaceDataArray.Length > 0)
                            {
                                for (int index = 0; index < spaceDataArray.Length; index++)
                                {
                                    Dictionary<string, object> spaceDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(spaceDataArray[index]);
                                    if (spaceDataDic != null && spaceDataDic.Count > 0)
                                    {
                                        var spcData = new PNSpaceResult
                                        {
                                            Id = (spaceDataDic.ContainsKey("id") && spaceDataDic["id"] != null) ? spaceDataDic["id"].ToString() : "",
                                            Name = (spaceDataDic.ContainsKey("name") && spaceDataDic["name"] != null) ? spaceDataDic["name"].ToString() : "",
                                            Description = (spaceDataDic.ContainsKey("description") && spaceDataDic["description"] != null) ? spaceDataDic["description"].ToString() : "",
                                            Created = (spaceDataDic.ContainsKey("created") && spaceDataDic["created"] != null) ? spaceDataDic["created"].ToString() : "",
                                            Updated = (spaceDataDic.ContainsKey("updated") && spaceDataDic["updated"] != null) ? spaceDataDic["updated"].ToString() : ""
                                        };

                                        if (spaceDataDic.ContainsKey("custom"))
                                        {
                                            spcData.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(spaceDataDic["custom"]);
                                        }
                                        result.Spaces.Add(spcData);
                                    }
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
                            result.Page = new PNPage();
                        }
                        result.Page.Next = dicObj["next"].ToString();
                    }
                    else if (dicObj.ContainsKey("prev") && dicObj["prev"] != null)
                    {
                        if (result.Page == null)
                        {
                            result.Page = new PNPage();
                        }
                        result.Page.Prev = dicObj["prev"].ToString();
                    }
                }
            }

            return result;
        }
    }
}
