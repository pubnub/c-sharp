using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PubnubApi.JsonV2
{
    /// <summary>
    /// Enhanced Newtonsoft.Json implementation with optimized parsing methods
    /// </summary>
    public class NewtonsoftJsonDotNetV2 : NewtonsoftJsonDotNet, IJsonPluggableLibraryV2
    {
        public NewtonsoftJsonDotNetV2(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
        }

        #region IJsonPluggableLibraryV2 Implementation

        public T GetValue<T>(object source, string propertyName, T defaultValue = default)
        {
            if (source == null || string.IsNullOrEmpty(propertyName))
                return defaultValue;

            try
            {
                object value = null;

                // Optimized path for JObject (direct token access)
                if (source is JObject jObject)
                {
                    var token = jObject[propertyName];
                    if (token == null || token.Type == JTokenType.Null)
                        return defaultValue;
                    
                    value = token.ToObject<object>();
                }
                // Optimized path for JToken
                else if (source is JToken jToken)
                {
                    var childToken = jToken[propertyName];
                    if (childToken == null || childToken.Type == JTokenType.Null)
                        return defaultValue;
                    
                    value = childToken.ToObject<object>();
                }
                // Dictionary path (fallback)
                else if (source is Dictionary<string, object> dict)
                {
                    if (!dict.TryGetValue(propertyName, out value))
                        return defaultValue;
                }
                // IDictionary path (fallback)
                else if (source is IDictionary<string, object> iDict)
                {
                    if (!iDict.TryGetValue(propertyName, out value))
                        return defaultValue;
                }
                else
                {
                    // Last resort - use base implementation
                    var convertedDict = ConvertToDictionaryObject(source);
                    if (convertedDict == null || !convertedDict.TryGetValue(propertyName, out value))
                        return defaultValue;
                }

                if (value == null) return defaultValue;

                // Use optimized conversion
                if (TryParseValue<T>(value, out T result))
                    return result;

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public object GetArrayElement(List<object> source, params int[] indices)
        {
            if (source == null || indices == null || indices.Length == 0)
                return null;

            object current = source;

            foreach (int index in indices)
            {
                if (current is List<object> list)
                {
                    if (index >= 0 && index < list.Count)
                        current = list[index];
                    else
                        return null;
                }
                else if (current is object[] array)
                {
                    if (index >= 0 && index < array.Length)
                        current = array[index];
                    else
                        return null;
                }
                else if (current is JArray jArray)
                {
                    if (index >= 0 && index < jArray.Count)
                        current = jArray[index].ToObject<object>();
                    else
                        return null;
                }
                else
                {
                    // Try to convert using base method
                    var convertedArray = ConvertToObjectArray(current);
                    if (convertedArray != null && index >= 0 && index < convertedArray.Length)
                        current = convertedArray[index];
                    else
                        return null;
                }
            }

            return current;
        }

        public List<T> GetArray<T>(object source, string propertyName = null)
        {
            var result = new List<T>();

            try
            {
                object arraySource = propertyName != null ? GetValue<object>(source, propertyName) : source;

                if (arraySource == null) return result;

                // Optimized JArray handling
                if (arraySource is JArray jArray)
                {
                    foreach (var item in jArray)
                    {
                        if (TryParseValue<T>(item.ToObject<object>(), out T convertedItem))
                            result.Add(convertedItem);
                    }
                }
                // List<object> handling
                else if (arraySource is List<object> list)
                {
                    foreach (var item in list)
                    {
                        if (TryParseValue<T>(item, out T convertedItem))
                            result.Add(convertedItem);
                    }
                }
                // Object array handling
                else if (arraySource is object[] array)
                {
                    foreach (var item in array)
                    {
                        if (TryParseValue<T>(item, out T convertedItem))
                            result.Add(convertedItem);
                    }
                }
                else
                {
                    // Fallback to base implementation
                    var convertedArray = ConvertToObjectArray(arraySource);
                    if (convertedArray != null)
                    {
                        foreach (var item in convertedArray)
                        {
                            if (TryParseValue<T>(item, out T convertedItem))
                                result.Add(convertedItem);
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return result;
        }

        public Dictionary<string, object> GetDictionary(object source, string propertyName = null)
        {
            try
            {
                object dictSource = propertyName != null ? GetValue<object>(source, propertyName) : source;

                if (dictSource == null)
                    return new Dictionary<string, object>();

                // Use base implementation for consistency
                return ConvertToDictionaryObject(dictSource) ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        public bool HasProperty(object source, string propertyName)
        {
            if (source == null || string.IsNullOrEmpty(propertyName))
                return false;

            try
            {
                // Optimized JObject check
                if (source is JObject jObj)
                    return jObj.ContainsKey(propertyName);

                // Optimized JToken check
                if (source is JToken jToken && jToken.Type == JTokenType.Object)
                    return ((JObject)jToken).ContainsKey(propertyName);

                // Dictionary checks
                if (source is Dictionary<string, object> dict)
                    return dict.ContainsKey(propertyName);

                if (source is IDictionary<string, object> iDict)
                    return iDict.ContainsKey(propertyName);

                // Fallback
                var convertedDict = ConvertToDictionaryObject(source);
                return convertedDict?.ContainsKey(propertyName) ?? false;
            }
            catch
            {
                return false;
            }
        }

        public bool TryParseValue<T>(object value, out T result)
        {
            result = default(T);

            if (value == null)
                return typeof(T).IsClass || Nullable.GetUnderlyingType(typeof(T)) != null;

            try
            {
                // Direct assignment if types match
                if (value is T directValue)
                {
                    result = directValue;
                    return true;
                }

                // Handle JValue specifically
                if (value is JValue jValue)
                {
                    value = jValue.Value;
                    if (value is T jDirectValue)
                    {
                        result = jDirectValue;
                        return true;
                    }
                }

                // Handle nullable types
                var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                // Optimized common conversions
                if (targetType == typeof(long))
                {
                    if (long.TryParse(value?.ToString(), out long longVal))
                    {
                        result = (T)(object)longVal;
                        return true;
                    }
                }
                else if (targetType == typeof(int))
                {
                    if (int.TryParse(value?.ToString(), out int intVal))
                    {
                        result = (T)(object)intVal;
                        return true;
                    }
                }
                else if (targetType == typeof(string))
                {
                    result = (T)(object)value?.ToString();
                    return true;
                }
                else if (targetType == typeof(bool))
                {
                    if (bool.TryParse(value?.ToString(), out bool boolVal))
                    {
                        result = (T)(object)boolVal;
                        return true;
                    }
                }
                else if (targetType == typeof(double))
                {
                    if (double.TryParse(value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleVal))
                    {
                        result = (T)(object)doubleVal;
                        return true;
                    }
                }
                else if (targetType == typeof(decimal))
                {
                    if (decimal.TryParse(value?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decimalVal))
                    {
                        result = (T)(object)decimalVal;
                        return true;
                    }
                }

                // For complex types, try direct conversion or JSON round-trip
                if (!targetType.IsPrimitive && targetType != typeof(string))
                {
                    // Try direct JToken conversion for complex types
                    if (value is JToken token)
                    {
                        result = token.ToObject<T>();
                        return true;
                    }

                    // JSON round-trip as last resort for complex types
                    var serialized = SerializeToJsonString(value);
                    result = DeserializeToObject<T>(serialized);
                    return result != null;
                }

                // Try Convert.ChangeType as final fallback
                result = (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public long GetTimetoken(object source, string propertyName = "timetoken")
        {
            return GetValue<long>(source, propertyName, 0);
        }

        #endregion

        #region Enhanced DeserializeToObject Integration

        /// <summary>
        /// Enhanced generic deserialization that uses modern V2 parsers when available
        /// Falls back to base V1 implementation for types not yet migrated
        /// </summary>
        public override T DeserializeToObject<T>(List<object> listObject)
        {
            // Try V2 parsers first for better performance and code reuse
            if (TryDeserializeWithV2Parser<T>(listObject, out T result))
            {
                return result;
            }

            // Fall back to base V1 implementation
            return base.DeserializeToObject<T>(listObject);
        }

        /// <summary>
        /// Enhanced dictionary-based deserialization
        /// </summary>
        public override T DeserializeToObject<T>(IDictionary<string, object> jsonFields)
        {
            // For dictionary input, convert to list format and use V2 parsers
            if (TryDeserializeWithV2Parser<T>(new List<object> { jsonFields }, out T result))
            {
                return result;
            }

            // Fall back to base V1 implementation
            return base.DeserializeToObject<T>(jsonFields);
        }

        /// <summary>
        /// Try to use V2 parsers for supported types
        /// </summary>
        private bool TryDeserializeWithV2Parser<T>(List<object> listObject, out T result)
        {
            result = default(T);

            try
            {
                logger?.Debug($"V2 Integration: Attempting to parse {typeof(T).Name}");

                if (typeof(T) == typeof(PNSetUuidMetadataResult))
                {
                    logger?.Debug("V2 Integration: Using PNSetUuidMetadataResultParser");
                    var parser = new Parsers.PNSetUuidMetadataResultParser(this, logger);
                    var parsedResult = parser.Parse(listObject);
                    result = (T)(object)parsedResult;
                    logger?.Debug($"V2 Integration: Parse result - Uuid={((PNSetUuidMetadataResult)(object)result)?.Uuid}");
                    return true;
                }

                if (typeof(T) == typeof(PNSetChannelMetadataResult))
                {
                    logger?.Debug("V2 Integration: Using PNSetChannelMetadataResultParser");
                    var parser = new Parsers.PNSetChannelMetadataResultParser(this, logger);
                    var parsedResult = parser.Parse(listObject);
                    result = (T)(object)parsedResult;
                    return true;
                }

                if (typeof(T) == typeof(PNHistoryResult))
                {
                    logger?.Debug("V2 Integration: Using PNHistoryResultParser");
                    var parser = new Parsers.PNHistoryResultParser(this, logger);
                    var parsedResult = parser.Parse(listObject);
                    result = (T)(object)parsedResult;
                    return true;
                }

                if (typeof(T) == typeof(PNAccessManagerGrantResult))
                {
                    logger?.Debug("V2 Integration: Using PNAccessManagerGrantResultParser");
                    var parser = new Parsers.PNAccessManagerGrantResultParser(this, logger);
                    var parsedResult = parser.Parse(listObject);
                    result = (T)(object)parsedResult;
                    return true;
                }

                logger?.Debug($"V2 Integration: No V2 parser available for {typeof(T).Name}");
                // Add more V2 parsers as they're implemented
            }
            catch (Exception ex)
            {
                logger?.Error($"V2 Integration: Exception in parser for {typeof(T).Name}: {ex.Message}");
                // If V2 parser fails, fall back to V1
                result = default(T);
                return false;
            }

            return false;
        }

        #endregion
    }
}
