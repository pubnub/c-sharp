using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PubnubApi
{
    public static class DeserializeToInternalObjectUtility
    {
        public static T DeserializeToInternalObject<T>(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            T ret = default(T);

            if (listObject == null)
            {
                return ret;
            }
            
            if (typeof(T) == typeof(PNAccessManagerGrantResult))
            {
                #region "PNAccessManagerGrantResult"

                PNAccessManagerGrantResult result = PNAccessManagerGrantJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNAccessManagerGrantResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNAccessManagerTokenResult))
            {
                #region "PNAccessManagerTokenResult"

                PNAccessManagerTokenResult result = PNGrantTokenJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNAccessManagerTokenResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNAccessManagerRevokeTokenResult))
            {
                #region "PNAccessManagerRevokeTokenResult"

                PNAccessManagerRevokeTokenResult result = PNRevokeTokenJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNAccessManagerRevokeTokenResult),
                    CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNAccessManagerAuditResult))
            {
                #region "PNAccessManagerAuditResult"

                Dictionary<string, object> auditDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNAccessManagerAuditResult ack = null;

                if (auditDicObj != null)
                {
                    ack = new PNAccessManagerAuditResult();

                    if (auditDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> auditAckPayloadDic =
                            jsonPlug.ConvertToDictionaryObject(auditDicObj["payload"]);
                        if (auditAckPayloadDic != null && auditAckPayloadDic.Count > 0)
                        {
                            if (auditAckPayloadDic.ContainsKey("level"))
                            {
                                ack.Level = auditAckPayloadDic["level"].ToString();
                            }

                            if (auditAckPayloadDic.ContainsKey("subscribe_key"))
                            {
                                ack.SubscribeKey = auditAckPayloadDic["subscribe_key"].ToString();
                            }

                            if (auditAckPayloadDic.ContainsKey("channel"))
                            {
                                ack.Channel = auditAckPayloadDic["channel"].ToString();
                            }

                            if (auditAckPayloadDic.ContainsKey("channel-group"))
                            {
                                ack.ChannelGroup = auditAckPayloadDic["channel-group"].ToString();
                            }

                            if (auditAckPayloadDic.ContainsKey("auths"))
                            {
                                Dictionary<string, object> auditAckAuthListDic =
                                    jsonPlug.ConvertToDictionaryObject(auditAckPayloadDic["auths"]);
                                if (auditAckAuthListDic != null && auditAckAuthListDic.Count > 0)
                                {
                                    ack.AuthKeys = new Dictionary<string, PNAccessManagerKeyData>();

                                    foreach (string authKey in auditAckAuthListDic.Keys)
                                    {
                                        Dictionary<string, object> authDataDic =
                                            jsonPlug.ConvertToDictionaryObject(auditAckAuthListDic[authKey]);
                                        if (authDataDic != null && authDataDic.Count > 0)
                                        {
                                            PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                            authData.ReadEnabled = authDataDic["r"].ToString() == "1";
                                            authData.WriteEnabled = authDataDic["w"].ToString() == "1";
                                            authData.ManageEnabled = authDataDic.ContainsKey("m")
                                                ? authDataDic["m"].ToString() == "1"
                                                : false;
                                            authData.DeleteEnabled = authDataDic.ContainsKey("d")
                                                ? authDataDic["d"].ToString() == "1"
                                                : false;

                                            ack.AuthKeys.Add(authKey, authData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNAccessManagerAuditResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNPublishResult))
            {
                #region "PNPublishResult"

                PNPublishResult result = null;
                if (listObject.Count >= 2)
                {
                    long publishTimetoken;
                    var _ = Int64.TryParse(listObject[2].ToString(), out publishTimetoken);
                    result = new PNPublishResult
                    {
                        Timetoken = publishTimetoken
                    };
                }

                ret = (T)Convert.ChangeType(result, typeof(PNPublishResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNPresenceEventResult))
            {
                #region "PNPresenceEventResult"

                Dictionary<string, object> presenceDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNPresenceEventResult ack = null;

                if (presenceDicObj != null)
                {
                    ack = new PNPresenceEventResult();
                    ack.Event = presenceDicObj["action"].ToString();
                    long presenceTimeStamp;
                    if (Int64.TryParse(presenceDicObj["timestamp"].ToString(), out presenceTimeStamp))
                    {
                        ack.Timestamp = presenceTimeStamp;
                    }

                    if (presenceDicObj.ContainsKey("uuid"))
                    {
                        ack.Uuid = presenceDicObj["uuid"].ToString();
                    }

                    int presenceOccupany;
                    if (Int32.TryParse(presenceDicObj["occupancy"].ToString(), out presenceOccupany))
                    {
                        ack.Occupancy = presenceOccupany;
                    }

                    if (presenceDicObj.ContainsKey("data"))
                    {
                        Dictionary<string, object> stateDic = presenceDicObj["data"] as Dictionary<string, object>;
                        if (stateDic != null)
                        {
                            ack.State = stateDic;
                        }
                    }

                    long presenceTimetoken;
                    if (Int64.TryParse(listObject[2].ToString(), out presenceTimetoken))
                    {
                        ack.Timetoken = presenceTimetoken;
                    }

                    ack.Channel = (listObject.Count == 6) ? listObject[5].ToString() : listObject[4].ToString();
                    ack.Channel = ack.Channel.Replace("-pnpres", "");

                    if (listObject.Count == 6)
                    {
                        ack.Subscription = listObject[5]?.ToString();
                        ack.Subscription = ack.Subscription?.Replace("-pnpres", "");
                    }

                    if (listObject[1] != null)
                    {
                        ack.UserMetadata = jsonPlug.ConvertToDictionaryObject(listObject[1]);
                    }

                    if (ack.Event != null && ack.Event.ToLowerInvariant() == "interval")
                    {
                        if (presenceDicObj.ContainsKey("join"))
                        {
                            List<object> joinDeltaList = presenceDicObj["join"] as List<object>;
                            if (joinDeltaList != null && joinDeltaList.Count > 0)
                            {
                                ack.Join = joinDeltaList.Select(x => x.ToString()).ToArray();
                            }
                        }

                        if (presenceDicObj.ContainsKey("timeout"))
                        {
                            List<object> timeoutDeltaList = presenceDicObj["timeout"] as List<object>;
                            if (timeoutDeltaList != null && timeoutDeltaList.Count > 0)
                            {
                                ack.Timeout = timeoutDeltaList.Select(x => x.ToString()).ToArray();
                            }
                        }

                        if (presenceDicObj.ContainsKey("leave"))
                        {
                            List<object> leaveDeltaList = presenceDicObj["leave"] as List<object>;
                            if (leaveDeltaList != null && leaveDeltaList.Count > 0)
                            {
                                ack.Leave = leaveDeltaList.Select(x => x.ToString()).ToArray();
                            }
                        }

                        if (presenceDicObj.ContainsKey("here_now_refresh"))
                        {
                            string hereNowRefreshStr = presenceDicObj["here_now_refresh"].ToString();
                            if (!string.IsNullOrEmpty(hereNowRefreshStr))
                            {
                                bool boolHereNowRefresh = false;
                                if (Boolean.TryParse(hereNowRefreshStr, out boolHereNowRefresh))
                                {
                                    ack.HereNowRefresh = boolHereNowRefresh;
                                }
                            }
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNPresenceEventResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNHistoryResult))
            {
                #region "PNHistoryResult"

                PNHistoryResult result = PNHistoryJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNHistoryResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNFetchHistoryResult))
            {
                #region "PNFetchHistoryResult"

                PNFetchHistoryResult result = PNFetchHistoryJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNFetchHistoryResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNDeleteMessageResult))
            {
                #region "PNDeleteMessageResult"

                PNDeleteMessageResult ack = new PNDeleteMessageResult();
                ret = (T)Convert.ChangeType(ack, typeof(PNDeleteMessageResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNMessageCountResult))
            {
                #region "PNMessageCountResult"

                PNMessageCountResult ack = null;
                Dictionary<string, object> messageCouuntContainerDicObj =
                    jsonPlug.ConvertToDictionaryObject(listObject[0]);
                if (messageCouuntContainerDicObj != null && messageCouuntContainerDicObj.ContainsKey("channels"))
                {
                    ack = new PNMessageCountResult();
                    Dictionary<string, object> messageCountDic =
                        jsonPlug.ConvertToDictionaryObject(messageCouuntContainerDicObj["channels"]);
                    if (messageCountDic != null)
                    {
                        ack.Channels = new Dictionary<string, long>();
                        foreach (string channel in messageCountDic.Keys)
                        {
                            long msgCount = 0;
                            if (Int64.TryParse(messageCountDic[channel].ToString(), out msgCount))
                            {
                                ack.Channels.Add(channel, msgCount);
                            }
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNMessageCountResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNHereNowResult))
            {
                #region "PNHereNowResult"

                Dictionary<string, object> herenowDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNHereNowResult hereNowResult = null;

                if (herenowDicObj != null)
                {
                    hereNowResult = new PNHereNowResult();

                    string hereNowChannelName = listObject[1].ToString();

                    if (herenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> hereNowPayloadDic =
                            jsonPlug.ConvertToDictionaryObject(herenowDicObj["payload"]);
                        if (hereNowPayloadDic != null && hereNowPayloadDic.Count > 0)
                        {
                            int hereNowTotalOccupancy;
                            int hereNowTotalChannel;
                            if (Int32.TryParse(hereNowPayloadDic["total_occupancy"].ToString(),
                                    out hereNowTotalOccupancy))
                            {
                                hereNowResult.TotalOccupancy = hereNowTotalOccupancy;
                            }

                            if (Int32.TryParse(hereNowPayloadDic["total_channels"].ToString(), out hereNowTotalChannel))
                            {
                                hereNowResult.TotalChannels = hereNowTotalChannel;
                            }

                            if (hereNowPayloadDic.ContainsKey("channels"))
                            {
                                Dictionary<string, object> hereNowChannelListDic =
                                    jsonPlug.ConvertToDictionaryObject(hereNowPayloadDic["channels"]);
                                if (hereNowChannelListDic != null && hereNowChannelListDic.Count > 0)
                                {
                                    foreach (string channel in hereNowChannelListDic.Keys)
                                    {
                                        Dictionary<string, object> hereNowChannelItemDic =
                                            jsonPlug.ConvertToDictionaryObject(hereNowChannelListDic[channel]);
                                        if (hereNowChannelItemDic != null && hereNowChannelItemDic.Count > 0)
                                        {
                                            PNHereNowChannelData channelData = new PNHereNowChannelData();
                                            channelData.ChannelName = channel;
                                            int hereNowOccupancy;
                                            if (Int32.TryParse(hereNowChannelItemDic["occupancy"].ToString(),
                                                    out hereNowOccupancy))
                                            {
                                                channelData.Occupancy = hereNowOccupancy;
                                            }

                                            if (hereNowChannelItemDic.ContainsKey("uuids"))
                                            {
                                                object[] hereNowChannelUuidList =
                                                    jsonPlug.ConvertToObjectArray(hereNowChannelItemDic["uuids"]);
                                                if (hereNowChannelUuidList != null && hereNowChannelUuidList.Length > 0)
                                                {
                                                    List<PNHereNowOccupantData> uuidDataList =
                                                        new List<PNHereNowOccupantData>();

                                                    for (int index = 0; index < hereNowChannelUuidList.Length; index++)
                                                    {
                                                        if (hereNowChannelUuidList[index].GetType() == typeof(string))
                                                        {
                                                            PNHereNowOccupantData uuidData =
                                                                new PNHereNowOccupantData();
                                                            uuidData.Uuid = hereNowChannelUuidList[index].ToString();
                                                            uuidDataList.Add(uuidData);
                                                        }
                                                        else
                                                        {
                                                            Dictionary<string, object> hereNowChannelItemUuidsDic =
                                                                jsonPlug.ConvertToDictionaryObject(
                                                                    hereNowChannelUuidList[index]);
                                                            if (hereNowChannelItemUuidsDic != null &&
                                                                hereNowChannelItemUuidsDic.Count > 0)
                                                            {
                                                                PNHereNowOccupantData uuidData =
                                                                    new PNHereNowOccupantData();
                                                                uuidData.Uuid = hereNowChannelItemUuidsDic["uuid"]
                                                                    .ToString();
                                                                if (hereNowChannelItemUuidsDic.ContainsKey("state"))
                                                                {
                                                                    uuidData.State =
                                                                        jsonPlug.ConvertToDictionaryObject(
                                                                            hereNowChannelItemUuidsDic["state"]);
                                                                }

                                                                uuidDataList.Add(uuidData);
                                                            }
                                                        }
                                                    }

                                                    channelData.Occupants = uuidDataList;
                                                }
                                            }

                                            hereNowResult.Channels.Add(channel, channelData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (herenowDicObj.ContainsKey("occupancy"))
                    {
                        int hereNowTotalOccupancy;
                        if (Int32.TryParse(herenowDicObj["occupancy"].ToString(), out hereNowTotalOccupancy))
                        {
                            hereNowResult.TotalOccupancy = hereNowTotalOccupancy;
                        }

                        hereNowResult.Channels = new Dictionary<string, PNHereNowChannelData>();
                        if (herenowDicObj.ContainsKey("uuids"))
                        {
                            object[] uuidArray = jsonPlug.ConvertToObjectArray(herenowDicObj["uuids"]);
                            if (uuidArray != null && uuidArray.Length > 0)
                            {
                                List<PNHereNowOccupantData> uuidDataList = new List<PNHereNowOccupantData>();
                                for (int index = 0; index < uuidArray.Length; index++)
                                {
                                    Dictionary<string, object> hereNowChannelItemUuidsDic =
                                        jsonPlug.ConvertToDictionaryObject(uuidArray[index]);
                                    if (hereNowChannelItemUuidsDic != null && hereNowChannelItemUuidsDic.Count > 0)
                                    {
                                        PNHereNowOccupantData uuidData = new PNHereNowOccupantData();
                                        uuidData.Uuid = hereNowChannelItemUuidsDic["uuid"].ToString();
                                        if (hereNowChannelItemUuidsDic.ContainsKey("state"))
                                        {
                                            uuidData.State =
                                                jsonPlug.ConvertToDictionaryObject(hereNowChannelItemUuidsDic["state"]);
                                        }

                                        uuidDataList.Add(uuidData);
                                    }
                                    else
                                    {
                                        PNHereNowOccupantData uuidData = new PNHereNowOccupantData();
                                        uuidData.Uuid = uuidArray[index].ToString();
                                        uuidDataList.Add(uuidData);
                                    }
                                }

                                PNHereNowChannelData channelData = new PNHereNowChannelData();
                                channelData.ChannelName = hereNowChannelName;
                                channelData.Occupants = uuidDataList;
                                channelData.Occupancy = hereNowResult.TotalOccupancy;

                                hereNowResult.Channels.Add(hereNowChannelName, channelData);
                                hereNowResult.TotalChannels = hereNowResult.Channels.Count;
                            }
                        }
                        else
                        {
                            string channels = listObject[1].ToString();
                            string[] arrChannel = channels.Split(',');
                            int totalChannels = 0;
                            foreach (string channel in arrChannel)
                            {
                                PNHereNowChannelData channelData = new PNHereNowChannelData();
                                channelData.Occupancy = 1;
                                hereNowResult.Channels.Add(channel, channelData);
                                totalChannels++;
                            }

                            hereNowResult.TotalChannels = totalChannels;
                        }
                    }
                }

                ret = (T)Convert.ChangeType(hereNowResult, typeof(PNHereNowResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNWhereNowResult))
            {
                #region "WhereNowAck"

                Dictionary<string, object> wherenowDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNWhereNowResult ack = null;

                if (wherenowDicObj != null)
                {
                    ack = new PNWhereNowResult();

                    if (wherenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> whereNowPayloadDic =
                            jsonPlug.ConvertToDictionaryObject(wherenowDicObj["payload"]);
                        if (whereNowPayloadDic != null && whereNowPayloadDic.Count > 0)
                        {
                            if (whereNowPayloadDic.ContainsKey("channels"))
                            {
                                object[] whereNowChannelList =
                                    jsonPlug.ConvertToObjectArray(whereNowPayloadDic["channels"]);
                                if (whereNowChannelList != null && whereNowChannelList.Length >= 0)
                                {
                                    List<string> channelList = new List<string>();
                                    foreach (string channel in whereNowChannelList)
                                    {
                                        channelList.Add(channel);
                                    }

                                    ack.Channels = channelList;
                                }
                            }
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNWhereNowResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNSetStateResult))
            {
                #region "SetUserStateAck"

                Dictionary<string, object> setUserStatewDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNSetStateResult ack = null;

                if (setUserStatewDicObj != null)
                {
                    ack = new PNSetStateResult();

                    ack.State = new Dictionary<string, object>();

                    if (setUserStatewDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> setStateDic =
                            jsonPlug.ConvertToDictionaryObject(setUserStatewDicObj["payload"]);
                        if (setStateDic != null)
                        {
                            ack.State = setStateDic;
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNSetStateResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNGetStateResult))
            {
                #region "PNGetStateResult"

                Dictionary<string, object> getUserStatewDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNGetStateResult ack = null;

                if (getUserStatewDicObj != null)
                {
                    ack = new PNGetStateResult();

                    ack.StateByUUID = new Dictionary<string, object>();

                    if (getUserStatewDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> getStateDic =
                            jsonPlug.ConvertToDictionaryObject(getUserStatewDicObj["payload"]);
                        if (getStateDic != null)
                        {
                            ack.StateByUUID = getStateDic;
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNGetStateResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNChannelGroupsAllChannelsResult))
            {
                #region "PNChannelGroupsAllChannelsResult"

                Dictionary<string, object> getCgChannelsDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsAllChannelsResult ack = null;

                if (getCgChannelsDicObj != null)
                {
                    ack = new PNChannelGroupsAllChannelsResult();
                    Dictionary<string, object> getCgChannelPayloadDic =
                        jsonPlug.ConvertToDictionaryObject(getCgChannelsDicObj["payload"]);
                    if (getCgChannelPayloadDic != null && getCgChannelPayloadDic.Count > 0)
                    {
                        ack.ChannelGroup = getCgChannelPayloadDic["group"].ToString();
                        object[] channelGroupChPayloadChannels =
                            jsonPlug.ConvertToObjectArray(getCgChannelPayloadDic["channels"]);
                        if (channelGroupChPayloadChannels != null && channelGroupChPayloadChannels.Length > 0)
                        {
                            List<string> channelList = new List<string>();
                            for (int index = 0; index < channelGroupChPayloadChannels.Length; index++)
                            {
                                channelList.Add(channelGroupChPayloadChannels[index].ToString());
                            }

                            ack.Channels = channelList;
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNChannelGroupsAllChannelsResult),
                    CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNChannelGroupsListAllResult))
            {
                #region "PNChannelGroupsListAllResult"

                Dictionary<string, object> getAllCgDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsListAllResult ack = null;

                if (getAllCgDicObj != null)
                {
                    ack = new PNChannelGroupsListAllResult();

                    Dictionary<string, object> getAllCgPayloadDic =
                        jsonPlug.ConvertToDictionaryObject(getAllCgDicObj["payload"]);
                    if (getAllCgPayloadDic != null && getAllCgPayloadDic.Count > 0)
                    {
                        object[] channelGroupAllCgPayloadChannels =
                            jsonPlug.ConvertToObjectArray(getAllCgPayloadDic["groups"]);
                        if (channelGroupAllCgPayloadChannels != null && channelGroupAllCgPayloadChannels.Length > 0)
                        {
                            List<string> allCgList = new List<string>();
                            for (int index = 0; index < channelGroupAllCgPayloadChannels.Length; index++)
                            {
                                allCgList.Add(channelGroupAllCgPayloadChannels[index].ToString());
                            }

                            ack.Groups = allCgList;
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNChannelGroupsListAllResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNChannelGroupsAddChannelResult))
            {
                #region "AddChannelToChannelGroupAck"

                Dictionary<string, object> addChToCgDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsAddChannelResult ack = null;

                if (addChToCgDicObj != null)
                {
                    ack = new PNChannelGroupsAddChannelResult();
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNChannelGroupsAddChannelResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNChannelGroupsRemoveChannelResult))
            {
                #region "PNChannelGroupsRemoveChannelResult"

                Dictionary<string, object> removeChFromCgDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsRemoveChannelResult ack = null;

                int statusCode = 0;

                if (removeChFromCgDicObj != null)
                {
                    ack = new PNChannelGroupsRemoveChannelResult();

                    if (int.TryParse(removeChFromCgDicObj["status"].ToString(), out statusCode))
                    {
                        ack.Status = statusCode;
                    }

                    ack.Message = removeChFromCgDicObj["message"].ToString();
                    ack.Service = removeChFromCgDicObj["service"].ToString();

                    ack.Error = Convert.ToBoolean(removeChFromCgDicObj["error"].ToString(),
                        CultureInfo.InvariantCulture);

                    ack.ChannelGroup = listObject[1].ToString();
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNChannelGroupsRemoveChannelResult),
                    CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNChannelGroupsDeleteGroupResult))
            {
                #region "PNChannelGroupsDeleteGroupResult"

                Dictionary<string, object> removeCgDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsDeleteGroupResult ack = null;

                int statusCode = 0;

                if (removeCgDicObj != null)
                {
                    ack = new PNChannelGroupsDeleteGroupResult();

                    if (int.TryParse(removeCgDicObj["status"].ToString(), out statusCode))
                    {
                        ack.Status = statusCode;
                    }

                    ack.Service = removeCgDicObj["service"].ToString();
                    ack.Message = removeCgDicObj["message"].ToString();

                    ack.Error = Convert.ToBoolean(removeCgDicObj["error"].ToString(), CultureInfo.InvariantCulture);
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNChannelGroupsDeleteGroupResult),
                    CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNTimeResult))
            {
                #region "PNTimeResult"

                Int64 timetoken = 0;

                var _ = Int64.TryParse(listObject[0].ToString(), out timetoken);

                PNTimeResult result = new PNTimeResult
                {
                    Timetoken = timetoken
                };

                ret = (T)Convert.ChangeType(result, typeof(PNTimeResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNPushAddChannelResult))
            {
                #region "PNPushAddChannelResult"

                PNPushAddChannelResult result = new PNPushAddChannelResult();

                ret = (T)Convert.ChangeType(result, typeof(PNPushAddChannelResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNPushListProvisionsResult))
            {
                #region "PNPushListProvisionsResult"

                PNPushListProvisionsResult result = new PNPushListProvisionsResult();
                result.Channels = listObject.OfType<string>().Where(s => s.Trim() != "").ToList();

                ret = (T)Convert.ChangeType(result, typeof(PNPushListProvisionsResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNPushRemoveChannelResult))
            {
                #region "PNPushRemoveChannelResult"

                PNPushRemoveChannelResult result = new PNPushRemoveChannelResult();

                ret = (T)Convert.ChangeType(result, typeof(PNPushRemoveChannelResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNPushRemoveAllChannelsResult))
            {
                #region "PNPushRemoveAllChannelsResult"

                PNPushRemoveAllChannelsResult result = new PNPushRemoveAllChannelsResult();

                ret = (T)Convert.ChangeType(result, typeof(PNPushRemoveAllChannelsResult),
                    CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNHeartbeatResult))
            {
                #region "PNHeartbeatResult"

                Dictionary<string, object> heartbeatDicObj = jsonPlug.ConvertToDictionaryObject(listObject[0]);
                PNHeartbeatResult result = null;

                if (heartbeatDicObj != null && heartbeatDicObj.ContainsKey("status"))
                {
                    result = new PNHeartbeatResult();

                    int statusCode;
                    if (int.TryParse(heartbeatDicObj["status"].ToString(), out statusCode))
                    {
                        result.Status = statusCode;
                    }

                    if (heartbeatDicObj.ContainsKey("message"))
                    {
                        result.Message = heartbeatDicObj["message"].ToString();
                    }
                }

                ret = (T)Convert.ChangeType(result, typeof(PNHeartbeatResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNSetUuidMetadataResult))
            {
                #region "PNSetUuidMetadataResult"

                PNSetUuidMetadataResult result = PNSetUuidMetadataJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNSetUuidMetadataResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNRemoveUuidMetadataResult))
            {
                #region "PNDeleteUuidMetadataResult"

                PNRemoveUuidMetadataResult ack = new PNRemoveUuidMetadataResult();
                ret = (T)Convert.ChangeType(ack, typeof(PNRemoveUuidMetadataResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNGetAllUuidMetadataResult))
            {
                #region "PNGetAllUuidMetadataResult"

                PNGetAllUuidMetadataResult result = PNGetAllUuidMetadataJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNGetAllUuidMetadataResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNGetUuidMetadataResult))
            {
                #region "PNGetUuidMetadataResult"

                PNGetUuidMetadataResult result = PNGetUuidMetadataJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNGetUuidMetadataResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNSetChannelMetadataResult))
            {
                #region "PNSetChannelMetadataResult"

                PNSetChannelMetadataResult result = PNSetChannelMetadataJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNSetChannelMetadataResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNRemoveChannelMetadataResult))
            {
                #region "PNDeleteUserResult"

                PNRemoveChannelMetadataResult ack = new PNRemoveChannelMetadataResult();
                ret = (T)Convert.ChangeType(ack, typeof(PNRemoveChannelMetadataResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNGetAllChannelMetadataResult))
            {
                #region "PNGetSpacesResult"

                PNGetAllChannelMetadataResult result = PNGetAllChannelMetadataJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNGetAllChannelMetadataResult),
                    CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNGetChannelMetadataResult))
            {
                #region "PNGetSpaceResult"

                PNGetChannelMetadataResult result = PNGetChannelMetadataJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNGetChannelMetadataResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNMembershipsResult))
            {
                #region "PNMembershipsResult"

                PNMembershipsResult result = PNMembershipsJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNMembershipsResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNChannelMembersResult))
            {
                #region "PNChannelMembersResult"

                PNChannelMembersResult result = PNChannelMembersJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNChannelMembersResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNAddMessageActionResult))
            {
                #region "PNAddMessageActionResult"

                PNAddMessageActionResult result = PNAddMessageActionJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNAddMessageActionResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNRemoveMessageActionResult))
            {
                #region "PNRemoveMessageActionResult"

                PNRemoveMessageActionResult result = PNRemoveMessageActionJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNRemoveMessageActionResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNGetMessageActionsResult))
            {
                #region "PNGetMessageActionsResult"

                PNGetMessageActionsResult result = PNGetMessageActionsJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNGetMessageActionsResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNGenerateFileUploadUrlResult))
            {
                #region "PNGenerateFileUploadUrlResult"

                PNGenerateFileUploadUrlResult result = PNGenerateFileUploadUrlDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNGenerateFileUploadUrlResult),
                    CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNPublishFileMessageResult))
            {
                #region "PNPublishFileMessageResult"

                PNPublishFileMessageResult result = PNPublishFileMessageJsonDataParse.GetObject(listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNPublishFileMessageResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNListFilesResult))
            {
                #region "PNListFilesResult"

                PNListFilesResult result = PNListFilesJsonDataParse.GetObject(jsonPlug, listObject);
                ret = (T)Convert.ChangeType(result, typeof(PNListFilesResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else if (typeof(T) == typeof(PNDeleteFileResult))
            {
                #region "PNDeleteFileResult"

                PNDeleteFileResult ack = new PNDeleteFileResult();
                ret = (T)Convert.ChangeType(ack, typeof(PNDeleteFileResult), CultureInfo.InvariantCulture);

                #endregion
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DeserializeToObject<T>(list) => NO MATCH");
                try
                {
                    ret = (T)(object)listObject;
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"DeserializeToObject<T>(list) exception {e.Message}");
                }
            }
            return ret;
        }
    }
}