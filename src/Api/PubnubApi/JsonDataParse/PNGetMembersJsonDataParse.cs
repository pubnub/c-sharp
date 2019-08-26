using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNGetMembersJsonDataParse
    {
        internal static PNGetMembersResult GetObject(List<object> listObject)
        {
            PNGetMembersResult result = null;
            for (int listIndex = 0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    if (result == null)
                    {
                        result = new PNGetMembersResult();
                    }
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        result.Members = new List<PNGetMembersItemResult>();

                        object[] userArray = JsonDataParseInternalUtil.ConvertToObjectArray(dicObj["data"]);
                        if (userArray != null && userArray.Length > 0)
                        {
                            for (int index = 0; index < userArray.Length; index++)
                            {
                                Dictionary<string, object> getMbrItemDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(userArray[index]);
                                if (getMbrItemDataDic != null && getMbrItemDataDic.Count > 0)
                                {
                                    var mbrItem = new PNGetMembersItemResult
                                    {
                                        UserId = (getMbrItemDataDic.ContainsKey("id") && getMbrItemDataDic["id"] != null) ? getMbrItemDataDic["id"].ToString() : null,
                                        Created = (getMbrItemDataDic.ContainsKey("created") && getMbrItemDataDic["created"] != null) ? getMbrItemDataDic["created"].ToString() : null,
                                        Updated = (getMbrItemDataDic.ContainsKey("updated") && getMbrItemDataDic["updated"] != null) ? getMbrItemDataDic["updated"].ToString() : null
                                    };
                                    if (getMbrItemDataDic.ContainsKey("custom"))
                                    {
                                        mbrItem.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(getMbrItemDataDic["custom"]);
                                    }
                                    if (getMbrItemDataDic.ContainsKey("user") && getMbrItemDataDic["user"] != null)
                                    {
                                        Dictionary<string, object> userDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(getMbrItemDataDic["user"]);
                                        if (userDic != null && userDic.Count > 0)
                                        {
                                            var userResult = new PNUserResult
                                            {
                                                Id = (userDic.ContainsKey("id") && userDic["id"] != null) ? userDic["id"].ToString() : null,
                                                Name = (userDic.ContainsKey("name") && userDic["name"] != null) ? userDic["name"].ToString() : null,
                                                ExternalId = (userDic.ContainsKey("externalId") && userDic["externalId"] != null) ? userDic["externalId"].ToString() : null,
                                                ProfileUrl = (userDic.ContainsKey("profileUrl") && userDic["profileUrl"] != null) ? userDic["profileUrl"].ToString() : null,
                                                Email = (userDic.ContainsKey("email") && userDic["email"] != null) ? userDic["email"].ToString() : null,
                                                Created = (userDic.ContainsKey("created") && userDic["created"] != null) ? userDic["created"].ToString() : null,
                                                Updated = (userDic.ContainsKey("updated") && userDic["updated"] != null) ? userDic["updated"].ToString() : null
                                            };
                                            if (userDic.ContainsKey("custom"))
                                            {
                                                userResult.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(userDic["custom"]);
                                            }

                                            mbrItem.User = userResult;
                                        }
                                    }
                                    result.Members.Add(mbrItem);
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
