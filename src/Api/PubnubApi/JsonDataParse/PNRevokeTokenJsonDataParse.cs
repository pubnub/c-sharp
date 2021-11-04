using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNRevokeTokenJsonDataParse
    {
        internal static PNAccessManagerRevokeTokenResult GetObject(List<object> listObject)
        {
            PNAccessManagerRevokeTokenResult result = null;

            Dictionary<string, object> revokeTokenDictObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[0]);
            if (revokeTokenDictObj != null && revokeTokenDictObj.ContainsKey("data"))
            {
                result = new PNAccessManagerRevokeTokenResult();
            }

            return result;
        }

    }
}
