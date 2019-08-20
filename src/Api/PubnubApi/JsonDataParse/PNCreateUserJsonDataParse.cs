using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNCreateUserJsonDataParse
    {
        internal static PNCreateUserResult GetObject(List<object> listObject)
        {
            Dictionary<string, object> createUserDicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[1]);
            PNCreateUserResult result = null;
            if (createUserDicObj != null && createUserDicObj.ContainsKey("data"))
            {
                result = new PNCreateUserResult();

                Dictionary<string, object> getCreateUserDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(createUserDicObj["data"]);
                if (getCreateUserDataDic != null && getCreateUserDataDic.Count > 0)
                {
                    result.Id = getCreateUserDataDic.ContainsKey("id") ? getCreateUserDataDic["id"].ToString() : "";
                    result.Name = getCreateUserDataDic.ContainsKey("name") ? getCreateUserDataDic["name"].ToString() : "";
                    result.ExternalId = getCreateUserDataDic.ContainsKey("externalId") && getCreateUserDataDic["externalId"] != null ? getCreateUserDataDic["externalId"].ToString() : null;
                    result.ProfileUrl = getCreateUserDataDic.ContainsKey("profileUrl") && getCreateUserDataDic["profileUrl"] != null ? getCreateUserDataDic["profileUrl"].ToString() : null;
                    result.Email = getCreateUserDataDic.ContainsKey("email") && getCreateUserDataDic["email"] != null ? getCreateUserDataDic["email"].ToString() : null;
                    result.Created = getCreateUserDataDic.ContainsKey("created") && getCreateUserDataDic["created"] != null ? getCreateUserDataDic["created"].ToString() : "";
                    result.Updated = getCreateUserDataDic.ContainsKey("updated") && getCreateUserDataDic["updated"] != null ? getCreateUserDataDic["updated"].ToString() : "";
                    if (getCreateUserDataDic.ContainsKey("custom"))
                    {
                        result.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(getCreateUserDataDic["custom"]);
                    }
                }
            }

            return result;
        }
    }
}
