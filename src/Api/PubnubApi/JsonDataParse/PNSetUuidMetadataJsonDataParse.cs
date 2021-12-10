using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNSetUuidMetadataJsonDataParse
    {
        internal static PNSetUuidMetadataResult GetObject(List<object> listObject)
        {
            Dictionary<string, object> setUuidDicObj = (listObject != null && listObject.Count >= 2) ? JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[1]) : null;
            PNSetUuidMetadataResult result = null;
            if (setUuidDicObj != null && setUuidDicObj.ContainsKey("data"))
            {
                result = new PNSetUuidMetadataResult();

                Dictionary<string, object> setUuidDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(setUuidDicObj["data"]);
                if (setUuidDataDic != null && setUuidDataDic.Count > 0)
                {
                    result.Uuid = setUuidDataDic.ContainsKey("id") && setUuidDataDic["id"] != null ? setUuidDataDic["id"].ToString() : null;
                    result.Name = setUuidDataDic.ContainsKey("name") && setUuidDataDic["name"] != null ? setUuidDataDic["name"].ToString() : null;
                    result.ExternalId = setUuidDataDic.ContainsKey("externalId") && setUuidDataDic["externalId"] != null ? setUuidDataDic["externalId"].ToString() : null;
                    result.ProfileUrl = setUuidDataDic.ContainsKey("profileUrl") && setUuidDataDic["profileUrl"] != null ? setUuidDataDic["profileUrl"].ToString() : null;
                    result.Email = setUuidDataDic.ContainsKey("email") && setUuidDataDic["email"] != null ? setUuidDataDic["email"].ToString() : null;
                    result.Updated = setUuidDataDic.ContainsKey("updated") && setUuidDataDic["updated"] != null ? setUuidDataDic["updated"].ToString() : "";
                    if (setUuidDataDic.ContainsKey("custom"))
                    {
                        result.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(setUuidDataDic["custom"]);
                    }
                }
            }

            return result;
        }
    }
}
