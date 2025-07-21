using System;

namespace PubnubApi.EndPoint
{
    internal static class UrlParameterConverter
    {
        public static string MapEnumValueToEndpoint(string enumValue)
        {
            string endpointParameterName = enumValue.ToLowerInvariant() switch
            {
                "custom" => "custom",
                "uuid" => "uuid",
                "channel" => "channel",
                "channel_custom" => "channel.custom",
                "uuid_custom" => "uuid.custom",
                "status" => "status",
                "type" => "type",
                "channel_status" => "channel.status",
                "channel_type" => "channel.type",
                "uuid_status" => "uuid.status",
                "uuid_type" => "uuid.type",
                _ => String.Empty
            };

            return endpointParameterName;
        }
    }
}