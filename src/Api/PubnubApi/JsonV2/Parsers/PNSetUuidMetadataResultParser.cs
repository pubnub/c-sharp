using System.Collections.Generic;

namespace PubnubApi.JsonV2.Parsers
{
    /// <summary>
    /// Modern parser for PNSetUuidMetadataResult using the V2 approach
    /// Demonstrates code reuse through MetadataObjectParser base class
    /// </summary>
    public class PNSetUuidMetadataResultParser : MetadataObjectParser<PNSetUuidMetadataResult>
    {
        public PNSetUuidMetadataResultParser(IJsonPluggableLibraryV2 jsonLibrary, PubnubLogModule logger) : base(jsonLibrary, logger)
        {
        }

        #region MetadataObjectParser Implementation

        protected override string GetIdFieldName() => "id";

        protected override void SetId(PNSetUuidMetadataResult result, string id)
        {
            result.Uuid = id;
        }

        protected override void SetName(PNSetUuidMetadataResult result, string name)
        {
            result.Name = name;
        }

        protected override void SetUpdated(PNSetUuidMetadataResult result, string updated)
        {
            result.Updated = updated;
        }

        protected override void SetCustom(PNSetUuidMetadataResult result, Dictionary<string, object> custom)
        {
            result.Custom = custom;
        }

        protected override void SetStatus(PNSetUuidMetadataResult result, string status)
        {
            result.Status = status;
        }

        protected override void SetType(PNSetUuidMetadataResult result, string type)
        {
            result.Type = type;
        }

        /// <summary>
        /// Parse UUID-specific additional fields
        /// </summary>
        protected override void ParseAdditionalFields(PNSetUuidMetadataResult result, Dictionary<string, object> dataDict)
        {
            // UUID metadata has these additional fields not present in Channel metadata
            result.ExternalId = jsonLibrary.GetValue<string>(dataDict, "externalId");
            result.ProfileUrl = jsonLibrary.GetValue<string>(dataDict, "profileUrl");
            result.Email = jsonLibrary.GetValue<string>(dataDict, "email");
        }

        #endregion
    }
}
