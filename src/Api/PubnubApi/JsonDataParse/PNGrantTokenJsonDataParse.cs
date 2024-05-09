using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNGrantTokenJsonDataParse
    {
        internal static PNAccessManagerTokenResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNAccessManagerTokenResult result = null;

            Dictionary<string, object> grantDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);
            if (grantDicObj != null && grantDicObj.ContainsKey("data"))
            {
                result = new PNAccessManagerTokenResult();
                Dictionary<string, object> grantDataDic = jsonPlug.ConvertToDictionaryObject(grantDicObj["data"]);
                if (grantDataDic != null && grantDataDic.ContainsKey("token"))
                {
                    result.Token = grantDataDic["token"].ToString();
                }
            }

                

            return result;
        }
    }
}
