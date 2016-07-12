using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class AuditOperation : PubnubCoreBase
    {
        private PNConfiguration _pnConfig = null;
        private IJsonPluggableLibrary _jsonPluggableLibrary = null;

        public AuditOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            _pnConfig = pnConfig;
        }

        public AuditOperation(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pnConfig, jsonPluggableLibrary)
        {
            _pnConfig = pnConfig;
            _jsonPluggableLibrary = jsonPluggableLibrary;
        }

        public void AuditAccess<T>(Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            AuditAccess("", "", userCallback, errorCallback);
        }

        public void AuditAccess<T>(string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            AuditAccess(channel, "", userCallback, errorCallback);
        }

        public void AuditAccess<T>(string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(_pnConfig.SecretKey) || string.IsNullOrEmpty(_pnConfig.SecretKey.Trim()) || _pnConfig.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(_pnConfig, _jsonPluggableLibrary);
            Uri request = urlBuilder.BuildAuditAccessRequest(channel, authenticationKey);

            RequestState<T> requestState = new RequestState<T>();
            if (!string.IsNullOrEmpty(channel))
            {
                requestState.Channels = new string[] { channel };
            }
            requestState.ResponseType = ResponseType.AuditAccess;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T>(request, requestState);
        }

        public void AuditPresenceAccess<T>(string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            AuditPresenceAccess(channel, "", userCallback, errorCallback);
        }

        public void AuditPresenceAccess<T>(string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            string[] multiChannels = channel.Split(',');
            if (multiChannels.Length > 0)
            {
                for (int index = 0; index < multiChannels.Length; index++)
                {
                    multiChannels[index] = string.Format("{0}-pnpres", multiChannels[index]);
                }
            }
            string presenceChannel = string.Join(",", multiChannels);
            AuditAccess(presenceChannel, authenticationKey, userCallback, errorCallback);
        }

        public void ChannelGroupAuditAccess<T>(Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupAuditAccess("", "", userCallback, errorCallback);
        }

        public void ChannelGroupAuditAccess<T>(string channelGroup, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupAuditAccess(channelGroup, "", userCallback, errorCallback);
        }

        public void ChannelGroupAuditAccess<T>(string channelGroup, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (string.IsNullOrEmpty(_pnConfig.SecretKey) || string.IsNullOrEmpty(_pnConfig.SecretKey.Trim()) || _pnConfig.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(_pnConfig, _jsonPluggableLibrary);
            Uri request = urlBuilder.BuildChannelGroupAuditAccessRequest(channelGroup, authenticationKey);

            RequestState<T> requestState = new RequestState<T>();
            requestState.Channels = new string[] { };
            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestState.ChannelGroups = new string[] { channelGroup };
            }
            requestState.ResponseType = ResponseType.ChannelGroupAuditAccess;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<T>(request, requestState);
        }

        public void ChannelGroupAuditPresenceAccess<T>(string channelGroup, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            ChannelGroupAuditPresenceAccess(channelGroup, "", userCallback, errorCallback);
        }

        public void ChannelGroupAuditPresenceAccess<T>(string channelGroup, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            string[] multiChannelGroups = channelGroup.Split(',');
            if (multiChannelGroups.Length > 0)
            {
                for (int index = 0; index < multiChannelGroups.Length; index++)
                {
                    multiChannelGroups[index] = string.Format("{0}-pnpres", multiChannelGroups[index]);
                }
            }
            string presenceChannelGroup = string.Join(",", multiChannelGroups);
            ChannelGroupAuditAccess(presenceChannelGroup, authenticationKey, userCallback, errorCallback);
        }

    }
}
