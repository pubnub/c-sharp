using System.Collections.Generic;

namespace PubnubApi.JsonV2.Parsers
{
    /// <summary>
    /// Modern parser for PNSetChannelMetadataResult using the V2 approach
    /// Demonstrates code reuse through MetadataObjectParser base class
    /// Shows how similar parsers share common logic while handling type-specific differences
    /// </summary>
    public class PNSetChannelMetadataResultParser : MetadataObjectParser<PNSetChannelMetadataResult>
    {
        public PNSetChannelMetadataResultParser(IJsonPluggableLibraryV2 jsonLibrary, PubnubLogModule logger) : base(jsonLibrary, logger)
        {
        }

        #region MetadataObjectParser Implementation

        protected override string GetIdFieldName() => "id";

        protected override void SetId(PNSetChannelMetadataResult result, string id)
        {
            // Channel metadata uses "Channel" property name instead of "Id"
            result.Channel = id;
        }

        protected override void SetName(PNSetChannelMetadataResult result, string name)
        {
            result.Name = name;
        }

        protected override void SetUpdated(PNSetChannelMetadataResult result, string updated)
        {
            result.Updated = updated;
        }

        protected override void SetCustom(PNSetChannelMetadataResult result, Dictionary<string, object> custom)
        {
            result.Custom = custom;
        }

        protected override void SetStatus(PNSetChannelMetadataResult result, string status)
        {
            result.Status = status;
        }

        protected override void SetType(PNSetChannelMetadataResult result, string type)
        {
            result.Type = type;
        }

        /// <summary>
        /// Parse Channel-specific additional fields
        /// </summary>
        protected override void ParseAdditionalFields(PNSetChannelMetadataResult result, Dictionary<string, object> dataDict)
        {
            // Channel metadata has Description field that UUID metadata doesn't have
            result.Description = jsonLibrary.GetValue<string>(dataDict, "description");
            
            // Note: Channel metadata doesn't have ExternalId, ProfileUrl, or Email
            // This demonstrates how the base class handles common fields while
            // subclasses handle their specific variations
        }

        #endregion
    }
}
