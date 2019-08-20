using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNGetSpaceJsonDataParse
    {
        internal static PNGetSpaceResult GetObject(List<object> listObject)
        {
            PNGetSpaceResult result = null;
            Dictionary<string, object> getSpaceDicObj = (listObject.Count >= 2) ? JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[1]) : null;
            if (getSpaceDicObj != null && getSpaceDicObj.ContainsKey("data"))
            {
                result = new PNGetSpaceResult();

                Dictionary<string, object> getSpaceDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(getSpaceDicObj["data"]);
                if (getSpaceDataDic != null && getSpaceDataDic.Count > 0)
                {
                    var user = new PNGetSpaceResult
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
                    result = user;
                }
            }

            return result;
        }
    }
}
