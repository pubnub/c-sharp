using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using PubnubApi.Interface;

namespace PubnubApi
{
    public sealed class UrlRequestBuilder : IUrlRequestBuilder
    {
        private PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonLib = null;
        private IPubnubUnitTest pubnubUnitTest = null;
        private IPubnubLog pubnubLog = null;
        private string pubnubInstanceId = "";
        private EndPoint.TelemetryManager telemetryMgr;

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


        Uri IUrlRequestBuilder.BuildTimeRequest()
        {
            PNOperationType currentType = PNOperationType.PNTimeOperation;

            List<string> url = new List<string>();
            url.Add("time");
            url.Add("0");

            string queryString = BuildQueryString(currentType, url, null);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildMultiChannelSubscribeRequest(string[] channels, string[] channelGroups, long timetoken, string channelsJsonState, Dictionary<string, string> initialSubscribeUrlParams)
        {
            PNOperationType currentType = PNOperationType.PNSubscribeOperation;
            string channelForUrl = (channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("subscribe");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add(channelForUrl);
            url.Add("0");

            if (initialSubscribeUrlParams == null)
            {
                initialSubscribeUrlParams = new Dictionary<string, string>();
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>(initialSubscribeUrlParams);

            if (!requestQueryStringParams.ContainsKey("filter-expr") && !string.IsNullOrEmpty(pubnubConfig.FilterExpression))
            {
                requestQueryStringParams.Add("filter-expr", new UriUtil().EncodeUriComponent(pubnubConfig.FilterExpression, currentType, false, false));
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
                requestQueryStringParams.Add("channel-group", new UriUtil().EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), currentType, false, false));
            }

            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                requestQueryStringParams.Add("state", new UriUtil().EncodeUriComponent(channelsJsonState, currentType, false, false));
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildMultiChannelLeaveRequest(string[] channels, string[] channelGroups, string uuid, string jsonUserState)
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
                requestQueryStringParams.Add("state", new UriUtil().EncodeUriComponent(channelsJsonState, currentType, false, false));
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                requestQueryStringParams.Add("channel-group", new UriUtil().EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()),currentType, false, false));
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, uuid, queryParams);
        }

        Uri IUrlRequestBuilder.BuildPublishRequest(string channel, object originalMessage, bool storeInHistory, int ttl, Dictionary<string, object> userMetaData, bool usePOST, Dictionary<string, string> additionalUrlParams)
        {
            bool enableJsonEncodingForPublish = true; //by default. added placeholder for future for direct json input
            PNOperationType currentType = PNOperationType.PNPublishOperation;

            List<string> url = new List<string>();
            url.Add("publish");
            url.Add(pubnubConfig.PublishKey);
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("0");
            url.Add(channel);
            url.Add("0");
            if (!usePOST)
            {
                string message = enableJsonEncodingForPublish ? JsonEncodePublishMsg(originalMessage) : originalMessage.ToString();
                url.Add(message);
            }

            if (additionalUrlParams == null)
            {
                additionalUrlParams = new Dictionary<string, string>();
            }

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>(additionalUrlParams);

            if (userMetaData != null)
            {
                string jsonMetaData = jsonLib.SerializeToJsonString(userMetaData);
                requestQueryStringParams.Add("meta", new UriUtil().EncodeUriComponent(jsonMetaData, currentType, false, false));
            }

            if (storeInHistory && ttl >= 0)
            {
                requestQueryStringParams.Add("tt1", ttl.ToString());
            }

            if (!storeInHistory)
            {
                requestQueryStringParams.Add("store", "0");
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildHereNowRequest(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState)
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
                requestQueryStringParams.Add("channel-group", new UriUtil().EncodeUriComponent(commaDelimitedchannelGroup, currentType, false, false));
            }

            requestQueryStringParams.Add("disable_uuids", disableUUID.ToString());
            requestQueryStringParams.Add("state", userState.ToString());

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildHistoryRequest(string channel, long start, long end, int count, bool reverse, bool includeToken)
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
                requestQueryStringParams.Add("reverse", reverse.ToString().ToLower());
            }
            if (start != -1)
            {
                requestQueryStringParams.Add("start", start.ToString().ToLower());
            }
            if (end != -1)
            {
                requestQueryStringParams.Add("end", end.ToString().ToLower());
            }

            if (includeToken)
            {
                requestQueryStringParams.Add("include_token", includeToken.ToString().ToLower());
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildDeleteMessageRequest(string channel, long start, long end)
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
                requestQueryStringParams.Add("start", start.ToString().ToLower());
            }
            if (end != -1)
            {
                requestQueryStringParams.Add("end", end.ToString().ToLower());
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildWhereNowRequest(string uuid)
        {
            PNOperationType currentType = PNOperationType.PNWhereNowOperation;

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("uuid");
            url.Add(uuid);

            string queryString = BuildQueryString(currentType, url, null);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGrantAccessRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string authKeysCommaDelimited, bool read, bool write, bool manage, long ttl)
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
                requestQueryStringParams.Add("auth", new UriUtil().EncodeUriComponent(authKeysCommaDelimited, currentType, false, false));
            }

            if (!string.IsNullOrEmpty(channelsCommaDelimited))
            {
                requestQueryStringParams.Add("channel", new UriUtil().EncodeUriComponent(channelsCommaDelimited, currentType, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited))
            {
                requestQueryStringParams.Add("channel-group", new UriUtil().EncodeUriComponent(channelGroupsCommaDelimited, currentType, false, false));
            }

            if (ttl > -1)
            {
                requestQueryStringParams.Add("ttl", ttl.ToString());
            }

            requestQueryStringParams.Add("r", Convert.ToInt32(read).ToString());
            requestQueryStringParams.Add("w", Convert.ToInt32(write).ToString());
            requestQueryStringParams.Add("m", Convert.ToInt32(manage).ToString());

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildAuditAccessRequest(string channel, string channelGroup, string authKeysCommaDelimited)
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
                requestQueryStringParams.Add("auth", new UriUtil().EncodeUriComponent(authKeysCommaDelimited, currentType, false, false));
            }

            if (!string.IsNullOrEmpty(channel))
            {
                requestQueryStringParams.Add("channel", new UriUtil().EncodeUriComponent(channel, currentType, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestQueryStringParams.Add("channel-group", new UriUtil().EncodeUriComponent(channelGroup, currentType, false, false));
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid)
        {
            PNOperationType currentType = PNOperationType.PNGetStateOperation;

            if (string.IsNullOrEmpty(channelsCommaDelimited) && channelsCommaDelimited.Trim().Length <= 0)
            {
                channelsCommaDelimited = ",";
            }

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");
            url.Add(channelsCommaDelimited);
            url.Add("uuid");
            url.Add(uuid);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited) && channelGroupsCommaDelimited.Trim().Length > 0)
            {
                requestQueryStringParams.Add("channel-group", new UriUtil().EncodeUriComponent(channelGroupsCommaDelimited, currentType, false, false));
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildSetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, string jsonUserState)
        {
            PNOperationType currentType = PNOperationType.PNSetStateOperation;

            if (string.IsNullOrEmpty(channelsCommaDelimited) && channelsCommaDelimited.Trim().Length <= 0)
            {
                channelsCommaDelimited = ",";
            }

            List<string> url = new List<string>();
            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");
            url.Add(channelsCommaDelimited);
            url.Add("uuid");
            url.Add(uuid);
            url.Add("data");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited) && channelGroupsCommaDelimited.Trim().Length > 0)
            {
                requestQueryStringParams.Add("state", new UriUtil().EncodeUriComponent(jsonUserState, currentType, false, false));
                requestQueryStringParams.Add("channel-group", new UriUtil().EncodeUriComponent(channelGroupsCommaDelimited, currentType, false, false));
            }
            else
            {
                requestQueryStringParams.Add("state", new UriUtil().EncodeUriComponent(jsonUserState, currentType, false, false));
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildAddChannelsToChannelGroupRequest(string channelsCommaDelimited, string nameSpace, string groupName)
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

            requestQueryStringParams.Add("add", new UriUtil().EncodeUriComponent(channelsCommaDelimited, currentType,false,false));

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildRemoveChannelsFromChannelGroupRequest(string channelsCommaDelimited, string nameSpace, string groupName)
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
                requestQueryStringParams.Add("remove", new UriUtil().EncodeUriComponent(channelsCommaDelimited, currentType, false, false));
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetChannelsForChannelGroupRequest(string nameSpace, string groupName, bool limitToChannelGroupScopeOnly)
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

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetAllChannelGroupRequest()
        {
            PNOperationType currentType = PNOperationType.ChannelGroupAllGet;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("channel-registration");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel-group");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildRegisterDevicePushRequest(string channel, PNPushType pushType, string pushToken)
        {
            PNOperationType currentType = PNOperationType.PushRegister;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("devices");
            url.Add(pushToken.ToString());

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("type", pushType.ToString().ToLower());
            requestQueryStringParams.Add("add", new UriUtil().EncodeUriComponent(channel, currentType, true, false));

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildUnregisterDevicePushRequest(PNPushType pushType, string pushToken)
        {
            PNOperationType currentType = PNOperationType.PushUnregister;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("devices");
            url.Add(pushToken.ToString());
            url.Add("remove");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("type", pushType.ToString().ToLower());

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildRemoveChannelPushRequest(string channel, PNPushType pushType, string pushToken)
        {
            PNOperationType currentType = PNOperationType.PushRemove;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("devices");
            url.Add(pushToken.ToString());

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("type", pushType.ToString().ToLower());
            requestQueryStringParams.Add("remove", new UriUtil().EncodeUriComponent(channel, currentType, true, false));

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildGetChannelsPushRequest(PNPushType pushType, string pushToken)
        {
            PNOperationType currentType = PNOperationType.PushGet;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("push");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("devices");
            url.Add(pushToken.ToString());

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            requestQueryStringParams.Add("type", pushType.ToString().ToLower());

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildPresenceHeartbeatRequest(string[] channels, string[] channelGroups, string jsonUserState)
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
                requestQueryStringParams.Add("state", new UriUtil().EncodeUriComponent(channelsJsonState, currentType, false, false));
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                requestQueryStringParams.Add("channel-group", new UriUtil().EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), currentType, false,false));
            }

            if (pubnubConfig.PresenceTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig.PresenceTimeout.ToString());
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
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
            ret.Add("uuid", this.pubnubConfig.Uuid);
            ret.Add("pnsdk", new UriUtil().EncodeUriComponent(Pubnub.Version, PNOperationType.PNSubscribeOperation, false, true));

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
                    Dictionary<string, string> opsLatency = telemetryMgr.GetOperationsLatency();
                    if (opsLatency != null && opsLatency.Count > 0)
                    {
                        foreach (string key in opsLatency.Keys)
                        {
                            ret.Add(key, opsLatency[key]);
                        }
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
                    ret.Add("auth", new UriUtil().EncodeUriComponent(this.pubnubConfig.AuthKey, type, false, false));
                }
            }

            return ret;
        }

        private string GenerateSignature(PNOperationType type, string queryStringToSign, string partialUrl)
        {
            string signature = "";
            StringBuilder string_to_sign = new StringBuilder();
            string_to_sign.Append(this.pubnubConfig.SubscribeKey).Append("\n").Append(this.pubnubConfig.PublishKey).Append("\n");
            string_to_sign.Append(partialUrl.ToString()).Append("\n");
            string_to_sign.Append(queryStringToSign);

            PubnubCrypto pubnubCrypto = new PubnubCrypto(this.pubnubConfig.CipherKey, this.pubnubConfig, this.pubnubLog);
            signature = pubnubCrypto.PubnubAccessManagerSign(this.pubnubConfig.SecretKey, string_to_sign.ToString());
            System.Diagnostics.Debug.WriteLine("string_to_sign = " + string_to_sign.ToString());
            System.Diagnostics.Debug.WriteLine("signature = " + signature);
            return signature;
        }

        private string BuildQueryString(PNOperationType type, List<string> urlComponentList, Dictionary<string, string> queryStringParamDic)
        {
            string queryString = "";

            try
            {
                if (queryStringParamDic == null)
                {
                    queryStringParamDic = new Dictionary<string, string>();
                }

                Dictionary<string, string> commonQueryStringParams = GenerateCommonQueryParams(type);
                Dictionary<string, string> queryStringParams = new Dictionary<string, string>(commonQueryStringParams.Concat(queryStringParamDic).GroupBy(item => item.Key).ToDictionary(item => item.Key, item => item.First().Value));

                string queryToSign = string.Join("&", queryStringParams.OrderBy(kvp => kvp.Key).Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)).ToArray());

                if (this.pubnubConfig.SecretKey.Length > 0)
                {
                    StringBuilder partialUrl = new StringBuilder();
                    for (int componentIndex = 0; componentIndex < urlComponentList.Count; componentIndex++)
                    {
                        partialUrl.Append("/");
                        if (type == PNOperationType.PNPublishOperation && componentIndex == urlComponentList.Count - 1)
                        {
                            partialUrl.Append(new UriUtil().EncodeUriComponent(urlComponentList[componentIndex].ToString(), type, false, false));
                        }
                        else
                        {
                            partialUrl.Append(new UriUtil().EncodeUriComponent(urlComponentList[componentIndex].ToString(), type, true, false));
                        }
                    }

                    string signature = GenerateSignature(type, queryToSign, partialUrl.ToString());
                    queryString = string.Format("{0}&signature={1}", queryToSign, signature);
                }
                else
                {
                    queryString = queryToSign;
                }
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, "UrlRequestBuilder => BuildQueryString error " + ex.ToString(), pubnubConfig.LogVerbosity);
            }

            return queryString;
        }

        private Uri BuildRestApiRequest<T>(List<string> urlComponents, PNOperationType type, string queryString)
        {
            return BuildRestApiRequest<T>(urlComponents, type, this.pubnubConfig.Uuid, queryString);
        }

        private Uri BuildRestApiRequest<T>(List<string> urlComponents, PNOperationType type, string uuid, string queryString)
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
                    url.Append(new UriUtil().EncodeUriComponent(urlComponents[componentIndex].ToString(), type, false, false));
                }
                else
                {
                    url.Append(new UriUtil().EncodeUriComponent(urlComponents[componentIndex].ToString(), type, true, false));
                }
            }

            url.Append(queryString);

            Uri requestUri = new Uri(url.ToString());

            if (type == PNOperationType.PNPublishOperation || type == PNOperationType.PNSubscribeOperation || type == PNOperationType.Presence)
            {
                ForceCanonicalPathAndQuery(requestUri);
            }

            return requestUri;
        }

        private string JsonEncodePublishMsg(object originalMessage)
        {
            string message = jsonLib.SerializeToJsonString(originalMessage);

            if (pubnubConfig.CipherKey.Length > 0)
            {
                PubnubCrypto aes = new PubnubCrypto(pubnubConfig.CipherKey, pubnubConfig, pubnubLog);
                string encryptMessage = aes.Encrypt(message);
                message = jsonLib.SerializeToJsonString(encryptMessage);
            }

            return message;
        }


        private void ForceCanonicalPathAndQuery(Uri requestUri)
        {
#if !NETSTANDARD10 && !NETSTANDARD11 && !NETSTANDARD12 && !WP81 && !PORTABLE111
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
                LoggingMethod.WriteToLog(pubnubLog, "Exception Inside ForceCanonicalPathAndQuery = " + ex.ToString(), pubnubConfig.LogVerbosity);
            }
#endif
        }

        private static string Md5(string text)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] data = Encoding.Unicode.GetBytes(text);
                byte[] hash = md5.ComputeHash(data);
                string hexaHash = "";
                foreach (byte b in hash)
                {
                    hexaHash += String.Format("{0:x2}", b);
                }

                return hexaHash;
            }
        }

        public static long TranslateUtcDateTimeToSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds);
            return timeStamp;
        }

    }
}
