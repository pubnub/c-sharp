using System;
using System.Collections.Generic;
using System.Globalization;

namespace PubnubApi.JsonV2
{
    /// <summary>
    /// Extension methods that provide V2 functionality to V1 implementations
    /// Ensures backward compatibility while enabling modern parsing for all implementations
    /// </summary>
    public static class JsonPluggableLibraryExtensions
    {
        /// <summary>
        /// Safely extracts a typed value from a source object using a property name
        /// Automatically uses V2 implementation if available, otherwise provides V1 fallback
        /// </summary>
        public static T GetValue<T>(this IJsonPluggableLibrary jsonLib, object source, string propertyName, T defaultValue = default)
        {
            // Use V2 implementation if available for better performance
            if (jsonLib is IJsonPluggableLibraryV2 v2)
                return v2.GetValue<T>(source, propertyName, defaultValue);

            // V1 fallback implementation
            if (source == null || string.IsNullOrEmpty(propertyName))
                return defaultValue;

            try
            {
                var dict = jsonLib.ConvertToDictionaryObject(source);
                if (dict == null || !dict.TryGetValue(propertyName, out var value))
                    return defaultValue;

                if (value == null) return defaultValue;

                if (jsonLib.TryParseValue<T>(value, out T result))
                    return result;

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Navigates through nested array/list structures using multiple indices
        /// </summary>
        public static object GetArrayElement(this IJsonPluggableLibrary jsonLib, List<object> source, params int[] indices)
        {
            // Use V2 implementation if available
            if (jsonLib is IJsonPluggableLibraryV2 v2)
                return v2.GetArrayElement(source, indices);

            // V1 fallback implementation
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
                else
                {
                    var convertedArray = jsonLib.ConvertToObjectArray(current);
                    if (convertedArray != null && index >= 0 && index < convertedArray.Length)
                        current = convertedArray[index];
                    else
                        return null;
                }
            }

            return current;
        }

        /// <summary>
        /// Extracts an array of typed objects from a source
        /// </summary>
        public static List<T> GetArray<T>(this IJsonPluggableLibrary jsonLib, object source, string propertyName = null)
        {
            // Use V2 implementation if available
            if (jsonLib is IJsonPluggableLibraryV2 v2)
                return v2.GetArray<T>(source, propertyName);

            // V1 fallback implementation
            var result = new List<T>();

            try
            {
                object arraySource = propertyName != null ? jsonLib.GetValue<object>(source, propertyName) : source;

                var objectArray = jsonLib.ConvertToObjectArray(arraySource);
                if (objectArray != null)
                {
                    foreach (var item in objectArray)
                    {
                        if (jsonLib.TryParseValue<T>(item, out T convertedItem))
                            result.Add(convertedItem);
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return result;
        }

        /// <summary>
        /// Safely converts source to dictionary, optionally navigating to a property first
        /// </summary>
        public static Dictionary<string, object> GetDictionary(this IJsonPluggableLibrary jsonLib, object source, string propertyName = null)
        {
            // Use V2 implementation if available
            if (jsonLib is IJsonPluggableLibraryV2 v2)
                return v2.GetDictionary(source, propertyName);

            // V1 fallback implementation
            try
            {
                object dictSource = propertyName != null ? jsonLib.GetValue<object>(source, propertyName) : source;
                return jsonLib.ConvertToDictionaryObject(dictSource) ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Checks if a source object has a specific property
        /// </summary>
        public static bool HasProperty(this IJsonPluggableLibrary jsonLib, object source, string propertyName)
        {
            // Use V2 implementation if available
            if (jsonLib is IJsonPluggableLibraryV2 v2)
                return v2.HasProperty(source, propertyName);

            // V1 fallback implementation
            if (source == null || string.IsNullOrEmpty(propertyName))
                return false;

            try
            {
                var dict = jsonLib.ConvertToDictionaryObject(source);
                return dict?.ContainsKey(propertyName) ?? false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse a value to the specified type with comprehensive type handling
        /// </summary>
        public static bool TryParseValue<T>(this IJsonPluggableLibrary jsonLib, object value, out T result)
        {
            // Use V2 implementation if available
            if (jsonLib is IJsonPluggableLibraryV2 v2)
                return v2.TryParseValue<T>(value, out result);

            // V1 fallback implementation
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

                // Handle nullable types
                var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                // Common conversions
                if (targetType == typeof(long) && long.TryParse(value.ToString(), out long longVal))
                {
                    result = (T)(object)longVal;
                    return true;
                }

                if (targetType == typeof(int) && int.TryParse(value.ToString(), out int intVal))
                {
                    result = (T)(object)intVal;
                    return true;
                }

                if (targetType == typeof(string))
                {
                    result = (T)(object)value.ToString();
                    return true;
                }

                if (targetType == typeof(bool) && bool.TryParse(value.ToString(), out bool boolVal))
                {
                    result = (T)(object)boolVal;
                    return true;
                }

                // For complex types, try JSON round-trip
                if (!targetType.IsPrimitive && targetType != typeof(string))
                {
                    var serialized = jsonLib.SerializeToJsonString(value);
                    result = jsonLib.DeserializeToObject<T>(serialized);
                    return result != null;
                }

                // Try Convert.ChangeType as last resort
                result = (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Safely extracts a timetoken (long) value, handling various input formats
        /// </summary>
        public static long GetTimetoken(this IJsonPluggableLibrary jsonLib, object source, string propertyName = "timetoken")
        {
            // Use V2 implementation if available
            if (jsonLib is IJsonPluggableLibraryV2 v2)
                return v2.GetTimetoken(source, propertyName);

            // V1 fallback implementation
            return jsonLib.GetValue<long>(source, propertyName, 0);
        }
    }
}