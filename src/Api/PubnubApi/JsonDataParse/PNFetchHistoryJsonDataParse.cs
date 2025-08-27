using System;
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNFetchHistoryJsonDataParse
    {
        internal static PNFetchHistoryResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
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
                            object[] channelValArray = channelKVP.Value != null ? jsonPlug.ConvertToObjectArray(channelKVP.Value) : new object[0];
                            foreach (object msgContainerObj in channelValArray)
                            {
                                Dictionary<string, object> messagesContainer = jsonPlug.ConvertToDictionaryObject(msgContainerObj);
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
                                            result.Meta = jsonPlug.ConvertToDictionaryObject(messagesContainer["meta"]);
                                        }

                                        if (messagesContainer.ContainsKey("actions"))
                                        {
                                            var actions = new Dictionary<string, List<PNMessageActionItem>>();
                                            var actionsRaw = messagesContainer["actions"] as Dictionary<string, object>;
                                            foreach (var rawAction in actionsRaw)
                                            {
                                                var actionItems = new List<PNMessageActionItem>();
                                                var actionType = rawAction.Key;
                                                var rawActionValues = rawAction.Value as Dictionary<string, object>;
                                                foreach (var rawActionValue in rawActionValues)
                                                {
                                                    var actionValue = rawActionValue.Key;
                                                    var entries = rawActionValue.Value as List<object>;
                                                    foreach (var entry in entries)
                                                    {
                                                        var entryDict = entry as Dictionary<string, object>;
                                                        actionItems.Add(new PNMessageActionItem()
                                                        {
                                                            ActionTimetoken = long.Parse(entryDict["actionTimetoken"].ToString()),
                                                            Uuid = entryDict["uuid"].ToString(),
                                                            Action = new PNMessageAction()
                                                            {
                                                                Type = actionType,
                                                                Value = actionValue
                                                            },
                                                            MessageTimetoken = result.Timetoken
                                                        });
                                                    }
                                                }
                                                actions[actionType] = actionItems;
                                            }
                                            result.Actions = actions;
                                        }
                                        if (messagesContainer.ContainsKey("uuid") && messagesContainer["uuid"] != null)
                                        {
                                            result.Uuid = messagesContainer["uuid"].ToString();
                                        }
                                        int messageType;
                                        if (messagesContainer.ContainsKey("message_type") && messagesContainer["message_type"] != null && Int32.TryParse(messagesContainer["message_type"].ToString(), out messageType))
                                        {
                                            result.MessageType = messageType;
                                        }

                                        if (messagesContainer.ContainsKey("custom_message_type") && !string.IsNullOrEmpty((string)messagesContainer["custom_message_type"]))
                                        {
                                            result.CustomMessageType = messagesContainer["custom_message_type"].ToString();
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
