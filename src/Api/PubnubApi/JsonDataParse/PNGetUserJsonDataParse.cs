using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNGetUserJsonDataParse
    {
        internal static PNGetUserResult GetObject(List<object> listObject)
        {
            PNGetUserResult result = null;
            Dictionary<string, object> getUserDicObj = (listObject.Count >= 2) ? JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[1]) : null;
            if (getUserDicObj != null && getUserDicObj.ContainsKey("data"))
            {
                result = new PNGetUserResult();

                Dictionary<string, object> userDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(getUserDicObj["data"]);
                if (userDataDic != null && userDataDic.Count > 0)
                {
                    var usrData = new PNGetUserResult
                    {
                        Id = (userDataDic.ContainsKey("id") && userDataDic["id"] != null) ? userDataDic["id"].ToString() : "",
                        Name = (userDataDic.ContainsKey("name") && userDataDic["name"] != null) ? userDataDic["name"].ToString() : "",
                        ExternalId = (userDataDic.ContainsKey("externalId") && userDataDic["externalId"] != null) ? userDataDic["externalId"].ToString() : "",
                        ProfileUrl = (userDataDic.ContainsKey("profileUrl") && userDataDic["profileUrl"] != null) ? userDataDic["profileUrl"].ToString() : "",
                        Email = (userDataDic.ContainsKey("email") && userDataDic["email"] != null) ? userDataDic["email"].ToString() : "",
                        Created = (userDataDic.ContainsKey("created") && userDataDic["created"] != null) ? userDataDic["created"].ToString() : "",
                        Updated = (userDataDic.ContainsKey("updated") && userDataDic["updated"] != null) ? userDataDic["updated"].ToString() : ""
                    };

                    if (userDataDic.ContainsKey("custom"))
                    {
                        usrData.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(userDataDic["custom"]);
                    }
                    result = usrData;
                }
            }

            return result;
        }
    }
}
