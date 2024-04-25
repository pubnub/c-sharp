using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNRevokeTokenJsonDataParse
    {
        internal static PNAccessManagerRevokeTokenResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNAccessManagerRevokeTokenResult result = null;

            Dictionary<string, object> revokeTokenDictObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);
            if (revokeTokenDictObj != null && revokeTokenDictObj.ContainsKey("data"))
            {
                result = new PNAccessManagerRevokeTokenResult();
            }

            return result;
        }

    }
}
