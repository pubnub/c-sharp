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

        private string publishQueryparameters = "";
        private string hereNowParameters = "";
        private string historyParameters = "";
        private string globalHereNowParameters = "";
        private string grantParameters = "";
        private string auditParameters = "";
        private string revokeParameters = "";
        private string getUserStateParameters = "";
        private string setUserStateParameters = "";
        private string channelGroupAddParameters = "";
        private string channelGroupRemoveParameters = "";

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
            List<string> url = new List<string>();

            url.Add("time");
            url.Add("0");

            return BuildRestApiRequest<Uri>(url, ResponseType.Time);
        }

        Uri IUrlRequestBuilder.BuildPublishRequest(string channel, object originalMessage, bool storeInHistory, string jsonUserMetaData)
        {
            string message = pubnubConfig.EnableJsonEncodingForPublish ? JsonEncodePublishMsg(originalMessage) : originalMessage.ToString();

            StringBuilder publishParamBuilder = new StringBuilder();

            if (!storeInHistory)
            {
                publishParamBuilder.Append("store=0");
            }

            if (!string.IsNullOrEmpty(jsonUserMetaData) && jsonLib != null && jsonLib.IsDictionaryCompatible(jsonUserMetaData))
            {
                if (publishParamBuilder.ToString().Length > 0)
                {
                    publishParamBuilder.AppendFormat("&meta={0}", EncodeUricomponent(jsonUserMetaData, ResponseType.Publish, false, false));
                }
                else
                {
                    publishParamBuilder.AppendFormat("meta={0}", EncodeUricomponent(jsonUserMetaData, ResponseType.Publish, false, false));
                }
            }

            publishQueryparameters = publishParamBuilder.ToString();

            // Generate String to Sign
            string signature = "0";
            if (pubnubConfig.SecretKey.Length > 0)
            {
                StringBuilder string_to_sign = new StringBuilder();
                string_to_sign
                    .Append(pubnubConfig.PublishKey)
                        .Append('/')
                        .Append(pubnubConfig.SubscribeKey)
                        .Append('/')
                        .Append(pubnubConfig.SecretKey)
                        .Append('/')
                        .Append(channel)
                        .Append('/')
                        .Append(message); // 1

                // Sign Message
                signature = Md5(string_to_sign.ToString());
            }

            // Build URL
            List<string> url = new List<string>();
            url.Add("publish");
            url.Add(pubnubConfig.PublishKey);
            url.Add(pubnubConfig.SubscribeKey);
            url.Add(signature);
            url.Add(channel);
            url.Add("0");
            url.Add(message);

            return BuildRestApiRequest<Uri>(url, ResponseType.Publish);
        }

        Uri IUrlRequestBuilder.BuildHereNowRequest(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState)
        {
            string channel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : ",";
            if (channel.Trim() == "")
            {
                channel = ",";
            }

            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

            int disableUUID = showUUIDList ? 0 : 1;
            int userState = includeUserState ? 1 : 0;

            if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
            {
                hereNowParameters = string.Format("?channel-group={0}&disable_uuids={1}&state={2}", channelGroup, disableUUID, userState);
            }
            else
            {
                hereNowParameters = string.Format("?disable_uuids={0}&state={1}", disableUUID, userState);
            }

            List<string> url = new List<string>();

            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");
            url.Add(channel);

            return BuildRestApiRequest<Uri>(url, ResponseType.Here_Now);
        }

        Uri IUrlRequestBuilder.BuildHistoryRequest(string channel, long start, long end, int count, bool reverse, bool includeToken)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            historyParameters = "";
            if (count <= -1)
            {
                count = 100;
            }

            parameterBuilder.AppendFormat("?count={0}", count);
            if (reverse)
            {
                parameterBuilder.AppendFormat("&reverse={0}", reverse.ToString().ToLower());
            }

            if (start != -1)
            {
                parameterBuilder.AppendFormat("&start={0}", start.ToString().ToLower());
            }

            if (end != -1)
            {
                parameterBuilder.AppendFormat("&end={0}", end.ToString().ToLower());
            }

            if (!string.IsNullOrEmpty(pubnubConfig.AuthKey))
            {
                parameterBuilder.AppendFormat("&auth={0}", EncodeUricomponent(pubnubConfig.AuthKey, ResponseType.DetailedHistory, false, false));
            }

            parameterBuilder.AppendFormat("&uuid={0}", EncodeUricomponent(pubnubConfig.Uuid, ResponseType.DetailedHistory, false, false));

            if (includeToken)
            {
                parameterBuilder.AppendFormat("&include_token={0}", includeToken.ToString().ToLower());
            }

            parameterBuilder.AppendFormat("&pnsdk={0}", EncodeUricomponent(pubnubConfig.SdkVersion, ResponseType.DetailedHistory, false, true));

            historyParameters = parameterBuilder.ToString();

            List<string> url = new List<string>();

            url.Add("v2");
            url.Add("history");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("channel");
            url.Add(channel);

            return BuildRestApiRequest<Uri>(url, ResponseType.DetailedHistory);
        }

        Uri IUrlRequestBuilder.BuildGlobalHereNowRequest(bool showUUIDList, bool includeUserState)
        {
            int disableUUID = showUUIDList ? 0 : 1;
            int userState = includeUserState ? 1 : 0;
            globalHereNowParameters = string.Format("?disable_uuids={0}&state={1}", disableUUID, userState);

            List<string> url = new List<string>();

            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);

            return BuildRestApiRequest<Uri>(url, ResponseType.GlobalHere_Now);
        }

        Uri IUrlRequestBuilder.BuildWhereNowRequest(string uuid)
        {
            List<string> url = new List<string>();

            url.Add("v2");
            url.Add("presence");
            url.Add("sub_key");
            url.Add(pubnubConfig.SubscribeKey);
            url.Add("uuid");
            url.Add(uuid);

            return BuildRestApiRequest<Uri>(url, ResponseType.Where_Now);
        }

        Uri IUrlRequestBuilder.BuildGrantAccessRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string authKeysCommaDelimited, bool read, bool write, bool manage, int ttl)
        {
            string signature = "0";
            long timeStamp = TranslateDateTimeToSeconds(DateTime.UtcNow);
            string queryString = "";
            StringBuilder queryStringBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(authKeysCommaDelimited))
            {
                queryStringBuilder.AppendFormat("auth={0}", EncodeUricomponent(authKeysCommaDelimited, ResponseType.GrantAccess, false, false));
            }

            if (!string.IsNullOrEmpty(channelsCommaDelimited))
            {
                queryStringBuilder.AppendFormat("{0}channel={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(channelsCommaDelimited, ResponseType.GrantAccess, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited))
            {
                queryStringBuilder.AppendFormat("{0}channel-group={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(channelGroupsCommaDelimited, ResponseType.GrantAccess, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited))
            {
                queryStringBuilder.AppendFormat("{0}m={1}", (queryStringBuilder.Length > 0) ? "&" : "", Convert.ToInt32(manage));
            }

            queryStringBuilder.AppendFormat("{0}", (queryStringBuilder.Length > 0) ? "&" : "");

            queryStringBuilder.AppendFormat("pnsdk={0}", EncodeUricomponent(pubnubConfig.SdkVersion, ResponseType.GrantAccess, false, true));
            queryStringBuilder.AppendFormat("&r={0}", Convert.ToInt32(read));
            queryStringBuilder.AppendFormat("&timestamp={0}", timeStamp.ToString());

            if (ttl > -1)
            {
                queryStringBuilder.AppendFormat("&ttl={0}", ttl.ToString());
            }

            queryStringBuilder.AppendFormat("&uuid={0}", EncodeUricomponent(pubnubConfig.Uuid, ResponseType.GrantAccess, false, false));

            if (!string.IsNullOrEmpty(channelsCommaDelimited))
            {
                queryStringBuilder.AppendFormat("&w={0}", Convert.ToInt32(write));
            }

            if (pubnubConfig.SecretKey.Length > 0)
            {
                StringBuilder string_to_sign = new StringBuilder();
                string_to_sign.Append(pubnubConfig.SubscribeKey)
                    .Append("\n")
                        .Append(pubnubConfig.PublishKey)
                        .Append("\n")
                        .Append("grant")
                        .Append("\n")
                        .Append(queryStringBuilder.ToString());

                PubnubCrypto pubnubCrypto = new PubnubCrypto(pubnubConfig.CiperKey);
                signature = pubnubCrypto.PubnubAccessManagerSign(pubnubConfig.SecretKey, string_to_sign.ToString());
                queryString = string.Format("signature={0}&{1}", signature, queryStringBuilder.ToString());
            }

            grantParameters = "";
            grantParameters += "?" + queryString;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("auth");
            url.Add("grant");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);

            return BuildRestApiRequest<Uri>(url, ResponseType.GrantAccess);
        }

        Uri IUrlRequestBuilder.BuildAuditAccessRequest(string channel, string channelGroup, string authKeysCommaDelimited)
        {
            string signature = "0";
            long timeStamp = ((pubnubUnitTest == null) || (pubnubUnitTest is IPubnubUnitTest && !pubnubUnitTest.EnableStubTest))
                ? TranslateDateTimeToSeconds(DateTime.UtcNow)
                    : TranslateDateTimeToSeconds(new DateTime(2013, 01, 01));
            string queryString = "";
            StringBuilder queryStringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(authKeysCommaDelimited))
            {
                queryStringBuilder.AppendFormat("auth={0}", EncodeUricomponent(authKeysCommaDelimited, ResponseType.AuditAccess, false, false));
            }

            if (!string.IsNullOrEmpty(channel))
            {
                queryStringBuilder.AppendFormat("{0}channel={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(channel, ResponseType.AuditAccess, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroup))
            {
                queryStringBuilder.AppendFormat("{0}channel-group={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(channelGroup, ResponseType.AuditAccess, false, false));
            }

            queryStringBuilder.AppendFormat("{0}pnsdk={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(pubnubConfig.SdkVersion, ResponseType.AuditAccess, false, true));
            queryStringBuilder.AppendFormat("{0}timestamp={1}", (queryStringBuilder.Length > 0) ? "&" : "", timeStamp.ToString());
            queryStringBuilder.AppendFormat("{0}uuid={1}", (queryStringBuilder.Length > 0) ? "&" : "", EncodeUricomponent(pubnubConfig.Uuid, ResponseType.AuditAccess, false, false));

            if (pubnubConfig.SecretKey.Length > 0)
            {
                StringBuilder string_to_sign = new StringBuilder();
                string_to_sign.Append(pubnubConfig.SubscribeKey)
                    .Append("\n")
                        .Append(pubnubConfig.PublishKey)
                        .Append("\n")
                        .Append("audit")
                        .Append("\n")
                        .Append(queryStringBuilder.ToString());

                PubnubCrypto pubnubCrypto = new PubnubCrypto(pubnubConfig.CiperKey);
                signature = pubnubCrypto.PubnubAccessManagerSign(pubnubConfig.SecretKey, string_to_sign.ToString());
                queryString = string.Format("signature={0}&{1}", signature, queryStringBuilder.ToString());
            }

            auditParameters = "";
            auditParameters += "?" + queryString;

            List<string> url = new List<string>();
            url.Add("v1");
            url.Add("auth");
            url.Add("audit");
            url.Add("sub-key");
            url.Add(pubnubConfig.SubscribeKey);

            return BuildRestApiRequest<Uri>(url, ResponseType.AuditAccess);
        }

        Uri IUrlRequestBuilder.BuildGetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid)
        {
            getUserStateParameters = "";
            if (string.IsNullOrEmpty(channelsCommaDelimited) && channelsCommaDelimited.Trim().Length <= 0)
            {
                channelsCommaDelimited = ",";
            }

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited) && channelGroupsCommaDelimited.Trim().Length > 0)
            {
                getUserStateParameters = string.Format("&channel-group={0}", EncodeUricomponent(channelGroupsCommaDelimited, ResponseType.GetUserState, false, false));
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

            return BuildRestApiRequest<Uri>(url, ResponseType.GetUserState);
        }

        Uri IUrlRequestBuilder.BuildSetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, string jsonUserState)
        {
            if (string.IsNullOrEmpty(channelsCommaDelimited) && channelsCommaDelimited.Trim().Length <= 0)
            {
                channelsCommaDelimited = ",";
            }
            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited) && channelGroupsCommaDelimited.Trim().Length > 0)
            {
                setUserStateParameters = string.Format("?state={0}&channel-group={1}", EncodeUricomponent(jsonUserState, ResponseType.SetUserState, false, false), EncodeUricomponent(channelGroupsCommaDelimited, ResponseType.SetUserState, false, false));
            }
            else
            {
                setUserStateParameters = string.Format("?state={0}", EncodeUricomponent(jsonUserState, ResponseType.SetUserState, false, false));
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

            return BuildRestApiRequest<Uri>(url, ResponseType.SetUserState);
        }

        Uri IUrlRequestBuilder.BuildAddChannelsToChannelGroupRequest(string channelsCommaDelimited, string nameSpace, string groupName)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            channelGroupAddParameters = "";

            parameterBuilder.AppendFormat("?add={0}", channelsCommaDelimited);

            channelGroupAddParameters = parameterBuilder.ToString();

            // Build URL
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

            return BuildRestApiRequest<Uri>(url, ResponseType.ChannelGroupAdd);
        }

        Uri IUrlRequestBuilder.BuildRemoveChannelsFromChannelGroupRequest(string channelsCommaDelimited, string nameSpace, string groupName)
        {
            bool groupNameAvailable = false;
            bool nameSpaceAvailable = false;
            bool channelAvaiable = false;

            StringBuilder parameterBuilder = new StringBuilder();
            channelGroupRemoveParameters = "";

            if (!String.IsNullOrEmpty(channelsCommaDelimited))
            {
                channelAvaiable = true;
                parameterBuilder.AppendFormat("?remove={0}", channelsCommaDelimited);
                channelGroupRemoveParameters = parameterBuilder.ToString();
            }

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

            return BuildRestApiRequest<Uri>(url, ResponseType.ChannelGroupRemove);
        }

        Uri IUrlRequestBuilder.BuildGetChannelsForChannelGroupRequest(string nameSpace, string groupName, bool limitToChannelGroupScopeOnly)
        {
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
            return BuildRestApiRequest<Uri>(url, ResponseType.ChannelGroupGet);
        }

        private Uri BuildRestApiRequest<T>(List<string> urlComponents, ResponseType type)
        {
            return BuildRestApiRequest<T>(urlComponents, type, this.pubnubConfig.Uuid);
        }

        private Uri BuildRestApiRequest<T>(List<string> urlComponents, ResponseType type, string uuid)
        {
            bool queryParamExist = false;
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

                if (type == ResponseType.Publish && componentIndex == urlComponents.Count - 1)
                {
                    url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, false, false));
                }
                else
                {
                    url.Append(EncodeUricomponent(urlComponents[componentIndex].ToString(), type, true, false));
                }
            }

            if (type == ResponseType.Presence || type == ResponseType.Subscribe || type == ResponseType.Leave)
            {
                //queryParamExist = true;
                //url.AppendFormat("?uuid={0}", uuid);
                //url.Append(subscribeParameters);
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //if (_pubnubPresenceHeartbeatInSeconds != 0)
                //{
                //    url.AppendFormat("&heartbeat={0}", _pubnubPresenceHeartbeatInSeconds);
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.PresenceHeartbeat)
            {
                //queryParamExist = true;
                //url.AppendFormat("?uuid={0}", uuid);
                //url.Append(presenceHeartbeatParameters);
                //if (_pubnubPresenceHeartbeatInSeconds != 0)
                //{
                //    url.AppendFormat("&heartbeat={0}", _pubnubPresenceHeartbeatInSeconds);
                //}
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.SetUserState)
            {
                queryParamExist = true;
                url.Append(setUserStateParameters);
                url.AppendFormat("&uuid={0}", uuid);
                if (!string.IsNullOrEmpty(pubnubConfig.AuthKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(pubnubConfig.AuthKey, type, false, false));
                }

                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(pubnubConfig.SdkVersion, type, false, true));
            }
            else if (type == ResponseType.GetUserState)
            {
                queryParamExist = true;
                url.AppendFormat("?uuid={0}", uuid);
                url.Append(getUserStateParameters);
                if (!string.IsNullOrEmpty(pubnubConfig.AuthKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(pubnubConfig.AuthKey, type, false, false));
                }

                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(pubnubConfig.SdkVersion, type, false, true));
            }
            else if (type == ResponseType.Here_Now)
            {
                queryParamExist = true;
                url.Append(hereNowParameters);
                url.AppendFormat("&uuid={0}", uuid);
                if (!string.IsNullOrEmpty(pubnubConfig.AuthKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(pubnubConfig.AuthKey, type, false, false));
                }

                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(pubnubConfig.SdkVersion, type, false, true));
            }
            else if (type == ResponseType.GlobalHere_Now)
            {
                queryParamExist = true;
                url.Append(globalHereNowParameters);
                url.AppendFormat("&uuid={0}", uuid);

                if (!string.IsNullOrEmpty(pubnubConfig.AuthKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(pubnubConfig.AuthKey, type, false, false));
                }

                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(pubnubConfig.SdkVersion, type, false, true));
            }
            else if (type == ResponseType.Where_Now)
            {
                queryParamExist = true;
                url.AppendFormat("?uuid={0}", uuid);
                if (!string.IsNullOrEmpty(pubnubConfig.AuthKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(pubnubConfig.AuthKey, type, false, false));
                }

                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(pubnubConfig.SdkVersion, type, false, true));
            }
            else if (type == ResponseType.Publish)
            {
                queryParamExist = true;
                url.AppendFormat("?uuid={0}", uuid);
                if (publishQueryparameters != "")
                {
                    url.AppendFormat("&{0}", publishQueryparameters);
                }

                if (!string.IsNullOrEmpty(pubnubConfig.AuthKey))
                {
                    url.AppendFormat("&auth={0}", EncodeUricomponent(pubnubConfig.AuthKey, type, false, false));
                }

                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(pubnubConfig.SdkVersion, type, false, true));
            }
            else if (type == ResponseType.PushRegister || type == ResponseType.PushRemove || type == ResponseType.PushGet || type == ResponseType.PushUnregister)
            {
                //queryParamExist = true;
                //switch (type)
                //{
                //    case ResponseType.PushRegister:
                //        url.Append(pushRegisterDeviceParameters);
                //        break;
                //    case ResponseType.PushRemove:
                //        url.Append(pushRemoveChannelParameters);
                //        break;
                //    case ResponseType.PushUnregister:
                //        url.Append(pushUnregisterDeviceParameters);
                //        break;
                //    default:
                //        url.Append(pushGetChannelsParameters);
                //        break;
                //}
                //url.AppendFormat("&uuid={0}", uuid);
                //if (!string.IsNullOrEmpty(_authenticationKey))
                //{
                //    url.AppendFormat("&auth={0}", EncodeUricomponent(_authenticationKey, type, false, false));
                //}
                //url.AppendFormat("&pnsdk={0}", EncodeUricomponent(_pnsdkVersion, type, false, true));
            }
            else if (type == ResponseType.ChannelGroupAdd || type == ResponseType.ChannelGroupRemove || type == ResponseType.ChannelGroupGet)
            {
                queryParamExist = true;
                switch (type)
                {
                    case ResponseType.ChannelGroupAdd:
                        url.Append(channelGroupAddParameters);
                        break;
                    case ResponseType.ChannelGroupRemove:
                        url.Append(channelGroupRemoveParameters);
                        break;
                    case ResponseType.ChannelGroupGet:
                        break;
                    default:
                        break;
                }
            }
            else if (type == ResponseType.DetailedHistory)
            {
                url.Append(historyParameters);
                queryParamExist = true;
            }
            else if (type == ResponseType.GrantAccess || type == ResponseType.ChannelGroupGrantAccess)
            {
                url.Append(grantParameters);
                queryParamExist = true;
            }
            else if (type == ResponseType.AuditAccess || type == ResponseType.ChannelGroupAuditAccess)
            {
                url.Append(auditParameters);
                queryParamExist = true;
            }
            else if (type == ResponseType.RevokeAccess || type == ResponseType.ChannelGroupRevokeAccess)
            {
                url.Append(revokeParameters);
                queryParamExist = true;
            }

            if (!queryParamExist)
            {
                url.AppendFormat("?uuid={0}", uuid);
                url.AppendFormat("&pnsdk={0}", EncodeUricomponent(pubnubConfig.SdkVersion, type, false, true));
            }

            Uri requestUri = new Uri(url.ToString());

            if (type == ResponseType.Publish || type == ResponseType.Subscribe || type == ResponseType.Presence)
            {
                ForceCanonicalPathAndQuery(requestUri);
            }

            return requestUri;
        }

        private string EncodeUricomponent(string s, ResponseType type, bool ignoreComma, bool ignorePercent2fEncode)
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

                        int cpValue = int.Parse(codepoint, NumberStyles.HexNumber);
                        if (cpValue <= 0x7F)
                        {
                            System.Diagnostics.Debug.WriteLine("0x7F");
                            string utf8HexValue = string.Format("%{0}", cpValue);
                            o.Append(utf8HexValue);
                        }
                        else if (cpValue <= 0x7FF)
                        {
                            string one = (0xC0 | ((cpValue >> 6) & 0x1F)).ToString("X");
                            string two = (0x80 | (cpValue & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}", one, two);
                            o.Append(utf8HexValue);
                        }
                        else if (cpValue <= 0xFFFF)
                        {
                            string one = (0xE0 | ((cpValue >> 12) & 0x0F)).ToString("X");
                            string two = (0x80 | ((cpValue >> 6) & 0x3F)).ToString("X");
                            string three = (0x80 | (cpValue & 0x3F)).ToString("X");
                            string utf8HexValue = string.Format("%{0}%{1}%{2}", one, two, three);
                            o.Append(utf8HexValue);
                        }
                        else if (cpValue <= 0x10FFFF)
                        {
                            string one = (0xF0 | ((cpValue >> 18) & 0x07)).ToString("X");
                            string two = (0x80 | ((cpValue >> 12) & 0x3F)).ToString("X");
                            string three = (0x80 | ((cpValue >> 6) & 0x3F)).ToString("X");
                            string four = (0x80 | (cpValue & 0x3F)).ToString("X");
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
            if (type == ResponseType.Here_Now || type == ResponseType.DetailedHistory || type == ResponseType.Leave || type == ResponseType.PresenceHeartbeat || type == ResponseType.PushRegister || type == ResponseType.PushRemove || type == ResponseType.PushGet || type == ResponseType.PushUnregister)
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

        internal const int HIGH_SURROGATE_START = 0x00d800;
        internal const int LOW_SURROGATE_END = 0x00dfff;
        internal const int LOW_SURROGATE_START = 0x00dc00;
        internal const int UNICODE_PLANE01_START = 0x10000;

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
            int temp1 = (int)s[index] - HIGH_SURROGATE_START;
            if (temp1 >= 0 && temp1 <= 0x7ff)
            {
                // Found a surrogate char.
                if (temp1 <= 0x3ff)
                {
                    // Found a high surrogate.
                    if (index < s.Length - 1)
                    {
                        int temp2 = (int)s[index + 1] - LOW_SURROGATE_START;
                        if (temp2 >= 0 && temp2 <= 0x3ff)
                        {
                            // Found a low surrogate.
                            return (temp1 * 0x400) + temp2 + UNICODE_PLANE01_START;
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

        public static long TranslateDateTimeToSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds);
            return timeStamp;
        }
    }
}
