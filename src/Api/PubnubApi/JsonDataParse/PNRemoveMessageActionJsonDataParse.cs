using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNRemoveMessageActionJsonDataParse
    {
        internal static PNRemoveMessageActionResult GetObject(List<object> listObject)
        {
            Dictionary<string, object> removeMsgActionDicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[0]);
            PNRemoveMessageActionResult result = null;
            if (removeMsgActionDicObj != null && removeMsgActionDicObj.ContainsKey("status"))
            {
                int status;
                int.TryParse(removeMsgActionDicObj["status"].ToString(), out status);
                if (status == 200)
                {
                    result = new PNRemoveMessageActionResult();
                }
            }

            return result;
        }
    }
}
