using System;
using System.Collections.Generic;

namespace PubnubApi.JsonV2.Parsers
{
    /// <summary>
    /// Base parser for metadata objects (UUID metadata, Channel metadata, etc.)
    /// Eliminates code duplication across similar metadata parsing operations
    /// </summary>
    /// <typeparam name="T">Target metadata result type</typeparam>
    public abstract class MetadataObjectParser<T> : IModernJsonParser<T> where T : class, new()
    {
        protected readonly IJsonPluggableLibraryV2 jsonLibrary;
        protected readonly PubnubLogModule logger;

        protected MetadataObjectParser(IJsonPluggableLibraryV2 jsonLibrary, PubnubLogModule logger)
        {
            this.jsonLibrary = jsonLibrary ?? throw new ArgumentNullException(nameof(jsonLibrary));
            this.logger = logger;
        }

        public virtual T Parse(List<object> listObject)
        {
            if (listObject == null || listObject.Count < 2)
                return new T();

            // Common pattern: metadata is in listObject[1]["data"]
            var dataContainer = jsonLibrary.GetArrayElement(listObject, 1);
            var dataDict = jsonLibrary.GetDictionary(dataContainer, "data");

            // Debug: Check if data extraction is working
            if (dataContainer == null)
            {
                logger?.Debug($"V2 Parser: dataContainer is null for {typeof(T).Name}");
                return new T();
            }

            if (dataDict == null || dataDict.Count == 0)
            {
                logger?.Debug($"V2 Parser: dataDict is null or empty for {typeof(T).Name}");
                return new T();
            }

            return ParseMetadataObject(dataDict);
        }

        public virtual T Parse(string jsonString)
        {
            var listObject = jsonLibrary.DeserializeToListOfObject(jsonString);
            return Parse(listObject);
        }

        public virtual T Parse(IDictionary<string, object> jsonData)
        {
            var listObject = new List<object> { jsonData };
            return Parse(listObject);
        }

        /// <summary>
        /// Parse metadata object from dictionary with common fields handling
        /// </summary>
        /// <param name="dataDict">Data dictionary containing metadata fields</param>
        /// <returns>Parsed metadata object</returns>
        protected virtual T ParseMetadataObject(Dictionary<string, object> dataDict)
        {
            if (dataDict == null || dataDict.Count == 0)
                return new T();

            var result = new T();

            // Parse common metadata fields
            var id = jsonLibrary.GetValue<string>(dataDict, GetIdFieldName());
            if (!string.IsNullOrEmpty(id))
                SetId(result, id);

            var name = jsonLibrary.GetValue<string>(dataDict, "name");
            if (!string.IsNullOrEmpty(name))
                SetName(result, name);

            var updated = jsonLibrary.GetValue<string>(dataDict, "updated");
            // V1 parsers set Updated to empty string when missing, so match that behavior
            SetUpdated(result, updated ?? string.Empty);

            var custom = jsonLibrary.GetDictionary(dataDict, "custom");
            if (custom.Count > 0)
                SetCustom(result, custom);

            var status = jsonLibrary.GetValue<string>(dataDict, "status");
            if (!string.IsNullOrEmpty(status))
                SetStatus(result, status);

            var type = jsonLibrary.GetValue<string>(dataDict, "type");
            if (!string.IsNullOrEmpty(type))
                SetType(result, type);

            // Allow subclasses to parse additional fields specific to their type
            ParseAdditionalFields(result, dataDict);

            return result;
        }

        #region Abstract Methods - Must be implemented by subclasses

        /// <summary>
        /// Get the field name used for the ID (e.g., "id", "uuid", "channel")
        /// </summary>
        /// <returns>Field name for the identifier</returns>
        protected abstract string GetIdFieldName();

        /// <summary>
        /// Set the ID field on the result object
        /// </summary>
        /// <param name="result">Target result object</param>
        /// <param name="id">ID value to set</param>
        protected abstract void SetId(T result, string id);

        /// <summary>
        /// Set the Name field on the result object
        /// </summary>
        /// <param name="result">Target result object</param>
        /// <param name="name">Name value to set</param>
        protected abstract void SetName(T result, string name);

        /// <summary>
        /// Set the Updated field on the result object
        /// </summary>
        /// <param name="result">Target result object</param>
        /// <param name="updated">Updated timestamp to set</param>
        protected abstract void SetUpdated(T result, string updated);

        /// <summary>
        /// Set the Custom data field on the result object
        /// </summary>
        /// <param name="result">Target result object</param>
        /// <param name="custom">Custom data dictionary to set</param>
        protected abstract void SetCustom(T result, Dictionary<string, object> custom);

        /// <summary>
        /// Set the Status field on the result object
        /// </summary>
        /// <param name="result">Target result object</param>
        /// <param name="status">Status value to set</param>
        protected abstract void SetStatus(T result, string status);

        /// <summary>
        /// Set the Type field on the result object
        /// </summary>
        /// <param name="result">Target result object</param>
        /// <param name="type">Type value to set</param>
        protected abstract void SetType(T result, string type);

        #endregion

        #region Virtual Methods - Optional override for subclasses

        /// <summary>
        /// Parse additional fields specific to the metadata type
        /// Override this method to handle type-specific fields
        /// </summary>
        /// <param name="result">Target result object</param>
        /// <param name="dataDict">Source data dictionary</param>
        protected virtual void ParseAdditionalFields(T result, Dictionary<string, object> dataDict)
        {
            // Default implementation does nothing
            // Subclasses can override to parse additional fields
        }

        #endregion
    }
}
