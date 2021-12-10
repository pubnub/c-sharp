using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNGrantTokenJsonDataParse
    {
        internal static PNAccessManagerTokenResult GetObject(List<object> listObject)
        {
            PNAccessManagerTokenResult result = null;

            Dictionary<string, object> grantDicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[0]);
            if (grantDicObj != null && grantDicObj.ContainsKey("data"))
            {
                result = new PNAccessManagerTokenResult();
                Dictionary<string, object> grantDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantDicObj["data"]);
                if (grantDataDic != null && grantDataDic.ContainsKey("token"))
                {
                    result.Token = grantDataDic["token"].ToString();
                }
            }

                

            return result;
        }
    }
}
