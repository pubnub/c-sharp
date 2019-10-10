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
            for (int index=0; index < listObject.Count; index++)
            {
                if (listObject[index].GetType() == typeof(List<object>))
                {
                    List<object> channelMessagesList = listObject[index] as List<object>;
                    if (channelMessagesList.Count >= 1)
                    {
                        channelMessagesContainer = channelMessagesList[0] as Dictionary<string, List<object>>;
                    }
                    if (channelMessagesContainer != null)
                    {
                        ack.Messages = new Dictionary<string, List<PNHistoryItemResult>>();
                        foreach (var channelKVP in channelMessagesContainer)
                        {
                            string channel = channelKVP.Key;
                            List<PNHistoryItemResult> resultList = new List<PNHistoryItemResult>();
                            object[] channelValArray = channelKVP.Value != null ? JsonDataParseInternalUtil.ConvertToObjectArray(channelKVP.Value) : new object[0];
                            foreach (object msgContainerObj in channelValArray)
                            {
                                Dictionary<string, object> messagesContainer = JsonDataParseInternalUtil.ConvertToDictionaryObject(msgContainerObj);
                                if (messagesContainer != null)
                                {
                                    PNHistoryItemResult result = new PNHistoryItemResult();
                                    if (messagesContainer.ContainsKey("message") &&
                                        (messagesContainer.ContainsKey("timetoken") || messagesContainer.ContainsKey("meta") || messagesContainer.ContainsKey("actions")))
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

                                        if (messagesContainer.ContainsKey("actions"))
                                        {
                                            result.Actions = messagesContainer["actions"];
                                        }
                                    }
                                    resultList.Add(result);
                                }
                            }
                            ack.Messages.Add(channel, resultList);
                        }
                    }
                }
                else if (listObject[index].GetType() == typeof(Dictionary<string, object>))
                {
                    Dictionary<string, object> moreContainer = listObject[index] as Dictionary<string, object>;
                    if (moreContainer != null && moreContainer.ContainsKey("more"))
                    {
                        Dictionary<string, object> moreDic = moreContainer["more"] as Dictionary<string, object>;
                        if (moreDic != null)
                        {
                            ack.More = new PNFetchHistoryResult.MoreInfo();
                            if (moreDic.ContainsKey("start"))
                            {
                                long moreStart;
                                if (moreDic.ContainsKey("start") && Int64.TryParse(moreDic["start"].ToString(), out moreStart))
                                {
                                    ack.More.Start = moreStart;
                                }

                                long moreEnd;
                                if (moreDic.ContainsKey("end") && Int64.TryParse(moreDic["end"].ToString(), out moreEnd))
                                {
                                    ack.More.End = moreEnd;
                                }

                                int moreMax;
                                if (moreDic.ContainsKey("max") && Int32.TryParse(moreDic["max"].ToString(), out moreMax))
                                {
                                    ack.More.Max = moreMax;
                                }
                            }
                        }
                    }
                }
            }

            return ack;
        }
    }
}
