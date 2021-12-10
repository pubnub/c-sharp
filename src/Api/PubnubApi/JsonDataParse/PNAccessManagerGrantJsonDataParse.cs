using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNAccessManagerGrantJsonDataParse
    {
        internal static PNAccessManagerGrantResult GetObject(List<object> listObject)
        {
            PNAccessManagerGrantResult ack = null;

            Dictionary<string, object> grantDictObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[0]);

            if (grantDictObj != null)
            {
                ack = new PNAccessManagerGrantResult();

                if (grantDictObj.ContainsKey("payload"))
                {
                    Dictionary<string, object> grantAckPayloadDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantDictObj["payload"]);
                    if (grantAckPayloadDict != null && grantAckPayloadDict.Count > 0)
                    {
                        if (grantAckPayloadDict.ContainsKey("level"))
                        {
                            ack.Level = grantAckPayloadDict["level"].ToString();
                        }

                        if (grantAckPayloadDict.ContainsKey("subscribe_key"))
                        {
                            ack.SubscribeKey = grantAckPayloadDict["subscribe_key"].ToString();
                        }

                        if (grantAckPayloadDict.ContainsKey("ttl"))
                        {
                            int grantTtl;
                            if (Int32.TryParse(grantAckPayloadDict["ttl"].ToString(), out grantTtl))
                            {
                                ack.Ttl = grantTtl;
                            }
                        }

                        if (!string.IsNullOrEmpty(ack.Level) && ack.Level == "subkey")
                        {
                            //Placeholder for subkey level
                        }
                        else
                        {
                            if (grantAckPayloadDict.ContainsKey("channels"))
                            {
                                ack.Channels = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                Dictionary<string, object> grantAckChannelListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDict["channels"]);
                                if (grantAckChannelListDict != null && grantAckChannelListDict.Count > 0)
                                {
                                    foreach (string channel in grantAckChannelListDict.Keys)
                                    {
                                        Dictionary<string, object> grantAckChannelDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelListDict[channel]);
                                        if (grantAckChannelDataDict != null && grantAckChannelDataDict.Count > 0)
                                        {
                                            if (grantAckChannelDataDict.ContainsKey("auths"))
                                            {
                                                Dictionary<string, PNAccessManagerKeyData> authKeyDataDict = new Dictionary<string, PNAccessManagerKeyData>();

                                                Dictionary<string, object> grantAckChannelAuthListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelDataDict["auths"]);
                                                if (grantAckChannelAuthListDict != null && grantAckChannelAuthListDict.Count > 0)
                                                {
                                                    foreach (string authKey in grantAckChannelAuthListDict.Keys)
                                                    {
                                                        Dictionary<string, object> grantAckChannelAuthDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDict[authKey]);

                                                        if (grantAckChannelAuthDataDict != null && grantAckChannelAuthDataDict.Count > 0)
                                                        {
                                                            PNAccessManagerKeyData authData = GetAccessManagerKeyData(grantAckChannelAuthDataDict);
                                                            authKeyDataDict.Add(authKey, authData);
                                                        }

                                                    }

                                                    ack.Channels.Add(channel, authKeyDataDict);
                                                }
                                            }
                                        }
                                    }
                                }
                            }//end of if channels
                            else if (grantAckPayloadDict.ContainsKey("channel"))
                            {
                                ack.Channels = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                string channelName = grantAckPayloadDict["channel"].ToString();
                                if (grantAckPayloadDict.ContainsKey("auths"))
                                {
                                    Dictionary<string, PNAccessManagerKeyData> authKeyDataDict = new Dictionary<string, PNAccessManagerKeyData>();

                                    Dictionary<string, object> grantAckChannelAuthListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDict["auths"]);

                                    if (grantAckChannelAuthListDict != null && grantAckChannelAuthListDict.Count > 0)
                                    {
                                        foreach (string authKey in grantAckChannelAuthListDict.Keys)
                                        {
                                            Dictionary<string, object> grantAckChannelAuthDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDict[authKey]);
                                            if (grantAckChannelAuthDataDict != null && grantAckChannelAuthDataDict.Count > 0)
                                            {
                                                PNAccessManagerKeyData authData = GetAccessManagerKeyData(grantAckChannelAuthDataDict);
                                                authKeyDataDict.Add(authKey, authData);
                                            }

                                        }

                                        ack.Channels.Add(channelName, authKeyDataDict);
                                    }
                                }
                            } //end of if channel

                            if (grantAckPayloadDict.ContainsKey("channel-groups"))
                            {
                                ack.ChannelGroups = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                Dictionary<string, object> grantAckCgListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDict["channel-groups"]);
                                if (grantAckCgListDict != null && grantAckCgListDict.Count > 0)
                                {
                                    foreach (string channelgroup in grantAckCgListDict.Keys)
                                    {
                                        Dictionary<string, object> grantAckCgDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckCgListDict[channelgroup]);
                                        if (grantAckCgDataDict != null && grantAckCgDataDict.Count > 0)
                                        {
                                            if (grantAckCgDataDict.ContainsKey("auths"))
                                            {
                                                Dictionary<string, PNAccessManagerKeyData> authKeyDataDict = new Dictionary<string, PNAccessManagerKeyData>();

                                                Dictionary<string, object> grantAckCgAuthListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckCgDataDict["auths"]);
                                                if (grantAckCgAuthListDict != null && grantAckCgAuthListDict.Count > 0)
                                                {
                                                    foreach (string authKey in grantAckCgAuthListDict.Keys)
                                                    {
                                                        Dictionary<string, object> grantAckCgAuthDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckCgAuthListDict[authKey]);
                                                        if (grantAckCgAuthDataDict != null && grantAckCgAuthDataDict.Count > 0)
                                                        {
                                                            PNAccessManagerKeyData authData = GetAccessManagerKeyData(grantAckCgAuthDataDict);
                                                            authKeyDataDict.Add(authKey, authData);
                                                        }

                                                    }

                                                    ack.ChannelGroups.Add(channelgroup, authKeyDataDict);
                                                }
                                            }
                                        }
                                    }
                                }// if no dictionary due to REST bug
                                else
                                {
                                    string channelGroupName = grantAckPayloadDict["channel-groups"].ToString();
                                    if (grantAckPayloadDict.ContainsKey("auths"))
                                    {
                                        Dictionary<string, PNAccessManagerKeyData> authKeyDataDict = new Dictionary<string, PNAccessManagerKeyData>();

                                        Dictionary<string, object> grantAckChannelAuthListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDict["auths"]);

                                        if (grantAckChannelAuthListDict != null && grantAckChannelAuthListDict.Count > 0)
                                        {
                                            foreach (string authKey in grantAckChannelAuthListDict.Keys)
                                            {
                                                Dictionary<string, object> grantAckChannelAuthDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDict[authKey]);
                                                if (grantAckChannelAuthDataDict != null && grantAckChannelAuthDataDict.Count > 0)
                                                {
                                                    PNAccessManagerKeyData authData = GetAccessManagerKeyData(grantAckChannelAuthDataDict);
                                                    authKeyDataDict.Add(authKey, authData);
                                                }

                                            }

                                            ack.ChannelGroups.Add(channelGroupName, authKeyDataDict);
                                        }
                                    }

                                } //end of else if for REST bug
                            }//end of if channel-groups
                            else if (grantAckPayloadDict.ContainsKey("channel-group"))
                            {
                                ack.ChannelGroups = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                string channelGroupName = grantAckPayloadDict["channel-group"].ToString();
                                if (grantAckPayloadDict.ContainsKey("auths"))
                                {
                                    Dictionary<string, PNAccessManagerKeyData> authKeyDataDict = new Dictionary<string, PNAccessManagerKeyData>();

                                    Dictionary<string, object> grantAckChannelAuthListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDict["auths"]);

                                    if (grantAckChannelAuthListDict != null && grantAckChannelAuthListDict.Count > 0)
                                    {
                                        foreach (string authKey in grantAckChannelAuthListDict.Keys)
                                        {
                                            Dictionary<string, object> grantAckChannelAuthDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDict[authKey]);
                                            if (grantAckChannelAuthDataDict != null && grantAckChannelAuthDataDict.Count > 0)
                                            {
                                                PNAccessManagerKeyData authData = GetAccessManagerKeyData(grantAckChannelAuthDataDict);
                                                authKeyDataDict.Add(authKey, authData);
                                            }

                                        }

                                        ack.ChannelGroups.Add(channelGroupName, authKeyDataDict);
                                    }
                                }
                            } //end of if channel-group

                            if (grantAckPayloadDict.ContainsKey("uuids"))
                            {
                                ack.Uuids = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                Dictionary<string, object> grantAckCgListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDict["uuids"]);
                                if (grantAckCgListDict != null && grantAckCgListDict.Count > 0)
                                {
                                    foreach (string uuid in grantAckCgListDict.Keys)
                                    {
                                        Dictionary<string, object> grantAckUuidDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckCgListDict[uuid]);
                                        if (grantAckUuidDataDict != null && grantAckUuidDataDict.Count > 0)
                                        {
                                            if (grantAckUuidDataDict.ContainsKey("auths"))
                                            {
                                                Dictionary<string, PNAccessManagerKeyData> authKeyDataDict = new Dictionary<string, PNAccessManagerKeyData>();

                                                Dictionary<string, object> grantAckUuidAuthListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckUuidDataDict["auths"]);
                                                if (grantAckUuidAuthListDict != null && grantAckUuidAuthListDict.Count > 0)
                                                {
                                                    foreach (string authKey in grantAckUuidAuthListDict.Keys)
                                                    {
                                                        Dictionary<string, object> grantAckUuidAuthDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckUuidAuthListDict[authKey]);
                                                        if (grantAckUuidAuthDataDict != null && grantAckUuidAuthDataDict.Count > 0)
                                                        {
                                                            PNAccessManagerKeyData authData = GetAccessManagerKeyData(grantAckUuidAuthDataDict);
                                                            authKeyDataDict.Add(authKey, authData);
                                                        }

                                                    }

                                                    ack.Uuids.Add(uuid, authKeyDataDict);
                                                }
                                            }
                                        }
                                    }
                                }// if no dictionary due to REST bug
                                else
                                {
                                    string targetUuid = grantAckPayloadDict["uuids"].ToString();
                                    if (grantAckPayloadDict.ContainsKey("auths"))
                                    {
                                        Dictionary<string, PNAccessManagerKeyData> authKeyDataDict = new Dictionary<string, PNAccessManagerKeyData>();

                                        Dictionary<string, object> grantAckUuidAuthListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDict["auths"]);

                                        if (grantAckUuidAuthListDict != null && grantAckUuidAuthListDict.Count > 0)
                                        {
                                            foreach (string authKey in grantAckUuidAuthListDict.Keys)
                                            {
                                                Dictionary<string, object> grantAckUuidAuthDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckUuidAuthListDict[authKey]);
                                                if (grantAckUuidAuthDataDict != null && grantAckUuidAuthDataDict.Count > 0)
                                                {
                                                    PNAccessManagerKeyData authData = GetAccessManagerKeyData(grantAckUuidAuthDataDict);
                                                    authKeyDataDict.Add(authKey, authData);
                                                }

                                            }

                                            ack.Uuids.Add(targetUuid, authKeyDataDict);
                                        }
                                    }

                                } //end of else if for REST bug
                            }//end of if uuids
                            else if (grantAckPayloadDict.ContainsKey("uuid"))
                            {
                                ack.Uuids = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                string uuid = grantAckPayloadDict["uuid"].ToString();
                                if (grantAckPayloadDict.ContainsKey("auths"))
                                {
                                    Dictionary<string, PNAccessManagerKeyData> authKeyDataDict = new Dictionary<string, PNAccessManagerKeyData>();

                                    Dictionary<string, object> grantAckChannelAuthListDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDict["auths"]);

                                    if (grantAckChannelAuthListDict != null && grantAckChannelAuthListDict.Count > 0)
                                    {
                                        foreach (string authKey in grantAckChannelAuthListDict.Keys)
                                        {
                                            Dictionary<string, object> grantAckChannelAuthDataDict = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDict[authKey]);
                                            if (grantAckChannelAuthDataDict != null && grantAckChannelAuthDataDict.Count > 0)
                                            {
                                                PNAccessManagerKeyData authData = GetAccessManagerKeyData(grantAckChannelAuthDataDict);
                                                authKeyDataDict.Add(authKey, authData);
                                            }

                                        }

                                        ack.Uuids.Add(uuid, authKeyDataDict);
                                    }
                                }
                            } //end of if uuid

                        } //end of else subkey

                    }

                }
            }

            return ack;
        }

        private static PNAccessManagerKeyData GetAccessManagerKeyData(Dictionary<string, object> grantAccessDataDict)
        {
            PNAccessManagerKeyData pamData = new PNAccessManagerKeyData();
            pamData.ReadEnabled = grantAccessDataDict.ContainsKey("r") && grantAccessDataDict["r"].ToString() == "1";
            pamData.WriteEnabled = grantAccessDataDict.ContainsKey("w") && grantAccessDataDict["w"].ToString() == "1";
            pamData.ManageEnabled = grantAccessDataDict.ContainsKey("m") && grantAccessDataDict["m"].ToString() == "1";
            pamData.DeleteEnabled = grantAccessDataDict.ContainsKey("d") && grantAccessDataDict["d"].ToString() == "1";
            pamData.GetEnabled = grantAccessDataDict.ContainsKey("g") && grantAccessDataDict["g"].ToString() == "1";
            pamData.UpdateEnabled = grantAccessDataDict.ContainsKey("u") && grantAccessDataDict["u"].ToString() == "1";
            pamData.JoinEnabled = grantAccessDataDict.ContainsKey("j") && grantAccessDataDict["j"].ToString() == "1";

            return pamData;
        }
    }
}
