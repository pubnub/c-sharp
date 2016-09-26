using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class GrantOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit;
        private string[] channelNames = null;
        private string[] channelGroupNames = null;
        private string[] authenticationKeys = null;
        private bool grantWrite = false;
        private bool grantRead = false;
        private bool grantManage = false;
        private long grantTTL = -1;


        public GrantOperation(PNConfiguration pubnubConfig):base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public GrantOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public GrantOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public GrantOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public GrantOperation ChannelGroups(string[] channelGroups)
        {
            this.channelGroupNames = channelGroups;
            return this;
        }

        public GrantOperation AuthKeys(string[] authKeys)
        {
            this.authenticationKeys = authKeys;
            return this;
        }

        public GrantOperation Write(bool write)
        {
            this.grantWrite = write;
            return this;
        }

        public GrantOperation Read(bool read)
        {
            this.grantRead = read;
            return this;
        }

        public GrantOperation Manage(bool manage)
        {
            this.grantManage = manage;
            return this;
        }

        public GrantOperation TTL(long ttl)
        {
            this.grantTTL = ttl;
            return this;
        }

        public void Async(PNCallback<PNAccessManagerGrantResult> callback)
        {
            GrantAccess(this.channelNames, this.channelGroupNames, this.authenticationKeys, this.grantRead, this.grantWrite, this.grantManage, this.grantTTL, callback);
        }

        internal void GrantAccess(string[] channels, string[] channelGroups, string[] authKeys, bool read, bool write, bool manage, long ttl, PNCallback<PNAccessManagerGrantResult> callback)
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
                authList = authList.Where(auth => !string.IsNullOrEmpty(auth) && auth.Trim().Length > 0).Distinct<string>().ToList();
            }

            string channelsCommaDelimited = string.Join(",", channelList.ToArray());
            string channelGroupsCommaDelimited = string.Join(",", channelGroupList.ToArray());
            string authKeysCommaDelimited = string.Join(",", authList.ToArray());

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
            Uri request = urlBuilder.BuildGrantAccessRequest(channelsCommaDelimited, channelGroupsCommaDelimited, authKeysCommaDelimited, read, write, manage, ttl);

            RequestState<PNAccessManagerGrantResult> requestState = new RequestState<PNAccessManagerGrantResult>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = PNOperationType.PNAccessManagerGrant;
            requestState.Callback = callback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNAccessManagerGrantResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNAccessManagerGrantResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
