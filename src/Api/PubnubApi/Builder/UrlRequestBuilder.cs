using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using PubnubApi.Interface;
using System.Globalization;

namespace PubnubApi
{
    public sealed class UrlRequestBuilder : IUrlRequestBuilder
    {
        private readonly PNConfiguration pubnubConfig;
        private readonly IJsonPluggableLibrary jsonLib ;
        private readonly IPubnubUnitTest pubnubUnitTest;
        private readonly IPubnubLog pubnubLog;
        private string pubnubInstanceId = "";
        private readonly EndPoint.TelemetryManager telemetryMgr;

        public UrlRequestBuilder(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest, IPubnubLog log, EndPoint.TelemetryManager pubnubTelemetryMgr)
        {
            this.pubnubConfig = config;
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubUnitTest = pubnubUnitTest;
            this.pubnubLog = log;
            this.telemetryMgr = pubnubTelemetryMgr;
        }

        string IUrlRequestBuilder.PubnubInstanceId
        {
            get
            {
                return pubnubInstanceId;
            }

            set
            {
                pubnubInstanceId = value;
            }
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
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, "", currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildMultiChannelSubscribeRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, long timetoken, string channelsJsonState, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNSubscribeOperation;
            string channelForUrl = (channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("subscribe");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add(channelForUrl);
            url.Add("0");

            Dictionary<string, string> internalInitialSubscribeUrlParams = new Dictionary<string, string>();
            if (initialSubscribeUrlParams != null)
            {
                internalInitialSubscribeUrlParams = initialSubscribeUrlParams;
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>(internalInitialSubscribeUrlParams);

            if (!requestQueryStringParams.ContainsKey("filter-expr") && !string.IsNullOrEmpty(pubnubConfig.FilterExpression))
            {
                requestQueryStringParams.Add("filter-expr", UriUtil.EncodeUriComponent(false, pubnubConfig.FilterExpression, currentType, false, false, false));
            }

            if (!requestQueryStringParams.ContainsKey("tt"))
            {
                requestQueryStringParams.Add("tt", timetoken.ToString());
            }

            if (pubnubConfig.PresenceTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig.PresenceTimeout.ToString());
            }

            if (channelGroups != null && channelGroups.Length > 0 && channelGroups[0] != "")
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(false, string.Join(",", channelGroups.OrderBy(x => x).ToArray()), currentType, false, false, false));
            }

            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(false, channelsJsonState, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, "", currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildMultiChannelLeaveRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.Leave;
            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");
            url.Add(multiChannel);
            url.Add("leave");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (pubnubConfig.PresenceTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig.PresenceTimeout.ToString());
            }

            string channelsJsonState = jsonUserState;
            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(false, channelsJsonState, currentType, false, false, false));
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(false, string.Join(",", channelGroups.OrderBy(x => x).ToArray()),currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, "", currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildPublishRequest(string requestMethod, string requestBody, string channel, object originalMessage, bool storeInHistory, int ttl, Dictionary<string, object> userMetaData, Dictionary<string, string> additionalUrlParams, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNPublishOperation;

            List<string> url = new List<string>();
            url.Add("publish");
            url.Add(pubnubConfig.PublishKey);
            url.Add(pubnubConfig.SubscribeKey);
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
                requestQueryStringParams.Add("meta", UriUtil.EncodeUriComponent(false, jsonMetaData, currentType, false, false, false));
            }

            if (storeInHistory && ttl >= 0)
            {
                requestQueryStringParams.Add("tt1", ttl.ToString());
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
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            bool allowPAMv3Sign = (requestMethod.ToUpperInvariant() == "POST") ? false : true;
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, allowPAMv3Sign);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildSignalRequest(string requestMethod, string requestBody, string channel, object originalMessage, Dictionary<string, object> userMetaData, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNSignalOperation;

            List<string> url = new List<string>();
            url.Add("signal");
            url.Add(pubnubConfig.PublishKey);
            url.Add(pubnubConfig.SubscribeKey);
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
                requestQueryStringParams.Add("meta", UriUtil.EncodeUriComponent(false, jsonMetaData, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildHereNowRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNHereNowOperation;
            string channel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
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
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(false, commaDelimitedchannelGroup, currentType, false, false, false));
            }

            requestQueryStringParams.Add("disable_uuids", disableUUID.ToString());
            requestQueryStringParams.Add("state", userState.ToString());

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildHistoryRequest(string requestMethod, string requestBody, string channel, long start, long end, int count, bool reverse, bool includeToken, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNHistoryOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("history");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");
            url.Add(channel);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("count", (count <= -1) ? "100" : count.ToString());

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

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach(KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildMessageCountsRequest(string requestMethod, string requestBody, string[] channels, long[] timetokens, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNMessageCountsOperation;
            string channel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : "";

            List<string> url = new List<string>();
            url.Add("v3");
            url.Add("history");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("message-counts");
            if (!string.IsNullOrEmpty(channel))
            {
                url.Add(UriUtil.EncodeUriComponent(false, channel, currentType, false, false, false));
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (timetokens != null && timetokens.Length > 0)
            {
                string tt = string.Join(",", timetokens.Select(x => x.ToString()).ToArray());
                if (timetokens.Length == 1)
                {
                    requestQueryStringParams.Add("timetoken", tt);
                }
                else
                {
                    requestQueryStringParams.Add("channelsTimetoken", UriUtil.EncodeUriComponent(false, tt, currentType, false, false, false));
                }
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildDeleteMessageRequest(string requestMethod, string requestBody, string channel, long start, long end, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNDeleteMessageOperation;

            List<string> url = new List<string>();
            url.Add("v3");
            url.Add("history");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
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
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildWhereNowRequest(string requestMethod, string requestBody, string uuid, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNWhereNowOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("uuid");
            url.Add(uuid);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGrantV2AccessRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string authKeysCommaDelimited, bool read, bool write, bool delete, bool manage, long ttl, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAccessManagerGrant;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("auth");
            url.Add("grant");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(authKeysCommaDelimited))
            {
                requestQueryStringParams.Add("auth", UriUtil.EncodeUriComponent(false, authKeysCommaDelimited, currentType, false, false, false));
            }

            if (!string.IsNullOrEmpty(channelsCommaDelimited))
            {
                requestQueryStringParams.Add("channel", UriUtil.EncodeUriComponent(false, channelsCommaDelimited, currentType, false, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited))
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(false, channelGroupsCommaDelimited, currentType, false, false, false));
            }

            if (ttl > -1)
            {
                requestQueryStringParams.Add("ttl", ttl.ToString());
            }

            requestQueryStringParams.Add("r", Convert.ToInt32(read).ToString());
            requestQueryStringParams.Add("w", Convert.ToInt32(write).ToString());
            requestQueryStringParams.Add("d", Convert.ToInt32(delete).ToString());
            requestQueryStringParams.Add("m", Convert.ToInt32(manage).ToString());

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGrantV3AccessRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAccessManagerGrant;

            List<string> url = new List<string>();
            url.Add("v3");
            url.Add("pam");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("grant");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                        //requestQueryStringParams.Add(kvp.Key, kvp.Value.ToString());
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildAuditAccessRequest(string requestMethod, string requestBody, string channel, string channelGroup, string authKeysCommaDelimited, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAccessManagerAudit;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("auth");
            url.Add("audit");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(authKeysCommaDelimited))
            {
                requestQueryStringParams.Add("auth", UriUtil.EncodeUriComponent(false, authKeysCommaDelimited, currentType, false, false, false));
            }

            if (!string.IsNullOrEmpty(channel))
            {
                requestQueryStringParams.Add("channel", UriUtil.EncodeUriComponent(false, channel, currentType, false, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(false, channelGroup, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetUserStateRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetStateOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");

            if (string.IsNullOrEmpty(channelsCommaDelimited) && channelsCommaDelimited.Trim().Length <= 0)
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
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(false, channelGroupsCommaDelimited, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildSetUserStateRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNSetStateOperation;
            string internalChannelsCommaDelimited;

            if (string.IsNullOrEmpty(channelsCommaDelimited) && channelsCommaDelimited.Trim().Length <= 0)
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
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");
            url.Add(internalChannelsCommaDelimited);
            url.Add("uuid");
            url.Add(uuid);
            url.Add("data");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited) && channelGroupsCommaDelimited.Trim().Length > 0)
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(false, jsonUserState, currentType, false, false, false));
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(false, channelGroupsCommaDelimited, currentType, false, false, false));
            }
            else
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(false, jsonUserState, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildAddChannelsToChannelGroupRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNAddChannelsToGroupOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            if (!string.IsNullOrEmpty(nameSpace) && nameSpace.Trim().Length > 0)
            {
                url.Add("namespace");
                url.Add(nameSpace);
            }
            url.Add("channel-group");
            url.Add(groupName);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("add", UriUtil.EncodeUriComponent(false, channelsCommaDelimited, currentType,false, false, false));

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
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
            url.Add(pubnubConfig.SubscribeKey);
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
                requestQueryStringParams.Add("remove", UriUtil.EncodeUriComponent(false, channelsCommaDelimited, currentType, false, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
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
            url.Add(pubnubConfig.SubscribeKey);
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
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetAllChannelGroupRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.ChannelGroupAllGet;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel-group");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildRegisterDevicePushRequest(string requestMethod, string requestBody, string channel, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PushRegister;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("devices");
            url.Add(pushToken);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("type", pushType.ToString().ToLower());
            requestQueryStringParams.Add("add", UriUtil.EncodeUriComponent(false, channel, currentType, true, false, false));

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildUnregisterDevicePushRequest(string requestMethod, string requestBody, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PushUnregister;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("devices");
            url.Add(pushToken);
            url.Add("remove");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("type", pushType.ToString().ToLower());

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildRemoveChannelPushRequest(string requestMethod, string requestBody, string channel, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PushRemove;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("devices");
            url.Add(pushToken);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("type", pushType.ToString().ToLower());
            requestQueryStringParams.Add("remove", UriUtil.EncodeUriComponent(false, channel, currentType, true, false, false));

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetChannelsPushRequest(string requestMethod, string requestBody, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PushGet;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("devices");
            url.Add(pushToken);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("type", pushType.ToString().ToLower());

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildPresenceHeartbeatRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, string jsonUserState)
        {
            PNOperationType currentType = PNOperationType.PNHeartbeatOperation;

            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");
            url.Add(multiChannel);
            url.Add("heartbeat");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            string channelsJsonState = jsonUserState;
            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(false, channelsJsonState, currentType, false, false, false));
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(false, string.Join(",", channelGroups.OrderBy(x => x).ToArray()), currentType, false, false, false));
            }

            if (pubnubConfig.PresenceTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig.PresenceTimeout.ToString());
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildCreateUserRequest(string requestMethod, string requestBody, Dictionary<string, object> userCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNCreateUserOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("users");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (userCustom != null)
            {
                requestQueryStringParams.Add("include", "custom");
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildUpdateUserRequest(string requestMethod, string requestBody, string userId, Dictionary<string, object> userCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNUpdateUserOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("users");
            url.Add(userId);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (userCustom != null)
            {
                requestQueryStringParams.Add("include", "custom");
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildDeleteUserRequest(string requestMethod, string requestBody, string userId, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNDeleteUserOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("users");
            url.Add(string.IsNullOrEmpty(userId) ? "" : userId);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetAllUsersRequest(string requestMethod, string requestBody, string start, string end, int limit, bool includeCount, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetUsersOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("users");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(false, start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(false, end, currentType, false, false, false));
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

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetSingleUserRequest(string requestMethod, string requestBody, string userId, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetUserOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("users");
            url.Add(string.IsNullOrEmpty(userId) ? "": userId);

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
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildCreateSpaceRequest(string requestMethod, string requestBody, Dictionary<string, object> spaceCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNCreateSpaceOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("spaces");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (spaceCustom != null)
            {
                requestQueryStringParams.Add("include", "custom");
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildUpdateSpaceRequest(string requestMethod, string requestBody, string spaceId, Dictionary<string, object> spaceCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNUpdateUserOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("spaces");
            url.Add(spaceId);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (spaceCustom != null)
            {
                requestQueryStringParams.Add("include", "custom");
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildDeleteSpaceRequest(string requestMethod, string requestBody, string spaceId, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNDeleteSpaceOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("spaces");
            url.Add(string.IsNullOrEmpty(spaceId) ? "" : spaceId);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }

            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetAllSpacesRequest(string requestMethod, string requestBody, string start, string end, int limit, bool includeCount, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetSpacesOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("spaces");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(false, start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(false, end, currentType, false, false, false));
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

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetSingleSpaceRequest(string requestMethod, string requestBody, string spaceId, bool includeCustom, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetSpaceOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("spaces");
            url.Add(string.IsNullOrEmpty(spaceId) ? "" : spaceId);

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
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildUpdateSpaceMembershipsWithUserRequest(string requestMethod, string requestBody, string userId, string start, string end, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNManageMembershipsOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("users");
            url.Add(string.IsNullOrEmpty(userId) ? "" : userId);
            url.Add("spaces");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(false, start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(false, end, currentType, false, false, false));
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
                requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(false, includeOptions, currentType, true, false, false));
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildMembersAddUpdateRemoveRequest(string requestMethod, string requestBody, string spaceId, string start, string end, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNManageMembersOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("spaces");
            url.Add(string.IsNullOrEmpty(spaceId) ? "" : spaceId);
            url.Add("users");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(false, start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(false, end, currentType, false, false, false));
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
                requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(false, includeOptions, currentType, true, false, false));
            }
            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetAllMembershipsRequest(string requestMethod, string requestBody, string userId, string start, string end, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetMembershipsOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("users");
            url.Add(string.IsNullOrEmpty(userId) ? "" : userId);
            url.Add("spaces");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(false, start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(false, end, currentType, false, false, false));
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
                requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(false, includeOptions, currentType, true, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetAllMembersRequest(string requestMethod, string requestBody, string spaceId, string start, string end, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam)
        {
            PNOperationType currentType = PNOperationType.PNGetMembersOperation;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("objects");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("spaces");
            url.Add(string.IsNullOrEmpty(spaceId) ? "" : spaceId);
            url.Add("users");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(start))
            {
                requestQueryStringParams.Add("start", UriUtil.EncodeUriComponent(false, start, currentType, false, false, false));
            }
            if (!string.IsNullOrEmpty(end))
            {
                requestQueryStringParams.Add("end", UriUtil.EncodeUriComponent(false, end, currentType, false, false, false));
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
                requestQueryStringParams.Add("include", UriUtil.EncodeUriComponent(false, includeOptions, currentType, true, false, false));
            }

            if (externalQueryParam != null && externalQueryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in externalQueryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value.ToString(), currentType, false, false, false));
                    }
                }
            }
            string queryString = BuildQueryString(requestMethod, requestBody, currentType, url, requestQueryStringParams, true);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest(url, currentType, queryParams);
        }

        private Dictionary<string, string> GenerateCommonQueryParams(PNOperationType type)
        {
            long timeStamp = TranslateUtcDateTimeToSeconds(DateTime.UtcNow);
            string requestid = Guid.NewGuid().ToString();

            if (pubnubUnitTest != null)
            {
                timeStamp = pubnubUnitTest.Timetoken;
                requestid = string.IsNullOrEmpty(pubnubUnitTest.RequestId) ? "" : pubnubUnitTest.RequestId;
            }

            Dictionary<string, string> ret = new Dictionary<string, string>();
            if (pubnubUnitTest != null)
            {
                if (pubnubUnitTest.IncludeUuid)
                {
                    ret.Add("uuid", UriUtil.EncodeUriComponent(false, this.pubnubConfig.Uuid, PNOperationType.PNSubscribeOperation, false, false, true));
                }

                if (pubnubUnitTest.IncludePnsdk)
                {
                    ret.Add("pnsdk", UriUtil.EncodeUriComponent(false, Pubnub.Version, PNOperationType.PNSubscribeOperation, false, false, true));
                }
            }
            else
            {
                ret.Add("uuid", UriUtil.EncodeUriComponent(false, this.pubnubConfig.Uuid, PNOperationType.PNSubscribeOperation, false, false, true));
                ret.Add("pnsdk", UriUtil.EncodeUriComponent(false, Pubnub.Version, PNOperationType.PNSubscribeOperation, false, false, true));
            }

            if (pubnubConfig != null)
            {
                if (pubnubConfig.IncludeRequestIdentifier)
                {
                    ret.Add("requestid", requestid);
                }

                if (pubnubConfig.IncludeInstanceIdentifier && !string.IsNullOrEmpty(pubnubInstanceId) && pubnubInstanceId.Trim().Length > 0)
                {
                    ret.Add("instanceid", pubnubInstanceId);
                }

                if (pubnubConfig.EnableTelemetry && telemetryMgr != null)
                {
                    Dictionary<string, string> opsLatency = telemetryMgr.GetOperationsLatency().Result;
                    if (opsLatency != null && opsLatency.Count > 0)
                    {
                        foreach (string key in opsLatency.Keys)
                        {
                            ret.Add(key, opsLatency[key]);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(pubnubConfig.SecretKey))
                {
                    ret.Add("timestamp", timeStamp.ToString());
                }

                if (type != PNOperationType.PNTimeOperation
                        && type != PNOperationType.PNAccessManagerGrant && type != PNOperationType.ChannelGroupGrantAccess
                        && type != PNOperationType.PNAccessManagerAudit && type != PNOperationType.ChannelGroupAuditAccess)
                {
                    if (!string.IsNullOrEmpty(this.pubnubConfig.AuthKey))
                    {
                        ret.Add("auth", UriUtil.EncodeUriComponent(false, this.pubnubConfig.AuthKey, type, false, false, false));
                    }
                }
            }

            return ret;
        }

        private string GeneratePAMv2Signature(string queryStringToSign, string partialUrl, PNOperationType opType)
        {
            string signature = "";
            StringBuilder string_to_sign = new StringBuilder();
            string_to_sign.Append(this.pubnubConfig.SubscribeKey).Append("\n").Append(this.pubnubConfig.PublishKey).Append("\n");
            string_to_sign.Append(partialUrl).Append("\n");
            string_to_sign.Append(queryStringToSign);

            PubnubCrypto pubnubCrypto = new PubnubCrypto((opType != PNOperationType.PNSignalOperation) ? this.pubnubConfig.CipherKey : "", this.pubnubConfig, this.pubnubLog);
            signature = pubnubCrypto.PubnubAccessManagerSign(this.pubnubConfig.SecretKey, string_to_sign.ToString());
            System.Diagnostics.Debug.WriteLine("string_to_sign = " + string_to_sign);
            System.Diagnostics.Debug.WriteLine("signature = " + signature);
            return signature;
        }

        private string GeneratePAMv3Signature(string method, string requestBody, string queryStringToSign, string partialUrl, PNOperationType opType)
        {
            string signature = "";
            StringBuilder string_to_sign = new StringBuilder();
            string_to_sign.AppendFormat("{0}\n", method.ToUpperInvariant());
            string_to_sign.AppendFormat("{0}\n", this.pubnubConfig.PublishKey);
            string_to_sign.AppendFormat("{0}\n", partialUrl);
            string_to_sign.AppendFormat("{0}\n", queryStringToSign);
            string_to_sign.Append(requestBody);

            PubnubCrypto pubnubCrypto = new PubnubCrypto((opType != PNOperationType.PNSignalOperation) ? this.pubnubConfig.CipherKey : "", this.pubnubConfig, this.pubnubLog);
            signature = pubnubCrypto.PubnubAccessManagerSign(this.pubnubConfig.SecretKey, string_to_sign.ToString());
            //signature = string.Format("v2.{0}", signature.TrimEnd(new char[] { '=' }));
            signature = string.Format("v2.{0}", signature.TrimEnd(new char[] { '=' }));
            System.Diagnostics.Debug.WriteLine("string_to_sign = " + string_to_sign);
            System.Diagnostics.Debug.WriteLine("signature = " + signature);
            return signature;
        }

        private string BuildQueryString(string requestMethod, string requestBody, PNOperationType type, List<string> urlComponentList, Dictionary<string, string> queryStringParamDic, bool isPamV3Sign)
        {
            string queryString = "";

            try
            {
                Dictionary<string, string> internalQueryStringParamDic = new Dictionary<string, string>();
                if (queryStringParamDic != null)
                {
                    internalQueryStringParamDic = queryStringParamDic;
                }

                Dictionary<string, string> commonQueryStringParams = GenerateCommonQueryParams(type);
                Dictionary<string, string> queryStringParams = new Dictionary<string, string>(commonQueryStringParams.Concat(internalQueryStringParamDic).GroupBy(item => item.Key).ToDictionary(item => item.Key, item => item.First().Value));

                string queryToSign = string.Join("&", queryStringParams.OrderBy(kvp => kvp.Key, StringComparer.Ordinal).Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)).ToArray());
                //string queryToSign = string.Join("&", queryStringParams.OrderBy(kvp => kvp.Key).Select(kvp => string.Format("{0}={1}", kvp.Key, UriUtil.EncodeUriComponent(false, kvp.Value, type, false, false, false))).ToArray());

                if (this.pubnubConfig.SecretKey.Length > 0)
                {
                    StringBuilder partialUrl = new StringBuilder();
                    for (int componentIndex = 0; componentIndex < urlComponentList.Count; componentIndex++)
                    {
                        partialUrl.Append("/");
                        if (type == PNOperationType.PNPublishOperation && componentIndex == urlComponentList.Count - 1)
                        {
                            partialUrl.Append(UriUtil.EncodeUriComponent(true, urlComponentList[componentIndex], type, false, true, false));
                        }
                        else
                        {
                            partialUrl.Append(UriUtil.EncodeUriComponent(true, urlComponentList[componentIndex], type, true, false, false));
                        }
                    }

                    string signature = "";
                    if (isPamV3Sign)
                    {
                        signature = GeneratePAMv3Signature(requestMethod, requestBody, queryToSign, partialUrl.ToString(), type);
                    }
                    else
                    {
                        signature = GeneratePAMv2Signature(queryToSign, partialUrl.ToString(), type);
                    }
                    queryString = string.Format("{0}&signature={1}", queryToSign, signature);
                }
                else
                {
                    queryString = queryToSign;
                }
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, "UrlRequestBuilder => BuildQueryString error " + ex, pubnubConfig.LogVerbosity);
            }

            return queryString;
        }

        private Uri BuildRestApiRequest(List<string> urlComponents, PNOperationType type, string queryString)
        {   
            StringBuilder url = new StringBuilder();

            if (pubnubConfig.Secure)
            {
                url.Append("https://");
            }
            else
            {
                url.Append("http://");
            }

            url.Append(pubnubConfig.Origin);

            for (int componentIndex = 0; componentIndex < urlComponents.Count; componentIndex++)
            {
                url.Append("/");

                if (type == PNOperationType.PNPublishOperation && componentIndex == urlComponents.Count - 1)
                {
                    url.Append(UriUtil.EncodeUriComponent(false, urlComponents[componentIndex], type, false, true, false));
                }
                else
                {
                    url.Append(UriUtil.EncodeUriComponent(false, urlComponents[componentIndex], type, true, false, false));
                }
            }

            url.Append(queryString);
            System.Diagnostics.Debug.WriteLine("sb = " + url);
            Uri requestUri = new Uri(url.ToString());

            if (type == PNOperationType.PNPublishOperation || type == PNOperationType.PNSubscribeOperation || type == PNOperationType.Presence)
            {
                ForceCanonicalPathAndQuery(requestUri);
            }
            System.Diagnostics.Debug.WriteLine("Uri = " + requestUri.ToString());
            return requestUri;
        }

        private string JsonEncodePublishMsg(object originalMessage, PNOperationType opType)
        {
            string message = jsonLib.SerializeToJsonString(originalMessage);

            if (pubnubConfig.CipherKey.Length > 0 && opType != PNOperationType.PNSignalOperation)
            {
                PubnubCrypto aes = new PubnubCrypto(pubnubConfig.CipherKey, pubnubConfig, pubnubLog);
                string encryptMessage = aes.Encrypt(message);
                message = jsonLib.SerializeToJsonString(encryptMessage);
            }

            return message;
        }

        private void ForceCanonicalPathAndQuery(Uri requestUri)
        {
#if !NETSTANDARD10 && !NETSTANDARD11 && !NETSTANDARD12 && !WP81
            LoggingMethod.WriteToLog(pubnubLog, "Inside ForceCanonicalPathAndQuery = " + requestUri.ToString(), pubnubConfig.LogVerbosity);
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
                LoggingMethod.WriteToLog(pubnubLog, "Exception Inside ForceCanonicalPathAndQuery = " + ex, pubnubConfig.LogVerbosity);
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
