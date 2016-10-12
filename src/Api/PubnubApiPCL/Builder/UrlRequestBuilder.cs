using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Globalization;

using System.Reflection;

namespace PubnubApi
{
    public sealed class UrlRequestBuilder : IUrlRequestBuilder
    {
        private PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonLib = null;
        private IPubnubUnitTest pubnubUnitTest = null;

        public UrlRequestBuilder(PNConfiguration config)
        {
            this.pubnubConfig = config;
        }

        public UrlRequestBuilder(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            this.pubnubConfig = config;
            this.jsonLib = jsonPluggableLibrary;
        }

        public UrlRequestBuilder(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest)
        {
            this.pubnubConfig = config;
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubUnitTest = pubnubUnitTest;
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

        Uri IUrlRequestBuilder.BuildMultiChannelSubscribeRequest(string[] channels, string[] channelGroups, object timetoken, string channelsJsonState)
        {
            PNOperationType currentType = PNOperationType.PNSubscribeOperation;
            string channelForUrl = (channels.Length > 0) ? string.Join(",", channels) : ",";

            List<string> url = new List<string>();
            url.Add("subscribe");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add(channelForUrl);
            url.Add("0");
            url.Add(timetoken.ToString());

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (pubnubConfig.PresenceHeartbeatTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig.PresenceHeartbeatTimeout.ToString());
            }

            if (channelGroups != null && channelGroups.Length > 0 && channelGroups[0] != "")
            {
                requestQueryStringParams.Add("channel-group", string.Join(",", channelGroups));
            }

            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                requestQueryStringParams.Add("state", EncodeUricomponent(channelsJsonState, currentType, false, false));
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildMultiChannelLeaveRequest(string[] channels, string[] channelGroups, string uuid, string jsonUserState)
        {
            PNOperationType currentType = PNOperationType.Leave;
            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : ",";
            List<string> url = new List<string>();

            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");
            url.Add(multiChannel);
            url.Add("leave");

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (pubnubConfig.PresenceHeartbeatTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig.PresenceHeartbeatTimeout.ToString());
            }

            string channelsJsonState = jsonUserState;
            if (channelsJsonState != "{}" && channelsJsonState != "")
            {
                requestQueryStringParams.Add("state", EncodeUricomponent(channelsJsonState, currentType, false, false));
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                requestQueryStringParams.Add("channel-group", string.Join(",", channelGroups));
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, uuid, queryParams);
        }

        Uri IUrlRequestBuilder.BuildPublishRequest(string channel, object originalMessage, bool storeInHistory, string jsonUserMetaData)
        {
            PNOperationType currentType = PNOperationType.PNPublishOperation;
            string message = pubnubConfig.EnableJsonEncodingForPublish ? JsonEncodePublishMsg(originalMessage) : originalMessage.ToString();

            List<string> url = new List<string>();
            url.Add("publish");
            url.Add(pubnubConfig.PublishKey);
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("0");
            url.Add(channel);
            url.Add("0");
            url.Add(message);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(jsonUserMetaData) && jsonLib != null && jsonLib.IsDictionaryCompatible(jsonUserMetaData))
            {
                requestQueryStringParams.Add("meta", EncodeUricomponent(jsonUserMetaData, currentType, false, false));
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
            string channel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : "";

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

            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";
            if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
            {
                requestQueryStringParams.Add("channel-group", channelGroup);
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
            url.Add("v1");
            url.Add("auth");
            url.Add("grant");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(authKeysCommaDelimited))
            {
                requestQueryStringParams.Add("auth", EncodeUricomponent(authKeysCommaDelimited, currentType, false, false));
            }

            if (!string.IsNullOrEmpty(channelsCommaDelimited))
            {
                requestQueryStringParams.Add("channel", EncodeUricomponent(channelsCommaDelimited, currentType, false, false));
                requestQueryStringParams.Add("w", Convert.ToInt32(write).ToString());
            }

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited))
            {
                requestQueryStringParams.Add("channel-group", EncodeUricomponent(channelGroupsCommaDelimited, currentType, false, false));
                requestQueryStringParams.Add("m", Convert.ToInt32(manage).ToString());
            }

            if (ttl > -1)
            {
                requestQueryStringParams.Add("ttl", ttl.ToString());
            }

            requestQueryStringParams.Add("r", Convert.ToInt32(read).ToString());
            

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        Uri IUrlRequestBuilder.BuildAuditAccessRequest(string channel, string channelGroup, string authKeysCommaDelimited)
        {
            PNOperationType currentType = PNOperationType.PNAccessManagerAudit;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("auth");
            url.Add("audit");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(authKeysCommaDelimited))
            {
                requestQueryStringParams.Add("auth", EncodeUricomponent(authKeysCommaDelimited, currentType, false, false));
            }

            if (!string.IsNullOrEmpty(channel))
            {
                requestQueryStringParams.Add("channel", EncodeUricomponent(channel, currentType, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestQueryStringParams.Add("channel-group", EncodeUricomponent(channelGroup, currentType, false, false));
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
                requestQueryStringParams.Add("channel-group", EncodeUricomponent(channelGroupsCommaDelimited, currentType, false, false));
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
                requestQueryStringParams.Add("state", EncodeUricomponent(jsonUserState, currentType, false, false));
                requestQueryStringParams.Add("channel-group", EncodeUricomponent(channelGroupsCommaDelimited, currentType, false, false));
            }
            else
            {
                requestQueryStringParams.Add("state", EncodeUricomponent(jsonUserState, currentType, false, false));
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

            requestQueryStringParams.Add("add", channelsCommaDelimited);

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

            if (!String.IsNullOrEmpty(channelsCommaDelimited))
            {
                channelAvaiable = true;
                requestQueryStringParams.Add("remove", channelsCommaDelimited);
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

            // Build URL
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
            PNOperationType currentType = PNOperationType.ChannelGroupGet;

            // Build URL
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
            requestQueryStringParams.Add("add", EncodeUricomponent(channel, currentType, true, false));

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
            requestQueryStringParams.Add("remove", EncodeUricomponent(channel, currentType, true, false));

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

            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : ",";

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
                requestQueryStringParams.Add("state", EncodeUricomponent(channelsJsonState, currentType, false, false));
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                requestQueryStringParams.Add("channel-group", string.Join(",", channelGroups));
            }

            if (pubnubConfig.PresenceHeartbeatTimeout != 0)
            {
                requestQueryStringParams.Add("heartbeat", pubnubConfig.PresenceHeartbeatTimeout.ToString());
            }

            string queryString = BuildQueryString(currentType, url, requestQueryStringParams);
            string queryParams = string.Format("?{0}", queryString);

            return BuildRestApiRequest<Uri>(url, currentType, queryParams);
        }

        private Dictionary<string, string> GenerateCommonQueryParams(PNOperationType type)
        {
            long timeStamp = TranslateUtcDateTimeToSeconds(DateTime.UtcNow);

            if ((type == PNOperationType.PNAccessManagerGrant || type == PNOperationType.PNAccessManagerAudit)
                && pubnubUnitTest != null)
            {
                timeStamp = pubnubUnitTest.Timetoken;
            }

            Dictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("uuid", this.pubnubConfig.Uuid);
            ret.Add("pnsdk", EncodeUricomponent(this.pubnubConfig.SdkVersion, PNOperationType.PNSubscribeOperation, false, true));
            ret.Add("requestid", Guid.NewGuid().ToString());
            ret.Add("timestamp", timeStamp.ToString());

            if (type != PNOperationType.PNTimeOperation
                    && type != PNOperationType.PNAccessManagerGrant && type != PNOperationType.ChannelGroupGrantAccess
                    && type != PNOperationType.PNAccessManagerAudit && type != PNOperationType.ChannelGroupAuditAccess)
            {
                if (!string.IsNullOrEmpty(this.pubnubConfig.AuthKey))
                {
                    ret.Add("auth", EncodeUricomponent(this.pubnubConfig.AuthKey, type, false, false));
                }
            }

            return ret;
        }

        private string GenerateSignature(PNOperationType type, string queryStringToSign, string partialUrl)
        {
            string signature = "";
            StringBuilder string_to_sign = new StringBuilder();
            string_to_sign.Append(this.pubnubConfig.SubscribeKey).Append("\n").Append(this.pubnubConfig.PublishKey).Append("\n");
            if (type == PNOperationType.PNAccessManagerGrant || type == PNOperationType.ChannelGroupGrantAccess)
            {
                string_to_sign.Append("grant").Append("\n");
            }
            else if (type == PNOperationType.PNAccessManagerAudit || type == PNOperationType.ChannelGroupAuditAccess)
            {
                string_to_sign.Append("audit").Append("\n");
            }
            else
            {
                string_to_sign.Append(partialUrl.ToString()).Append("\n");
            }
            string_to_sign.Append(queryStringToSign);

            PubnubCrypto pubnubCrypto = new PubnubCrypto(this.pubnubConfig.CiperKey);
            signature = pubnubCrypto.PubnubAccessManagerSign(this.pubnubConfig.SecretKey, string_to_sign.ToString());
            System.Diagnostics.Debug.WriteLine(string_to_sign.ToString() + " => Signature = " + signature);
            return signature;
        }

        private string BuildQueryString(PNOperationType type, List<string> urlComponentList, Dictionary<string, string> queryStringParamDic)
        {
            string queryString = "";

            if (queryStringParamDic == null)
            {
                queryStringParamDic = new Dictionary<string, string>();
            }

            Dictionary<string, string> commonQueryStringParams = GenerateCommonQueryParams(type);
            Dictionary<string, string> queryStringParams = new Dictionary<string, string>(commonQueryStringParams.Concat(queryStringParamDic).GroupBy(item => item.Key).ToDictionary(item => item.Key, item => item.First().Value));

            string queryToSign = string.Join("&", queryStringParams.OrderBy(kvp => kvp.Key).Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));

            if (this.pubnubConfig.SecretKey.Length > 0)
            {
                StringBuilder partialUrl = new StringBuilder();
                for (int componentIndex = 0; componentIndex < urlComponentList.Count; componentIndex++)
                {
                    partialUrl.Append("/");
                    if (type == PNOperationType.PNPublishOperation && componentIndex == urlComponentList.Count - 1)
                    {
                        partialUrl.Append(EncodeUricomponent(urlComponentList[componentIndex].ToString(), type, false, false));
                    }
                    else
                    {
                        partialUrl.Append(EncodeUricomponent(urlComponentList[componentIndex].ToString(), type, true, false));
                    }
                }

                string signature = GenerateSignature(type, queryToSign, partialUrl.ToString());
                queryString = string.Format("{0}&signature={1}", queryToSign, signature);
            }
            else
            {
                queryString = queryToSign;
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

            uuid = EncodeUricomponent(uuid, type, false, false);

            // Add http or https based on SSL flag
            if (pubnubConfig.Secure)
            {
                url.Append("https://");
            }
            else
            {
                url.Append("http://");
            }

            // Add Origin To The Request
            url.Append(pubnubConfig.Origin);

            // Generate URL with UTF-8 Encoding
            for (int componentIndex = 0; componentIndex < urlComponents.Count; componentIndex++)
            {
                url.Append("/");

                if (type == PNOperationType.PNPublishOperation && componentIndex == urlComponents.Count - 1)
                {
                    url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, false, false));
                }
                else
                {
                    url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, true, false));
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

        private string EncodeUricomponent(string s, PNOperationType type, bool ignoreComma, bool ignorePercent2fEncode)
        {
            string encodedUri = "";
            bool prevSurroagePair = false;
            StringBuilder o = new StringBuilder();
            foreach (char ch in s)
            {
                if (prevSurroagePair)
                {
                    prevSurroagePair = false;
                    continue;
                }

                if (IsUnsafe(ch, ignoreComma))
                {
                    o.Append('%');
                    o.Append(ToHex(ch / 16));
                    o.Append(ToHex(ch % 16));
                }
                else
                {
                    int positionOfChar = s.IndexOf(ch);
                    if (ch == ',' && ignoreComma)
                    {
                        o.Append(ch.ToString());
                    }
                    else if (Char.IsSurrogatePair(s, positionOfChar))
                    {
                        string codepoint = ConvertToUtf32(s, positionOfChar).ToString("X4");

                        int codePointValue = int.Parse(codepoint, NumberStyles.HexNumber);
                        if (codePointValue <= 0x7F)
                        {
                            System.Diagnostics.Debug.WriteLine("0x7F");
                            string utf8HexValue = string.Format("%{0}", codePointValue);
                            o.Append(utf8HexValue);
                        }
                        else if (codePointValue <= 0x7FF)
                        {
                            string one = (0xC0 | ((codePointValue >> 6) & 0x1F)).ToString("X");
                            string two = (0x80 | (codePointValue & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}", one, two);
                            o.Append(utf8HexValue);
                        }
                        else if (codePointValue <= 0xFFFF)
                        {
                            string one = (0xE0 | ((codePointValue >> 12) & 0x0F)).ToString("X");
                            string two = (0x80 | ((codePointValue >> 6) & 0x3F)).ToString("X");
                            string three = (0x80 | (codePointValue & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}%{2}", one, two, three);
                            o.Append(utf8HexValue);
                        }
                        else if (codePointValue <= 0x10FFFF)
                        {
                            string one = (0xF0 | ((codePointValue >> 18) & 0x07)).ToString("X");
                            string two = (0x80 | ((codePointValue >> 12) & 0x3F)).ToString("X");
                            string three = (0x80 | ((codePointValue >> 6) & 0x3F)).ToString("X");
                            string four = (0x80 | (codePointValue & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}%{2}%{3}", one, two, three, four);
                            o.Append(utf8HexValue);
                        }

                        prevSurroagePair = true;
                    }
                    else
                    {
                        string escapeChar = System.Uri.EscapeDataString(ch.ToString());
                        o.Append(escapeChar);
                    }
                }
            }

            encodedUri = o.ToString();
            if (type == PNOperationType.PNHereNowOperation || type == PNOperationType.PNHistoryOperation || type == PNOperationType.Leave || type == PNOperationType.PNHeartbeatOperation || type == PNOperationType.PushRegister || type == PNOperationType.PushRemove || type == PNOperationType.PushGet || type == PNOperationType.PushUnregister)
            {
                if (!ignorePercent2fEncode)
                {
                    encodedUri = encodedUri.Replace("%2F", "%252F");
                }
            }

            return encodedUri;
        }

        private bool IsUnsafe(char ch, bool ignoreComma)
        {
            if (ignoreComma)
            {
                return " ~`!@#$%^&*()+=[]\\{}|;':\"/<>?".IndexOf(ch) >= 0;
            }
            else
            {
                return " ~`!@#$%^&*()+=[]\\{}|;':\",/<>?".IndexOf(ch) >= 0;
            }
        }

        private string JsonEncodePublishMsg(object originalMessage)
        {
            string message = jsonLib.SerializeToJsonString(originalMessage);

            if (pubnubConfig.CiperKey.Length > 0)
            {
                PubnubCrypto aes = new PubnubCrypto(pubnubConfig.CiperKey);
                string encryptMessage = aes.Encrypt(message);
                message = jsonLib.SerializeToJsonString(encryptMessage);
            }

            return message;
        }

        private char ToHex(int ch)
        {
            return (char)(ch < 10 ? '0' + ch : 'A' + ch - 10);
        }

        internal const int HighSurrogateStart = 0x00d800;
        internal const int LowSurrogateEnd = 0x00dfff;
        internal const int LowSurrogateStart = 0x00dc00;
        internal const int UnicodePlane01Start = 0x10000;

        private static int ConvertToUtf32(String s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (index < 0 || index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            //Contract.EndContractBlock();
            // Check if the character at index is a high surrogate.
            int temp1 = (int)s[index] - HighSurrogateStart;
            if (temp1 >= 0 && temp1 <= 0x7ff)
            {
                // Found a surrogate char.
                if (temp1 <= 0x3ff)
                {
                    // Found a high surrogate.
                    if (index < s.Length - 1)
                    {
                        int temp2 = (int)s[index + 1] - LowSurrogateStart;
                        if (temp2 >= 0 && temp2 <= 0x3ff)
                        {
                            // Found a low surrogate.
                            return (temp1 * 0x400) + temp2 + UnicodePlane01Start;
                        }
                        else
                        {
                            throw new ArgumentException("index");
                        }
                    }
                    else
                    {
                        // Found a high surrogate at the end of the string.
                        throw new ArgumentException("index");
                    }
                }
                else
                {
                    // Find a low surrogate at the character pointed by index.
                    throw new ArgumentException("index");
                }
            }

            // Not a high-surrogate or low-surrogate. Genereate the UTF32 value for the BMP characters.
            return (int)s[index];
        }

        private void ForceCanonicalPathAndQuery(Uri requestUri)
        {
            LoggingMethod.WriteToLog("Inside ForceCanonicalPathAndQuery = " + requestUri.ToString(), LoggingMethod.LevelInfo);
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
                LoggingMethod.WriteToLog("Exception Inside ForceCanonicalPathAndQuery = " + ex.ToString(), LoggingMethod.LevelInfo);
            }
        }

        private static string Md5(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.Unicode.GetBytes(text);
            byte[] hash = md5.ComputeHash(data);
            string hexaHash = "";
            foreach (byte b in hash)
            {
                hexaHash += String.Format("{0:x2}", b);
            }

            return hexaHash;
        }

        public static long TranslateUtcDateTimeToSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds);
            return timeStamp;
        }

    }
}
