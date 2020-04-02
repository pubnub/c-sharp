using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNGetMessageActionsJsonDataParse
    {
        internal static PNGetMessageActionsResult GetObject(List<object> listObject)
        {
            Dictionary<string, object> getMsgActionsDicObj = (listObject != null && listObject.Count >= 2) ? JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[1]) : null;
            PNGetMessageActionsResult result = null;
            if (getMsgActionsDicObj != null && getMsgActionsDicObj.ContainsKey("data"))
            {
                result = new PNGetMessageActionsResult();

                object[] getMsgActionsDataList = JsonDataParseInternalUtil.ConvertToObjectArray(getMsgActionsDicObj["data"]);
                if (getMsgActionsDataList != null && getMsgActionsDataList.Length > 0)
                {
                    result.MessageActions = new List<PNMessageActionItem>();

                    foreach (object getMsgActionObj in getMsgActionsDataList)
                    {
                        Dictionary<string, object> getMsgActionItemDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(getMsgActionObj);
                        if (getMsgActionItemDic != null && getMsgActionItemDic.Count > 0)
                        {
                            PNMessageActionItem actionItem = new PNMessageActionItem();

                            long messageTimetoken;
                            if (getMsgActionItemDic.ContainsKey("messageTimetoken") && Int64.TryParse(getMsgActionItemDic["messageTimetoken"].ToString(), out messageTimetoken))
                            {
                                actionItem.MessageTimetoken = messageTimetoken;
                            }

                            long actionTimetoken;
                            if (getMsgActionItemDic.ContainsKey("actionTimetoken") && Int64.TryParse(getMsgActionItemDic["actionTimetoken"].ToString(), out actionTimetoken))
                            {
                                actionItem.ActionTimetoken = actionTimetoken;
                            }

                            actionItem.Action = new PNMessageAction
                            {
                                Type = getMsgActionItemDic.ContainsKey("type") && getMsgActionItemDic["type"] != null ? getMsgActionItemDic["type"].ToString() : null,
                                Value = getMsgActionItemDic.ContainsKey("value") && getMsgActionItemDic["value"] != null ? getMsgActionItemDic["value"].ToString() : null
                            };

                            actionItem.Uuid = getMsgActionItemDic.ContainsKey("uuid") && getMsgActionItemDic["uuid"] != null ? getMsgActionItemDic["uuid"].ToString() : null;

                            result.MessageActions.Add(actionItem);
                        }
                    }
                }

                if (getMsgActionsDicObj.ContainsKey("more"))
                {
                    Dictionary<string, object> getMsgActionsMoreDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(getMsgActionsDicObj["more"]);
                    if (getMsgActionsMoreDic != null && getMsgActionsMoreDic.Count > 0)
                    {
                        result.More = new PNGetMessageActionsResult.MoreInfo();
                        long moreStart;
                        if (getMsgActionsMoreDic.ContainsKey("start") && Int64.TryParse(getMsgActionsMoreDic["start"].ToString(), out moreStart))
                        {
                            result.More.Start = moreStart;
                        }

                        long moreEnd;
                        if (getMsgActionsMoreDic.ContainsKey("end") && Int64.TryParse(getMsgActionsMoreDic["end"].ToString(), out moreEnd))
                        {
                            result.More.End = moreEnd;
                        }

                        int moreLimit;
                        if (getMsgActionsMoreDic.ContainsKey("limit") && Int32.TryParse(getMsgActionsMoreDic["limit"].ToString(), out moreLimit))
                        {
                            result.More.Limit = moreLimit;
                        }
                    }
                }
            }

            return result;
        }
    }
}
