using System.Collections.Generic;

namespace PubnubApi.JsonV2.Parsers
{
    /// <summary>
    /// Modern parser for PNAccessManagerGrantResult using the V2 approach
    /// Demonstrates significant code reduction through strategic use of helper classes
    /// and the Strategy pattern for handling different section types
    /// </summary>
    public class PNAccessManagerGrantResultParser : IModernJsonParser<PNAccessManagerGrantResult>
    {
        private readonly IJsonPluggableLibraryV2 jsonLibrary;
        private readonly PubnubLogModule logger;

        // Strategy pattern instances for different section types
        private static readonly AccessManagerHelpers.ChannelsParser channelsParser = new AccessManagerHelpers.ChannelsParser();
        private static readonly AccessManagerHelpers.ChannelGroupsParser channelGroupsParser = new AccessManagerHelpers.ChannelGroupsParser();
        private static readonly AccessManagerHelpers.UuidsParser uuidsParser = new AccessManagerHelpers.UuidsParser();

        public PNAccessManagerGrantResultParser(IJsonPluggableLibraryV2 jsonLibrary, PubnubLogModule logger)
        {
            this.jsonLibrary = jsonLibrary ?? throw new System.ArgumentNullException(nameof(jsonLibrary));
            this.logger = logger;
        }

        public PNAccessManagerGrantResult Parse(List<object> listObject)
        {
            if (listObject == null || listObject.Count == 0)
                return null;

            // Extract the main grant dictionary from listObject[0]
            var grantDict = jsonLibrary.GetDictionary(
                jsonLibrary.GetArrayElement(listObject, 0));

            if (grantDict.Count == 0)
                return null;

            var result = new PNAccessManagerGrantResult();

            // Extract payload section
            var payloadDict = jsonLibrary.GetDictionary(grantDict, "payload");
            if (payloadDict.Count == 0)
                return result;

            // Parse basic payload fields
            ParseBasicPayloadFields(result, payloadDict);

            // Skip detailed parsing for subkey level
            if (result.Level == "subkey")
                return result;

            // Parse all access manager sections using strategy pattern
            ParseAllAccessManagerSections(result, payloadDict);

            return result;
        }

        public PNAccessManagerGrantResult Parse(string jsonString)
        {
            var listObject = jsonLibrary.DeserializeToListOfObject(jsonString);
            return Parse(listObject);
        }

        public PNAccessManagerGrantResult Parse(IDictionary<string, object> jsonData)
        {
            var listObject = new List<object> { jsonData };
            return Parse(listObject);
        }

        /// <summary>
        /// Parses basic payload fields like level, subscribe_key, ttl
        /// </summary>
        private void ParseBasicPayloadFields(PNAccessManagerGrantResult result, Dictionary<string, object> payloadDict)
        {
            result.Level = SharedParsingHelpers.ParseString(jsonLibrary, payloadDict, "level");
            result.SubscribeKey = SharedParsingHelpers.ParseString(jsonLibrary, payloadDict, "subscribe_key");
            result.Ttl = SharedParsingHelpers.ParseInteger(jsonLibrary, payloadDict, "ttl");
        }

        /// <summary>
        /// Parses all access manager sections (channels, channel-groups, uuids) using strategy pattern
        /// This replaces ~200 lines of repetitive code with a clean, maintainable approach
        /// </summary>
        private void ParseAllAccessManagerSections(PNAccessManagerGrantResult result, Dictionary<string, object> payloadDict)
        {
            // Parse channels section
            AccessManagerHelpers.ParseAccessManagerSection(
                jsonLibrary, payloadDict, channelsParser, result);

            // Parse channel-groups section  
            AccessManagerHelpers.ParseAccessManagerSection(
                jsonLibrary, payloadDict, channelGroupsParser, result);

            // Parse uuids section
            AccessManagerHelpers.ParseAccessManagerSection(
                jsonLibrary, payloadDict, uuidsParser, result);
        }
    }
}
