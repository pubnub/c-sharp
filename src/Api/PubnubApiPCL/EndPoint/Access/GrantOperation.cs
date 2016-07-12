using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class GrantOperation : PubnubCoreBase
    {
        private PNConfiguration _pnConfig = null;
        private IJsonPluggableLibrary _jsonPluggableLibrary = null;

        public GrantOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            _pnConfig = pnConfig;
        }

        public GrantOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pnConfig, jsonPluggableLibrary)
        {
            _pnConfig = pnConfig;
            _jsonPluggableLibrary = jsonPluggableLibrary;
        }

        public void GrantAccess<T>(string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            GrantAccess(channel, "", read, write, -1, userCallback, errorCallback);
        }

        public void GrantAccess<T>(string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            GrantAccess<T>(channel, "", read, write, ttl, userCallback, errorCallback);
        }

        public void GrantAccess<T>(string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            GrantAccess(channel, authenticationKey, read, write, -1, userCallback, errorCallback);
        }

        public void GrantAccess<T>(string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(_pnConfig.SecretKey) || string.IsNullOrEmpty(_pnConfig.SecretKey.Trim()) || _pnConfig.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(_pnConfig, _jsonPluggableLibrary);
            Uri request = urlBuilder.BuildGrantAccessRequest(channel, authenticationKey, read, write, ttl);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = ResponseType.GrantAccess;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T>(request, requestState);
        }

        public void GrantPresenceAccess<T>(string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            GrantPresenceAccess(channel, "", read, write, -1, userCallback, errorCallback);
        }

        public void GrantPresenceAccess<T>(string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            GrantPresenceAccess(channel, "", read, write, ttl, userCallback, errorCallback);
        }

        public void GrantPresenceAccess<T>(string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            GrantPresenceAccess<T>(channel, authenticationKey, read, write, -1, userCallback, errorCallback);
        }

        public void GrantPresenceAccess<T>(string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            string[] multiChannels = channel.Split(',');
            if (multiChannels.Length > 0)
            {
                for (int index = 0; index < multiChannels.Length; index++)
                {
                    if (!string.IsNullOrEmpty(multiChannels[index]) && multiChannels[index].Trim().Length > 0)
                    {
                        multiChannels[index] = string.Format("{0}-pnpres", multiChannels[index]);
                    }
                    else
                    {
                        throw new MissingMemberException("Invalid channel");
                    }
                }
            }
            string presenceChannel = string.Join(",", multiChannels);
            GrantAccess(presenceChannel, authenticationKey, read, write, ttl, userCallback, errorCallback);
        }

        public void ChannelGroupGrantAccess<T>(string channelGroup, bool read, bool write, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupGrantAccess(channelGroup, "", read, write, manage, -1, userCallback, errorCallback);
        }

        public void ChannelGroupGrantAccess<T>(string channelGroup, bool read, bool write, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupGrantAccess<T>(channelGroup, "", read, write, manage, ttl, userCallback, errorCallback);
        }

        public void ChannelGroupGrantAccess<T>(string channelGroup, string authenticationKey, bool read, bool write, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupGrantAccess(channelGroup, authenticationKey, read, write, manage, -1, userCallback, errorCallback);
        }

        public void ChannelGroupGrantAccess<T>(string channelGroup, string authenticationKey, bool read, bool write, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(_pnConfig.SecretKey) || string.IsNullOrEmpty(_pnConfig.SecretKey.Trim()) || _pnConfig.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(_pnConfig, _jsonPluggableLibrary);
            Uri request = urlBuilder.BuildChannelGroupGrantAccessRequest(channelGroup, authenticationKey, read, write, manage, ttl);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new string[] { channelGroup };
            requestState.ResponseType = ResponseType.ChannelGroupGrantAccess;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T>(request, requestState);
        }

        public void ChannelGroupGrantPresenceAccess<T>(string channelGroup, bool read, bool write, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupGrantPresenceAccess(channelGroup, "", read, write, manage, -1, userCallback, errorCallback);
        }

        public void ChannelGroupGrantPresenceAccess<T>(string channelGroup, bool read, bool write, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupGrantPresenceAccess(channelGroup, "", read, write, manage, ttl, userCallback, errorCallback);
        }

        public void ChannelGroupGrantPresenceAccess<T>(string channelGroup, string authenticationKey, bool read, bool write, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupGrantPresenceAccess<T>(channelGroup, authenticationKey, read, write, manage, -1, userCallback, errorCallback);
        }

        public void ChannelGroupGrantPresenceAccess<T>(string channelGroup, string authenticationKey, bool read, bool write, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            string[] multiChannelGroups = channelGroup.Split(',');
            if (multiChannelGroups.Length > 0)
            {
                for (int index = 0; index < multiChannelGroups.Length; index++)
                {
                    if (!string.IsNullOrEmpty(multiChannelGroups[index]) && multiChannelGroups[index].Trim().Length > 0)
                    {
                        multiChannelGroups[index] = string.Format("{0}-pnpres", multiChannelGroups[index]);
                    }
                    else
                    {
                        throw new MissingMemberException("Invalid channelgroup");
                    }
                }
            }
            string presenceChannel = string.Join(",", multiChannelGroups);
            ChannelGroupGrantAccess(presenceChannel, authenticationKey, read, write, manage, ttl, userCallback, errorCallback);
        }

    }
}
