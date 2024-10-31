using System;
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNMessageActionEventJsonDataParse
    {
        internal static PNMessageActionEventResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNMessageActionEventResult result = null;

            Dictionary<string, object> msgActionEventDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);
            if (msgActionEventDicObj != null)
            {
                result = new PNMessageActionEventResult();

                if (msgActionEventDicObj.ContainsKey("event") && msgActionEventDicObj["event"] != null)
                {
                    result.Event = msgActionEventDicObj["event"].ToString();
                }

                if (msgActionEventDicObj.ContainsKey("data") && msgActionEventDicObj["data"] != null)
                {
                    Dictionary<string, object> dataDic = msgActionEventDicObj["data"] as Dictionary<string, object>;
                    if (dataDic != null)
                    {
                        long messageTimetoken;
                        if (dataDic.ContainsKey("messageTimetoken") && Int64.TryParse(dataDic["messageTimetoken"].ToString(), out messageTimetoken))
                        {
                            result.MessageTimetoken = messageTimetoken;
                        }

                        long actionTimetoken;
                        if (dataDic.ContainsKey("actionTimetoken") && Int64.TryParse(dataDic["actionTimetoken"].ToString(), out actionTimetoken))
                        {
                            result.ActionTimetoken = actionTimetoken;
                        }

                        result.Action = new PNMessageAction
                        {
                            Type = dataDic.ContainsKey("type") && dataDic["type"] != null ? dataDic["type"].ToString() : null,
                            Value = dataDic.ContainsKey("value") && dataDic["value"] != null ? dataDic["value"].ToString() : null
                        };
                    }
                }
                result.Uuid = listObject[3].ToString();
                if (listObject.Count == 6) {
                    result.Subscription = listObject[4].ToString();
                    result.Channel = listObject[5].ToString();
                } else if (listObject.Count == 5) {
                    result.Channel = listObject[4].ToString();
                }

            }

            return result;
        }
    }

}
