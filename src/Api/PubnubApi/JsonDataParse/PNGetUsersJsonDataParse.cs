using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNGetUsersJsonDataParse
    {
        internal static PNGetUsersResult GetObject(List<object> listObject)
        {
            PNGetUsersResult result = null;
            for (int listIndex=0; listIndex < listObject.Count; listIndex++)
            {
                Dictionary<string, object> dicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[listIndex]);
                if (dicObj != null && dicObj.Count > 0)
                {
                    if  (result == null)
                    {
                        result = new PNGetUsersResult();
                    }
                    if (dicObj.ContainsKey("data") && dicObj["data"] != null)
                    {
                        result.Users = new List<PNUserResult>();

                        Dictionary<string, object> getUserDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(dicObj["data"]);
                        if (getUserDataDic != null && getUserDataDic.Count > 0)
                        {
                            var user = new PNUserResult
                            {
                                Id = (getUserDataDic.ContainsKey("id") && getUserDataDic["id"] != null) ? getUserDataDic["id"].ToString() : null,
                                Name = (getUserDataDic.ContainsKey("name") && getUserDataDic["name"] != null) ? getUserDataDic["name"].ToString() : null,
                                ExternalId = (getUserDataDic.ContainsKey("externalId") && getUserDataDic["externalId"] != null) ? getUserDataDic["externalId"].ToString() : null,
                                ProfileUrl = (getUserDataDic.ContainsKey("profileUrl") && getUserDataDic["profileUrl"] != null) ? getUserDataDic["profileUrl"].ToString() : null,
                                Email = (getUserDataDic.ContainsKey("email") && getUserDataDic["email"] != null) ? getUserDataDic["email"].ToString() : null,
                                Created = (getUserDataDic.ContainsKey("created") && getUserDataDic["created"] != null) ? getUserDataDic["created"].ToString() : null,
                                Updated = (getUserDataDic.ContainsKey("updated") && getUserDataDic["updated"] != null) ? getUserDataDic["updated"].ToString() : null
                            };
                            if (getUserDataDic.ContainsKey("custom"))
                            {
                                user.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(getUserDataDic["custom"]);
                            }
                            result.Users.Add(user);
                        }
                        else
                        {
                            object[] userDataArray = JsonDataParseInternalUtil.ConvertToObjectArray(dicObj["data"]);
                            if (userDataArray != null && userDataArray.Length > 0)
                            {
                                for (int index = 0; index < userDataArray.Length; index++)
                                {
                                    Dictionary<string, object> userDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(userDataArray[index]);
                                    if (userDataDic != null && userDataDic.Count > 0)
                                    {
                                        var usrData = new PNUserResult
                                        {
                                            Id = (userDataDic.ContainsKey("id") && userDataDic["id"] != null) ? userDataDic["id"].ToString() : null,
                                            Name = (userDataDic.ContainsKey("name") && userDataDic["name"] != null) ? userDataDic["name"].ToString() : null,
                                            ExternalId = (userDataDic.ContainsKey("externalId") && userDataDic["externalId"] != null) ? userDataDic["externalId"].ToString() : null,
                                            ProfileUrl = (userDataDic.ContainsKey("profileUrl") && userDataDic["profileUrl"] != null) ? userDataDic["profileUrl"].ToString() : null,
                                            Email = (userDataDic.ContainsKey("email") && userDataDic["email"] != null) ? userDataDic["email"].ToString() : null,
                                            Created = (userDataDic.ContainsKey("created") && userDataDic["created"] != null) ? userDataDic["created"].ToString() : null,
                                            Updated = (userDataDic.ContainsKey("updated") && userDataDic["updated"] != null) ? userDataDic["updated"].ToString() : null
                                        };

                                        if (userDataDic.ContainsKey("custom"))
                                        {
                                            usrData.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(userDataDic["custom"]);
                                        }
                                        result.Users.Add(usrData);
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
