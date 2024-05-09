using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNGetUuidMetadataJsonDataParse
    {
        internal static PNGetUuidMetadataResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNGetUuidMetadataResult result = null;
            Dictionary<string, object> getUserDicObj = (listObject.Count >= 2) ? jsonPlug.ConvertToDictionaryObject(listObject[1]) : null;
            if (getUserDicObj != null && getUserDicObj.ContainsKey("data"))
            {
                result = new PNGetUuidMetadataResult();

                Dictionary<string, object> userDataDic = jsonPlug.ConvertToDictionaryObject(getUserDicObj["data"]);
                if (userDataDic != null && userDataDic.Count > 0)
                {
                    var usrData = new PNGetUuidMetadataResult
                    {
                        Uuid = (userDataDic.ContainsKey("id") && userDataDic["id"] != null) ? userDataDic["id"].ToString() : null,
                        Name = (userDataDic.ContainsKey("name") && userDataDic["name"] != null) ? userDataDic["name"].ToString() : null,
                        ExternalId = (userDataDic.ContainsKey("externalId") && userDataDic["externalId"] != null) ? userDataDic["externalId"].ToString() : null,
                        ProfileUrl = (userDataDic.ContainsKey("profileUrl") && userDataDic["profileUrl"] != null) ? userDataDic["profileUrl"].ToString() : null,
                        Email = (userDataDic.ContainsKey("email") && userDataDic["email"] != null) ? userDataDic["email"].ToString() : null,
                        Updated = (userDataDic.ContainsKey("updated") && userDataDic["updated"] != null) ? userDataDic["updated"].ToString() : null
                    };

                    if (userDataDic.ContainsKey("custom"))
                    {
                        usrData.Custom = jsonPlug.ConvertToDictionaryObject(userDataDic["custom"]);
                    }
                    result = usrData;
                }
            }

            return result;
        }
    }
}
