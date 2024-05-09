using System;
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNAddMessageActionJsonDataParse
    {
        internal static PNAddMessageActionResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            Dictionary<string, object> addMsgActionDicObj = jsonPlug.ConvertToDictionaryObject(listObject[1]);
            PNAddMessageActionResult result = null;
            if (addMsgActionDicObj != null && addMsgActionDicObj.ContainsKey("data"))
            {
                result = new PNAddMessageActionResult();

                Dictionary<string, object> addMsgActionDataDic = jsonPlug.ConvertToDictionaryObject(addMsgActionDicObj["data"]);
                if (addMsgActionDataDic != null && addMsgActionDataDic.Count > 0)
                {
                    long messageTimetoken;
                    if (addMsgActionDataDic.ContainsKey("messageTimetoken") && Int64.TryParse(addMsgActionDataDic["messageTimetoken"].ToString(), out messageTimetoken))
                    {
                        result.MessageTimetoken = messageTimetoken;
                    }

                    long actionTimetoken;
                    if (addMsgActionDataDic.ContainsKey("actionTimetoken") && Int64.TryParse(addMsgActionDataDic["actionTimetoken"].ToString(), out actionTimetoken))
                    {
                        result.ActionTimetoken = actionTimetoken;
                    }

                    result.Action = new PNMessageAction
                    {
                        Type = addMsgActionDataDic.ContainsKey("type") && addMsgActionDataDic["type"] != null ? addMsgActionDataDic["type"].ToString() : null,
                        Value = addMsgActionDataDic.ContainsKey("value") && addMsgActionDataDic["value"] != null ? addMsgActionDataDic["value"].ToString() : null
                    };

                    result.Uuid = addMsgActionDataDic.ContainsKey("uuid") && addMsgActionDataDic["uuid"] != null ? addMsgActionDataDic["uuid"].ToString() : null;
                }
            }

            return result;
        }
    }
}
