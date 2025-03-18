using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
	public class UnsubscribeOperation<T> : PubnubCoreBase, IUnsubscribeOperation<T>
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly TokenManager pubnubTokenManager;

        private string[] subscribeChannelNames;
        private string[] subscribeChannelGroupNames;
        private Dictionary<string, object> queryParam;

        public UnsubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubTokenManager = tokenManager;
        }

        public IUnsubscribeOperation<T> Channels(string[] channels)
        {
            this.subscribeChannelNames = channels;
            return this;
        }

        public IUnsubscribeOperation<T> ChannelGroups(string[] channelGroups)
        {
            this.subscribeChannelGroupNames = channelGroups;
            return this;
        }

        public IUnsubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute()
        {
            logger?.Trace($"{GetType().Name} Execute invoked");
            Unsubscribe(subscribeChannelNames, subscribeChannelGroupNames);
        }

        private void Unsubscribe(string[] channels, string[] channelGroups)
        {
            if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            string channel = (channels != null) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            logger?.Debug($"requested unsubscribe for channel(s)={channel}, cg(s)={channelGroup}");
            
            SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unit, pubnubTokenManager, PubnubInstance);
            manager.CurrentPubnubInstance(PubnubInstance);
            manager.MultiChannelUnSubscribeInit<T>(PNOperationType.PNUnsubscribeOperation, channel, channelGroup, this.queryParam);
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;
        }
    }
}
