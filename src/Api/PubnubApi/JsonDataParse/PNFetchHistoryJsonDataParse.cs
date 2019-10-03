using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNFetchHistoryJsonDataParse
    {
        internal static PNFetchHistoryResult GetObject(List<object> listObject)
        {
            PNFetchHistoryResult ack = new PNFetchHistoryResult();

            Dictionary<string, List<object>> channelMessagesContainer = null;
            if (listObject.Count >= 1)
            {
                channelMessagesContainer = listObject[0] as Dictionary<string, List<object>>;
            }

            if (channelMessagesContainer != null)
            {
                ack.Messages = new Dictionary<string, List<PNHistoryItemResult>>();
                foreach(var channelKVP in channelMessagesContainer)
                {
                    string channel = channelKVP.Key;
                    List<PNHistoryItemResult> resultList = new List<PNHistoryItemResult>();
                    object[] channelValArray = channelKVP.Value != null ? JsonDataParseInternalUtil.ConvertToObjectArray(channelKVP.Value) : new object[0];
                    foreach(object msgContainerObj in channelValArray)
                    {
                        Dictionary<string, object> messagesContainer = JsonDataParseInternalUtil.ConvertToDictionaryObject(msgContainerObj);
                        if (messagesContainer != null)
                        {
                            PNHistoryItemResult result = new PNHistoryItemResult();
                            if (messagesContainer.ContainsKey("message") &&
                                (messagesContainer.ContainsKey("timetoken") || messagesContainer.ContainsKey("meta")))
                            {
                                result.Entry = messagesContainer["message"];

                                long messageTimetoken;
                                if (messagesContainer.ContainsKey("timetoken") && Int64.TryParse(messagesContainer["timetoken"].ToString(), out messageTimetoken))
                                {
                                    result.Timetoken = messageTimetoken;
                                }

                                if (messagesContainer.ContainsKey("meta"))
                                {
                                    result.Meta = messagesContainer["meta"];
                                }
                            }
                            resultList.Add(result);
                        }
                    }
                    ack.Messages.Add(channel, resultList);
                }
            }

            return ack;
        }
    }
}
