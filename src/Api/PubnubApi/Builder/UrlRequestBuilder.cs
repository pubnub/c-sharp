using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;
#if !NETSTANDARD10 && !NETSTANDARD11 && !NETSTANDARD12 && !WP81
using System.Reflection;
#endif
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi
{
    public sealed class UrlRequestBuilder : IUrlRequestBuilder
    {
        private ConcurrentDictionary<string, PNConfiguration> pubnubConfig { get; } = new ConcurrentDictionary<string, PNConfiguration>();
        private readonly IJsonPluggableLibrary jsonLib ;
        private readonly IPubnubUnitTest pubnubUnitTest;
        private readonly IPubnubLog pubnubLog;
        private readonly string pubnubInstanceId;
        private readonly EndPoint.TelemetryManager telemetryMgr;
        private readonly EndPoint.TokenManager tokenMgr;

        public UrlRequestBuilder(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest, IPubnubLog log, EndPoint.TelemetryManager pubnubTelemetryMgr, EndPoint.TokenManager pubnubTokenMgr, string pnInstanceId)
        {
            pubnubConfig.AddOrUpdate(pnInstanceId, config, (k, o) => config);
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubUnitTest = pubnubUnitTest;
            this.pubnubLog = log;
            this.telemetryMgr = pubnubTelemetryMgr;
            this.tokenMgr = pubnubTokenMgr;
            this.pubnubInstanceId = string.IsNullOrEmpty(pnInstanceId) ? "" : pnInstanceId;
        }

        Uri IUrlRequestBuilder.BuildTimeRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNTimeOperation;

            List<string> url = new List<string>();
            url.Add("time");
            url.Add("0");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildMultiChannelSubscribeRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, long timetoken, int region, string channelsJsonState, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNSubscribeOperation;
            string channelForUrl = (channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("subscribe");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add(channelForUrl);
            url.Add("0");

            Dictionary<string, string> internalInitialSubscribeUrlParams = new Dictionary<string, string>();
            if (initialSubscribeUrlParams != null)
            {
                internalInitialSubscribeUrlParams = initialSubscribeUrlParams;
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>(internalInitialSubscribeUrlParams);

            if (!requestQueryStringParams.ContainsKey("filter-expr") && pubnubConfig.ContainsKey(pubnubInstanceId) && !string.IsNullOrEmpty(pubnubConfig[pubnubInstanceId].FilterExpression))
            {
                requestQueryStringParams.Add("filter-expr", UriUtil.EncodeUriComponent(pubnubConfig[pubnubInstanceId].FilterExpression, currentType, false, false, false));
            }

            if (!requestQueryStringParams.ContainsKey("tt"))
            {
                requestQueryStringParams.Add("tt", timetoken.ToString(CultureInfo.InvariantCulture));
            }

            if (!requestQueryStringParams.ContainsKey("tr") && region > 0)
            {
                requestQueryStringParams.Add("tr", region.ToString(CultureInfo.InvariantCulture));
            }

            if (pubnubConfig.ContainsKey(pubnubInstanceId) && pubnubConfig[pubnubInstanceId].PresenceTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig[pubnubInstanceId].PresenceTimeout.ToString(CultureInfo.InvariantCulture));
            }

            if (channelGroups != null && channelGroups.Length > 0 && channelGroups[0] != "")
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), currentType, false, false, false));
            }

            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(channelsJsonState, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildMultiChannelLeaveRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, string jsonUserState, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.Leave;
            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");
            url.Add(multiChannel);
            url.Add("leave");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (pubnubConfig.ContainsKey(pubnubInstanceId) && pubnubConfig[pubnubInstanceId].PresenceTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig[pubnubInstanceId].PresenceTimeout.ToString(CultureInfo.InvariantCulture));
            }

            string channelsJsonState = jsonUserState;
            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(channelsJsonState, currentType, false, false, false));
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()),currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildPublishRequest(string requestMethod, string requestBody, string channel, object originalMessage, bool storeInHistory, int ttl, Dictionary<string, object> userMetaData, Dictionary<string, string> additionalUrlParams, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNPublishOperation;

            List<string> url = new List<string>();
            url.Add("publish");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].PublishKey : "");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("0");
            url.Add(channel);
            url.Add("0");
            if (requestMethod.ToUpperInvariant() == "GET")
            {
                string message = JsonEncodePublishMsg(originalMessage, currentType);
                url.Add(message);
            }

            Dictionary<string, string> additionalUrlParamsDic = new Dictionary<string, string>();
            if (additionalUrlParams != null)
            {
                additionalUrlParamsDic = additionalUrlParams;
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>(additionalUrlParamsDic);

            if (userMetaData != null)
            {
                string jsonMetaData = jsonLib.SerializeToJsonString(userMetaData);
                requestQueryStringParams.Add("meta", UriUtil.EncodeUriComponent(jsonMetaData, currentType, false, false, false));
            }

            if (storeInHistory && ttl >= 0)
            {
                requestQueryStringParams.Add("tt1", ttl.ToString(CultureInfo.InvariantCulture));
            }

            if (!storeInHistory)
            {
                requestQueryStringParams.Add("store", "0");
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            bool allowPAMv3Sign = requestMethod.ToUpperInvariant() != "POST";
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, allowPAMv3Sign);
        }

        Uri IUrlRequestBuilder.BuildSignalRequest(string requestMethod, string requestBody, string channel, object originalMessage, Dictionary<string, object> userMetaData, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNSignalOperation;

            List<string> url = new List<string>();
            url.Add("signal");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].PublishKey : "");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("0");
            url.Add(channel);
            url.Add("0");
            if (requestMethod.ToUpperInvariant() == "GET")
            {
                string message = JsonEncodePublishMsg(originalMessage, currentType);
                url.Add(message);
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (userMetaData != null)
            {
                string jsonMetaData = jsonLib.SerializeToJsonString(userMetaData);
                requestQueryStringParams.Add("meta", UriUtil.EncodeUriComponent(jsonMetaData, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildHereNowRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNHereNowOperation;
            string channel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            if (!string.IsNullOrEmpty(channel))
            {
                url.Add("channel");
                url.Add(channel);
            }

            int disableUUID = showUUIDList ? 0 : 1;
            int userState = includeUserState ? 1 : 0;

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            string commaDelimitedchannelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";
            if (!string.IsNullOrEmpty(commaDelimitedchannelGroup) && commaDelimitedchannelGroup.Trim().Length > 0)
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(commaDelimitedchannelGroup, currentType, false, false, false));
            }

            requestQueryStringParams.Add("disable_uuids", disableUUID.ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("state", userState.ToString(CultureInfo.InvariantCulture));

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildHistoryRequest(string requestMethod, string requestBody, string channel, long start, long end, int count, bool reverse, bool includeToken, bool includeMeta, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNHistoryOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("history");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");
            url.Add(channel);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("count", (count <= -1) ? "100" : count.ToString(CultureInfo.InvariantCulture));

            if (reverse)
            {
                requestQueryStringParams.Add("reverse", "true");
            }
            if (start != -1)
            {
                requestQueryStringParams.Add("start", start.ToString(CultureInfo.InvariantCulture));
            }
            if (end != -1)
            {
                requestQueryStringParams.Add("end", end.ToString(CultureInfo.InvariantCulture));
            }

            if (includeToken)
            {
                requestQueryStringParams.Add("include_token", "true");
            }

            if (includeMeta)
            {
                requestQueryStringParams.Add("include_meta", "true");
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach(KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildFetchRequest(string requestMethod, string requestBody, string[] channels, long start, long end, int count, bool reverse, bool includeMeta, bool includeMessageActions, bool includeUuid, bool includeMessageType, Dictionary<string, object> externalQueryParam)
        {
            string channel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";

            PNOperationType currentType = PNOperationType.PNFetchHistoryOperation;

            List<string> url = new List<string>();
            url.Add("v3");
            url.Add(includeMessageActions ? "history-with-actions" : "history");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");
            url.Add(channel);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("max", (count <= -1) ? (includeMessageActions || (channels != null && channels.Length > 1) ? "25" : "100") : count.ToString(CultureInfo.InvariantCulture));

            if (reverse)
            {
                requestQueryStringParams.Add("reverse", "true");
            }
            if (start != -1)
            {
                requestQueryStringParams.Add("start", start.ToString(CultureInfo.InvariantCulture));
            }
            if (end != -1)
            {
                requestQueryStringParams.Add("end", end.ToString(CultureInfo.InvariantCulture));
            }

            if (includeMeta)
            {
                requestQueryStringParams.Add("include_meta", "true");
            }

            if (includeUuid)
            {
                requestQueryStringParams.Add("include_uuid", "true");
            }

            if (includeMessageType)
            {
                requestQueryStringParams.Add("include_message_type", "true");
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildMessageCountsRequest(string requestMethod, string requestBody, string[] channels, long[] timetokens, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNMessageCountsOperation;
            string channel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : "";

            List<string> url = new List<string>();
            url.Add("v3");
            url.Add("history");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("message-counts");
            if (!string.IsNullOrEmpty(channel))
            {
                url.Add(UriUtil.EncodeUriComponent(channel, currentType, true, false, false));
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (timetokens != null && timetokens.Length > 0)
            {
                string tt = string.Join(",", timetokens.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
                if (timetokens.Length == 1)
                {
                    requestQueryStringParams.Add("timetoken", tt);
                }
                else
                {
                    requestQueryStringParams.Add("channelsTimetoken", UriUtil.EncodeUriComponent(tt, currentType, false, false, false));
                }
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildDeleteMessageRequest(string requestMethod, string requestBody, string channel, long start, long end, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNDeleteMessageOperation;

            List<string> url = new List<string>();
            url.Add("v3");
            url.Add("history");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");
            url.Add(channel);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (start != -1)
            {
                requestQueryStringParams.Add("start", start.ToString(CultureInfo.InvariantCulture));
            }
            if (end != -1)
            {
                requestQueryStringParams.Add("end", end.ToString(CultureInfo.InvariantCulture));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildWhereNowRequest(string requestMethod, string requestBody, string uuid, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNWhereNowOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("uuid");
            url.Add(uuid);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGrantV2AccessRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string targetUuidsCommaDelimited, string authKeysCommaDelimited, bool read, bool write, bool delete, bool manage, bool get, bool update, bool join, long ttl, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAccessManagerGrant;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("auth");
            url.Add("grant");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(authKeysCommaDelimited))
            {
                requestQueryStringParams.Add("auth", UriUtil.EncodeUriComponent(authKeysCommaDelimited, currentType, false, false, false));
            }

            if (!string.IsNullOrEmpty(channelsCommaDelimited))
            {
                requestQueryStringParams.Add("channel", UriUtil.EncodeUriComponent(channelsCommaDelimited, currentType, false, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited))
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(channelGroupsCommaDelimited, currentType, false, false, false));
            }

            if (!string.IsNullOrEmpty(targetUuidsCommaDelimited))
            {
                requestQueryStringParams.Add("target-uuid", UriUtil.EncodeUriComponent(targetUuidsCommaDelimited, currentType, false, false, false));
            }

            if (ttl > -1)
            {
                requestQueryStringParams.Add("ttl", ttl.ToString(CultureInfo.InvariantCulture));
            }

            requestQueryStringParams.Add("r", Convert.ToInt32(read).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("w", Convert.ToInt32(write).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("d", Convert.ToInt32(delete).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("m", Convert.ToInt32(manage).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("g", Convert.ToInt32(get).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("u", Convert.ToInt32(update).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("j", Convert.ToInt32(join).ToString(CultureInfo.InvariantCulture));

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGrantV3AccessRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAccessManagerGrantToken;

            List<string> url = new List<string>();
            url.Add("v3");
            url.Add("pam");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("grant");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildRevokeV3AccessRequest(string requestMethod, string requestBody, string token, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAccessManagerRevokeToken;

            List<string> url = new List<string>();
            url.Add("v3");
            url.Add("pam");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("grant");
            url.Add(token);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildAuditAccessRequest(string requestMethod, string requestBody, string channel, string channelGroup, string authKeysCommaDelimited, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAccessManagerAudit;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("auth");
            url.Add("audit");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(authKeysCommaDelimited))
            {
                requestQueryStringParams.Add("auth", UriUtil.EncodeUriComponent(authKeysCommaDelimited, currentType, false, false, false));
            }

            if (!string.IsNullOrEmpty(channel))
            {
                requestQueryStringParams.Add("channel", UriUtil.EncodeUriComponent(channel, currentType, false, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(channelGroup, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetUserStateRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetStateOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");

            if (string.IsNullOrEmpty(channelsCommaDelimited) || channelsCommaDelimited.Trim().Length <= 0)
            {
                url.Add(",");
            }
            else
            {
                url.Add(channelsCommaDelimited);
            }

            url.Add("uuid");
            url.Add(uuid);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited) && channelGroupsCommaDelimited.Trim().Length > 0)
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(channelGroupsCommaDelimited, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildSetUserStateRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNSetStateOperation;
            string internalChannelsCommaDelimited;

            if (string.IsNullOrEmpty(channelsCommaDelimited) || channelsCommaDelimited.Trim().Length <= 0)
            {
                internalChannelsCommaDelimited = ",";
            }
            else
            {
                internalChannelsCommaDelimited = channelsCommaDelimited;
            }

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");
            url.Add(internalChannelsCommaDelimited);
            url.Add("uuid");
            url.Add(uuid);
            url.Add("data");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited) && channelGroupsCommaDelimited.Trim().Length > 0)
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(jsonUserState, currentType, false, false, false));
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(channelGroupsCommaDelimited, currentType, false, false, false));
            }
            else
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(jsonUserState, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildAddChannelsToChannelGroupRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAddChannelsToGroupOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            if (!string.IsNullOrEmpty(nameSpace) && nameSpace.Trim().Length > 0)
            {
                url.Add("namespace");
                url.Add(nameSpace);
            }
            url.Add("channel-group");
            url.Add(groupName);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("add", UriUtil.EncodeUriComponent(channelsCommaDelimited, currentType,false, false, false));

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildRemoveChannelsFromChannelGroupRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNRemoveGroupOperation;

            bool groupNameAvailable = false;
            bool nameSpaceAvailable = false;
            bool channelAvaiable = false;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            if (!string.IsNullOrEmpty(nameSpace) && nameSpace.Trim().Length > 0)
            {
                nameSpaceAvailable = true;
                url.Add("namespace");
                url.Add(nameSpace);
            }

            if (!string.IsNullOrEmpty(groupName) && groupName.Trim().Length > 0)
            {
                groupNameAvailable = true;
                url.Add("channel-group");
                url.Add(groupName);
            }

            if (!String.IsNullOrEmpty(channelsCommaDelimited))
            {
                channelAvaiable = true;
            }

            if (nameSpaceAvailable && groupNameAvailable && !channelAvaiable)
            {
                url.Add("remove");
            }
            else if (nameSpaceAvailable && !groupNameAvailable && !channelAvaiable)
            {
                url.Add("remove");
            }
            else if (!nameSpaceAvailable && groupNameAvailable && !channelAvaiable)
            {
                url.Add("remove");
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (channelAvaiable)
            {
                requestQueryStringParams.Add("remove", UriUtil.EncodeUriComponent(channelsCommaDelimited, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetChannelsForChannelGroupRequest(string requestMethod, string requestBody, string nameSpace, string groupName, bool limitToChannelGroupScopeOnly, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.ChannelGroupGet;

            bool groupNameAvailable = false;
            bool nameSpaceAvailable = false;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            if (!string.IsNullOrEmpty(nameSpace) && nameSpace.Trim().Length > 0)
            {
                nameSpaceAvailable = true;
                url.Add("namespace");
                url.Add(nameSpace);
            }
            if (limitToChannelGroupScopeOnly)
            {
                url.Add("channel-group");
            }
            else
            {
                if (!string.IsNullOrEmpty(groupName) && groupName.Trim().Length > 0)
                {
                    groupNameAvailable = true;
                    url.Add("channel-group");
                    url.Add(groupName);
                }

                if (!nameSpaceAvailable && !groupNameAvailable)
                {
                    url.Add("namespace");
                }
                else if (nameSpaceAvailable && !groupNameAvailable)
                {
                    url.Add("channel-group");
                }
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetAllChannelGroupRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.ChannelGroupAllGet;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel-group");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildRegisterDevicePushRequest(string requestMethod, string requestBody, string channel, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PushRegister;

            List<string> url = new List<string>();
            if (pushType == PNPushType.APNS2)
            {
                url.Add("v2");
                url.Add("push");
                url.Add("sub-key");
                url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
                url.Add("devices-apns2");
                url.Add(pushToken);
            }
            else
            {
                url.Add("v1");
                url.Add("push");
                url.Add("sub-key");
                url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
                url.Add("devices");
                url.Add(pushToken);
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (pushType == PNPushType.APNS2)
            {
                requestQueryStringParams.Add("environment", environment.ToString().ToLowerInvariant());
                requestQueryStringParams.Add("topic", UriUtil.EncodeUriComponent(deviceTopic, currentType, false, false, false));
            }
            else
            {
                requestQueryStringParams.Add("type", pushType.ToString().ToLowerInvariant());
            }
            requestQueryStringParams.Add("add", UriUtil.EncodeUriComponent(channel, currentType, false, false, false));

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildUnregisterDevicePushRequest(string requestMethod, string requestBody, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PushUnregister;

            List<string> url = new List<string>();
            if (pushType == PNPushType.APNS2)
            {
                url.Add("v2");
                url.Add("push");
                url.Add("sub-key");
                url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
                url.Add("devices-apns2");
                url.Add(pushToken);
                url.Add("remove");
            }
            else
            {
                url.Add("v1");
                url.Add("push");
                url.Add("sub-key");
                url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
                url.Add("devices");
                url.Add(pushToken);
                url.Add("remove");
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (pushType == PNPushType.APNS2)
            {
                requestQueryStringParams.Add("environment", environment.ToString().ToLowerInvariant());
                requestQueryStringParams.Add("topic", UriUtil.EncodeUriComponent(deviceTopic, currentType, false, false, false));
            }
            else
            {
                requestQueryStringParams.Add("type", pushType.ToString().ToLowerInvariant());
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildRemoveChannelPushRequest(string requestMethod, string requestBody, string channel, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PushRemove;

            List<string> url = new List<string>();
            if (pushType == PNPushType.APNS2)
            {
                url.Add("v2");
                url.Add("push");
                url.Add("sub-key");
                url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
                url.Add("devices-apns2");
                url.Add(pushToken);
            }
            else
            {
                url.Add("v1");
                url.Add("push");
                url.Add("sub-key");
                url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
                url.Add("devices");
                url.Add(pushToken);
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (pushType == PNPushType.APNS2)
            {
                requestQueryStringParams.Add("environment", environment.ToString().ToLowerInvariant());
                requestQueryStringParams.Add("topic", UriUtil.EncodeUriComponent(deviceTopic, currentType, false, false, false));
            }
            else
            {
                requestQueryStringParams.Add("type", pushType.ToString().ToLowerInvariant());
            }
            requestQueryStringParams.Add("remove", UriUtil.EncodeUriComponent(channel, currentType, false, false, false));

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetChannelsPushRequest(string requestMethod, string requestBody, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PushGet;

            List<string> url = new List<string>();
            if (pushType == PNPushType.APNS2)
            {
                url.Add("v2");
                url.Add("push");
                url.Add("sub-key");
                url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
                url.Add("devices-apns2");
                url.Add(pushToken);
            }
            else
            {
                url.Add("v1");
                url.Add("push");
                url.Add("sub-key");
                url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
                url.Add("devices");
                url.Add(pushToken);
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (pushType == PNPushType.APNS2)
            {
                requestQueryStringParams.Add("environment", environment.ToString().ToLowerInvariant());
                requestQueryStringParams.Add("topic", UriUtil.EncodeUriComponent(deviceTopic, currentType, false, false, false));
            }
            else
            {
                requestQueryStringParams.Add("type", pushType.ToString().ToLowerInvariant());
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildPresenceHeartbeatRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, string jsonUserState)
        {
            PNOperationType currentType = PNOperationType.PNHeartbeatOperation;

            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");
            url.Add(multiChannel);
            url.Add("heartbeat");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            string channelsJsonState = jsonUserState;
            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(channelsJsonState, currentType, false, false, false));
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), currentType, false, false, false));
            }

            if (pubnubConfig.ContainsKey(pubnubInstanceId) && pubnubConfig[pubnubInstanceId].PresenceTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig[pubnubInstanceId].PresenceTimeout.ToString(CultureInfo.InvariantCulture));
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildSetUuidMetadataRequest(string requestMethod, string requestBody, string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNSetUuidMetadataOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("uuids");
            url.Add(string.IsNullOrEmpty(uuid) ? "" : uuid);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (includeCustom)
            {
                requestQueryStringParams.Add("include", "custom");
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildDeleteUuidMetadataRequest(string requestMethod, string requestBody, string uuid, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNDeleteUuidMetadataOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("uuids");
            url.Add(string.IsNullOrEmpty(uuid) ? "" : uuid);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetAllUuidMetadataRequest(string requestMethod, string requestBody, string start, string end, int limit, bool includeCount, bool includeCustom, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetAllUuidMetadataOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("uuids");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(end, currentType, false, false, false));
            }
            if (limit >= 0)
            {
                requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }
            if (includeCount)
            {
                requestQueryStringParams.Add("count", "true");
            }
            if (includeCustom)
            {
                requestQueryStringParams.Add("include", "custom");
            }
            if (!string.IsNullOrEmpty(filter))
            {
                requestQueryStringParams.Add("filter", UriUtil.EncodeUriComponent(filter, currentType, false, false, false));
            }
            if (sort != null && sort.Count > 0)
            {
                requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",",sort.ToArray()), currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetSingleUuidMetadataRequest(string requestMethod, string requestBody, string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetUuidMetadataOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("uuids");
            url.Add(string.IsNullOrEmpty(uuid) ? "": uuid);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (includeCustom)
            {
                requestQueryStringParams.Add("include", "custom");
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildSetChannelMetadataRequest(string requestMethod, string requestBody, string channel, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNSetChannelMetadataOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channels");
            url.Add(string.IsNullOrEmpty(channel) ? "" : channel);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (includeCustom)
            {
                requestQueryStringParams.Add("include", "custom");
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildDeleteChannelMetadataRequest(string requestMethod, string requestBody, string channel, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNDeleteChannelMetadataOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channels");
            url.Add(string.IsNullOrEmpty(channel) ? "" : channel);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetAllChannelMetadataRequest(string requestMethod, string requestBody, string start, string end, int limit, bool includeCount, bool includeCustom, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetAllChannelMetadataOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channels");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(end, currentType, false, false, false));
            }
            if (limit >= 0)
            {
                requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }
            if (includeCount)
            {
                requestQueryStringParams.Add("count", "true");
            }
            if (includeCustom)
            {
                requestQueryStringParams.Add("include", "custom");
            }
            if (!string.IsNullOrEmpty(filter))
            {
                requestQueryStringParams.Add("filter", UriUtil.EncodeUriComponent(filter, currentType, false, false, false));
            }
            if (sort != null && sort.Count > 0)
            {
                requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sort.ToArray()), currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetSingleChannelMetadataRequest(string requestMethod, string requestBody, string channel, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetChannelMetadataOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channels");
            url.Add(string.IsNullOrEmpty(channel) ? "" : channel);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (includeCustom)
            {
                requestQueryStringParams.Add("include", "custom");
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildMembershipSetRemoveManageUserRequest(PNOperationType type, string requestMethod, string requestBody, string uuid, string start, string end, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = type;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("uuids");
            url.Add(string.IsNullOrEmpty(uuid) ? "" : uuid);
            url.Add("channels");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(end, currentType, false, false, false));
            }
            if (limit >= 0)
            {
                requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }
            if (includeCount)
            {
                requestQueryStringParams.Add("count", "true");
            }
            if (!string.IsNullOrEmpty(includeOptions))
            {
                requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(includeOptions, currentType, false, false, false));
            }
            if (sort != null && sort.Count > 0)
            {
                requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sort.ToArray()), currentType, false, false, false));
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildMemberAddUpdateRemoveChannelRequest(string requestMethod, string requestBody, string channel, string start, string end, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNManageChannelMembersOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channels");
            url.Add(string.IsNullOrEmpty(channel) ? "" : channel);
            url.Add("uuids");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(end, currentType, false, false, false));
            }
            if (limit >= 0)
            {
                requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }
            if (includeCount)
            {
                requestQueryStringParams.Add("count", "true");
            }
            if (!string.IsNullOrEmpty(includeOptions))
            {
                requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(includeOptions, currentType, false, false, false));
            }
            if (sort != null && sort.Count > 0)
            {
                requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sort.ToArray()), currentType, false, false, false));
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetAllMembershipsRequest(string requestMethod, string requestBody, string uuid, string start, string end, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetMembershipsOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("uuids");
            url.Add(string.IsNullOrEmpty(uuid) ? "" : uuid);
            url.Add("channels");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(end, currentType, false, false, false));
            }
            if (limit >= 0)
            {
                requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }
            if (includeCount)
            {
                requestQueryStringParams.Add("count", "true");
            }
            if (!string.IsNullOrEmpty(includeOptions))
            {
                requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(includeOptions, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(filter))
            {
                requestQueryStringParams.Add("filter", UriUtil.EncodeUriComponent(filter, currentType, false, false, false));
            }
            if (sort != null && sort.Count > 0)
            {
                requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sort.ToArray()), currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetAllMembersRequest(string requestMethod, string requestBody, string channel, string start, string end, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetChannelMembersOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("objects");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channels");
            url.Add(string.IsNullOrEmpty(channel) ? "" : channel);
            url.Add("uuids");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(end, currentType, false, false, false));
            }
            if (limit >= 0)
            {
                requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }
            if (includeCount)
            {
                requestQueryStringParams.Add("count", "true");
            }
            if (!string.IsNullOrEmpty(includeOptions))
            {
                requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(includeOptions, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(filter))
            {
                requestQueryStringParams.Add("filter", UriUtil.EncodeUriComponent(filter, currentType, false, false, false));
            }
            if (sort != null && sort.Count > 0)
            {
                requestQueryStringParams.Add("sort", UriUtil.EncodeUriComponent(string.Join(",", sort.ToArray()), currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildAddMessageActionRequest(string requestMethod, string requestBody, string channel, long messageTimetoken, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAddMessageActionOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("message-actions");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");
            url.Add(channel);
            url.Add("message");
            url.Add(messageTimetoken.ToString(CultureInfo.InvariantCulture));

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildRemoveMessageActionRequest(string requestMethod, string requestBody, string channel, long messageTimetoken, long actionTimetoken, string messageActionUuid, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNRemoveMessageActionOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("message-actions");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");
            url.Add(channel);
            url.Add("message");
            url.Add(messageTimetoken.ToString(CultureInfo.InvariantCulture));
            url.Add("action");
            url.Add(actionTimetoken.ToString(CultureInfo.InvariantCulture));

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (messageActionUuid != null)
            {
                requestQueryStringParams.Add("uuid", UriUtil.EncodeUriComponent(messageActionUuid, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetMessageActionsRequest(string requestMethod, string requestBody, string channel, long start, long end, int limit, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetMessageActionsOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("message-actions");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channel");
            url.Add(channel);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (start >= 0)
            {
                requestQueryStringParams.Add("start", start.ToString(CultureInfo.InvariantCulture));
            }
            if (end >= 0)
            {
                requestQueryStringParams.Add("end", end.ToString(CultureInfo.InvariantCulture));
            }
            if (limit >= 0)
            {
                requestQueryStringParams.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGenerateFileUploadUrlRequest(string requestMethod, string requestBody, string channel, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGenerateFileUploadUrlOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("files");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channels");
            url.Add(channel);
            url.Add("generate-upload-url");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildPublishFileMessageRequest(string requestMethod, string requestBody, string channel, object originalMessage, bool storeInHistory, int ttl, Dictionary<string, object> userMetaData, Dictionary<string, string> additionalUrlParams, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNPublishFileMessageOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("files");
            url.Add("publish-file");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].PublishKey : "");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("0");
            url.Add(channel);
            url.Add("0");
            if (requestMethod.ToUpperInvariant() == "GET")
            {
                string message = JsonEncodePublishMsg(originalMessage, currentType);
                url.Add(message);
            }

            Dictionary<string, string> additionalUrlParamsDic = new Dictionary<string, string>();
            if (additionalUrlParams != null)
            {
                additionalUrlParamsDic = additionalUrlParams;
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>(additionalUrlParamsDic);

            if (userMetaData != null)
            {
                string jsonMetaData = jsonLib.SerializeToJsonString(userMetaData);
                requestQueryStringParams.Add("meta", UriUtil.EncodeUriComponent(jsonMetaData, currentType, false, false, false));
            }

            if (storeInHistory && ttl >= 0)
            {
                requestQueryStringParams.Add("tt1", ttl.ToString(CultureInfo.InvariantCulture));
            }

            if (!storeInHistory)
            {
                requestQueryStringParams.Add("store", "0");
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildGetFileUrlOrDeleteReqest(string requestMethod, string requestBody, string channel, string fileId, string fileName, Dictionary<string, object> externalQueryParam, PNOperationType operationType)
        {
            PNOperationType currentType = operationType;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("files");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channels");
            url.Add(channel);
            url.Add("files");
            url.Add(fileId);
            url.Add(fileName);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }

        Uri IUrlRequestBuilder.BuildListFilesReqest(string requestMethod, string requestBody, string channel, int limit, string nextToken, Dictionary<string, object> externalQueryParam, PNOperationType operationType)
        {
            PNOperationType currentType = operationType;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("files");
            url.Add(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].SubscribeKey : "");
            url.Add("channels");
            url.Add(channel);
            url.Add("files");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            requestQueryStringParams.Add("limit", (limit <= -1) ? "100" : limit.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(nextToken))
            {
                requestQueryStringParams.Add("next", nextToken);
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(currentType, requestQueryStringParams);

            return BuildRestApiRequest(requestMethod, requestBody, url, currentType, queryString, true);
        }


        private Dictionary<string, string> GenerateCommonQueryParams(PNOperationType type, string uuid)
        {
            long timeStamp = TranslateUtcDateTimeToSeconds(DateTime.UtcNow);
            string requestid = Guid.NewGuid().ToString();

            if (pubnubUnitTest != null)
            {
                timeStamp = pubnubUnitTest.Timetoken;
                requestid = string.IsNullOrEmpty(pubnubUnitTest.RequestId) ? "" : pubnubUnitTest.RequestId;
            }

            Dictionary<string, string> ret = new Dictionary<string, string>();
            if (pubnubUnitTest != null && pubnubConfig.ContainsKey(pubnubInstanceId))
            {
                if (pubnubUnitTest.IncludeUuid)
                {
                    ret.Add("uuid", UriUtil.EncodeUriComponent(pubnubConfig[pubnubInstanceId].UserId, PNOperationType.PNSubscribeOperation, false, false, true));
                }

                if (pubnubUnitTest.IncludePnsdk)
                {
                    ret.Add("pnsdk", UriUtil.EncodeUriComponent(Pubnub.Version, PNOperationType.PNSubscribeOperation, false, false, true));
                }
            }
            else
            {
                ret.Add("uuid", UriUtil.EncodeUriComponent(uuid != null ? uuid : 
                                        (pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].UserId.ToString() : ""), 
                                        PNOperationType.PNSubscribeOperation, false, false, true));
                ret.Add("pnsdk", UriUtil.EncodeUriComponent(Pubnub.Version, PNOperationType.PNSubscribeOperation, false, false, true));
            }

            if (pubnubConfig != null)
            {
                if (pubnubConfig.ContainsKey(pubnubInstanceId) && pubnubConfig[pubnubInstanceId].IncludeRequestIdentifier)
                {
                    ret.Add("requestid", requestid);
                }

                if (pubnubConfig.ContainsKey(pubnubInstanceId) && pubnubConfig[pubnubInstanceId].IncludeInstanceIdentifier && !string.IsNullOrEmpty(pubnubInstanceId) && pubnubInstanceId.Trim().Length > 0)
                {
                    ret.Add("instanceid", pubnubInstanceId);
                }

                if (pubnubConfig.ContainsKey(pubnubInstanceId) && pubnubConfig[pubnubInstanceId].EnableTelemetry && telemetryMgr != null)
                {
                    Dictionary<string, string> opsLatency = telemetryMgr.GetOperationsLatency().ConfigureAwait(false).GetAwaiter().GetResult();
                    if (opsLatency != null && opsLatency.Count > 0)
                    {
                        foreach (string key in opsLatency.Keys)
                        {
                            ret.Add(key, opsLatency[key]);
                        }
                    }
                }

                if (pubnubConfig.ContainsKey(pubnubInstanceId) && !string.IsNullOrEmpty(pubnubConfig[pubnubInstanceId].SecretKey))
                {
                    ret.Add("timestamp", timeStamp.ToString(CultureInfo.InvariantCulture));
                }

                if (type != PNOperationType.PNTimeOperation
                        && type != PNOperationType.PNAccessManagerGrant && type != PNOperationType.PNAccessManagerGrantToken && type != PNOperationType.PNAccessManagerRevokeToken && type != PNOperationType.ChannelGroupGrantAccess
                        && type != PNOperationType.PNAccessManagerAudit && type != PNOperationType.ChannelGroupAuditAccess)
                {
                    if (tokenMgr != null && !string.IsNullOrEmpty(tokenMgr.AuthToken) && tokenMgr.AuthToken.Trim().Length > 0)
                    {
                        ret.Add("auth", UriUtil.EncodeUriComponent(tokenMgr.AuthToken, type, false, false, false));
                    }
                    else if (pubnubConfig.ContainsKey(pubnubInstanceId) && !string.IsNullOrEmpty(pubnubConfig[pubnubInstanceId].AuthKey) && pubnubConfig[pubnubInstanceId].AuthKey.Trim().Length > 0)
                    {
                        ret.Add("auth", UriUtil.EncodeUriComponent(pubnubConfig[pubnubInstanceId].AuthKey, type, false, false, false));
                    }
                }
            }

            return ret;
        }

        private string GeneratePAMv2Signature(string queryStringToSign, string partialUrl, PNOperationType opType)
        {
            string signature = "";
            StringBuilder string_to_sign = new StringBuilder();
            if (pubnubConfig.ContainsKey(pubnubInstanceId))
            {
                string_to_sign.Append(pubnubConfig[pubnubInstanceId].SubscribeKey).Append('\n').Append(pubnubConfig[pubnubInstanceId].PublishKey).Append('\n');
                string_to_sign.Append(partialUrl).Append('\n');
                string_to_sign.Append(queryStringToSign);

                PubnubCrypto pubnubCrypto = new PubnubCrypto((opType != PNOperationType.PNSignalOperation) ? pubnubConfig[pubnubInstanceId].CipherKey : "", pubnubConfig[pubnubInstanceId], this.pubnubLog, null);
                signature = pubnubCrypto.PubnubAccessManagerSign(pubnubConfig[pubnubInstanceId].SecretKey, string_to_sign.ToString());
                if (this.pubnubLog != null && this.pubnubConfig != null)
                {
                    LoggingMethod.WriteToLog(pubnubLog, "string_to_sign = " + string_to_sign, pubnubConfig[pubnubInstanceId].LogVerbosity);
                    LoggingMethod.WriteToLog(pubnubLog, "signature = " + signature, pubnubConfig[pubnubInstanceId].LogVerbosity);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("string_to_sign = " + string_to_sign);
                    System.Diagnostics.Debug.WriteLine("signature = " + signature);
                }
            }
            return signature;
        }

        private string GeneratePAMv3Signature(string method, string requestBody, string queryStringToSign, string partialUrl, PNOperationType opType)
        {
            string signature = "";
            StringBuilder string_to_sign = new StringBuilder();
            if (pubnubConfig.ContainsKey(pubnubInstanceId))
            {
                string_to_sign.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", method.ToUpperInvariant());
                string_to_sign.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", pubnubConfig[pubnubInstanceId].PublishKey);
                string_to_sign.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", partialUrl);
                string_to_sign.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", queryStringToSign);
                string_to_sign.Append(requestBody);

                PubnubCrypto pubnubCrypto = new PubnubCrypto((opType != PNOperationType.PNSignalOperation) ? pubnubConfig[pubnubInstanceId].CipherKey : "", pubnubConfig[pubnubInstanceId], this.pubnubLog, null);
                signature = pubnubCrypto.PubnubAccessManagerSign(pubnubConfig[pubnubInstanceId].SecretKey, string_to_sign.ToString());
                signature = string.Format(CultureInfo.InvariantCulture, "v2.{0}", signature.TrimEnd(new[] { '=' }));
                if (this.pubnubLog != null && this.pubnubConfig != null)
                {
                    LoggingMethod.WriteToLog(pubnubLog, "string_to_sign = " + string_to_sign, pubnubConfig[pubnubInstanceId].LogVerbosity);
                    LoggingMethod.WriteToLog(pubnubLog, "signature = " + signature, pubnubConfig[pubnubInstanceId].LogVerbosity);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("string_to_sign = " + string_to_sign);
                    System.Diagnostics.Debug.WriteLine("signature = " + signature);
                }
            }
            return signature;
        }

        private string BuildQueryString(PNOperationType type, Dictionary<string, string> queryStringParamDic)
        {
            string queryString = "";

            try
            {
                Dictionary<string, string> internalQueryStringParamDic = new Dictionary<string, string>();
                if (queryStringParamDic != null)
                {
                    internalQueryStringParamDic = queryStringParamDic;
                }

                string qsUuid = internalQueryStringParamDic.ContainsKey("uuid") ? internalQueryStringParamDic["uuid"] : null;
                
                Dictionary<string, string> commonQueryStringParams = GenerateCommonQueryParams(type, qsUuid);
                Dictionary<string, string> queryStringParams = new Dictionary<string, string>(commonQueryStringParams.Concat(internalQueryStringParamDic).GroupBy(item => item.Key).ToDictionary(item => item.Key, item => item.First().Value));

                queryString = string.Join("&", queryStringParams.OrderBy(kvp => kvp.Key, StringComparer.Ordinal).Select(kvp => string.Format(CultureInfo.InvariantCulture, "{0}={1}", kvp.Key, kvp.Value)).ToArray());

            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, "UrlRequestBuilder => BuildQueryString error " + ex, pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].LogVerbosity : PNLogVerbosity.BODY);
            }

            return queryString;
        }

        private Uri BuildRestApiRequest(string requestMethod, string requestBody, List<string> urlComponents, PNOperationType type, string queryString, bool isPamV3Sign)
        {   
            StringBuilder url = new StringBuilder();

            if (pubnubConfig.ContainsKey(pubnubInstanceId) && pubnubConfig[pubnubInstanceId].Secure)
            {
                url.Append("https://");
            }
            else
            {
                url.Append("http://");
            }

            url.Append(pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].Origin : "");

            for (int componentIndex = 0; componentIndex < urlComponents.Count; componentIndex++)
            {
                url.Append('/');

                if ((type == PNOperationType.PNPublishOperation || type == PNOperationType.PNPublishFileMessageOperation) && componentIndex == urlComponents.Count - 1)
                {
                    url.Append(UriUtil.EncodeUriComponent(urlComponents[componentIndex], type, false, true, false));
                }
                else if (type == PNOperationType.PNAccessManagerRevokeToken)
                {
                    url.Append(UriUtil.EncodeUriComponent(urlComponents[componentIndex], type, false, false, false));
                }
                else
                {
                    url.Append(UriUtil.EncodeUriComponent(urlComponents[componentIndex], type, true, false, false));
                }
            }

            url.Append('?');
            url.Append(queryString);
            System.Diagnostics.Debug.WriteLine("sb = " + url);
            Uri requestUri = new Uri(url.ToString());

            if (type == PNOperationType.PNPublishOperation || type == PNOperationType.PNPublishFileMessageOperation || type == PNOperationType.PNSubscribeOperation || type == PNOperationType.Presence)
            {
                ForceCanonicalPathAndQuery(requestUri);
            }
            System.Diagnostics.Debug.WriteLine("Uri = " + requestUri.ToString());

            if (pubnubConfig.ContainsKey(pubnubInstanceId) && pubnubConfig[pubnubInstanceId].SecretKey.Length > 0)
            {
                StringBuilder partialUrl = new StringBuilder();
                partialUrl.Append(requestUri.AbsolutePath);

                string signature;
                if (isPamV3Sign)
                {
                    signature = GeneratePAMv3Signature(requestMethod, requestBody, queryString, partialUrl.ToString(), type);
                }
                else
                {
                    signature = GeneratePAMv2Signature(queryString, partialUrl.ToString(), type);
                }
                string queryStringWithSignature = string.Format(CultureInfo.InvariantCulture, "{0}&signature={1}", queryString, signature);
                UriBuilder uriBuilder = new UriBuilder(requestUri);
                uriBuilder.Query = queryStringWithSignature;

                requestUri = uriBuilder.Uri;
            }

            return requestUri;
        }

        private string JsonEncodePublishMsg(object originalMessage, PNOperationType opType)
        {
            string message = jsonLib.SerializeToJsonString(originalMessage);

            if (pubnubConfig.ContainsKey(pubnubInstanceId) && pubnubConfig[pubnubInstanceId].CipherKey.Length > 0 && opType != PNOperationType.PNSignalOperation)
            {
                PubnubCrypto aes = new PubnubCrypto(pubnubConfig[pubnubInstanceId].CipherKey, pubnubConfig[pubnubInstanceId], pubnubLog, null);
                string encryptMessage = aes.Encrypt(message);
                message = jsonLib.SerializeToJsonString(encryptMessage);
            }

            return message;
        }

        private void ForceCanonicalPathAndQuery(Uri requestUri)
        {
#if !NETSTANDARD10 && !NETSTANDARD11 && !NETSTANDARD12 && !WP81
            LoggingMethod.WriteToLog(pubnubLog, "Inside ForceCanonicalPathAndQuery = " + requestUri.ToString(), pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].LogVerbosity : PNLogVerbosity.NONE);
            try
            {
                FieldInfo flagsFieldInfo = typeof(Uri).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
                if (flagsFieldInfo != null)
                {
                    ulong flags = (ulong)flagsFieldInfo.GetValue(requestUri);
                    flags &= ~((ulong)0x30); // Flags.PathNotCanonical|Flags.QueryNotCanonical
                    flagsFieldInfo.SetValue(requestUri, flags);
                }
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, "Exception Inside ForceCanonicalPathAndQuery = " + ex, pubnubConfig.ContainsKey(pubnubInstanceId) ? pubnubConfig[pubnubInstanceId].LogVerbosity : PNLogVerbosity.BODY);
            }
#endif
        }

        public static long TranslateUtcDateTimeToSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds);
            return timeStamp;
        }

    }
}
