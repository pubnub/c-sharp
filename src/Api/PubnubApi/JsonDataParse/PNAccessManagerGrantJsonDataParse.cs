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

            Dictionary<string, object> grantDicObj = JsonDataParseInternalUtil.ConvertToDictionaryObject(listObject[0]);

            if (grantDicObj != null)
            {
                ack = new PNAccessManagerGrantResult();

                if (grantDicObj.ContainsKey("payload"))
                {
                    Dictionary<string, object> grantAckPayloadDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantDicObj["payload"]);
                    if (grantAckPayloadDic != null && grantAckPayloadDic.Count > 0)
                    {
                        if (grantAckPayloadDic.ContainsKey("level"))
                        {
                            ack.Level = grantAckPayloadDic["level"].ToString();
                        }

                        if (grantAckPayloadDic.ContainsKey("subscribe_key"))
                        {
                            ack.SubscribeKey = grantAckPayloadDic["subscribe_key"].ToString();
                        }

                        if (grantAckPayloadDic.ContainsKey("ttl"))
                        {
                            int grantTtl;
                            if (Int32.TryParse(grantAckPayloadDic["ttl"].ToString(), out grantTtl))
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
                            if (grantAckPayloadDic.ContainsKey("channels"))
                            {
                                ack.Channels = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                Dictionary<string, object> grantAckChannelListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDic["channels"]);
                                if (grantAckChannelListDic != null && grantAckChannelListDic.Count > 0)
                                {
                                    foreach (string channel in grantAckChannelListDic.Keys)
                                    {
                                        Dictionary<string, object> grantAckChannelDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelListDic[channel]);
                                        if (grantAckChannelDataDic != null && grantAckChannelDataDic.Count > 0)
                                        {
                                            if (grantAckChannelDataDic.ContainsKey("auths"))
                                            {
                                                Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                                Dictionary<string, object> grantAckChannelAuthListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelDataDic["auths"]);
                                                if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                                {
                                                    foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                                    {
                                                        Dictionary<string, object> grantAckChannelAuthDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);

                                                        if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                        {
                                                            PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                            authData.ReadEnabled =   grantAckChannelAuthDataDic.ContainsKey("r") && grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                            authData.WriteEnabled =  grantAckChannelAuthDataDic.ContainsKey("w") && grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                            authData.ManageEnabled = grantAckChannelAuthDataDic.ContainsKey("m") && grantAckChannelAuthDataDic["m"].ToString() == "1";
                                                            authData.DeleteEnabled = grantAckChannelAuthDataDic.ContainsKey("d") && grantAckChannelAuthDataDic["d"].ToString() == "1";
                                                            authData.GetEnabled =    grantAckChannelAuthDataDic.ContainsKey("g") && grantAckChannelAuthDataDic["g"].ToString() == "1";
                                                            authData.UpdateEnabled = grantAckChannelAuthDataDic.ContainsKey("u") && grantAckChannelAuthDataDic["u"].ToString() == "1";
                                                            authData.JoinEnabled =   grantAckChannelAuthDataDic.ContainsKey("j") && grantAckChannelAuthDataDic["j"].ToString() == "1";

                                                            authKeyDataDic.Add(authKey, authData);
                                                        }

                                                    }

                                                    ack.Channels.Add(channel, authKeyDataDic);
                                                }
                                            }
                                        }
                                    }
                                }
                            }//end of if channels
                            else if (grantAckPayloadDic.ContainsKey("channel"))
                            {
                                ack.Channels = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                string channelName = grantAckPayloadDic["channel"].ToString();
                                if (grantAckPayloadDic.ContainsKey("auths"))
                                {
                                    Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                    Dictionary<string, object> grantAckChannelAuthListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDic["auths"]);

                                    if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                    {
                                        foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                        {
                                            Dictionary<string, object> grantAckChannelAuthDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                            if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                            {
                                                PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                authData.ReadEnabled = grantAckChannelAuthDataDic.ContainsKey("r") && grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                authData.WriteEnabled = grantAckChannelAuthDataDic.ContainsKey("w") && grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                authData.ManageEnabled = grantAckChannelAuthDataDic.ContainsKey("m") && grantAckChannelAuthDataDic["m"].ToString() == "1";
                                                authData.DeleteEnabled = grantAckChannelAuthDataDic.ContainsKey("d") && grantAckChannelAuthDataDic["d"].ToString() == "1";
                                                authData.GetEnabled = grantAckChannelAuthDataDic.ContainsKey("g") && grantAckChannelAuthDataDic["g"].ToString() == "1";
                                                authData.UpdateEnabled = grantAckChannelAuthDataDic.ContainsKey("u") && grantAckChannelAuthDataDic["u"].ToString() == "1";
                                                authData.JoinEnabled = grantAckChannelAuthDataDic.ContainsKey("j") && grantAckChannelAuthDataDic["j"].ToString() == "1";

                                                authKeyDataDic.Add(authKey, authData);
                                            }

                                        }

                                        ack.Channels.Add(channelName, authKeyDataDic);
                                    }
                                }
                            } //end of if channel

                            if (grantAckPayloadDic.ContainsKey("channel-groups"))
                            {
                                ack.ChannelGroups = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                Dictionary<string, object> grantAckCgListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDic["channel-groups"]);
                                if (grantAckCgListDic != null && grantAckCgListDic.Count > 0)
                                {
                                    foreach (string channelgroup in grantAckCgListDic.Keys)
                                    {
                                        Dictionary<string, object> grantAckCgDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckCgListDic[channelgroup]);
                                        if (grantAckCgDataDic != null && grantAckCgDataDic.Count > 0)
                                        {
                                            if (grantAckCgDataDic.ContainsKey("auths"))
                                            {
                                                Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                                Dictionary<string, object> grantAckCgAuthListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckCgDataDic["auths"]);
                                                if (grantAckCgAuthListDic != null && grantAckCgAuthListDic.Count > 0)
                                                {
                                                    foreach (string authKey in grantAckCgAuthListDic.Keys)
                                                    {
                                                        Dictionary<string, object> grantAckCgAuthDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckCgAuthListDic[authKey]);
                                                        if (grantAckCgAuthDataDic != null && grantAckCgAuthDataDic.Count > 0)
                                                        {
                                                            PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                            authData.ReadEnabled =   grantAckCgAuthDataDic.ContainsKey("r") && grantAckCgAuthDataDic["r"].ToString() == "1";
                                                            authData.WriteEnabled =  grantAckCgAuthDataDic.ContainsKey("w") && grantAckCgAuthDataDic["w"].ToString() == "1";
                                                            authData.ManageEnabled = grantAckCgAuthDataDic.ContainsKey("m") && grantAckCgAuthDataDic["m"].ToString() == "1";
                                                            authData.DeleteEnabled = grantAckCgAuthDataDic.ContainsKey("d") && grantAckCgAuthDataDic["d"].ToString() == "1";
                                                            authData.GetEnabled =    grantAckCgAuthDataDic.ContainsKey("g") && grantAckCgAuthDataDic["g"].ToString() == "1";
                                                            authData.UpdateEnabled = grantAckCgAuthDataDic.ContainsKey("u") && grantAckCgAuthDataDic["u"].ToString() == "1";
                                                            authData.JoinEnabled =   grantAckCgAuthDataDic.ContainsKey("j") && grantAckCgAuthDataDic["j"].ToString() == "1";

                                                            authKeyDataDic.Add(authKey, authData);
                                                        }

                                                    }

                                                    ack.ChannelGroups.Add(channelgroup, authKeyDataDic);
                                                }
                                            }
                                        }
                                    }
                                }// if no dictionary due to REST bug
                                else
                                {
                                    string channelGroupName = grantAckPayloadDic["channel-groups"].ToString();
                                    if (grantAckPayloadDic.ContainsKey("auths"))
                                    {
                                        Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                        Dictionary<string, object> grantAckChannelAuthListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDic["auths"]);

                                        if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                        {
                                            foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                            {
                                                Dictionary<string, object> grantAckChannelAuthDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                                if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                {
                                                    PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                    authData.ReadEnabled =   grantAckChannelAuthDataDic.ContainsKey("r") && grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                    authData.WriteEnabled =  grantAckChannelAuthDataDic.ContainsKey("w") && grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                    authData.ManageEnabled = grantAckChannelAuthDataDic.ContainsKey("m") && grantAckChannelAuthDataDic["m"].ToString() == "1";
                                                    authData.DeleteEnabled = grantAckChannelAuthDataDic.ContainsKey("d") && grantAckChannelAuthDataDic["d"].ToString() == "1";
                                                    authData.GetEnabled =    grantAckChannelAuthDataDic.ContainsKey("g") && grantAckChannelAuthDataDic["g"].ToString() == "1";
                                                    authData.UpdateEnabled = grantAckChannelAuthDataDic.ContainsKey("u") && grantAckChannelAuthDataDic["u"].ToString() == "1";
                                                    authData.JoinEnabled =   grantAckChannelAuthDataDic.ContainsKey("j") && grantAckChannelAuthDataDic["j"].ToString() == "1";

                                                    authKeyDataDic.Add(authKey, authData);
                                                }

                                            }

                                            ack.ChannelGroups.Add(channelGroupName, authKeyDataDic);
                                        }
                                    }

                                } //end of else if for REST bug
                            }//end of if channel-groups
                            else if (grantAckPayloadDic.ContainsKey("channel-group"))
                            {
                                ack.ChannelGroups = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                string channelGroupName = grantAckPayloadDic["channel-group"].ToString();
                                if (grantAckPayloadDic.ContainsKey("auths"))
                                {
                                    Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                    Dictionary<string, object> grantAckChannelAuthListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDic["auths"]);

                                    if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                    {
                                        foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                        {
                                            Dictionary<string, object> grantAckChannelAuthDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                            if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                            {
                                                PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                authData.ReadEnabled =   grantAckChannelAuthDataDic.ContainsKey("r") && grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                authData.WriteEnabled =  grantAckChannelAuthDataDic.ContainsKey("w") && grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                authData.ManageEnabled = grantAckChannelAuthDataDic.ContainsKey("m") && grantAckChannelAuthDataDic["m"].ToString() == "1";
                                                authData.DeleteEnabled = grantAckChannelAuthDataDic.ContainsKey("d") && grantAckChannelAuthDataDic["d"].ToString() == "1";
                                                authData.GetEnabled =    grantAckChannelAuthDataDic.ContainsKey("g") && grantAckChannelAuthDataDic["g"].ToString() == "1";
                                                authData.UpdateEnabled = grantAckChannelAuthDataDic.ContainsKey("u") && grantAckChannelAuthDataDic["u"].ToString() == "1";
                                                authData.JoinEnabled =   grantAckChannelAuthDataDic.ContainsKey("j") && grantAckChannelAuthDataDic["j"].ToString() == "1";

                                                authKeyDataDic.Add(authKey, authData);
                                            }

                                        }

                                        ack.ChannelGroups.Add(channelGroupName, authKeyDataDic);
                                    }
                                }
                            } //end of if channel-group

                            if (grantAckPayloadDic.ContainsKey("uuids"))
                            {
                                ack.TargetUuids = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                Dictionary<string, object> grantAckCgListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDic["uuids"]);
                                if (grantAckCgListDic != null && grantAckCgListDic.Count > 0)
                                {
                                    foreach (string uuid in grantAckCgListDic.Keys)
                                    {
                                        Dictionary<string, object> grantAckUuidDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckCgListDic[uuid]);
                                        if (grantAckUuidDataDic != null && grantAckUuidDataDic.Count > 0)
                                        {
                                            if (grantAckUuidDataDic.ContainsKey("auths"))
                                            {
                                                Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                                Dictionary<string, object> grantAckUuidAuthListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckUuidDataDic["auths"]);
                                                if (grantAckUuidAuthListDic != null && grantAckUuidAuthListDic.Count > 0)
                                                {
                                                    foreach (string authKey in grantAckUuidAuthListDic.Keys)
                                                    {
                                                        Dictionary<string, object> grantAckUuidAuthDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckUuidAuthListDic[authKey]);
                                                        if (grantAckUuidAuthDataDic != null && grantAckUuidAuthDataDic.Count > 0)
                                                        {
                                                            PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                            authData.ReadEnabled = grantAckUuidAuthDataDic.ContainsKey("r") && grantAckUuidAuthDataDic["r"].ToString() == "1";
                                                            authData.WriteEnabled = grantAckUuidAuthDataDic.ContainsKey("w") && grantAckUuidAuthDataDic["w"].ToString() == "1";
                                                            authData.ManageEnabled = grantAckUuidAuthDataDic.ContainsKey("m") && grantAckUuidAuthDataDic["m"].ToString() == "1";
                                                            authData.DeleteEnabled = grantAckUuidAuthDataDic.ContainsKey("d") && grantAckUuidAuthDataDic["d"].ToString() == "1";
                                                            authData.GetEnabled = grantAckUuidAuthDataDic.ContainsKey("g") && grantAckUuidAuthDataDic["g"].ToString() == "1";
                                                            authData.UpdateEnabled = grantAckUuidAuthDataDic.ContainsKey("u") && grantAckUuidAuthDataDic["u"].ToString() == "1";
                                                            authData.JoinEnabled = grantAckUuidAuthDataDic.ContainsKey("j") && grantAckUuidAuthDataDic["j"].ToString() == "1";

                                                            authKeyDataDic.Add(authKey, authData);
                                                        }

                                                    }

                                                    ack.TargetUuids.Add(uuid, authKeyDataDic);
                                                }
                                            }
                                        }
                                    }
                                }// if no dictionary due to REST bug
                                else
                                {
                                    string targetUuid = grantAckPayloadDic["uuids"].ToString();
                                    if (grantAckPayloadDic.ContainsKey("auths"))
                                    {
                                        Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                        Dictionary<string, object> grantAckUuidAuthListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDic["auths"]);

                                        if (grantAckUuidAuthListDic != null && grantAckUuidAuthListDic.Count > 0)
                                        {
                                            foreach (string authKey in grantAckUuidAuthListDic.Keys)
                                            {
                                                Dictionary<string, object> grantAckUuidAuthDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckUuidAuthListDic[authKey]);
                                                if (grantAckUuidAuthDataDic != null && grantAckUuidAuthDataDic.Count > 0)
                                                {
                                                    PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                    authData.ReadEnabled = grantAckUuidAuthDataDic.ContainsKey("r") && grantAckUuidAuthDataDic["r"].ToString() == "1";
                                                    authData.WriteEnabled = grantAckUuidAuthDataDic.ContainsKey("w") && grantAckUuidAuthDataDic["w"].ToString() == "1";
                                                    authData.ManageEnabled = grantAckUuidAuthDataDic.ContainsKey("m") && grantAckUuidAuthDataDic["m"].ToString() == "1";
                                                    authData.DeleteEnabled = grantAckUuidAuthDataDic.ContainsKey("d") && grantAckUuidAuthDataDic["d"].ToString() == "1";
                                                    authData.GetEnabled = grantAckUuidAuthDataDic.ContainsKey("g") && grantAckUuidAuthDataDic["g"].ToString() == "1";
                                                    authData.UpdateEnabled = grantAckUuidAuthDataDic.ContainsKey("u") && grantAckUuidAuthDataDic["u"].ToString() == "1";
                                                    authData.JoinEnabled = grantAckUuidAuthDataDic.ContainsKey("j") && grantAckUuidAuthDataDic["j"].ToString() == "1";

                                                    authKeyDataDic.Add(authKey, authData);
                                                }

                                            }

                                            ack.TargetUuids.Add(targetUuid, authKeyDataDic);
                                        }
                                    }

                                } //end of else if for REST bug
                            }//end of if uuids
                            else if (grantAckPayloadDic.ContainsKey("uuid"))
                            {
                                ack.TargetUuids = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                string uuid = grantAckPayloadDic["uuid"].ToString();
                                if (grantAckPayloadDic.ContainsKey("auths"))
                                {
                                    Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                    Dictionary<string, object> grantAckChannelAuthListDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckPayloadDic["auths"]);

                                    if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                    {
                                        foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                        {
                                            Dictionary<string, object> grantAckChannelAuthDataDic = JsonDataParseInternalUtil.ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                            if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                            {
                                                PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                authData.ReadEnabled = grantAckChannelAuthDataDic.ContainsKey("r") && grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                authData.WriteEnabled = grantAckChannelAuthDataDic.ContainsKey("w") && grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                authData.ManageEnabled = grantAckChannelAuthDataDic.ContainsKey("m") && grantAckChannelAuthDataDic["m"].ToString() == "1";
                                                authData.DeleteEnabled = grantAckChannelAuthDataDic.ContainsKey("d") && grantAckChannelAuthDataDic["d"].ToString() == "1";
                                                authData.GetEnabled = grantAckChannelAuthDataDic.ContainsKey("g") && grantAckChannelAuthDataDic["g"].ToString() == "1";
                                                authData.UpdateEnabled = grantAckChannelAuthDataDic.ContainsKey("u") && grantAckChannelAuthDataDic["u"].ToString() == "1";
                                                authData.JoinEnabled = grantAckChannelAuthDataDic.ContainsKey("j") && grantAckChannelAuthDataDic["j"].ToString() == "1";

                                                authKeyDataDic.Add(authKey, authData);
                                            }

                                        }

                                        ack.TargetUuids.Add(uuid, authKeyDataDic);
                                    }
                                }
                            } //end of if uuid

                        } //end of else subkey

                    }

                }
            }

            return ack;
        }
    }
}
