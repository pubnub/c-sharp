using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class OtherOperation : PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;
        private static IPubnubUnitTest unitTest = null;

        public OtherOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public OtherOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public OtherOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
        }

        public void ChangeUUID(string newUUID)
        {
            if (string.IsNullOrEmpty(newUUID) || config.Uuid == newUUID)
            {
                return;
            }

            UuidChanged = true;

            string oldUUID = config.Uuid;

            config.Uuid = newUUID;
            Uuid = newUUID;

            string[] channels = GetCurrentSubscriberChannels();
            string[] channelGroups = GetCurrentSubscriberChannelGroups();

            channels = (channels != null) ? channels : new string[] { };
            channelGroups = (channelGroups != null) ? channelGroups : new string[] { };

            if (channels.Length > 0 || channelGroups.Length > 0)
            {
                string channelsJsonState = BuildJsonUserState(channels.ToArray(), channelGroups.ToArray(), false);
                IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
                Uri request = urlBuilder.BuildMultiChannelLeaveRequest(channels, channelGroups, oldUUID, channelsJsonState);

                RequestState<string> requestState = new RequestState<string>();
                requestState.Channels = channels;
                requestState.ChannelGroups = channelGroups;
                requestState.ResponseType = ResponseType.Leave;
                requestState.SubscribeRegularCallback = null;
                requestState.PresenceRegularCallback = null;
                requestState.ErrorCallback = null;
                requestState.ConnectCallback = null;
                requestState.Reconnect = false;

                string json = UrlProcessRequest(request, requestState, false); // connectCallback = null
            }

            TerminateCurrentSubscriberRequest();

        }
    }
}
