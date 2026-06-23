using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    /// <summary>
    /// Centralized formatting for structured log blocks (parameters and network traffic),
    /// producing the aligned multi-line layout used across PubNub SDKs:
    ///   key   : value
    ///   nested:
    ///     - item
    /// </summary>
    internal static class PubnubLogFormatter
    {
        private const int IndentSize = 2;
        private const int MaxDepth = 10;

        /// <summary>
        /// Builds a "&lt;name&gt; with parameters:" block from alternating key/value
        /// arguments (key1, value1, key2, value2, ...). Pairs with a null value are
        /// rendered as "null" (matching cross-SDK behavior).
        /// </summary>
        /// <remarks>
        /// Uses a flat <c>object[]</c> instead of value tuples so the API compiles on
        /// targets (e.g. UWP) whose reference assemblies do not provide
        /// <c>System.ValueTuple</c>.
        /// </remarks>
        public static string Parameters(string name, params object[] keyValues) =>
            Block($"{name} with parameters:", keyValues);

        /// <summary>
        /// Builds an aligned key/value block under an explicit header line, e.g.
        /// "Create with configuration:", from alternating key/value arguments
        /// (key1, value1, key2, value2, ...).
        /// </summary>
        public static string Block(string header, params object[] keyValues)
        {
            var map = new OrderedMap();
            if (keyValues != null)
            {
                for (int i = 0; i + 1 < keyValues.Length; i += 2)
                {
                    map.Add(Convert.ToString(keyValues[i], CultureInfo.InvariantCulture), keyValues[i + 1]);
                }
            }
            var sb = new StringBuilder();
            sb.Append(header);
            AppendObject(sb, map, 1);
            return sb.ToString();
        }

        /// <summary>
        /// Formats an outgoing HTTP request. Headers are only included at Trace level,
        /// mirroring the JS SDK behavior.
        /// </summary>
        public static string HttpRequest(string method, string url, IDictionary<string, string> headers,
            int bodySizeBytes, bool includeHeaders)
        {
            var sb = new StringBuilder();
            sb.Append("Sending HTTP request:");
            AppendLabel(sb, "Method", method);
            AppendLabel(sb, "URL", url);
            if (includeHeaders && headers != null && headers.Count > 0)
            {
                AppendHeaders(sb, headers);
            }
            AppendBodySize(sb, bodySizeBytes);
            return sb.ToString();
        }

        /// <summary>
        /// Formats a received HTTP response. Headers are only included at Trace level.
        /// </summary>
        public static string HttpResponse(string url, int statusCode, IDictionary<string, string> headers,
            int bodySizeBytes, bool includeHeaders)
        {
            var sb = new StringBuilder();
            sb.Append("Received HTTP response:");
            AppendLabel(sb, "URL", url);
            AppendLabel(sb, "Status", statusCode.ToString(CultureInfo.InvariantCulture));
            if (includeHeaders && headers != null && headers.Count > 0)
            {
                AppendHeaders(sb, headers);
            }
            AppendBodySize(sb, bodySizeBytes);
            return sb.ToString();
        }

        private static void AppendLabel(StringBuilder sb, string label, string value)
        {
            sb.Append('\n').Append(Indent(1)).Append(label).Append(": ").Append(value);
        }

        private static void AppendHeaders(StringBuilder sb, IDictionary<string, string> headers)
        {
            sb.Append('\n').Append(Indent(1)).Append("Headers:");
            int maxKey = headers.Keys.Select(k => k.Length).DefaultIfEmpty(0).Max();
            foreach (var kvp in headers)
            {
                sb.Append('\n').Append(Indent(2)).Append("- ")
                    .Append(kvp.Key.PadRight(maxKey)).Append(": ").Append(kvp.Value);
            }
        }

        private static void AppendBodySize(StringBuilder sb, int bodySizeBytes)
        {
            sb.Append('\n').Append(Indent(1)).Append("Body size: ")
                .Append(bodySizeBytes.ToString(CultureInfo.InvariantCulture)).Append(" bytes");
        }

        private static string Indent(int level) => new string(' ', level * IndentSize);

        // Recursively appends a value, choosing scalar/array/object rendering. Caller has
        // already written the key prefix (or this is a top-level object). For objects/arrays
        // the content is written on following lines indented at "level".
        private static void AppendValue(StringBuilder sb, object value, int level)
        {
            if (level >= MaxDepth)
            {
                sb.Append("...");
                return;
            }
            switch (value)
            {
                case null:
                    sb.Append("null");
                    break;
                case string s:
                    sb.Append(s);
                    break;
                case bool b:
                    sb.Append(b ? "true" : "false");
                    break;
                case IDictionary dict:
                    AppendDictionary(sb, dict, level);
                    break;
                case OrderedMap map:
                    AppendObjectInline(sb, map, level);
                    break;
                case IEnumerable enumerable when !(value is string):
                    AppendArray(sb, enumerable, level);
                    break;
                default:
                    sb.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                    break;
            }
        }

        private static bool IsScalar(object value) =>
            value == null || value is string || value is bool || value.GetType().IsPrimitive
            || value is decimal || value is Enum;

        private static void AppendObject(StringBuilder sb, OrderedMap map, int level)
        {
            if (map.Count == 0)
            {
                sb.Append(" {}");
                return;
            }
            int maxKey = map.Keys.Select(k => k.Length).DefaultIfEmpty(0).Max();
            foreach (var key in map.Keys)
            {
                var value = map[key];
                sb.Append('\n').Append(Indent(level)).Append(key.PadRight(maxKey));
                if (IsScalar(value) || HasInlineToString(value))
                {
                    sb.Append(": ");
                    AppendValue(sb, value, level + 1);
                }
                else if (IsEmptyCollection(value))
                {
                    sb.Append(": ").Append(value is IDictionary ? "{}" : "[]");
                }
                else
                {
                    sb.Append(':');
                    AppendValue(sb, value, level + 1);
                }
            }
        }

        private static void AppendObjectInline(StringBuilder sb, OrderedMap map, int level) =>
            AppendObject(sb, map, level);

        private static void AppendDictionary(StringBuilder sb, IDictionary dict, int level)
        {
            var map = new OrderedMap();
            foreach (DictionaryEntry entry in dict)
            {
                map.Add(Convert.ToString(entry.Key, CultureInfo.InvariantCulture), entry.Value);
            }
            AppendObject(sb, map, level);
        }

        private static void AppendArray(StringBuilder sb, IEnumerable enumerable, int level)
        {
            foreach (var item in enumerable)
            {
                sb.Append('\n').Append(Indent(level)).Append("- ");
                if (IsScalar(item) || HasInlineToString(item))
                {
                    AppendValue(sb, item, level + 1);
                }
                else
                {
                    // Nested object/array continues on following lines.
                    AppendValue(sb, item, level + 1);
                }
            }
        }

        private static bool IsEmptyCollection(object value)
        {
            if (value is IDictionary d) return d.Count == 0;
            if (value is OrderedMap m) return m.Count == 0;
            if (value is IEnumerable e && !(value is string)) return !e.Cast<object>().Any();
            return false;
        }

        // Non-collection, non-scalar values that have a meaningful ToString are printed inline.
        private static bool HasInlineToString(object value)
        {
            if (value == null) return false;
            if (value is IDictionary || value is OrderedMap) return false;
            if (value is IEnumerable && !(value is string)) return false;
            return true;
        }

        // Preserves insertion order for keyed parameter blocks.
        private sealed class OrderedMap
        {
            private readonly List<string> keys = new List<string>();
            private readonly Dictionary<string, object> values = new Dictionary<string, object>();

            public int Count => keys.Count;
            public IReadOnlyList<string> Keys => keys;
            public object this[string key] => values[key];

            public void Add(string key, object value)
            {
                if (!values.ContainsKey(key)) keys.Add(key);
                values[key] = value;
            }
        }
    }
}
