using System.Collections.Generic;

namespace PubnubApi.JsonV2.Parsers
{
    /// <summary>
    /// Modern JSON parser interface for clean, type-safe parsing
    /// </summary>
    /// <typeparam name="T">Target result type</typeparam>
    public interface IModernJsonParser<T> where T : class
    {
        /// <summary>
        /// Parse from List&lt;object&gt; format (most common in PubNub SDK)
        /// </summary>
        /// <param name="listObject">Source list object</param>
        /// <returns>Parsed result</returns>
        T Parse(List<object> listObject);

        /// <summary>
        /// Parse from JSON string
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <returns>Parsed result</returns>
        T Parse(string jsonString);

        /// <summary>
        /// Parse from dictionary format
        /// </summary>
        /// <param name="jsonData">Dictionary data</param>
        /// <returns>Parsed result</returns>
        T Parse(IDictionary<string, object> jsonData);
    }
}
