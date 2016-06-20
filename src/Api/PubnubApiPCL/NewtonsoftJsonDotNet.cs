using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PubnubApi
{
    public class NewtonsoftJsonDotNet : IJsonPluggableLibrary
    {
        #region IJsonPlugableLibrary methods implementation
        private bool IsValidJson(string jsonString)
        {
            bool ret = false;
            try
            {
                JObject.Parse(jsonString);
                ret = true;
            }
            catch { }
            return ret;
        }

        public bool IsArrayCompatible(string jsonString)
        {
            bool ret = false;
            if (IsValidJson(jsonString))
            {
                JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
                while (reader.Read())
                {
                    if (reader.LineNumber == 1 && reader.LinePosition == 1 && reader.TokenType == JsonToken.StartArray)
                    {
                        ret = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return ret;
        }

        public bool IsDictionaryCompatible(string jsonString)
        {
            bool ret = false;
            if (IsValidJson(jsonString))
            {
                JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
                while (reader.Read())
                {
                    if (reader.LineNumber == 1 && reader.LinePosition == 1 && reader.TokenType == JsonToken.StartObject)
                    {
                        ret = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return ret;
        }

        public string SerializeToJsonString(object objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize);
        }

        public List<object> DeserializeToListOfObject(string jsonString)
        {
            List<object> result = JsonConvert.DeserializeObject<List<object>>(jsonString);

            return result;
        }

        public object DeserializeToObject(string jsonString)
        {
            object result = JsonConvert.DeserializeObject<object>(jsonString);
            if (result.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                JArray jarrayResult = result as JArray;
                List<object> objectContainer = jarrayResult.ToObject<List<object>>();
                if (objectContainer != null && objectContainer.Count > 0)
                {
                    for (int index = 0; index < objectContainer.Count; index++)
                    {
                        if (objectContainer[index].GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                        {
                            JArray internalItem = objectContainer[index] as JArray;
                            objectContainer[index] = internalItem.Select(item => (object)item).ToArray();
                        }
                    }
                    result = objectContainer;
                }
            }
            return result;
        }

        public void PopulateObject(string value, object target)
        {
            JsonConvert.PopulateObject(value, target);
        }

        public virtual T DeserializeToObject<T>(string jsonString)
        {
            T ret = default(T);

            try
            {
                ret = JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch { }

            return ret;
        }

        public virtual T DeserializeToObject<T>(List<object> listObject)
        {
            T ret = default(T);

            if (listObject == null)
            {
                return ret;
            }

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Message<>))
            {
                #region "Subscribe Message<>"
                Type dataType = typeof(T).GetGenericArguments()[0];
                Type generic = typeof(Message<>);
                Type specific = generic.MakeGenericType(dataType);

                //ConstructorInfo ci = specific.GetConstructor(Type.EmptyTypes);
                ConstructorInfo ci = specific.GetConstructors().FirstOrDefault();
                if (ci != null)
                {
                    object message = ci.Invoke(new object[] { });

                    //Set data
                    PropertyInfo dataProp = specific.GetProperty("Data");

                    object userMessage = null;
                    if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JValue))
                    {
                        JValue jValue = listObject[0] as JValue;
                        userMessage = jValue.Value;

                        dataProp.SetValue(message, userMessage, null);
                    }
                    else if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                    {
                        JToken token = listObject[0] as JToken;
                        if (dataProp.PropertyType == typeof(string))
                        {
                            userMessage = JsonConvert.SerializeObject(token);
                        }
                        else
                        {
                            userMessage = token.ToObject(dataProp.PropertyType, JsonSerializer.Create());
                        }

                        //userMessage = ConvertJTokenToObject(listObject[0] as JToken);
                        //userMessage = Activator.CreateInstance(
                        //PopulateObject(listObject[0].ToString(), message);
                        dataProp.SetValue(message, userMessage, null);
                    }
                    else if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                    {
                        JToken token = listObject[0] as JToken;
                        userMessage = token.ToObject(dataProp.PropertyType, JsonSerializer.Create());

                        //userMessage = ConvertJTokenToObject(listObject[0] as JToken);
                        //userMessage = Activator.CreateInstance(
                        //PopulateObject(listObject[0].ToString(), message);
                        dataProp.SetValue(message, userMessage, null);
                    }
                    else if (listObject[0].GetType() == typeof(System.String))
                    {
                        userMessage = listObject[0] as string;
                        dataProp.SetValue(message, userMessage, null);
                    }

                    //Set Time
                    PropertyInfo timeProp = specific.GetProperty("Time");
                    timeProp.SetValue(message, Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(listObject[1].ToString()), null);

                    // Set ChannelName
                    PropertyInfo channelNameProp = specific.GetProperty("ChannelName");
                    channelNameProp.SetValue(message, (listObject.Count == 4) ? listObject[3].ToString() : listObject[2].ToString(), null);

                    PropertyInfo typeProp = specific.GetProperty("Type");
                    typeProp.SetValue(message, dataType, null);

                    ret = (T)Convert.ChangeType(message, specific, CultureInfo.InvariantCulture);
                }
                #endregion
            }
            else if (typeof(T) == typeof(GrantAck))
            {
                #region "GrantAck"
                Dictionary<string, object> grantDicObj = ConvertToDictionaryObject(listObject[0]);

                GrantAck ack = null;

                int statusCode = 0; //For Grant, status code 200 = success

                if (grantDicObj != null)
                {
                    ack = new GrantAck();

                    if (int.TryParse(grantDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = grantDicObj["message"].ToString();

                    ack.Service = grantDicObj["service"].ToString();

                    if (grantDicObj.ContainsKey("warning"))
                    {
                        ack.Warning = Convert.ToBoolean(grantDicObj["warning"].ToString());
                    }

                    ack.Payload = new GrantAck.Data();

                    if (grantDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> grantAckPayloadDic = ConvertToDictionaryObject(grantDicObj["payload"]);
                        if (grantAckPayloadDic != null && grantAckPayloadDic.Count > 0)
                        {
                            if (grantAckPayloadDic.ContainsKey("level"))
                            {
                                ack.Payload.Level = grantAckPayloadDic["level"].ToString();
                            }

                            if (grantAckPayloadDic.ContainsKey("subscribe_key"))
                            {
                                ack.Payload.SubscribeKey = grantAckPayloadDic["subscribe_key"].ToString();
                            }

                            if (grantAckPayloadDic.ContainsKey("ttl"))
                            {
                                ack.Payload.TTL = Convert.ToInt32(grantAckPayloadDic["ttl"].ToString());
                            }

                            if (ack.Payload != null && ack.Payload.Level != null && ack.Payload.Level == "subkey")
                            {
                                ack.Payload.Access = new GrantAck.Data.SubkeyAccess();
                                ack.Payload.Access.read = grantAckPayloadDic["r"].ToString() == "1";
                                ack.Payload.Access.write = grantAckPayloadDic["w"].ToString() == "1";
                                ack.Payload.Access.manage = grantAckPayloadDic["m"].ToString() == "1";
                            }
                            else
                            {
                                if (grantAckPayloadDic.ContainsKey("channels"))
                                {
                                    ack.Payload.channels = new Dictionary<string, GrantAck.Data.ChannelData>();

                                    Dictionary<string, object> grantAckChannelListDic = ConvertToDictionaryObject(grantAckPayloadDic["channels"]);
                                    if (grantAckChannelListDic != null && grantAckChannelListDic.Count > 0)
                                    {
                                        foreach (string channel in grantAckChannelListDic.Keys)
                                        {
                                            Dictionary<string, object> grantAckChannelDataDic = ConvertToDictionaryObject(grantAckChannelListDic[channel]);
                                            if (grantAckChannelDataDic != null && grantAckChannelDataDic.Count > 0)
                                            {
                                                GrantAck.Data.ChannelData grantAckChannelData = new GrantAck.Data.ChannelData();
                                                if (grantAckChannelDataDic.ContainsKey("auths"))
                                                {
                                                    grantAckChannelData.auths = new Dictionary<string, GrantAck.Data.ChannelData.AuthData>();

                                                    Dictionary<string, object> grantAckChannelAuthListDic = ConvertToDictionaryObject(grantAckChannelDataDic["auths"]);
                                                    if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                                    {
                                                        foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                                        {
                                                            Dictionary<string, object> grantAckChannelAuthDataDic = ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                                            if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                            {
                                                                GrantAck.Data.ChannelData.AuthData authData = new GrantAck.Data.ChannelData.AuthData();
                                                                authData.Access = new GrantAck.Data.ChannelData.AuthData.AuthAccess();
                                                                authData.Access.read = grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                                authData.Access.write = grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                                authData.Access.manage = grantAckChannelAuthDataDic["m"].ToString() == "1";

                                                                grantAckChannelData.auths.Add(authKey, authData);
                                                            }

                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    grantAckChannelData.Access = new GrantAck.Data.ChannelData.ChannelAccess();
                                                    grantAckChannelData.Access.read = grantAckChannelDataDic["r"].ToString() == "1";
                                                    grantAckChannelData.Access.write = grantAckChannelDataDic["w"].ToString() == "1";
                                                    grantAckChannelData.Access.manage = grantAckChannelDataDic["m"].ToString() == "1";
                                                }

                                                ack.Payload.channels.Add(channel, grantAckChannelData);
                                            }
                                        }
                                    }
                                }//end of if channels
                                else if (grantAckPayloadDic.ContainsKey("channel"))
                                {
                                    ack.Payload.channels = new Dictionary<string, GrantAck.Data.ChannelData>();

                                    string channelName = grantAckPayloadDic["channel"].ToString();
                                    if (grantAckPayloadDic.ContainsKey("auths"))
                                    {
                                        GrantAck.Data.ChannelData grantAckChannelData = new GrantAck.Data.ChannelData();

                                        grantAckChannelData.auths = new Dictionary<string, GrantAck.Data.ChannelData.AuthData>();

                                        Dictionary<string, object> grantAckChannelAuthListDic = ConvertToDictionaryObject(grantAckPayloadDic["auths"]);
                                        if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                        {
                                            foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                            {
                                                Dictionary<string, object> grantAckChannelAuthDataDic = ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                                if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                {
                                                    GrantAck.Data.ChannelData.AuthData authData = new GrantAck.Data.ChannelData.AuthData();
                                                    authData.Access = new GrantAck.Data.ChannelData.AuthData.AuthAccess();
                                                    authData.Access.read = grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                    authData.Access.write = grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                    authData.Access.manage = grantAckChannelAuthDataDic["m"].ToString() == "1";

                                                    grantAckChannelData.auths.Add(authKey, authData);
                                                }

                                            }
                                            ack.Payload.channels.Add(channelName, grantAckChannelData);
                                        }
                                    }
                                }

                                if (grantAckPayloadDic.ContainsKey("channel-groups"))
                                {
                                    ack.Payload.channelgroups = new Dictionary<string, GrantAck.Data.ChannelGroupData>();

                                    Dictionary<string, object> grantAckCgListDic = ConvertToDictionaryObject(grantAckPayloadDic["channel-groups"]);
                                    if (grantAckCgListDic != null && grantAckCgListDic.Count > 0)
                                    {
                                        foreach (string channelgroup in grantAckCgListDic.Keys)
                                        {
                                            Dictionary<string, object> grantAckCgDataDic = ConvertToDictionaryObject(grantAckCgListDic[channelgroup]);
                                            if (grantAckCgDataDic != null && grantAckCgDataDic.Count > 0)
                                            {
                                                GrantAck.Data.ChannelGroupData grantAckCgData = new GrantAck.Data.ChannelGroupData();
                                                if (grantAckCgDataDic.ContainsKey("auths"))
                                                {
                                                    grantAckCgData.auths = new Dictionary<string, GrantAck.Data.ChannelGroupData.AuthData>();

                                                    Dictionary<string, object> grantAckCgAuthListDic = ConvertToDictionaryObject(grantAckCgDataDic["auths"]);
                                                    if (grantAckCgAuthListDic != null && grantAckCgAuthListDic.Count > 0)
                                                    {
                                                        foreach (string authKey in grantAckCgAuthListDic.Keys)
                                                        {
                                                            Dictionary<string, object> grantAckCgAuthDataDic = ConvertToDictionaryObject(grantAckCgAuthListDic[authKey]);
                                                            if (grantAckCgAuthDataDic != null && grantAckCgAuthDataDic.Count > 0)
                                                            {
                                                                GrantAck.Data.ChannelGroupData.AuthData authData = new GrantAck.Data.ChannelGroupData.AuthData();
                                                                authData.Access = new GrantAck.Data.ChannelGroupData.AuthData.AuthAccess();
                                                                authData.Access.read = grantAckCgAuthDataDic["r"].ToString() == "1";
                                                                authData.Access.write = grantAckCgAuthDataDic["w"].ToString() == "1";
                                                                authData.Access.manage = grantAckCgAuthDataDic["m"].ToString() == "1";

                                                                grantAckCgData.auths.Add(authKey, authData);
                                                            }

                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    grantAckCgData.Access = new GrantAck.Data.ChannelGroupData.ChannelGroupAccess();
                                                    grantAckCgData.Access.read = grantAckCgDataDic["r"].ToString() == "1";
                                                    grantAckCgData.Access.write = grantAckCgDataDic["w"].ToString() == "1";
                                                    grantAckCgData.Access.manage = grantAckCgDataDic["m"].ToString() == "1";
                                                }

                                                ack.Payload.channelgroups.Add(channelgroup, grantAckCgData);
                                            }
                                        }
                                    }
                                }//end of if channel-groups
                            } //end of else subkey

                        }

                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(GrantAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(AuditAck))
            {
                #region "AuditAck"
                Dictionary<string, object> auditDicObj = ConvertToDictionaryObject(listObject[0]);

                AuditAck ack = null;

                int statusCode = 0; //For Audit, status code 200 = success

                if (auditDicObj != null)
                {
                    ack = new AuditAck();

                    if (int.TryParse(auditDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = auditDicObj["message"].ToString();

                    ack.Service = auditDicObj["service"].ToString();

                    if (auditDicObj.ContainsKey("warning"))
                    {
                        ack.Warning = Convert.ToBoolean(auditDicObj["warning"].ToString());
                    }

                    ack.Payload = new AuditAck.Data();

                    //AuditAckPayload auditAckPayload = DeserializeToObject<AuditAckPayload>(ack.Payload);
                    if (auditDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> auditAckPayloadDic = ConvertToDictionaryObject(auditDicObj["payload"]);
                        if (auditAckPayloadDic != null && auditAckPayloadDic.Count > 0)
                        {
                            if (auditAckPayloadDic.ContainsKey("level"))
                            {
                                ack.Payload.Level = auditAckPayloadDic["level"].ToString();
                            }

                            if (auditAckPayloadDic.ContainsKey("subscribe_key"))
                            {
                                ack.Payload.SubscribeKey = auditAckPayloadDic["subscribe_key"].ToString();
                            }

                            if (auditAckPayloadDic.ContainsKey("channels"))
                            {
                                ack.Payload.channels = new Dictionary<string, AuditAck.Data.ChannelData>();

                                Dictionary<string, object> auditAckChannelListDic = ConvertToDictionaryObject(auditAckPayloadDic["channels"]);
                                if (auditAckChannelListDic != null && auditAckChannelListDic.Count > 0)
                                {
                                    foreach (string channel in auditAckChannelListDic.Keys)
                                    {
                                        Dictionary<string, object> auditAckChannelDataDic = ConvertToDictionaryObject(auditAckChannelListDic[channel]);
                                        if (auditAckChannelDataDic != null && auditAckChannelDataDic.Count > 0)
                                        {
                                            AuditAck.Data.ChannelData auditAckChannelData = new AuditAck.Data.ChannelData();
                                            if (auditAckChannelDataDic.ContainsKey("auths"))
                                            {
                                                auditAckChannelData.auths = new Dictionary<string, AuditAck.Data.ChannelData.AuthData>();

                                                Dictionary<string, object> auditAckChannelAuthListDic = ConvertToDictionaryObject(auditAckChannelDataDic["auths"]);
                                                if (auditAckChannelAuthListDic != null && auditAckChannelAuthListDic.Count > 0)
                                                {
                                                    foreach (string authKey in auditAckChannelAuthListDic.Keys)
                                                    {
                                                        Dictionary<string, object> auditAckChannelAuthDataDic = ConvertToDictionaryObject(auditAckChannelAuthListDic[authKey]);
                                                        if (auditAckChannelAuthDataDic != null && auditAckChannelAuthDataDic.Count > 0)
                                                        {
                                                            AuditAck.Data.ChannelData.AuthData authData = new AuditAck.Data.ChannelData.AuthData();
                                                            authData.Access = new AuditAck.Data.ChannelData.AuthData.AuthAccess();
                                                            authData.Access.read = auditAckChannelAuthDataDic["r"].ToString() == "1";
                                                            authData.Access.write = auditAckChannelAuthDataDic["w"].ToString() == "1";
                                                            authData.Access.manage = auditAckChannelAuthDataDic.ContainsKey("m") ? auditAckChannelAuthDataDic["m"].ToString() == "1" : false;
                                                            if (auditAckChannelAuthDataDic.ContainsKey("ttl"))
                                                            {
                                                                authData.Access.TTL = Int32.Parse(auditAckChannelAuthDataDic["ttl"].ToString());
                                                            }

                                                            auditAckChannelData.auths.Add(authKey, authData);
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                auditAckChannelData.Access = new AuditAck.Data.ChannelData.ChannelAccess();
                                                auditAckChannelData.Access.read = auditAckChannelDataDic["r"].ToString() == "1";
                                                auditAckChannelData.Access.write = auditAckChannelDataDic["w"].ToString() == "1";
                                                auditAckChannelData.Access.manage = auditAckChannelDataDic.ContainsKey("m") ? auditAckChannelDataDic["m"].ToString() == "1" : false;
                                                if (auditAckChannelDataDic.ContainsKey("ttl"))
                                                {
                                                    auditAckChannelData.Access.TTL = Int32.Parse(auditAckChannelDataDic["ttl"].ToString());
                                                }
                                            }

                                            ack.Payload.channels.Add(channel, auditAckChannelData);
                                        }
                                    }
                                }
                            }//end of if channels
                            if (auditAckPayloadDic.ContainsKey("channel-groups"))
                            {
                                ack.Payload.channelgroups = new Dictionary<string, AuditAck.Data.ChannelGroupData>();

                                Dictionary<string, object> auditAckCgListDic = ConvertToDictionaryObject(auditAckPayloadDic["channel-groups"]);
                                if (auditAckCgListDic != null && auditAckCgListDic.Count > 0)
                                {
                                    foreach (string channelgroup in auditAckCgListDic.Keys)
                                    {
                                        Dictionary<string, object> auditAckCgDataDic = ConvertToDictionaryObject(auditAckCgListDic[channelgroup]);
                                        if (auditAckCgDataDic != null && auditAckCgDataDic.Count > 0)
                                        {
                                            AuditAck.Data.ChannelGroupData auditAckCgData = new AuditAck.Data.ChannelGroupData();
                                            if (auditAckCgDataDic.ContainsKey("auths"))
                                            {
                                                auditAckCgData.auths = new Dictionary<string, AuditAck.Data.ChannelGroupData.AuthData>();

                                                Dictionary<string, object> auditAckCgAuthListDic = ConvertToDictionaryObject(auditAckCgDataDic["auths"]);
                                                if (auditAckCgAuthListDic != null && auditAckCgAuthListDic.Count > 0)
                                                {
                                                    foreach (string authKey in auditAckCgAuthListDic.Keys)
                                                    {
                                                        Dictionary<string, object> auditAckCgAuthDataDic = ConvertToDictionaryObject(auditAckCgAuthListDic[authKey]);
                                                        if (auditAckCgAuthDataDic != null && auditAckCgAuthDataDic.Count > 0)
                                                        {
                                                            AuditAck.Data.ChannelGroupData.AuthData authData = new AuditAck.Data.ChannelGroupData.AuthData();
                                                            authData.Access = new AuditAck.Data.ChannelGroupData.AuthData.AuthAccess();
                                                            authData.Access.read = auditAckCgAuthDataDic["r"].ToString() == "1";
                                                            authData.Access.write = auditAckCgAuthDataDic["w"].ToString() == "1";
                                                            authData.Access.manage = auditAckCgAuthDataDic.ContainsKey("m") ? auditAckCgAuthDataDic["m"].ToString() == "1" : false;
                                                            if (auditAckCgAuthDataDic.ContainsKey("ttl"))
                                                            {
                                                                authData.Access.TTL = Int32.Parse(auditAckCgAuthDataDic["ttl"].ToString());
                                                            }

                                                            auditAckCgData.auths.Add(authKey, authData);
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                auditAckCgData.Access = new AuditAck.Data.ChannelGroupData.ChannelGroupAccess();
                                                auditAckCgData.Access.read = auditAckCgDataDic["r"].ToString() == "1";
                                                auditAckCgData.Access.write = auditAckCgDataDic["w"].ToString() == "1";
                                                auditAckCgData.Access.manage = auditAckCgDataDic.ContainsKey("m") ? auditAckCgDataDic["m"].ToString() == "1" : false;
                                                if (auditAckCgDataDic.ContainsKey("ttl"))
                                                {
                                                    auditAckCgData.Access.TTL = Int32.Parse(auditAckCgDataDic["ttl"].ToString());
                                                }
                                            }

                                            ack.Payload.channelgroups.Add(channelgroup, auditAckCgData);
                                        }
                                    }
                                }
                            }//end of if channel-groups

                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(AuditAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(ConnectOrDisconnectAck))
            {
                #region "ConnectOrDisconnectAck"
                var ack = new ConnectOrDisconnectAck
                {
                    StatusMessage = listObject[1].ToString(),
                    ChannelGroupName = (listObject.Count == 4) ? listObject[2].ToString() : "",
                    ChannelName = (listObject.Count == 4) ? listObject[3].ToString() : listObject[2].ToString()
                };
                int statusCode;
                if (int.TryParse(listObject[0].ToString(), out statusCode))
                    ack.StatusCode = statusCode;

                ret = (T)Convert.ChangeType(ack, typeof(ConnectOrDisconnectAck), CultureInfo.InvariantCulture);
            }
            else if (typeof(T) == typeof(PublishAck))
            {
                var ack = new PublishAck
                {
                    StatusMessage = listObject[1].ToString(),
                    Timetoken = Int64.Parse(listObject[2].ToString()),
                    ChannelName = listObject[3].ToString(),
                    Payload = (listObject.Count == 5) ? listObject[4] : null
                };
                int statusCode;
                if (int.TryParse(listObject[0].ToString(), out statusCode))
                    ack.StatusCode = statusCode;

                ret = (T)Convert.ChangeType(ack, typeof(PublishAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(PresenceAck))
            {
                #region "PresenceAck"
                Dictionary<string, object> presenceDicObj = ConvertToDictionaryObject(listObject[0]);

                PresenceAck ack = null;

                if (presenceDicObj != null)
                {
                    ack = new PresenceAck();
                    ack.Action = presenceDicObj["action"].ToString();
                    ack.Timestamp = Convert.ToInt64(presenceDicObj["timestamp"].ToString());
                    ack.UUID = presenceDicObj["uuid"].ToString();
                    ack.Occupancy = Int32.Parse(presenceDicObj["occupancy"].ToString());

                    //ack.Timetoken = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(listObject[1].ToString()),
                    ack.Timetoken = Convert.ToInt64(listObject[1].ToString());
                    ack.ChannelGroupName = (listObject.Count == 4) ? listObject[2].ToString() : "";
                    ack.ChannelName = (listObject.Count == 4) ? listObject[3].ToString() : listObject[2].ToString();
                }


                ret = (T)Convert.ChangeType(ack, typeof(PresenceAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(DetailedHistoryAck))
            {
                #region "DetailedHistoryAck"
                DetailedHistoryAck ack = new DetailedHistoryAck();
                ack.StartTimeToken = Convert.ToInt64(listObject[1].ToString());
                ack.EndTimeToken = Convert.ToInt64(listObject[2].ToString());
                ack.ChannelName = listObject[3].ToString();
                ack.Message = ConvertToObjectArray(listObject[0]);

                ret = (T)Convert.ChangeType(ack, typeof(DetailedHistoryAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(HereNowAck))
            {
                #region "HereNowAck"
                Dictionary<string, object> herenowDicObj = ConvertToDictionaryObject(listObject[0]);

                HereNowAck ack = null;

                int statusCode = 0;

                if (herenowDicObj != null)
                {
                    ack = new HereNowAck();

                    if (int.TryParse(herenowDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = herenowDicObj["message"].ToString();

                    ack.Service = herenowDicObj["service"].ToString();

                    ack.ChannelName = listObject[1].ToString();

                    ack.Payload = new HereNowAck.Data();

                    if (herenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> hereNowPayloadDic = ConvertToDictionaryObject(herenowDicObj["payload"]);
                        if (hereNowPayloadDic != null && hereNowPayloadDic.Count > 0)
                        {
                            ack.Payload.total_occupancy = Int32.Parse(hereNowPayloadDic["total_occupancy"].ToString());
                            ack.Payload.total_channels = Int32.Parse(hereNowPayloadDic["total_channels"].ToString());
                            if (hereNowPayloadDic.ContainsKey("channels"))
                            {
                                ack.Payload.channels = new Dictionary<string, HereNowAck.Data.ChannelData>();

                                Dictionary<string, object> hereNowChannelListDic = ConvertToDictionaryObject(hereNowPayloadDic["channels"]);
                                if (hereNowChannelListDic != null && hereNowChannelListDic.Count > 0)
                                {
                                    foreach (string channel in hereNowChannelListDic.Keys)
                                    {
                                        Dictionary<string, object> hereNowChannelItemDic = ConvertToDictionaryObject(hereNowChannelListDic[channel]);
                                        if (hereNowChannelItemDic != null && hereNowChannelItemDic.Count > 0)
                                        {
                                            HereNowAck.Data.ChannelData channelData = new HereNowAck.Data.ChannelData();
                                            channelData.occupancy = Convert.ToInt32(hereNowChannelItemDic["occupancy"].ToString());
                                            if (hereNowChannelItemDic.ContainsKey("uuids"))
                                            {
                                                object[] hereNowChannelUuidList = ConvertToObjectArray(hereNowChannelItemDic["uuids"]);
                                                if (hereNowChannelUuidList != null && hereNowChannelUuidList.Length > 0)
                                                {
                                                    List<HereNowAck.Data.ChannelData.UuidData> uuidDataList = new List<HereNowAck.Data.ChannelData.UuidData>();

                                                    for (int index = 0; index < hereNowChannelUuidList.Length; index++)
                                                    {
                                                        if (hereNowChannelUuidList[index].GetType() == typeof(string))
                                                        {
                                                            HereNowAck.Data.ChannelData.UuidData uuidData = new HereNowAck.Data.ChannelData.UuidData();
                                                            uuidData.uuid = hereNowChannelUuidList[index].ToString();
                                                            uuidDataList.Add(uuidData);
                                                        }
                                                        else
                                                        {
                                                            Dictionary<string, object> hereNowChannelItemUuidsDic = ConvertToDictionaryObject(hereNowChannelUuidList[index]);
                                                            if (hereNowChannelItemUuidsDic != null && hereNowChannelItemUuidsDic.Count > 0)
                                                            {
                                                                HereNowAck.Data.ChannelData.UuidData uuidData = new HereNowAck.Data.ChannelData.UuidData();
                                                                uuidData.uuid = hereNowChannelItemUuidsDic["uuid"].ToString();
                                                                if (hereNowChannelItemUuidsDic.ContainsKey("state"))
                                                                {
                                                                    uuidData.state = ConvertToDictionaryObject(hereNowChannelItemUuidsDic["state"]);
                                                                }
                                                                uuidDataList.Add(uuidData);
                                                            }
                                                        }
                                                    }
                                                    channelData.uuids = uuidDataList.ToArray();
                                                }
                                            }
                                            ack.Payload.channels.Add(channel, channelData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (herenowDicObj.ContainsKey("occupancy"))
                    {
                        ack.Payload.total_occupancy = Int32.Parse(herenowDicObj["occupancy"].ToString());
                        ack.Payload.channels = new Dictionary<string, HereNowAck.Data.ChannelData>();
                        if (herenowDicObj.ContainsKey("uuids"))
                        {
                            object[] uuidArray = ConvertToObjectArray(herenowDicObj["uuids"]);
                            if (uuidArray != null && uuidArray.Length > 0)
                            {
                                List<HereNowAck.Data.ChannelData.UuidData> uuidDataList = new List<HereNowAck.Data.ChannelData.UuidData>();
                                for (int index = 0; index < uuidArray.Length; index++)
                                {
                                    Dictionary<string, object> hereNowChannelItemUuidsDic = ConvertToDictionaryObject(uuidArray[index]);
                                    if (hereNowChannelItemUuidsDic != null && hereNowChannelItemUuidsDic.Count > 0)
                                    {
                                        HereNowAck.Data.ChannelData.UuidData uuidData = new HereNowAck.Data.ChannelData.UuidData();
                                        uuidData.uuid = hereNowChannelItemUuidsDic["uuid"].ToString();
                                        if (hereNowChannelItemUuidsDic.ContainsKey("state"))
                                        {
                                            uuidData.state = ConvertToDictionaryObject(hereNowChannelItemUuidsDic["state"]);
                                        }
                                        uuidDataList.Add(uuidData);
                                    }
                                    else
                                    {
                                        HereNowAck.Data.ChannelData.UuidData uuidData = new HereNowAck.Data.ChannelData.UuidData();
                                        uuidData.uuid = uuidArray[index].ToString();
                                        uuidDataList.Add(uuidData);
                                    }
                                }
                                HereNowAck.Data.ChannelData channelData = new HereNowAck.Data.ChannelData();
                                channelData.uuids = uuidDataList.ToArray();
                                channelData.occupancy = ack.Payload.total_occupancy;

                                ack.Payload.channels.Add(ack.ChannelName, channelData);
                                ack.Payload.total_channels = ack.Payload.channels.Count;
                            }
                        }
                        else
                        {
                            string channels = listObject[1].ToString();
                            string[] arrChannel = channels.Split(',');
                            int totalChannels = 0;
                            foreach (string channel in arrChannel)
                            {
                                HereNowAck.Data.ChannelData channelData = new HereNowAck.Data.ChannelData();
                                channelData.occupancy = 1;
                                ack.Payload.channels.Add(channel, channelData);
                                totalChannels++;
                            }
                            ack.Payload.total_channels = totalChannels;


                        }
                    }

                }

                ret = (T)Convert.ChangeType(ack, typeof(HereNowAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GlobalHereNowAck))
            {
                #region "GlobalHereNowAck"
                Dictionary<string, object> globalHerenowDicObj = ConvertToDictionaryObject(listObject[0]);

                GlobalHereNowAck ack = null;

                int statusCode = 0;

                if (globalHerenowDicObj != null)
                {
                    ack = new GlobalHereNowAck();

                    if (int.TryParse(globalHerenowDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = globalHerenowDicObj["message"].ToString();

                    ack.Service = globalHerenowDicObj["service"].ToString();

                    ack.Payload = new GlobalHereNowAck.Data();
                    if (globalHerenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> globalHereNowPayloadDic = ConvertToDictionaryObject(globalHerenowDicObj["payload"]);
                        if (globalHereNowPayloadDic != null && globalHereNowPayloadDic.Count > 0)
                        {
                            ack.Payload.total_occupancy = Int32.Parse(globalHereNowPayloadDic["total_occupancy"].ToString());
                            ack.Payload.total_channels = Int32.Parse(globalHereNowPayloadDic["total_channels"].ToString());
                            if (globalHereNowPayloadDic.ContainsKey("channels"))
                            {
                                ack.Payload.channels = new Dictionary<string, GlobalHereNowAck.Data.ChannelData>();

                                Dictionary<string, object> globalHereNowChannelListDic = ConvertToDictionaryObject(globalHereNowPayloadDic["channels"]);
                                if (globalHereNowChannelListDic != null && globalHereNowChannelListDic.Count > 0)
                                {
                                    foreach (string channel in globalHereNowChannelListDic.Keys)
                                    {
                                        Dictionary<string, object> globalHereNowChannelItemDic = ConvertToDictionaryObject(globalHereNowChannelListDic[channel]);
                                        if (globalHereNowChannelItemDic != null && globalHereNowChannelItemDic.Count > 0)
                                        {
                                            GlobalHereNowAck.Data.ChannelData channelData = new GlobalHereNowAck.Data.ChannelData();
                                            channelData.occupancy = Convert.ToInt32(globalHereNowChannelItemDic["occupancy"].ToString());
                                            if (globalHereNowChannelItemDic.ContainsKey("uuids"))
                                            {
                                                object[] globalHereNowChannelUuidList = ConvertToObjectArray(globalHereNowChannelItemDic["uuids"]);
                                                if (globalHereNowChannelUuidList != null && globalHereNowChannelUuidList.Length > 0)
                                                {
                                                    List<GlobalHereNowAck.Data.ChannelData.UuidData> uuidDataList = new List<GlobalHereNowAck.Data.ChannelData.UuidData>();

                                                    for (int index = 0; index < globalHereNowChannelUuidList.Length; index++)
                                                    {
                                                        if (globalHereNowChannelUuidList[index].GetType() == typeof(string))
                                                        {
                                                            GlobalHereNowAck.Data.ChannelData.UuidData uuidData = new GlobalHereNowAck.Data.ChannelData.UuidData();
                                                            uuidData.uuid = globalHereNowChannelUuidList[index].ToString();
                                                            uuidDataList.Add(uuidData);
                                                        }
                                                        else
                                                        {
                                                            Dictionary<string, object> globalHereNowChannelItemUuidsDic = ConvertToDictionaryObject(globalHereNowChannelUuidList[index]);
                                                            if (globalHereNowChannelItemUuidsDic != null && globalHereNowChannelItemUuidsDic.Count > 0)
                                                            {
                                                                GlobalHereNowAck.Data.ChannelData.UuidData uuidData = new GlobalHereNowAck.Data.ChannelData.UuidData();
                                                                uuidData.uuid = globalHereNowChannelItemUuidsDic["uuid"].ToString();
                                                                if (globalHereNowChannelItemUuidsDic.ContainsKey("state"))
                                                                {
                                                                    uuidData.state = ConvertToDictionaryObject(globalHereNowChannelItemUuidsDic["state"]);
                                                                }
                                                                uuidDataList.Add(uuidData);
                                                            }
                                                        }
                                                    }
                                                    channelData.uuids = uuidDataList.ToArray();
                                                }
                                            }
                                            ack.Payload.channels.Add(channel, channelData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(GlobalHereNowAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(WhereNowAck))
            {
                #region "WhereNowAck"
                Dictionary<string, object> wherenowDicObj = ConvertToDictionaryObject(listObject[0]);

                WhereNowAck ack = null;

                int statusCode = 0;

                if (wherenowDicObj != null)
                {
                    ack = new WhereNowAck();

                    if (int.TryParse(wherenowDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = wherenowDicObj["message"].ToString();

                    ack.Service = wherenowDicObj["service"].ToString();

                    ack.Payload = new WhereNowAck.Data();
                    if (wherenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> whereNowPayloadDic = ConvertToDictionaryObject(wherenowDicObj["payload"]);
                        if (whereNowPayloadDic != null && whereNowPayloadDic.Count > 0)
                        {
                            if (whereNowPayloadDic.ContainsKey("channels"))
                            {
                                //ack.Payload.channels = null;
                                object[] whereNowChannelList = ConvertToObjectArray(whereNowPayloadDic["channels"]);
                                if (whereNowChannelList != null && whereNowChannelList.Length >= 0)
                                {
                                    List<string> channelList = new List<string>();
                                    foreach (string channel in whereNowChannelList)
                                    {
                                        channelList.Add(channel);
                                    }
                                    ack.Payload.channels = channelList.ToArray();
                                }

                            }
                        }
                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(WhereNowAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(SetUserStateAck))
            {
                #region "SetUserStateAck"
                Dictionary<string, object> setUserStatewDicObj = ConvertToDictionaryObject(listObject[0]);

                SetUserStateAck ack = null;

                int statusCode = 0;

                if (setUserStatewDicObj != null)
                {
                    ack = new SetUserStateAck();

                    if (int.TryParse(setUserStatewDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = setUserStatewDicObj["message"].ToString();

                    ack.Service = setUserStatewDicObj["service"].ToString();

                    if (listObject != null && listObject.Count >= 2 && listObject[1] != null && !string.IsNullOrEmpty(listObject[1].ToString()))
                    {
                        ack.ChannelGroupName = listObject[1].ToString().Split(',');
                    }

                    if (listObject != null && listObject.Count >= 3 && listObject[2] != null && !string.IsNullOrEmpty(listObject[2].ToString()))
                    {
                        ack.ChannelName = listObject[2].ToString().Split(',');
                    }

                    ack.Payload = new Dictionary<string, object>();

                    if (setUserStatewDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> setStateDic = ConvertToDictionaryObject(setUserStatewDicObj["payload"]);
                        if (setStateDic != null)
                        {
                            ack.Payload = setStateDic;
                        }
                    }

                }

                ret = (T)Convert.ChangeType(ack, typeof(SetUserStateAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GetUserStateAck))
            {
                #region "GetUserStateAck"
                Dictionary<string, object> getUserStatewDicObj = ConvertToDictionaryObject(listObject[0]);

                GetUserStateAck ack = null;

                int statusCode = 0;

                if (getUserStatewDicObj != null)
                {
                    ack = new GetUserStateAck();

                    if (int.TryParse(getUserStatewDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = getUserStatewDicObj["message"].ToString();

                    ack.Service = getUserStatewDicObj["service"].ToString();

                    ack.UUID = getUserStatewDicObj["uuid"].ToString();

                    if (listObject != null && listObject.Count >= 2 && listObject[1] != null && !string.IsNullOrEmpty(listObject[1].ToString()))
                    {
                        ack.ChannelGroupName = listObject[1].ToString().Split(',');
                    }
                    if (listObject != null && listObject.Count >= 3 && listObject[2] != null && !string.IsNullOrEmpty(listObject[2].ToString()))
                    {
                        ack.ChannelName = listObject[2].ToString().Split(',');
                    }

                    ack.Payload = new Dictionary<string, object>();

                    if (getUserStatewDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> getStateDic = ConvertToDictionaryObject(getUserStatewDicObj["payload"]);
                        if (getStateDic != null)
                        {
                            ack.Payload = getStateDic;
                        }
                    }


                }

                ret = (T)Convert.ChangeType(ack, typeof(GetUserStateAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GetChannelGroupChannelsAck))
            {
                #region "GetChannelGroupChannelsAck"
                Dictionary<string, object> getCgChannelsDicObj = ConvertToDictionaryObject(listObject[0]);

                GetChannelGroupChannelsAck ack = null;

                int statusCode = 0;

                if (getCgChannelsDicObj != null)
                {
                    ack = new GetChannelGroupChannelsAck();

                    if (int.TryParse(getCgChannelsDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.Service = getCgChannelsDicObj["service"].ToString();

                    Dictionary<string, object> getCgChannelPayloadDic = ConvertToDictionaryObject(getCgChannelsDicObj["payload"]);
                    if (getCgChannelPayloadDic != null && getCgChannelPayloadDic.Count > 0)
                    {
                        ack.Payload = new GetChannelGroupChannelsAck.Data();
                        ack.Payload.ChannelGroupName = getCgChannelPayloadDic["group"].ToString();

                        object[] cgChPayloadChannels = ConvertToObjectArray(getCgChannelPayloadDic["channels"]);
                        if (cgChPayloadChannels != null && cgChPayloadChannels.Length > 0)
                        {
                            List<string> chList = new List<string>();
                            for (int index = 0; index < cgChPayloadChannels.Length; index++)
                            {
                                chList.Add(cgChPayloadChannels[index].ToString());
                            }
                            ack.Payload.ChannelName = chList.ToArray();
                        }
                    }

                    ack.Error = Convert.ToBoolean(getCgChannelsDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(GetChannelGroupChannelsAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GetAllChannelGroupsAck))
            {
                #region "GetAllChannelGroupsAck"
                Dictionary<string, object> getAllCgDicObj = ConvertToDictionaryObject(listObject[0]);

                GetAllChannelGroupsAck ack = null;

                int statusCode = 0;

                if (getAllCgDicObj != null)
                {
                    ack = new GetAllChannelGroupsAck();

                    if (int.TryParse(getAllCgDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.Service = getAllCgDicObj["service"].ToString();

                    Dictionary<string, object> getAllCgPayloadDic = ConvertToDictionaryObject(getAllCgDicObj["payload"]);
                    if (getAllCgPayloadDic != null && getAllCgPayloadDic.Count > 0)
                    {
                        ack.Payload = new GetAllChannelGroupsAck.Data();
                        ack.Payload.Namespace = getAllCgPayloadDic["namespace"].ToString();

                        object[] cgAllCgPayloadChannels = ConvertToObjectArray(getAllCgPayloadDic["groups"]);
                        if (cgAllCgPayloadChannels != null && cgAllCgPayloadChannels.Length > 0)
                        {
                            List<string> allCgList = new List<string>();
                            for (int index = 0; index < cgAllCgPayloadChannels.Length; index++)
                            {
                                allCgList.Add(cgAllCgPayloadChannels[index].ToString());
                            }
                            ack.Payload.ChannelGroupName = allCgList.ToArray();
                        }
                    }

                    ack.Error = Convert.ToBoolean(getAllCgDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(GetAllChannelGroupsAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(GetAllNamespacesAck))
            {
                #region "GetAllNamespacesAck"
                Dictionary<string, object> getAllNamespaceDicObj = ConvertToDictionaryObject(listObject[0]);

                GetAllNamespacesAck ack = null;

                int statusCode = 0;

                if (getAllNamespaceDicObj != null)
                {
                    ack = new GetAllNamespacesAck();

                    if (int.TryParse(getAllNamespaceDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.Service = getAllNamespaceDicObj["service"].ToString();

                    Dictionary<string, object> getAllNsPayloadDic = ConvertToDictionaryObject(getAllNamespaceDicObj["payload"]);
                    if (getAllNsPayloadDic != null && getAllNsPayloadDic.Count > 0)
                    {
                        ack.Payload = new GetAllNamespacesAck.Data();
                        ack.Payload.SubKey = getAllNsPayloadDic["sub_key"].ToString();

                        object[] cgAllNsPayloadNamespaces = ConvertToObjectArray(getAllNsPayloadDic["namespaces"]);
                        if (cgAllNsPayloadNamespaces != null && cgAllNsPayloadNamespaces.Length > 0)
                        {
                            List<string> allCgList = new List<string>();
                            for (int index = 0; index < cgAllNsPayloadNamespaces.Length; index++)
                            {
                                allCgList.Add(cgAllNsPayloadNamespaces[index].ToString());
                            }
                            ack.Payload.NamespaceName = allCgList.ToArray();
                        }
                    }

                    ack.Error = Convert.ToBoolean(getAllNamespaceDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(GetAllNamespacesAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(AddChannelToChannelGroupAck))
            {
                #region "AddChannelToChannelGroupAck"
                Dictionary<string, object> addChToCgDicObj = ConvertToDictionaryObject(listObject[0]);

                AddChannelToChannelGroupAck ack = null;

                int statusCode = 0;

                if (addChToCgDicObj != null)
                {
                    ack = new AddChannelToChannelGroupAck();

                    if (int.TryParse(addChToCgDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = addChToCgDicObj["message"].ToString();
                    ack.Service = addChToCgDicObj["service"].ToString();

                    ack.Error = Convert.ToBoolean(addChToCgDicObj["error"].ToString());

                    ack.ChannelGroupName = listObject[1].ToString();
                }

                ret = (T)Convert.ChangeType(ack, typeof(AddChannelToChannelGroupAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(RemoveChannelFromChannelGroupAck))
            {
                #region "RemoveChannelFromChannelGroupAck"
                Dictionary<string, object> removeChFromCgDicObj = ConvertToDictionaryObject(listObject[0]);

                RemoveChannelFromChannelGroupAck ack = null;

                int statusCode = 0;

                if (removeChFromCgDicObj != null)
                {
                    ack = new RemoveChannelFromChannelGroupAck();

                    if (int.TryParse(removeChFromCgDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = removeChFromCgDicObj["message"].ToString();
                    ack.Service = removeChFromCgDicObj["service"].ToString();

                    ack.Error = Convert.ToBoolean(removeChFromCgDicObj["error"].ToString());

                    ack.ChannelGroupName = listObject[1].ToString();
                }

                ret = (T)Convert.ChangeType(ack, typeof(RemoveChannelFromChannelGroupAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(RemoveChannelGroupAck))
            {
                #region "RemoveChannelGroupAck"
                Dictionary<string, object> removeCgDicObj = ConvertToDictionaryObject(listObject[0]);

                RemoveChannelGroupAck ack = null;

                int statusCode = 0;

                if (removeCgDicObj != null)
                {
                    ack = new RemoveChannelGroupAck();

                    if (int.TryParse(removeCgDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.Service = removeCgDicObj["service"].ToString();
                    ack.StatusMessage = removeCgDicObj["message"].ToString();

                    //Dictionary<string, object> removeCgPayloadDic = ConvertToDictionaryObject(removeCgDicObj["payload"]);
                    //if (removeCgPayloadDic != null && removeCgPayloadDic.Count > 0)
                    //{
                    //    ack.Payload = new RemoveChannelGroupAck.Data();
                    //    ack.Payload.ChannelGroupName = removeCgPayloadDic["group"].ToString();

                    //    object[] cgChPayloadChannels = ConvertToObjectArray(removeCgPayloadDic["channels"]);
                    //    if (cgChPayloadChannels != null && cgChPayloadChannels.Length > 0)
                    //    {
                    //        List<string> chList = new List<string>();
                    //        for (int index = 0; index < cgChPayloadChannels.Length; index++)
                    //        {
                    //            chList.Add(cgChPayloadChannels[index].ToString());
                    //        }
                    //        ack.Payload.ChannelName = chList.ToArray();
                    //    }
                    //}

                    ack.Error = Convert.ToBoolean(removeCgDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(RemoveChannelGroupAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else if (typeof(T) == typeof(RemoveNamespaceAck))
            {
                #region "RemoveNamespaceAck"
                Dictionary<string, object> removeNsDicObj = ConvertToDictionaryObject(listObject[0]);

                RemoveNamespaceAck ack = null;

                int statusCode = 0;

                if (removeNsDicObj != null)
                {
                    ack = new RemoveNamespaceAck();

                    if (int.TryParse(removeNsDicObj["status"].ToString(), out statusCode))
                        ack.StatusCode = statusCode;

                    ack.StatusMessage = removeNsDicObj["message"].ToString();

                    ack.Service = removeNsDicObj["service"].ToString();

                    ack.Error = Convert.ToBoolean(removeNsDicObj["error"].ToString());
                }

                ret = (T)Convert.ChangeType(ack, typeof(RemoveNamespaceAck), CultureInfo.InvariantCulture);
                #endregion
            }
            else
            {
                ret = (T)(object)listObject;
            }

            return ret;
        }

        public Dictionary<string, object> DeserializeToDictionaryOfObject(string jsonString)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
        }

        public Dictionary<string, object> ConvertToDictionaryObject(object localContainer)
        {
            Dictionary<string, object> ret = null;

            if (localContainer != null)
            {
                if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JObject")
                {
                    ret = new Dictionary<string, object>();

                    IDictionary<string, JToken> jDictionary = localContainer as JObject;
                    if (jDictionary != null)
                    {
                        foreach (KeyValuePair<string, JToken> pair in jDictionary)
                        {
                            JToken token = pair.Value;
                            ret.Add(pair.Key, ConvertJTokenToObject(token));
                        }
                    }
                }
                else if (localContainer.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
                {
                    ret = new Dictionary<string, object>();
                    Dictionary<string, object> dictionary = localContainer as Dictionary<string, object>;
                    foreach (string key in dictionary.Keys)
                    {
                        ret.Add(key, dictionary[key]);
                    }
                }
            }

            return ret;

        }

        public Dictionary<string, object>[] ConvertToDictionaryObjectArray(object localContainer)
        {
            Dictionary<string, object>[] ret = null;

            if (localContainer != null && localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JObject[]")
            {
                IDictionary<string, JToken>[] iDictionary = localContainer as IDictionary<string, JToken>[];
                if (iDictionary != null && iDictionary.Length > 0)
                {
                    ret = new Dictionary<string, object>[iDictionary.Length];

                    for (int index = 0; index < iDictionary.Length; index++)
                    {
                        IDictionary<string, JToken> iItem = iDictionary[index];
                        foreach (KeyValuePair<string, JToken> pair in iItem)
                        {
                            JToken token = pair.Value;
                            ret[index].Add(pair.Key, ConvertJTokenToObject(token));
                        }
                    }
                }
            }

            return ret;
        }

        public object[] ConvertToObjectArray(object localContainer)
        {
            object[] ret = null;

            if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                JArray jarrayResult = localContainer as JArray;
                List<object> objectContainer = jarrayResult.ToObject<List<object>>();
                if (objectContainer != null && objectContainer.Count > 0)
                {
                    for (int index = 0; index < objectContainer.Count; index++)
                    {
                        if (objectContainer[index].GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                        {
                            JArray internalItem = objectContainer[index] as JArray;
                            objectContainer[index] = internalItem.Select(item => (object)item).ToArray();
                        }
                    }
                    ret = objectContainer.ToArray<object>();
                }
            }
            else if (localContainer.GetType().ToString() == "System.Collections.Generic.List`1[System.Object]")
            {
                List<object> listResult = localContainer as List<object>;
                ret = listResult.ToArray<object>();
            }

            return ret;
        }

        private static object ConvertJTokenToObject(JToken token)
        {
            if (token == null)
            {
                return null;
            }

            var jValue = token as JValue;
            if (jValue != null)
            {
                return jValue.Value;
            }

            var jContainer = token as JArray;
            if (jContainer != null)
            {
                List<object> jsonList = new List<object>();
                foreach (JToken arrayItem in jContainer)
                {
                    jsonList.Add(ConvertJTokenToObject(arrayItem));
                }
                return jsonList;
            }

            IDictionary<string, JToken> jsonObject = token as JObject;
            if (jsonObject != null)
            {
                var jsonDict = new Dictionary<string, object>();
                List<JProperty> propertyList = (from childToken in token
                                                where childToken is JProperty
                                                select childToken as JProperty).ToList();
                foreach (JProperty property in propertyList)
                {
                    jsonDict.Add(property.Name, ConvertJTokenToObject(property.Value));
                }

                //(from childToken in token 
                //    where childToken is JProperty select childToken as JProperty)
                //    .ToList()
                //    .ForEach(property => jsonDict.Add(property.Name, ConvertJTokenToDictionary(property.Value)));
                return jsonDict;
            }

            return null;
        }

        #endregion

    }

}
