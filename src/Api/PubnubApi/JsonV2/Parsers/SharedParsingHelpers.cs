using System;
using System.Collections.Generic;

namespace PubnubApi.JsonV2.Parsers
{
    /// <summary>
    /// Shared helper methods for common parsing patterns across different result types
    /// Reduces code duplication and provides consistent parsing behavior
    /// </summary>
    internal static class SharedParsingHelpers
    {
        /// <summary>
        /// Safely parses a timetoken from various input formats
        /// </summary>
        public static long ParseTimetoken(IJsonPluggableLibraryV2 jsonLibrary, object source, string propertyName = "timetoken", long defaultValue = 0)
        {
            var value = jsonLibrary.GetValue<object>(source, propertyName);
            if (value == null) return defaultValue;

            if (long.TryParse(value.ToString(), out long timetoken))
                return timetoken;

            return defaultValue;
        }

        /// <summary>
        /// Safely parses a timetoken directly from an object value
        /// </summary>
        public static long ParseTimetoken(object value, long defaultValue = 0)
        {
            if (value == null) return defaultValue;

            if (long.TryParse(value.ToString(), out long timetoken))
                return timetoken;

            return defaultValue;
        }

        /// <summary>
        /// Safely parses an integer value from various input formats
        /// </summary>
        public static int ParseInteger(IJsonPluggableLibraryV2 jsonLibrary, object source, string propertyName, int defaultValue = 0)
        {
            var value = jsonLibrary.GetValue<object>(source, propertyName);
            if (value == null) return defaultValue;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Safely parses an integer directly from an object value
        /// </summary>
        public static int ParseInteger(object value, int defaultValue = 0)
        {
            if (value == null) return defaultValue;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Safely extracts a string value, handling null and type conversion
        /// </summary>
        public static string ParseString(IJsonPluggableLibraryV2 jsonLibrary, object source, string propertyName, string defaultValue = null)
        {
            var value = jsonLibrary.GetValue<object>(source, propertyName);
            return value?.ToString() ?? defaultValue;
        }

        /// <summary>
        /// Converts various array/list formats to List<object>
        /// Handles both List<object> and object[] inputs consistently
        /// </summary>
        public static List<object> NormalizeToList(object arraySource)
        {
            if (arraySource == null)
                return new List<object>();

            if (arraySource is List<object> list)
                return list;

            if (arraySource is object[] array)
                return new List<object>(array);

            // Single item - wrap in list
            return new List<object> { arraySource };
        }

        /// <summary>
        /// Safely parses boolean values with flexible input handling
        /// Supports "1"/"0", "true"/"false", boolean values
        /// </summary>
        public static bool ParseBoolean(IJsonPluggableLibraryV2 jsonLibrary, object source, string propertyName, bool defaultValue = false)
        {
            var value = jsonLibrary.GetValue<object>(source, propertyName);
            return ParseBoolean(value, defaultValue);
        }

        /// <summary>
        /// Safely parses boolean values directly from object
        /// </summary>
        public static bool ParseBoolean(object value, bool defaultValue = false)
        {
            if (value == null) return defaultValue;

            if (value is bool boolValue)
                return boolValue;

            var stringValue = value.ToString();
            if (stringValue == "1") return true;
            if (stringValue == "0") return false;

            if (bool.TryParse(stringValue, out bool result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Creates a safe dictionary wrapper that handles null inputs
        /// </summary>
        public static Dictionary<string, object> SafeDictionary(Dictionary<string, object> source)
        {
            return source ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Safely gets a nested dictionary value, creating path if needed
        /// </summary>
        public static Dictionary<string, object> GetNestedDictionary(IJsonPluggableLibraryV2 jsonLibrary, object source, params string[] path)
        {
            object current = source;

            foreach (string key in path)
            {
                if (current == null) return new Dictionary<string, object>();

                var dict = jsonLibrary.GetDictionary(current);
                if (!dict.ContainsKey(key)) return new Dictionary<string, object>();

                current = dict[key];
            }

            return jsonLibrary.GetDictionary(current);
        }
    }
}
