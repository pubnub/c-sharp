using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text.Json;

namespace PubnubApi
{
    /// <summary>
    /// System.Text.Json implementation of IJsonPluggableLibrary
    /// This provides a lighter alternative to Newtonsoft.Json while maintaining backward compatibility
    /// </summary>
    public class SystemTextJsonDotNet : IJsonPluggableLibrary
    {
        private readonly PNConfiguration config;
        private readonly JsonSerializerOptions defaultJsonSerializerOptions;
        private readonly PubnubLogModule logger;

        public SystemTextJsonDotNet(PNConfiguration pubnubConfig)
        {
            this.config = pubnubConfig;
            defaultJsonSerializerOptions = new JsonSerializerOptions
            {
                MaxDepth = 64,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null, // Keep original property names (like Newtonsoft.Json default)
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                AllowTrailingCommas = true, // Allow trailing commas like Newtonsoft.Json
                ReadCommentHandling = JsonCommentHandling.Skip, // Skip comments like Newtonsoft.Json
                WriteIndented = false, // Compact JSON like Newtonsoft.Json default
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Less aggressive escaping
                IgnoreNullValues = false // Don't ignore null values (like Newtonsoft.Json default behavior)
            };
            logger = pubnubConfig.Logger;
        }

        #region IJsonPluggableLibrary methods implementation

        private static bool IsValidJson(string jsonString, PNOperationType operationType)
        {
            bool ret = false;
            try
            {
                using var doc = JsonDocument.Parse(jsonString);
                ret = true;
            }
            catch
            {
                /* ignore */
            }

            return ret;
        }

        public object BuildJsonObject(string jsonString)
        {
            object ret = null;

            try
            {
                using var doc = JsonDocument.Parse(jsonString);
                ret = doc.RootElement.Clone();
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
                    using var doc = JsonDocument.Parse(jsonString);
                    ret = doc.RootElement.ValueKind == JsonValueKind.Object;
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
            return JsonSerializer.Serialize(objectToSerialize, defaultJsonSerializerOptions);
        }

        public List<object> DeserializeToListOfObject(string jsonString)
        {
            List<object> result = JsonSerializer.Deserialize<List<object>>(jsonString, defaultJsonSerializerOptions);
            return result;
        }

        public object DeserializeToObject(object rawObject, Type type)
        {
            try
            {
                logger?.Debug("SystemTextJson Deserializing object data.");
                if (rawObject is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize(jsonElement.GetRawText(), type, defaultJsonSerializerOptions);
                }
                else if (rawObject is string jsonString)
                {
                    return JsonSerializer.Deserialize(jsonString, type, defaultJsonSerializerOptions);
                }
                else
                {
                    return rawObject;
                }
            }
            catch (Exception e)
            {
                logger?.Error($"Deserialize To Object failed with exception {e.Message}, stack trace {e.StackTrace}");
                return rawObject;
            }
        }

        public object DeserializeToObject(string jsonString)
        {
            logger?.Debug("SystemTextJson Deserializing json string data.");
            object result = null;
            try
            {
                using var doc = JsonDocument.Parse(jsonString);
                result = ConvertJsonElementToObject(doc.RootElement);
                
                logger?.Debug("SystemTextJson Deserializer json string data successfully.");
            }
            catch (Exception e)
            {
                logger?.Error($"Deserialize To Object failed with exception {e.Message} stack trace {e.StackTrace} reason: {e?.InnerException?.StackTrace}");
                // throw;
            }

            return result;
        }

        public void PopulateObject(string value, object target)
        {
            try
            {
                // System.Text.Json doesn't have direct PopulateObject equivalent
                // We need to deserialize and then copy properties
                using var doc = JsonDocument.Parse(value);
                var sourceDict = ConvertJsonElementToDictionary(doc.RootElement);
                
                var targetType = target.GetType();
                foreach (var kvp in sourceDict)
                {
                    var property = targetType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property != null && property.CanWrite)
                    {
                        var convertedValue = ConvertToDataType(property.PropertyType, kvp.Value);
                        property.SetValue(target, convertedValue);
                    }
                }
            }
            catch (Exception e)
            {
                logger?.Error($"PopulateObject failed with exception {e.Message}");
            }
        }

        public virtual T DeserializeToObject<T>(string jsonString)
        {
            T ret = default(T);

            try
            {
                ret = JsonSerializer.Deserialize<T>(jsonString, defaultJsonSerializerOptions);
            }
            catch
            {
                /* ignore */
            }

            return ret;
        }

        private bool IsGenericTypeForMessage<T>()
        {
            bool ret = typeof(T).GetTypeInfo().IsGenericType &&
                       typeof(T).GetGenericTypeDefinition() == typeof(PNMessageResult<>);
            return ret;
        }

        private T DeserializeMessageToObjectBasedOnPlatform<T>(List<object> listObject)
        {
            logger?.Debug("SystemTextJson Deserializing Messages data.");
            T ret = default(T);
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
                if (listObject[0] is JsonElement jsonElement)
                {
                    userMessage = ConvertJsonElementToObject(jsonElement);
                    userMessage = ConvertToDataType(dataType, userMessage);
                    dataProp.SetValue(message, userMessage, null);
                }
                else if (listObject[0] is string)
                {
                    userMessage = listObject[0] as string;
                    dataProp.SetValue(message, userMessage, null);
                }
                else
                {
                    // Fallback: try to serialize and deserialize
                    try
                    {
                        string jsonString = JsonSerializer.Serialize(listObject[0], defaultJsonSerializerOptions);
                        userMessage = JsonSerializer.Deserialize(jsonString, dataProp.PropertyType, defaultJsonSerializerOptions);
                        dataProp.SetValue(message, userMessage, null);
                    }
                    catch
                    {
                        dataProp.SetValue(message, listObject[0], null);
                    }
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
                channelNameProp.SetValue(message, listObject[5]?.ToString(), null);

                // Set ChannelGroup
                PropertyInfo subsciptionProp = specific.GetRuntimeProperty("Subscription");
                subsciptionProp.SetValue(message, listObject[4]?.ToString(), null);
                
                PropertyInfo customMessageType = specific.GetRuntimeProperty("CustomMessageType");
                customMessageType.SetValue(message, listObject[6], null);
                
                //Set Metadata list second position, index=1
                if (listObject[1] != null)
                {
                    PropertyInfo userMetadataProp = specific.GetRuntimeProperty("UserMetadata");
                    userMetadataProp.SetValue(message, listObject[1], null);
                }

                ret = (T)Convert.ChangeType(message, specific, CultureInfo.InvariantCulture);
            }
            logger?.Debug("SystemTextJson Deserialized Messages successfully.");
            return ret;
        }

        public T DeserializeToObject<T>(IDictionary<string, object> jsonFields)
        {
            T response = default(T);
            Type dataType = typeof(T).GetTypeInfo().GenericTypeArguments[0];
            Type generic = typeof(PNMessageResult<>);
            Type specific = generic.MakeGenericType(dataType);

            var content = jsonFields["payload"];
            ConstructorInfo ci = specific.GetTypeInfo().DeclaredConstructors.FirstOrDefault();
            if (ci != null)
            {
                object message = ci.Invoke(new object[] { });
                PropertyInfo dataProp = specific.GetRuntimeProperty("Message");
                object userMessage = null;
                
                if (content is JsonElement jsonElement)
                {
                    userMessage = ConvertJsonElementToObject(jsonElement);
                    userMessage = ConvertToDataType(dataType, userMessage);
                    dataProp.SetValue(message, userMessage, null);
                }
                else if (content is string)
                {
                    userMessage = content as string;
                    dataProp.SetValue(message, userMessage, null);
                }
                else
                {
                    // Fallback: try to serialize and deserialize
                    try
                    {
                        string jsonString = JsonSerializer.Serialize(content, defaultJsonSerializerOptions);
                        userMessage = JsonSerializer.Deserialize(jsonString, dataProp.PropertyType, defaultJsonSerializerOptions);
                        dataProp.SetValue(message, userMessage, null);
                    }
                    catch
                    {
                        dataProp.SetValue(message, content, null);
                    }
                }

                PropertyInfo timeProp = specific.GetRuntimeProperty("Timetoken");
                long timetoken;
                Int64.TryParse(jsonFields["publishTimetoken"].ToString(), out timetoken);
                timeProp.SetValue(message, timetoken, null);

                PropertyInfo publisherProp = specific.GetRuntimeProperty("Publisher");
                string publisherValue = (jsonFields["userId"] != null) ? jsonFields["userId"].ToString() : "";
                publisherProp.SetValue(message, publisherValue, null);

                PropertyInfo channelNameProp = specific.GetRuntimeProperty("Channel");
                channelNameProp.SetValue(message, jsonFields["channel"]?.ToString(), null);

                PropertyInfo subsciptionProp = specific.GetRuntimeProperty("Subscription");
                subsciptionProp.SetValue(message, jsonFields["channelGroup"]?.ToString(), null);

                if (jsonFields.ContainsKey("customMessageType"))
                {
                    PropertyInfo customMessageType = specific.GetRuntimeProperty("CustomMessageType");
                    customMessageType.SetValue(message, jsonFields["customMessageType"], null);
                }

                if (jsonFields["userMetadata"] != null)
                {
                    PropertyInfo userMetadataProp = specific.GetRuntimeProperty("UserMetadata");
                    userMetadataProp.SetValue(message, jsonFields["userMetadata"], null);
                }

                response = (T)Convert.ChangeType(message, specific, CultureInfo.InvariantCulture);
            }

            return response;
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
                    result = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, defaultJsonSerializerOptions);
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
                    if (localContainer is JsonElement jsonElement)
                    {
                        ret = ConvertJsonElementToDictionary(jsonElement);
                    }
                    else if (localContainer is Dictionary<string, object> dictionary)
                    {
                        ret = new Dictionary<string, object>();
                        foreach (string key in dictionary.Keys)
                        {
                            ret.Add(key, dictionary[key]);
                        }
                    }
                    else if (localContainer is List<object> localList)
                    {
                        ret = new Dictionary<string, object>();
                        for (int index = 0; index < localList.Count; index++)
                        {
                            ret.Add(index.ToString(), localList[index]);
                        }
                    }
                    else
                    {
                        // Fallback: try to serialize and deserialize as dictionary
                        string jsonString = JsonSerializer.Serialize(localContainer, defaultJsonSerializerOptions);
                        ret = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, defaultJsonSerializerOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.Error($"ConvertToDictionaryObject failed: {ex.Message}");
            }

            return ret;
        }

        public object[] ConvertToObjectArray(object localContainer)
        {
            object[] ret = null;

            try
            {
                if (localContainer != null)
                {
                    if (localContainer is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                    {
                        ret = jsonElement.EnumerateArray().Select(ConvertJsonElementToObject).ToArray();
                    }
                    else if (localContainer is List<object> localList)
                    {
                        ret = localList.ToArray();
                    }
                    else if (localContainer is object[] objectArray)
                    {
                        ret = objectArray;
                    }
                    else
                    {
                        // Fallback: try to serialize and deserialize as array
                        string jsonString = JsonSerializer.Serialize(localContainer, defaultJsonSerializerOptions);
                        ret = JsonSerializer.Deserialize<object[]>(jsonString, defaultJsonSerializerOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.Error($"ConvertToObjectArray failed: {ex.Message}");
            }

            return ret;
        }

        #endregion

        #region Helper Methods

        public static bool JsonFastCheck(string rawJson)
        {
            return !string.IsNullOrEmpty(rawJson) && rawJson.Trim().StartsWith("{") && rawJson.Trim().EndsWith("}");
        }

        private static object ConvertJsonElementToObject(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object>();
                    foreach (JsonProperty property in element.EnumerateObject())
                    {
                        obj[property.Name] = ConvertJsonElementToObject(property.Value);
                    }
                    return obj;
                case JsonValueKind.Array:
                    return element.EnumerateArray().Select(ConvertJsonElementToObject).ToList();
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    if (element.TryGetInt64(out long longValue))
                        return longValue;
                    if (element.TryGetDouble(out double doubleValue))
                        return doubleValue;
                    return element.GetDecimal();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.ToString();
            }
        }



        private static Dictionary<string, object> ConvertJsonElementToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, object>();
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    dict[property.Name] = ConvertJsonElementToObject(property.Value);
                }
            }
            return dict;
        }



        private static object ConvertToDataType(Type dataType, object inputValue)
        {
            if (inputValue == null || dataType == null)
                return inputValue;

            try
            {
                if (dataType == inputValue.GetType())
                    return inputValue;

                if (dataType == typeof(string))
                    return inputValue.ToString();

                if (dataType.IsEnum)
                    return Enum.Parse(dataType, inputValue.ToString());

                if (dataType == typeof(DateTime) || dataType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(inputValue.ToString(), out DateTime dateTime))
                        return dateTime;
                }

                // Handle nullable types
                if (dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    Type underlyingType = Nullable.GetUnderlyingType(dataType);
                    return ConvertToDataType(underlyingType, inputValue);
                }

                return Convert.ChangeType(inputValue, dataType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return inputValue;
            }
        }

        #endregion
    }
} 