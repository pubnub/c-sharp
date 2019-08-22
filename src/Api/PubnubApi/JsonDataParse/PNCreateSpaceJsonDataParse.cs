﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNCreateSpaceJsonDataParse
    {
        internal static PNCreateSpaceResult GetObject(List<object> listObject)
        {
            Dictionary<string, object> createSpaceDicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[1]);
            PNCreateSpaceResult result = null;
            if (createSpaceDicObj != null && createSpaceDicObj.ContainsKey("data"))
            {
                result = new PNCreateSpaceResult();

                Dictionary<string, object> getCreateSpaceDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(createSpaceDicObj["data"]);
                if (getCreateSpaceDataDic != null && getCreateSpaceDataDic.Count > 0)
                {
                    result.Id = getCreateSpaceDataDic.ContainsKey("id") && getCreateSpaceDataDic["id"] != null ? getCreateSpaceDataDic["id"].ToString() : null;
                    result.Name = getCreateSpaceDataDic.ContainsKey("name") && getCreateSpaceDataDic["name"] != null ? getCreateSpaceDataDic["name"].ToString() : null;
                    result.Description = getCreateSpaceDataDic.ContainsKey("description") && getCreateSpaceDataDic["description"] != null ? getCreateSpaceDataDic["description"].ToString() : null;
                    result.Created = getCreateSpaceDataDic.ContainsKey("created") && getCreateSpaceDataDic["created"] != null ? getCreateSpaceDataDic["created"].ToString() : null;
                    result.Updated = getCreateSpaceDataDic.ContainsKey("updated") && getCreateSpaceDataDic["updated"] != null ? getCreateSpaceDataDic["updated"].ToString() : null;
                    if (getCreateSpaceDataDic.ContainsKey("custom"))
                    {
                        result.Custom = JsonDataParseInternalUtil.ConvertToDictionaryObject(getCreateSpaceDataDic["custom"]);
                    }
                }
            }

            return result;
        }
    }
}
