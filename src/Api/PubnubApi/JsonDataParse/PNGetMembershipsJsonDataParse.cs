using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNGetMembershipsJsonDataParse
    {
        internal static PNGetMembershipsResult GetObject(List<object> listObject)
        {
            PNGetMembershipsResult result = null;
            for (int listIndex = 0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    result = new PNGetMembershipsResult();
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        result.Memberships = new List<PNMembershipsItemResult>();

                        object[] spaceArray = JsonDataParseInternalUtil.ConvertToObjectArray(dicObj["data"]);
                        if (spaceArray != null && spaceArray.Length > 0)
                        {
                            for (int index = 0; index < spaceArray.Length; index++)
                            {
                                Dictionary<string, object> getMbrshipItemDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(spaceArray[index]);
                                if (getMbrshipItemDataDic != null && getMbrshipItemDataDic.Count > 0)
                                {
                                    var mbrshipItem = new PNMembershipsItemResult
                                    {
                                        SpaceId = (getMbrshipItemDataDic.ContainsKey("id") && getMbrshipItemDataDic["id"] != null) ? getMbrshipItemDataDic["id"].ToString() : "",
                                        Created = (getMbrshipItemDataDic.ContainsKey("created") && getMbrshipItemDataDic["created"] != null) ? getMbrshipItemDataDic["created"].ToString() : "",
                                        Updated = (getMbrshipItemDataDic.ContainsKey("updated") && getMbrshipItemDataDic["updated"] != null) ? getMbrshipItemDataDic["updated"].ToString() : ""
                                    };
                                    if (getMbrshipItemDataDic.ContainsKey("custom"))
                                    {
                                        mbrshipItem.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(getMbrshipItemDataDic["custom"]);
                                    }
                                    if (getMbrshipItemDataDic.ContainsKey("space"))
                                    {
                                        Dictionary<string, object> spaceDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(getMbrshipItemDataDic["space"]);
                                        if (spaceDic != null && spaceDic.Count > 0)
                                        {
                                            var spaceResult = new PNGetSpaceResult
                                            {
                                                Id = (spaceDic.ContainsKey("id") && spaceDic["id"] != null) ? spaceDic["id"].ToString() : "",
                                                Name = (spaceDic.ContainsKey("name") && spaceDic["name"] != null) ? spaceDic["name"].ToString() : "",
                                                Description = (spaceDic.ContainsKey("description") && spaceDic["description"] != null) ? spaceDic["description"].ToString() : "",
                                                Created = (spaceDic.ContainsKey("created") && spaceDic["created"] != null) ? spaceDic["created"].ToString() : "",
                                                Updated = (spaceDic.ContainsKey("updated") && spaceDic["updated"] != null) ? spaceDic["updated"].ToString() : ""
                                            };
                                            if (spaceDic.ContainsKey("custom"))
                                            {
                                                spaceResult.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(spaceDic["custom"]);
                                            }

                                            mbrshipItem.Space = spaceResult;
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
