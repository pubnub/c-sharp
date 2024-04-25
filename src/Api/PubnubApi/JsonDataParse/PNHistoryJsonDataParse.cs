using System;
using System.Collections.Generic;
using System.Linq;

namespace PubnubApi
{
    internal static class PNHistoryJsonDataParse
    {
        internal static PNHistoryResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            PNHistoryResult ack = new PNHistoryResult();
            if (listObject == null || listObject.Count == 0)
            {
                return ack;
            }

            long historyStartTime;
            long historyEndTime;
            if (listObject.Count >= 2 && Int64.TryParse(listObject[1].ToString(), out historyStartTime))
            {
                ack.StartTimeToken = historyStartTime;
            }
            if (listObject.Count >= 2 && Int64.TryParse(listObject[2].ToString(), out historyEndTime))
            {
                ack.EndTimeToken = historyEndTime;
            }
            List<object> messagesContainer = listObject[0] as List<object>;
            if (messagesContainer == null)
            {
                object[] messagesCollection = listObject[0] as object[];
                if (messagesCollection != null && messagesCollection.Length > 0)
                {
                    messagesContainer = messagesCollection.ToList();
                }
            }
            if (messagesContainer != null)
            {
                ack.Messages = new List<PNHistoryItemResult>();
                foreach (var message in messagesContainer)
                {
                    PNHistoryItemResult result = new PNHistoryItemResult();
                    Dictionary<string, object> dicMessageTimetoken = jsonPlug.ConvertToDictionaryObject(message);
                    if (dicMessageTimetoken != null)
                    {
                        if (dicMessageTimetoken.ContainsKey("message") &&
                            (dicMessageTimetoken.ContainsKey("timetoken") || dicMessageTimetoken.ContainsKey("meta")))
                        {
                            result.Entry = dicMessageTimetoken["message"];

                            long messageTimetoken;
                            if (dicMessageTimetoken.ContainsKey("timetoken") && Int64.TryParse(dicMessageTimetoken["timetoken"].ToString(), out messageTimetoken))
                            {
                                result.Timetoken = messageTimetoken;
                            }

                            if (dicMessageTimetoken.ContainsKey("meta"))
                            {
                                result.Meta = dicMessageTimetoken["meta"];
                            }
                            if (dicMessageTimetoken.ContainsKey("uuid") && dicMessageTimetoken["uuid"] != null)
                            {
                                result.Uuid = dicMessageTimetoken["uuid"].ToString();
                            }
                            int messageType;
                            if (dicMessageTimetoken.ContainsKey("message_type") && dicMessageTimetoken["message_type"] != null && Int32.TryParse(dicMessageTimetoken["message_type"].ToString(), out messageType))
                            {
                                result.MessageType = messageType;
                            }
                        }
                        else
                        {
                            result.Entry = dicMessageTimetoken;
                        }
                    }
                    else
                    {
                        result.Entry = message;
                    }

                    ack.Messages.Add(result);
                }
            }

            return ack;
        }
    }
}
