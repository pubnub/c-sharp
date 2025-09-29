using System.Collections.Generic;

namespace PubnubApi.JsonV2.Parsers
{
    /// <summary>
    /// Modern parser for PNHistoryResult using the V2 approach
    /// Demonstrates efficient array parsing and flexible message format handling
    /// </summary>
    public class PNHistoryResultParser : IModernJsonParser<PNHistoryResult>
    {
        private readonly IJsonPluggableLibraryV2 jsonLibrary;
        private readonly PubnubLogModule logger;

        public PNHistoryResultParser(IJsonPluggableLibraryV2 jsonLibrary, PubnubLogModule logger)
        {
            this.jsonLibrary = jsonLibrary ?? throw new System.ArgumentNullException(nameof(jsonLibrary));
            this.logger = logger;
        }

        public PNHistoryResult Parse(List<object> listObject)
        {
            var result = new PNHistoryResult();

            if (listObject == null || listObject.Count == 0)
                return result;

            // Extract timetokens from array positions 1 and 2 
            if (listObject.Count >= 2)
            {
                result.StartTimeToken = SharedParsingHelpers.ParseTimetoken(listObject[1]);
            }
            
            if (listObject.Count >= 3)
            {
                result.EndTimeToken = SharedParsingHelpers.ParseTimetoken(listObject[2]);
            }

            // Extract messages array from position 0
            var messagesContainer = SharedParsingHelpers.NormalizeToList(
                jsonLibrary.GetArrayElement(listObject, 0));

            if (messagesContainer.Count > 0)
            {
                result.Messages = ParseHistoryMessages(messagesContainer);
            }

            return result;
        }

        public PNHistoryResult Parse(string jsonString)
        {
            var listObject = jsonLibrary.DeserializeToListOfObject(jsonString);
            return Parse(listObject);
        }

        public PNHistoryResult Parse(IDictionary<string, object> jsonData)
        {
            var listObject = new List<object> { jsonData };
            return Parse(listObject);
        }

        /// <summary>
        /// Parses the messages array, handling different message formats
        /// </summary>
        private List<PNHistoryItemResult> ParseHistoryMessages(List<object> messagesContainer)
        {
            var messages = new List<PNHistoryItemResult>();

            foreach (var messageItem in messagesContainer)
            {
                var historyItem = ParseSingleHistoryItem(messageItem);
                messages.Add(historyItem);
            }

            return messages;
        }

        /// <summary>
        /// Parses a single history item, handling both structured and simple message formats
        /// </summary>
        private PNHistoryItemResult ParseSingleHistoryItem(object messageItem)
        {
            var result = new PNHistoryItemResult();

            // Try to convert to dictionary for structured message format
            var messageDict = jsonLibrary.GetDictionary(messageItem);

            if (IsStructuredMessage(messageDict))
            {
                // Structured format: {"message": ..., "timetoken": ..., "meta": ..., etc}
                result.Entry = jsonLibrary.GetValue<object>(messageDict, "message");
                result.Timetoken = SharedParsingHelpers.ParseTimetoken(jsonLibrary, messageDict, "timetoken");
                
                // Only set Meta if the key exists, to match V1 behavior (V1 leaves it null otherwise)
                if (jsonLibrary.HasProperty(messageDict, "meta"))
                {
                    result.Meta = jsonLibrary.GetDictionary(messageDict, "meta");
                }
                
                result.Uuid = SharedParsingHelpers.ParseString(jsonLibrary, messageDict, "uuid");
                result.MessageType = SharedParsingHelpers.ParseInteger(jsonLibrary, messageDict, "message_type");
            }
            else if (messageDict.Count > 0)
            {
                // Dictionary format but not structured - use entire dictionary as entry
                result.Entry = messageDict;
            }
            else
            {
                // Simple format - use raw message as entry
                result.Entry = messageItem;
            }

            return result;
        }

        /// <summary>
        /// Determines if a message is in structured format
        /// Structured messages have "message" field and either "timetoken" or "meta"
        /// </summary>
        private bool IsStructuredMessage(Dictionary<string, object> messageDict)
        {
            if (messageDict == null || messageDict.Count == 0)
                return false;

            return jsonLibrary.HasProperty(messageDict, "message") &&
                   (jsonLibrary.HasProperty(messageDict, "timetoken") || 
                    jsonLibrary.HasProperty(messageDict, "meta"));
        }
    }
}
