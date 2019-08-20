using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNUpdateUserJsonDataParse
    {
        internal static PNUpdateUserResult GetObject(List<object> listObject)
        {
            Dictionary<string, object> updateUserDicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[1]);
            PNUpdateUserResult result = null;
            if (updateUserDicObj != null && updateUserDicObj.ContainsKey("data"))
            {
                result = new PNUpdateUserResult();

                Dictionary<string, object> getUpdateUserDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(updateUserDicObj["data"]);
                if (getUpdateUserDataDic != null && getUpdateUserDataDic.Count > 0)
                {
                    result.Id = getUpdateUserDataDic.ContainsKey("id") ? getUpdateUserDataDic["id"].ToString() : "";
                    result.Name = getUpdateUserDataDic.ContainsKey("name") ? getUpdateUserDataDic["name"].ToString() : "";
                    result.ExternalId = getUpdateUserDataDic.ContainsKey("externalId") && getUpdateUserDataDic["externalId"] != null ? getUpdateUserDataDic["externalId"].ToString() : null;
                    result.ProfileUrl = getUpdateUserDataDic.ContainsKey("profileUrl") && getUpdateUserDataDic["profileUrl"] != null ? getUpdateUserDataDic["profileUrl"].ToString() : null;
                    result.Email = getUpdateUserDataDic.ContainsKey("email") && getUpdateUserDataDic["email"] != null ? getUpdateUserDataDic["email"].ToString() : null;
                    result.Created = getUpdateUserDataDic.ContainsKey("created") && getUpdateUserDataDic["created"] != null ? getUpdateUserDataDic["created"].ToString() : "";
                    result.Updated = getUpdateUserDataDic.ContainsKey("updated") && getUpdateUserDataDic["updated"] != null ? getUpdateUserDataDic["updated"].ToString() : "";
                    if (getUpdateUserDataDic.ContainsKey("custom"))
                    {
                        result.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(getUpdateUserDataDic["custom"]);
                    }
                }
            }

            return result;
        }
    }

}
