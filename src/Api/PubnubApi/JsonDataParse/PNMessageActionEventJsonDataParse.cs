using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNMessageActionEventJsonDataParse
    {
        internal static PNMessageActionEventResult GetObject(List<object> listObject)
        {
            PNMessageActionEventResult result = null;

            Dictionary<string, object> msgActionEventDicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[0]);
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
                result.Channel = listObject[4].ToString();
            }

            return result;
        }
    }

}
