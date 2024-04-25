using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNRemoveMessageActionJsonDataParse
    {
        internal static PNRemoveMessageActionResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            Dictionary<string, object> removeMsgActionDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);
            PNRemoveMessageActionResult result = null;
            if (removeMsgActionDicObj != null && removeMsgActionDicObj.ContainsKey("status"))
            {
                int status;
                var _ = int.TryParse(removeMsgActionDicObj["status"].ToString(), out status);
                if (status == 200)
                {
                    result = new PNRemoveMessageActionResult();
                }
            }

            return result;
        }
    }
}
