using System.Collections.Generic;

namespace PubnubApi.JsonV2.Parsers
{
    /// <summary>
    /// Helper methods specific to Access Manager parsing patterns
    /// Encapsulates the complex logic for parsing permissions and auth structures
    /// </summary>
    internal static class AccessManagerHelpers
    {
        /// <summary>
        /// Interface for parsing different access manager sections (channels, channel-groups, uuids)
        /// Allows the same parsing logic to be reused for all three types
        /// </summary>
        public interface IAccessManagerSectionParser
        {
            string GetSectionKey();
            string GetSingleItemKey();
            void AssignToResult(PNAccessManagerGrantResult result, Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> sectionData);
        }

        /// <summary>
        /// Parser for channels section
        /// </summary>
        public class ChannelsParser : IAccessManagerSectionParser
        {
            public string GetSectionKey() => "channels";
            public string GetSingleItemKey() => "channel";
            public void AssignToResult(PNAccessManagerGrantResult result, Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> sectionData)
            {
                result.Channels = sectionData;
            }
        }

        /// <summary>
        /// Parser for channel-groups section
        /// </summary>
        public class ChannelGroupsParser : IAccessManagerSectionParser
        {
            public string GetSectionKey() => "channel-groups";
            public string GetSingleItemKey() => "channel-group";
            public void AssignToResult(PNAccessManagerGrantResult result, Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> sectionData)
            {
                result.ChannelGroups = sectionData;
            }
        }

        /// <summary>
        /// Parser for uuids section
        /// </summary>
        public class UuidsParser : IAccessManagerSectionParser
        {
            public string GetSectionKey() => "uuids";
            public string GetSingleItemKey() => "uuid";
            public void AssignToResult(PNAccessManagerGrantResult result, Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> sectionData)
            {
                result.Uuids = sectionData;
            }
        }

        /// <summary>
        /// Parses a complete access manager section (channels, channel-groups, or uuids)
        /// Handles both multi-item and single-item formats
        /// </summary>
        public static void ParseAccessManagerSection(
            IJsonPluggableLibraryV2 jsonLibrary,
            Dictionary<string, object> payloadDict,
            IAccessManagerSectionParser sectionParser,
            PNAccessManagerGrantResult result)
        {
            var sectionKey = sectionParser.GetSectionKey();
            var singleItemKey = sectionParser.GetSingleItemKey();

            // Check for multi-item format (e.g., "channels")
            if (jsonLibrary.HasProperty(payloadDict, sectionKey))
            {
                var sectionData = ParseMultiItemSection(jsonLibrary, payloadDict, sectionKey);
                if (sectionData.Count > 0)
                {
                    sectionParser.AssignToResult(result, sectionData);
                }
            }
            // Check for single-item format (e.g., "channel")
            else if (jsonLibrary.HasProperty(payloadDict, singleItemKey))
            {
                var sectionData = ParseSingleItemSection(jsonLibrary, payloadDict, singleItemKey);
                if (sectionData.Count > 0)
                {
                    sectionParser.AssignToResult(result, sectionData);
                }
            }
        }

        /// <summary>
        /// Parses multi-item section format: {"channels": {"ch1": {...}, "ch2": {...}}}
        /// </summary>
        private static Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> ParseMultiItemSection(
            IJsonPluggableLibraryV2 jsonLibrary, 
            Dictionary<string, object> payloadDict, 
            string sectionKey)
        {
            var result = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();
            var sectionDict = jsonLibrary.GetDictionary(payloadDict, sectionKey);

            // Handle REST API bug where section value might be a string instead of dictionary
            if (sectionDict.Count == 0)
            {
                var sectionValue = SharedParsingHelpers.ParseString(jsonLibrary, payloadDict, sectionKey);
                if (!string.IsNullOrEmpty(sectionValue))
                {
                    // Treat string value as single item name and parse auths from payload level
                    var authDict = ParseAuthKeyData(jsonLibrary, payloadDict);
                    if (authDict.Count > 0)
                    {
                        result[sectionValue] = authDict;
                    }
                }
                return result;
            }

            foreach (var itemName in sectionDict.Keys)
            {
                var itemDict = jsonLibrary.GetDictionary(sectionDict, itemName);
                var authDict = ParseAuthKeyData(jsonLibrary, itemDict);
                
                if (authDict.Count > 0)
                {
                    result[itemName] = authDict;
                }
            }

            return result;
        }

        /// <summary>
        /// Parses single-item section format: {"channel": "channelName", "auths": {...}}
        /// </summary>
        private static Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> ParseSingleItemSection(
            IJsonPluggableLibraryV2 jsonLibrary,
            Dictionary<string, object> payloadDict,
            string singleItemKey)
        {
            var result = new Dictionary<string, Dictionary<string, PNAccessManagerKeyData>>();
            var itemName = SharedParsingHelpers.ParseString(jsonLibrary, payloadDict, singleItemKey);

            if (!string.IsNullOrEmpty(itemName))
            {
                var authDict = ParseAuthKeyData(jsonLibrary, payloadDict);
                if (authDict.Count > 0)
                {
                    result[itemName] = authDict;
                }
            }

            return result;
        }

        /// <summary>
        /// Parses auth key data from an "auths" section
        /// </summary>
        public static Dictionary<string, PNAccessManagerKeyData> ParseAuthKeyData(
            IJsonPluggableLibraryV2 jsonLibrary, 
            Dictionary<string, object> source)
        {
            var result = new Dictionary<string, PNAccessManagerKeyData>();
            var authsDict = jsonLibrary.GetDictionary(source, "auths");

            foreach (var authKey in authsDict.Keys)
            {
                var authDataDict = jsonLibrary.GetDictionary(authsDict, authKey);
                if (authDataDict.Count > 0)
                {
                    var keyData = ParseAccessManagerKeyData(jsonLibrary, authDataDict);
                    result[authKey] = keyData;
                }
            }

            return result;
        }

        /// <summary>
        /// Parses individual PNAccessManagerKeyData from permission flags
        /// </summary>
        public static PNAccessManagerKeyData ParseAccessManagerKeyData(
            IJsonPluggableLibraryV2 jsonLibrary, 
            Dictionary<string, object> authDataDict)
        {
            return new PNAccessManagerKeyData
            {
                ReadEnabled = SharedParsingHelpers.ParseBoolean(jsonLibrary, authDataDict, "r"),
                WriteEnabled = SharedParsingHelpers.ParseBoolean(jsonLibrary, authDataDict, "w"),
                ManageEnabled = SharedParsingHelpers.ParseBoolean(jsonLibrary, authDataDict, "m"),
                DeleteEnabled = SharedParsingHelpers.ParseBoolean(jsonLibrary, authDataDict, "d"),
                GetEnabled = SharedParsingHelpers.ParseBoolean(jsonLibrary, authDataDict, "g"),
                UpdateEnabled = SharedParsingHelpers.ParseBoolean(jsonLibrary, authDataDict, "u"),
                JoinEnabled = SharedParsingHelpers.ParseBoolean(jsonLibrary, authDataDict, "j")
            };
        }
    }
}
