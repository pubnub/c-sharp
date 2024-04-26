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
        private readonly PNConfiguration config;
        private readonly IPubnubLog pubnubLog;
        private readonly JsonSerializerSettings defaultJsonSerializerSettings;

        #region "IL2CPP workarounds"

        //Got an exception when using JSON serialisation for [],
        //IL2CPP needs to know about the array type at compile time.
        //So please define private static filed like this:
#pragma warning disable
        private static readonly System.String[][] _unused;
        private static readonly System.Int32[][] _unused2;
        private static readonly System.Int64[][] _unused3;
        private static readonly System.Int16[][] _unused4;
        private static readonly System.UInt16[][] _unused5;
        private static readonly System.UInt64[][] _unused6;
        private static readonly System.UInt32[][] _unused7;
        private static readonly System.Decimal[][] _unused8;
        private static readonly System.Double[][] _unused9;
        private static readonly System.Boolean[][] _unused91;
        private static readonly System.Object[][] _unused92;

        private static readonly long[][] _unused10;
        private static readonly int[][] _unused11;
        private static readonly float[][] _unused12;
        private static readonly decimal[][] _unused13;
        private static readonly uint[][] _unused14;
        private static readonly ulong[][] _unused15;
#pragma warning restore

        #endregion

        public NewtonsoftJsonDotNet(PNConfiguration pubnubConfig, IPubnubLog log)
        {
            this.config = pubnubConfig;
            this.pubnubLog = log;
            defaultJsonSerializerSettings = new JsonSerializerSettings { MaxDepth = 64 };
        }

        #region IJsonPlugableLibrary methods implementation

        private static bool IsValidJson(string jsonString, PNOperationType operationType)
        {
            bool ret = false;
            try
            {
                if (operationType == PNOperationType.PNPublishOperation
                    || operationType == PNOperationType.PNHistoryOperation
                    || operationType == PNOperationType.PNTimeOperation
                    || operationType == PNOperationType.PNPublishFileMessageOperation)
                {
                    JArray.Parse(jsonString);
                }
                else
                {
                    JObject.Parse(jsonString);
                }

                ret = true;
            }
            catch
            {
                try
                {
                    if (operationType == PNOperationType.PNPublishOperation
                        || operationType == PNOperationType.PNHistoryOperation
                        || operationType == PNOperationType.PNTimeOperation
                        || operationType == PNOperationType.PNPublishFileMessageOperation)
                    {
                        JObject.Parse(jsonString);
                        ret = true;
                    }
                }
                catch
                {
                    /* igonore */
                }
            }

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
            catch
            {
                /* ignore */
            }

            return ret;
        }

        public bool IsDictionaryCompatible(string jsonString, PNOperationType operationType)
        {
            bool ret = false;
            if (JsonFastCheck(jsonString) && IsValidJson(jsonString, operationType))
            {
                try
                {
                    using (StringReader strReader = new StringReader(jsonString))
                    {
                        using (JsonTextReader jsonTxtreader = new JsonTextReader(strReader))
                        {
                            while (jsonTxtreader.Read())
                            {
                                if (jsonTxtreader.LineNumber == 1 && jsonTxtreader.LinePosition == 1 &&
                                    jsonTxtreader.TokenType == JsonToken.StartObject)
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
#if (NET35 || NET40 || NET45 || NET461 || NET48)
                        strReader.Close();
#endif
                    }
                }
                catch
                {
                    /* ignore */
                }
            }

            return ret;
        }

        public string SerializeToJsonString(object objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize, defaultJsonSerializerSettings);
        }

        public List<object> DeserializeToListOfObject(string jsonString)
        {
            List<object> result =
                JsonConvert.DeserializeObject<List<object>>(jsonString, defaultJsonSerializerSettings);

            return result;
        }

        public object DeserializeToObject(object rawObject, Type type)
        {
            try
            {
                if (rawObject is JObject jObject)
                {
                    return jObject.ToObject(type);
                }
                else
                {
                    return rawObject;
                }
            }
            catch (Exception e)
            {
                pubnubLog.WriteToLog(e.ToString());
                return rawObject;
            }
        }

        public object DeserializeToObject(string jsonString)
        {
            object result = JsonConvert.DeserializeObject<object>(jsonString,
                new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, MaxDepth = 64 });
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
            JsonConvert.PopulateObject(value, target, defaultJsonSerializerSettings);
        }

        public virtual T DeserializeToObject<T>(string jsonString)
        {
            T ret = default(T);

            try
            {
                ret = JsonConvert.DeserializeObject<T>(jsonString,
                    new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, MaxDepth = 64 });
            }
            catch
            {
                /* ignore */
            }

            return ret;
        }

        private bool IsGenericTypeForMessage<T>()
        {
            bool ret = false;
            PNPlatform.Print(config, pubnubLog);

#if (NET35 || NET40 || NET45 || NET461 || NET48)
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(PNMessageResult<>))
            {
                ret = true;
            }
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, NET35/40 IsGenericTypeForMessage = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ret.ToString()), config.LogVerbosity);
#elif (NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12 || NETSTANDARD13 || NETSTANDARD14 || NETSTANDARD20 || NET60 || UAP || NETFX_CORE || WINDOWS_UWP)
            if (typeof(T).GetTypeInfo().IsGenericType &&
                typeof(T).GetGenericTypeDefinition() == typeof(PNMessageResult<>))
            {
                ret = true;
            }

            LoggingMethod.WriteToLog(pubnubLog,
                string.Format(CultureInfo.InvariantCulture,
                    "DateTime: {0}, typeof(T).GetTypeInfo().IsGenericType = {1}",
                    DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    typeof(T).GetTypeInfo().IsGenericType.ToString()), config.LogVerbosity);
            if (typeof(T).GetTypeInfo().IsGenericType)
            {
                LoggingMethod.WriteToLog(pubnubLog,
                    string.Format(CultureInfo.InvariantCulture,
                        "DateTime: {0}, typeof(T).GetGenericTypeDefinition() = {1}",
                        DateTime.Now.ToString(CultureInfo.InvariantCulture),
                        typeof(T).GetGenericTypeDefinition().ToString()), config.LogVerbosity);
            }

            LoggingMethod.WriteToLog(pubnubLog,
                string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, PCL/CORE IsGenericTypeForMessage = {1}",
                    DateTime.Now.ToString(CultureInfo.InvariantCulture), ret.ToString()), config.LogVerbosity);
#endif
            LoggingMethod.WriteToLog(pubnubLog,
                string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, IsGenericTypeForMessage = {1}",
                    DateTime.Now.ToString(CultureInfo.InvariantCulture), ret.ToString()), config.LogVerbosity);
            return ret;
        }

        private T DeserializeMessageToObjectBasedOnPlatform<T>(List<object> listObject)
        {
            T ret = default(T);

#if NET35 || NET40 || NET45 || NET461 || NET48
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
                else if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JObject) || listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                {
                    JToken token = listObject[0] as JToken;
                    if (dataProp.PropertyType == typeof(string))
                    {
                        userMessage = JsonConvert.SerializeObject(token, defaultJsonSerializerSettings);
                    }
                    else
                    {
                        userMessage = token.ToObject(dataProp.PropertyType, JsonSerializer.Create());
                    }

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
                var _ = Int64.TryParse(listObject[2].ToString(), out timetoken);
                timeProp.SetValue(message, timetoken, null);

                //Set Publisher
                PropertyInfo publisherProp = specific.GetProperty("Publisher");
                string publisherValue = (listObject[3] != null) ? listObject[3].ToString() : "";
                publisherProp.SetValue(message, publisherValue, null);

                // Set ChannelName
                PropertyInfo channelNameProp = specific.GetProperty("Channel");
                channelNameProp.SetValue(message, (listObject.Count == 6) ? listObject[5].ToString() : listObject[4].ToString(), null);

                // Set ChannelGroup
                if (listObject.Count == 6)
                {
                    PropertyInfo subsciptionProp = specific.GetProperty("Subscription");
                    subsciptionProp.SetValue(message, listObject[4].ToString(), null);
                }
                
                //Set Metadata list second position, index=1
                if (listObject[1] != null)
                {
                    PropertyInfo userMetadataProp = specific.GetProperty("UserMetadata");
                    userMetadataProp.SetValue(message, listObject[1], null);
                }

                ret = (T)Convert.ChangeType(message, specific, CultureInfo.InvariantCulture);
            }
#elif NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12 || NETSTANDARD13 || NETSTANDARD14 || NETSTANDARD20 || NET60 || UAP || NETFX_CORE || WINDOWS_UWP
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
                else if (listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JObject) ||
                         listObject[0].GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                {
                    JToken token = listObject[0] as JToken;
                    if (dataProp.PropertyType == typeof(string))
                    {
                        userMessage = JsonConvert.SerializeObject(token, defaultJsonSerializerSettings);
                    }
                    else
                    {
                        userMessage = token.ToObject(dataProp.PropertyType, JsonSerializer.Create());
                    }

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

                //Set Publisher
                PropertyInfo publisherProp = specific.GetRuntimeProperty("Publisher");
                string publisherValue = (listObject[3] != null) ? listObject[3].ToString() : "";
                publisherProp.SetValue(message, publisherValue, null);

                // Set ChannelName
                PropertyInfo channelNameProp = specific.GetRuntimeProperty("Channel");
                channelNameProp.SetValue(message,
                    (listObject.Count == 6) ? listObject[5].ToString() : listObject[4].ToString(), null);

                // Set ChannelGroup
                if (listObject.Count == 6)
                {
                    PropertyInfo subsciptionProp = specific.GetRuntimeProperty("Subscription");
                    subsciptionProp.SetValue(message, listObject[4].ToString(), null);
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
            else
            {
                return DeserializeToInternalObjectUtility.DeserializeToInternalObject<T>(this, listObject);
            }
        }

        public Dictionary<string, object> DeserializeToDictionaryOfObject(string jsonString)
        {
            Dictionary<string, object> result = null;
            try
            {
                if (JsonFastCheck(jsonString))
                {
                    result = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString,
                        defaultJsonSerializerSettings);
                }
            }
            catch
            {
                //ignore
            }

            return result;
        }

        public Dictionary<string, object> ConvertToDictionaryObject(object localContainer)
        {
            Dictionary<string, object> ret = null;

            try
            {
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
                    else if (localContainer.GetType().ToString() ==
                             "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
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
                    else if (localContainer.GetType().ToString() == "System.Collections.Generic.List`1[System.Object]")
                    {
                        List<object> localList = localContainer as List<object>;
                        if (localList != null && localList.Count > 0)
                        {
                            if (localList[0].GetType() == typeof(KeyValuePair<string, object>))
                            {
                                ret = new Dictionary<string, object>();
                                foreach (object item in localList)
                                {
                                    if (item is KeyValuePair<string, object> kvpItem)
                                    {
                                        ret.Add(kvpItem.Key, kvpItem.Value);
                                    }
                                    else
                                    {
                                        ret = null;
                                        break;
                                    }
                                }
                            }
                            else if (localList[0].GetType() == typeof(Dictionary<string, object>))
                            {
                                ret = new Dictionary<string, object>();
                                foreach (object item in localList)
                                {
                                    if (item is Dictionary<string, object> dicItem)
                                    {
                                        if (dicItem.Count > 0 && dicItem.ContainsKey("key") &&
                                            dicItem.ContainsKey("value"))
                                        {
                                            ret.Add(dicItem["key"].ToString(), dicItem["value"]);
                                        }
                                    }
                                    else
                                    {
                                        ret = null;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                /* ignore */
            }

            return ret;
        }

        public object[] ConvertToObjectArray(object localContainer)
        {
            object[] ret = null;

            try
            {
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
            }
            catch { /* ignore */ }

            return ret;
        }

        public static bool JsonFastCheck(string rawJson)
        {
            var c = rawJson.TrimStart()[0];
            return c == '[' || c == '{';
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