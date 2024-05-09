using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNSetChannelMetadataJsonDataParse
    {
        internal static PNSetChannelMetadataResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            Dictionary<string, object> setChannelMetadataDicObj = (listObject != null && listObject.Count >= 2) ? jsonPlug.ConvertToDictionaryObject(listObject[1]) : null;
            PNSetChannelMetadataResult result = null;
            if (setChannelMetadataDicObj != null && setChannelMetadataDicObj.ContainsKey("data"))
            {
                result = new PNSetChannelMetadataResult();

                Dictionary<string, object> getSetChMetadataDataDic = jsonPlug.ConvertToDictionaryObject(setChannelMetadataDicObj["data"]);
                if (getSetChMetadataDataDic != null && getSetChMetadataDataDic.Count > 0)
                {
                    result.Channel = getSetChMetadataDataDic.ContainsKey("id") && getSetChMetadataDataDic["id"] != null ? getSetChMetadataDataDic["id"].ToString() : null;
                    result.Name = getSetChMetadataDataDic.ContainsKey("name") && getSetChMetadataDataDic["name"] != null ? getSetChMetadataDataDic["name"].ToString() : null;
                    result.Description = getSetChMetadataDataDic.ContainsKey("description") && getSetChMetadataDataDic["description"] != null ? getSetChMetadataDataDic["description"].ToString() : null;
                    result.Updated = getSetChMetadataDataDic.ContainsKey("updated") && getSetChMetadataDataDic["updated"] != null ? getSetChMetadataDataDic["updated"].ToString() : null;
                    if (getSetChMetadataDataDic.ContainsKey("custom"))
                    {
                        result.Custom = jsonPlug.ConvertToDictionaryObject(getSetChMetadataDataDic["custom"]);
                    }
                }
            }

            return result;
        }
    }
}
