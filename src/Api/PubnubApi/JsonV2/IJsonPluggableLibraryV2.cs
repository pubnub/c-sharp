using System;
using System.Collections.Generic;

namespace PubnubApi.JsonV2
{
    /// <summary>
    /// Enhanced JSON library interface that provides optimized parsing methods
    /// while maintaining backward compatibility with IJsonPluggableLibrary
    /// </summary>
    public interface IJsonPluggableLibraryV2 : IJsonPluggableLibrary
    {
        /// <summary>
        /// Safely extracts a typed value from a source object using a property name
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="source">Source object (dictionary, JObject, etc.)</param>
        /// <param name="propertyName">Property name to extract</param>
        /// <param name="defaultValue">Default value if property doesn't exist or conversion fails</param>
        /// <returns>Converted value or default</returns>
        T GetValue<T>(object source, string propertyName, T defaultValue = default);

        /// <summary>
        /// Navigates through nested array/list structures using multiple indices
        /// </summary>
        /// <param name="source">Source list</param>
        /// <param name="indices">Array indices to navigate (e.g., [0, 1] gets source[0][1])</param>
        /// <returns>Object at the specified path or null</returns>
        object GetArrayElement(List<object> source, params int[] indices);

        /// <summary>
        /// Extracts an array of typed objects from a source
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="propertyName">Optional property name if source is an object with array property</param>
        /// <returns>List of converted objects</returns>
        List<T> GetArray<T>(object source, string propertyName = null);

        /// <summary>
        /// Safely converts source to dictionary, optionally navigating to a property first
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="propertyName">Optional property name to navigate to first</param>
        /// <returns>Dictionary or empty dictionary if conversion fails</returns>
        Dictionary<string, object> GetDictionary(object source, string propertyName = null);

        /// <summary>
        /// Checks if a source object has a specific property
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="propertyName">Property name to check</param>
        /// <returns>True if property exists</returns>
        bool HasProperty(object source, string propertyName);

        /// <summary>
        /// Attempts to parse a value to the specified type with comprehensive type handling
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="value">Value to convert</param>
        /// <param name="result">Converted result</param>
        /// <returns>True if conversion successful</returns>
        bool TryParseValue<T>(object value, out T result);

        /// <summary>
        /// Safely extracts a timetoken (long) value, handling various input formats
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="propertyName">Property name (defaults to "timetoken")</param>
        /// <returns>Parsed timetoken or 0</returns>
        long GetTimetoken(object source, string propertyName = "timetoken");
    }
}
