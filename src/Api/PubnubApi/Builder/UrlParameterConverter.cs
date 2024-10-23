using System;

namespace PubnubApi.EndPoint
{

    internal static class UrlParameterConverter
    {
        public static string MapEnumValueToEndpoint(string enumValue)
        {
            string endpointParameterName = String.Empty;
            if (enumValue.ToLowerInvariant() == "custom")
            {
                endpointParameterName = "custom";
            }
            else if (enumValue.ToLowerInvariant() == "uuid")
            {
                endpointParameterName = "uuid";
            }
            else if (enumValue.ToLowerInvariant() == "channel")
            {
                endpointParameterName = "channel";
            }
            else if (enumValue.ToLowerInvariant() == "channel_custom")
            {
                endpointParameterName = "channel.custom";
            }
            else if (enumValue.ToLowerInvariant() == "uuid_custom")
            {
                endpointParameterName = "uuid.custom";
            }

            return endpointParameterName;
        }
    }
}