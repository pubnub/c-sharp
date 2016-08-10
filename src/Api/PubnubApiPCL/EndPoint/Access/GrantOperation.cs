using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class GrantOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit;

        public GrantOperation(PNConfiguration pubnubConfig):base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public GrantOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public GrantOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public void GrantAccess<T>(string[] channels, string[] channelGroups, string[] authKeys, bool read, bool write, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            if ((channels == null && channelGroups == null) 
                || (channels != null && channelGroups != null && channels.Length == 0 && channelGroups.Length == 0))
            {
                throw new MissingMemberException("Invalid Channels/ChannelGroups");
            }

            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();
            List<string> authList = new List<string>();

            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (authKeys != null && authKeys.Length > 0)
            {
                authList = new List<string>(authKeys);
                authList = channelGroupList.Where(auth => !string.IsNullOrEmpty(auth) && auth.Trim().Length > 0).Distinct<string>().ToList();
            }

            string channelsCommaDelimited = string.Join(",", channelList.ToArray());
            string channelGroupsCommaDelimited = string.Join(",", channelGroupList.ToArray());
            string authKeysCommaDelimited = string.Join(",", authList.ToArray());

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
            Uri request = urlBuilder.BuildGrantAccessRequest(channelsCommaDelimited, channelGroupsCommaDelimited, authKeysCommaDelimited, read, write, manage, ttl);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = ResponseType.GrantAccess;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<T>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<T>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
