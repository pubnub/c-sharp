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
        private PNConfiguration config = null;
        private IPubnubLog pubnubLog = null;

        public NewtonsoftJsonDotNet(PNConfiguration pubnubConfig, IPubnubLog log)
        {
            this.config = pubnubConfig;
            this.pubnubLog = log;
        }

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

        public object BuildJsonObject(string jsonString)
        {
            object ret = null;

            try
            {
                var token = JToken.Parse(jsonString);
                ret = token;
            }
            catch { }

            return ret;
        }

        public bool IsArrayCompatible(string jsonString)
        {
            bool ret = false;
            if (IsValidJson(jsonString))
            {
                try
                {
                    using (StringReader strReader = new StringReader(jsonString))
                    {
                        using (JsonTextReader jsonTxtreader = new JsonTextReader(strReader))
                        {
                            while (jsonTxtreader.Read())
                            {
                                if (jsonTxtreader.LineNumber == 1 && jsonTxtreader.LinePosition == 1 && jsonTxtreader.TokenType == JsonToken.StartArray)
                                {
                                    ret = true;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            jsonTxtreader.Close();
                        }
                    }
                }
                catch { }
            }
            return ret;
        }

        public bool IsDictionaryCompatible(string jsonString)
        {
            bool ret = false;
            if (IsValidJson(jsonString))
            {
                try
                {
                    using (StringReader strReader = new StringReader(jsonString))
                    {
                        using (JsonTextReader jsonTxtreader = new JsonTextReader(strReader))
                        {
                            while (jsonTxtreader.Read())
                            {
                                if (jsonTxtreader.LineNumber == 1 && jsonTxtreader.LinePosition == 1 && jsonTxtreader.TokenType == JsonToken.StartObject)
                                {
                                    ret = true;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            jsonTxtreader.Close();
                        }
#if (NET35 || NET40 || NET45 || NET461)
                        strReader.Close();
#endif
                    }
                }
                catch { }
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

        private bool IsGenericTypeForMessage<T>()
        {
            bool ret = false;
            PNPlatform.Print(config, pubnubLog);

#if (NET35 || NET40 || NET45 || NET461)
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(PNMessageResult<>))
            {
                ret = true;
            }
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, NET35/40 IsGenericTypeForMessage = {1}", DateTime.Now.ToString(), ret.ToString()), config.LogVerbosity);
#elif (PORTABLE111 || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12 || NETSTANDARD13 || NETSTANDARD14 || UAP || NETFX_CORE || WINDOWS_UWP)
            if (typeof(T).GetTypeInfo().IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(PNMessageResult<>))
            {
                ret = true;
            }
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, typeof(T).GetTypeInfo().IsGenericType = {1}", DateTime.Now.ToString(), typeof(T).GetTypeInfo().IsGenericType.ToString()), config.LogVerbosity);
            if (typeof(T).GetTypeInfo().IsGenericType)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, typeof(T).GetGenericTypeDefinition() = {1}", DateTime.Now.ToString(), typeof(T).GetGenericTypeDefinition().ToString()), config.LogVerbosity);
            }
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, PCL/CORE IsGenericTypeForMessage = {1}", DateTime.Now.ToString(), ret.ToString()), config.LogVerbosity);
#endif
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, IsGenericTypeForMessage = {1}", DateTime.Now.ToString(), ret.ToString()), config.LogVerbosity);
            return ret;
        }

        private T DeserializeMessageToObjectBasedOnPlatform<T>(List<object> listObject)
        {
            T ret = default(T);

#if NET35 || NET40 || NET45 || NET461
            Type dataType = typeof(T).GetGenericArguments()[0];
            Type generic = typeof(PNMessageResult<>);
            Type specific = generic.MakeGenericType(dataType);

            ConstructorInfo ci = specific.GetConstructors().FirstOrDefault();
            if (ci != null)
            {
                object message = ci.Invoke(new object[] { });

                //Set data
                PropertyInfo dataProp = specific.GetProperty("Message");

                object userMessage = null;
                if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JValue))
                {
                    JValue jsonValue = listObject[0] as JValue;
                    userMessage = jsonValue.Value;
                    userMessage = ConvertToDataType(dataType, userMessage);

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

                    dataProp.SetValue(message, userMessage, null);
                }
                else if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                {
                    JToken token = listObject[0] as JToken;
                    userMessage = token.ToObject(dataProp.PropertyType, JsonSerializer.Create());

                    dataProp.SetValue(message, userMessage, null);
                }
                else if (listObject[0].GetType() == typeof(System.String))
                {
                    userMessage = listObject[0] as string;
                    dataProp.SetValue(message, userMessage, null);
                }

                //Set Time
                PropertyInfo timeProp = specific.GetProperty("Timetoken");
                long timetoken;
                Int64.TryParse(listObject[2].ToString(), out timetoken);
                timeProp.SetValue(message, timetoken, null);

                // Set ChannelName
                PropertyInfo channelNameProp = specific.GetProperty("Channel");
                channelNameProp.SetValue(message, (listObject.Count == 5) ? listObject[4].ToString() : listObject[3].ToString(), null);

                // Set ChannelGroup
                if (listObject.Count == 5)
                {
                    PropertyInfo subsciptionProp = specific.GetProperty("Subscription");
                    subsciptionProp.SetValue(message, listObject[3].ToString(), null);
                }
                
                //Set Metadata list second position, index=1
                if (listObject[1] != null)
                {
                    PropertyInfo userMetadataProp = specific.GetProperty("UserMetadata");
                    userMetadataProp.SetValue(message, listObject[1], null);
                }

                ret = (T)Convert.ChangeType(message, specific, CultureInfo.InvariantCulture);
            }
#elif PORTABLE111 || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12 || NETSTANDARD13 || NETSTANDARD14 || UAP || NETFX_CORE || WINDOWS_UWP
            Type dataType = typeof(T).GetTypeInfo().GenericTypeArguments[0];
            Type generic = typeof(PNMessageResult<>);
            Type specific = generic.MakeGenericType(dataType);

            ConstructorInfo ci = specific.GetTypeInfo().DeclaredConstructors.FirstOrDefault();
            if (ci != null)
            {
                object message = ci.Invoke(new object[] { });

                //Set data
                PropertyInfo dataProp = specific.GetRuntimeProperty("Message");

                object userMessage = null;
                if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JValue))
                {
                    JValue jsonValue = listObject[0] as JValue;
                    userMessage = jsonValue.Value;
                    userMessage = ConvertToDataType(dataType, userMessage);

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

                    dataProp.SetValue(message, userMessage, null);
                }
                else if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                {
                    JToken token = listObject[0] as JToken;
                    userMessage = token.ToObject(dataProp.PropertyType, JsonSerializer.Create());

                    dataProp.SetValue(message, userMessage, null);
                }
                else if (listObject[0].GetType() == typeof(System.String))
                {
                    userMessage = listObject[0] as string;
                    dataProp.SetValue(message, userMessage, null);
                }

                //Set Time
                PropertyInfo timeProp = specific.GetRuntimeProperty("Timetoken");
                long timetoken;
                Int64.TryParse(listObject[2].ToString(), out timetoken);
                timeProp.SetValue(message, timetoken, null);

                // Set ChannelName
                PropertyInfo channelNameProp = specific.GetRuntimeProperty("Channel");
                channelNameProp.SetValue(message, (listObject.Count == 5) ? listObject[4].ToString() : listObject[3].ToString(), null);

                // Set ChannelGroup
                if (listObject.Count == 5)
                {
                    PropertyInfo subsciptionProp = specific.GetRuntimeProperty("Subscription");
                    subsciptionProp.SetValue(message, listObject[3].ToString(), null);
                }
                
                //Set Metadata list second position, index=1
                if (listObject[1] != null)
                {
                    PropertyInfo userMetadataProp = specific.GetRuntimeProperty("UserMetadata");
                    userMetadataProp.SetValue(message, listObject[1], null);
                }

                ret = (T)Convert.ChangeType(message, specific, CultureInfo.InvariantCulture);
            }
#endif

            return ret;
        }

        public virtual T DeserializeToObject<T>(List<object> listObject)
        {
            T ret = default(T);

            if (listObject == null)
            {
                return ret;
            }

            if (IsGenericTypeForMessage<T>())
            {
#region "Subscribe Message<>"
                return DeserializeMessageToObjectBasedOnPlatform<T>(listObject);
#endregion
            }
            else if (typeof(T) == typeof(PNAccessManagerGrantResult))
            {
#region "PNAccessManagerGrantResult"
                Dictionary<string, object> grantDicObj = ConvertToDictionaryObject(listObject[0]);

                PNAccessManagerGrantResult ack = null;

                if (grantDicObj != null)
                {
                    ack = new PNAccessManagerGrantResult();

                    if (grantDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> grantAckPayloadDic = ConvertToDictionaryObject(grantDicObj["payload"]);
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
                                ack.Ttl = Convert.ToInt32(grantAckPayloadDic["ttl"].ToString());
                            }

                            if (!string.IsNullOrEmpty(ack.Level) && ack.Level == "subkey")
                            {
                                //ack.Payload.Access = new PNAccessManagerGrantResult.Data.SubkeyAccess();
                                //ack.Payload.Access.Read = grantAckPayloadDic["r"].ToString() == "1";
                                //ack.Payload.Access.Write = grantAckPayloadDic["w"].ToString() == "1";
                                //ack.Payload.Access.Manage = grantAckPayloadDic["m"].ToString() == "1";
                            }
                            else
                            {
                                if (grantAckPayloadDic.ContainsKey("channels"))
                                {
                                    ack.Channels = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();

                                    Dictionary<string, object> grantAckChannelListDic = ConvertToDictionaryObject(grantAckPayloadDic["channels"]);
                                    if (grantAckChannelListDic != null && grantAckChannelListDic.Count > 0)
                                    {
                                        foreach (string channel in grantAckChannelListDic.Keys)
                                        {
                                            Dictionary<string, object> grantAckChannelDataDic = ConvertToDictionaryObject(grantAckChannelListDic[channel]);
                                            if (grantAckChannelDataDic != null && grantAckChannelDataDic.Count > 0)
                                            {
                                                if (grantAckChannelDataDic.ContainsKey("auths"))
                                                {
                                                    Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                                    Dictionary<string, object> grantAckChannelAuthListDic = ConvertToDictionaryObject(grantAckChannelDataDic["auths"]);
                                                    if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                                    {
                                                        foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                                        {
                                                            Dictionary<string, object> grantAckChannelAuthDataDic = ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);

                                                            if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                            {
                                                                PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                                authData.ReadEnabled = grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                                authData.WriteEnabled = grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                                authData.ManageEnabled = grantAckChannelAuthDataDic.ContainsKey("m") ? grantAckChannelAuthDataDic["m"].ToString() == "1" : false;

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

                                        Dictionary<string, object> grantAckChannelAuthListDic = ConvertToDictionaryObject(grantAckPayloadDic["auths"]);

                                        if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                        {
                                            foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                            {
                                                Dictionary<string, object> grantAckChannelAuthDataDic = ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                                if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                {
                                                    PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                    authData.ReadEnabled = grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                    authData.WriteEnabled = grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                    authData.ManageEnabled = grantAckChannelAuthDataDic.ContainsKey("m") ? grantAckChannelAuthDataDic["m"].ToString() == "1" : false;

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

                                    Dictionary<string, object> grantAckCgListDic = ConvertToDictionaryObject(grantAckPayloadDic["channel-groups"]);
                                    if (grantAckCgListDic != null && grantAckCgListDic.Count > 0)
                                    {
                                        foreach (string channelgroup in grantAckCgListDic.Keys)
                                        {
                                            Dictionary<string, object> grantAckCgDataDic = ConvertToDictionaryObject(grantAckCgListDic[channelgroup]);
                                            if (grantAckCgDataDic != null && grantAckCgDataDic.Count > 0)
                                            {
                                                if (grantAckCgDataDic.ContainsKey("auths"))
                                                {
                                                    Dictionary<string, PNAccessManagerKeyData> authKeyDataDic = new Dictionary<string, PNAccessManagerKeyData>();

                                                    Dictionary<string, object> grantAckCgAuthListDic = ConvertToDictionaryObject(grantAckCgDataDic["auths"]);
                                                    if (grantAckCgAuthListDic != null && grantAckCgAuthListDic.Count > 0)
                                                    {
                                                        foreach (string authKey in grantAckCgAuthListDic.Keys)
                                                        {
                                                            Dictionary<string, object> grantAckCgAuthDataDic = ConvertToDictionaryObject(grantAckCgAuthListDic[authKey]);
                                                            if (grantAckCgAuthDataDic != null && grantAckCgAuthDataDic.Count > 0)
                                                            {
                                                                PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                                authData.ReadEnabled = grantAckCgAuthDataDic["r"].ToString() == "1";
                                                                authData.WriteEnabled = grantAckCgAuthDataDic["w"].ToString() == "1";
                                                                authData.ManageEnabled = grantAckCgAuthDataDic.ContainsKey("m") ? grantAckCgAuthDataDic["m"].ToString() == "1" : false;

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

                                            Dictionary<string, object> grantAckChannelAuthListDic = ConvertToDictionaryObject(grantAckPayloadDic["auths"]);

                                            if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                            {
                                                foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                                {
                                                    Dictionary<string, object> grantAckChannelAuthDataDic = ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                                    if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                    {
                                                        PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                        authData.ReadEnabled = grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                        authData.WriteEnabled = grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                        authData.ManageEnabled = grantAckChannelAuthDataDic.ContainsKey("m") ? grantAckChannelAuthDataDic["m"].ToString() == "1" : false;

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

                                        Dictionary<string, object> grantAckChannelAuthListDic = ConvertToDictionaryObject(grantAckPayloadDic["auths"]);

                                        if (grantAckChannelAuthListDic != null && grantAckChannelAuthListDic.Count > 0)
                                        {
                                            foreach (string authKey in grantAckChannelAuthListDic.Keys)
                                            {
                                                Dictionary<string, object> grantAckChannelAuthDataDic = ConvertToDictionaryObject(grantAckChannelAuthListDic[authKey]);
                                                if (grantAckChannelAuthDataDic != null && grantAckChannelAuthDataDic.Count > 0)
                                                {
                                                    PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                                    authData.ReadEnabled = grantAckChannelAuthDataDic["r"].ToString() == "1";
                                                    authData.WriteEnabled = grantAckChannelAuthDataDic["w"].ToString() == "1";
                                                    authData.ManageEnabled = grantAckChannelAuthDataDic.ContainsKey("m") ? grantAckChannelAuthDataDic["m"].ToString() == "1" : false;

                                                    authKeyDataDic.Add(authKey, authData);
                                                }

                                            }

                                            ack.ChannelGroups.Add(channelGroupName, authKeyDataDic);
                                        }
                                    }
                                } //end of if channel-group
                            } //end of else subkey

                        }

                    }
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNAccessManagerGrantResult), CultureInfo.InvariantCulture);
#endregion
            }
            else if (typeof(T) == typeof(PNAccessManagerAuditResult))
            {
#region "PNAccessManagerAuditResult"
                Dictionary<string, object> auditDicObj = ConvertToDictionaryObject(listObject[0]);

                PNAccessManagerAuditResult ack = null;

                if (auditDicObj != null)
                {
                    ack = new PNAccessManagerAuditResult();

                    if (auditDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> auditAckPayloadDic = ConvertToDictionaryObject(auditDicObj["payload"]);
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
                                Dictionary<string, object> auditAckAuthListDic = ConvertToDictionaryObject(auditAckPayloadDic["auths"]);
                                if (auditAckAuthListDic != null && auditAckAuthListDic.Count > 0)
                                {
                                    ack.AuthKeys = new Dictionary<string, PNAccessManagerKeyData>();

                                    foreach (string authKey in auditAckAuthListDic.Keys)
                                    {
                                        Dictionary<string, object> authDataDic = ConvertToDictionaryObject(auditAckAuthListDic[authKey]);
                                        if (authDataDic != null && authDataDic.Count > 0)
                                        {
                                            PNAccessManagerKeyData authData = new PNAccessManagerKeyData();
                                            authData.ReadEnabled = authDataDic["r"].ToString() == "1";
                                            authData.WriteEnabled = authDataDic["w"].ToString() == "1";
                                            authData.ManageEnabled = authDataDic.ContainsKey("m") ? authDataDic["m"].ToString() == "1" : false;

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
                    result = new PNPublishResult
                    {
                        Timetoken = Int64.Parse(listObject[2].ToString()),
                    };
                }

                ret = (T)Convert.ChangeType(result, typeof(PNPublishResult), CultureInfo.InvariantCulture);
#endregion
            }
            else if (typeof(T) == typeof(PNPresenceEventResult))
            {
#region "PNPresenceEventResult"
                Dictionary<string, object> presenceDicObj = ConvertToDictionaryObject(listObject[0]);

                PNPresenceEventResult ack = null;

                if (presenceDicObj != null)
                {
                    ack = new PNPresenceEventResult();
                    ack.Event = presenceDicObj["action"].ToString();
                    ack.Timestamp = Convert.ToInt64(presenceDicObj["timestamp"].ToString());
                    if (presenceDicObj.ContainsKey("uuid"))
                    {
                        ack.Uuid = presenceDicObj["uuid"].ToString();
                    }
                    ack.Occupancy = Int32.Parse(presenceDicObj["occupancy"].ToString());

                    if (presenceDicObj.ContainsKey("data"))
                    {
                        Dictionary<string, object> stateDic = presenceDicObj["data"] as Dictionary<string, object>;
                        if (stateDic != null)
                        {
                            ack.State = stateDic;
                        }
                    }

                    ack.Timetoken = Convert.ToInt64(listObject[2].ToString());
                    //ack.ChannelGroupName = (listObject.Count == 4) ? listObject[2].ToString() : "";
                    ack.Channel = (listObject.Count == 5) ? listObject[4].ToString() : listObject[3].ToString();
                    ack.Channel = ack.Channel.Replace("-pnpres", "");

                    if (listObject.Count == 5)
                    {
                        ack.Subscription = listObject[3].ToString();
                        ack.Subscription = ack.Subscription.Replace("-pnpres", "");
                    }

                    if (listObject[1] != null)
                    {
                        ack.UserMetadata = listObject[1];
                    }

                    if (ack.Event != null && ack.Event.ToLower() == "interval")
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
                PNHistoryResult ack = new PNHistoryResult();
                ack.StartTimeToken = Convert.ToInt64(listObject[1].ToString());
                ack.EndTimeToken = Convert.ToInt64(listObject[2].ToString());
                //ack.ChannelName = listObject[3].ToString();
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
                    foreach(var message in messagesContainer)
                    {
                        PNHistoryItemResult result = new PNHistoryItemResult();
                        Dictionary<string, object> dicMessageTimetoken = ConvertToDictionaryObject(message);
                        if (dicMessageTimetoken != null)
                        {
                            if (dicMessageTimetoken.ContainsKey("message") && dicMessageTimetoken.ContainsKey("timetoken"))
                            {
                                result.Entry = dicMessageTimetoken["message"];

                                long messageTimetoken;
                                Int64.TryParse(dicMessageTimetoken["timetoken"].ToString(), out messageTimetoken);
                                result.Timetoken = messageTimetoken;
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

                ret = (T)Convert.ChangeType(ack, typeof(PNHistoryResult), CultureInfo.InvariantCulture);
#endregion
            }
            else if (typeof(T) == typeof(PNDeleteMessageResult))
            {
#region "PNDeleteMessageResult"
                PNDeleteMessageResult ack = new PNDeleteMessageResult();
                ret = (T)Convert.ChangeType(ack, typeof(PNDeleteMessageResult), CultureInfo.InvariantCulture);
#endregion
            }
            else if (typeof(T) == typeof(PNHereNowResult))
            {
#region "PNHereNowResult"
                Dictionary<string, object> herenowDicObj = ConvertToDictionaryObject(listObject[0]);

                PNHereNowResult hereNowResult = null;

                if (herenowDicObj != null)
                {
                    hereNowResult = new PNHereNowResult();

                    string hereNowChannelName = listObject[1].ToString();

                    if (herenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> hereNowPayloadDic = ConvertToDictionaryObject(herenowDicObj["payload"]);
                        if (hereNowPayloadDic != null && hereNowPayloadDic.Count > 0)
                        {
                            hereNowResult.TotalOccupancy = Int32.Parse(hereNowPayloadDic["total_occupancy"].ToString());
                            hereNowResult.TotalChannels = Int32.Parse(hereNowPayloadDic["total_channels"].ToString());
                            if (hereNowPayloadDic.ContainsKey("channels"))
                            {
                                Dictionary<string, PNHereNowChannelData> hereNowChannelData = new Dictionary<string, PNHereNowChannelData>();

                                Dictionary<string, object> hereNowChannelListDic = ConvertToDictionaryObject(hereNowPayloadDic["channels"]);
                                if (hereNowChannelListDic != null && hereNowChannelListDic.Count > 0)
                                {
                                    foreach (string channel in hereNowChannelListDic.Keys)
                                    {
                                        Dictionary<string, object> hereNowChannelItemDic = ConvertToDictionaryObject(hereNowChannelListDic[channel]);
                                        if (hereNowChannelItemDic != null && hereNowChannelItemDic.Count > 0)
                                        {
                                            PNHereNowChannelData channelData = new PNHereNowChannelData();
                                            channelData.ChannelName = channel;
                                            channelData.Occupancy = Convert.ToInt32(hereNowChannelItemDic["occupancy"].ToString());
                                            if (hereNowChannelItemDic.ContainsKey("uuids"))
                                            {
                                                object[] hereNowChannelUuidList = ConvertToObjectArray(hereNowChannelItemDic["uuids"]);
                                                if (hereNowChannelUuidList != null && hereNowChannelUuidList.Length > 0)
                                                {
                                                    List<PNHereNowOccupantData> uuidDataList = new List<PNHereNowOccupantData>();

                                                    for (int index = 0; index < hereNowChannelUuidList.Length; index++)
                                                    {
                                                        if (hereNowChannelUuidList[index].GetType() == typeof(string))
                                                        {
                                                            PNHereNowOccupantData uuidData = new PNHereNowOccupantData();
                                                            uuidData.Uuid = hereNowChannelUuidList[index].ToString();
                                                            uuidDataList.Add(uuidData);
                                                        }
                                                        else
                                                        {
                                                            Dictionary<string, object> hereNowChannelItemUuidsDic = ConvertToDictionaryObject(hereNowChannelUuidList[index]);
                                                            if (hereNowChannelItemUuidsDic != null && hereNowChannelItemUuidsDic.Count > 0)
                                                            {
                                                                PNHereNowOccupantData uuidData = new PNHereNowOccupantData();
                                                                uuidData.Uuid = hereNowChannelItemUuidsDic["uuid"].ToString();
                                                                if (hereNowChannelItemUuidsDic.ContainsKey("state"))
                                                                {
                                                                    uuidData.State = ConvertToDictionaryObject(hereNowChannelItemUuidsDic["state"]);
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
                        hereNowResult.TotalOccupancy = Int32.Parse(herenowDicObj["occupancy"].ToString());
                        hereNowResult.Channels = new Dictionary<string, PNHereNowChannelData>();
                        if (herenowDicObj.ContainsKey("uuids"))
                        {
                            object[] uuidArray = ConvertToObjectArray(herenowDicObj["uuids"]);
                            if (uuidArray != null && uuidArray.Length > 0)
                            {
                                List<PNHereNowOccupantData> uuidDataList = new List<PNHereNowOccupantData>();
                                for (int index = 0; index < uuidArray.Length; index++)
                                {
                                    Dictionary<string, object> hereNowChannelItemUuidsDic = ConvertToDictionaryObject(uuidArray[index]);
                                    if (hereNowChannelItemUuidsDic != null && hereNowChannelItemUuidsDic.Count > 0)
                                    {
                                        PNHereNowOccupantData uuidData = new PNHereNowOccupantData();
                                        uuidData.Uuid = hereNowChannelItemUuidsDic["uuid"].ToString();
                                        if (hereNowChannelItemUuidsDic.ContainsKey("state"))
                                        {
                                            uuidData.State = ConvertToDictionaryObject(hereNowChannelItemUuidsDic["state"]);
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
                Dictionary<string, object> wherenowDicObj = ConvertToDictionaryObject(listObject[0]);

                PNWhereNowResult ack = null;

                if (wherenowDicObj != null)
                {
                    ack = new PNWhereNowResult();

                    if (wherenowDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> whereNowPayloadDic = ConvertToDictionaryObject(wherenowDicObj["payload"]);
                        if (whereNowPayloadDic != null && whereNowPayloadDic.Count > 0)
                        {
                            if (whereNowPayloadDic.ContainsKey("channels"))
                            {
                                object[] whereNowChannelList = ConvertToObjectArray(whereNowPayloadDic["channels"]);
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
                Dictionary<string, object> setUserStatewDicObj = ConvertToDictionaryObject(listObject[0]);

                PNSetStateResult ack = null;

                if (setUserStatewDicObj != null)
                {
                    ack = new PNSetStateResult();

                    //if (listObject != null && listObject.Count >= 2 && listObject[1] != null && !string.IsNullOrEmpty(listObject[1].ToString()))
                    //{
                    //    ack.ChannelGroupName = listObject[1].ToString().Split(',');
                    //}

                    //if (listObject != null && listObject.Count >= 3 && listObject[2] != null && !string.IsNullOrEmpty(listObject[2].ToString()))
                    //{
                    //    ack.ChannelName = listObject[2].ToString().Split(',');
                    //}

                    ack.State = new Dictionary<string, object>();

                    if (setUserStatewDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> setStateDic = ConvertToDictionaryObject(setUserStatewDicObj["payload"]);
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
                Dictionary<string, object> getUserStatewDicObj = ConvertToDictionaryObject(listObject[0]);

                PNGetStateResult ack = null;

                if (getUserStatewDicObj != null)
                {
                    ack = new PNGetStateResult();

                    //if (listObject != null && listObject.Count >= 2 && listObject[1] != null && !string.IsNullOrEmpty(listObject[1].ToString()))
                    //{
                    //    ack.ChannelGroupName = listObject[1].ToString().Split(',');
                    //}
                    //if (listObject != null && listObject.Count >= 3 && listObject[2] != null && !string.IsNullOrEmpty(listObject[2].ToString()))
                    //{
                    //    ack.ChannelName = listObject[2].ToString().Split(',');
                    //}

                    ack.StateByUUID = new Dictionary<string, object>();

                    if (getUserStatewDicObj.ContainsKey("payload"))
                    {
                        Dictionary<string, object> getStateDic = ConvertToDictionaryObject(getUserStatewDicObj["payload"]);
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
                Dictionary<string, object> getCgChannelsDicObj = ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsAllChannelsResult ack = null;

                if (getCgChannelsDicObj != null)
                {
                    ack = new PNChannelGroupsAllChannelsResult();
                    Dictionary<string, object> getCgChannelPayloadDic = ConvertToDictionaryObject(getCgChannelsDicObj["payload"]);
                    if (getCgChannelPayloadDic != null && getCgChannelPayloadDic.Count > 0)
                    {
                        ack.ChannelGroup = getCgChannelPayloadDic["group"].ToString();
                        object[] channelGroupChPayloadChannels = ConvertToObjectArray(getCgChannelPayloadDic["channels"]);
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

                ret = (T)Convert.ChangeType(ack, typeof(PNChannelGroupsAllChannelsResult), CultureInfo.InvariantCulture);
#endregion
            }
            else if (typeof(T) == typeof(PNChannelGroupsListAllResult))
            {
#region "PNChannelGroupsListAllResult"
                Dictionary<string, object> getAllCgDicObj = ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsListAllResult ack = null;

                if (getAllCgDicObj != null)
                {
                    ack = new PNChannelGroupsListAllResult();

                    Dictionary<string, object> getAllCgPayloadDic = ConvertToDictionaryObject(getAllCgDicObj["payload"]);
                    if (getAllCgPayloadDic != null && getAllCgPayloadDic.Count > 0)
                    {
                        object[] channelGroupAllCgPayloadChannels = ConvertToObjectArray(getAllCgPayloadDic["groups"]);
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
                Dictionary<string, object> addChToCgDicObj = ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsAddChannelResult ack = null;

                //int statusCode = 0;

                if (addChToCgDicObj != null)
                {
                    ack = new PNChannelGroupsAddChannelResult();

                    //if (int.TryParse(addChToCgDicObj["status"].ToString(), out statusCode))
                    //    ack.Status = statusCode;

                    //ack.Message = addChToCgDicObj["message"].ToString();
                    //ack.Service = addChToCgDicObj["service"].ToString();

                    //ack.Error = Convert.ToBoolean(addChToCgDicObj["error"].ToString());

                    //ack.ChannelGroup = listObject[1].ToString();
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNChannelGroupsAddChannelResult), CultureInfo.InvariantCulture);
#endregion
            }
            else if (typeof(T) == typeof(PNChannelGroupsRemoveChannelResult))
            {
#region "PNChannelGroupsRemoveChannelResult"
                Dictionary<string, object> removeChFromCgDicObj = ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsRemoveChannelResult ack = null;

                int statusCode = 0;

                if (removeChFromCgDicObj != null)
                {
                    ack = new PNChannelGroupsRemoveChannelResult();

                    if (int.TryParse(removeChFromCgDicObj["status"].ToString(), out statusCode))
                        ack.Status = statusCode;

                    ack.Message = removeChFromCgDicObj["message"].ToString();
                    ack.Service = removeChFromCgDicObj["service"].ToString();

                    ack.Error = Convert.ToBoolean(removeChFromCgDicObj["error"].ToString());

                    ack.ChannelGroup = listObject[1].ToString();
                }

                ret = (T)Convert.ChangeType(ack, typeof(PNChannelGroupsRemoveChannelResult), CultureInfo.InvariantCulture);
#endregion
            }
            else if (typeof(T) == typeof(PNChannelGroupsDeleteGroupResult))
            {
#region "PNChannelGroupsDeleteGroupResult"
                Dictionary<string, object> removeCgDicObj = ConvertToDictionaryObject(listObject[0]);

                PNChannelGroupsDeleteGroupResult ack = null;

                int statusCode = 0;

                if (removeCgDicObj != null)
                {
                    ack = new PNChannelGroupsDeleteGroupResult();

                    if (int.TryParse(removeCgDicObj["status"].ToString(), out statusCode))
                        ack.Status = statusCode;

                    ack.Service = removeCgDicObj["service"].ToString();
                    ack.Message = removeCgDicObj["message"].ToString();

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

                ret = (T)Convert.ChangeType(ack, typeof(PNChannelGroupsDeleteGroupResult), CultureInfo.InvariantCulture);
#endregion
            }
            else if (typeof(T) == typeof(PNTimeResult))
            {
#region "PNTimeResult"

                Int64 timetoken = 0;

                Int64.TryParse(listObject[0].ToString(), out timetoken);

                PNTimeResult result = new PNTimeResult()
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

                ret = (T)Convert.ChangeType(result, typeof(PNPushRemoveAllChannelsResult), CultureInfo.InvariantCulture);
#endregion
            }
            else if (typeof(T) == typeof(PNHeartbeatResult))
            {
#region "PNHeartbeatResult"
                Dictionary<string, object> heartbeatDicObj = ConvertToDictionaryObject(listObject[0]);
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
            else
            {
                System.Diagnostics.Debug.WriteLine("DeserializeToObject<T>(list) => NO MATCH");
                try
                {
                    ret = (T)(object)listObject;
                }
                catch { }
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

                    IDictionary<string, JToken> jsonDictionary = localContainer as JObject;
                    if (jsonDictionary != null)
                    {
                        foreach (KeyValuePair<string, JToken> pair in jsonDictionary)
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
                else if (localContainer.GetType().ToString() == "Newtonsoft.Json.Linq.JProperty")
                {
                    ret = new Dictionary<string, object>();

                    JProperty jsonProp = localContainer as JProperty;
                    if (jsonProp != null)
                    {
                        string propName = jsonProp.Name;
                        ret.Add(propName, ConvertJTokenToObject(jsonProp.Value));
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
                IDictionary<string, JToken>[] jsonDictionary = localContainer as IDictionary<string, JToken>[];
                if (jsonDictionary != null && jsonDictionary.Length > 0)
                {
                    ret = new Dictionary<string, object>[jsonDictionary.Length];

                    for (int index = 0; index < jsonDictionary.Length; index++)
                    {
                        IDictionary<string, JToken> jsonItem = jsonDictionary[index];
                        foreach (KeyValuePair<string, JToken> pair in jsonItem)
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

            var jsonValue = token as JValue;
            if (jsonValue != null)
            {
                return jsonValue.Value;
            }

            var jsonContainer = token as JArray;
            if (jsonContainer != null)
            {
                List<object> jsonList = new List<object>();
                foreach (JToken arrayItem in jsonContainer)
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

        private static object ConvertToDataType(Type dataType, object inputValue)
        {
            if (dataType == inputValue.GetType())
            {
                return inputValue;
            }

            object userMessage = inputValue;
            switch (dataType.FullName)
            {
                case "System.Int32":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.Int32), CultureInfo.InvariantCulture);
                    break;
                case "System.Int16":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.Int16), CultureInfo.InvariantCulture);
                    break;
                case "System.UInt64":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.UInt64), CultureInfo.InvariantCulture);
                    break;
                case "System.UInt32":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.UInt32), CultureInfo.InvariantCulture);
                    break;
                case "System.UInt16":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.UInt16), CultureInfo.InvariantCulture);
                    break;
                case "System.Byte":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.Byte), CultureInfo.InvariantCulture);
                    break;
                case "System.SByte":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.SByte), CultureInfo.InvariantCulture);
                    break;
                case "System.Decimal":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.Decimal), CultureInfo.InvariantCulture);
                    break;
                case "System.Boolean":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.Boolean), CultureInfo.InvariantCulture);
                    break;
                case "System.Double":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.Double), CultureInfo.InvariantCulture);
                    break;
                case "System.Char":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.Char), CultureInfo.InvariantCulture);
                    break;
                case "System.String":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.String), CultureInfo.InvariantCulture);
                    break;
                case "System.Object":
                    userMessage = Convert.ChangeType(inputValue, typeof(System.Object), CultureInfo.InvariantCulture);
                    break;
                default:
                    break;
            }

            return userMessage;
        }

#endregion

    }

}
