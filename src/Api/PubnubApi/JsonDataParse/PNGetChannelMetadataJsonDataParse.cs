using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNGetChannelMetadataJsonDataParse
    {
        internal static PNGetChannelMetadataResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNGetChannelMetadataResult result = null;
            Dictionary<string, object> getChMetadataDicObj = (listObject != null && listObject.Count >= 2) ? jsonPlug.ConvertToDictionaryObject(listObject[1]) : null;
            if (getChMetadataDicObj != null && getChMetadataDicObj.ContainsKey("data"))
            {
                result = new PNGetChannelMetadataResult();

                Dictionary<string, object> getChMetadataDataDic = jsonPlug.ConvertToDictionaryObject(getChMetadataDicObj["data"]);
                if (getChMetadataDataDic != null && getChMetadataDataDic.Count > 0)
                {
                    var chMetadata = new PNGetChannelMetadataResult
                    {
                        Channel = (getChMetadataDataDic.ContainsKey("id") && getChMetadataDataDic["id"] != null) ? getChMetadataDataDic["id"].ToString() : null,
                        Name = (getChMetadataDataDic.ContainsKey("name") && getChMetadataDataDic["name"] != null) ? getChMetadataDataDic["name"].ToString() : null,
                        Description = (getChMetadataDataDic.ContainsKey("description") && getChMetadataDataDic["description"] != null) ? getChMetadataDataDic["description"].ToString() : null,
                        Updated = (getChMetadataDataDic.ContainsKey("updated") && getChMetadataDataDic["updated"] != null) ? getChMetadataDataDic["updated"].ToString() : null
                    };
                    if (getChMetadataDataDic.ContainsKey("custom"))
                    {
                        chMetadata.Custom = jsonPlug.ConvertToDictionaryObject(getChMetadataDataDic["custom"]);
                    }
                    result = chMetadata;
                }
            }

            return result;
        }
    }
}
